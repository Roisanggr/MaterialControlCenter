using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MaterialControlCenter.Models
{
    public class getPartMasterModelUpdated
    {
        public string ToyNum { get; set; }
        public string PartNum { get; set; }
        public string Description { get; set; }
        public decimal? BasePrice { get; set; }
        public string Measurement { get; set; }
        public string ProcessPoint { get; set; }

        public string ftypit { get; set; }

        public string typeit { get; set; }
        public string commit { get; set; }
        public string planit { get; set; }
        public string cmidit { get; set; }
       
    }

    public class TcAndTypeMaster
    {
        public int TC { get; set; }
        public string Type { get; set; }
        public string TcDescription { get; set; }
        public string ShowScrapCode { get; set; }
        public string Facility { get; set; }
    }

    public class ScrapCode
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Location { get; set; }
        public string Application { get; set; }
        public string Facility { get; set; }
        public string Area { get; set; }
        public string Description { get; set; }
        public List<string> Remarks { get; set; }
        public int IdRemarks { get; set; }
    }

    public class ScrapCodeRemarkModel
    {
        public int IdRemarks { get; set; }
        public string Remarks { get; set; }
        public string ScrapCode { get; set; }
    }

    public class WorkCenterModel
    {
        public int Id_WorkCenter { get; set; }
        public string Facility { get; set; }
        public int WC { get; set; }
        public string Description { get; set; }
    }

    public class FetchingScrapMasterModel
    {
        public string IdScrap { get; set; }
        public string Facility { get; set; }
        public string TC { get; set; }
        public string InitiatorKpk { get; set; }
        public bool isSubmitted { get; set; }
        public string ScrapCode { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool isDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? CurrentStatus { get; set; }
        public string WC { get; set; }
        public int Type_ID { get; set; }
        public string SpecialCodeRemarks { get; set; }
        public string SpecialCodeTcCompanion { get; set; }
    }

    public class ScrapWithSourceDataModel
    {
        public int IdScrap { get; set; }
        public string Facility { get; set; }
        public string TC { get; set; }
        public string InitiatorKpk { get; set; }
        public bool isSubmitted { get; set; }
        public string ScrapCode { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool isDeleted { get; set; }
        public int? CurrentStatus { get; set; }
        public string WC { get; set; }
        public string SourceDataMasterID { get; set; }
        public int SourceDataStatus { get; set; }
        public DateTime SourceDataCreatedDate { get; set; }
    }
    public class ScrapPartSummaryModel
    {
        public string IdScrap { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalValue { get; set; }
    }
    public class ScrapDisposalModel
    {
        public int Scrap_Disposal_ID { get; set; }
        public string Scrap_ID { get; set; }           
        public int Disposal_ID { get; set; }
        public decimal Disposal_Quantity { get; set; }
        public string Disposal_Remarks { get; set; }
        public bool Disposal_In { get; set; }
        public bool Disposal_Out { get; set; }
        public bool Disposal_B3 { get; set; }
        public bool Disposal_NonB3 { get; set; }
    }

    public class DisposalItem
    {
        public int Disposal_ID { get; set; }
        public string Disposal_Code { get; set; }
        public string Disposal_Desc { get; set; }
        public string Disposal_Unit { get; set; }
    }
    public class TypeScrapModel
    {
        public int Type_ID { get; set; }
        public string Type_Desc { get; set; }
        public bool IsDelete { get; set; }
    }
    public class TCCompanion
    {
        public int Id { get; set; }
        public string TC { get; set; }
        public string Name { get; set; }
        public string TypeTC { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}