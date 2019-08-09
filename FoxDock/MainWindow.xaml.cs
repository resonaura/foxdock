using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.IO.Ports;
using System.IO;
using Microsoft.Win32;
using System.Xml.Serialization;
using System.Drawing;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Windows.Threading;
using System.Drawing.Drawing2D;
using System.Windows.Media.Effects;
using Point = System.Windows.Point;

namespace FoxDock
{
    public partial class MainWindow : Window
    {

        public static Cache cache = new Cache();
        private System.Timers.Timer mainTimer = new System.Timers.Timer();
        private System.Timers.Timer mouseTimer = new System.Timers.Timer();

        private Tooltip tooltip = new Tooltip();
        private Settings settings = new Settings();
        private Dialog dialog;


        public static bool lock_slider = true;
        public WinStates winStates = new WinStates();
        public bool isInitedAS = false;
        public int taskbar_g = 0;
        public MainWindow()
        {
            InitializeComponent();
            WindowAPI.window = this;

            Process[] explorer_p = Process.GetProcessesByName("explorer");
            string explorer_name = explorer_p[0].MainModule.FileVersionInfo.FileDescription;
            ExplorerIcon.Label = explorer_name;

            int taskbar = 0;
            WindowAPI.TaskBarLocation location = WindowAPI.GetTaskBarLocation();
            if (location == WindowAPI.TaskBarLocation.BOTTOM)
            {
                taskbar = Application.Current.Dispatcher.Invoke(() => (int)(WpfScreen.GetScreenFrom(this).DeviceBounds.Bottom - WpfScreen.GetScreenFrom(this).WorkingArea.Bottom));
            }
            taskbar_g = taskbar;



            RoutedEventHandler handler = null;
            handler = (s, e) =>
            {


                PresentationSource source = PresentationSource.FromVisual(this);

                double dpiY = 1;
                if (source != null)
                {
                    dpiY = source.CompositionTarget.TransformToDevice.M22;
                }
                tooltip.Hide();

                Loaded -= handler;
                cache = CacheOperations.LoadCache(cache);
                if (cache.disableBlur == false)
                    NativeMethods.EnableBlur(this);
                if (cache.enableStarDust)
                    StarDust.Visibility = Visibility.Visible;

                settings.DisableBlurToggle.IsChecked = cache.disableBlur;
                settings.StarDustEnableToggle.IsChecked = cache.enableStarDust;
                settings.EnableTopmostToggle.IsChecked = cache.enableTopmost;
                settings.AHToggle.IsChecked = cache.dockAutoHide;
                settings.Trans_bar.Value = cache.bg_trans;
                settings.ScaleSlider.Value = cache.scaleFactor;


                size = (int)(defsize * cache.scaleFactor);

                double new_h = size + size / 2.5;
                double new_top = System.Windows.SystemParameters.PrimaryScreenHeight - new_h;

                animateHChange(new_top, new_h);

                AutoWallUI(true);
                DockLockUpdateUI();

                cache = CacheOperations.LoadCache(cache);

                App_full_bg.Opacity = cache.bg_trans;


                settings.StartupToggle.IsChecked = cache.runAtStartup;

                Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 60 });

                cache = CacheOperations.LoadCache(cache);
                size = (int)(defsize * cache.scaleFactor);
                //consoleLog(size + " !!! " + cache.scaleFactor);


                this.Height = new_h;

                double top = System.Windows.SystemParameters.PrimaryScreenHeight - this.Height - taskbar_g;
                DoubleAnimation myDoubleAnimation = new DoubleAnimation
                {
                    From = top + this.Height + taskbar_g,
                    To = top,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new PowerEase(),

                };
                Timeline.SetDesiredFrameRate(myDoubleAnimation, 60);
                myDoubleAnimation.Completed += MyDoubleAnimation_Completed;
                this.BeginAnimation(Window.TopProperty, myDoubleAnimation);
                this.BeginAnimation(OpacityProperty, Animations.OpacityAnimation(0, 1));
            };
            Loaded += handler;

            WindowAPI.MakeWin();

            AutoWallUI();

            this.StateChanged += MainWindow_StateChanged;

            settings.window = this;

            var exists = System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1;
            if (exists)
            {
                WindowAPI.ShowDesktop();
                Close();
                tooltip.Close();
                settings.Close();

                Environment.Exit(0);
            }


            GraphicsPath gp = new GraphicsPath();


            mainTimer.Interval = 2000;
            mainTimer.Elapsed += MainTimer_Tick;
            mainTimer.Start();

            mouseTimer.Interval = 2000;
            mouseTimer.Elapsed += MouseTimer_Elapsed;
            mouseTimer.Start();


            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            SystemEvents.UserPreferenceChanging += SystemEvents_UserPreferenceChanging;
            cache = CacheOperations.LoadCache(cache);
            if (!isInitedAS)
            {
                size = (int)(defsize * cache.scaleFactor);
                settings.ScaleSlider.Value = cache.scaleFactor;
                isInitedAS = true;
            }

            if (cache.dock_apps_path != null)
            {
                foreach (string path in cache.dock_apps_path)
                {
                    addIconToPanel(path);
                }
            }
            lock_slider = false;




        }
        public void DockLockUpdateUI()
        {
            if (cache.dockLock)
            {
                LockDockButton.Header = "Unlock Dock";
                LockDockIcon.Text = "\uE785";
            }
            else
            {
                LockDockButton.Header = "Lock Dock";
                LockDockIcon.Text = "\uE72E";
            }
        }
        public double dpiY = 1;
        private void MouseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {

            double y = WindowAPI.GetMousePosition().Y / dpiY;
            //consoleLog(dpiY);
            if (cache.enableTopmost)
            {

                double top = System.Windows.SystemParameters.PrimaryScreenHeight - (size + size / 2.5) - taskbar_g;

                if (y >= System.Windows.SystemParameters.PrimaryScreenHeight - 20)
                {

                    if (dockHidden)
                        Application.Current.Dispatcher.Invoke(() => ShowDock());
                }
                else
                {
                    if (y < System.Windows.SystemParameters.PrimaryScreenHeight - (System.Windows.SystemParameters.PrimaryScreenHeight - top))
                    {
                        if (WindowAPI.IsOnDesktop())
                        {
                            if (dockHidden)
                                Application.Current.Dispatcher.Invoke(() => ShowDock());
                        }
                        else
                        {
                            if (!dockHidden && cache.dockAutoHide)
                            {
                                Application.Current.Dispatcher.Invoke(() => HideDock());
                                hide_trigger = false;
                            }

                        }
                    }

                }
            }


        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (!cache.enableTopmost)
                WindowAPI.SendToBack(this);
        }

        private void SystemEvents_UserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
        {
            //Debug.WriteLine(2);
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            //Debug.WriteLine(1);
            //AutoWallUI();
        }

        // required constants.
        public const int SPI_SETDESKWALLPAPER = 20;
        public const int WM_SETTINGCHANGE = 0x001A;
        public const int WM_SYSCOMMAND = 0x0112;
        public const int WM_THEMECHANGED = 0x031A;
        public const int SC_MINIMIZE = 0xF020;
        public const int WM_WININICHANGE = 0x001A;

        // let's override WndProc...
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // if it's WM_SETTINGCHANGE
            if (msg == WM_SETTINGCHANGE)
            {
                AutoWallUI();
            }



            return IntPtr.Zero;
        }

        private void WallTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            AutoWallUI();
        }

        private void addIconToPanel(string path)
        {
            object icn = new object();
            Icon icon = icn as Icon;

            try
            {
                icon = GetSystemIcon(path);
            }
            catch (Exception ex)
            {
                icon = null;
                Debug.WriteLine(ex.Message + " - ошибка получения значка приложения");
            }

            if (icon != null)
            {
                Bitmap bitmap = icon.ToBitmap();


                DockIcon dockIcon = new DockIcon();

                dockIcon.Source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                dockIcon.MouseDown += Img_MouseDown;
                dockIcon.MouseEnter += Img_MouseEnter;
                dockIcon.MouseLeave += Img_MouseLeave;
                dockIcon.MouseMove += Img_MouseMove;
                dockIcon.MouseUp += Img_MouseUp;

                int index = MainPanel.Children.Count;
                MainPanel.Children.Insert(index, dockIcon);

                Task.Factory.StartNew(() =>
                {
                    System.Threading.Thread.Sleep(300);
                    Application.Current.Dispatcher.Invoke(() => UpdateWidthAndHighlight());
                });


            }
        }
        private void Img_MouseMoveDo(object sender)
        {
            DockIcon image = sender as DockIcon;

            Label label = tooltip.app_hint;


            label.Measure(new System.Windows.Size(Double.PositiveInfinity, Double.PositiveInfinity));

            label.Arrange(new Rect(label.DesiredSize));


            //SizeF size = tooltip.app_hint.CreateGraphics().MeasureString(myLabel.Text, myLabel.Font);
            double real_hint_width = label.ActualWidth;

            DockIcon uIElement = image;

            var element_Visual_Relative = uIElement.TransformToVisual((Visual)Content);

            System.Windows.Point offset = element_Visual_Relative.Transform(new System.Windows.Point(0, 0));
            var offsetX = offset.X;


            //consoleLog(offsetX);
            int addt = 0;
            string direction = getMouseDirection();
            if (direction == "left")
            {
                addt = 0;
            }
            double left = offsetX + (image.Size) / 2 - (real_hint_width / 2) + addt + 30 + 5;
            //if (left < 5) left = 5;
            double width = hint_width;

            animateHint(left, width, 0, 0, 1, 0);
        }
        private void Img_MouseMove(object sender, MouseEventArgs e)
        {
            Img_MouseMoveDo(sender);
        }
        private bool breakHintMove = false;
        private string lastTheme = string.Empty;
        private System.Drawing.Color ldominant = new System.Drawing.Color();


        private void ifAppRunned(DockIcon image)
        {
            if (!move_lock)
            {
                image.Highlight = true;
            }

        }
        private void ifNotAppRunned(DockIcon image)
        {
            if (!move_lock)
            {
                image.Highlight = false;
            }
        }
        private bool substrInStr(string substr, string str)
        {
            return str.IndexOf(substr) > -1;
        }
        private string appFromPath(string path)
        {
            string app_name = System.IO.Path.GetFileNameWithoutExtension(path);

            if (substrInStr("Microsoft Edge", app_name))
                app_name = "msedge";
            if (substrInStr("Explorer", app_name))
                app_name = "explorer";
            if (substrInStr("Проводник", app_name))
                app_name = "explorer";
            if (substrInStr("Chrome", app_name))
                app_name = "chrome";
            if (substrInStr("FL Studio", app_name))
                app_name = "fl64";
            if (substrInStr("Visual Studio Code", app_name))
                app_name = "vscode";
            if (substrInStr("Visual Studio", app_name) && substrInStr("Blend", app_name))
                app_name = "Blend";
            if (substrInStr("Visual Studio", app_name))
                app_name = "devenv";
            if (substrInStr("DAEMON Tools", app_name))
                app_name = "dtlite";
            if (substrInStr("Word", app_name))
                app_name = "winword";
            if (substrInStr("REAPER", app_name))
                app_name = "reaper";
            if (app_name == "Paint")
                app_name = "mspaint";
            if (substrInStr("Advanced SystemCare", app_name))
                app_name = "asc";
            if (app_name == "Start Zoom")
                app_name = "zoom";
            if (substrInStr("OBS Studio", app_name))
                app_name = "obs64";


            return app_name;
        }
        private bool CheckIfAppRunned(string path)
        {
            string app_path = getRealAppPath(path);
            string app_name = appFromPath(app_path);

            return System.Diagnostics.Process.GetProcessesByName(app_name).Length >= 1;
        }
        private void killProcess(string path)
        {
            string app_path = getRealAppPath(path);
            string app_name = appFromPath(app_path);

            System.Diagnostics.Process.GetProcessesByName(app_name)[0].Kill();
        }
        private bool dockHidden = false;
        private void AppsActiveLogic()
        {
            try
            {
                int i = 0;
                foreach (string path in cache.dock_apps_path)
                {

                    var already_runned = CheckIfAppRunned(path);

                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (already_runned)
                            {
                                if (i < MainPanel.Children.Count && i >= 0) ifAppRunned(MainPanel.Children[i] as DockIcon);
                            }
                            else
                            {
                                if (i < MainPanel.Children.Count && i >= 0) ifNotAppRunned(MainPanel.Children[i] as DockIcon);
                            }
                        });

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message + " beda #1");
                    }


                    i++;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " beda #2");
            }
        }

        public void ShowDock()
        {

            double top = System.Windows.SystemParameters.PrimaryScreenHeight - this.Height - taskbar_g;
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = top + this.Height + taskbar_g,
                To = top,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new PowerEase(),

            };
            myDoubleAnimation.Completed += (x, y) =>
            {
                lockSizeChange = false;
                dockHidden = false;
            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation, 60);
            this.BeginAnimation(Window.TopProperty, myDoubleAnimation);
            this.BeginAnimation(OpacityProperty, Animations.OpacityAnimation(this.Opacity, 1));


        }
        public void HideDock()
        {
            dockHidden = true;
            lockSizeChange = true;
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = this.Top,
                To = this.Top + this.Height + taskbar_g,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new PowerEase(),

            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation, 60);
            this.BeginAnimation(Window.TopProperty, myDoubleAnimation);
            this.BeginAnimation(OpacityProperty, Animations.OpacityAnimation(this.Opacity, 0));
        }
        private void MainTimer_Tick(object sender, EventArgs e)
        {

            int taskbar = 0;
            WindowAPI.TaskBarLocation location = WindowAPI.GetTaskBarLocation();
            if (location == WindowAPI.TaskBarLocation.BOTTOM)
            {
                taskbar = Application.Current.Dispatcher.Invoke(() => (int)(WpfScreen.GetScreenFrom(this).DeviceBounds.Bottom - WpfScreen.GetScreenFrom(this).WorkingArea.Bottom));
            }
            taskbar_g = taskbar;
            if (!move_lock)
            {

                Task.Factory.StartNew(() =>
                {
                    AppsActiveLogic();
                });

                try
                {
                    Application.Current.Dispatcher.Invoke(() => animateHChange(System.Windows.SystemParameters.PrimaryScreenHeight - this.Height, this.Height));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + " beda #3");
                }
            }





            try
            {
                if (startup_animation_completed && !lockSizeChange)
                    Application.Current.Dispatcher.Invoke(() => AutoTooltipAndDockPosition());
            }
            catch
            {
                consoleLog("AAAAAA BLET");
            }

        }

        public void AutoTooltipAndDockPosition()
        {
            if (!startup_animation_completed) return;
            double top = System.Windows.SystemParameters.PrimaryScreenHeight - this.Height - taskbar_g;
            //consoleLog(taskbar_g);
            tooltip.Top = top - tooltip.Height;

            DoubleAnimation fastda = new DoubleAnimation
            {
                From = this.Top,
                To = top,
                Duration = TimeSpan.FromMilliseconds(0)
            };
            if (!dockHidden)
                this.BeginAnimation(TopProperty, fastda);
        }
        public void AutoWallUI(bool upd = false)
        {
            Task.Factory.StartNew(() =>
            {
                var wpReg = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", false);
                var theme = wpReg.GetValue("SystemUsesLightTheme").ToString();
                wpReg.Close();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    List<DockIcon> combined = new List<DockIcon>();
                    foreach (DockIcon di in MainPanel.Children)
                    {
                        combined.Add(di);
                    }
                    foreach (DockIcon di in AIcons.Children)
                    {
                        combined.Add(di);
                    }
                    Animations.ThemeAnimate(theme, App_bg, tooltip, WhiteOverlay, BlackOverlay, combined);
                });
            });
        }


        public void consoleLog(object cdd)
        {
            Debug.WriteLine(cdd);
        }
        private static Icon GetSystemIcon(string path)
        {
            try
            {
                Win32E.SHFILEINFO psfi = new Win32E.SHFILEINFO();
                int dwFileAttributes = 2048;
                Win32E.SHGFI uFlags = Win32E.SHGFI.SHGFI_SYSICONINDEX;
                if (Win32E.SHGetFileInfo(path, dwFileAttributes, out psfi, (uint)Marshal.SizeOf((object)psfi), uFlags) == 0)
                    return (Icon)null;
                int i = psfi.iIcon;
                Guid riid = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
                Win32E.IImageList ppv;
                Win32E.SHGetImageList(4, ref riid, out ppv);
                IntPtr picon = IntPtr.Zero;
                int flags = 0;
                ppv.GetIcon(i, flags, ref picon);
                Icon icon = (Icon)System.Drawing.Icon.FromHandle(picon).Clone();
                Win32E.DestroyIcon(psfi.hIcon);
                return icon;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " beda #4");
            }
            return (Icon)null;
        }
        private bool move_lock = false;

        private void Main_Drop(object sender, DragEventArgs e)
        {
            if (cache.dockLock) return;
            isDrop = false;

            
            consoleLog(string.Join(", ", e.Data.GetFormats()));

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                try
                {
                    string[] s = (string[])e.Data.GetData(DataFormats.FileDrop);
                    

                    foreach (string fn in s)
                    {
                        string lname = System.IO.Path.GetFileNameWithoutExtension(fn);
                        if (cache.dock_apps_path.IndexOf(fn) == -1)
                        {
                            cache.dock_apps.Add(lname);
                            cache.dock_apps_path.Add(fn);
                            CacheOperations.StoreCache(cache);

                            addIconToPanel(fn);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + " beda #5");
                }
            }
        }


        private bool apprunned = false;
        /// <summary>
        /// Returns whether the given path/file is a link
        /// </summary>
        /// <param name="shortcutFilename"></param>
        /// <returns></returns>
        public static bool IsLink(string path)
        {
            /*
            try
            {
                string directory = System.IO.Path.GetDirectoryName(path);
                string file = System.IO.Path.GetFileName(path);

                Shell32.Shell shell = new Shell32.Shell();
                Shell32.Folder folder = shell.NameSpace(directory);
                Shell32.FolderItem folderItem = folder.ParseName(file);

                if (folderItem != null)
                {
                    return folderItem.IsLink;
                }

                return false;


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " beda #6" + ex.TargetSite);
            }
            */

            return false; // not found
        }

        public static string GetShortcutTarget(string shortcutFilename)
        {
            /*
            string pathOnly = System.IO.Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = System.IO.Path.GetFileName(shortcutFilename);

            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder folder = shell.NameSpace(pathOnly);
            Shell32.FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                if (folderItem.IsLink)
                {
                    try
                    {
                        Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
                        return link.Path;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message + " beda #7");
                    }

                }
                return shortcutFilename;
            }
            return string.Empty;  // not found
            */
            return shortcutFilename;
        }
        public string getRealAppPath(string path)
        {
            if (IsLink(path))
            {
                return GetShortcutTarget(path);
            }
            else
            {
                return path;
            }

        }
        private void Img_MouseUp(object sender, MouseButtonEventArgs e)
        {


            DockIcon img = sender as DockIcon;

            int current_index = MainPanel.Children.IndexOf(img);

            if (down_icon != null && isDown && !isDrop)
            {

                if (down_icon == img)
                {
                    Process[] processes = Process.GetProcesses();

                    string app_path = getRealAppPath(cache.dock_apps_path[current_index]);
                    string app_name = appFromPath(app_path);


                    var already_runned = System.Diagnostics.Process.GetProcessesByName(app_name).Count() >= 1;
                    MainPanel.Opacity = 1;
                    apprunned = true;
                    try
                    {

                        if (already_runned)
                        {


                            Process[] process = System.Diagnostics.Process.GetProcessesByName(app_name);

                            int proc_c = process.Length;

                            int real_windows = 0;

                            var allChildWindows = WindowAPI.EnumerateProcessWindowHandles(process.First().Id);

                            for (int i = 0; i < proc_c; i++)
                            {
                                Process proc = process[i];



                                if (proc.MainWindowHandle == IntPtr.Zero || app_name == "explorer")
                                {
                                    if (i == proc_c - 1 && real_windows == 0 || app_name == "explorer")
                                        System.Diagnostics.Process.Start(cache.dock_apps_path[current_index]);
                                }
                                else
                                {
                                    real_windows++;

                                    if (WindowAPI.IsIconic(proc.MainWindowHandle))
                                    {
                                        WindowAPI.SetForegroundWindow(proc.MainWindowHandle);
                                        WindowAPI.ShowWindowAsync(proc.MainWindowHandle, 9);

                                    }
                                    else
                                    {

                                        WindowAPI.ShowWindowAsync(proc.MainWindowHandle, WindowAPI.SW_MINIMIZE);

                                    }
                                }
                                // consoleLog(real_windows);


                            }




                        }
                        else
                        {
                            System.Diagnostics.Process.Start(cache.dock_apps_path[current_index]);
                        }

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message + " beda #8");
                    }
                }
                else
                {


                }
            }
            isDown = false;

            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = img.Opacity,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new SineEase(),

            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation, 30);
            img.BeginAnimation(DockIcon.OpacityProperty, myDoubleAnimation);
        }
        public void SetContextIcon(DockIcon img)
        {
            RemoveFromDockButton.Opacity = 1;
            RemoveFromDockButton.IsEnabled = true;

            string current_name = cache.dock_apps[MainPanel.Children.IndexOf(img)];
            string current_path = cache.dock_apps_path[MainPanel.Children.IndexOf(img)];

            CloseSomeAppButton.Header = "Close " + current_name;

            bool apprunned = CheckIfAppRunned(current_path);

            if (apprunned)
            {
                CloseSomeAppButton.IsEnabled = true;
            }
            else
            {
                CloseSomeAppButton.IsEnabled = false;
            }
            context_icon = img;
        }
        public class WinStates
        {
            public List<string> names = new List<string>();
            public List<int> states = new List<int>();

            public void Set(string name, int state)
            {
                if (!names.Contains(name))
                {
                    names.Add(name);
                    states.Add(state);

                }
                else
                {
                    states[names.IndexOf(name)] = state;
                }
            }
            public int Get(string name)
            {
                int index = names.IndexOf(name);
                if (index != -1)
                {
                    return states[index];
                }
                else
                {
                    return 1;
                }
            }
        }

        private bool isDown;
        private UIElement down_icon;
        private UIElement context_icon;
        private void Img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            apprunned = false;
            if (e.LeftButton == MouseButtonState.Pressed)
                isDown = true;


            DockIcon img = sender as DockIcon;

            if (e.RightButton == MouseButtonState.Pressed)
            {
                SetContextIcon(img);
            }


            down_icon = img;

            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = img.Opacity,
                To = 0.5,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new SineEase()
            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation, 30);
            img.BeginAnimation(DockIcon.OpacityProperty, myDoubleAnimation);



        }
        public static int defsize = 56;

        public int size = (int)(defsize * cache.scaleFactor);


        private void Img_MouseLeaveDo(object sender)
        {
            breakHintMove = true;
            isHovered = false;
        }
        private void Img_MouseLeave(object sender, MouseEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() => Img_MouseLeaveDo(sender));
            });
        }
        private void animateSizeChange(int start, int end, DockIcon e, double dur = 0.1)
        {
            /*
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = start,
                To = end,
                Duration = TimeSpan.FromSeconds(dur),
                EasingFunction = new QuadraticEase {EasingMode = EasingMode.EaseInOut }
            };
            */

            //Timeline.SetDesiredFrameRate(myDoubleAnimation, 60);
            //e.BeginAnimation(DockIcon.WidthProperty, myDoubleAnimation, HandoffBehavior.Compose);
            //.BeginAnimation(DockIcon.HeightProperty, myDoubleAnimation, HandoffBehavior.Compose);

            e.Size = end;
        }
        public bool lockSizeChange = false;
        public void animateWChange(int start, int end, Window e)
        {
            if (lockSizeChange) return;
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = start,
                To = end,
                Duration = TimeSpan.FromSeconds(0),
            };
            e.BeginAnimation(Window.WidthProperty, myDoubleAnimation);
        }
        public void animateHChange(double top, double height)
        {
            if (lockSizeChange) return;
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = this.Top,
                To = top,
                Duration = TimeSpan.FromSeconds(0)
            };
            DoubleAnimation myDoubleAnimation2 = new DoubleAnimation
            {
                From = this.Height,
                To = height,
                Duration = TimeSpan.FromSeconds(0)
            };
            if (startup_animation_completed)
            {
                this.BeginAnimation(Window.TopProperty, myDoubleAnimation);
            }

            this.BeginAnimation(Window.HeightProperty, myDoubleAnimation2);

        }
        private double hint_width = 0;
        private void animateHint(double left_pos, double width, int index, double top_pos, int opacity, double dur = 0.1, bool breakdis = false)
        {

            DoubleAnimation myDoubleAnimation2 = new DoubleAnimation
            {
                From = tooltip.app_hint.Opacity,
                To = opacity,
                Duration = TimeSpan.FromSeconds(dur),
                EasingFunction = new SineEase()
            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation2, 100);
            ThicknessAnimation thicknessAnimation = new ThicknessAnimation
            {
                From = tooltip.app_hint.Margin,
                To = new Thickness(left_pos, top_pos, 0, 0),
                Duration = TimeSpan.FromSeconds(dur),
                EasingFunction = new SineEase()
            };

            thicknessAnimation.Completed += (x, y) =>
            {
                if (breakdis)
                {
                    breakHintMove = false;
                }
            };
            Timeline.SetDesiredFrameRate(thicknessAnimation, 100);

            tooltip.app_hint.BeginAnimation(Window.MarginProperty, thicknessAnimation);
            //tooltip.app_hint.BeginAnimation(Window.OpacityProperty, myDoubleAnimation2);

        }
        private double oldX = 0;
        private double mouseSpeed = 0;
        private string getMouseDirection()
        {
            string xDirection = string.Empty;

            if (oldX < System.Windows.Forms.Cursor.Position.X - this.Left)
            {
                xDirection = "right";
            }
            else
            {
                xDirection = "left";
            }
            mouseSpeed = System.Windows.Forms.Cursor.Position.X - this.Left - oldX;
            if (xDirection == "left") mouseSpeed = -mouseSpeed;
            oldX = System.Windows.Forms.Cursor.Position.X - this.Left;
            return xDirection;
        }
        private bool isHovered = false;
        private void Img_MouseEnterDo(object sender)
        {
            /*
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = Icons_highlights.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.1),
                EasingFunction = new SineEase()
            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation, 30);
            Icons_highlights.BeginAnimation(DockIcon.OpacityProperty, myDoubleAnimation);
            */
            if (!move_lock)
            {


                DockIcon img = sender as DockIcon;

                IEnumerator enumerator = MainPanel.Children.GetEnumerator();
                enumerator.Reset();



                int current_index = MainPanel.Children.IndexOf(img);

                string current_label = string.Empty;

                if (current_index != -1)
                {
                    current_label = cache.dock_apps[current_index];
                }
                else
                {
                    current_label = img.Label;
                }


                if ((string)tooltip.app_hint.Content != current_label)
                {
                    int speed = 0;
                    speed = System.Windows.Forms.SystemInformation.MouseSpeed;

                    bool break_an = false;
                    if (mouseSpeed > 13) break_an = true;
                    //Debug.WriteLine(mouseSpeed);

                    tooltip.app_hint.Content = current_label;
                    DoubleAnimation opacityAnimation = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(100),
                        EasingFunction = new SineEase()
                    };
                    if (!break_an)
                        tooltip.app_hint.BeginAnimation(OpacityProperty, opacityAnimation);

                    DoubleAnimation scaleAnimation = new DoubleAnimation
                    {
                        From = 0.6,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(200),
                        EasingFunction = new SineEase()
                    };
                    //if (!break_an)
                    //    tooltip.hintScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                    //if (!break_an)
                    //    tooltip.hintScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);

                    DoubleAnimation transYAnimation = new DoubleAnimation
                    {
                        From = 20,
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(200),
                        EasingFunction = new SineEase()
                    };
                    //if (!break_an)
                    //    tooltip.hintTrans.BeginAnimation(TranslateTransform.YProperty, transYAnimation);


                }

                tooltip.app_hint.Visibility = Visibility.Visible;

                if (current_index != -1)
                {
                    context_icon = img;
                }
                //tooltip.app_hint.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 40, 40, 40));


                int single_icons_count = current_index - 2;
                if (single_icons_count < 0) single_icons_count = 0;
                int single_icons_size = single_icons_count * size;

                int low_icons_count = 0;
                if (current_index - 2 > 0)
                {
                    low_icons_count = 1;
                }
                double low_icons_size = low_icons_count * (size * 1.05);

                int mid_icons_count = 0;
                if (current_index - 2 >= 0)
                {
                    mid_icons_count = 1;
                }
                double mid_icons_size = mid_icons_count * (size * 1.1);

                double big_icon_size = size * 1.2;
                double l_size = single_icons_size + low_icons_size + mid_icons_size + big_icon_size;

                //consoleLog(single_icons_count + "-" + low_icons_count + "-" + mid_icons_count + "-" + 1);

                Label label = tooltip.app_hint;


                label.Measure(new System.Windows.Size(Double.PositiveInfinity, Double.PositiveInfinity));

                label.Arrange(new Rect(label.DesiredSize));


                //SizeF size = tooltip.app_hint.CreateGraphics().MeasureString(myLabel.Text, myLabel.Font);
                double real_hint_width = label.ActualWidth;

                DockIcon uIElement = img;

                var element_Visual_Relative = uIElement.TransformToVisual((Visual)Content);

                System.Windows.Point offset = element_Visual_Relative.Transform(new System.Windows.Point(0, 0));
                var offsetX = offset.X;


                //consoleLog(offsetX);
                int addt = 0;
                string direction = getMouseDirection();
                if (direction == "left")
                {
                    addt = 0;
                }
                double left = offsetX + (img.Size) / 2 - (real_hint_width / 2) + addt + 30 + 5;
                //if (left < 5) left = 5;
                double width = hint_width;

                //consoleLog(real_hint_width);

                //if (left > (this.Width - real_hint_width - 5)) left = this.Width - real_hint_width - 5;
                breakHintMove = false;
                animateHint(left, width, 0, 0, 1, 0);

            }
        }
        private void Img_MouseEnter(object sender, MouseEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                isHovered = true;
                Application.Current.Dispatcher.Invoke(() => Img_MouseEnterDo(sender));
            });

        }
        private bool isDrop = false;
        private void Main_Drop_Enter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if(!cache.dockLock)
                {
                    e.Effects = DragDropEffects.Copy;
                    isDrop = true;
                } else
                {
                    e.Effects = DragDropEffects.None;
                }
                
            }
        }
        private void ExitDock()
        {
            mainTimer.Stop();
            mouseTimer.Stop();
            move_lock = true;
            this.IsEnabled = false;
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = this.Top,
                To = this.Top + this.Height + taskbar_g,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new PowerEase(),

            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation, 60);
            myDoubleAnimation.Completed += MyDoubleAnimation_Completed1;
            myDoubleAnimation.RemoveRequested += MyDoubleAnimation_Completed1;
            tooltip.Close();
            settings.Close();
            lockSizeChange = true;
            this.BeginAnimation(Window.TopProperty, myDoubleAnimation);
            this.BeginAnimation(OpacityProperty, Animations.OpacityAnimation(this.Opacity, 0));
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExitDock();
        }

        private void MyDoubleAnimation_Completed1(object sender, EventArgs e)
        {
            Close();
            Environment.Exit(0);
        }

        private bool dactive = false;
        private System.Timers.Timer hide_timer;
        private bool hide_trigger = false;
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (hide_timer == null)
            {
                hide_timer = new System.Timers.Timer();
                hide_timer.Interval = 3000;
                hide_timer.Elapsed += (xx, yx) =>
                {
                    hide_timer.Stop();
                    hide_trigger = true;
                    hide_timer = null;
                };
                hide_timer.Start();
            }
            panelIconsAnimated = false;

            Draggable_icon.Source = null;
            /*
            DoubleAnimation myDoubleAnimation3 = new DoubleAnimation
            {
                From = Icons_highlights.Opacity,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.1),
                EasingFunction = new SineEase()
            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation3, 30);
            Icons_highlights.BeginAnimation(DockIcon.OpacityProperty, myDoubleAnimation3);
            */

            dactive = true;
            if (!move_lock)
            {





                List<DockIcon> combined = new List<DockIcon>();
                foreach (DockIcon di in MainPanel.Children)
                {
                    combined.Add(di);
                }
                foreach (DockIcon di in AIcons.Children)
                {
                    combined.Add(di);
                }



                foreach (DockIcon img_cur in combined)
                {

                    if (img_cur != null)
                    {
                        DoubleAnimation myDoubleAnimation = new DoubleAnimation
                        {
                            From = img_cur.Opacity,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.2),
                            EasingFunction = new SineEase()
                        };
                        Timeline.SetDesiredFrameRate(myDoubleAnimation, 30);
                        img_cur.BeginAnimation(DockIcon.OpacityProperty, myDoubleAnimation);
                        DoubleAnimation doubleAnimation = new DoubleAnimation
                        {
                            From = img_cur.Size,
                            To = size,
                            Duration = TimeSpan.FromMilliseconds(200),
                            EasingFunction = new SineEase()
                        };
                        doubleAnimation.Completed += (x, y) =>
                        {
                            tooltip.Hide();
                        };
                        img_cur.BeginAnimation(DockIcon.SizeProperty, doubleAnimation);
                    }



                }
            }
            if (down_icon != null && isDown && !apprunned && !cache.dockLock)
            {
                DockIcon dimg = down_icon as DockIcon;
                RemoveFromDock(dimg);
            }
            tooltip.app_hint.Content = string.Empty;


            //tooltip.app_hint.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 25, 25, 25));
            animateHint(tooltip.app_hint.Margin.Left - 100, tooltip.app_hint.Width, -1, 0, 0);
            tooltip.app_hint.Visibility = Visibility.Hidden;


            IEnumerator enumerator = MainPanel.Children.GetEnumerator();
            enumerator.Reset();
            for (int i = 0; i < MainPanel.Children.Count; i++)
            {

                enumerator.MoveNext();
                DockIcon img_cur = enumerator.Current as DockIcon;


                if (img_cur != null)
                {
                    animateSizeChange((int)img_cur.Size, (int)(size), img_cur);
                }

            }


        }
        public void UpdateWidthAndHighlight()
        {
            int summary_icons_count = (MainPanel.Children.Count + AIcons.Children.Count);

            double summary_icons_width = summary_icons_count * size;
            double summary_icons_margin = summary_icons_count * 10;
            double separator_size_and_margin = 2 + 20;
            double free_space = size / 2;

            double addt = 0;

            if (size < 56) addt = size / 2;
            if (size < 53) addt = size;

            consoleLog(size);

            double new_width = summary_icons_width + summary_icons_margin + separator_size_and_margin + free_space + addt;
            //consoleLog(new_width);
            if (new_width < 100) new_width = 100;
            this.Width = new_width;
        }
        private void RemoveFromDock(DockIcon image)
        {
            if (dialog == null) dialog = new Dialog("Are you sure you want to delete this item?");
            if (dialog != null)
            {
                DoubleAnimation opacityAnimation = new DoubleAnimation
                {
                    From = image.Opacity,
                    To = 0.2,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new BackEase()
                };
                MainPanel.Opacity = 1;
                Timeline.SetDesiredFrameRate(opacityAnimation, 30);
                image.BeginAnimation(StackPanel.OpacityProperty, opacityAnimation);

                try
                {
                    dialog.Show();
                    dialog.onResult += () =>
                    {
                        if (dialog != null)
                        {
                            if (dialog.result == true)
                            {
                                int dindex = MainPanel.Children.IndexOf(image);

                                try
                                {
                                    if (dindex < cache.dock_apps.Count)
                                        cache.dock_apps.RemoveAt(dindex);
                                    if (dindex < cache.dock_apps_path.Count)
                                        cache.dock_apps_path.RemoveAt(dindex);

                                    CacheOperations.StoreCache(cache);

                                    MainPanel.Children.Remove(image);
                                    /*
                                    Icons_highlights.Children.RemoveAt(dindex);
                                    */




                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.Message + " beda #13");
                                }

                                Task.Factory.StartNew(() =>
                                {
                                    System.Threading.Thread.Sleep(300);
                                    Application.Current.Dispatcher.Invoke(() => UpdateWidthAndHighlight());
                                });

                            }
                            else
                            {
                                DoubleAnimation opacityAnimation2 = new DoubleAnimation
                                {
                                    From = image.Opacity,
                                    To = 1,
                                    Duration = TimeSpan.FromSeconds(0.3),
                                    EasingFunction = new BackEase()
                                };
                                Timeline.SetDesiredFrameRate(opacityAnimation2, 30);
                                image.BeginAnimation(StackPanel.OpacityProperty, opacityAnimation2);
                            }
                            isDown = false;
                        }
                        if (dialog != null)
                        {
                            dialog.CloseDialog();
                        }

                        dialog = null;
                    };
                }
                catch
                {
                    Debug.WriteLine("Пздц");
                }

            }

        }
        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            isDrop = false;
        }





        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {

        }
        private bool startup_animation_completed = false;
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            consoleLog(1);
            double left = SystemParameters.PrimaryScreenWidth / 2 - e.NewSize.Width / 2;
            this.Left = left;
            tooltip.Left = left - 30;
            tooltip.Width = e.NewSize.Width + 60;

            double new_h = size + size / 2.5;
            double top = System.Windows.SystemParameters.PrimaryScreenHeight - new_h - taskbar_g;
            if (startup_animation_completed)
            {
                DoubleAnimation fastda = new DoubleAnimation
                {
                    From = this.Top,
                    To = top,
                    Duration = TimeSpan.FromMilliseconds(0)
                };
                this.BeginAnimation(TopProperty, fastda);
            }

            tooltip.Top = top - tooltip.Height;

        }

        private void MyDoubleAnimation_Completed(object sender, EventArgs e)
        {
            startup_animation_completed = true;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                settings.Show();
                settings.Activate();
                cache = CacheOperations.LoadCache(cache);
                settings.StartupToggle.IsChecked = cache.runAtStartup;
                settings.DisableBlurToggle.IsChecked = cache.disableBlur;
                settings.StarDustEnableToggle.IsChecked = cache.enableStarDust;
                settings.EnableTopmostToggle.IsChecked = cache.enableTopmost;
                settings.AHToggle.IsChecked = cache.dockAutoHide;
                settings.Trans_bar.Value = cache.bg_trans;
                settings.ScaleSlider.Value = cache.scaleFactor;

                settings.Toggle_Loaded_Do(settings.StartupToggle);
                settings.Toggle_Loaded_Do(settings.DisableBlurToggle);
                settings.Toggle_Loaded_Do(settings.StarDustEnableToggle);
            }
            catch
            {
                settings = new Settings();
                settings.Show();
                settings.Activate();
                cache = CacheOperations.LoadCache(cache);
                settings.window = this;
                settings.StartupToggle.IsChecked = cache.runAtStartup;
                settings.DisableBlurToggle.IsChecked = cache.disableBlur;
                settings.StarDustEnableToggle.IsChecked = cache.enableStarDust;
                settings.EnableTopmostToggle.IsChecked = cache.enableTopmost;
                settings.AHToggle.IsChecked = cache.dockAutoHide;
                settings.Trans_bar.Value = cache.bg_trans;
                settings.ScaleSlider.Value = cache.scaleFactor;

                settings.Toggle_Loaded_Do(settings.StartupToggle);
                settings.Toggle_Loaded_Do(settings.DisableBlurToggle);
                settings.Toggle_Loaded_Do(settings.StarDustEnableToggle);
            }

        }

        private void RemoveFromDockButton_Click(object sender, RoutedEventArgs e)
        {
            DockIcon context_img = context_icon as DockIcon;

            if (context_img != null)
            {
                RemoveFromDock(context_img);
            }
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            mainTimer.Stop();
            mouseTimer.Stop();
            move_lock = true;
            this.IsEnabled = false;
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = this.Top,
                To = this.Top + this.Height + taskbar_g,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new PowerEase(),

            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation, 60);
            myDoubleAnimation.Completed += (s, es) =>
            {
                System.Windows.Forms.Application.Restart();
                System.Windows.Application.Current.Shutdown();
            };
            myDoubleAnimation.RemoveRequested += (s, es) =>
            {
                System.Windows.Forms.Application.Restart();
                System.Windows.Application.Current.Shutdown();
            };

            tooltip.Close();
            settings.Close();
            this.BeginAnimation(Window.TopProperty, myDoubleAnimation);
            this.BeginAnimation(OpacityProperty, Animations.OpacityAnimation(this.Opacity, 0));

        }

        private bool AbsIconDrag = false;
        private void Draggable_icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                AbsIconDrag = true;
            move_lock = true;
        }
        private bool hitTest(UIElement el1, UIElement el2, MouseEventArgs e)
        {
            // Retrieve the coordinate of the mouse position.
            System.Windows.Point pt = e.GetPosition(el1);

            // Perform the hit test against a given portion of the visual object tree.
            HitTestResult result = VisualTreeHelper.HitTest(el2, pt);

            if (result != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private List<DockIcon> MoveImg(int item_index, int ditem_index, List<DockIcon> images)
        {
            List<DockIcon> left = new List<DockIcon>();
            List<DockIcon> right = new List<DockIcon>();

            int i = 0;
            right.Add(images[item_index]);
            images.Remove(images[item_index]);

            foreach (DockIcon img in images)
            {
                if (i >= ditem_index)
                {
                    right.Add(img);
                }
                else
                {
                    left.Add(img);
                }
                i++;
            }

            images.Clear();
            images.AddRange(left);
            images.AddRange(right);

            return images;
        }

        private List<string> MoveString(int item_index, int ditem_index, List<string> elements)
        {
            List<string> left = new List<string>();
            List<string> right = new List<string>();

            int i = 0;
            right.Add(elements[item_index]);
            elements.RemoveAt(item_index);

            foreach (string el in elements)
            {
                if (i >= ditem_index)
                {
                    right.Add(el);
                }
                else
                {
                    left.Add(el);
                }
                i++;
            }

            elements = new List<string>();
            elements.AddRange(left);
            elements.AddRange(right);

            return elements;
        }
        private void Draggable_icon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AbsIconDrag = false;
            isDown = false;

            List<DockIcon> allElements = new List<DockIcon>();
            List<DockIcon> oAllElements = new List<DockIcon>();
            foreach (UIElement cur in MainPanel.Children)
            {
                DockIcon ci = cur as DockIcon;
                if (ci != null)
                {
                    allElements.Add(ci);
                    oAllElements.Add(ci);
                }

            }
            bool lock_cycle = false;
            foreach (DockIcon cur in allElements)
            {
                if (hitTest(cur, Draggable_icon, e) && !lock_cycle)
                {
                    int cur_index = MainPanel.Children.IndexOf(cur);
                    int down_index = MainPanel.Children.IndexOf(down_icon);

                    if (cur_index != lastMindex)
                    {

                        if (cur_index != -1)
                        {
                            oAllElements = MoveImg(down_index, cur_index, oAllElements);
                            cache.dock_apps = MoveString(down_index, cur_index, cache.dock_apps);
                            cache.dock_apps_path = MoveString(down_index, cur_index, cache.dock_apps_path);
                            CacheOperations.StoreCache(cache);

                            MainPanel.Opacity = 1;

                            foreach (DockIcon img in allElements)
                            {
                                MainPanel.Children.Remove(img);
                            }

                            foreach (DockIcon img in oAllElements)
                            {
                                MainPanel.Children.Add(img);
                            }
                            DoubleAnimation myDoubleAnimation1 = new DoubleAnimation
                            {
                                From = Draggable_icon.Opacity,
                                To = 0,
                                Duration = TimeSpan.FromSeconds(0.3),
                                EasingFunction = new SineEase()
                            };
                            Timeline.SetDesiredFrameRate(myDoubleAnimation1, 30);
                            myDoubleAnimation1.Completed += (a, es) =>
                            {
                                Draggable_icon.Source = null;
                            };
                            Draggable_icon.BeginAnimation(DockIcon.OpacityProperty, myDoubleAnimation1);
                            lock_cycle = true;
                            Task.Factory.StartNew(() =>
                            {
                                System.Threading.Thread.Sleep(100);
                                move_lock = false;
                            });

                        }
                    }
                }
            }

        }
        private int lastMindex = -1;
        private DockIcon CloneIcon(DockIcon source)
        {
            DockIcon cloneimg = new DockIcon();
            cloneimg.Source = source.Source;
            cloneimg.Size = size;
            cloneimg.Height = size;
            cloneimg.MouseEnter += Img_MouseEnter;
            cloneimg.MouseLeave += Img_MouseLeave;
            cloneimg.MouseDown += Img_MouseDown;
            cloneimg.MouseUp += Img_MouseUp;
            cloneimg.MouseMove += Img_MouseMove;

            return cloneimg;
        }
        private bool Draggable_icon_an = true;

        private double fe_max_size = 0;
        private int fe_max_size_el = 0;
        private void fishEyeForIcons(double x)
        {
            List<DockIcon> combined = new List<DockIcon>();
            foreach (DockIcon di in MainPanel.Children)
            {
                combined.Add(di);
            }
            foreach (DockIcon di in AIcons.Children)
            {
                combined.Add(di);
            }
            double width = (combined.Count) * 80;
            if (width < 300) width = 300;
            double[] big_array = new double[(int)width];


            int end = 300;
            for (int i = 0; i < end; i++)
            {
                double m_val = 0;
                if (i < end / 2)
                {
                    m_val = i;
                }
                else
                {
                    m_val = end - (i);
                }

                int index = (int)(i + width * x - end / 2);

                if (index < width && index >= 0)
                {
                    big_array[index] = m_val / 3;
                }
            }

            double[] single_array = new double[(int)combined.Count];

            if (!panelIconsAnimating)
            {
                for (int i = 0; i < combined.Count; i++)
                {
                    int m = (int)(width / combined.Count) * (i + 1) - 20;
                    if (m >= big_array.Length)
                        m = big_array.Length - 1;
                    if (m < 0) m = 0;
                    single_array[i] = big_array[m];

                    DockIcon image = combined[i] as DockIcon;
                    double newsize = size * (big_array[m] / 50 / 5 + 1);

                    if (newsize >= fe_max_size - 1)
                    {
                        fe_max_size = newsize;


                        if (!isHovered && fe_max_size_el != i)
                        {
                            Img_MouseEnterDo(combined[i]);
                            if (i < MainPanel.Children.Count)
                            {
                                SetContextIcon((DockIcon)MainPanel.Children[i]);
                                context_icon = MainPanel.Children[i];
                            }


                            fe_max_size_el = i;
                        }
                        else
                        {
                            if (!isHovered)
                            {
                                Img_MouseMoveDo(combined[fe_max_size_el]);
                                if (i < MainPanel.Children.Count)
                                {
                                    context_icon = MainPanel.Children[fe_max_size_el];
                                }
                            }
                        }
                    }

                    if (!panelIconsAnimated)
                    {

                        DoubleAnimation doubleAnimation = new DoubleAnimation
                        {
                            From = image.Size,
                            To = newsize,
                            Duration = TimeSpan.FromMilliseconds(100),
                            EasingFunction = new SineEase()
                        };
                        doubleAnimation.Completed += (a, e) =>
                        {
                            panelIconsAnimated = true;
                            panelIconsAnimating = false;

                        };
                        if (image.Size != newsize)
                        {
                            image.BeginAnimation(DockIcon.SizeProperty, doubleAnimation);
                        }

                        panelIconsAnimating = true;
                    }
                    else
                    {
                        DoubleAnimation doubleAnimation = new DoubleAnimation
                        {
                            From = image.Size,
                            To = newsize,
                            Duration = TimeSpan.FromMilliseconds(0),
                            EasingFunction = new SineEase()
                        };
                        if (image.Size != newsize)
                        {
                            image.BeginAnimation(DockIcon.SizeProperty, doubleAnimation);
                        }
                    }
                }


            }

            //consoleLog(String.Join(", ", single_array));
        }
        private bool panelIconsAnimated = false;
        private bool panelIconsAnimating = false;
        private void Draggable_icon_MouseMove(object sender, MouseEventArgs e)
        {
            double gl_x = e.GetPosition(DockMain).X;
            double x = gl_x - Draggable_icon.Width / 2;
            double y = e.GetPosition(DockMain).Y - Draggable_icon.Height / 2;
            fishEyeForIcons(gl_x / DockMain.Width);


            if ((isDown || AbsIconDrag))
            {

                if (!cache.dockLock)
                {
                    DockIcon dicon = down_icon as DockIcon;

                    Draggable_icon.Source = dicon.Source;




                    Draggable_icon.Margin = new Thickness(x, y, 0, 0);

                    MainPanel.Opacity = .8;
                    if (Draggable_icon_an)
                    {
                        DoubleAnimation myDoubleAnimation1 = new DoubleAnimation
                        {
                            From = Draggable_icon.Opacity,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.1),
                            EasingFunction = new SineEase()
                        };
                        Draggable_icon_an = false;
                        Timeline.SetDesiredFrameRate(myDoubleAnimation1, 30);
                        Draggable_icon.BeginAnimation(DockIcon.OpacityProperty, myDoubleAnimation1);
                    }
                }

            }
            else
            {
                if (!cache.dockLock)
                {
                    DoubleAnimation myDoubleAnimation1 = new DoubleAnimation
                    {
                        From = Draggable_icon.Opacity,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.1),
                        EasingFunction = new SineEase()
                    };
                    myDoubleAnimation1.Completed += (a, es) =>
                    {
                        Draggable_icon.Source = null;
                        Draggable_icon_an = true;



                    };
                    Timeline.SetDesiredFrameRate(myDoubleAnimation1, 30);
                    Draggable_icon.BeginAnimation(DockIcon.OpacityProperty, myDoubleAnimation1);
                }

            }
            try
            {
                if (!lockSizeChange)
                    tooltip.Show();
            }
            catch
            {
                consoleLog("Tooltip show error");
            }


        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (context_icon == null)
                {
                    RemoveFromDockButton.Opacity = .5;
                    RemoveFromDockButton.IsEnabled = false;
                }
            }
        }

        private void OpenNewButton_Click(object sender, RoutedEventArgs e)
        {
            DockIcon context_img = context_icon as DockIcon;

            if (context_img != null)
            {
                int current_index = MainPanel.Children.IndexOf(context_img);
                Process.Start(cache.dock_apps_path[current_index]);
            }
        }

        private void CloseSomeAppButton_Click(object sender, RoutedEventArgs e)
        {
            string current_name = cache.dock_apps[MainPanel.Children.IndexOf(context_icon)];
            if (dialog == null) dialog = new Dialog("Are you sure you want to close " + current_name + "?");
            if (dialog != null)
            {
                try
                {
                    dialog.Show();
                    dialog.onResult += () =>
                    {
                        if (dialog != null)
                        {
                            if (dialog.result == true)
                            {
                                string current_path = cache.dock_apps_path[MainPanel.Children.IndexOf(context_icon)];


                                bool apprunned = CheckIfAppRunned(current_path);

                                if (apprunned)
                                {
                                    killProcess(current_path);
                                }
                            }
                            dialog.CloseDialog();
                        }

                        dialog = null;
                    };
                }
                catch
                {

                }
            }
        }

        private void DockIcon_Click(object sender, RoutedEventArgs e)
        {
            DockIcon icon = sender as DockIcon;


        }

        private void DockMain_Activated(object sender, EventArgs e)
        {
            if (!cache.enableTopmost)
                WindowAPI.SendToBack(this);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void LockDockButton_Click(object sender, RoutedEventArgs e)
        {
            cache = CacheOperations.LoadCache(cache);
            cache.dockLock = !cache.dockLock;
            DockLockUpdateUI();
            CacheOperations.StoreCache(cache);
        }
    }
}
