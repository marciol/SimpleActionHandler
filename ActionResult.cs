using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleActionHandler
{
    public class ActionResult
    {
        public string ContentType { get; set; }
        public string ResponseText { get; set; }

        public ActionResult withContentType(string contentType)
        {
            ContentType = contentType;
            return this;
        }
    }

}
