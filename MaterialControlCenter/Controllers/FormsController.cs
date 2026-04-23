using MaterialControlCenter.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sliced.Controllers
{
    public class FormsController : BaseController
    {
        public ActionResult CreateScrap()
        {
            return View();
        }

        public ActionResult CreatePia()
        {
            return View();
        }

        public ActionResult CreateTpr()
        {
            return View();
        }
    }
}