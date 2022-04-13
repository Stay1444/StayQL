using MahApps.Metro.Controls;
using MySql.Data.MySqlClient;
using StayQL.Managers;
using System;
using System.Collections.Generic;
using System.Text;
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
    /// Interaction logic for NewSchemaWindow.xaml
    /// </summary>
    public partial class NewSchemaWindow : MetroWindow
    {
        SQLConnection con;
        SQLWindow window;
        public NewSchemaWindow(SQLConnection connetion, SQLWindow window)
        {
            this.window = window;
            con = connetion;
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Height = 150;
            this.Width = 500;
            this.ResizeMode = ResizeMode.NoResize;
            this.Title = LanguageManager.GetString("NewSchemaWindow.title");
            this.SchemaName.Text = LanguageManager.GetString("NewSchemaWindow.default");
            OKbt.Click += OKbt_Click;
            this.SchemaName.TextChanged += SchemaName_TextChanged;
        }

        private string AllowedChars = "abcdefghijlmnopqrstuvwxyz_";
        private void SchemaName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.SchemaName.Text.Trim() == "")
            {
                this.OKbt.IsEnabled = false;
            }
            else
            {
                string newName = "";
                foreach (var item in this.SchemaName.Text)
                {
                    if (AllowedChars.Contains(item.ToString().ToLower()))
                    {
                        newName += item.ToString().ToLower();
                    }
                }
                this.SchemaName.Text = newName;
                this.SchemaName.CaretIndex = 999;
                this.OKbt.IsEnabled = true;
            }
        }

        private void OKbt_Click(object sender, RoutedEventArgs e)
        {
            con.UpdateConnectionString();
            MySqlConnection connection = new MySqlConnection(con.ConnectionString);
            string query = $"CREATE SCHEMA {this.SchemaName.Text}";
            MySqlCommand command = new MySqlCommand(query, connection);
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
            window.ReloadDatabases();
            this.Close();
        }
    }
}
