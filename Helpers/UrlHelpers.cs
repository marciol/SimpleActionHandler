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

        public static string AddQueryString(this IUrlHelpersMixin client, string key, string value)
        {
            return client.UrlHelpers.AddQueryString(key, value);
        }

        public static string RemoveQueryString(this IUrlHelpersMixin cliente, string key)
        {
            return cliente.UrlHelpers.RemoveQueryString(key);
        }
    }

    public class UrlHelpers
    {
        private RequestContext requestContext;
        private RouteCollection routeCollection;

        public UrlHelpers()
        {
            var contextWrapper = new HttpContextWrapper(HttpContext.Current);
            var routeData = RouteTable.Routes.GetRouteData(contextWrapper);

            if (routeData != null)
                requestContext =
                    new RequestContext(contextWrapper, routeData);

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

        public string AddQueryString(string key, string value)
        {
            var request = requestContext.HttpContext.Request;
            var col = request.QueryString.ToPairs();

            var updatedCol = col.Where(kv => kv.Key != key).ToList();
            updatedCol.Add(new KeyValuePair<string,string>(key, value));
            var str =
                "?" +
                String.Join(
                    "&", updatedCol.Select(kv => String.Format("{0}={1}", kv.Key, kv.Value))
                                   .ToArray());

            return str;
        }

        public string RemoveQueryString(string key)
        {
            var request = requestContext.HttpContext.Request;
            var col = request.QueryString.ToPairs();

            var updatedCol = col.Where(kv => kv.Key != key).ToList();
            var str =
                "?" +
                String.Join(
                    "&", updatedCol.Select(kv => String.Format("{0}={1}", kv.Key, kv.Value))
                                   .ToArray());

            return str;
        }

    }
}
