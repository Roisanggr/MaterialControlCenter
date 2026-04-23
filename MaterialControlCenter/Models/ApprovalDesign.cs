using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MaterialControlCenter.Models
{
   

    public class ScrapCodeSpecialCaseApprovalRequirement
    {
        public int Id { get; set; }
        public string ScrapCode { get; set; }
        public int Role_Id { get; set; }
        public int RequiredApproverCount { get; set; }
        public string ScrapTcType { get; set; }
        public int? minValue { get; set; }
        public int? maxValue { get; set; }
        public string commit { get; set; }
        public int? PriorityScrapCase { get; set; }
        public string Tc { get; set; }

    }
    public class ApprovalUpdateModel
    {
        public int ApprovalListId { get; set; }
        public string NewKpk { get; set; }
    }

    public class ApprovalModel
    {
        public int Centralized_ApprovalList_ID { get; set; }
        public string Centralized_ApprovalList_KpkApproval { get; set; }
        public int Centralized_ApprovalList_Step { get; set; }
        public int Centralized_StatusList_ID { get; set; }
        public DateTime Centralized_ApprovalList_Date { get; set; }
    }

    public class UserDelegate
    {
        public int Id { get; set; }
        public string UserKpk { get; set; }
        public string DelegateKpk { get; set; }
        public string DelegateTime { get; set; }
    }
    public class DelegateApprovalRecord
    {
        public int Id { get; set; }
        public int Centralized_ApprovalList_ID { get; set; }
        public int Centralized_StatusList_ID { get; set; }
        public string Delegate_ApprovalList_KpkApproval { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class DelegateApprovalInfo
    {
        public int Centralized_ApprovalList_ID { get; set; }
        public string Delegate_ApprovalList_KpkApproval { get; set; }
    }
    public class ApprovalDtoV2Fastest
    {
        public int Centralized_ApprovalList_ID { get; set; }
        public int Centralized_SourceData_ID { get; set; }
        public int Centralized_ApprovalList_Step { get; set; }
        public int Centralized_StatusList_ID { get; set; }
        public DateTime? Centralized_ApprovalList_Date { get; set; }
      
        public string Kpk { get; set; }
        public string ApproverName { get; set; }
    }



}