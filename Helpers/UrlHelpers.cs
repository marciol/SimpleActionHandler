using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;

namespace SimpleActionHandler.Helpers
{

    public interface IUrlHelpersMixin
    {
        UrlHelpers UrlHelpers { get; }
    }

    public static class UrlHelpersMixin
    {
        public static string UrlFor(this IUrlHelpersMixin client, string routeName, object args)
        {
            return client.UrlHelpers.UrlFor(routeName, args);
        }

        public static string UrlFor(this IUrlHelpersMixin client, string routeName)
        {
            return client.UrlHelpers.UrlFor(routeName);
        }

        public static string RootPath(this IUrlHelpersMixin client)
        {
            return client.UrlHelpers.RootPath();
        }

        public static string Path(this IUrlHelpersMixin client, string path)
        {
            return client.UrlHelpers.Path(path);
        }

        public static string QueryString(this IUrlHelpersMixin client)
        {
            return client.UrlHelpers.QueryString();
        }
    }

    public class UrlHelpers
    {
        private RequestContext requestContext;
        private RouteCollection routeCollection;

        public UrlHelpers()
        {
            var contextWrapper = new HttpContextWrapper(HttpContext.Current);
            requestContext =
                new RequestContext(contextWrapper, 
                    RouteTable.Routes.GetRouteData(contextWrapper));

            routeCollection = RouteTable.Routes;
        }

        public string UrlFor(string routeName, object args)
        {
            var path = routeCollection.GetVirtualPath(
                requestContext, routeName, new RouteValueDictionary(args));

            return path.VirtualPath;
        }

        public string UrlFor(string routeName)
        {
            var path = routeCollection.GetVirtualPath(
                requestContext, routeName, new RouteValueDictionary());

            return path.VirtualPath;
        }

        public string RootPath()
        {
            return HttpContext.Current.Request.ApplicationPath; 
        }

        public string Path(string path)
        {
            return VirtualPathUtility.ToAbsolute("~/" + path, HttpContext.Current.Request.ApplicationPath);
        }

        public string QueryString()
        {
            return requestContext.HttpContext.Request.Url.Query;
        }

    }
}
