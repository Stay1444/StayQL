using StayQL.Managers;
using StayQL.Managers.DataClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace StayQL.StayWindows
{
    /// <summary>
    /// Interaction logic for ConnectionsW.xaml
    /// </summary>
    public partial class ConnectionsW : Window
    {


        private Dictionary<SQLConnection, Grid> Connections = new Dictionary<SQLConnection, Grid>();
        private SQLConnection SelectedConnection = null;
        private TimeSpan UpdateWainting = TimeSpan.FromSeconds(10);
        private System.Timers.Timer UpdateTimer;
        private System.Timers.Timer NotificationTimer;
        private List<NotificationData> Notifications = new List<NotificationData>();
        public ConnectionsW(NotificationData notification = null)
        {
            InitializeComponent();
            LoadSavedConnections();
            NotificationTimer = new System.Timers.Timer(1000);
            NotificationTimer.Enabled = true;
            NotificationTimer.AutoReset = true;
            NotificationTimer.Elapsed += NotificationTimer_Elapsed;
            this.MinWidth = 800;
            this.MinHeight = 600;
            this.Title = LanguageManager.GetString("window.connections.title");
            ControlsGrid.Visibility = Visibility.Collapsed;
            ControlsGrid.IsEnabled = false;
            ConnectButton.Content = LanguageManager.GetString("connect");
            ConnectionName.TextChanged += ConnectionName_TextChanged;
            ConnectionHost.TextChanged += ConnectionHost_TextChanged;
            ConnectionUsername.TextChanged += ConnectionUsername_TextChanged;
            ConnectionPassword.TextChanged += ConnectionPassword_TextChanged;
            this.NewConnectionButton.Content = LanguageManager.GetString("new");
            ConnectionPort.TextChanged += ConnectionPort_TextChanged;
            NewConnectionButton.Click += NewConnectionButton_Click;
            ConnectionDescription.TextChanged += ConnectionDescription_TextChanged;
            UpdateTimer = new System.Timers.Timer(1000);
            UpdateTimer.Enabled = true;
            UpdateTimer.AutoReset = true;
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
            this.Closing += ConnectionsW_Closing;
            ConnectButton.Click += ConnectButton_Click;
            ShowNotification(notification);
            NotificationTimer.Start();

        }

        private void NotificationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Notifications.RemoveAll(x => x.Duration.TotalSeconds < 0);
            foreach (var item in Notifications)
            {
                item.Duration = item.Duration - TimeSpan.FromSeconds(1);
            }
            UpdateNotifications();
        }

        public void ShowNotification(NotificationData notification)
        {
            if (notification == null)
            {
                return;

            }else
            {
                Notifications.Add(notification);
                UpdateNotifications();
            }
        }

        public void RemoveNotification(NotificationData not)
        {
            Notifications.RemoveAll(x => x.Message == not.Message && x.nType == not.nType);
        }

        private void UpdateNotifications()
        {
            try
            {

                if (Application.Current == null)
                    return;
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    try
                    {

                        if (Notifications.Count == 0)
                        {
                            NotificationsSP.Children.Clear();
                            NotificationsSV.IsEnabled = false;
                            NotificationsSV.Visibility = Visibility.Collapsed;
                            Grid.SetColumnSpan(ControlsGrid, 2);
                        }
                        else
                        {
                            Grid.SetColumnSpan(ControlsGrid, 1); 
                            NotificationsSP.Children.Clear();
                            NotificationsSV.IsEnabled = true;
                            NotificationsSV.Visibility = Visibility.Visible;
                        }

                        foreach (var notification in Notifications)
                        {
                            Grid Gridnotification = new Grid();
                            Border border = new Border();

                            Gridnotification.Margin = new Thickness(0, 0, 0, 5);
                            border.BorderThickness = new Thickness(1);

                            if (notification.nType == NotificationData.Type.Succeed)
                            {
                                border.BorderBrush = new SolidColorBrush(Colors.LightGreen);
                            }else if (notification.nType == NotificationData.Type.Failed)
                            {
                                border.BorderBrush = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                border.BorderBrush = new SolidColorBrush(Colors.White);
                            }

                            Gridnotification.Children.Add(border);
                            TextBlock text = new TextBlock();
                            text.Text = notification.Message;
                            text.FontSize = 16;
                            text.FontFamily = new FontFamily("Sans Serif");
                            text.Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
                            Gridnotification.Height = 100;
                            Gridnotification.Children.Add(text);
                            NotificationsSP.Children.Add(Gridnotification);
                        }

                    }
                    catch (Exception) { }
                }));

            }catch(Exception err) { }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {

            ConnectButton.IsEnabled = false;
            Task.Run(() => Program.StartNewConnection(SelectedConnection, this));
        }

        private void ConnectionsW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveConnections(true);
        }

        private void ConnectionUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedConnection != null && SelectedConnection.Username != ((WatermarkTextBox)sender).Text)
            {
                SelectedConnection.Username = ((WatermarkTextBox)sender).Text;

                SaveConnections();
                Task.Run(() => PingConnections(SelectedConnection));
                UpdateConnectionProperties();

            }
        }

        ~ConnectionsW()
        {
            NotificationTimer.Stop();
            NotificationTimer.Dispose();
            UpdateTimer.Stop();
            UpdateTimer = null;
        }

        private void UpdateConnectionProperties()
        {
            foreach (var Connection in Connections)
            {
                Grid ConnectionGrid = Connection.Value;
                Label host = null;
                Label Name = null;
                Label Username = null;

                foreach (var child in ConnectionGrid.Children)
                {
                    if (child is Label)
                    {
                        Label clabel = (Label)child;
                        if (clabel.Tag.ToString() == "host")
                            host = clabel;
                        else if (clabel.Tag.ToString() == "username")
                            Username = clabel;
                        else if (clabel.Tag.ToString() == "title")
                            Name = clabel;
                    }
                }

                if (host != null)
                    host.Content = Connection.Key.Hostname;

                if (Name != null)
                    Name.Content = Connection.Key.Name;

                if (Username != null)
                    Username.Content = Connection.Key.Username;


            }
        }

        private void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateWainting -= TimeSpan.FromSeconds(1);
            if (UpdateWainting.TotalSeconds == 0)
            {
                List<SQLConnection> con = new List<SQLConnection>();
                foreach (var item in Connections)
                {
                    con.Add(item.Key);
                }
                ConfigurationManager.UpdateConnections(con);
            }
        }

        private void NewConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            SQLConnection newConnection = new SQLConnection(LanguageManager.GetString("newConection.defaultName"), "127.0.0.1", 3306, "", "root", LanguageManager.GetString("newConection.defaultDescription"));
            AddSqlConnection(newConnection);
            SaveConnections();
            Task.Run(() => PingConnections(newConnection));
            UpdateConnectionProperties();

        }

        private void ConnectionDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedConnection != null && SelectedConnection.Description != ((WatermarkTextBox)sender).Text)
            {
                SelectedConnection.Description = ((WatermarkTextBox)sender).Text;
                SaveConnections();
                UpdateConnectionProperties();
            }
        }

        private void ConnectionPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedConnection == null)
                return;
            string numbers = "0123456789";
            WatermarkTextBox tb = (WatermarkTextBox)sender;
            if (tb.Text == SelectedConnection.Port.ToString())
                return;
            string NewText = "";
            foreach (var item in tb.Text.ToCharArray())
            {
                if (numbers.Contains(item))
                    NewText += item;
            }

            if (int.TryParse(NewText, out int res))
            {
                res = Math.Clamp(res, 1, 65535);
            }
            tb.Text = res.ToString();

            if (SelectedConnection != null)
            {
                SelectedConnection.Port = res;
                SaveConnections();
                Task.Run(() => PingConnections(SelectedConnection));

                UpdateConnectionProperties();

            }
        }

        

        private void ConnectionPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedConnection != null && SelectedConnection.GetPassword() != ((WatermarkTextBox)sender).Text)
            {
                SelectedConnection.SetPassword(((WatermarkTextBox)sender).Text);
                SaveConnections();
                Task.Run(() => PingConnections(SelectedConnection));

                UpdateConnectionProperties();
            }
        }

        private void ConnectionHost_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedConnection != null && SelectedConnection.Hostname != ((WatermarkTextBox)sender).Text)
            {
                SelectedConnection.Hostname = ((WatermarkTextBox)sender).Text;
                SaveConnections();
                Task.Run(() => PingConnections(SelectedConnection));
                UpdateConnectionProperties();


            }
        }

        private void ConnectionName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedConnection != null && SelectedConnection.Name != ((WatermarkTextBox)sender).Text)
            {
                SelectedConnection.Name = ((WatermarkTextBox)sender).Text;
                SaveConnections();
                UpdateConnectionProperties();


            }
        }

        private void LoadSavedConnections() 
        {
            var Result = ConfigurationManager.GetSavedConnections();

            foreach (var item in Result)
            {
                AddSqlConnection(item);
            }

            Task.Run(() => PingConnections());

        }

        private void SaveConnections(bool force = false)
        {
            if (force)
            {
                UpdateWainting = TimeSpan.FromSeconds(1);
                UpdateTimer_Elapsed(null, null);
            }
            UpdateWainting = TimeSpan.FromSeconds(5);
        }

        private void AddSqlConnection(SQLConnection con)
        {
            Grid Grid = new Grid();
            Label Title = new Label();
            Label Host = new Label();
            Label Username = new Label();
            Ellipse Status = new Ellipse();
            var ContextMenu = new ContextMenu();
            var DuplicateCM = new MenuItem();
            var DeleteCM = new MenuItem();
            DuplicateCM.Header = LanguageManager.GetString("connections.connection.contextmenu.duplicateButton.header");
            DeleteCM.Header = LanguageManager.GetString("connections.connection.contextmenu.deleteButton.header");
            DeleteCM.Foreground = new SolidColorBrush(Colors.Red);
            ContextMenu.Items.Add(DuplicateCM);
            ContextMenu.Items.Add(DeleteCM);
            DeleteCM.Tag = con;
            DuplicateCM.Tag = con;
            DeleteCM.Click += DeleteCM_Click;
            DuplicateCM.Click += DuplicateCM_Click;
            Grid.ContextMenu = ContextMenu;
            Title.Content = con.Name;
            Host.Content = con.Hostname;
            Username.Content = con.Username;
            #region rows
            Grid.RowDefinitions.Add(new RowDefinition());
            Grid.RowDefinitions.Add(new RowDefinition());
            Grid.RowDefinitions.Add(new RowDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            #endregion
            Grid.Background = new SolidColorBrush(Color.FromArgb(255, 54, 54, 54));
            Grid.Children.Add(Title);
            Grid.Height = 100;
            Grid.Margin = new Thickness(0, 0, 0, 5);
            Grid.Children.Add(Status);
            Grid.SetColumn(Title, 1);
            Grid.SetColumnSpan(Title, 4);
            Grid.SetRow(Title, 0);
            Grid.SetRowSpan(Title, 3);
            Title.HorizontalContentAlignment = HorizontalAlignment.Left;
            Title.Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
            Host.Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
            Username.Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
            Title.FontSize = 24;
            Title.FontFamily = new FontFamily("Sans Serif");
            Host.FontFamily = new FontFamily("Sans Serif");
            Username.FontFamily = new FontFamily("Sans Serif");
            Title.Margin = new Thickness(0, 3, 0, 0);
            Status.Margin = new Thickness(6, 8, 0, 0);
            Title.Tag = "title";
            Username.Tag = "username";
            Host.Tag = "host";
            Grid.SetRow(Host, 3);
            Grid.SetColumn(Host, 100);            
            Grid.SetRow(Username, 1);
            Grid.SetColumn(Username, 100);
            Grid.SetRow(Status, 0);
            Grid.SetColumn(Status, 0);
            Status.Height = 10;
            Status.Width = 10;
            Status.Fill = new SolidColorBrush(Colors.Transparent);
            Border border = new Border();
            Grid.Children.Add(Host);
            Grid.SetRowSpan(border, 999);
            Grid.SetColumnSpan(border, 999);
            Grid.Children.Add(border);
            Grid.Children.Add(Username);
            border.Visibility = Visibility.Collapsed;
            Connections.Add(con, Grid);
            SavedConnectionsSP.Children.Add(Grid);
            Grid.MouseLeftButtonDown += Grid_MouseLeftButtonDown;
            Grid.MouseEnter += Grid_MouseEnter;
            
            Grid.MouseLeave += Grid_MouseLeave;
        }

        private void DuplicateCM_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mitem = (MenuItem)sender;
            SQLConnection connection = (SQLConnection)mitem.Tag;
            SQLConnection newconnection = new SQLConnection(connection.Name, connection.Hostname, connection.Port, connection.Password, connection.Username, connection.Description);
            AddSqlConnection(newconnection);
            Task.Run(() => PingConnections(newconnection));
            SaveConnections(true);
            FakeGridClick(Connections[newconnection]);
        }

        private void DeleteCM_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mitem = (MenuItem)sender;
            SQLConnection connection = (SQLConnection)mitem.Tag;
            Grid grid = Connections[connection];
            Connections.Remove(connection);
            SavedConnectionsSP.Children.Remove(grid);
            SaveConnections(true);

            if (SavedConnectionsSP.Children.Count == 0 || SelectedConnection == connection)
            {
                ControlsGrid.IsEnabled = false;
                ControlsGrid.Visibility = Visibility.Collapsed;
            }
            
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
            Grid grid = (Grid)sender;
            grid.Background = new SolidColorBrush(Color.FromArgb(255, 54, 54, 54));
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
            Grid grid = (Grid)sender;
            grid.Background = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));

        }

        private void FakeGridClick(object sender)
        {
            this.Focus();
            ControlsGrid.Visibility = Visibility.Visible;
            ControlsGrid.IsEnabled = true;
            Grid grid = (Grid)sender;
            SQLConnection connection = null;
            foreach (var item in Connections)
            {
                if (item.Value == grid)
                {
                    connection = item.Key;
                }
            }
            if (connection == null)
                return;
            SelectedConnection = connection;

            ConnectionName.Text = connection.Name;
            ConnectionHost.Text = connection.Hostname;
            ConnectionPort.Text = connection.Port.ToString();
            ConnectionDescription.Text = connection.Description;
            ConnectionUsername.Text = connection.Username;
            ConnectionPassword.Text = connection.GetPassword();

            foreach (var item in grid.Children)
            {
                if (item is Border)
                {
                    Border border = (Border)item;
                    border.Visibility = Visibility.Visible;
                    border.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 137, 123));
                    border.BorderThickness = new Thickness(5, 0, 0, 0);
                }
            }

            foreach (var item in Connections)
            {
                if (item.Key == connection)
                    continue;
                Grid tempgrid = (Grid)item.Value;

                foreach (var s in tempgrid.Children)
                {
                    if (s is Border)
                    {
                        Border border = (Border)s;
                        border.Visibility = Visibility.Collapsed;

                    }
                }

            }


        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                FakeGridClick(sender);
                Program.StartNewConnection(SelectedConnection, this);
            }
            else
            {
                FakeGridClick(sender);
            }
        }

        private async Task PingConnections(SQLConnection connect = null)
        {


            if (connect == null)
            {

                try
                {
                    foreach (var item in Connections)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            foreach (var child in item.Value.Children)
                            {
                                if (child is Ellipse)
                                {
                                    var ecl = (Ellipse)child;
                                    ecl.Fill = new SolidColorBrush(Colors.White);
                                }
                            }
                        }));
                    }

                    foreach (var item in Connections)
                    {

                        new Thread(() =>
                        {
                            item.Key.GetStatus().Wait();
                            if (Application.Current == null)
                                return;
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                foreach (var child in item.Value.Children)
                                {
                                    if (child is Ellipse)
                                    {
                                        var ecl = (Ellipse)child;
                                        ecl.Fill = new SolidColorBrush(item.Key.GetStatusColor());
                                    }
                                }
                            }));

                        }).Start();
                        


                    }


                }
                catch (Exception err)
                {
                }
            }
            else
            {
                var item = Connections[connect];
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    foreach (var child in item.Children)
                    {
                        if (child is Ellipse)
                        {
                            var ecl = (Ellipse)child;
                            ecl.Fill = new SolidColorBrush(Colors.White);
                        }
                    }
                }));
                await connect.GetStatus();
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    foreach (var child in item.Children)
                    {
                        if (child is Ellipse)
                        {
                            var ecl = (Ellipse)child;
                            ecl.Fill = new SolidColorBrush(connect.GetStatusColor());
                        }
                    }
                }));
            }
        }

    }
}
