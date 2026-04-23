using MaterialControlCenter.Models;
using MaterialControlCenter.Service;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MaterialControlCenter
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //// Build cache pertama kali
            //WarmUpCache();

            //// Background refresher
           
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
            var context = HttpContext.Current;

            Server.ClearError();
            Response.Clear();

            new ErrorHandlerService().Handle(context, exception);
        }
        

    }
}
