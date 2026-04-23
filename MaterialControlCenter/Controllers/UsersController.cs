using MaterialControlCenter.Controllers;
using MaterialControlCenter.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MaterialControlCenter.Controllers
{
    public class UsersController : BaseController
    {
        // GET: Users
        public ActionResult Profile()
        {
            return View();
        }
        [RoleAuthorize(1)]
        public ActionResult Management()
        {
            return View();
        }
    }
}