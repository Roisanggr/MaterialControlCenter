using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MaterialControlCenter.Service
{
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly int[] _allowedRoles;

        public RoleAuthorizeAttribute(params int[] roles)
        {
            _allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var roleIdObj = httpContext.Session["RoleId"];
            if (roleIdObj == null || !int.TryParse(roleIdObj.ToString(), out int roleId))
                return false;

            return _allowedRoles.Contains(roleId);
        }


        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Redirect ke halaman custom error 403
            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary(
                    new
                    {
                        controller = "Pages",
                        action = "Error403"
                    }
                )
            );
        }

    }
}