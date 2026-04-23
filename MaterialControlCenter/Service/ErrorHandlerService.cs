using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MaterialControlCenter.Service
{
    public class ErrorHandlerService
    {
        public void Handle(HttpContext context, Exception exception)
        {
            var httpException = exception as HttpException;
            var routeData = new RouteData();
            routeData.Values["controller"] = "Pages";

            if (httpException == null)
            {
                routeData.Values["action"] = "Error500";
            }
            else
            {
                int code = httpException.GetHttpCode();
                switch (code)
                {
                    case 403:
                        routeData.Values["action"] = "Error403";
                        break;
                    case 404:
                        routeData.Values["action"] = "Error404";
                        break;
                    case 503:
                        routeData.Values["action"] = "Error503";
                        break;
                    case 500:
                    default:
                        routeData.Values["action"] = "Error500";
                        break;
                }
            }

            // Clear the error to prevent default IIS behavior
            context.ClearError();

            // Eksekusi controller PagesController
            IController controller = new MaterialControlCenter.Controllers.PagesController();
            controller.Execute(new RequestContext(new HttpContextWrapper(context), routeData));
        }
    }
}