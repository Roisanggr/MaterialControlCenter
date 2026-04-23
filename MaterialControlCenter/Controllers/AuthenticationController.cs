using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MaterialControlCenter.Controllers
{
    public class AuthenticationController : Controller
    {
        // GET: Authentication
        public ActionResult Signin()
        {
            return View();
        }
        public ActionResult Signup()
        {
            return View();
        }
        public ActionResult ResetPw()
        {
            return View();
        }
    }
}