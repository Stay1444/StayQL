using MahApps.Metro.Controls;
using MySql.Data.MySqlClient;
using StayQL.Managers;
using StayQL.Managers.DataClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static StayQL.Managers.ConfigurationManager;

namespace StayQL.StayWindows
{
    /// <summary>
    /// Interaction logic for SQLWindow.xaml
    /// </summary>
    public partial class SQLWindow : MetroWindow
    {

        public SQLConnection Connection;
        private List<Database> SDatabases = new List<Database>();
        private MySqlConnection SqlC;
        Database SelectedDB;
        string SelectedTable;

        public SQLWindow(SQLConnection connection)
        {
            Connection = connection;
            Connection.UpdateConnectionString();
            SqlC = new MySqlConnection(Connection.ConnectionString);
            SqlC.Open();
            InitializeComponent();
            this.MinHeight = 720;
            this.MinWidth = 1280;
            ReadDatabases();
            MainGrid.ColumnDefinitions[0].Width = new GridLength(Math.Round(this.Width / 4));
            this.WindowState = WindowState.Maximized;
            this.Title = connection.Name;
            TableDG.CellEditEnding += TableDG_CellEditEnding;
            NewConnectionButtonBar.Click += NewConnectionButtonBar_Click;
            NewConnectionButtonBar.ToolTip = LanguageManager.GetString("newconection.tooltip");
            TableDG.PreviewKeyDown += TableDG_PreviewKeyDown;
            ExitButton.Click += ExitButton_Click;
            NewWindowButton.Click += NewWindowButton_Click;
            DisconnectButton.Click += DisconnectButton_Click;
            WipeConfigurationButton.Click += WipeConfigurationButton_Click;
            OpenConnectionsFileButton.Click += OpenConnectionsFileButton_Click;
            AddSchemaButton.Click += AddSchemaButton_Click;
            RefreshDatabases.Click += RefreshDatabases_Click;
            NewQueryButton.Click += NewQueryButton_Click;
            OpenQueryFileButton.Click += OpenQueryFileButton_Click;
            ShowSystemTablesButton.Click += ShowSystemTablesButton_Click;
            UpdateMenuItemLanguage(UpMenuGrid.Children[0] as Menu);
            SpanishButtonL.Click += SpanishButtonL_Click;
            EnglishButtonL.Click += EnglishButtonL_Click;

            ShowSystemTablesButton_Click(null, null);

        }

        private void EnglishButtonL_Click(object sender, RoutedEventArgs e)
        {
            LanguageManager.SelectedLanguage = LanguageManager.Language.English;
            Program.StartNewConnection(Connection, null);
            this.Close();
        }

        private void SpanishButtonL_Click(object sender, RoutedEventArgs e)
        {
            LanguageManager.SelectedLanguage = LanguageManager.Language.Spanish;
            Program.StartNewConnection(Connection, null);
            this.Close();
        }

        private void ShowSystemTablesButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null && e == null)
            {

                if (ConfigurationManager.Config.ShowSystemTables == true)
                {

                    ShowSystemTablesButton.Tag = "Hide System Tables";
                    UpdateMenuItemLanguage(ShowSystemTablesButton);
                }
                else
                {

                    ShowSystemTablesButton.Tag = "Show System Tables";
                    UpdateMenuItemLanguage(ShowSystemTablesButton);
                }
                return;
            }

            if (ConfigurationManager.Config.ShowSystemTables == false)
            {

                ShowSystemTablesButton.Tag = "Hide System Tables";
                UpdateMenuItemLanguage(ShowSystemTablesButton);
                ConfigurationManager.Config.ShowSystemTables = true;
            }
            else
            {

                ShowSystemTablesButton.Tag = "Show System Tables";
                UpdateMenuItemLanguage(ShowSystemTablesButton);
                ConfigurationManager.Config.ShowSystemTables = false;
            }
            ConfigurationManager.SaveConfig();
            ReloadDatabases();
        }

        private void UpdateMenuItemLanguage(MenuItem item)
        {
            if (item.Tag != null)
                if (item.Tag.ToString().Trim() != "")
                    item.Header = LanguageManager.GetString(item.Tag.ToString());

            foreach (var c in item.Items)
            {
                if (c is MenuItem)
                    UpdateMenuItemLanguage((MenuItem)c);
                else if (c is Menu)
                    UpdateMenuItemLanguage((Menu)c);
            }
        }

        private void UpdateMenuItemLanguage(Menu menu)
        {
            foreach (var item in menu.Items)
            {
                if (item is MenuItem)
                    UpdateMenuItemLanguage((MenuItem)item);

                if (item is Menu)
                    UpdateMenuItemLanguage((Menu)item);
            }
        }

        private void OpenQueryFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                string file = File.ReadAllText(filename);
                QueryScriptWindow window = new QueryScriptWindow(Connection, this, file);
                window.Show();
            }
        }

        private void NewQueryButton_Click(object sender, RoutedEventArgs e)
        {
            QueryScriptWindow window = new QueryScriptWindow(Connection, this);
            window.Show();
        }

        private void RefreshDatabases_Click(object sender, RoutedEventArgs e)
        {
            ReloadDatabases();
        }

        private void AddSchemaButton_Click(object sender, RoutedEventArgs e)
        {
            NewSchemaWindow ns = new NewSchemaWindow(Connection, this);
            ns.Show();
        }

        private void OpenConnectionsFileButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("notepad.exe", System.IO.Path.Combine(Program.AppData, "Connections.json"));
        }

        private void WipeConfigurationButton_Click(object sender, RoutedEventArgs e)
        {
            Directory.Delete(Program.AppData, true);
        }

        private void NewTabToCurrentServerButton_Click(object sender, RoutedEventArgs e)
        {
            Program.StartNewConnection(Connection, null);
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();

            Program.NewConnectionWindow();
            Thread.Sleep(100);
            this.Close();
        }

        private void NewWindowButton_Click(object sender, RoutedEventArgs e)
        {
            Program.NewConnectionWindow();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void NewConnectionButtonBar_Click(object sender, RoutedEventArgs e)
        {
            Program.NewConnectionWindow();
        }


        public bool IsValid(DependencyObject parent)
        {
            if (Validation.GetHasError(parent))
                return false;

            // Validate all the bindings on the children
            for (int i = 0; i != VisualTreeHelper.GetChildrenCount(parent); ++i)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (!IsValid(child)) { return false; }
            }

            return true;
        }

        private void TableDG_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            (sender as DataGrid).CellEditEnding -= TableDG_CellEditEnding;
            (sender as DataGrid).CommitEdit();
            (sender as DataGrid).CellEditEnding += TableDG_CellEditEnding;

            if (!IsValid(TableDG))
            {
                LogC(LanguageManager.GetString("Error.DataGridError"), true);
                return;
            }

            DataTable table = SelectedDB.Tables[SelectedTable];

            int indexTest = e.Row.GetIndex();

            string ColumnPrimary = "";
            foreach (var item in SelectedDB.GetAllColumns(SelectedTable, new MySqlConnection(Connection.ConnectionString)))
            {
                if (item[3].ToString() == "PRI")
                {
                    ColumnPrimary = item[0].ToString();
                    break;
                }
            }
            int cindex = 0;
            foreach (DataColumn item in table.Columns)
            {
                if (item.ColumnName == ColumnPrimary)
                {
                    cindex = table.Columns.IndexOf(item);
                }
            }

            if (table.Rows.Count > indexTest)
                Task.Run(() => FinishEdit(e, table.Rows[indexTest].ItemArray[cindex]));
            else
                Task.Run(() => FinishEditNewRow(e));

        }

        private void TableDG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var grid = (DataGrid)sender;
            if (Key.Delete == e.Key)
            {

                if (e.Device.Target.GetType().Name == "DataGridCell")
                {
                    if (grid.SelectedItems.Count == 1)
                    {

                    }
                    else
                    {
                        var Result = MessageBox.Show(LanguageManager.GetString("confirm.deletion.description").Replace("{0}", grid.SelectedItems.Count.ToString()), LanguageManager.GetString("confirm.deletion.title"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (Result == MessageBoxResult.No)
                        {

                            grid.CanUserDeleteRows = false;
                            Task.Run(() => ReEnableDeletion());
                            return;
                        }

                    }

                    foreach (DataRowView item in grid.SelectedItems)
                    {
                        DataTable table = SelectedDB.Tables[SelectedTable];
                        string ColumnPrimary = "";
                        foreach (var c in SelectedDB.GetAllColumns(SelectedTable, new MySqlConnection(Connection.ConnectionString)))
                        {
                            if (c[3].ToString() == "PRI")
                            {
                                ColumnPrimary = c[0].ToString();
                                break;
                            }
                        }
                        int cindex = 0;
                        foreach (DataColumn c in table.Columns)
                        {
                            if (c.ColumnName == ColumnPrimary)
                            {
                                cindex = table.Columns.IndexOf(c);
                            }
                        }

                        string query = $"DELETE FROM {SelectedDB.Name}.{SelectedTable} WHERE {ColumnPrimary}='{item.Row.ItemArray[cindex]}'";
                        LogC(query);

                        SelectedDB.RunCommand(query);
                    }

                    SelectedDB.ReadData(SelectedTable);

                }
                    
            }
        }

        private async Task ReEnableDeletion()
        {
            await Task.Delay(100);
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                TableDG.CanUserDeleteRows = true;
            }));
        }


        private async Task FinishEdit(DataGridCellEditEndingEventArgs e, object idRowBefore)
        {
            await Task.Delay(50);
            
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                DataTable table = SelectedDB.Tables[SelectedTable];

                object da = table.Rows[e.Row.GetIndex()].ItemArray[e.Column.DisplayIndex];
                string newdata = "";
                if (da is bool)
                {
                    newdata = da.ToString().Replace("True", "1").Replace("False", "0");
                }
                else
                {
                    newdata = da.ToString();
                }

                string ColumnPrimary = "";
                foreach (var item in SelectedDB.GetAllColumns(SelectedTable, new MySqlConnection(Connection.ConnectionString)))
                {
                    if (item[3].ToString() == "PRI")
                    {
                        ColumnPrimary = item[0].ToString();
                        break;
                    }
                }

                string query = $"UPDATE {SelectedDB.Name}.{SelectedTable} SET {e.Column.Header.ToString()}='{newdata}' WHERE {ColumnPrimary}='{idRowBefore}'";
                SelectedDB.RunCommand(query);
                SelectedDB.ReadData(SelectedTable);
                LogC(query);
            }));
            
        }

        private async Task FinishEditNewRow(DataGridCellEditEndingEventArgs e)
        {
            await Task.Delay(50);
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    DataTable table = SelectedDB.Tables[SelectedTable];
                    object da = table.Rows[e.Row.GetIndex()].ItemArray[e.Column.DisplayIndex];
                    string newdata = "";
                    if (da is bool)
                    {
                        newdata = da.ToString().Replace("True", "1").Replace("False", "0");
                    }
                    else
                    {
                        newdata = da.ToString();
                    }

                    string query = $"INSERT INTO {SelectedDB.Name}.{SelectedTable} ({e.Column.Header}) VALUES ('{newdata}')";
                    SelectedDB.RunCommand(query);
                    SelectedDB.ReadData(SelectedTable);
                    LogC(query);
                    }catch(Exception err)
                    { Program.ExceptionHandler(err); }
            }));
        }

        public void LogC(string msg, bool IsError = false)
        {
            msg = "\n" + msg;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {


                if (IsError)
                {
                    LogsTb.Inlines.Add(new Run(msg) { Foreground = Brushes.Red });

                }
                else
                {
                    LogsTb.Inlines.Add(new Run(msg) { Foreground = Brushes.White });

                }

                this.LogsSV.ScrollToBottom();

            }));
        }

        private void ReadDatabases()
        {
            DatabasesSP.Children.Clear();
            SDatabases.Clear();
            SelectedDB = null;
            TableDG.ItemsSource = null;
            MySqlCommand command = SqlC.CreateCommand();
            command.CommandText = "SHOW DATABASES;";
            MySqlDataReader Reader;
            Reader = command.ExecuteReader();
            List<string> rab = new List<string>();
            while (Reader.Read())
            {
                string row = "";
                for (int i = 0; i < Reader.FieldCount; i++)
                    row += Reader.GetValue(i).ToString();
                rab.Add(row);
            }
            Reader.Close();
            foreach (var item in rab)
            {

                if (!ConfigurationManager.Config.ShowSystemTables)
                {
                    if (ConfigurationManager.Config.SystemTables.Contains(item))
                        continue;
                }
                AddDatabase(new Database(item, Connection));

            }


        }

        private void AddDatabase(Database b)
        {
            TreeView tv = new TreeView();
            tv.Focusable = false;
            tv.Background = new SolidColorBrush(Colors.Transparent);
            tv.BorderThickness = new Thickness(0);
            tv.Margin = new Thickness(0, 0, 0, 5);
            tv.Tag = "imTreeview";
            TreeViewItem Schema = new TreeViewItem();
            Schema.Header = b.Name;
            Schema.Focusable = false;
            ContextMenu cm = new ContextMenu();
            MenuItem newTable = new MenuItem();
            MenuItem deleteSchema = new MenuItem();
            newTable.Header = LanguageManager.GetString("database.tree.newTable.header");
            cm.Items.Add(newTable);
            deleteSchema.Foreground = new SolidColorBrush(Colors.Red);
            deleteSchema.Header = LanguageManager.GetString("delete");
            deleteSchema.Click += DeleteSchema_Click;
            deleteSchema.Tag = b;
            cm.Items.Add(deleteSchema);
            newTable.Tag = b;
            newTable.Click += NewTable_Click;
            Schema.ContextMenu = cm;
            tv.Items.Add(Schema);
            foreach (var item in b.Tables)
            {
                ContextMenu tcm = new ContextMenu();
                MenuItem titm = new MenuItem();
                MenuItem UpdateTableMenuItem = new MenuItem();
                UpdateTableMenuItem.Header = LanguageManager.GetString("database.tree.modifyTable.header");
                UpdateTableMenuItem.Click += UpdateTableMenuItem_Click;
                UpdateTableMenuItem.Tag = new object[] { b, item.Key };
                tcm.Items.Add(UpdateTableMenuItem);
                titm.Header = LanguageManager.GetString("database.tree.deleteTable.header");
                titm.Foreground = new SolidColorBrush(Colors.Red);
                tcm.Items.Add(titm);
                titm.Tag = new object[] { b, item.Key};
                titm.Click += Titm_Click;
                TreeViewItem ta = new TreeViewItem();
                ta.Header = item.Key;
                ta.Focusable = false;
                ta.MouseLeave += Ta_MouseLeave;
                ta.MouseEnter += Ta_MouseEnter;
                ta.ContextMenu = tcm;
                ta.Tag = b;
                ta.MouseLeftButtonDown += Ta_MouseDown;
                Schema.Items.Add(ta);
            }
            DatabasesSP.Children.Add(tv);

            SDatabases.Add(b);
        }

        private void DeleteSchema_Click(object sender, RoutedEventArgs e)
        {
            Database db = (sender as MenuItem).Tag as Database;
            var Result = MessageBox.Show(LanguageManager.GetString("deleteTable.message.content").Replace("{0}", db.Name), LanguageManager.GetString("deleteTable.message.title").Replace("{0}", db.Name), MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (Result != MessageBoxResult.Yes)
                return;
            DBDownloadManager.DownloadSchema(DBDownloadManager.DownloadType.Backup, db);
            LogC("A backup of the schema has been saved");
            db.RunCommand($"DROP DATABASE {db.Name};");
            ReloadDatabases();
        }

        public void ReloadDatabases()
        {
            List<string> expandedSchemas = new List<string>();
            string CurrentItemSource = SelectedTable;
            Database currentDatabase = SelectedDB;
            foreach (TreeView treeview in DatabasesSP.Children)
            {
                TreeViewItem sch = treeview.Items[0] as TreeViewItem;
                if (sch.IsExpanded)
                    expandedSchemas.Add(sch.Header.ToString());
            }
            ReadDatabases();
            foreach (TreeView treeview in DatabasesSP.Children)
            {
                TreeViewItem sch = treeview.Items[0] as TreeViewItem;
                sch.IsExpanded = expandedSchemas.Contains(sch.Header.ToString());

                if (CurrentItemSource != null && CurrentItemSource.Trim() != "" && currentDatabase != null)
                {
                    foreach (TreeViewItem item in sch.Items)
                    {
                        if (item.Header.ToString() == CurrentItemSource)
                        {
                            if ((item.Tag as Database).Name == currentDatabase.Name)
                            {
                                TableDG.ItemsSource = ((item.Tag as Database).Tables[CurrentItemSource].AsDataView());
                                (item.Tag as Database).ReadData(CurrentItemSource);
                            }
                        }
                    }
                }
            }


        }
        private void UpdateTableMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem MI = sender as MenuItem;
            Database DB = (Database)((object[])MI.Tag)[0];
            string Table = (string)((object[])MI.Tag)[1];

            NewTableWindow.NewTableNow(Connection, this, DB, NewTableWindow.Mode.Update, Table);
        }

        private void Titm_Click(object sender, RoutedEventArgs e)
        {
            MenuItem MI = sender as MenuItem;
            Database DB = (Database)((object[])MI.Tag)[0];
            string Table = (string)((object[])MI.Tag)[1];
            var Result =  MessageBox.Show(LanguageManager.GetString("deleteTable.message.content").Replace("{0}", Table), LanguageManager.GetString("deleteTable.message.title").Replace("{0}",Table), MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (Result != MessageBoxResult.Yes)
                return;

            DB.RunCommand($"DROP TABLE {DB.Name}.{Table}");
            LogC($"DROP TABLE {DB.Name}.{Table}");
            ReloadDatabases();
        }

        private void NewTable_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            Database db = mi.Tag as Database;
            NewTableWindow.NewTableNow(Connection, this, db, NewTableWindow.Mode.New);

        }

        private void Ta_MouseDown(object sender, MouseButtonEventArgs e)
        {

            TreeViewItem tvi = (TreeViewItem)sender;
            Database db = (Database)tvi.Tag;
            Mouse.OverrideCursor = Cursors.Wait;
            //TableDG.ItemsSource = db.Tables[tvi.Header.ToString()].AsDataView();
            ReloadDatabases();
            SelectedDB = db;
            SelectedTable = tvi.Header.ToString();
            SelectedDB.ReadData(SelectedTable);
            TableDG.ItemsSource = SelectedDB.Tables[SelectedTable].AsDataView();
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void Ta_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void Ta_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;

        }

        
    }
}
