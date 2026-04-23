using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MaterialControlCenter.Models
{
    public class PartGroupResult
    {
        public string PartNum { get; set; }
        public string Description { get; set; }
        public decimal TotalValue { get; set; }
        public double ContributionPercent { get; set; }
    }

    public class LocationScrapSummaryCount
    {
        public string Location { get; set; }
        public int TotalScrap { get; set; }
    }


    public class ScrapItemResult
    {
        public string IdScrap { get; set; }
        public string ScrapCode { get; set; }
        public int SourceDataStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalValue { get; set; }
    }
    public class ScrapDailyResult
    {
        public DateTime Date { get; set; }
        public List<ScrapItemResult> ScrapItems { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalValue { get; set; }
    }
    public class ScrapReportingCache
    {
        public List<ScrapDailyResult> DailyResults { get; set; }
    }

    public class ScrapRawDto
    {
        public int SourceDataId { get; set; }
        public int Status { get; set; }
        public string IdScrap { get; set; }
        public string ScrapCode { get; set; }
        public string InitiatorKpk { get; set; }
        public string InitiatorName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<LeaderDto> Leaders { get; set; }
        public List<string> Approvals { get; set; }
    }

    public class LeaderDto
    {
        public string LeaderKpk { get; set; }
        public decimal Qty { get; set; }
        public decimal Value { get; set; }
    }
    public class ScrapPartRawDto
    {
        public int Status { get; set; }
        public string InitiatorKpk { get; set; }
        public string PartNum { get; set; }
        public string Description { get; set; }
        public decimal Qty { get; set; }
        public decimal Value { get; set; }
        public HashSet<string> ApproverSet { get; set; }
    }
    public class ScrapPartRemarkRawDto
    {
        public int SourceDataId { get; set; }
        public int Status { get; set; }
        public string InitiatorKpk { get; set; }
        public int RemarksId { get; set; }
        public decimal Qty { get; set; }
        public decimal Value { get; set; }
        public HashSet<string> ApproverSet { get; set; }
    }

    public class ScrapDocumentPartRawDto
    {
        public int SourceDataId { get; set; }
        public string ScrapId { get; set; }

        public string Facility { get; set; }
        public string TC { get; set; }

        public string InitiatorKpk { get; set; }
        public string InitiatorName { get; set; }

        public string ScrapCode { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string PartNum { get; set; }
        public string Description { get; set; }
        public decimal Qty { get; set; }
        public decimal Value { get; set; }
        public int Status { get; set; }

        public List<ApprovalDto> Approvals { get; set; }
    }

    public class ApprovalDto
    {
        public string ApproverKpk { get; set; }
        public string ApproverName { get; set; }
    }


    public class ScrapReportRow
    {
        public DateTime CreatedDate { get; set; }
        public string DocumentID { get; set; }
        public string TypeName { get; set; }
        public string Facility { get; set; }
        public string InitiatorName { get; set; }
        public string SupervisorName { get; set; }
        public string CurrentApprovalName { get; set; }
        public string TC { get; set; }
        public string TcCompanion { get; set; }
        public string ScrapCode { get; set; }
        public string WC { get; set; }
        public string RnNumber { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public string Measit { get; set; }
        public string Planit { get; set; }
        public string Cmidit { get; set; }
        public string Commit { get; set; }
        public decimal Quantity { get; set; }
        public decimal BasePrice { get; set; }
        public decimal Total { get; set; }
        public string RemarksName { get; set; }
        public string LeaderKPK { get; set; }
        public DateTime? KeyInDateTime { get; set; }
        public string Status { get; set; }
        public string SpecialCodeRemarks { get; set; }
        public string SpecialcodeRemarksParts { get; set; }
    }


}