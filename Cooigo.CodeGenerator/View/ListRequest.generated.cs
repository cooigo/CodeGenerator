﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cooigo.CodeGenerator.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    public partial class ListRequest : RazorGenerator.Templating.RazorTemplateBase
    {
#line hidden

        #line 2 "..\..\View\ListRequest.cshtml"
 public dynamic Model { get; set; } 
        #line default
        #line hidden

        public override void Execute()
        {


WriteLiteral("\r\n");



            
            #line 2 "..\..\View\ListRequest.cshtml"
                                                   
    var dotModule = Model.Module == null ? "" : ("." + Model.Module);
    var moduleDot = Model.Module == null ? "" : (Model.Module + ".");


            
            #line default
            #line hidden
WriteLiteral("namespace ");


            
            #line 6 "..\..\View\ListRequest.cshtml"
      Write(Model.RootNamespace);

            
            #line default
            #line hidden

            
            #line 6 "..\..\View\ListRequest.cshtml"
                            Write(dotModule);

            
            #line default
            #line hidden
WriteLiteral(@".Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ListRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public SortBy[] Sort { get; set; }
        public FilterEntity Filter { get; set; }
    }

    public abstract class FilterEntity { }
    
    public class SortBy
    {
        public SortBy()
        {
        }
        public SortBy(string field)
        {
            Field = field;
        }

        public SortBy(string field, bool descending)
        {
            Field = field;
            Descending = descending;
        }

        public string Field { get; set; }
        public bool Descending { get; set; }

        public override string ToString()
        {
            return Field + "" "" + (Descending ? ""DESC"" : ""ASC"");
        }
    }

    public static class SortByHelper
    {
        public static string ToOrderByString(this SortBy[] sorts)
        {
            if (sorts != null && sorts.Length > 0)
            {
                return "" ORDER BY "" + string.Join("", "", sorts.Select(m => m.ToString()));
            }
            return """";
        }
    }
    
}");


        }
    }
}
#pragma warning restore 1591