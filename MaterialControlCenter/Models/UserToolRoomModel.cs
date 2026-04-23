using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MaterialControlCenter.Models
{
    public class UserToolRoomModel
    {
        public int Id { get; set; } // BUKAN long

        public string Kpk { get; set; }
        public int? RoleId { get; set; }
        public DateTime? DateCreated { get; set; }
        public bool? IsActive { get; set; }
        public string Division { get; set; }
        public string Plant { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Supervisor { get; set; }
        public string Hierarchy { get; set; }
        public string NetworkId { get; set; }
      
        public string[] Facility { get; set; }
        public string[] TC { get; set; } // <-- Tambahkan properti baru
        public string[] ScrapCodeResponsible { get; set; }
    }

    public class ShiftModel
    {
        public int Id { get; set; }
        public string ShiftId { get; set; }
        public string RangeHour { get; set; }
    }
    public class RoleModel
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
    }

    public class UpdateUserRequest
    {
        public string Kpk { get; set; }           // Wajib
        public string RoleId { get; set; }        // Optional
        public List<string> Facility { get; set; } // Optional (bisa null)
        public bool? IsActive { get; set; }       // Optional (nullable)
        public List<string> TC { get; set; }      // Optional
        public List<string> ScrapCodeResponsible { get; set; }
    }

    public class UserModelInsert
    {
        public string Kpk { get; set; }
        public string Name { get; set; }
        public int RoleId { get; set; }
        public string Facility { get; set; }
        public string TC { get; set; }
        public string ScrapCodeResponsible { get; set; }
    }


}