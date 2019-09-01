using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxDock
{
    public static class FileTools
    {
        public static string GetExplorerPath()
        {
            return Environment.GetEnvironmentVariable("windir") + "\\explorer.exe";
        }
        public static string GetRecentsPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Recent);
        }
    }
}
