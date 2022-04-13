using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace StayQL.Managers
{
    class ConfigurationManager
    {
        public struct ConfigurationStruct
        {
            public bool ShowSystemTables;
            public List<string> SystemTables;
        }

        public static ConfigurationStruct Config;


        public static List<SQLConnection> GetSavedConnections()
        {
            List<SQLConnection> connections = new List<SQLConnection>();
            
            try
            {
                connections = JsonConvert.DeserializeObject<List<SQLConnection>>(File.ReadAllText(Path.Combine(Program.AppData, "Connections.json")));

            }catch(Exception err)
            {
                Program.ExceptionHandler(err);
            }

            if (connections == null)
                connections = new List<SQLConnection>();

            foreach (var item in connections)
            {
                item.UpdateConnectionString();
            }

            return connections;
        }

        public static void SaveConnection(SQLConnection connection)
        {
            List<SQLConnection> connections = GetSavedConnections();
            connections.Add(connection);
            UpdateConnections(connections);
        }

        public static void SaveConfig()
        {
            File.WriteAllText(Path.Combine(Program.AppData, "Config.json"), JsonConvert.SerializeObject(Config));

        }

        public static void UpdateConnections(List<SQLConnection> connections)
        {

            try
            {
                File.WriteAllText(Path.Combine(Program.AppData, "Connections.json"), JsonConvert.SerializeObject(connections, Formatting.Indented));
            }catch(Exception err)
            {
                Program.ExceptionHandler(err);
            }


        }

    }

    [Serializable]
    public class SQLConnection
    {
        public enum SQLStatus
        {
            Unknown = -1,
            Failed = 0,
            Connected = 1,
            AuthCompleted = 2
        }

        public string Name;
        private string _hostname;
        private string _password;
        private int _port;
        private string _username;

        public string Hostname
        {
            get { return _hostname; }

            set
            {
                _hostname = value;
                UpdateConnectionString();
            }
        }
        public string Description;
        public int Port
        {
            get { return _port; }

            set
            {
                _port = value;
                UpdateConnectionString();
            }
        }
        public string Password
        {
            get { return _password; }

            set
            {
                _password = value;
                UpdateConnectionString();
            }
        }
        public string Username
        {
            get { return _username; }

            set
            {
                _username = value;
                UpdateConnectionString();
            }
        }
        [JsonIgnore]
        public string ConnectionString { get; private set; }
        [JsonIgnore]
        public SQLStatus Status { get; private set; }

        public SQLConnection(string Name, string Hostname, int Port, string Password, string Username, string Description)
        {
            this.Name = Name;
            this.Description = Description;
            this.Port = Port;
            this.Hostname = Hostname;
            this.Password = Password;
            this.Username = Username;
        }

        public void SetPassword(string newpassword, bool encrypted = false)
        {
            if (encrypted)
            {
                Password = CryptoManager.Decrypt(newpassword, Environment.MachineName.ToString());
            }
            else
            {
                Password = CryptoManager.Encrypt(newpassword, Environment.MachineName.ToString());
            }
            UpdateConnectionString();
        }

        public string GetPassword()
        {
            return CryptoManager.Decrypt(Password, Environment.MachineName.ToString());
        }

        public void UpdateConnectionString()
        {
            ConnectionString = $"Server={Hostname};Port={Port};Uid={Username};Pwd={GetPassword()};";
        }


        public async Task<SQLStatus> GetStatus()
        {
            MySqlConnection con = new MySqlConnection(ConnectionString);
            SQLStatus res = SQLStatus.Unknown;

            try
            {
                await con.OpenAsync();
                if (con.State == System.Data.ConnectionState.Open)
                    res = SQLStatus.AuthCompleted;
                await con.CloseAsync();
            }catch(MySqlException exception)
            {
                string errorString = exception.ToString();


                if (errorString.ToLower().Contains("access denied for user"))
                {
                    res = SQLStatus.Connected;
                }
                else if (errorString.ToLower().Contains("unable to connect to any of the specified mysql hosts"))
                {
                    res = SQLStatus.Failed;
                }

            }
            
            Status = res;
            return res;

        }

        public Color GetStatusColor()
        {
            if (Status == SQLStatus.Failed)
                return Colors.Red;
            else if (Status == SQLStatus.Connected)
                return Colors.Orange;
            else if (Status == SQLStatus.AuthCompleted)
                return Colors.Green;
            else
                return Colors.Gray;

        }

    }
}
