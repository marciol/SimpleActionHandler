using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SimpleActionHandler
{
    public class ActionResult
    {
        public string ContentType { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string ResponseText { get; set; }
        public int? StatusCode { get; set; }

        public ActionResult()
        {
            Headers = new Dictionary<string,string>();
        }

        public virtual void ExecuteResult(HttpResponse response)
        {
            BuildResponse(response);
            response.Write(ResponseText);
        }

        public void BuildResponse(HttpResponse response)
        {
            if (ContentType != null)
                response.ContentType = ContentType;

            if (StatusCode != null)
                response.StatusCode = StatusCode.Value;

            response.ContentEncoding = System.Text.Encoding.UTF8;

            foreach (var header in Headers)
                response.AddHeader(header.Key, header.Value);
        }

        public ActionResult withContentType(string contentType)
        {
            ContentType = contentType;
            return this;
        }

        public ActionResult withHeader(string key, string value)
        {
            Headers.Add(key, value);
            return this;
        }
    }

}
