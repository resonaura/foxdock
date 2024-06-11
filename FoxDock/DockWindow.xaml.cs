using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.IO;
using System.Diagnostics;
using System.Windows.Threading;
using Point = System.Windows.Point;
using Shell32;
using Path = System.IO.Path;
using FoxDock.API;

namespace FoxDock
{
    public partial class DockWindow : Window
    {
        //Подключаем кеш
        public static Cache cache = new Cache();

        //Основные таймеры
        private readonly System.Timers.Timer mainTimer = new System.Timers.Timer();

        //Инициализируем окна

        private List<SmartHomeDevice> smartHomeDevices = new List<SmartHomeDevice>();
        /// <summary>
        /// Инициализация дока
        /// </summary>
        public DockWindow()
        {
            API.WindowsManager.SendToBack(this);
            InitializeComponent(); //Инициализируем все компоненты
        }

        private void DockMain_Activated(object sender, EventArgs e)
        {
            API.WindowsManager.SendToBack(this);
        }

        private void DockMain_StateChanged(object sender, EventArgs e)
        {
            API.WindowsManager.SendToBack(this);
        }
    }
}
