using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

namespace SimpleActionHandler.View
{
    public interface IRenderable
    {
        string TransformText();
        string Render<T>(object parameters);
        void Initialize();
    }

    public partial class Layout : IRenderable
    {
        public IRenderable Content { get; set; }

        public virtual void Initialize()
        {
        }
        
        public virtual string Render<T>(object parameters)
        {
            return this.TransformText();
        }

        public string YieldContent()
        {
            return Content.TransformText();
        }

        protected string Asset(string path)
        {
            return HttpContext.Current.Request.ApplicationPath + path;
        }

    }

    public partial class Template : IRenderable
    {

        public virtual void Initialize()
        {
        }

        public string Render<T>(object parameters)
        {
            this.Session = FromAnonymousObject(parameters);
            this.Initialize();
            var layout = System.Activator.CreateInstance<T>() as Layout;
            layout.Content = this;
            return layout.TransformText();
        }

        private Dictionary<string, object> FromAnonymousObject<T>(T obj)
        {
            var d = new Dictionary<string, object>();
            foreach (var p in obj.GetType().GetProperties())
                d.Add(p.Name, p.GetValue(obj, null));

            return d;
        }

        protected T CastTo<T>(object value, T type)
        {
            return (T)value;
        }

        //TODO: Colocar em um mixin de helpers

        protected string UrlFor(string routeName, params object[] args)
        {
            var handler = (ModuleHandler)HttpContext.Current.Handler;
            var str = (string)handler.NamedRoutes[routeName];
            var pattern = @"\(.*?\)";
            var matches = Regex.Matches(str, pattern);
            var fmtStr = str;
            for (var i = 0; i < matches.Count; i++)
                fmtStr = fmtStr.Replace(matches[i].Value, String.Format("{{0}}", i));

            return String.Format(fmtStr, args);
        }

        protected string NumberDecimalFormat(decimal input, string culture, string format)
        {
            var c = System.Globalization.CultureInfo.CreateSpecificCulture(culture);
            return input.ToString(format, c);
        }

        protected bool IsNegative(decimal number)
        {
            return (int)number < 0.0m;
        }

    }
}
