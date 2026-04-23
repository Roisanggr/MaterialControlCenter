using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MaterialControlCenter.Controllers;
using MaterialControlCenter.services;


namespace MaterialControlCenter.Controllers
{
    public class DashboardController : BaseController
    {
        // GET: Dashboard
        public ActionResult Index()
        {
           
            return View();
        }
        public ActionResult Project()
        {
            return View();
        }
        public ActionResult HeadcountReport()
        {
            return View();
        }

       
    }
}