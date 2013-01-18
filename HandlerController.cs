using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;

namespace SimpleActionHandler
{
    public class HandlerController : IController
    {
        public Params<string, object> Params { get; set; }

        public HttpRequest Request { get; set; }
        public HttpResponse Response { get; set; }

        public HandlerController()
        {
            Params = new Params<string, object>();
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
                Headers = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Location", path)
                }
            };
        }

        public class ResponseSelector
        {
            private Dictionary<string, Func<string>> responseTypes = new Dictionary<string, Func<string>>(); 

            public void Js(Func<string> result)
            {
                responseTypes["Js"] = result;
            }

            public void JSON(Func<string> result)
            {
                responseTypes["JSON"] = result;
            }

            public void Html(Func<string> result)
            {
                responseTypes["Html"] = result;
            }

            public string Execute(string responseType)
            {
                return responseTypes[responseType].Invoke();
            }
        }

        public string RespondTo(Action<ResponseSelector> selector)
        {
            var responseSelector = new ResponseSelector(); 
            selector.Invoke(responseSelector);
            if (Regex.IsMatch(Request.Headers["Accept"], ".*json.*"))
            {
                return responseSelector.Execute("JSON");
            }
            else if (Regex.IsMatch(Request.Headers["Accept"], ".*javascript.*"))
            {
                return responseSelector.Execute("Js");
            }
            else
            {
                return responseSelector.Execute("Html");
            }
        }
    }
}
