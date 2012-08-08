using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SimpleActionHandler
{
    public class Route
    {
        string method;
        string path;
        Func<HttpRequest, ActionResult> action;

        public string Path
        {
            get
            {
                return path;
            }
        }

        public string Method
        {
            get
            {
                return method;
            }
        }

        public Route(string method, string path, Func<HttpRequest, ActionResult> action)
        {
            this.method = method;
            this.path = path;
            this.action = action;
        }

        public ActionResult InvokeAction(HttpRequest request)
        {
            return action.Invoke(request);
        }
    }
}
