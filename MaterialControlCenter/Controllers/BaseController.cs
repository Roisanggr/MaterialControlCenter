using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MaterialControlCenter.Models;
using MaterialControlCenter.Service;
using MaterialControlCenter.services;
using System.Runtime.Caching;

namespace MaterialControlCenter.Controllers
{
    [ValidateJwt]
    public class BaseController : Controller
    {
        protected string Plant => Session["Plant"] as string ?? "P1";
        protected string TC => Session["TC"] as string ?? "23";
        protected string Name => Session["UserName"] as string ?? "No Name";
        protected string Kpk => Session["Kpk"] as string ?? "(unknown)";
        protected string RoleId => Session["RoleId"] as string ?? "(unknown)";
        protected string supervisorKpk  => Session["SupervisorKpk"] as string ?? "(unknown)";

        protected readonly DatabaseConnection dbSSO = new DatabaseConnection("SSO");
        protected readonly DatabaseConnection dbToolRoom = new DatabaseConnection("ToolRoomDBTV");
        protected readonly DatabaseConnection dbScrap = new DatabaseConnection("ScrapDatabaseString");
        protected readonly DatabaseConnection dbCentralizedNotification = new DatabaseConnection("CentralizedNotification");

      

        public void LoadUserInfoFromJwt()
        {
            var jwt = HttpContext.Items["JwtToken"] as string;
            if (string.IsNullOrEmpty(jwt))
            {
              
                ClearSessionAndViewBag();
                return;
            }

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            var kpkClaim = token.Claims.FirstOrDefault(c => c.Type.Equals("kpk", StringComparison.OrdinalIgnoreCase));
            string kpk = kpkClaim?.Value;

            if (string.IsNullOrEmpty(kpk))
            {
             
                ClearSessionAndViewBag();
                return;
            }

            var employee = dbSSO.GetUserByKpkSSO(kpk);

            if (employee != null)
            {
                ViewBag.UserName = employee.Name;
                Session["UserName"] = employee.Name;
               
            }
            else
            {
                ViewBag.UserName = null;
                
            }

            var userToolRoom = dbScrap.GetUserByKpkScrap(kpk);
            Session["Kpk"] = kpk;
            ViewBag.Kpk = kpk;
            if (userToolRoom != null)
            {
                ViewBag.RoleId = userToolRoom.RoleId;
                ViewBag.Plant = userToolRoom.Plant;
                Session["RoleId"] = userToolRoom.RoleId;
                Session["Plant"] = userToolRoom.Plant;
                Session["TC"] = string.Join(",", userToolRoom.TC);

            }
            else
            {
                ViewBag.RoleId = null;
                ViewBag.Plant = null;
               
            }

            var supervisor = FindSupervisorWithRole4(kpk);

            if (supervisor != null)
            {
                ViewBag.SupervisorKpk = supervisor.Kpk;
                Session["SupervisorKpk"] = supervisor.Kpk;
               
            }
            else
            {
                ViewBag.SupervisorKpk = null;
                Session["SupervisorKpk"] = null;
               
            }

            ViewBag.Kpk = kpk;
        }

        private void ClearSessionAndViewBag()
        {
            ViewBag.Kpk = null;
            ViewBag.UserName = null;
            ViewBag.RoleId = null;
            ViewBag.Plant = null;
            ViewBag.SupervisorKpk = null;

            Session["Kpk"] = null;
            Session["UserName"] = null;
            Session["RoleId"] = null;
            Session["Plant"] = null;
            Session["SupervisorKpk"] = null;
        }

        public UserToolRoomModel FindSupervisorWithRole4(string kpk)
        {
            string currentKpk = kpk;

            while (!string.IsNullOrEmpty(currentKpk))
            {
                var employee = dbSSO.GetUserByKpkSSO(currentKpk);
                if (employee == null)
                    break;

                var userScrap = dbScrap.GetUserByKpkScrap(currentKpk);
                if (userScrap != null && userScrap.RoleId == 4)
                {
                    return userScrap;
                }

                currentKpk = employee.Supervisor;
            }

            return null;
        }

    }
}