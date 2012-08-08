using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;

namespace SimpleActionHandler
{
    public class ModuleHandler : IHttpHandler 
    {
        private List<Route> routes;
        private HttpContext context;

        private static string Layout { get; set; }
        private static IViewEngine RenderEngine { get; set; }

        public static void setTemplatePath(string path)
        {
            RenderEngine.SetTemplatePath(HttpContext.Current.Server.MapPath("~/") + path);
        }

        static ModuleHandler()
        {
            Layout = "Application";
            RenderEngine = new DotLiquidViewEngine { Layout = Layout };
        }

        public ModuleHandler()
        {
            routes = new List<Route>();
        }

        public void Get(string path, Func<HttpRequest, ActionResult> action)
        {
            routes.Add(new Route("GET", path, action));
        }

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            //var requiredResourceSegments = context.Request.Url.Segments;
            this.context = context;
            var requestedPath = String.Join("", context.Request.Url.Segments.Skip(3).ToArray());
            var possibleActions = routes.ToLookup(r => r.Path, v => v)[requestedPath];
            var choosedAction = possibleActions.ToDictionary(a => a.Method, v => v)[context.Request.HttpMethod];
            var actionResult = choosedAction.InvokeAction(context.Request);
            buildResponseFrom(actionResult);
            //context.Response.ContentType = "application/json";
            //var jsonText = JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
            //response.ContentType = "text/html";
            //context.Response.Write(jsonText);
        }

        private void buildResponseFrom(ActionResult actionResult) 
        {
            var response = context.Response;
            response.ContentType = actionResult.ContentType;
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.Write(actionResult.ResponseText);
        }

        public void setRenderEngine(IViewEngine engine)
        {
            RenderEngine = engine;
            engine.Layout = Layout;
        }
        
        public void setLayout(string layout)
        {
            Layout = layout;
            RenderEngine.Layout = Layout;
        }

        public string Render<T>(string template, T parameters)
        {
            return RenderEngine.Render<T>(template, parameters);
        }

        public ActionResult Ok(string responseText)
        {
            return new ActionResult { ResponseText = responseText, ContentType = "text/html" };
        }

    }
}
