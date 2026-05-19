using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MaterialControlCenter.Controllers
{
    public class ScrapRecordsController : BaseController
    {
        // GET: ScrapRecords
        public ActionResult AllScrap()
        {
            return View();
        }

        public ActionResult MyScrap()
        {
            return View();
        }

        public ActionResult DetailScrap(string token)
        {
            string docId = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            ViewBag.DocumentID = docId;
            return View();
        }




    }
}