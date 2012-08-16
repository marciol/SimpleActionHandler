using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DotLiquid;
using System.Web;

namespace SimpleActionHandler
{
    public static class Utils
    {
        public static string RenderTemplate<T>(string path, T parameters)
        {
            var file = String.Empty;
            path = Path.Combine(HttpContext.Current.Server.MapPath("~/"), path);

            using (StreamReader reader = new StreamReader(path))
            {
                file = reader.ReadToEnd();
            }

            var tpl = Template.Parse(file);
            var result = tpl.Render(new RenderParameters { LocalVariables = Hash.FromAnonymousObject(parameters) });
            return result;
        }
    }
}
