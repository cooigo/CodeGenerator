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
    public partial class EntityModel : RazorGenerator.Templating.RazorTemplateBase
    {
#line hidden

        #line 2 "..\..\View\EntityModel.cshtml"
 public dynamic Model { get; set; } 
        #line default
        #line hidden

        public override void Execute()
        {


WriteLiteral("\r\n");



            
            #line 2 "..\..\View\EntityModel.cshtml"
                                                   
    var dotModule = Model.Module == null ? "" : ("." + Model.Module);
    var moduleDot = Model.Module == null ? "" : (Model.Module + ".");


            
            #line default
            #line hidden
WriteLiteral("namespace ");


            
            #line 6 "..\..\View\EntityModel.cshtml"
      Write(Model.RootNamespace);

            
            #line default
            #line hidden

            
            #line 6 "..\..\View\EntityModel.cshtml"
                            Write(dotModule);

            
            #line default
            #line hidden
WriteLiteral(".Entities\r\n{\r\n    using System;\r\n    using System.ComponentModel;\r\n    using Syst" +
"em.Collections.Generic;\r\n    using System.IO;\r\n    using Dapper.Contrib.Extensio" +
"ns;\r\n\r\n    public class ");


            
            #line 14 "..\..\View\EntityModel.cshtml"
             Write(Model.ClassName);

            
            #line default
            #line hidden

            
            #line 14 "..\..\View\EntityModel.cshtml"
                                   WriteLiteral("\r\n    {");

            
            #line default
            #line hidden
            
            #line 15 "..\..\View\EntityModel.cshtml"
      foreach (var x in Model.Fields) {
        if (x.Flags == "PrimaryKey" || x.Flags== "Identity")
        {
            
            #line default
            #line hidden
WriteLiteral("\r\n        [Key]");


            
            #line 18 "..\..\View\EntityModel.cshtml"
                    }
            
            #line default
            #line hidden
WriteLiteral("\r\n        public ");


            
            #line 19 "..\..\View\EntityModel.cshtml"
          Write(x.Type);

            
            #line default
            #line hidden
WriteLiteral(" ");


            
            #line 19 "..\..\View\EntityModel.cshtml"
                  Write(x.Ident);

            
            #line default
            #line hidden
WriteLiteral(" { get; set; }");


            
            #line 19 "..\..\View\EntityModel.cshtml"
                                                    }

            
            #line default
            #line hidden
WriteLiteral("\r\n    }\r\n}");


        }
    }
}
#pragma warning restore 1591
