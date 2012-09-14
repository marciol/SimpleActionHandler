using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;

namespace SimpleActionHandler
{
    public class HandlerController : IController
    {
        public Dictionary<string, object> Params { get; set; }

        public HttpRequest Request { get; set; }
        public HttpResponse Response { get; set; }

        public HandlerController()
        {
            Params = new Dictionary<string, object>();
        }

        public ActionResult Ok(string responseText)
        {
            return new ActionResult { 
                ResponseText = responseText,
                ContentType = "text/html",
                StatusCode = 200,
            };
        }

        public ActionResult Content(string responseText)
        {
            return new ActionResult { ResponseText = responseText };
        }

        public ActionResult Ok(Stream responseStream)
        {
            return new StreamResult
            {
                ResponseStream = responseStream,
                StatusCode = 200,
            };
        }

        public ActionResult RedirectTo(string path)
        {
            return new ActionResult
            {
                StatusCode = 302,
                Headers = new Dictionary<string, string>
                {
                    { "Location", path }
                }
            };
        }
  
    }
}
