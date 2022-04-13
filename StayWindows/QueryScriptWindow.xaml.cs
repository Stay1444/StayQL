using ICSharpCode.AvalonEdit.Highlighting;
using MahApps.Metro.Controls;
using MySql.Data.MySqlClient;
using StayQL.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StayQL.StayWindows
{
    /// <summary>
    /// Interaction logic for QueryScriptWindow.xaml
    /// </summary>
    public partial class QueryScriptWindow : MetroWindow
    {
        private SQLConnection con = null;
        SQLWindow Win;
        bool OnlyEditor = false;
        public QueryScriptWindow(SQLConnection conection, SQLWindow window, string text = "", bool onlyEditor = false)
        {
            OnlyEditor = onlyEditor;
            Win = window;
            InitializeComponent();
            con = conection;
            if (onlyEditor == true)
            {
                this.RunBt.Content = "Close";
                this.QueryScript.IsReadOnly = true;
            }
            this.Title = "Query";
            this.RunBt.Click += RunBt_Click;
            this.QueryScript.Text =text;
            this.QueryScript.ShowLineNumbers = true;
            this.QueryScript.TextChanged += QueryScript_TextChanged1;
            QueryScript.Foreground = new SolidColorBrush(Colors.White);
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("StayQL.Assets.sql.xshd"))
            {
                using (var reader = new System.Xml.XmlTextReader(stream))
                {
                    QueryScript.SyntaxHighlighting =
                        ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader,
                        ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                }
            }
        }

        private void QueryScript_TextChanged1(object sender, EventArgs e)
        {
            if (this.QueryScript.Text.Trim() == "")
                this.RunBt.IsEnabled = false;
            else
                this.RunBt.IsEnabled = true;
        }

        private void RunBt_Click(object sender, RoutedEventArgs e)
        {
            if (OnlyEditor)
            {
                this.Close();
                return;
            }
            try
            {

            MySqlConnection conn = new MySqlConnection(con.ConnectionString);
            MySqlCommand command = new MySqlCommand(this.QueryScript.Text, conn);
            conn.Open();
            command.ExecuteNonQuery();
            conn.Close();
            }catch(Exception err)
            {
                MessageBox.Show(err.Message);
            }

            Win.ReloadDatabases();

        }
    }
}
