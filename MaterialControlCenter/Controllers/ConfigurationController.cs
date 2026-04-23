using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MaterialControlCenter.Controllers;
using MaterialControlCenter.Service;
using MaterialControlCenter.services;


namespace MaterialControlCenter.Controllers
{

    [RoleAuthorize(1)]
    public class ConfigurationController : BaseController
    {
        // GET: Dashboard

        public ActionResult Remarks()
        {

            return View();
        }
        public ActionResult ScrapCode()
        {
            return View();
        }
        public ActionResult SpecialCase()
        {
            return View();
        }


    }
}