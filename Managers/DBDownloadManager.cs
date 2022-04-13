using Newtonsoft.Json;
using StayQL.Managers.DataClasses;
using StayQL.StayWindows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace StayQL.Managers
{
    class DBDownloadManager
    {
        public enum DownloadType
        {
            Backup = 0,
            User = 1
        }

        public static void DownloadSchema(DownloadType type, Database db)
        {
            Directory.CreateDirectory(Path.Combine(Program.AppData, "Backups"));
            string BackupDir = Path.Combine(Program.AppData, "Backups");
            foreach (var item in db.Tables)
            {
                db.ReadData(item.Key, false);
            }
            File.WriteAllText(Path.Combine(BackupDir, $"{db.Name}-{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Hour}-{DateTime.Now.Minute}.json"), JsonConvert.SerializeObject(db, Formatting.Indented));

        }
    }
}
