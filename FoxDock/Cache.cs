using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxDock
{
    public class Cache
    {
        public List<string> dock_apps = new List<string>();
        public List<string> dock_apps_path = new List<string>();
        public bool runAtStartup = false;
        public bool disableBlur = false;
        public bool enableStarDust = false;
        public bool enableTopmost = false;
        public double scaleFactor = 1;
        public double bg_trans = 1;
        public bool dockAutoHide = true;
    }
}
