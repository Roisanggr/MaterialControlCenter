using MaterialControlCenter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaterialControlCenter.Service
{
    public static class ScrapReportingCacheBuilder
    {
       
        public static async Task<ScrapReportingCache> BuildAsync(
    string approverKpk,
    string allowedStatuses,
    string period
)
        {
            var dbScrap = new DatabaseConnection("ScrapDatabaseString");
            var dbCentralized = new DatabaseConnection("CentralizedNotification");
            var scrapTask = dbScrap.GetScrapMasterAsync();
            var scrapPartsTask = dbScrap.GetScrapPartsAllAsync();
            var approvalTask = dbCentralized.GetApprovalListAsync();
            var sourceTask = Task.Run(() => dbCentralized.GetSourceDataSystemList());

            await Task.WhenAll(scrapTask, scrapPartsTask, approvalTask, sourceTask);

            var scrapList = scrapTask.Result;
            var scrapParts = scrapPartsTask.Result;
            var approvalList = approvalTask.Result;
            var sourceDataList = sourceTask.Result;
            var approvalLookup = approvalList
                .GroupBy(a => a.Centralized_SourceData_ID)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.Centralized_ApprovalList_Step).ToList()
                );

            var docTotals = scrapParts
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
            if (!string.IsNullOrEmpty(allowedStatuses))
                allowedStatusList = allowedStatuses.Split(',').Select(int.Parse).ToList();

            var startDate =
                period == "monthly" ? DateTime.Today.AddMonths(-1) :
                period == "yearly" ? DateTime.Today.AddYears(-1) :
                                      DateTime.Today.AddDays(-7);

            DateTime ResolveDate(DateTime? sourceDate, DateTime? scrapDate)
            {
                if (sourceDate.HasValue && sourceDate.Value != DateTime.MinValue)
                    return sourceDate.Value;

                if (scrapDate.HasValue && scrapDate.Value != DateTime.MinValue)
                    return scrapDate.Value;

                return DateTime.Today;
            }
            var query =
                from scr in scrapList
                join src in sourceDataList
                    on scr.IdScrap equals src.Centralized_SourceData_Master_ID_Str
                let createdDate = ResolveDate(
                    src.Centralized_SourceData_Master_CreatedDate,
                    scr.CreatedDate
                )
                where createdDate >= startDate
                where allowedStatusList == null || allowedStatusList.Contains(src.Centralized_SourceData_Master_Status)
                select new
                {
                    scr.IdScrap,
                    scr.ScrapCode,
                    SourceDataStatus = src.Centralized_SourceData_Master_Status,
                    SourceDataCreatedDate = createdDate,
                    Approvals = approvalLookup.ContainsKey(src.Centralized_SourceData_ID)
                        ? approvalLookup[src.Centralized_SourceData_ID]
                        : null
                };
            if (!string.IsNullOrEmpty(approverKpk))
            {
                query = query.Where(j =>
                {
                    var approvals = j.Approvals;
                    if (approvals == null)
                        return false;

                    for (int i = 0; i < approvals.Count; i++)
                    {
                        var current = approvals[i];
                        if (current.Centralized_ApprovalList_KpkApproval == approverKpk &&
                            (current.Centralized_StatusList_ID == 1 ||
                             current.Centralized_StatusList_ID == 2))
                        {
                            for (int k = 0; k < i; k++)
                                if (approvals[k].Centralized_StatusList_ID == 1)
                                    return false;

                            return true;
                        }
                    }
                    return false;
                });
            }
            var joinedList = query.ToList();
            var dailyResults = joinedList
                .GroupBy(x => x.SourceDataCreatedDate.Date)
                .Select(g => new ScrapDailyResult
                {
                    Date = g.Key,
                    ScrapItems = g.Select(x =>
                    {
                        var totals = docTotals.ContainsKey(x.IdScrap)
                            ? docTotals[x.IdScrap]
                            : null;

                        return new ScrapItemResult
                        {
                            IdScrap = x.IdScrap,
                            ScrapCode = x.ScrapCode,
                            SourceDataStatus = x.SourceDataStatus,
                            CreatedDate = x.SourceDataCreatedDate,
                            TotalQty = totals != null ? totals.Qty : 0,
                            TotalValue = totals != null ? totals.Value : 0
                        };
                    }).ToList(),

                    TotalQty = g.Sum(x => docTotals.ContainsKey(x.IdScrap) ? docTotals[x.IdScrap].Qty : 0),
                    TotalValue = g.Sum(x => docTotals.ContainsKey(x.IdScrap) ? docTotals[x.IdScrap].Value : 0)
                })
                .OrderByDescending(x => x.Date)
                .ToList();

            return new ScrapReportingCache
            {
                DailyResults = dailyResults
            };
        }

    }
}
