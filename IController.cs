using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SimpleActionHandler
{
    public interface IController
    {
        Params<string, object> Params { get; set; }
        HttpRequest Request { get; set; }
        HttpResponse Response { get; set; }
    }
}
