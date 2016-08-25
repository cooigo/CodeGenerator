using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Cooigo.CodeGenerator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private BindingList<GeneratorConfig.Connection> _connections;
        private BindingList<string> _tables;
        private GeneratorConfig config;

        public MainWindow()
        {
            InitializeComponent();

            _connections = new BindingList<GeneratorConfig.Connection>();
            _tables = new BindingList<string>();

            this.ConnectionsCombo.DataContext = _connections;
            this.DataContext = this;

            this.config = GeneratorConfig.Load();

            foreach (var connection in config.Connections)
                _connections.Add(connection);
        }

        public BindingList<string> Tables { get { return _tables; } }


        private string _connectionKey;
        public string ConnectionKey
        {
            get { return _connectionKey; }
            set
            {
                if (value != _connectionKey)
                {
                    _connectionKey = value;
                    Changed("ConnectionKey");
                }
            }
        }
        private void Changed(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void Sil_Click(object sender, RoutedEventArgs e)
        {
            var connection = (GeneratorConfig.Connection)this.ConnectionsCombo.SelectedItem;
            config.Connections.Remove(connection);
            _connections.Remove(connection);
            //config.Save();
        }

        private void Ekle_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AddConnectionStringWindow();
            if (dlg.ShowDialog() == true)
            {
                var cstr = dlg.Key.Text.Trim();
                if (cstr.Length < 0)
                    throw new ArgumentNullException("connectionKey");

                var connection = config.Connections.FirstOrDefault(x => String.Compare(x.Key, cstr, StringComparison.OrdinalIgnoreCase) == 0);
                if (connection == null)
                {
                    connection = new GeneratorConfig.Connection
                    {
                        Key = cstr,
                        ConnectionString = dlg.ConnectionString.Text.Trim(),
                        ProviderName = dlg.Provider.Text.Trim(),
                        Tables = new List<GeneratorConfig.Table>
                        {

                        }
                    };

                    config.Connections.Add(connection);
                    _connections.Clear();
                    _connections.AddRange(config.Connections);
                }
                else
                {
                    connection.ConnectionString = dlg.ConnectionString.Text.Trim();
                    connection.ProviderName = dlg.Provider.Text.Trim();
                }

                this.ConnectionsCombo.SelectedItem = connection;
                config.Save();
            }
        }

        private void ConnectionsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this._tables.Clear();

            if (this.ConnectionsCombo.SelectedItem != null)
            {
                var conn = (GeneratorConfig.Connection)this.ConnectionsCombo.SelectedItem;

                try
                {
                    using (var connection = new SqlConnection(conn.ConnectionString))
                    {
                        connection.Open();

                        foreach (var t in SqlSchemaInfo.GetTableNames(connection))
                            _tables.Add(((t.Item1 != null) ? (t.Item1 + ".") : "") + t.Item2);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                ConnectionKey = conn.Key;
            }
        }


        private void DataAccessProjectFileBrowse(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            if (string.IsNullOrWhiteSpace(DataAccessProjectFile))
            {
                dlg.FileName = "*.csproj";
                dlg.InitialDirectory = Path.GetDirectoryName(GeneratorConfig.GetConfigurationFilePath());
            }
            else
            {
                var webProjectFile = Path.GetFullPath(DataAccessProjectFile);
                dlg.FileName = Path.GetFileName(webProjectFile);
                dlg.InitialDirectory = Path.GetDirectoryName(webProjectFile);
            }

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                DataAccessProjectFile = GeneratorConfig.GetRelativePath(dlg.FileName, AppDomain.CurrentDomain.BaseDirectory);
                config.Save();
            }
        }

        public string DataAccessProjectFile
        {
            get { return config.DataAccessProjectFile; }
            set
            {
                if (value != config.DataAccessProjectFile)
                {
                    config.DataAccessProjectFile = value;
                    Changed("DataAccessProjectFile");
                }
            }
        }

        private void ModelProjectFileBrowse(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            if (string.IsNullOrWhiteSpace(ModelProjectFile))
            {
                dlg.FileName = "*.csproj";
                dlg.InitialDirectory = Path.GetDirectoryName(GeneratorConfig.GetConfigurationFilePath());
            }
            else
            {
                var webProjectFile = Path.GetFullPath(ModelProjectFile);
                dlg.FileName = Path.GetFileName(webProjectFile);
                dlg.InitialDirectory = Path.GetDirectoryName(webProjectFile);
            }

            bool? result = dlg.ShowDialog();

            if (result.Value == true)
            {
                ModelProjectFile = GeneratorConfig.GetRelativePath(dlg.FileName, AppDomain.CurrentDomain.BaseDirectory);
                config.Save();
            }
        }

        public string ModelProjectFile
        {
            get { return config.ModelProjectFile; }
            set
            {
                if (value != config.ModelProjectFile)
                {
                    config.ModelProjectFile = value;
                    Changed("ModelProjectFile");
                }
            }
        }
        public string RootNamespace
        {
            get { return config.RootNamespace; }
            set
            {
                if (value != config.RootNamespace)
                {
                    config.RootNamespace = value;
                    Changed("RootNamespace");
                }
            }
        }



        private void btnGenerateCodes_Click(object sender, RoutedEventArgs e)
        {
            if (this.lstTable.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select tables to generate code for!");
                return;
            }
            

            foreach (string tableName in lstTable.SelectedItems)
            {
                try
                {
                    GenerateCodeFor(tableName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            MessageBox.Show("Code files for selected tables are generated. Please REBUILD SOLUTION before running application, otherwise you may have script errors!");
        }

        private void GenerateCodeFor(string tableName)
        {
            RootNamespace = txtRootNamespace.Text;

            EntityCodeGenerationModel rowModel;
            var conn = (GeneratorConfig.Connection)this.ConnectionsCombo.SelectedItem;
            using (var connection = new SqlConnection(conn.ConnectionString))
            {
                connection.Open();
                var table = tableName;
                string tableSchema = null;
                if (table.IndexOf('.') > 0)
                {
                    tableSchema = table.Substring(0, table.IndexOf('.'));
                    table = table.Substring(table.IndexOf('.') + 1);
                }


                var e = LoadTableInfo(tableName);


                rowModel = RowGenerator.GenerateModel(connection, tableSchema, table,
                    "xxx", ConnectionKey, e, "", config);
                new EntityCodeGenerator(rowModel, config).Run();
            }
        }
        private string LoadTableInfo(string tableName)
        {
            var connection = this.ConnectionsCombo.SelectedItem as GeneratorConfig.Connection;
            var table = connection != null ? connection.Tables.FirstOrDefault(x => x.Tablename == tableName) : null;
            var tableOnly = tableName;
            if (tableOnly.IndexOf('.') >= 0)
                tableOnly = tableOnly.Substring(tableOnly.IndexOf('.') + 1);

            return (table == null ? Inflector.Inflector.Titleize(tableOnly).Replace(" ", "") : table.Identifier);

        }
    }
}
