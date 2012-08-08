using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DotLiquid;
using DotLiquid.FileSystems;
using System.Text.RegularExpressions;

namespace SimpleActionHandler
{
    class DotLiquidViewEngine : IViewEngine
    {

        public string Layout { get; set; }

        private string templatePath;

        static DotLiquidViewEngine()
        {
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
        }

        public void SetTemplatePath(string path)
        {
            templatePath = path;
            Template.FileSystem = new LocalFileSystem(path);
        }

        public string Render<T>(string path, T parameters)
        {
            var parsedParameters = Hash.FromAnonymousObject(parameters);
            parsedParameters["layout"] = "Layouts/" + Layout; 
            return RenderInternal(path, parsedParameters);
        }
        public string RenderWithoutLayout<T>(string path, T parameters)
        {
            var parsedParameters = Hash.FromAnonymousObject(parameters);
            parsedParameters["layout"] = "Layouts/NoLayout.html"; 
            return RenderInternal(path, parsedParameters);
        }

        private string RenderInternal(string path, Hash parameters)
        {
            var tplFile = GetRenderTemplate(path);
            var tpl = Template.Parse(tplFile);
            var html = tpl.Render(parameters);
            return html;
        }

        private string GetRenderTemplate(string template)
        {
            string tmpSource = String.Empty;
            using (StreamReader reader = new StreamReader(templatePath + "\\" + template))
            {
                tmpSource = reader.ReadToEnd();
            }

            return tmpSource;
        }

    }
}
