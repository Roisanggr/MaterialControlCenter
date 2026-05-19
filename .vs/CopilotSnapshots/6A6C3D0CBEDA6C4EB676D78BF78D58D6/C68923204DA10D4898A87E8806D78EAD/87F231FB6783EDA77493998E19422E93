using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MaterialControlCenter.Models
{
    public class MccApprovalRule
    {
        public int Id { get; set; }
        public string Application { get; set; }  // "SCRAP" | "PIA"
        public string Code { get; set; }  // MAL/MQC/QV/NULL — filter by scrap/pia code
        public string Tc { get; set; }  // filter by TC number
        public string TcType { get; set; }  // F/R/A/X — filter by typeit
        public string Cmmit { get; set; }  // X = commit required
        public long? MinValue { get; set; }  // batas bawah nilai (inclusive)
        public long? MaxValue { get; set; }  // batas atas nilai (exclusive)
        public int RoleId { get; set; }  // FK ke [Scrap].[dbo].[role]
        public int RequiredApproverCount { get; set; }
        public int? Priority { get; set; }  // urutan step approval
        public bool IsActive { get; set; }
    }
}
