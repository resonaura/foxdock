using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxDock
{
    public enum DockBackground
    {
        Auto,
        Black,
        Gray,
        White,
        Accent
    }
    public enum HintBackground
    {
        Auto,
        Black,
        Gray,
        White,
        Accent
    }
    public enum IndicatorColor
    {
        Auto,
        Black,
        Gray,
        White,
        Accent
    }
    public enum MenuColor
    {
        Auto,
        Black,
        Gray,
        White,
        Accent
    }
    /// <summary>
    /// Класс кеша
    /// </summary>
    public class Cache
    {
        public List<string> dock_apps = new List<string>();
        public List<string> dock_apps_path = new List<string>();
        public bool runAtStartup = false;
        public bool disableBlur = false;
        public bool enableStarDust = false;
        public bool enableTopmost = false;
        public double scaleFactor = 1;
        public double bg_trans = 0.5;
        public double hm_trans = 1;
        public bool dockAutoHide = true;
        public bool dockLock = false;
        public DockBackground background = DockBackground.Auto;
        public HintBackground hintBackground = HintBackground.Auto;
        public IndicatorColor IndicatorColor = IndicatorColor.Auto;
        public MenuColor MenuColor = MenuColor.Auto;
        public bool smart_disable = true;
    }
}
