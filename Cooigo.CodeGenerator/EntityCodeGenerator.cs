using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cooigo.CodeGenerator
{
    public class EntityCodeGenerator
    {
        private GeneratorConfig config;
        private EntityCodeGenerationModel model;
        private string siteWebPath;
        private string siteWebProj;
        private string scriptPath;
        private string scriptProject;
        private Encoding utf8 = new UTF8Encoding(true);

        private void AppendComment(StreamWriter sw)
        {
            sw.WriteLine();
            sw.WriteLine();
            sw.WriteLine("/* ------------------------------------------------------------------------- */");
            sw.WriteLine("/*                                                                           */");
            sw.WriteLine("/* ------------------------------------------------------------------------- */");
        }

        public EntityCodeGenerator(EntityCodeGenerationModel model, GeneratorConfig config)
        {
            var kdiff3Paths = new[]
            {
                config.KDiff3Path,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "KDiff3\\kdiff3.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "KDiff3\\kdiff3.exe"),
            };

            this.model = model;
            CodeFileHelper.Kdiff3Path = kdiff3Paths.FirstOrDefault(File.Exists);

            if (config.TFSIntegration)
                CodeFileHelper.SetupTFSIntegration(config.TFPath);

            CodeFileHelper.SetupTSCPath(config.TSCPath);

            siteWebProj = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.ModelProjectFile));
            siteWebPath = Path.GetDirectoryName(siteWebProj);
            if (!string.IsNullOrEmpty(config.ScriptProjectFile))
            {
                scriptProject = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.ScriptProjectFile));
                scriptPath = Path.GetDirectoryName(scriptProject);

                if (!File.Exists(scriptProject))
                {
                    scriptProject = null;
                    scriptPath = null;
                }
            }

            this.config = config;
        }

        public void Run()
        {
            if (!scriptPath.IsEmptyOrNull())
                Directory.CreateDirectory(scriptPath);

            Directory.CreateDirectory(siteWebPath);

            GenerateRow();
            GenerateFilter();
            GenerateRepository();

            GenerateDeleteRequest();
            GenerateListRequest();
            GenerateListResponse();
            GenerateSaveRequest();

            GenerateUnitOfWork();


        }

        private string CreateDirectoryOrBackupFile(string file)
        {
            if (File.Exists(file))
                return BackupFile(file);
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                return null;
            }
        }

        private string BackupFile(string file)
        {
            if (File.Exists(file))
            {
                var backupFile = string.Format("{0}.{1}.bak", file, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                CodeFileHelper.CheckoutAndWrite(backupFile, File.ReadAllBytes(file), false);
                return backupFile;
            }

            return null;
        }

        private void CreateNewSiteWebFile(string code, string relativeFile, string dependentUpon = null)
        {
            string file = Path.Combine(siteWebPath, relativeFile);
            var backup = CreateDirectoryOrBackupFile(file);
            CodeFileHelper.CheckoutAndWrite(file, code, true);
            CodeFileHelper.MergeChanges(backup, file);
            ProjectFileHelper.AddFileToProject(siteWebProj, relativeFile, dependentUpon);
        }

        private bool ScriptFileExists(string relativeFile)
        {
            return File.Exists(scriptProject) &&
                File.Exists(Path.Combine(Path.GetDirectoryName(scriptProject), relativeFile));
        }

        private void CreateNewSiteScriptFile(string code, string relativeFile, string dependentUpon = null)
        {
            string file = Path.Combine(scriptPath, relativeFile);
            var backup = CreateDirectoryOrBackupFile(file);
            CodeFileHelper.CheckoutAndWrite(file, code, true);
            CodeFileHelper.MergeChanges(backup, file);
            ProjectFileHelper.AddFileToProject(scriptProject, relativeFile, dependentUpon);
        }
        

        private void GenerateRow()
        {
            CreateNewSiteWebFile(Templates.Render(new Views.EntityModel(), new
            {
                ClassName = model.ClassName,
                RowClassName = model.RowClassName,
                Module = model.Module,
                RootNamespace = model.RootNamespace,
                Fields = model.Fields,
                IdField = model.Identity,
                NameField = model.NameField
            }), Path.Combine(@"Modules\", Path.Combine("", Path.Combine(model.ClassName, model.ClassName + ".cs"))));
        }
        private void GenerateFilter()
        {
            CreateNewSiteWebFile(Templates.Render(new Views.FilterEntity(), new
            {
                ClassName = model.ClassName,
                RowClassName = model.RowClassName,
                Module = model.Module,
                RootNamespace = model.RootNamespace,
                Fields = model.Fields,
                IdField = model.Identity,
                NameField = model.NameField
            }), Path.Combine(@"Modules\", Path.Combine("", Path.Combine(model.ClassName, model.ClassName + "Filter.cs"))));
        }

        private void GenerateRepository()
        {
            CreateNewSiteWebFile(Templates.Render(new Views.EntityRepository(), new
            {
                ClassName = model.ClassName,
                RowClassName = model.RowClassName,
                Module = model.Module,
                RootNamespace = model.RootNamespace,
                Fields = model.Fields,
                IdField = model.Identity,
                NameField = model.NameField
            }), Path.Combine(@"Modules\", Path.Combine("", Path.Combine(model.ClassName, model.ClassName + "Repository.cs"))));
        }

        private void GenerateDeleteRequest()
        {
            CreateNewSiteWebFile(Templates.Render(new Views.DeleteModel(), new
            {
                ClassName = model.ClassName,
                RowClassName = model.RowClassName,
                Module = model.Module,
                RootNamespace = model.RootNamespace,
                Fields = model.Fields,
                IdField = model.Identity,
                NameField = model.NameField
            }), Path.Combine(@"Modules\", Path.Combine("", "DeleteModel.cs")));
        }
        private void GenerateListRequest()
        {
            CreateNewSiteWebFile(Templates.Render(new Views.ListRequest(), new
            {
                ClassName = model.ClassName,
                RowClassName = model.RowClassName,
                Module = model.Module,
                RootNamespace = model.RootNamespace,
                Fields = model.Fields,
                IdField = model.Identity,
                NameField = model.NameField
            }), Path.Combine(@"Modules\", Path.Combine("", "ListRequest.cs")));
        }

        private void GenerateListResponse()
        {
            CreateNewSiteWebFile(Templates.Render(new Views.ListResponse(), new
            {
                ClassName = model.ClassName,
                RowClassName = model.RowClassName,
                Module = model.Module,
                RootNamespace = model.RootNamespace,
                Fields = model.Fields,
                IdField = model.Identity,
                NameField = model.NameField
            }), Path.Combine(@"Modules\", Path.Combine("", "ListResponse.cs")));
        }
        private void GenerateSaveRequest()
        {
            CreateNewSiteWebFile(Templates.Render(new Views.SaveModel(), new
            {
                ClassName = model.ClassName,
                RowClassName = model.RowClassName,
                Module = model.Module,
                RootNamespace = model.RootNamespace,
                Fields = model.Fields,
                IdField = model.Identity,
                NameField = model.NameField
            }), Path.Combine(@"Modules\", Path.Combine("", "SaveModel.cs")));
        }

        private void GenerateUnitOfWork()
        {
            CreateNewSiteWebFile(Templates.Render(new Views.UnitOfWork(), new
            {
                ClassName = model.ClassName,
                RowClassName = model.RowClassName,
                Module = model.Module,
                RootNamespace = model.RootNamespace,
                Fields = model.Fields,
                IdField = model.Identity,
                NameField = model.NameField
            }), Path.Combine(@"Modules\", Path.Combine("", "UnitOfWork.cs")));
        }

    }
}