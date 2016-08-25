using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using RazorGenerator.Templating;

namespace Cooigo.CodeGenerator
{
    public static class Templates
    {
        public static string Render<T>(RazorTemplateBase template, T model)
        {
            ((dynamic)template).Model = model;
            return template.TransformText();
        }
    }
}
