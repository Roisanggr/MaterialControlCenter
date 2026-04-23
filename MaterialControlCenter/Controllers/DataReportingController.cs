using MaterialControlCenter.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Web.UI.WebControls.WebParts;
using MaterialControlCenter.Service;
using System.Runtime.Caching;




namespace MaterialControlCenter.Controllers
{
    public class DataReportingController : BaseController
    {
        private Timer _timer;

        public async Task<int> UpdateScrapStatusActiveSourceAsync()
        {
            var sourceStatus4 = dbCentralizedNotification.GetSourceDataSystemList()
                .Where(src => src.Centralized_SourceData_Master_Status == 4)
                .Select(src => src.Centralized_SourceData_Master_ID_Str)
                .ToList();

            var sourceStatus5 = dbCentralizedNotification.GetSourceDataSystemList()
                .Where(src => src.Centralized_SourceData_Master_Status == 5)
                .Select(src => src.Centralized_SourceData_Master_ID_Str)
                .ToList();

            var allScrapParts = await dbScrap.GetScrapPartsAllAsync();

          
            var scrapIdsToUpdate4 = sourceStatus4
                .Where(scrapId =>
                    allScrapParts.Any(p => p.IdScrap == scrapId && p.CurrentStatus == 1)
                )
                .ToList();

            if (scrapIdsToUpdate4.Any())
            {
                dbScrap.UpdateScrapStatusBatch(scrapIdsToUpdate4, 3);
            }

           
            var scrapIdsToUpdate5 = sourceStatus5
                .Where(scrapId =>
                    allScrapParts.Any(p => p.IdScrap == scrapId && p.CurrentStatus == 1)
                )
                .ToList();

            if (scrapIdsToUpdate5.Any())
            {
                dbScrap.UpdateScrapStatusBatch(scrapIdsToUpdate5, 2);
            }

            return scrapIdsToUpdate4.Count + scrapIdsToUpdate5.Count;
        }



        public async Task<int> UpdateSourceDataStatusFromPartsAsync()
        {
           
            var allScrapParts = await dbScrap.GetScrapPartsAllAsync();
            var scrapIdsWithAllStatus5 = allScrapParts
                .Where(p => int.TryParse(p.IdScrap, out _)) 
                .GroupBy(p => int.Parse(p.IdScrap))
                .Where(g => g.All(p => p.CurrentStatus == 5))
                .Select(g => g.Key)
                .ToList();

            if (!scrapIdsWithAllStatus5.Any())
                return 0;

          
            var allSourceData = dbCentralizedNotification.GetSourceDataSystemList();

           
            var sourceDataToUpdate = allSourceData
                .Where(sd => scrapIdsWithAllStatus5.Contains(sd.Centralized_SourceData_Master_ID))
                .ToList();

            int updatedCount = 0;

          
            foreach (var src in sourceDataToUpdate)
            {
                bool updated = dbCentralizedNotification.UpdateSourceDataStatus(src.Centralized_SourceData_ID, 9);
                if (updated) updatedCount++;
            }

            return updatedCount;
        }



     
        [HttpGet]
        public async Task<ActionResult> GetScrapMasterWithSourceDataReporting(
      string approverKpk = null,
      string allowedStatuses = null,
      string period = null)
        {
           
            if (string.IsNullOrEmpty(period))
            {
                var cookie = Request.Cookies["scrap_period"];
                if (cookie != null)
                    period = cookie.Value;
            }

            if (string.IsNullOrEmpty(approverKpk))
            {
                var cookie = Request.Cookies["scrap_approverKpk"];
                if (cookie != null)
                    approverKpk = cookie.Value;
            }

            if (string.IsNullOrEmpty(allowedStatuses))
            {
                var cookie = Request.Cookies["scrap_allowedStatuses"];
                if (cookie != null)
                    allowedStatuses = cookie.Value;
            }
            period = string.IsNullOrWhiteSpace(period)
                ? "weekly"
                : period.Trim().ToLower();

            approverKpk = string.IsNullOrWhiteSpace(approverKpk)
                ? "ALL"
                : approverKpk.Trim();

            allowedStatuses = string.IsNullOrWhiteSpace(allowedStatuses)
                ? "ALL"
                : string.Join(",",
                    allowedStatuses
                        .Split(',')
                        .Select(x => x.Trim())
                        .Where(x => x != "")
                        .OrderBy(x => x)
                );

            Response.Cookies.Add(new HttpCookie("scrap_period", period)
            {
                Expires = DateTime.Now.AddDays(7)
            });

            Response.Cookies.Add(new HttpCookie("scrap_approverKpk", approverKpk)
            {
                Expires = DateTime.Now.AddDays(7)
            });

            Response.Cookies.Add(new HttpCookie("scrap_allowedStatuses", allowedStatuses)
            {
                Expires = DateTime.Now.AddDays(7)
            });

            var cacheKey = $"scrap_reporting:{period}:{approverKpk}:{allowedStatuses}";

            var cache = await AppCache.GetOrSetAsync(
                cacheKey,
                () => ScrapReportingCacheBuilder.BuildAsync(
                    approverKpk == "ALL" ? null : approverKpk,
                    allowedStatuses == "ALL" ? null : allowedStatuses,
                    period
                ),
                10
            );

            return Json(new
            {
                Data = cache.DailyResults,
                CacheReady = true
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetScrapMasterGroupedByScrapCode(
    string approverKpk = null,
    string allowedStatuses = null,
    string period = null)
        {
          
            approverKpk = string.IsNullOrWhiteSpace(approverKpk) ? "ALL" : approverKpk.Trim();
            period = string.IsNullOrWhiteSpace(period) ? "weekly" : period.Trim().ToLower();

            allowedStatuses = string.IsNullOrWhiteSpace(allowedStatuses)
                ? "ALL"
                : string.Join(",",
                    allowedStatuses.Split(',')
                        .Select(x => x.Trim())
                        .Where(x => x != "")
                        .OrderBy(x => x)
                );

            string cacheKey =
                $"SCRAP_GROUPED::{approverKpk}::{allowedStatuses}::{period}";

         
            var rawData = await AppCache.GetOrSetAsync(
                "SCRAP_GROUPED_RAWDATA",
                async () =>
                {
                    var scrapTask = dbScrap.GetScrapMasterAsync();
                    var partTask = dbScrap.GetScrapPartsAllAsync();
                    var approvalTask = dbCentralizedNotification.GetApprovalListAsync();
                    var sourceTask = Task.Run(() => dbCentralizedNotification.GetSourceDataSystemList());
                    var employeeTask = Task.Run(() => dbCentralizedNotification.GetEmployeeMasterSSO());

                    await Task.WhenAll(scrapTask, partTask, approvalTask, sourceTask, employeeTask);

                    return new
                    {
                        Scrap = scrapTask.Result,
                        Parts = partTask.Result,
                        Approvals = approvalTask.Result,
                        Source = sourceTask.Result,
                        Employees = employeeTask.Result
                    };
                },
                minutes: 30
            );

            var cachedResult = await AppCache.GetOrSetAsync(
                cacheKey,
                async () =>
                {

                    var employeeDict = rawData.Employees
                        .GroupBy(e => e.Kpk)
                        .ToDictionary(g => g.Key, g => g.First().Name);

                    var approvalLookup = rawData.Approvals
                        .GroupBy(a => a.Centralized_SourceData_ID)
                        .ToDictionary(
                            g => g.Key,
                            g => g.OrderBy(x => x.Centralized_ApprovalList_Step).ToList()
                        );

                    var docTotals = rawData.Parts
                        .GroupBy(p => p.IdScrap)
                        .ToDictionary(
                            g => g.Key,
                            g => new
                            {
                                Qty = g.Sum(x => x.Qty),
                                Value = g.Sum(x => x.Value)
                            }
                        );

                    List<int> allowedStatusList = null;
                    if (allowedStatuses != "ALL")
                        allowedStatusList = allowedStatuses.Split(',').Select(int.Parse).ToList();

                    DateTime startDate =
                        period == "monthly" ? DateTime.Today.AddMonths(-1) :
                        period == "yearly" ? DateTime.Today.AddYears(-1) :
                                              DateTime.Today.AddDays(-7);

                    var query =
                        from scr in rawData.Scrap
                        join src in rawData.Source
                            on scr.IdScrap equals src.Centralized_SourceData_Master_ID_Str
                        let approvals = approvalLookup.ContainsKey(src.Centralized_SourceData_ID)
                            ? approvalLookup[src.Centralized_SourceData_ID]
                            : null
                        where src.Centralized_SourceData_Master_CreatedDate >= startDate
                        where allowedStatusList == null || allowedStatusList.Contains(src.Centralized_SourceData_Master_Status)
                        select new
                        {
                            scr.IdScrap,
                            scr.ScrapCode,
                            scr.isSubmitted,
                            scr.CreatedDate,
                            scr.isDeleted,
                            SourceDataStatus = src.Centralized_SourceData_Master_Status,
                            Approvals = approvals
                        };

                    if (approverKpk != "ALL")
                    {
                        query = query.Where(j =>
                        {
                            if (j.Approvals == null) return false;

                            for (int i = 0; i < j.Approvals.Count; i++)
                            {
                                var cur = j.Approvals[i];
                                if (cur.Centralized_ApprovalList_KpkApproval == approverKpk &&
                                    (cur.Centralized_StatusList_ID == 1 ||
                                     cur.Centralized_StatusList_ID == 2))
                                {
                                    for (int k = 0; k < i; k++)
                                        if (j.Approvals[k].Centralized_StatusList_ID == 1)
                                            return false;

                                    return true;
                                }
                            }
                            return false;
                        });
                    }

                    var joinedList = query.ToList();
                    var grouped = joinedList
                        .GroupBy(x => x.ScrapCode)
                        .Select(g =>
                        {
                            var items = g.Select(x =>
                            {
                                var totals = docTotals.ContainsKey(x.IdScrap)
                                    ? docTotals[x.IdScrap]
                                    : null;

                                return new
                                {
                                    x.IdScrap,
                                    x.ScrapCode,
                                    x.isSubmitted,
                                    x.CreatedDate,
                                    x.isDeleted,
                                    x.SourceDataStatus,
                                    TotalQty = totals != null ? totals.Qty : 0,
                                    TotalValue = totals != null ? totals.Value : 0
                                };
                            }).ToList();

                            return new
                            {
                                ScrapCode = g.Key,
                                ScrapItems = items,
                                TotalQty = items.Sum(i => i.TotalQty),
                                TotalValue = items.Sum(i => i.TotalValue)
                            };
                        })
                        .OrderByDescending(x => x.TotalValue)
                        .ToList();

                    var grandQty = grouped.Sum(x => x.TotalQty);
                    var grandValue = grouped.Sum(x => x.TotalValue);

                    return new
                    {
                        Data = grouped.Select(g => new
                        {
                            g.ScrapCode,
                            g.ScrapItems,
                            g.TotalQty,
                            g.TotalValue,
                            ContributionPercent = grandValue > 0
                                ? Math.Round((g.TotalValue / grandValue) * 100, 2)
                                : 0
                        }).ToList(),
                        GrandTotalQty = grandQty,
                        GrandTotalValue = grandValue
                    };
                },
                minutes: 10
            );

            return Json(cachedResult, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GroupPartsByPartNum(
    string approverKpk = null,
    string allowedStatuses = null)
        {
            approverKpk = string.IsNullOrWhiteSpace(approverKpk) ? "ALL" : approverKpk.Trim();

            allowedStatuses = string.IsNullOrWhiteSpace(allowedStatuses)
                ? "ALL"
                : string.Join(",",
                    allowedStatuses.Split(',')
                        .Select(x => x.Trim())
                        .Where(x => x != "")
                        .OrderBy(x => x)
                );

            string cacheKey =
                $"SCRAP_PART_GROUP::{approverKpk}::{allowedStatuses}";

            var cachedResult = await AppCache.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var scrapTask = dbScrap.GetScrapMasterAsync();
                    var partTask = dbScrap.GetScrapPartsAllAsync();
                    var approvalTask = dbCentralizedNotification.GetApprovalListAsync();
                    var sourceTask = Task.Run(() => dbCentralizedNotification.GetSourceDataSystemList());
                    var employeeTask = Task.Run(() => dbCentralizedNotification.GetEmployeeMasterSSO());

                    await Task.WhenAll(scrapTask, partTask, approvalTask, sourceTask, employeeTask);

                    var scrapList = scrapTask.Result;
                    var allScrapParts = partTask.Result;
                    var approvalList = approvalTask.Result;
                    var sourceDataList = sourceTask.Result;
                    var employees = employeeTask.Result;
                    var employeeDict = employees
                        .GroupBy(e => e.Kpk)
                        .ToDictionary(g => g.Key, g => g.First().Name);

                    var approvalLookup = approvalList
                        .GroupBy(a => a.Centralized_SourceData_ID)
                        .ToDictionary(
                            g => g.Key,
                            g => g.OrderBy(x => x.Centralized_ApprovalList_Step).ToList()
                        );

                    var sourceLookup = sourceDataList
                        .GroupBy(s => s.Centralized_SourceData_Master_ID_Str)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    var partLookup = allScrapParts
                        .GroupBy(p => p.IdScrap)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    List<int> allowedStatusList = null;
                    if (allowedStatuses != "ALL")
                        allowedStatusList = allowedStatuses.Split(',').Select(int.Parse).ToList();
                    var validScrapIds = new HashSet<string>();

                    foreach (var scr in scrapList)
                    {
                        if (!sourceLookup.ContainsKey(scr.IdScrap))
                            continue;

                        foreach (var src in sourceLookup[scr.IdScrap])
                        {
                            if (allowedStatusList != null &&
                                !allowedStatusList.Contains(src.Centralized_SourceData_Master_Status))
                                continue;

                            if (approverKpk != "ALL")
                            {
                                if (!approvalLookup.ContainsKey(src.Centralized_SourceData_ID))
                                    continue;

                                var approvals = approvalLookup[src.Centralized_SourceData_ID];
                                bool ok = false;

                                for (int i = 0; i < approvals.Count; i++)
                                {
                                    var cur = approvals[i];
                                    if (cur.Centralized_ApprovalList_KpkApproval == approverKpk &&
                                        (cur.Centralized_StatusList_ID == 1 ||
                                         cur.Centralized_StatusList_ID == 2))
                                    {
                                        bool prevDone = true;
                                        for (int k = 0; k < i; k++)
                                            if (approvals[k].Centralized_StatusList_ID == 1)
                                                prevDone = false;

                                        ok = prevDone;
                                        break;
                                    }
                                }

                                if (!ok)
                                    continue;
                            }

                            validScrapIds.Add(scr.IdScrap);
                        }
                    }
                    var partTotals = new Dictionary<(string PartNum, string Description), decimal>();

                    foreach (var scrapId in validScrapIds)
                    {
                        if (!partLookup.ContainsKey(scrapId))
                            continue;

                        foreach (var p in partLookup[scrapId])
                        {
                            var key = (p.PartNum, p.Description);
                            if (!partTotals.ContainsKey(key))
                                partTotals[key] = 0;

                            partTotals[key] += p.Value;
                        }
                    }
                    var result = partTotals
                        .Select(k => new PartGroupResult
                        {
                            PartNum = k.Key.PartNum,
                            Description = k.Key.Description,
                            TotalValue = k.Value
                        })
                        .OrderByDescending(x => x.TotalValue)
                        .ToList();

                    var grandTotal = result.Sum(x => x.TotalValue);

                    foreach (var r in result)
                    {
                        r.ContributionPercent = grandTotal > 0
                            ? Math.Round((double)(r.TotalValue / grandTotal) * 100, 2)
                            : 0;
                    }

                    return result;
                },
                minutes: 10
            );

            return Json(cachedResult, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GroupPartsByRemarks(
    string approverKpk = null,
    string allowedStatuses = null)
        {
            approverKpk = string.IsNullOrWhiteSpace(approverKpk) ? "ALL" : approverKpk.Trim();

            allowedStatuses = string.IsNullOrWhiteSpace(allowedStatuses)
                ? "ALL"
                : string.Join(",",
                    allowedStatuses.Split(',')
                        .Select(x => x.Trim())
                        .Where(x => x != "")
                        .OrderBy(x => x)
                );

            string cacheKey =
                $"SCRAP_PART_BY_REMARKS::{approverKpk}::{allowedStatuses}";

            var cachedResult = await AppCache.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var scrapTask = dbScrap.GetScrapMasterAsync();
                    var partTask = dbScrap.GetScrapPartsAllAsync();
                    var approvalTask = dbCentralizedNotification.GetApprovalListAsync();
                    var sourceTask = Task.Run(() => dbCentralizedNotification.GetSourceDataSystemList());
                    var employeeTask = Task.Run(() => dbCentralizedNotification.GetEmployeeMasterSSO());
                    var remarksTask = Task.Run(() => dbScrap.GetScrapCodeRemarks());

                    await Task.WhenAll(
                        scrapTask, partTask, approvalTask,
                        sourceTask, employeeTask, remarksTask
                    );

                    var scrapList = scrapTask.Result;
                    var allScrapParts = partTask.Result;
                    var approvalList = approvalTask.Result;
                    var sourceDataList = sourceTask.Result;
                    var employees = employeeTask.Result;
                    var remarksList = remarksTask.Result;
                    var employeeDict = employees
                        .GroupBy(e => e.Kpk)
                        .ToDictionary(g => g.Key, g => g.First().Name);

                    var remarksDict = remarksList
                        .GroupBy(r => r.IdRemarks.ToString())
                        .ToDictionary(g => g.Key, g => g.First().Remarks);

                    var approvalLookup = approvalList
                        .GroupBy(a => a.Centralized_SourceData_ID)
                        .ToDictionary(
                            g => g.Key,
                            g => g.OrderBy(x => x.Centralized_ApprovalList_Step).ToList()
                        );

                    var sourceLookup = sourceDataList
                        .GroupBy(s => s.Centralized_SourceData_Master_ID_Str)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    var partLookup = allScrapParts
                        .GroupBy(p => p.IdScrap)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    List<int> allowedStatusList = null;
                    if (allowedStatuses != "ALL")
                        allowedStatusList = allowedStatuses.Split(',').Select(int.Parse).ToList();
                    var validScrapIds = new HashSet<string>();

                    foreach (var scr in scrapList)
                    {
                        if (!sourceLookup.ContainsKey(scr.IdScrap))
                            continue;

                        foreach (var src in sourceLookup[scr.IdScrap])
                        {
                            if (allowedStatusList != null &&
                                !allowedStatusList.Contains(src.Centralized_SourceData_Master_Status))
                                continue;

                            if (approverKpk != "ALL")
                            {
                                if (!approvalLookup.ContainsKey(src.Centralized_SourceData_ID))
                                    continue;

                                var approvals = approvalLookup[src.Centralized_SourceData_ID];
                                bool ok = false;

                                for (int i = 0; i < approvals.Count; i++)
                                {
                                    var cur = approvals[i];
                                    if (cur.Centralized_ApprovalList_KpkApproval == approverKpk &&
                                        (cur.Centralized_StatusList_ID == 1 ||
                                         cur.Centralized_StatusList_ID == 2))
                                    {
                                        bool prevDone = true;
                                        for (int k = 0; k < i; k++)
                                            if (approvals[k].Centralized_StatusList_ID == 1)
                                                prevDone = false;

                                        ok = prevDone;
                                        break;
                                    }
                                }

                                if (!ok)
                                    continue;
                            }

                            validScrapIds.Add(scr.IdScrap);
                        }
                    }
                    var remarkTotals =
                        new Dictionary<string, (decimal Qty, decimal Value, HashSet<string> Ids)>();

                    foreach (var scrapId in validScrapIds)
                    {
                        if (!partLookup.ContainsKey(scrapId))
                            continue;

                        foreach (var p in partLookup[scrapId])
                        {
                            var remarkName = remarksDict.ContainsKey(p.Remarks)
                                ? remarksDict[p.Remarks]
                                : "Unknown";

                            if (!remarkTotals.ContainsKey(remarkName))
                                remarkTotals[remarkName] = (0, 0, new HashSet<string>());

                            var current = remarkTotals[remarkName];
                            current.Qty += p.Qty;
                            current.Value += p.Value;
                            current.Ids.Add(p.Remarks);

                            remarkTotals[remarkName] = current;
                        }
                    }
                    var result = remarkTotals
                        .Select(r => new
                        {
                            RemarksName = r.Key,
                            RemarksIds = string.Join(",", r.Value.Ids),
                            TotalQty = r.Value.Qty,
                            TotalValue = r.Value.Value
                        })
                        .OrderByDescending(x => x.TotalValue)
                        .ToList();

                    var grandTotal = result.Sum(x => x.TotalValue);

                    return result.Select(r => new
                    {
                        r.RemarksIds,
                        r.RemarksName,
                        r.TotalQty,
                        r.TotalValue,
                        ContributionPercent = grandTotal > 0
                            ? Math.Round((double)(r.TotalValue / grandTotal) * 100, 2)
                            : 0
                    }).ToList();
                },
                minutes: 10
            );

            return Json(cachedResult, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public JsonResult GetSubordinateChainJson(string kpk)
        {
            try
            {
                var subordinates = dbSSO.GetUsersBySupervisorKpk(kpk)
                    .Select(emp => new
                    {
                        Kpk = emp.Kpk,
                        Name = emp.Name?.Trim(),
                        Supervisor = emp.Supervisor
                    })
                    .ToList();

                return Json(subordinates, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        private HashSet<string> GetSubordinateChain(string supervisorKpk)
        {
            var result = new HashSet<string>();
            var queue = new Queue<string>();
            queue.Enqueue(supervisorKpk);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var subs = dbSSO.GetUsersBySupervisorKpk(current)
                    .Select(e => e.Kpk)
                    .ToList();

                foreach (var sub in subs)
                {
                    if (result.Add(sub))
                        queue.Enqueue(sub);
                }
            }

            return result;
        }

        [HttpGet]
        public async Task<JsonResult> GetJoinedSourceDataScrap(
    string initiatorKpk = null,
    string approverKpk = null,
    string supervisorKpk = null,
    List<int> statusList = null,
    int pageNumber = 1,
    int pageSize = 10)
        {
            initiatorKpk = string.IsNullOrWhiteSpace(initiatorKpk) ? null : initiatorKpk.Trim();
            approverKpk = string.IsNullOrWhiteSpace(approverKpk) ? null : approverKpk.Trim();
            supervisorKpk = string.IsNullOrWhiteSpace(supervisorKpk) ? null : supervisorKpk.Trim();

            string statusKey = statusList == null || !statusList.Any()
                ? "ALL"
                : string.Join(",", statusList.OrderBy(x => x));

            string cacheKey =
                $"JOINED_SCRAP::{initiatorKpk ?? "ALL"}::{approverKpk ?? "ALL"}::{supervisorKpk ?? "ALL"}::{statusKey}";

            var cached = await AppCache.GetOrSetAsync(
                cacheKey,
                async () =>
                {

                    var scrapTask = dbScrap.GetScrapMasterAsync();
                    var partTask = dbScrap.GetScrapPartsAllAsync();
                    var approvalTask = dbCentralizedNotification.GetApprovalListAsync();
                    var sourceTask = Task.Run(() => dbCentralizedNotification.GetSourceDataSystemList());
                    var employeeTask = Task.Run(() => dbCentralizedNotification.GetEmployeeMasterSSO());

                    await Task.WhenAll(scrapTask, partTask, approvalTask, sourceTask, employeeTask);

                    var scrapList = scrapTask.Result;
                    var parts = partTask.Result;
                    var approvals = approvalTask.Result;
                    var sources = sourceTask.Result;
                    var employees = employeeTask.Result;
                    var employeeDict = employees
                        .GroupBy(e => e.Kpk)
                        .ToDictionary(g => g.Key, g => g.First().Name);

                    var partLookup = parts
                        .GroupBy(p => p.IdScrap)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    var approvalBySource = approvals
                        .GroupBy(a => a.Centralized_SourceData_ID)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    var approvalByApprover = approvals
                        .GroupBy(a => a.Centralized_ApprovalList_KpkApproval)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.Centralized_SourceData_ID).ToHashSet());
                    var joined = (
                        from src in sources
                        join scr in scrapList
                            on src.Centralized_SourceData_Master_ID_Str equals scr.IdScrap
                        where (initiatorKpk == null || scr.InitiatorKpk == initiatorKpk)
                        where (statusList == null || !statusList.Any()
                               || statusList.Contains(src.Centralized_SourceData_Master_Status))
                        select new
                        {
                            src.Centralized_SourceData_ID,
                            scr.IdScrap,
                            scr.ScrapCode,
                            scr.CreatedDate,
                            InitiatorName = employeeDict.ContainsKey(scr.InitiatorKpk)
                                ? employeeDict[scr.InitiatorKpk]
                                : "Unknown",
                            Parts = partLookup.ContainsKey(scr.IdScrap)
                                ? partLookup[scr.IdScrap]
                                : new List<ScrapPartModel>(),
                            Approvals = approvalBySource.ContainsKey(src.Centralized_SourceData_ID)
                                ? approvalBySource[src.Centralized_SourceData_ID]
                                : new List<CentralizedApprovalListModel>()

                        }
                    ).ToList();
                    var groupedByApprover =
                        new Dictionary<string, List<object>>();

                    foreach (var j in joined)
                    {
                        foreach (var a in j.Approvals)
                        {
                            var kpk = a.Centralized_ApprovalList_KpkApproval;
                            if (!groupedByApprover.ContainsKey(kpk))
                                groupedByApprover[kpk] = new List<object>();

                            groupedByApprover[kpk].Add(j);
                        }
                    }

                    return new
                    {
                        Joined = joined,
                        GroupedByApprover = groupedByApprover
                    };
                },
                minutes: 10
            );
            var subordinates = new List<dynamic>();
            if (!string.IsNullOrEmpty(supervisorKpk))
            {
                subordinates = dbSSO.GetUsersBySupervisorKpk(supervisorKpk)
                    .Select(s => new
                    {
                        Kpk = s.Kpk,
                        Name = s.Name?.Trim(),
                        Supervisor = s.Supervisor
                    }).ToList<dynamic>();
            }

            var result = subordinates.Select(sub =>
            {
                var data =
                    cached.GroupedByApprover.ContainsKey(sub.Kpk)
                        ? cached.GroupedByApprover[sub.Kpk]
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToList()
                        : new List<object>();

                return new
                {
                    SubordinateKpk = sub.Kpk,
                    SubordinateName = sub.Name,
                    sub.Supervisor,
                    ScrapData = data
                };
            }).ToList();

            return Json(new { Data = result }, JsonRequestBehavior.AllowGet);
        }

        private async Task<List<ScrapDocumentPartRawDto>> GetRawScrapDocumentPartsAsync()
        {
            return await AppCache.GetOrSetAsync(
                "SCRAP_DOC_PARTS_RAW_V3",
                async () =>
                {
                    var sourceDataList = dbCentralizedNotification.GetSourceDataSystemList();
                    var scrapList = await dbScrap.GetScrapMasterAsync();
                    var approvalList = await dbCentralizedNotification.GetApprovalListAsync();
                    var employees = dbCentralizedNotification.GetEmployeeMasterSSO();
                    var allScrapParts = await dbScrap.GetScrapPartsAllAsync();
                    var employeeDict = employees
                        .Where(e => !string.IsNullOrEmpty(e.Kpk))
                        .ToDictionary(e => e.Kpk.Trim(), e => e.Name?.Trim());

                    var partLookup = allScrapParts
                        .GroupBy(p => p.IdScrap)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    var approvalLookup = approvalList
                        .GroupBy(a => a.Centralized_SourceData_ID)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(a => new ApprovalDto
                            {
                                ApproverKpk = a.Centralized_ApprovalList_KpkApproval,
                                ApproverName = employeeDict.ContainsKey(a.Centralized_ApprovalList_KpkApproval)
                                    ? employeeDict[a.Centralized_ApprovalList_KpkApproval]
                                    : "Unknown"
                            }).ToList()
                        );

                    var result = new List<ScrapDocumentPartRawDto>();

                    foreach (var src in sourceDataList)
                    {
                        var scrap = scrapList.FirstOrDefault(s => s.IdScrap == src.Centralized_SourceData_Master_ID_Str);
                        if (scrap == null) continue;

                        if (!partLookup.TryGetValue(scrap.IdScrap, out var parts))
                            continue;

                        List<ApprovalDto> approvals;
                        if (!approvalLookup.TryGetValue(src.Centralized_SourceData_ID, out approvals))
                        {
                            approvals = new List<ApprovalDto>();
                        }



                        foreach (var part in parts)
                        {
                            result.Add(new ScrapDocumentPartRawDto
                            {
                                SourceDataId = src.Centralized_SourceData_ID,
                                ScrapId = scrap.IdScrap,
                                Facility = scrap.Facility,
                                TC = scrap.TC,
                                InitiatorKpk = scrap.InitiatorKpk,
                                InitiatorName = employeeDict.ContainsKey(scrap.InitiatorKpk)
                                    ? employeeDict[scrap.InitiatorKpk]
                                    : "Unknown",
                                ScrapCode = scrap.ScrapCode,
                                CreatedDate = scrap.CreatedDate,

                                PartNum = part.PartNum,
                                Description = part.Description,
                                Qty = part.Qty,
                                Value = part.Value,
                                Status = part.CurrentStatus,

                                Approvals = approvals
                            });
                        }
                    }

                    return result;
                },
                minutes: 15
            );
        }


        [HttpGet]
        public async Task<JsonResult> GetScrapDocumentsPartsDetail(
     string initiatorKpk = null,
     string approverKpk = null,
     string supervisorKpk = null,
     List<int> statusList = null,
     int pageNumber = 1,
     int pageSize = 5,
     string documentId = "",
     string partNumber = "",
     string supervisorName = "",
     decimal? totalFrom = null,
     decimal? totalTo = null
 )
        {
            string cacheKey =
                $"SCRAP_DOC_PARTS_FINAL::{initiatorKpk ?? "ALL"}::{approverKpk ?? "ALL"}::{supervisorKpk ?? "ALL"}::" +
                $"{(statusList != null ? string.Join("-", statusList) : "ALL")}::{documentId}::{partNumber}::{supervisorName}::" +
                $"{totalFrom}::{totalTo}::P{pageNumber}::S{pageSize}";

            var result = await AppCache.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var raw = await GetRawScrapDocumentPartsAsync();
                    IEnumerable<ScrapDocumentPartRawDto> q = raw;
                    HashSet<string> supervisorChain = null;
                    if (!string.IsNullOrEmpty(supervisorKpk))
                    {
                        supervisorChain = GetSubordinateChain(supervisorKpk);
                        supervisorChain.Add(supervisorKpk);
                    }

                    if (!string.IsNullOrEmpty(initiatorKpk))
                        q = q.Where(x => x.InitiatorKpk == initiatorKpk);

                    if (statusList?.Any() == true)
                        q = q.Where(x => statusList.Contains(x.Status));

                    if (!string.IsNullOrEmpty(approverKpk))
                        q = q.Where(x => x.Approvals.Any(a => a.ApproverKpk == approverKpk));

                    if (supervisorChain != null)
                        q = q.Where(x => x.Approvals.Any(a => supervisorChain.Contains(a.ApproverKpk)));

                    if (!string.IsNullOrEmpty(documentId))
                        q = q.Where(x => x.ScrapId.Contains(documentId));

                    if (!string.IsNullOrEmpty(partNumber))
                        q = q.Where(x => x.PartNum != null && x.PartNum.Contains(partNumber));

                    if (!string.IsNullOrEmpty(supervisorName))
                        q = q.Where(x => x.Approvals.Any(a => a.ApproverName.Contains(supervisorName)));

                    if (totalFrom.HasValue)
                        q = q.Where(x => x.Value >= totalFrom.Value);

                    if (totalTo.HasValue)
                        q = q.Where(x => x.Value <= totalTo.Value);

                    var totalParts = q.Count();

                    var pagedParts = q
                        .OrderByDescending(x => x.CreatedDate)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                    var data = pagedParts
                        .GroupBy(x => new { x.SourceDataId, x.ScrapId })
                        .Select(g => new
                        {
                            DocumentId = g.Key.SourceDataId,
                            ScrapId = g.Key.ScrapId,
                            Facility = g.First().Facility,
                            TC = g.First().TC,
                            InitiatorKpk = g.First().InitiatorKpk,
                            InitiatorName = g.First().InitiatorName,
                            ScrapCode = g.First().ScrapCode,
                            CreatedDate = g.First().CreatedDate,
                            Parts = g.Select(p => new
                            {
                                p.PartNum,
                                p.Description,
                                p.Qty,
                                p.Value,
                                p.Status
                            }).ToList(),
                            Approvals = supervisorChain != null
                                ? g.First().Approvals
                                    .Where(a => supervisorChain.Contains(a.ApproverKpk))
                                    .ToList()
                                : g.First().Approvals
                        })
                        .ToList();

                    return new
                    {
                        Data = data,
                        TotalParts = totalParts
                    };

                },
                minutes: 5
            );

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public async Task<JsonResult> GetScrapDocumentsPartsDetailForReporting(
    string initiatorKpk = null,
    string approverKpk = null,
    string supervisorKpk = null,
    List<int> statusList = null,
    string filterType = null
)
        {
            var sourceDataList = dbCentralizedNotification.GetSourceDataSystemList();
            var scrapList = await dbScrap.GetScrapMasterAsync();
            var approvalList = await dbCentralizedNotification.GetApprovalListAsync();
            var employees = dbCentralizedNotification.GetEmployeeMasterSSO();
            var allScrapParts = await dbScrap.GetScrapPartsAllAsync();
            var remarksList = dbScrap.GetScrapCodeRemarks();
            var typeScrapList = await dbScrap.GetTypeScrapAsync();
            var employeeDict = employees.ToDictionary(e => e.Kpk, e => e.Name);
            var remarksDict = remarksList.ToDictionary(r => r.IdRemarks.ToString(), r => r.Remarks);
            var typeDict = typeScrapList.ToDictionary(t => t.Type_ID, t => t.Type_Desc);
            var partsByScrap = allScrapParts
                .GroupBy(p => p.IdScrap)
                .ToDictionary(g => g.Key, g => g.ToList());
            var approvalDict = approvalList
                .Where(a => a.Centralized_ApprovalList_Step == 1)
                .GroupBy(a => a.Centralized_SourceData_ID)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.Centralized_ApprovalList_Date).FirstOrDefault()
                );
            var allApprovalsBySource = approvalList
                .GroupBy(a => a.Centralized_SourceData_ID)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.Centralized_ApprovalList_Step).ToList()
                );
            if (!string.IsNullOrEmpty(initiatorKpk))
                scrapList = scrapList.Where(s => s.InitiatorKpk == initiatorKpk).ToList();

            if (!string.IsNullOrEmpty(filterType))
            {
                var today = DateTime.Today;

                DateTime? startDate = null;
                DateTime? endDate = null;

                if (filterType.Equals("weekly", StringComparison.OrdinalIgnoreCase))
                {
                    startDate = today.AddDays(-(int)today.DayOfWeek);
                    endDate = startDate.Value.AddDays(7);
                }
                else if (filterType.Equals("monthly", StringComparison.OrdinalIgnoreCase))
                {
                    startDate = new DateTime(today.Year, today.Month, 1);
                    endDate = startDate.Value.AddMonths(1);
                }
                else if (filterType.Equals("yearly", StringComparison.OrdinalIgnoreCase))
                {
                    startDate = new DateTime(today.Year, 1, 1);
                    endDate = startDate.Value.AddYears(1);
                }

                if (startDate.HasValue && endDate.HasValue)
                {
                    scrapList = scrapList
                        .Where(s => s.CreatedDate >= startDate.Value &&
                                    s.CreatedDate < endDate.Value)
                        .ToList();
                }
            }

            var scrapDict = scrapList
              .GroupBy(s => s.IdScrap)
              .ToDictionary(g => g.Key, g => g.First());

            var result = new List<object>();
            int totalParts = 0;

            foreach (var src in sourceDataList)
            {
                if (!scrapDict.ContainsKey(src.Centralized_SourceData_Master_ID_Str))
                    continue;

                var scrap = scrapDict[src.Centralized_SourceData_Master_ID_Str];

                if (!partsByScrap.ContainsKey(scrap.IdScrap))
                    continue;

                var parts = partsByScrap[scrap.IdScrap];

                if (statusList != null && statusList.Any())
                    parts = parts.Where(p => statusList.Contains(p.CurrentStatus)).ToList();

                if (parts.Count == 0)
                    continue;

                approvalDict.TryGetValue(src.Centralized_SourceData_ID, out var approval);

                if (!string.IsNullOrEmpty(supervisorKpk) && approval == null)
                    continue;

                totalParts += parts.Count;

                result.Add(new
                {
                    DocumentId = src.Centralized_SourceData_ID,
                    ScrapId = scrap.IdScrap,
                    DocumentStatus = src.Centralized_SourceData_Master_Status,
                    Facility = scrap.Facility,
                    TC = scrap.TC,
                    SpecialCodeTcCompanion = scrap.SpecialCodeTcCompanion,
                    ScrapCode = scrap.ScrapCode,
                    WC = scrap.WC,
                    SpecialCodeRemarks = scrap.SpecialCodeRemarks,
                    TypeName = typeDict.ContainsKey(scrap.Type_ID)
                        ? typeDict[scrap.Type_ID]
                        : "Unknown",
                    InitiatorKpk = scrap.InitiatorKpk,
                    InitiatorName = employeeDict.ContainsKey(scrap.InitiatorKpk)
                        ? employeeDict[scrap.InitiatorKpk]
                        : "Unknown",
                    CreatedDate = scrap.CreatedDate,
                    CurrentApprovalName = src.Centralized_SourceData_Master_Status != 1
                        ? "-"
                        : (!allApprovalsBySource.ContainsKey(src.Centralized_SourceData_ID)
                            ? "-"
                            : (allApprovalsBySource[src.Centralized_SourceData_ID]
                                .Where(a => a.Centralized_StatusList_ID == 1)
                                .OrderBy(a => a.Centralized_ApprovalList_Step)
                                .Select(a => employeeDict.ContainsKey(a.Centralized_ApprovalList_KpkApproval)
                                    ? employeeDict[a.Centralized_ApprovalList_KpkApproval]
                                    : "Unknown")
                                .FirstOrDefault() ?? "-")),
                    Parts = parts.Select(p => new
                    {
                        p.PartNum,
                        p.Description,
                        p.Qty,
                        p.BasePrice,
                        p.Measit,
                        p.Planit,
                        p.Cmidit,
                        p.Commit,
                        p.RnNumber,
                        p.LeaderKPK,
                        RemarksID = p.Remarks,
                        RemarksName = remarksDict.ContainsKey(p.Remarks)
                            ? remarksDict[p.Remarks]
                            : "Deleted",
                        p.Value,
                        p.KeyInDateTime,
                        Status = p.CurrentStatus,
                        SpecialcodeRemarksParts = p.SpecialcodeRemarksParts
                    }).ToList(),
                    Approvals = approval != null
                        ? new[]
                        {
                    new
                    {
                        approval.Centralized_ApprovalList_ID,
                        approval.Centralized_ApprovalList_KpkApproval,
                        approval.Centralized_ApprovalList_Date,
                        ApproverName = employeeDict.ContainsKey(approval.Centralized_ApprovalList_KpkApproval)
                            ? employeeDict[approval.Centralized_ApprovalList_KpkApproval]
                            : "Unknown"
                    }
                        }
                        : new object[] { }
                });
            }

            return Json(new { Data = result, TotalParts = totalParts },
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> DownloadScrapReport(
            string type = null,
            int? week = null,
            int? month = null,
            int? year = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? status = null)
        {
            var normalizedType = (type ?? string.Empty).Trim().ToLowerInvariant();

            if (normalizedType == "monthly" && month.HasValue)
            {
                var selectedYear = year ?? DateTime.Today.Year;
                startDate = new DateTime(selectedYear, month.Value, 1);
                endDate = startDate.Value.AddMonths(1).AddDays(-1);
            }
            else if (normalizedType == "weekly" && (!startDate.HasValue || !endDate.HasValue))
            {
                var selectedYear = year ?? DateTime.Today.Year;
                var selectedMonth = month ?? DateTime.Today.Month;

                if (week.HasValue)
                {
                    var startDay = ((week.Value - 1) * 7) + 1;
                    var maxDayInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);
                    var endDay = Math.Min(startDay + 6, maxDayInMonth);

                    startDate = new DateTime(selectedYear, selectedMonth, startDay);
                    endDate = new DateTime(selectedYear, selectedMonth, endDay);
                }
                else
                {
                    var today = DateTime.Today;
                    startDate = today.AddDays(-(int)today.DayOfWeek);
                    endDate = startDate.Value.AddDays(6);
                }
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                var daysDiff = (endDate.Value.Date - startDate.Value.Date).TotalDays;
                if (daysDiff < 0)
                {
                    return new HttpStatusCodeResult(400, "Start date cannot be greater than end date.");
                }

                if (daysDiff > 366)
                {
                    return new HttpStatusCodeResult(400, "Date range cannot exceed 1 year (365 days)");
                }
            }

            var sourceDataTask = Task.Run(() => dbCentralizedNotification.GetSourceDataSystemList());
            var scrapTask = dbScrap.GetScrapMasterAsync();
            var approvalTask = dbCentralizedNotification.GetApprovalListAsync();
            var employeeTask = Task.Run(() => dbCentralizedNotification.GetEmployeeMasterSSO());
            var partsTask = dbScrap.GetScrapPartsAllAsync();
            var remarksTask = Task.Run(() => dbScrap.GetScrapCodeRemarks());
            var typeScrapTask = dbScrap.GetTypeScrapAsync();

            await Task.WhenAll(sourceDataTask, scrapTask, approvalTask, employeeTask, partsTask, remarksTask, typeScrapTask);

            var sourceDataList = sourceDataTask.Result;
            var scrapList = scrapTask.Result;
            var approvalList = approvalTask.Result;
            var employees = employeeTask.Result;
            var allScrapParts = partsTask.Result;
            var remarksList = remarksTask.Result;
            var typeScrapList = typeScrapTask.Result;

            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var endExclusive = endDate.Value.Date.AddDays(1);
                scrapList = scrapList
                    .Where(s => s.CreatedDate.HasValue && s.CreatedDate.Value >= start && s.CreatedDate.Value < endExclusive)
                    .ToList();
            }

            if (!scrapList.Any())
            {
                return new HttpStatusCodeResult(404, "No data available for the selected date range");
            }

            var employeeDict = employees
                .Where(e => !string.IsNullOrWhiteSpace(e.Kpk))
                .GroupBy(e => e.Kpk)
                .ToDictionary(g => g.Key, g => g.First().Name ?? "Unknown");

            var remarksDict = remarksList
                .GroupBy(r => r.IdRemarks.ToString())
                .ToDictionary(g => g.Key, g => g.First().Remarks ?? "Deleted");

            var typeDict = typeScrapList
                .GroupBy(t => t.Type_ID)
                .ToDictionary(g => g.Key, g => g.First().Type_Desc ?? "Unknown");

            var scrapDict = scrapList
                .GroupBy(s => s.IdScrap)
                .ToDictionary(g => g.Key, g => g.First());

            var relevantScrapIds = new HashSet<string>(scrapDict.Keys);

            var partsByScrap = allScrapParts
                .Where(p => !string.IsNullOrEmpty(p.IdScrap) && relevantScrapIds.Contains(p.IdScrap))
                .GroupBy(p => p.IdScrap)
                .ToDictionary(g => g.Key, g => g.ToList());

            var approvalsBySource = approvalList
                .GroupBy(a => a.Centralized_SourceData_ID)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Centralized_ApprovalList_Step).ToList());

            var rows = new List<ScrapReportRow>();

            foreach (var src in sourceDataList)
            {
                if (!scrapDict.TryGetValue(src.Centralized_SourceData_Master_ID_Str, out var scrap))
                {
                    continue;
                }

                if (!partsByScrap.TryGetValue(scrap.IdScrap, out var parts) || parts.Count == 0)
                {
                    continue;
                }

                int docStatus = src.Centralized_SourceData_Master_Status;
                bool isDeletedDoc = docStatus == 16;

                string supervisor = "Unknown";
                string currentApprovalName = "-";

                if (approvalsBySource.TryGetValue(src.Centralized_SourceData_ID, out var approvalChain) && approvalChain.Any())
                {
                    var firstApprover = approvalChain
                        .OrderBy(a => a.Centralized_ApprovalList_Step)
                        .FirstOrDefault();

                    if (firstApprover != null && !string.IsNullOrWhiteSpace(firstApprover.Centralized_ApprovalList_KpkApproval))
                    {
                        employeeDict.TryGetValue(firstApprover.Centralized_ApprovalList_KpkApproval, out supervisor);
                        if (string.IsNullOrWhiteSpace(supervisor))
                        {
                            supervisor = "Unknown";
                        }
                    }

                    if (docStatus == 1)
                    {
                        var currentApprover = approvalChain
                            .Where(a => a.Centralized_StatusList_ID == 1)
                            .OrderBy(a => a.Centralized_ApprovalList_Step)
                            .FirstOrDefault();

                        if (currentApprover != null && !string.IsNullOrWhiteSpace(currentApprover.Centralized_ApprovalList_KpkApproval))
                        {
                            employeeDict.TryGetValue(currentApprover.Centralized_ApprovalList_KpkApproval, out currentApprovalName);
                            if (string.IsNullOrWhiteSpace(currentApprovalName))
                            {
                                currentApprovalName = "Unknown";
                            }
                        }
                    }
                }

                foreach (var part in parts)
                {
                    if (status.HasValue)
                    {
                        if (status.Value == 16)
                        {
                            if (!isDeletedDoc) continue;
                        }
                        else
                        {
                            if (isDeletedDoc) continue;
                            if (part.CurrentStatus != status.Value) continue;
                        }
                    }

                    string statusName;
                    if (isDeletedDoc)
                    {
                        statusName = "DELETED by INITIATOR";
                    }
                    else
                    {
                        switch (part.CurrentStatus)
                        {
                            case 1:
                                statusName = "Pending";
                                break;
                            case 2:
                                statusName = "Rejected";
                                break;
                            case 3:
                                statusName = "Active";
                                break;
                            case 5:
                                statusName = "Keyed In";
                                break;
                            default:
                                statusName = "Unknown";
                                break;
                        }
                    }

                    rows.Add(new ScrapReportRow
                    {
                        CreatedDate = scrap.CreatedDate ?? src.Centralized_SourceData_Master_CreatedDate,
                        DocumentID = scrap.IdScrap,
                        TypeName = typeDict.ContainsKey(scrap.Type_ID) ? typeDict[scrap.Type_ID] : "Unknown",
                        Facility = scrap.Facility,
                        InitiatorName = (!string.IsNullOrWhiteSpace(scrap.InitiatorKpk) && employeeDict.ContainsKey(scrap.InitiatorKpk))
                            ? employeeDict[scrap.InitiatorKpk]
                            : "Unknown",
                        SupervisorName = supervisor,
                        CurrentApprovalName = currentApprovalName,
                        TC = scrap.TC ?? "",
                        TcCompanion = scrap.SpecialCodeTcCompanion ?? "",
                        ScrapCode = scrap.ScrapCode ?? "",
                        WC = scrap.WC ?? "",
                        RnNumber = part.RnNumber ?? "",
                        PartNumber = part.PartNum ?? "",
                        Description = part.Description ?? "",
                        Measit = part.Measit ?? "",
                        Planit = part.Planit ?? "",
                        Cmidit = part.Cmidit ?? "",
                        Commit = part.Commit ?? "",
                        Quantity = part.Qty,
                        BasePrice = part.BasePrice ?? 0,
                        Total = part.Value,
                        RemarksName = (!string.IsNullOrEmpty(part.Remarks) && remarksDict.ContainsKey(part.Remarks))
                            ? remarksDict[part.Remarks]
                            : "Deleted",
                        LeaderKPK = part.LeaderKPK ?? "",
                        KeyInDateTime = part.KeyInDateTime,
                        Status = statusName,
                        SpecialCodeRemarks = scrap.SpecialCodeRemarks ?? "",
                        SpecialcodeRemarksParts = part.SpecialcodeRemarksParts ?? ""
                    });
                }
            }

            if (rows.Count == 0)
            {
                string statusMessage = status.HasValue ? $" with status {status.Value}" : "";
                return new HttpStatusCodeResult(404, $"No data available for the selected criteria{statusMessage}");
            }

            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("ScrapReport");

                ws.Cells["A1"].LoadFromArrays(new List<object[]>
                {
                    new object[]
                    {
                        "Created Date","DocumentID","Type Scrap","Facility","Initiator","Supervisor Name","Current Approval",
                        "TC","Special Code TcCompanion","Scrap Code","WC","RN",
                        "PartNumber","Description","UM","Planner Code","Commodity Code","Commodity Category",
                        "Quantity","Base Value","Total","Remarks Name","Leader KPK","Keyed In Date","Status",
                        "Special Code Remarks","Special Code Part Remarks"
                    }
                });

                using (var range = ws.Cells[1, 1, 1, 27])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                ws.Cells["A2"].LoadFromCollection(rows, false);
                int lastRow = rows.Count + 1;

                ws.Cells["A2:A" + lastRow]
                    .Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";

                ws.Cells["X2:X" + lastRow]
                    .Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                var bytes = package.GetAsByteArray();

                string filename;
                var currentDate = DateTime.Now;

                string statusSuffix = "";
                if (status.HasValue)
                {
                    switch (status.Value)
                    {
                        case 1:
                            statusSuffix = " [PENDING]";
                            break;
                        case 2:
                            statusSuffix = " [REJECTED]";
                            break;
                        case 3:
                            statusSuffix = " [ACTIVE]";
                            break;
                        case 5:
                            statusSuffix = " [KEYED IN]";
                            break;
                        case 16:
                            statusSuffix = " [DELETED]";
                            break;
                        default:
                            statusSuffix = " [UNKNOWN]";
                            break;
                    }
                }
                else
                {
                    statusSuffix = " [ALL]";
                }

                if (normalizedType == "weekly" && week.HasValue)
                {
                    int currentYear = year ?? currentDate.Year;
                    int currentMonth = month ?? currentDate.Month;
                    filename = $"ScrapReport_{currentYear}-{currentMonth:D2}-W{week.Value}.xlsx";
                }
                else if (normalizedType == "monthly" && month.HasValue)
                {
                    int currentYear = year ?? currentDate.Year;
                    filename = $"ScrapReport_{currentYear}-{month.Value:D2}.xlsx";
                }
                else if (normalizedType == "yearly" && startDate.HasValue && endDate.HasValue)
                {
                    filename = $"ScrapReport_{startDate.Value:yyyy-MM-dd}_to_{endDate.Value:yyyy-MM-dd}{statusSuffix}.xlsx";
                }
                else
                {
                    filename = "ScrapReport_" + currentDate.ToString("yyyyMMddHHmmss") + ".xlsx";
                }

                return File(
                    bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    filename
                );
            }
        }


        private async Task<List<ScrapPartRawDto>> GetRawScrapPartsJoinAsync()
        {
            return await AppCache.GetOrSetAsync(
                "SCRAP_PARTS_RAW_V1",
                async () =>
                {
                    var sourceDataList = dbCentralizedNotification.GetSourceDataSystemList();
                    var scrapList = await dbScrap.GetScrapMasterAsync();
                    var approvalList = await dbCentralizedNotification.GetApprovalListAsync();
                    var allScrapParts = await dbScrap.GetScrapPartsAllAsync();

                    var approvalDict = approvalList
                        .GroupBy(a => a.Centralized_SourceData_ID)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x.Centralized_ApprovalList_KpkApproval).ToHashSet()
                        );

                    return (
                        from src in sourceDataList
                        join scr in scrapList
                            on src.Centralized_SourceData_Master_ID_Str equals scr.IdScrap
                        from part in allScrapParts.Where(p => p.IdScrap == scr.IdScrap)
                        select new ScrapPartRawDto
                        {
                            Status = src.Centralized_SourceData_Master_Status,
                            InitiatorKpk = scr.InitiatorKpk,
                            PartNum = part.PartNum,
                            Description = part.Description,
                            Qty = (decimal)part.Qty,
                            Value = (decimal)part.Value,
                            ApproverSet = approvalDict.ContainsKey(src.Centralized_SourceData_ID)
                                ? approvalDict[src.Centralized_SourceData_ID]
                                : new HashSet<string>()
                        }
                    ).ToList();
                },
                minutes: 15
            );
        }

        [HttpGet]
        public async Task<JsonResult> GetScrapPartsSummary(
        string initiatorKpk = null,
        string approverKpk = null,
        string supervisorKpk = null,
        List<int> statusList = null)
        {
            string cacheKey =
                $"SCRAP_PART_SUMMARY::" +
                $"{initiatorKpk ?? "ALL"}::" +
                $"{approverKpk ?? "ALL"}::" +
                $"{supervisorKpk ?? "ALL"}::" +
                $"{(statusList != null ? string.Join("-", statusList.OrderBy(x => x)) : "ALL")}";

            var result = await AppCache.GetOrSetAsync(
                cacheKey,
                async () =>
                {

                    var rawData = await GetRawScrapPartsJoinAsync();

                    // MATERIALIZE SEKALI
                    var data = rawData.ToList();

                    HashSet<int> statusSet = null;
                    if (statusList?.Any() == true)
                        statusSet = new HashSet<int>(statusList);

                    HashSet<string> supervisorChain = null;
                    if (!string.IsNullOrEmpty(supervisorKpk))
                    {
                        supervisorChain = new HashSet<string>(
                            GetSubordinateChain(supervisorKpk)
                        );
                        supervisorChain.Add(supervisorKpk);
                    }

                    var grouped = data
                        .AsParallel()                 
                        .WithDegreeOfParallelism(
                            Math.Max(1, Environment.ProcessorCount - 1)
                        )
                        .Where(x =>
                            (statusSet == null || statusSet.Contains(x.Status)) &&
                            (initiatorKpk == null || x.InitiatorKpk == initiatorKpk) &&
                            (supervisorChain == null ||
                             x.ApproverSet.Any(a => supervisorChain.Contains(a)))
                        )
                        .GroupBy(x => new { x.PartNum, x.Description })
                        .Select(g => new
                        {
                            PartNum = g.Key.PartNum,
                            Description = g.Key.Description,
                            TotalQty = g.Sum(x => x.Qty),
                            TotalValue = g.Sum(x => x.Value)
                        })
                        .OrderByDescending(x => x.TotalValue)
                        .ToList();

                    return new
                    {
                        Data = grouped,
                        TotalItems = grouped.Count
                    };
                },
                minutes: 5
            );

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private async Task<List<ScrapPartRemarkRawDto>> GetRawScrapPartsByRemarksAsync()
        {
            return await AppCache.GetOrSetAsync(
                "SCRAP_PARTS_REMARKS_RAW_V2",
                async () =>
                {

                    var scrapTask = dbScrap.GetScrapMasterAsync();
                    var partTask = dbScrap.GetScrapPartsAllAsync();
                    var approvalTask = dbCentralizedNotification.GetApprovalListAsync();
                    var sourceTask = Task.Run(() => dbCentralizedNotification.GetSourceDataSystemList());

                    await Task.WhenAll(scrapTask, partTask, approvalTask, sourceTask);

                    var scraps = scrapTask.Result;
                    var parts = partTask.Result;
                    var approvals = approvalTask.Result;
                    var sources = sourceTask.Result;
                    var approvalDict = approvals
                        .GroupBy(a => a.Centralized_SourceData_ID)
                        .ToDictionary(
                            g => g.Key,
                            g => g
                                .Select(x => x.Centralized_ApprovalList_KpkApproval)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Select(x => x.Trim())
                                .ToHashSet()
                        );

                    var partByScrap = parts
                        .GroupBy(p => p.IdScrap)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    var result = (
                        from src in sources
                        join scr in scraps
                            on src.Centralized_SourceData_Master_ID_Str equals scr.IdScrap
                        where partByScrap.ContainsKey(scr.IdScrap)
                        from part in partByScrap[scr.IdScrap]
                        select new ScrapPartRemarkRawDto
                        {
                            SourceDataId = src.Centralized_SourceData_ID,
                            Status = src.Centralized_SourceData_Master_Status,
                            InitiatorKpk = scr.InitiatorKpk,
                            RemarksId = part.Remarks != null ? Convert.ToInt32(part.Remarks) : 0,
                            Qty = (decimal)part.Qty,
                            Value = (decimal)part.Value,
                            ApproverSet = approvalDict.ContainsKey(src.Centralized_SourceData_ID)
                                ? approvalDict[src.Centralized_SourceData_ID]
                                : EmptyApproverSet.Instance
                        }
                    ).ToList();

                    return result;
                },
                minutes: 15
            );
        }
        public static class EmptyApproverSet
        {
            public static readonly HashSet<string> Instance = new HashSet<string>();
        }
        [HttpGet]
        public async Task<JsonResult> GetScrapPartsSummaryByRemarks(
    string initiatorKpk = null,
    string approverKpk = null,
    string supervisorKpk = null,
    List<int> statusList = null)
        {
            string cacheKey =
                $"SCRAP_REMARK_SUMMARY::" +
                $"{initiatorKpk ?? "ALL"}::" +
                $"{approverKpk ?? "ALL"}::" +
                $"{supervisorKpk ?? "ALL"}::" +
                $"{(statusList != null ? string.Join("-", statusList.OrderBy(x => x)) : "ALL")}";

            var result = await AppCache.GetOrSetAsync(
                cacheKey,
                async () =>
                {

                    var rawData = await GetRawScrapPartsByRemarksAsync();
                    var data = rawData.ToList();
                    HashSet<int> statusSet = null;
                    if (statusList?.Any() == true)
                        statusSet = new HashSet<int>(statusList);

                    HashSet<string> supervisorChain = null;
                    if (!string.IsNullOrEmpty(supervisorKpk))
                    {
                        supervisorChain = new HashSet<string>(
                            GetSubordinateChain(supervisorKpk)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Select(x => x.Trim())
                        );
                        supervisorChain.Add(supervisorKpk.Trim());
                    }

                    var remarksList = dbScrap.GetScrapCodeRemarks() ?? new List<ScrapCodeRemarkModel>();
                    var remarksDict = remarksList.ToDictionary(r => r.IdRemarks, r => r.Remarks);
                    var grouped = data
                        .AsParallel()
                        .WithDegreeOfParallelism(
                            Math.Max(1, Environment.ProcessorCount - 1)
                        )
                        .Where(x =>
                            (statusSet == null || statusSet.Contains(x.Status)) &&
                            (initiatorKpk == null || x.InitiatorKpk == initiatorKpk) &&
                            (supervisorChain == null ||
                             x.ApproverSet.Any(a => supervisorChain.Contains(a)))
                        )
                        .GroupBy(x => x.RemarksId)
                        .Select(g => new
                        {
                            Remarks = remarksDict.ContainsKey(g.Key)
                                ? remarksDict[g.Key]
                                : "Unknown",
                            TotalQty = g.Sum(x => x.Qty),
                            TotalValue = g.Sum(x => x.Value)
                        })
                        .OrderByDescending(x => x.TotalValue)
                        .ToList();

                    return grouped;
                },
                minutes: 5
            );

            return Json(
                new { Data = result, TotalItems = result.Count },
                JsonRequestBehavior.AllowGet
            );
        }





        [HttpGet]
        public JsonResult GetSupervisorChain(string supervisorKpk)
        {
            var chain = GetSubordinateChain(supervisorKpk);
            chain.Add(supervisorKpk);
            return Json(chain, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public async Task<JsonResult> GetJoinedSourceDataScrapTotalPerSubordinate(
    string initiatorKpk = null,
    string approverKpk = null,
    string supervisorKpk = null,
    List<int> statusList = null,
    string period = null,   
    string scrapCode = null,
    string leaderKpk = null,
    int? monthClicked = null,
    int pageNumber = 1,
    int pageSize = 10)
        {
            string cacheKey = $"ScrapTotal_{initiatorKpk}_{approverKpk}_{supervisorKpk}_{scrapCode}_{leaderKpk}_{period}_{monthClicked}";

            ObjectCache cache = MemoryCache.Default;
            if (cache.Contains(cacheKey))
            {
                var cachedResult = cache.Get(cacheKey);
                return Json(cachedResult, JsonRequestBehavior.AllowGet);
            }
            var sourceDataList = dbCentralizedNotification.GetSourceDataSystemList();
            var scrapList = await dbScrap.GetScrapMasterAsync();
            var approvalList = await dbCentralizedNotification.GetApprovalListAsync();
            var employees = dbCentralizedNotification.GetEmployeeMasterSSO();
            var scrapParts = await dbScrap.GetScrapPartsAllAsync();
            var leaderSummary = scrapParts
                .GroupBy(p => Tuple.Create(p.IdScrap, p.LeaderKPK?.Trim()))
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        TotalQty = g.Sum(x => x.Qty),
                        TotalValue = g.Sum(x => x.Value)
                    }
                );

            var employeeDict = employees
                .GroupBy(e => e.Kpk.Trim())
                .ToDictionary(g => g.Key, g => g.First().Name?.Trim());

            var subordinates = new List<dynamic>();
            if (!string.IsNullOrEmpty(supervisorKpk))
            {
                subordinates = dbSSO.GetUsersBySupervisorKpk(supervisorKpk.Trim())
                    .Select(s => new
                    {
                        Kpk = s.Kpk?.Trim(),
                        Name = s.Name?.Trim(),
                        Supervisor = s.Supervisor?.Trim()
                    }).ToList<dynamic>();
            }

            // === 🔹 Join data utama ===
            var joinedData = from src in sourceDataList
                             join scr in scrapList on src.Centralized_SourceData_Master_ID_Str equals scr.IdScrap
                             join part in scrapParts on scr.IdScrap equals part.IdScrap into scrapPartGroup
                             from part in scrapPartGroup.DefaultIfEmpty()
                             let key = Tuple.Create(scr.IdScrap, part?.LeaderKPK?.Trim())
                             select new
                             {
                                 src.Centralized_SourceData_ID,
                                 src.Centralized_SourceData_Master_ID_Str,
                                 src.Centralized_SourceData_Master_Status,
                                 scr.IdScrap,
                                 scr.Facility,
                                 scr.TC,
                                 scr.InitiatorKpk,
                                 InitiatorName = employeeDict.ContainsKey(scr.InitiatorKpk?.Trim())
                                         ? employeeDict[scr.InitiatorKpk.Trim()]
                                         : "Unknown",
                                 scr.ScrapCode,
                                 scr.CurrentStatus,
                                 scr.CreatedDate,
                                 CreatedDateParsed = scr.CreatedDate,
                                 LeaderKPK = part != null ? part.LeaderKPK?.Trim() : null,
                                 TotalQty = (part != null && leaderSummary.ContainsKey(key))
                                     ? leaderSummary[key].TotalQty
                                     : 0,
                                 TotalValue = (part != null && leaderSummary.ContainsKey(key))
                                     ? leaderSummary[key].TotalValue
                                     : 0,
                                 Approvals = approvalList
                                     .Where(a => a.Centralized_SourceData_ID == src.Centralized_SourceData_ID)
                                     .Select(a => new
                                     {
                                         a.Centralized_ApprovalList_ID,
                                         Centralized_ApprovalList_KpkApproval = a.Centralized_ApprovalList_KpkApproval?.Trim(),
                                         a.Centralized_ApprovalList_Step,
                                         a.Centralized_StatusList_ID,
                                         a.Centralized_ApprovalList_Date,
                                         ApproverName = employeeDict.ContainsKey(a.Centralized_ApprovalList_KpkApproval?.Trim())
                                             ? employeeDict[a.Centralized_ApprovalList_KpkApproval.Trim()]
                                             : "Unknown"
                                     }).ToList()
                             };

            // === 🔹 Filter sesuai input ===
            if (!string.IsNullOrEmpty(initiatorKpk))
                joinedData = joinedData.Where(j => j.InitiatorKpk?.Trim() == initiatorKpk.Trim());

            if (statusList != null && statusList.Any())
                joinedData = joinedData.Where(j => statusList.Contains(j.Centralized_SourceData_Master_Status));

            if (!string.IsNullOrEmpty(scrapCode))
                joinedData = joinedData.Where(j => j.ScrapCode == scrapCode);

            if (!string.IsNullOrEmpty(leaderKpk))
                joinedData = joinedData.Where(j => j.LeaderKPK != null && j.LeaderKPK.Trim() == leaderKpk.Trim());

            // === 🔹 Filter period ===
            if (!string.IsNullOrEmpty(period))
            {
                var now = DateTime.Now;
                switch (period.ToLower())
                {
                    case "weekly":
                        if (monthClicked.HasValue)
                        {
                            var startMonth = new DateTime(now.Year, monthClicked.Value + 1, 1);
                            var endMonth = startMonth.AddMonths(1);
                            joinedData = joinedData.Where(j => j.CreatedDateParsed >= startMonth && j.CreatedDateParsed < endMonth);
                        }
                        else
                        {
                            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
                            var endOfWeek = startOfWeek.AddDays(7);
                            joinedData = joinedData.Where(j => j.CreatedDateParsed >= startOfWeek && j.CreatedDateParsed < endOfWeek);
                        }
                        break;

                    case "monthly":
                        var startOfYear = new DateTime(now.Year, 1, 1);
                        var endOfYear = startOfYear.AddYears(1);
                        joinedData = joinedData.Where(j => j.CreatedDateParsed >= startOfYear && j.CreatedDateParsed < endOfYear);
                        break;

                    case "yearly":
                        var startOfFiveYear = new DateTime(now.Year - 4, 1, 1);
                        joinedData = joinedData.Where(j => j.CreatedDateParsed >= startOfFiveYear);
                        break;
                }
            }

            // === 🔹 Hasil akhir ===
            object result;
            if (!string.IsNullOrEmpty(scrapCode))
            {
                // === Mode Leader ===
                result = joinedData
                    .Where(j => !string.IsNullOrEmpty(j.LeaderKPK))
                    .GroupBy(j => j.LeaderKPK.Trim())
                    .Select(g => new
                    {
                        LeaderKpk = g.Key,
                        LeaderName = employeeDict.ContainsKey(g.Key) ? employeeDict[g.Key] : "Unknown",
                        ScrapData = g.GroupBy(x => x.IdScrap)
                                     .Select(doc => new
                                     {
                                         IdScrap = doc.Key,
                                         DocumentData = doc.FirstOrDefault(),
                                         TotalQty = doc.FirstOrDefault()?.TotalQty ?? 0,
                                         TotalValue = doc.FirstOrDefault()?.TotalValue ?? 0
                                     })
                                     .ToList(),
                        GrandTotalQty = g.GroupBy(x => x.IdScrap).Sum(doc => doc.FirstOrDefault()?.TotalQty ?? 0),
                        GrandTotalValue = g.GroupBy(x => x.IdScrap).Sum(doc => doc.FirstOrDefault()?.TotalValue ?? 0)
                    })
                    .ToList();
            }
            else
            {
               
                result = subordinates.Select(sub =>
                {
                    var scrapDataForSub = joinedData
                        .Where(j => j.Approvals.Any(a => a.Centralized_ApprovalList_KpkApproval == sub.Kpk))
                        .ToList();

                    var groupedScrapData = scrapDataForSub
                        .GroupBy(x => x.IdScrap)
                        .Select(doc => new
                        {
                            IdScrap = doc.Key,
                            DocumentData = doc.FirstOrDefault(),
                            TotalQty = doc.FirstOrDefault()?.TotalQty ?? 0,
                            TotalValue = doc.FirstOrDefault()?.TotalValue ?? 0
                        })
                        .ToList();

                    return new
                    {
                        SubordinateKpk = sub.Kpk,
                        SubordinateName = sub.Name,
                        Supervisor = sub.Supervisor,
                        ScrapData = groupedScrapData,
                        GrandTotalQty = groupedScrapData.Sum(j => j.TotalQty),
                        GrandTotalValue = groupedScrapData.Sum(j => j.TotalValue)
                    };
                }).ToList();
            }

            var finalResult = new { Data = result };
            cache.Set(cacheKey, finalResult, DateTimeOffset.Now.AddMinutes(5));
            return Json(finalResult, JsonRequestBehavior.AllowGet);
        }

      
        public JsonResult GetScrapCodeSummaryCount()
        {
            try
            {
                var allScrapCodes = dbScrap.GetAllScrapCodes();

                
                var locationCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var code in allScrapCodes)
                {
                    if (string.IsNullOrWhiteSpace(code.Location))
                        continue;

                   
                    var locations = code.Location.Split(',')
                        .Select(loc => loc.Trim())
                        .Where(loc => !string.IsNullOrEmpty(loc));

                    foreach (var loc in locations)
                    {
                        if (locationCounts.ContainsKey(loc))
                            locationCounts[loc]++;
                        else
                            locationCounts[loc] = 1;
                    }
                }
                var summary = locationCounts
                    .Select(kvp => new LocationScrapSummaryCount
                    {
                        Location = kvp.Key,
                        TotalScrap = kvp.Value
                    })
                    .OrderBy(s => s.Location)
                    .ToList();

                // Tambahkan total keseluruhan
                summary.Add(new LocationScrapSummaryCount
                {
                    Location = "Overall Total",
                    TotalScrap = allScrapCodes.Count
                });

                return Json(new
                {
                    success = true,
                    data = summary
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}