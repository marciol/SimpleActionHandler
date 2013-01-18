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
        public List<KeyValuePair<string, string>> Headers { get; set; }
        public Dictionary<string, string> Cookies { get; set; }
        public string ResponseText { get; set; }
        public int? StatusCode { get; set; }

        public ActionResult()
        {
            Headers = new List<KeyValuePair<string, string>>();
            Cookies = new Dictionary<string, string>();
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

            foreach (var cookie in Cookies)
                response.AppendCookie(new HttpCookie(cookie.Key, cookie.Value));
        }

        public ActionResult WithContentType(string contentType)
        {
            ContentType = contentType;
            return this;
        }

        public ActionResult WithHeader(string key, string value)
        {
            Headers.Add(new KeyValuePair<string,string>(key, value));
            return this;
        }

        public ActionResult WithCookie(string key, string value)
        {
            Cookies.Add(key, value);
            return this;
        }
    }

}
