using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using SimpleActionHandler.Helpers;

namespace SimpleActionHandler.View
{
    public interface IRenderable
    {
        string RenderContent(string contentName);
        string TransformText();
        string Render<T>(object parameters);
        void Initialize();
    }

    public partial class Layout : IRenderable, IUrlHelpersMixin
    {
        private IRenderable templateContent;

        public UrlHelpers UrlHelpers { get; private set; }

        public Layout()
        {
            UrlHelpers = new UrlHelpers();
        }

        public virtual void Initialize()
        {
        }

        public void SetTemplate(Template templateContent)
        {
            this.templateContent = templateContent;
        }

        public virtual string Render<T>(object parameters)
        {
            return this.TransformText();
        }

        public string YieldContent()
        {
            return templateContent.RenderContent("main");
        }

        public string YieldContentFor(string content)
        {
            return templateContent.RenderContent(content);
        }

        protected string Asset(string path)
        {
            return VirtualPathUtility.ToAbsolute("~/" + path, HttpContext.Current.Request.ApplicationPath);
        }

        public string RenderContent(string contentName) {
            return templateContent.RenderContent(contentName);
        }

    }

    public partial class Template : IRenderable, IUrlHelpersMixin
    {

        public UrlHelpers UrlHelpers { get; private set; }

        public virtual void Initialize()
        {
        }

        private Dictionary<string, string> contents
            = new Dictionary<string, string>();
        
        public Template()
        {
            UrlHelpers = new UrlHelpers();
        }
        
        public virtual string Render<T>(object parameters)
        {
            this.Session = FromAnonymousObject(parameters);
            this.Initialize();
            var layout = System.Activator.CreateInstance<T>() as Layout;
            layout.SetTemplate(this);
            this.Render();
            return layout.TransformText();
        }

        public virtual string Render(object parameters)
        {
            this.Session = FromAnonymousObject(parameters);
            this.Initialize();
            return this.TransformText();
        }

        private void Render()
        {
            contents.Add("main", this.TransformText());
        }

        private Dictionary<string, object> FromAnonymousObject<T>(T obj)
        {
            var d = new Dictionary<string, object>();
            foreach (var p in obj.GetType().GetProperties())
                d.Add(p.Name, p.GetValue(obj, null));

            return d;
        }

        protected void ContentFor(string contentName, Action contentBody)
        {
            var bkpGenEnv = this.GenerationEnvironment;
            var currentGenEnv = new StringBuilder();
            this.GenerationEnvironment = currentGenEnv;
            contentBody.Invoke();
            var x = currentGenEnv.ToString();
            contents.Add(contentName, x);
            this.GenerationEnvironment = bkpGenEnv;
        }

        public string RenderContent(string contentName)
        {
            try
            {
                return contents[contentName];
            }
            catch
            {
                return "";
            }
        }

        protected T CastTo<T>(object value, T type)
        {
            return (T)value;
        }

        //TODO: Colocar em um mixin de helpers

        //protected string UrlFor(string routeName, object args)
        //{
        //    return urlHelpers.UrlFor(routeName, args);
        //}

        //protected string UrlFor(string routeName)
        //{
        //    return urlHelpers.UrlFor(routeName);
        //}

        //protected string RootPath()
        //{
        //    return urlHelpers.RootPath();
        //}

        //protected string Path(string path)
        //{
        //    return urlHelpers.RootPath();
        //}

        //protected string QueryString()
        //{
        //    return urlHelpers.QueryString();
        //}

        protected string NumberDecimalFormat(decimal input, string culture, string format)
        {
            var c = System.Globalization.CultureInfo.CreateSpecificCulture(culture);
            return input.ToString(format, c);
        }

        protected bool IsNegative(decimal number)
        {
            return number < 0.0m;
        }

        protected string JsonData(object obj)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return System.Web.HttpUtility.HtmlEncode(json);
        }

    }
}
