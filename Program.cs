using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using StayQL;
using StayQL.Managers;
using StayQL.Managers.DataClasses;
using StayQL.StayWindows;
using static StayQL.Managers.ConfigurationManager;

namespace StayQL
{
    class Program
    {
        public static string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static void main()
        {
            AppData = Path.Combine(AppData, "StayQL");
            CreateNecessaryFiles();
            ConfigurationManager.Config = JsonConvert.DeserializeObject<ConfigurationStruct>(File.ReadAllText(Path.Combine(AppData, "Config.json")));
            CultureInfo ci = CultureInfo.InstalledUICulture;
            if (ci.EnglishName.Contains("Spanish"))
            {
                LanguageManager.SelectedLanguage = LanguageManager.Language.Spanish;
            }
            else
            {
                LanguageManager.SelectedLanguage = LanguageManager.Language.English;
            }
            NewConnectionWindow();
        }

        public static void NewConnectionWindow()
        {
           ConnectionsW ConnectionsWindow = new ConnectionsW();
           ConnectionsWindow.Show();
        }

        public static async Task StartNewConnection(SQLConnection con, ConnectionsW win)
        {
            if (win != null)
            {

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                win.ShowNotification(new NotificationData(NotificationData.Type.Loading, $"{LanguageManager.GetString("notification.Connecting")} {con.Name}", TimeSpan.FromHours(5)));

            }));

            }
            var re = await con.GetStatus();
            if (win != null)
            {

            if (re != SQLConnection.SQLStatus.AuthCompleted)
            {


                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    win.RemoveNotification(new Managers.DataClasses.NotificationData(Managers.DataClasses.NotificationData.Type.Loading, $"{LanguageManager.GetString("notification.Connecting")} {con.Name}", TimeSpan.FromHours(5)));

                    if (re == SQLConnection.SQLStatus.Connected)
                    {
                        win.ShowNotification(new NotificationData(NotificationData.Type.Failed, LanguageManager.GetString("notification.authFailed").Replace("{0}", con.Name), TimeSpan.FromSeconds(10)));
                        win.ConnectButton.IsEnabled = true;
                    }else if (re == SQLConnection.SQLStatus.Failed)
                    {
                        win.ShowNotification(new NotificationData(NotificationData.Type.Failed, LanguageManager.GetString("notification.timedOut").Replace("{0}", con.Name), TimeSpan.FromSeconds(10)));
                        win.ConnectButton.IsEnabled = true;

                    }



                }));
            }

            }

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                SQLWindow SqlWindow = new SQLWindow(con);
                SqlWindow.Activate();
                SqlWindow.Show();
                if (win != null)
                {
                    win.Close();

                    win = null;
                }
            }));


        }



        private static void CreateNecessaryFiles()
        {
            
            Directory.CreateDirectory(AppData);
            if (!File.Exists(Path.Combine(AppData, "Connections.json")))
            {
                var c = new List<SQLConnection>();
                c.Add(new SQLConnection("Local", "127.0.0.1", 3306, CryptoManager.Encrypt("Ggo61ktHdFEaqtlE", Environment.MachineName.ToString()), "root", "Example SQL Connection"));
                ConfigurationManager.UpdateConnections(c);
            }

            if (!File.Exists(Path.Combine(AppData, "Config.json")))
            {
                ConfigurationStruct config = new ConfigurationStruct();
                config.ShowSystemTables = false;
                config.SystemTables = new List<string>() { "sys", "performance_schema", "mysql", "information_schema" };
                File.WriteAllText(Path.Combine(AppData, "Config.json"), JsonConvert.SerializeObject(config));
            }
        }

        public static void CloseEvent()
        {
            foreach (var item in App.Current.Windows)
            {
                Window window = (Window)item;
                window.Hide();
            }
            Environment.Exit(0);
        }

        public static void ExceptionHandler(Exception er)
        {
            MessageBox.Show(er.Message.ToString());
        }
    }
}
