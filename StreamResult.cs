using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;

namespace SimpleActionHandler
{
    public class StreamResult : ActionResult
    {

        public Stream ResponseStream { get; set; }

        public override void ExecuteResult(HttpResponse response)
        {
            base.ExecuteResult(response);
            ResponseStream.Seek(0, SeekOrigin.Begin);
            ResponseStream.CopyTo(response.OutputStream);
        }
    }
}
