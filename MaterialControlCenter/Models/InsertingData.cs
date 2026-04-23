using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MaterialControlCenter.Models
{
   
    public class ScrapMasterModel
    {
        public string Facility { get; set; }
        public string Type { get; set; }
        public string TC { get; set; }
        public string InitiatorKpk { get; set; }
        public int isSubmitted { get; set; }
        public string ScrapCode { get; set; }
        public string TCType { get; set; }
        public string WC { get; set; }
        public int? Type_ID { get; set; }
        public string SpecialCodeRemarks { get; set; }

        public string SpecialCodeTcCompanion { get; set; }

    }

    public class ScrapPartModel
    {
        public string IdScrap { get; set; }
        public string PartNum { get; set; }
        public string Description { get; set; }
        public decimal Qty { get; set; }
        public decimal Value { get; set; }
        public string Remarks { get; set; }
        public int IdPart { get; set; }
        public string ProcessPoint { get; set; }
        public string Ftypit { get; set; }
        public string Typeit { get; set; }
        public int CurrentStatus { get; set; }
        public string RnNumber { get; set; }
        public string RespCode { get; set; }
        public string Measit { get; set; }
        public string Planit { get; set; }
        public string Cmidit { get; set; }
        public decimal? BasePrice { get; set; }
        public string Commit { get; set; }
        public string LeaderKPK { get; set; }
        public DateTime? KeyInDateTime { get; set; }
        public string SpecialcodeRemarksParts { get; set; }
    }

    public class ScrapRequest
    {
        public ScrapMasterModel Master { get; set; }
        public List<ScrapPartModel> Parts { get; set; }
        public List<DetectedKpkItem> detectedKPKGlobal { get; set; } = new List<DetectedKpkItem>();
        public string PdfBase64 { get; set; }
    }

    public class DetectedKpkItem
    {
        public string kpk { get; set; }
        public int? roleId { get; set; }
    }

    public class UserUpsertRequest
    {
        public string Kpk { get; set; }
        public string Name { get; set; }
        public int RoleId { get; set; }
        public string Facility { get; set; }
        public string TC { get; set; }
        public string ScrapCodeResponsible { get; set; }
    }
    public class UserAddHistoryModel
    {
        public string AddedByKpk { get; set; }
        public string AddedUserKpk { get; set; }
    }

}