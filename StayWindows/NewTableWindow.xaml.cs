using MahApps.Metro.Controls;
using StayQL.Managers;
using StayQL.Managers.DataClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using Utils = StayQL.Managers.Utils;
using MySql.Data.MySqlClient;
using MySql.Data.MySqlClient.X.XDevAPI.Common;
using System.Collections.ObjectModel;
using static StayQL.StayWindows.NewTableWindow;

namespace StayQL.StayWindows
{
    /// <summary>
    /// Interaction logic for NewTableWindow.xaml
    /// </summary>
    public partial class NewTableWindow : MetroWindow
    {
        SQLConnection connection;
        SQLWindow win;


        public enum ColumnTypes
        {
            INT,
            VARCHAR,
            FLOAT,
            BIGINT,
            SMALLINT,
            TINYINT,
            JSON,
            LONGTEXT,
            TEXT
        }

        public enum Mode
        {
            New = 0,
            Update = 1
        }

        Mode mode;
        Database db;
        string TableName;
        public TRowsT Table = new TRowsT();
        public TRowsT TableDuplicate = new TRowsT();
        public List<TRows> AddedColumns = new List<TRows>();
        public Dictionary<string, TRows> ChangedColumns = new Dictionary<string, TRows>();
        public List<string> Drops = new List<string>();
        public NewTableWindow(SQLConnection con, SQLWindow win, Database db, Mode m, string tble = null)
        {
            TableName = tble;
            this.db = db;
            this.mode = m;
            this.win = win;
            this.connection = con;
            InitializeComponent();
            this.Title = LanguageManager.GetString("newtablewindow.title");
            TableNameTB.Text = LanguageManager.GetString("newtablewindow.defaultTableName");
            TableNameTB.TextChanged += TableNameTB_TextChanged;
            this.MinHeight = 400;
            this.MinWidth = 600;
            TableDG.CanUserAddRows = true;
            TableDG.AutoGenerateColumns = true;
            TableDG.CellEditEnding += DataGrid_CellEditEnding;
            TableDG.ItemsSource = Table;
            TableDG.PreviewKeyDown += TableDG_PreviewKeyDown;
            TableDG.CanUserAddRows = false;
            AddBt.Click += AddBt_Click;
            OKbt.Click += OKbt_Click;
            if (m == Mode.Update)
            {
                
                foreach (var item in db.GetAllColumns(TableName, new MySqlConnection(con.ConnectionString)))
                {
                    string typeParse = item[1].ToString();
                    string typeString = typeParse.Split("(")[0];
                    int? quantity = null;
                    if (typeParse.Contains(")"))
                    {
                        quantity = int.Parse(typeParse.Split("(")[1].Split(")")[0]);
                    }
                    string extra = item[5].ToString();
                    bool autoIncrement = false;
                    bool uniqueIndex = false;
                    if (extra.ToLower().Contains("auto_increment"))
                        autoIncrement = true;
                    if (extra.ToLower().Contains("unique_index"))
                        uniqueIndex = true;

                    bool notNull = false;
                    if (item[2].ToString() == "YES")
                        notNull = true;
                    ColumnTypes ctype = Enum.Parse<ColumnTypes>(typeString, true);
                    bool primaryKey = false;
                    if (item[3].ToString() == "PRI")
                    {
                        primaryKey = true;
                    }
                    if (ctype == ColumnTypes.TINYINT)
                    {
                        if (int.Parse(item[4].ToString()) == 1)
                        {
                            item[4] = true;
                        }
                        else
                        {
                            item[4] = false;

                        }

                    }
                    TRows row = new TRows();
                    row.Name = item[0].ToString();
                    row.Type = ctype;
                    row.Length = quantity;
                    row.AllowNull = notNull;
                    row.AutoIncremental = autoIncrement;
                    row.PrimaryKey = primaryKey;
                    row.UniqueIndex = uniqueIndex;
                    row.DefaultValue = item[4].ToString();
                    Table.Add(row);
                    TableDuplicate.Add(row);
                }
            }

        }

        private void TableDG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var grid = (DataGrid)sender;
            if (Key.Delete == e.Key)
            {

                if (e.Device.Target.GetType().Name == "DataGridCell")
                {
                    

                    foreach (TRows item in grid.SelectedItems)
                    {

                        string query = $"\nDROP COLUMN `{item.Name}`,";
                        Drops.Add(query);
                    }

                }

            }
        }

        private void OKbt_Click(object sender, RoutedEventArgs e)
        {
            connection.UpdateConnectionString();
            MySqlConnection c = new MySqlConnection(connection.ConnectionString);
            
            if (mode == Mode.New)
            {
                string query = $"CREATE TABLE {db.Name}.{TableNameTB.Text} (";
                string primarykeys = "";
                foreach (var item in Table)
                {
                    string name = item.Name;
                    string typeName = item.Type.ToString();
                    
                    if (item.Length != null)
                    {
                        typeName += $"({item.Length})";
                    }

                    string IsNull = "NOT NULL";

                    if (item.AllowNull == true)
                    {
                        IsNull = "NULL";
                    }
                    string AutoIncrement = "";

                    if (item.AutoIncremental == true)
                    {
                        AutoIncrement = "AUTO INCREMENT";
                    }

                    string UniqueIndex = "";

                    if (item.UniqueIndex == true)
                    {
                        UniqueIndex = "UNIQUE INDEX";
                    }

                    string defaults = "";

                    if (item.DefaultValue != null)
                    {
                        string d = item.DefaultValue;
                        if (d.Trim() != "")
                           defaults = $"DEFAULT `{item.DefaultValue}`"; 
                    }

                    query += $"\n`{name}` {typeName} {IsNull} {AutoIncrement} {UniqueIndex} {defaults},";
                    if (item.PrimaryKey)
                    {
                        if (primarykeys == "")
                        {
                            primarykeys += $"`{item.Name}`";
                        }
                        else
                        {
                            primarykeys += $",`{item.Name}`";
                        }
                    }
                }

                query += $"\nPRIMARY KEY ({primarykeys}));";

                try
                {
                    c.Open();
                    MySqlCommand command = new MySqlCommand(query, c);
                    command.ExecuteNonQuery();
                    c.Close();
                    this.Close();
                    win.ReloadDatabases();
                    win.Show();
                }
                catch(Exception err)
                {
                    QueryScriptWindow WINDOW = new QueryScriptWindow(null, null, $"/*{err.Message}*/\n\n\n{query}", true);
                    WINDOW.ShowDialog();
                }
                c.Close();

            }
            else
            {
                string Query = $"ALTER TABLE `{db.Name}`.`{TableName}`";

                foreach (var addedColumn in AddedColumns)
                {
                    string lengthString = "";
                    if (addedColumn.Length != null)
                    {
                        lengthString = $"({addedColumn.Length})";
                    }

                    if (addedColumn.Type == ColumnTypes.VARCHAR || addedColumn.Type == ColumnTypes.TEXT || addedColumn.Type == ColumnTypes.SMALLINT || addedColumn.Type == ColumnTypes.BIGINT)
                    {

                    }
                    else
                    {
                        lengthString = "";
                    }

                    string NotNullString = "NULL";

                    if (addedColumn.AllowNull == false)
                    {
                        NotNullString = "NOT NULL";
                    }

                    string uniqueIndex = "";

                    if (addedColumn.UniqueIndex)
                    {
                        uniqueIndex = "UNIQUE INDEX";
                    }

                    string AutoIncremental = "";
                    if (addedColumn.AutoIncremental)
                    {
                        AutoIncremental = "AUTO INCREMENTAL";
                    }

                    string defaulta = "";
                    if (addedColumn.DefaultValue != null)
                    {
                        defaulta = $"DEFAULT '{addedColumn.DefaultValue}'";
                    }

                    Query += $"\nADD COLUMN `{addedColumn.Name}` {addedColumn.Type}{lengthString} {NotNullString} {uniqueIndex} {AutoIncremental} {defaulta},";
                }
                Query += "\nDROP PRIMARY KEY,";
                foreach (var changedColumn in ChangedColumns)
                {
                    string lengthString = "";
                    if (changedColumn.Value.Length != null)
                    {
                        lengthString = $"({changedColumn.Value.Length})";
                    }
                    if (changedColumn.Value.Type == ColumnTypes.VARCHAR || changedColumn.Value.Type == ColumnTypes.TEXT || changedColumn.Value.Type == ColumnTypes.SMALLINT || changedColumn.Value.Type == ColumnTypes.BIGINT)
                    {

                    }
                    else
                    {
                        lengthString = "";
                    }
                    string NotNullString = "NULL";

                    if (changedColumn.Value.AllowNull == false)
                    {
                        NotNullString = "NOT NULL";
                    }

                    string uniqueIndex = "";

                    if (changedColumn.Value.UniqueIndex)
                    {
                        uniqueIndex = "UNIQUE INDEX";
                    }

                    string AutoIncremental = "";
                    if (changedColumn.Value.AutoIncremental)
                    {
                        AutoIncremental = "AUTO INCREMENTAL";
                    }

                    string defaulta = "";
                    if (changedColumn.Value.DefaultValue != null)
                    {
                        defaulta = $"DEFAULT '{changedColumn.Value.DefaultValue}'";
                    }

                    Query += $"\nCHANGE COLUMN `{changedColumn.Key}` `{changedColumn.Value.Name}` {changedColumn.Value.Type}{lengthString} {NotNullString} {uniqueIndex} {AutoIncremental} {defaulta},";
                }

                foreach (var item in Drops)
                {
                    Query += item;
                }

                string primaryKeys = "";
                foreach (var item in Table)
                {
                    if (item.PrimaryKey)
                    {
                        if (primaryKeys == "")
                        {
                            primaryKeys += $"`{item.Name}`";
                        }
                        else
                        {
                            primaryKeys += $",`{item.Name}`";
                        }
                    }
                }
                Query += $"\nADD PRIMARY KEY ({primaryKeys});\n;";

                try
                {
                    c.Open();
                    MySqlCommand command = new MySqlCommand(Query, c);
                    command.ExecuteNonQuery();
                    c.Close();
                    this.Close();
                    win.ReloadDatabases();
                    win.Show();
                }
                catch (Exception err)
                {
                    QueryScriptWindow WINDOW = new QueryScriptWindow(null, null, $"/*{err.Message}*/\n\n\n{Query}", true);
                    WINDOW.ShowDialog();
                }
                c.Close();
            }

                 
            
        }

        private void AddBt_Click(object sender, RoutedEventArgs e)
        {

            int Index = 0;
            foreach (var row in Table)
            {
                if (row.Type == ColumnTypes.VARCHAR || row.Type == ColumnTypes.TEXT || row.Type == ColumnTypes.SMALLINT || row.Type == ColumnTypes.BIGINT)
                {
                    Utils.GetCell(TableDG, Index, 2).IsEnabled = true;
                    if (row.Length == null)
                    {
                        row.Length = 1;
                    }

                }
                else
                {
                    Utils.GetCell(TableDG, Index, 2).Content = null;
                    row.Length = null;
                    Utils.GetCell(TableDG, Index, 2).IsEnabled = false;
                }

                Index++;
            }

            TRows item = new TRows();
            item.Name = "NewColumn";
            item.Type = ColumnTypes.VARCHAR;
            item.Length = 64;
            item.AllowNull = true;
            item.UniqueIndex = false;
            item.AutoIncremental = false;
            item.PrimaryKey = false;
            item.DefaultValue = null;
            Table.Add(item);
            TableDG.ItemsSource = null;
            TableDG.ItemsSource = Table;
            if (mode == Mode.Update)
                AddedColumns.Add(item);
        }
        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

            string CNAME = Table[e.Row.GetIndex()].Name;
            
            (sender as DataGrid).CellEditEnding -= DataGrid_CellEditEnding;
            (sender as DataGrid).CommitEdit();
            (sender as DataGrid).CellEditEnding += DataGrid_CellEditEnding;
            
            if (Table[e.Row.GetIndex()].Type == ColumnTypes.VARCHAR || Table[e.Row.GetIndex()].Type == ColumnTypes.TEXT || Table[e.Row.GetIndex()].Type == ColumnTypes.SMALLINT || Table[e.Row.GetIndex()].Type == ColumnTypes.BIGINT)
            {
                Utils.GetCell(TableDG, e.Row, 2).IsEnabled = true;
                if (Utils.GetCell(TableDG, e.Row, 2).Content == null)
                {
                    Utils.GetCell(TableDG, e.Row, 2).Content = 1;
                }
            }
            else
            {
                Utils.GetCell(TableDG, e.Row, 2).Content = null;
                Utils.GetCell(TableDG, e.Row, 2).IsEnabled = false;
            }

            if (mode == Mode.Update)
            {
                if (ChangedColumns.ContainsKey(CNAME))
                {
                    ChangedColumns[CNAME] = Table[e.Row.GetIndex()];
                }
                else
                {
                    bool found = false;
                    foreach (var item in AddedColumns)
                    {
                        if (item.Name == Table[e.Row.GetIndex()].Name)
                            found = true;
                    }
                    if (!found)
                        ChangedColumns.Add(CNAME, Table[e.Row.GetIndex()]);
                }
            }
            
            
        }

        private string AllowedChars = "abcdefghijlmnopqrstuvwxyz_";
        private void TableNameTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.TableNameTB.Text.Trim() == "")
            {
                this.OKbt.IsEnabled = false;
            }
            else
            {
                string newName = "";
                foreach (var item in this.TableNameTB.Text)
                {
                    if (AllowedChars.Contains(item.ToString().ToLower()))
                    {
                        newName += item.ToString().ToLower();
                    }
                }
                this.TableNameTB.Text = newName;
                this.TableNameTB.CaretIndex = 999;
                this.OKbt.IsEnabled = true;
            }
        }

        public static void NewTableNow(SQLConnection con, SQLWindow win, Database db, Mode mode, string table = null)
        {
            NewTableWindow Window = new NewTableWindow(con, win,db,mode, table);
            Window.ShowDialog();
            //win.Hide();
            
        }

    }

    public class TRows
    {
        public string Name { get; set; }
        public ColumnTypes Type { get; set; }
        public int? Length { get; set; }
        public bool AllowNull { get; set; }
        public bool AutoIncremental { get; set; }
        public bool UniqueIndex { get; set; }
        public string DefaultValue { get; set; }
        public bool PrimaryKey { get; set; }

    }

    public class TRowsT : List<TRows>
    {
        public TRowsT()
        {

        }
    }
}
