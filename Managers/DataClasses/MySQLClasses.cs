using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;

namespace StayQL.Managers.DataClasses
{
    public class Database
    {
        public string Name;

        [JsonIgnore]
        MySqlConnection connection;

        public Dictionary<string, DataTable> Tables = new Dictionary<string, DataTable>();
        public Database(string name, SQLConnection abc)
        {
            abc.UpdateConnectionString();
            this.connection = new MySqlConnection(abc.ConnectionString);
            Name = name;
            foreach (var item in GetAllTables(connection))
            {
                DataTable table = new DataTable();

                foreach (var c in GetAllColumns(item, connection))
                {

                    string ValueType = c[1].ToString().Split("(")[0].ToLower();


                    if (ValueType == "int")
                    {
                        table.Columns.Add(c[0].ToString(), typeof(int));
                    }else if (ValueType == "text" || ValueType == "varchar" || ValueType == "longtext" || ValueType == "json" || ValueType == "mediumtext")
                    {
                        table.Columns.Add(c[0].ToString(), typeof(string));
                    }else if (ValueType == "tinyint")
                    {
                        table.Columns.Add(c[0].ToString(), typeof(bool));
                    }else if (ValueType == "float")
                    {
                        table.Columns.Add(c[0].ToString(), typeof(float));

                    }else if (ValueType == "bigint")
                    {
                        table.Columns.Add(c[0].ToString(), typeof(long));
                    }
                    else
                    {
                    }
                }

                Tables.Add(item, table);

            }

        }

        public Database() { }

        public void RunCommand(string str)
        {
            MySqlCommand command = new MySqlCommand(str, connection);
            connection.Open();
            try
            {

            command.ExecuteNonQuery();
            }catch(Exception err)
            {
                Program.ExceptionHandler(err);
            }
            connection.Close();
        }

        public void ReadData(string tableName, bool clearRows = true)
        {
            if (clearRows)
            {
                foreach (var item in Tables)
                {
                    item.Value.Rows.Clear();
                }
            }
            DataTable table = Tables[tableName];
            table.Rows.Clear();
            MySqlCommand command = new MySqlCommand($"select * from {Name}.{tableName}", connection);
            connection.Open();
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                List<object> Values = new List<object>();
                foreach (var item in table.Columns)
                {
                    Values.Add(reader[item.ToString()]);
                }
                table.Rows.Add(Values.ToArray());
                
            }
            reader.Close();
            connection.Close();

        }

        string[] GetAllTables(MySqlConnection connection)
        {
            connection.Open();
            List<string> result = new List<string>();
            MySqlCommand cmd = new MySqlCommand($"show tables from {Name}", connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader.GetString(0));
            }
            reader.Close();
            connection.Close();
            return result.ToArray();
        }

        public List<object[]> GetAllColumns(string table, MySqlConnection connection)
        {
            connection.Open();
            List<object[]> result = new List<object[]>();
            MySqlCommand cmd = new MySqlCommand($"SHOW COLUMNS FROM {Name}.{table}", connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new object[] { reader.GetString(0), reader.GetString(1), reader.GetString(2), reader["Key"], reader["Default"], reader["Extra"]});
            }
            reader.Close();
            connection.Close();
            return result;
        }


    }

 


}
