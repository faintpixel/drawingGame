using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SignalRTest
{
    public class SignalRLoggingAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext) // TO DO - remove this.. was just fiddling with things.
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<DrawingHub>();
            hub.Clients.All.actionExecuting(filterContext.RequestContext.HttpContext.Request.Url.OriginalString);
            base.OnActionExecuting(filterContext);
        }
    }
}