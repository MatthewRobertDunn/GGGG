using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGGG.Interface.IO
{
    public static class AppFolder
    {
        public static string GetPathToAppFolder(string extraPath = null)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string path = appDataPath;

            path = Path.Combine(path, ConfigurationManager.AppSettings["AppFolderName"]);


            if (!string.IsNullOrEmpty(extraPath))
                path = Path.Combine(path, extraPath);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
    }
}
