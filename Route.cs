using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

namespace SimpleActionHandler
{
    public class Route
    {

        private Func<HttpRequest, ActionResult> action;

        public string Path { get; private set; }
        public Regex Pattern { get; private set; }
        public string Method { get; private set; }
        public string Name { get; private set; }

        public Route(string method, string name, string path, Func<HttpRequest, ActionResult> action)
        {
            this.Method = method;
            this.Path = path;
            this.Pattern = new Regex("^" + path + "$");
            this.Name = name;
            this.action = action;
        }

        public ActionResult InvokeAction(HttpRequest request)
        {
            return action.Invoke(request);
        }
    }
}
