using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;

namespace SimpleActionHandler
{
    public class ModuleHandler : IHttpHandler 
    {
        private List<Route> routes;
        private HttpContext context;

        public string Layout { get; set; }
        public Dictionary<string, object> Params { get; private set; }
        public Dictionary<string, object> NamedRoutes { get; private set; }

        public List<Route> Routes
        {
            get { return routes; }
        }

        public ModuleHandler()
        {
            routes = new List<Route>();
            Layout = "Application.html";
            Params = new Dictionary<string, object>();
        }

        private void BuildNamedRoutes()
        {
            var request = HttpContext.Current.Request;
            var appPath = request.ApplicationPath;
            var handlerName = request.AppRelativeCurrentExecutionFilePath
                                     .Split('/').Skip(1).First();

            var basePath = VirtualPathUtility.AppendTrailingSlash(
                    VirtualPathUtility.ToAbsolute("~/" + handlerName, appPath));

            NamedRoutes = Routes.ToDictionary(k => k.Name, v => (basePath + v.Path) as object);
        }

        public virtual void Initialize()
        {
            BuildNamedRoutes();
        }

        public void Get(string name, string path, Func<HttpRequest, ActionResult> action)
        {
            routes.Add(new Route("GET", name, path, action));
        }

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            var request = context.Request;

            var path = ExtractPath(request);

            var route = GetRoute(request.HttpMethod, path);

            //context.Response.Write(route.Path);

            ExtractQueryString(request);
            ExtractNamedRoutesParams(route.Pattern, path);
            
            var actionResult = route.InvokeAction(context.Request);
            buildResponseFrom(actionResult);

            //context.Response.ContentType = "application/json";
            //var jsonText = JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
            //response.ContentType = "text/html";
            //context.Response.Write(jsonText);
        }

        private string ExtractPath(HttpRequest request)
        {
            var currentUri = request.Url;
            var abs = request.ApplicationPath;
            var rel = request.AppRelativeCurrentExecutionFilePath;

            var uriBuilder = new UriBuilder(currentUri.Scheme, currentUri.Host);
            var uri = new Uri(uriBuilder.Uri, VirtualPathUtility.ToAbsolute(rel, "/"));

            var path = String.Join("", uri.Segments.Skip(2).ToArray());

            return path;
        }

        private Route GetRoute(string method, string path)
        {
            return routes.First(t => t.Method == context.Request.HttpMethod && t.Pattern.Match(path).Success);
        }

        private void ExtractQueryString(HttpRequest request)
        {
            var coll = request.QueryString;
            var keys = coll.AllKeys;
            foreach (var key in keys)
                Params[key] = coll[key];
        }

        private void ExtractNamedRoutesParams(Regex rgx, string path)
        {
            var groups = rgx.GetGroupNames().Skip(1);
            var match = rgx.Match(path);
            foreach (string name in groups)
                Params[name] = match.Groups[name].Value;
        }

        private void buildResponseFrom(ActionResult actionResult) 
        {
            var response = context.Response;
            response.ContentType = actionResult.ContentType;
            response.StatusCode = actionResult.StatusCode;
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.Write(actionResult.ResponseText);
        }

        public ActionResult Ok(string responseText)
        {
            return new ActionResult { 
                ResponseText = responseText, 
                ContentType = "text/html",
                StatusCode = 200
            };
        }

        public ActionResult UnAuthorized(string responseText)
        {
            return new ActionResult
            {
                ResponseText = responseText,
                ContentType = "text/html",
                StatusCode = 401
            };
        }
    }
}
