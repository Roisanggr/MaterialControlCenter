using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Web.Services.Description;
using MaterialControlCenter.Models;
using System.Data.SqlClient;
using System.Globalization;

namespace MaterialControlCenter.Controllers
{
    public class ListOfDataController : BaseController
    {
       
        [HttpGet]
        public JsonResult GetSourceDataSystemList()
        {
            var sourceDataList = dbCentralizedNotification.GetSourceDataSystemList();
            return Json(sourceDataList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetScrapMaster()
        {
            var scrapList = await dbScrap.GetScrapMasterAsync();

            return Json(scrapList, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetAllUsersWithRoleName(int pageNumber = 1, int pageSize = 10, string kpk = "", string name = "")
        {
            try
            {
                List<UserToolRoomModel> users = dbScrap.GetAllUsersScrapNoFilter();
                List<RoleModel> roles = dbScrap.GetAllRoles();
                List<EmployeeMasterModelSSO> employees = dbSSO.GetEmployeeMasterSSO();

                var result = users.Select(user =>
                {
                    var emp = employees.FirstOrDefault(e => e.Kpk == user.Kpk);
                    return new
                    {
                        user.Kpk,
                        Name = emp?.Name ?? "N/A",
                        user.RoleId,
                        RoleName = roles.FirstOrDefault(r => r.RoleId == user.RoleId)?.Name ?? "N/A",
                        user.Facility,
                        user.IsActive,
                        user.TC,
                        user.ScrapCodeResponsible
                    };
                });

                // 🔎 Filter
                if (!string.IsNullOrEmpty(kpk))
                {
                    string kpkLower = kpk.ToLower();
                    result = result.Where(u => (u.Kpk ?? "").ToLower().Contains(kpkLower));
                }

                if (!string.IsNullOrEmpty(name))
                {
                    string nameLower = name.ToLower();
                    result = result.Where(u => (u.Name ?? "").ToLower().Contains(nameLower));
                }
                result = result.OrderBy(u => u.Name);

                int totalCount = result.Count();
                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                if (pageNumber > totalPages && totalPages > 0)
                    pageNumber = totalPages;

                var pagedResult = result
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = pagedResult,
                    totalCount,
                    currentPage = pageNumber,
                    totalPages
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet]
        public ActionResult GetAllRoles()
        {
            try
            {
                // Ambil data role dari service/db
                List<RoleModel> roles = dbScrap.GetAllRoles();

                // Kembalikan sebagai JSON
                return Json(roles, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, $"Internal server error: {ex.Message}");
            }
        }



        //[HttpGet]
        //public async Task<JsonResult> GetJoinedSourceDataScrap(
        //    string initiatorKpk = null,
        //    string approverKpk = null,
        //    string approverKpkPdf = null,
        //    List<int> statusList = null,
        //    string documentId = null,
        //    DateTime? createdDate = null,
        //    string scrapCode = null,
        //    decimal? totalFrom = null,
        //    decimal? totalTo = null,
        //    string facility = null,
        //    DateTime? createdDateFrom = null,
        //    DateTime? createdDateTo = null,
        //    string tc = null,
        //    int pageNumber = 1,
        //    int pageSize = 10)
        //{

        //    var sourceDataTask = Task.Run(() => dbCentralizedNotification.GetSourceDataSystemList());
        //    var employeeTask = Task.Run(() => dbCentralizedNotification.GetEmployeeMasterSSO());

        //    var scrapTask = dbScrap.GetScrapMasterAsync();
        //    var approvalTask = dbCentralizedNotification.GetApprovalListAsync();
        //    var scrapPartsTask = dbScrap.GetScrapPartsAllAsync();

        //    await Task.WhenAll(
        //        sourceDataTask,
        //        employeeTask,
        //        scrapTask,
        //        approvalTask,
        //        scrapPartsTask
        //    );

        //    var sourceDataList = sourceDataTask.Result;
        //    var employees = employeeTask.Result;
        //    var scrapList = scrapTask.Result;
        //    var approvalList = approvalTask.Result;
        //    var allScrapParts = scrapPartsTask.Result;
        //    var employeeDict = employees
        //        .Where(e => !string.IsNullOrEmpty(e.Kpk))
        //        .GroupBy(e => e.Kpk.Trim())
        //        .ToDictionary(g => g.Key, g => g.First().Name);

        //    var approvalLookup = approvalList
        //        .GroupBy(a => a.Centralized_SourceData_ID)
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.OrderBy(x => x.Centralized_ApprovalList_Step)
        //                  .Select(a => new ApprovalDtoV2Fastest
        //                  {
        //                      Centralized_ApprovalList_ID = a.Centralized_ApprovalList_ID,
        //                      Centralized_SourceData_ID = a.Centralized_SourceData_ID,
        //                      Centralized_ApprovalList_Step = a.Centralized_ApprovalList_Step,
        //                      Centralized_StatusList_ID = a.Centralized_StatusList_ID,
        //                      Centralized_ApprovalList_Date = a.Centralized_ApprovalList_Date,
        //                      Kpk = a.Centralized_ApprovalList_KpkApproval,
        //                      ApproverName = employeeDict.TryGetValue(
        //                          a.Centralized_ApprovalList_KpkApproval,
        //                          out var name) ? name : "Unknown"
        //                  })
        //                  .ToList()
        //        );

        //    var scrapPartsLookup = allScrapParts
        //        .GroupBy(p => p.IdScrap)
        //        .ToDictionary(g => g.Key, g => new
        //        {
        //            Parts = g.ToList(),
        //            TotalQty = g.Sum(x => x.Qty),
        //            TotalValue = g.Sum(x => x.Value)
        //        });
        //    var joinedList =
        //        (from src in sourceDataList
        //         join scr in scrapList
        //            on src.Centralized_SourceData_Master_ID_Str equals scr.IdScrap
        //         let scrapAgg = scrapPartsLookup.ContainsKey(scr.IdScrap)
        //             ? scrapPartsLookup[scr.IdScrap]
        //             : null
        //         select new
        //         {
        //             src.Centralized_SourceData_ID,
        //             src.Centralized_SourceData_Master_ID_Str,
        //             src.Centralized_SourceData_Master_Status,
        //             src.Centralized_SourceData_Master_CreatedDate,

        //             scr.Facility,
        //             scr.TC,
        //             scr.InitiatorKpk,
        //             InitiatorName = employeeDict.TryGetValue(scr.InitiatorKpk, out var initName)
        //                 ? initName
        //                 : "Unknown",
        //             scr.ScrapCode,
        //             scr.CreatedDate,
        //             scr.CurrentStatus,
        //             scr.WC,

        //             Approvals = approvalLookup.TryGetValue(
        //                 src.Centralized_SourceData_ID,
        //                 out var apps)
        //                 ? apps
        //                 : new List<ApprovalDtoV2Fastest>(),

        //             ScrapParts = scrapAgg?.Parts ?? new List<ScrapPartModel>(),
        //             TotalQty = scrapAgg?.TotalQty ?? 0,
        //             TotalValue = scrapAgg?.TotalValue ?? 0
        //         }).ToList();

        //    if (!string.IsNullOrEmpty(documentId))
        //    {
        //        var docId = documentId.Trim();

        //        joinedList = joinedList
        //            .Where(x => !string.IsNullOrEmpty(x.Centralized_SourceData_Master_ID_Str) &&
        //                        x.Centralized_SourceData_Master_ID_Str.Contains(docId))
        //            .ToList();
        //    }

        //    if (!string.IsNullOrEmpty(initiatorKpk))
        //        joinedList = joinedList
        //            .Where(x => x.InitiatorKpk == initiatorKpk)
        //            .ToList();

        //    if (!string.IsNullOrEmpty(approverKpk))
        //        joinedList = joinedList.Where(j =>
        //        {
        //            if (j.Centralized_SourceData_Master_Status != 1)
        //                return false;

        //            var approvals = j.Approvals;
        //            for (int i = 0; i < approvals.Count; i++)
        //            {
        //                var current = approvals[i];
        //                if (current.Kpk == approverKpk)
        //                {
        //                    if (current.Centralized_StatusList_ID != 1)
        //                        return false;

        //                    return approvals
        //                        .Take(i)
        //                        .All(p => p.Centralized_StatusList_ID != 1);
        //                }
        //            }
        //            return false;
        //        }).ToList();

        //    if (statusList != null && statusList.Any())
        //        joinedList = joinedList
        //            .Where(x => statusList.Contains(x.Centralized_SourceData_Master_Status))
        //            .ToList();

        //    if (!string.IsNullOrEmpty(scrapCode))
        //    {
        //        var code = scrapCode.Trim().ToLower();
        //        joinedList = joinedList
        //            .Where(x => !string.IsNullOrEmpty(x.ScrapCode) &&
        //                        x.ScrapCode.ToLower().Contains(code))
        //            .ToList();
        //    }

        //    if (totalFrom.HasValue)
        //        joinedList = joinedList
        //            .Where(x => x.TotalValue >= totalFrom.Value)
        //            .ToList();

        //    if (totalTo.HasValue)
        //        joinedList = joinedList
        //            .Where(x => x.TotalValue <= totalTo.Value)
        //            .ToList();

        //    // === Filter by Facility ===
        //    if (!string.IsNullOrEmpty(facility))
        //        joinedList = joinedList
        //            .Where(x => !string.IsNullOrEmpty(x.Facility) &&
        //                        x.Facility.Contains(facility))
        //            .ToList();

        //    // === Filter CreatedDate Range ===
        //    if (createdDateFrom.HasValue)
        //        joinedList = joinedList
        //            .Where(x => x.CreatedDate.HasValue &&
        //                        x.CreatedDate.Value.Date >= createdDateFrom.Value.Date)
        //            .ToList();

        //    if (createdDateTo.HasValue)
        //        joinedList = joinedList
        //            .Where(x => x.CreatedDate.HasValue &&
        //                        x.CreatedDate.Value.Date <= createdDateTo.Value.Date)
        //            .ToList();

        //    // === Filter by TC ===
        //    if (!string.IsNullOrEmpty(tc))
        //        joinedList = joinedList
        //            .Where(x => !string.IsNullOrEmpty(x.TC) &&
        //                        x.TC.Contains(tc))
        //            .ToList();

        //    if (!string.IsNullOrWhiteSpace(approverKpkPdf))
        //    {
        //        joinedList = joinedList
        //            .Where(j =>
        //            {

        //                if (j.Centralized_SourceData_Master_Status != 1)
        //                    return false;

        //                var approvals = j.Approvals
        //                    .OrderBy(a => a.Centralized_ApprovalList_Step)
        //                    .ToList();

        //                if (!approvals.Any())
        //                    return false;


        //                bool containsApprover = approvals
        //                    .Any(a => a.Kpk == approverKpkPdf);

        //                if (!containsApprover)
        //                    return false;


        //                var pendingSteps = approvals
        //                    .Where(a => a.Centralized_StatusList_ID == 1)
        //                    .OrderBy(a => a.Centralized_ApprovalList_Step)
        //                    .ToList();


        //                if (pendingSteps.Count != 1)
        //                    return false;

        //                var pending = pendingSteps[0];
        //                bool allPreviousApproved = approvals
        //                    .Where(a => a.Centralized_ApprovalList_Step < pending.Centralized_ApprovalList_Step)
        //                    .All(a => a.Centralized_StatusList_ID == 2);

        //                return allPreviousApproved;
        //            })
        //            .ToList();
        //    }

        //    var totalCount = joinedList.Count;

        //    var pagedData = joinedList
        //        .OrderByDescending(x => x.Centralized_SourceData_Master_CreatedDate)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToList();

        //    return Json(new
        //    {
        //        TotalCount = totalCount,
        //        PageNumber = pageNumber,
        //        PageSize = pageSize,
        //        Data = pagedData
        //    }, JsonRequestBehavior.AllowGet);
        //}
        private static HashSet<string> GetEffectiveApproverKpks(
    string approverKpk,
    List<UserDelegate> delegates)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(approverKpk))
                return result;

            // KPK diri sendiri
            result.Add(approverKpk.Trim());

            // KPK yang mendelegasikan ke dia
            var delegatedFromUsers = delegates
                .Where(d => d.DelegateKpk.Equals(approverKpk, StringComparison.OrdinalIgnoreCase))
                .Select(d => d.UserKpk)
                .Where(kpk => !string.IsNullOrWhiteSpace(kpk));

            foreach (var kpk in delegatedFromUsers)
                result.Add(kpk.Trim());

            return result;
        }

        [HttpGet]
        public async Task<JsonResult> GetJoinedSourceDataScrap(
    string initiatorKpk = null,
    string approverKpk = null,
    string approverKpkPdf = null,
    List<int> statusList = null,
    string documentId = null,
    DateTime? createdDate = null,
    string scrapCode = null,
    decimal? totalFrom = null,
    decimal? totalTo = null,
    string facility = null,
    DateTime? createdDateFrom = null,
    DateTime? createdDateTo = null,
    string tc = null,
    bool includeDelegates = false,
    int pageNumber = 1,
    int pageSize = 10)
        {

            var sourceDataTask = Task.Run(() => dbCentralizedNotification.GetSourceDataSystemList());
            var employeeTask = Task.Run(() => dbCentralizedNotification.GetEmployeeMasterSSO());

            var scrapTask = dbScrap.GetScrapMasterAsync();
            var approvalTask = dbCentralizedNotification.GetApprovalListAsync();
            var scrapPartsTask = dbScrap.GetScrapPartsAllAsync();
            var delegateTask = dbScrap.GetUserDelegatesAsync();


            await Task.WhenAll(
                sourceDataTask,
                employeeTask,
                scrapTask,
                approvalTask,
                scrapPartsTask,
                delegateTask
            );
            var allDelegates = delegateTask.Result;
            var sourceDataList = sourceDataTask.Result;
            var employees = employeeTask.Result;
            var scrapList = scrapTask.Result;
            var approvalList = approvalTask.Result;
            var allScrapParts = scrapPartsTask.Result;
            var employeeDict = employees
                .Where(e => !string.IsNullOrEmpty(e.Kpk))
                .GroupBy(e => e.Kpk.Trim())
                .ToDictionary(g => g.Key, g => g.First().Name);

            var approvalLookup = approvalList
                .GroupBy(a => a.Centralized_SourceData_ID)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.Centralized_ApprovalList_Step)
                          .Select(a => new ApprovalDtoV2Fastest
                          {
                              Centralized_ApprovalList_ID = a.Centralized_ApprovalList_ID,
                              Centralized_SourceData_ID = a.Centralized_SourceData_ID,
                              Centralized_ApprovalList_Step = a.Centralized_ApprovalList_Step,
                              Centralized_StatusList_ID = a.Centralized_StatusList_ID,
                              Centralized_ApprovalList_Date = a.Centralized_ApprovalList_Date,
                              Kpk = a.Centralized_ApprovalList_KpkApproval,
                              ApproverName = employeeDict.TryGetValue(
                                  a.Centralized_ApprovalList_KpkApproval,
                                  out var name) ? name : "Unknown"
                          })
                          .ToList()
                );

            var scrapPartsLookup = allScrapParts
                .GroupBy(p => p.IdScrap)
                .ToDictionary(g => g.Key, g => new
                {
                    Parts = g.ToList(),
                    TotalQty = g.Sum(x => x.Qty),
                    TotalValue = g.Sum(x => x.Value)
                });
            var joinedList =
                (from src in sourceDataList
                 join scr in scrapList
                    on src.Centralized_SourceData_Master_ID_Str equals scr.IdScrap
                 let scrapAgg = scrapPartsLookup.ContainsKey(scr.IdScrap)
                     ? scrapPartsLookup[scr.IdScrap]
                     : null
                 select new
                 {
                     src.Centralized_SourceData_ID,
                     src.Centralized_SourceData_Master_ID_Str,
                     src.Centralized_SourceData_Master_Status,
                     src.Centralized_SourceData_Master_CreatedDate,

                     scr.Facility,
                     scr.TC,
                     scr.InitiatorKpk,
                     InitiatorName = employeeDict.TryGetValue(scr.InitiatorKpk, out var initName)
                         ? initName
                         : "Unknown",
                     scr.ScrapCode,
                     scr.CreatedDate,
                     scr.CurrentStatus,
                     scr.WC,
                     scr.SpecialCodeTcCompanion,
                     scr.DeletedAt,

                     Approvals = approvalLookup.TryGetValue(
                         src.Centralized_SourceData_ID,
                         out var apps)
                         ? apps
                         : new List<ApprovalDtoV2Fastest>(),

                     ScrapParts = scrapAgg?.Parts ?? new List<ScrapPartModel>(),
                     TotalQty = scrapAgg?.TotalQty ?? 0,
                     TotalValue = scrapAgg?.TotalValue ?? 0
                 }).ToList();

            if (!string.IsNullOrEmpty(documentId))
            {
                var docId = documentId.Trim();

                joinedList = joinedList
                    .Where(x => !string.IsNullOrEmpty(x.Centralized_SourceData_Master_ID_Str) &&
                                x.Centralized_SourceData_Master_ID_Str.Contains(docId))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(initiatorKpk))
                joinedList = joinedList
                    .Where(x => x.InitiatorKpk == initiatorKpk)
                    .ToList();
            if (!string.IsNullOrEmpty(approverKpk))
            {
                HashSet<string> effectiveApprovers;

                if (includeDelegates)
                {
                    effectiveApprovers = GetEffectiveApproverKpks(approverKpk, allDelegates);
                }
                else
                {
                    effectiveApprovers = new HashSet<string>(
                        new[] { approverKpk.Trim() },
                        StringComparer.OrdinalIgnoreCase
                    );
                }

                joinedList = joinedList.Where(j =>
                {
                    if (j.Centralized_SourceData_Master_Status != 1)
                        return false;

                    var approvals = j.Approvals;

                    for (int i = 0; i < approvals.Count; i++)
                    {
                        var current = approvals[i];

                        if (effectiveApprovers.Contains(current.Kpk))
                        {
                            if (current.Centralized_StatusList_ID != 1)
                                return false;

                            return approvals
                                .Take(i)
                                .All(p => p.Centralized_StatusList_ID != 1);
                        }
                    }
                    return false;
                }).ToList();
            }



            if (statusList != null && statusList.Any())
                joinedList = joinedList
                    .Where(x => statusList.Contains(x.Centralized_SourceData_Master_Status))
                    .ToList();

            if (!string.IsNullOrEmpty(scrapCode))
            {
                var code = scrapCode.Trim().ToLower();
                joinedList = joinedList
                    .Where(x => !string.IsNullOrEmpty(x.ScrapCode) &&
                                x.ScrapCode.ToLower().Contains(code))
                    .ToList();
            }

            if (totalFrom.HasValue)
                joinedList = joinedList
                    .Where(x => x.TotalValue >= totalFrom.Value)
                    .ToList();

            if (totalTo.HasValue)
                joinedList = joinedList
                    .Where(x => x.TotalValue <= totalTo.Value)
                    .ToList();

            // === Filter by Facility ===
            if (!string.IsNullOrEmpty(facility))
                joinedList = joinedList
                    .Where(x => !string.IsNullOrEmpty(x.Facility) &&
                                x.Facility.Contains(facility))
                    .ToList();

            // === Filter CreatedDate Range ===
            if (createdDateFrom.HasValue)
                joinedList = joinedList
                    .Where(x => x.CreatedDate.HasValue &&
                                x.CreatedDate.Value.Date >= createdDateFrom.Value.Date)
                    .ToList();

            if (createdDateTo.HasValue)
                joinedList = joinedList
                    .Where(x => x.CreatedDate.HasValue &&
                                x.CreatedDate.Value.Date <= createdDateTo.Value.Date)
                    .ToList();

            // === Filter by TC ===
            if (!string.IsNullOrEmpty(tc))
                joinedList = joinedList
                    .Where(x => !string.IsNullOrEmpty(x.TC) &&
                                x.TC.Contains(tc))
                    .ToList();

            if (!string.IsNullOrWhiteSpace(approverKpkPdf))
            {
                joinedList = joinedList
                    .Where(j =>
                    {

                        if (j.Centralized_SourceData_Master_Status != 1)
                            return false;

                        var approvals = j.Approvals
                            .OrderBy(a => a.Centralized_ApprovalList_Step)
                            .ToList();

                        if (!approvals.Any())
                            return false;


                        bool containsApprover = approvals
                            .Any(a => a.Kpk == approverKpkPdf);

                        if (!containsApprover)
                            return false;


                        var pendingSteps = approvals
                            .Where(a => a.Centralized_StatusList_ID == 1)
                            .OrderBy(a => a.Centralized_ApprovalList_Step)
                            .ToList();


                        if (pendingSteps.Count != 1)
                            return false;

                        var pending = pendingSteps[0];
                        bool allPreviousApproved = approvals
                            .Where(a => a.Centralized_ApprovalList_Step < pending.Centralized_ApprovalList_Step)
                            .All(a => a.Centralized_StatusList_ID == 2);

                        return allPreviousApproved;
                    })
                    .ToList();
            }

            var totalCount = joinedList.Count;

            var pagedData = joinedList
                .OrderByDescending(x => x.Centralized_SourceData_Master_CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Json(new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = pagedData
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetScrapTotals(
      string initiatorKpk = null,
      string approverKpk = null,
       bool includeDelegates = false,
      List<int> statusList = null)
        {
            // ================= PARALLEL FETCH =================
            var sourceTask = Task.Run(() =>
                dbCentralizedNotification.GetSourceDataSystemList());
            var delegateTask = dbScrap.GetUserDelegatesAsync();

            var scrapTask = dbScrap.GetScrapMasterAsync();
            var approvalTask = dbCentralizedNotification.GetApprovalListAsync();
            var scrapPartsTask = dbScrap.GetScrapPartsAllAsync();

            await Task.WhenAll(
                sourceTask,
                scrapTask,
                approvalTask,
                scrapPartsTask,
                delegateTask
            );

            var sourceDataList = sourceTask.Result;
            var scrapList = scrapTask.Result;
            var approvalList = approvalTask.Result;
            var allScrapParts = scrapPartsTask.Result;
            var allDelegates = delegateTask.Result;

            var approvalLookup = approvalList
                .GroupBy(a => a.Centralized_SourceData_ID)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.Centralized_ApprovalList_Step).ToList()
                );
            var scrapAggLookup = allScrapParts
                .GroupBy(p => p.IdScrap)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Qty = g.Sum(x => x.Qty),
                        Value = g.Sum(x => x.Value)
                    });
            var joinedList =
                (from src in sourceDataList
                 join scr in scrapList
                    on src.Centralized_SourceData_Master_ID_Str equals scr.IdScrap
                 let agg = scrapAggLookup.TryGetValue(scr.IdScrap, out var a) ? a : null
                 select new
                 {
                     src.Centralized_SourceData_Master_Status,
                     scr.InitiatorKpk,
                     Approvals = approvalLookup.TryGetValue(
                         src.Centralized_SourceData_ID,
                         out var apps)
                         ? apps
                         : new List<CentralizedApprovalListModel>(),

                     TotalQty = agg?.Qty ?? 0,
                     TotalValue = agg?.Value ?? 0
                 }).ToList();

            // ================= FILTER =================
            if (!string.IsNullOrEmpty(initiatorKpk))
                joinedList = joinedList
                    .Where(x => x.InitiatorKpk == initiatorKpk)
                    .ToList();

            if (statusList != null && statusList.Any())
                joinedList = joinedList
                    .Where(x => statusList.Contains(x.Centralized_SourceData_Master_Status))
                    .ToList();
            HashSet<string> effectiveApprovers = null;

            if (!string.IsNullOrEmpty(approverKpk))
            {
                effectiveApprovers = includeDelegates
                    ? GetEffectiveApproverKpks(approverKpk, allDelegates)
                    : new HashSet<string>(new[] { approverKpk }, StringComparer.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrEmpty(approverKpk))
                joinedList = joinedList.Where(j =>
                {
                    if (j.Centralized_SourceData_Master_Status != 1 &&
                        j.Centralized_SourceData_Master_Status != 4 &&
                        j.Centralized_SourceData_Master_Status != 9)
                        return false;

                    var approvals = j.Approvals
                        .OrderBy(a => a.Centralized_ApprovalList_Step)
                        .ToList();

                    for (int i = 0; i < approvals.Count; i++)
                    {
                        var cur = approvals[i];

                        if (effectiveApprovers.Contains(cur.Centralized_ApprovalList_KpkApproval))
                        {
                            if (cur.Centralized_StatusList_ID == 1 ||
                                cur.Centralized_StatusList_ID == 2)
                            {
                                return approvals
                                    .Take(i)
                                    .All(p => p.Centralized_StatusList_ID != 1);
                            }
                        }
                    }
                    return false;
                }).ToList();


            // ================= AGGREGATION =================
            var totalQtyAll = joinedList.Sum(x => x.TotalQty);
            var totalValueAll = joinedList.Sum(x => x.TotalValue);

            decimal totalQtyPending = 0, totalQtyApproved = 0;
            decimal totalValuePending = 0, totalValueApproved = 0;
            if (!string.IsNullOrEmpty(approverKpk))
            {
                totalQtyPending = joinedList
                    .Where(x => x.Approvals.Any(a =>
                        effectiveApprovers.Contains(a.Centralized_ApprovalList_KpkApproval) &&
                        a.Centralized_StatusList_ID == 1))
                    .Sum(x => x.TotalQty);

                totalValuePending = joinedList
                    .Where(x => x.Approvals.Any(a =>
                        effectiveApprovers.Contains(a.Centralized_ApprovalList_KpkApproval) &&
                        a.Centralized_StatusList_ID == 1))
                    .Sum(x => x.TotalValue);

                totalQtyApproved = joinedList
                    .Where(x => x.Approvals.Any(a =>
                        effectiveApprovers.Contains(a.Centralized_ApprovalList_KpkApproval) &&
                        a.Centralized_StatusList_ID == 2))
                    .Sum(x => x.TotalQty);

                totalValueApproved = joinedList
                    .Where(x => x.Approvals.Any(a =>
                        effectiveApprovers.Contains(a.Centralized_ApprovalList_KpkApproval) &&
                        a.Centralized_StatusList_ID == 2))
                    .Sum(x => x.TotalValue);
            }

            else
            {
                totalQtyPending = joinedList
                    .Where(x => x.Centralized_SourceData_Master_Status == 1)
                    .Sum(x => x.TotalQty);

                totalValuePending = joinedList
                    .Where(x => x.Centralized_SourceData_Master_Status == 1)
                    .Sum(x => x.TotalValue);

                totalQtyApproved = joinedList
                    .Where(x => x.Centralized_SourceData_Master_Status == 4 ||
                                x.Centralized_SourceData_Master_Status == 9)
                    .Sum(x => x.TotalQty);

                totalValueApproved = joinedList
                    .Where(x => x.Centralized_SourceData_Master_Status == 4 ||
                                x.Centralized_SourceData_Master_Status == 9)
                    .Sum(x => x.TotalValue);
            }

            return Json(new
            {
                TotalQtyAll = totalQtyAll,
                TotalValueAll = totalValueAll,
                TotalQtyPending = totalQtyPending,
                TotalValuePending = totalValuePending,
                TotalQtyApproved = totalQtyApproved,
                TotalValueApproved = totalValueApproved
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetScrapPartsPaged(
    string idScrap, int page = 1, int pageSize = 10,
    string sortBy = "Value", string sortOrder = "desc",
    string partNumber = "", string leader = "", string remarks = "",
    decimal? totalFrom = null, decimal? totalTo = null)
        {
            var scrapParts = await dbScrap.GetScrapPartsByScrapIdAsync(idScrap);
            var remarksList = dbScrap.GetScrapCodeRemarks();
            var employees = dbScrap.GetEmployeeMasterSSO();

            var scrapPartsWithNames = scrapParts.Select(p =>
            {
                int idRemarks;
                bool isValid = int.TryParse(p.Remarks, out idRemarks);
                var remarkName = isValid
                    ? remarksList.FirstOrDefault(r => r.IdRemarks == idRemarks)?.Remarks
                    : null;

                var leaderName = !string.IsNullOrEmpty(p.LeaderKPK)
                    ? employees.FirstOrDefault(e => e.Kpk == p.LeaderKPK)?.Name
                    : null;

                return new
                {
                    p.IdScrap,
                    p.PartNum,
                    p.Description,
                    Qty = Math.Round(p.Qty, 2),
                    p.Value,
                    p.Commit,
                    p.Measit,
                    p.Planit,
                    p.RnNumber,
                    Remarks = remarkName ?? "N/A",
                    p.CurrentStatus,
                    p.LeaderKPK,
                    LeaderName = (leaderName ?? "N/A").Trim()
                };
            });


            // 🔍 Apply filters
            if (!string.IsNullOrEmpty(partNumber))
                scrapPartsWithNames = scrapPartsWithNames
                    .Where(p => p.PartNum != null && p.PartNum.ToLower().Contains(partNumber.ToLower()));

            if (!string.IsNullOrEmpty(leader))
                scrapPartsWithNames = scrapPartsWithNames
                    .Where(p => p.LeaderName != null && p.LeaderName.ToLower().Contains(leader.ToLower()));

            if (!string.IsNullOrEmpty(remarks))
                scrapPartsWithNames = scrapPartsWithNames
                    .Where(p => p.Remarks != null && p.Remarks.ToLower().Contains(remarks.ToLower()));

            if (totalFrom.HasValue)
                scrapPartsWithNames = scrapPartsWithNames.Where(p => p.Value >= totalFrom.Value);

            if (totalTo.HasValue)
                scrapPartsWithNames = scrapPartsWithNames.Where(p => p.Value <= totalTo.Value);

            var totalPartsAll = scrapPartsWithNames.Count();

            // Sorting
            var sortedParts = (sortOrder == "desc")
                ? scrapPartsWithNames.OrderByDescending(x => sortBy == "Qty" ? x.Qty : x.Value)
                : scrapPartsWithNames.OrderBy(x => sortBy == "Qty" ? x.Qty : x.Value);

            // Pagination
            var pagedParts = sortedParts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Json(new { Data = pagedParts, TotalPartsAll = totalPartsAll }, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> GetScrapSummaryPerLeader(string idScrap)
        {
            
            var scrapMasterList = await dbScrap.GetScrapMasterAsync();
            var scrapMaster = scrapMasterList.FirstOrDefault(s => s.IdScrap == idScrap);
            if (scrapMaster == null)
                return Json(new { error = "Scrap not found" }, JsonRequestBehavior.AllowGet);
            var employees = dbScrap.GetEmployeeMasterSSO();
            var initiatorName = !string.IsNullOrEmpty(scrapMaster.InitiatorKpk)
                ? employees.FirstOrDefault(e => e.Kpk == scrapMaster.InitiatorKpk)?.Name ?? "N/A"
                : "N/A";
            var scrapParts = await dbScrap.GetScrapPartsByScrapIdAsync(idScrap);
            var remarksList = dbScrap.GetScrapCodeRemarks();
            var scrapPartsWithNames = scrapParts.Select(p =>
            {
                int idRemarks;
                bool isValid = int.TryParse(p.Remarks, out idRemarks);
                var remarkName = isValid
                    ? remarksList.FirstOrDefault(r => r.IdRemarks == idRemarks)?.Remarks
                    : null;

                var leaderName = !string.IsNullOrEmpty(p.LeaderKPK)
                    ? employees.FirstOrDefault(e => e.Kpk == p.LeaderKPK)?.Name
                    : null;

                return new
                {
                    p.IdScrap,
                    p.PartNum,
                    p.Description,
                    p.Qty,
                    p.Value,
                    Remarks = remarkName ?? "N/A",
                    p.CurrentStatus,
                    p.LeaderKPK,
                    LeaderName = (leaderName ?? "N/A").Trim()
                };
            }).ToList();

            var totalQtyOverall = scrapPartsWithNames.Sum(p => p.Qty);
            var totalValueOverall = scrapPartsWithNames.Sum(p => p.Value);
            var groupedByLeader = scrapPartsWithNames
                .GroupBy(p => p.LeaderKPK)
                .Select(g => new
                {
                    LeaderKPK = g.Key,
                    LeaderName = g.FirstOrDefault()?.LeaderName ?? "N/A",
                    TotalQty = g.Sum(x => x.Qty),
                    TotalValue = g.Sum(x => x.Value)
                })
                .ToList();

            var rnNumber = scrapParts.FirstOrDefault(p => !string.IsNullOrEmpty(p.RnNumber))?.RnNumber;

            return Json(new
            {
                SpecialCodeTcCompanion = scrapMaster.SpecialCodeTcCompanion,
                AreaCode = scrapMaster.ScrapCode,
                Facility = scrapMaster.Facility,
                TC = scrapMaster.TC,
                CreatedDate=scrapMaster.CreatedDate,
                WC = scrapMaster.WC,
                RnNumber = rnNumber,
                InitiatorKpk = scrapMaster.InitiatorKpk,
                InitiatorName = initiatorName,
                DeletedAt = scrapMaster.DeletedAt,
                CurrentStatus = scrapMaster.CurrentStatus,
                Data = groupedByLeader,
                TotalQtyOverall = totalQtyOverall,
                TotalValueOverall = totalValueOverall
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public async Task<JsonResult> GetScrapSourceApprovals(string scrapId)
        {
            if (string.IsNullOrEmpty(scrapId))
                return Json(new { Data = new List<object>(), Count = 0 }, JsonRequestBehavior.AllowGet);

            var sourceDataList = dbCentralizedNotification.GetSourceDataSystemList();
            var scrapList = await dbScrap.GetScrapMasterAsync();
            var approvalList = await dbCentralizedNotification.GetApprovalListAsync();
            var employees = dbCentralizedNotification.GetEmployeeMasterSSO();
            var employeeDict = employees
                .GroupBy(e => e.Kpk)
                .ToDictionary(g => g.Key, g => g.First().Name);
            var delegateInfos = await dbScrap.GetDelegateApprovalInfosAsync();
            var userDelegates = await dbScrap.GetUserDelegatesAsync();
            var userDelegateDict = userDelegates
                .GroupBy(d => d.UserKpk)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new
                    {
                        x.DelegateKpk,
                        DelegateName = employeeDict.ContainsKey(x.DelegateKpk)
                            ? employeeDict[x.DelegateKpk]
                            : "Unknown",
                        x.DelegateTime
                    }).ToList()
                );
            var matched = sourceDataList
                .Where(src => src.Centralized_SourceData_Master_ID_Str == scrapId)
                .Select(src =>
                {
                    var scrapMaster = scrapList.FirstOrDefault(s => s.IdScrap == scrapId);

                    return new
                    {
                        src.Centralized_SourceData_ID,
                        src.Centralized_SourceData_Master_ID_Str,
                        src.Centralized_SourceData_Master_Status,

                        InitiatorKpk = scrapMaster?.InitiatorKpk,
                        InitiatorName = scrapMaster != null && !string.IsNullOrEmpty(scrapMaster.InitiatorKpk)
                            && employeeDict.ContainsKey(scrapMaster.InitiatorKpk)
                            ? employeeDict[scrapMaster.InitiatorKpk]
                            : "Unknown",
                        CurrentStatus = src.Centralized_SourceData_Master_Status,
                        DeletedAt = scrapMaster?.DeletedAt,

                        // approvals
                        Approvals = approvalList
                            .Where(a => a.Centralized_SourceData_ID == src.Centralized_SourceData_ID)
                            .OrderBy(a => a.Centralized_ApprovalList_Step)
                            .Select(a =>
                            {
                               
                                var delegateKpks = delegateInfos
                                    .Where(d => d.Centralized_ApprovalList_ID == a.Centralized_ApprovalList_ID)
                                    .Select(d => d.Delegate_ApprovalList_KpkApproval)
                                    .ToList();

                               
                                var userDelegatesForThisApprover = userDelegateDict.ContainsKey(a.Centralized_ApprovalList_KpkApproval)
                                      ? userDelegateDict[a.Centralized_ApprovalList_KpkApproval]
                                          .Select(d => (object)new { d.DelegateKpk, d.DelegateName, d.DelegateTime})
                                          .ToList()
                                      : new List<object>();


                                return new
                                {
                                    a.Centralized_ApprovalList_ID,
                                    a.Centralized_SourceData_ID,
                                    a.Centralized_ApprovalList_Step,
                                    a.Centralized_StatusList_ID,
                                    a.Centralized_ApprovalList_Date,
                                    OriginalKpkApproval = a.Centralized_ApprovalList_KpkApproval,
                                    ApproverName = employeeDict.ContainsKey(a.Centralized_ApprovalList_KpkApproval)
                                        ? employeeDict[a.Centralized_ApprovalList_KpkApproval]
                                        : "Unknown",

                                    DelegateKpks = delegateKpks,
                                    DelegateNames = delegateKpks
                                        .Select(k => employeeDict.ContainsKey(k) ? employeeDict[k] : "Unknown")
                                        .ToList(),
                                    UserDelegateList = userDelegatesForThisApprover
                                };
                            })
                            .ToList()
                    };
                })
                .ToList();

            return Json(new
            {
                Data = matched,
                Count = matched.Count
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public async Task<JsonResult> GetAllDisposalItems()
        {
            try
            {
                var items = await dbScrap.GetAllDisposalItemsAsync();
                return Json(new { success = true, data = items }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetScrapDisposalDetails(string scrapId)
        {
            try
            {
                var disposalItems = await dbScrap.GetAllDisposalItemsAsync();
                var scrapDisposals = await dbScrap.GetScrapDisposalsByScrapIdAsync(scrapId);
                var result = scrapDisposals.Select(sd =>
                {
                    var item = disposalItems.FirstOrDefault(di => di.Disposal_ID == sd.Disposal_ID);
                    return new
                    {
                        Code = item?.Disposal_Code,
                        Description = item?.Disposal_Desc,
                        Quantity = sd.Disposal_Quantity,
                        Unit = item?.Disposal_Unit
                    };
                }).ToList();

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



    }
}