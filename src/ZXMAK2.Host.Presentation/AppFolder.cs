using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ZXMAK2.Host.Presentation
{
    public class AppFolder
    {
        public static String GetAppDataFolder()
        {
            var appName = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
            var appFolder = Path.GetDirectoryName(appName);
            return appFolder;
        }

        public static string GetAppFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
