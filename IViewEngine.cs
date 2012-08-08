using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace SimpleActionHandler
{
    public interface IViewEngine
    {

        string Layout { get; set; }

        void SetTemplatePath(string path);

        string Render<T>(string path, T parameters);

        string RenderWithoutLayout<T>(string path, T parameters);
    }
}
