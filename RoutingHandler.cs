using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;

namespace SimpleActionHandler
{
    public class RoutingHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            requestContext.HttpContext.Items["RouteData"] =
                requestContext.RouteData;

            return new DefaultHandler() as IHttpHandler;
        }
    }
}
