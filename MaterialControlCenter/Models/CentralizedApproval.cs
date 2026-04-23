using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MaterialControlCenter.Models
{
    public class CentralizedSourceDataModel
    {
        public int Centralized_SourceData_ID { get; set; }
        public int Centralized_SystemList_ID { get; set; }
        public string Centralized_SourceData_TableName { get; set; }
        public int Centralized_SourceData_Master_ID { get; set; }
        public string Centralized_SourceData_Master_Title { get; set; }
        public string Centralized_SourceData_Master_Desc { get; set; }
        public int Centralized_SourceData_Master_Status { get; set; }
        public DateTime Centralized_SourceData_Master_CreatedDate { get; set; }
        public string Centralized_SourceData_Master_ID_Str
        {
            get
            {
                return Centralized_SourceData_Master_ID.ToString("D6");
            }
        }
    }
    public class CentralizedInitiator
    {
        public int Centralized_Initiator_ID { get; set; }
        public int Centralized_SourceData_ID { get; set; }
        public string Centralized_Initiator_KPK { get; set; }
    }

    public class CentralizedApprovalListModel
    {
        public int Centralized_ApprovalList_ID { get; set; }
        public int Centralized_SourceData_ID { get; set; }
        public int Centralized_ApprovalList_Step { get; set; }
        public int Centralized_StatusList_ID { get; set; }
        public DateTime? Centralized_ApprovalList_Date { get; set; }
        public string Centralized_ApprovalList_Link { get; set; }
        public string Centralized_ApprovalList_KpkApproval { get; set; }
        public string Centralized_ApprovalList_Base64 { get; set; }
    }
    public class CreateWorkOrderResult
    {
        public int Wo_Master_ID { get; set; }
        public int Centralized_SourceData_ID { get; set; }
        public string Supervisor1 { get; set; }
        public string Supervisor2 { get; set; }
    }
   


}