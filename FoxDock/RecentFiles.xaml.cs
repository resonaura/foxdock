using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace FoxDock
{
    /// <summary>
    /// Логика взаимодействия для RecentApps.xaml
    /// </summary>
    public partial class RecentFiles : Window
    {
        public static AppLanguage.Locale locale = AppLanguage.GetSystemLocale();
        private void AddNew(string name, BitmapSource source, string path)
        {
            Grid grid = new Grid();
            ColumnDefinition c1 = new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            };
            ColumnDefinition c2 = new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Auto)
            };
            grid.ColumnDefinitions.Add(c2);
            grid.ColumnDefinitions.Add(c1);
            Label label = new Label();
            Image image = new Image
            {
                Source = source,
                Height = 64,
                Width = 64
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality); 
            image.SetValue(Grid.ColumnProperty, 0);
            label.SetValue(Grid.ColumnProperty, 1);
            label.HorizontalAlignment = HorizontalAlignment.Left;
            grid.Children.Add(image);

            Brush lfg = new SolidColorBrush();
            Brush lbg = new SolidColorBrush();
            string theme = Win32API.GetSysTheme();
            int hint_opacity = (int)(Dock.cache.hm_trans * 255);

            switch (Dock.cache.hintBackground)
            {
                case HintBackground.Auto:
                    if (theme == "0")
                    {
                        lbg = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 24, 24, 24));
                        lfg = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    }
                    if (theme == "1")
                    {
                        lbg = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 255, 255, 255));
                        lfg = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                    }
                    break;
                case HintBackground.Black:
                    lbg = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 24, 24, 24));
                    lfg = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    break;
                case HintBackground.Gray:
                    lbg = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 48, 48, 48));
                    lfg = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    break;
                case HintBackground.White:
                    lbg = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 255, 255, 255));
                    lfg = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                    break;
                case HintBackground.Accent:
                    lbg = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, SystemParameters.WindowGlassColor.R, SystemParameters.WindowGlassColor.G, SystemParameters.WindowGlassColor.B));
                    lfg = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    break;
            }
            grid.Cursor = Cursors.Hand;
            grid.Height = 64;
            label.Background = lbg;
            label.Foreground = lfg;
            label.Content = name;
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            label.VerticalContentAlignment = VerticalAlignment.Center;
            label.Height = 25;
            label.Padding = new Thickness(10, 5, 10, 5);
            label.Margin = new Thickness(10, 0, 0, 0);
            label.Style = (Style)Resources["CoolLabel"];
            label.MaxWidth = 600;
            grid.Children.Add(label);

            TransformGroup transformGroup = new TransformGroup();

            RotateTransform rotateTransform = new RotateTransform();

            TranslateTransform translateTransform = new TranslateTransform();

            transformGroup.Children.Add(rotateTransform);
            transformGroup.Children.Add(translateTransform);

            grid.RenderTransform = transformGroup;
            grid.RenderTransformOrigin = new Point(1, 1);
            grid.Margin = new Thickness(5);
            grid.Background = new SolidColorBrush(Color.FromArgb(1, 255, 255, 255));

            grid.MouseDown += (x, y) =>
            {
                (x as Grid).BeginAnimation(Grid.OpacityProperty, Animations.OpacityAnimation(.8, .4, 0.3));
            };
            grid.MouseUp += (x, y) =>
            {
                (x as Grid).BeginAnimation(Grid.OpacityProperty, Animations.OpacityAnimation(.4, .8, 0.3));
                Process.Start(path);
                window.RecentIcon.Highlight = false;
                CloseApp();
            };
            grid.MouseEnter += (x, y) =>
            {
                foreach(Grid g in container.Children)
                {
                    g.BeginAnimation(Grid.OpacityProperty, Animations.OpacityAnimation(g.Opacity, .5, .3));
                }
                (x as Grid).BeginAnimation(Grid.OpacityProperty, Animations.OpacityAnimation((x as Grid).Opacity, 1, 0.3));
            };
            this.MouseLeave += (x, y) =>
            {
                foreach (Grid g in container.Children)
                {
                    g.BeginAnimation(Grid.OpacityProperty, Animations.OpacityAnimation(g.Opacity, 1, 0.3));
                }
            };
            container.Children.Insert(0, grid);

        }
        public bool IsClosed { get; private set; }

        

        public void CloseApp()
        {
            TranslateTransform offsetTransform = new TranslateTransform();
            DoubleAnimation anim = new DoubleAnimation
            {
                From = 0,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuarticEase()
            };
            anim.Completed += (x,y) =>
            {
                IsClosed = true;
                window.RecentIcon.Highlight = false;
                this.Close();
            };
            offsetTransform.BeginAnimation(TranslateTransform.YProperty, anim);
            MainD.RenderTransform = offsetTransform;
            MainD.BeginAnimation(Grid.OpacityProperty, Animations.OpacityAnimation(1, 0, .2));

            
            
        }
        private readonly Dock window;
        public RecentFiles(Dock w)
        {
            InitializeComponent();
            this.Title = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.RecentFiles, locale);
            this.Top = -1000;
            this.Left = -1000;
            Dock.cache = CacheOperations.LoadCache(Dock.cache);
            window = w;
            //WindowAPI.SendToTop(w);

            double black_opacity = 0;
            double white_opacity = 0;
            double accent_opacity = 0;

            string theme = Win32API.GetSysTheme();
            switch (Dock.cache.background)
            {
                case DockBackground.Auto:
                    if (theme == "0")
                    {
                        black_opacity = 1;
                    }
                    if (theme == "1")
                    {
                        white_opacity = 1;
                    }

                    break;
                case DockBackground.Black:
                    black_opacity = 1;
                    break;
                case DockBackground.White:
                    white_opacity = 1;
                    break;
                case DockBackground.Gray:
                    black_opacity = 0.7;
                    white_opacity = 0.3;
                    break;
                case DockBackground.Accent:
                    accent_opacity = 1;
                    break;

            }
            WhiteOverlay.Opacity = white_opacity;
            BlackOverlay.Opacity = black_opacity;
            AccentOverlay.Opacity = accent_opacity;
            Overlays.Opacity = Dock.cache.bg_trans;

            IsClosed = false;
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
            DirectoryInfo d = new DirectoryInfo(path);
            IOrderedEnumerable<FileInfo> Files = d.GetFiles().OrderByDescending(f => f.LastWriteTime);

            int limit = 6;
            int x = 0;
            foreach (FileInfo file in Files)
            {
                if (x < limit)
                {
                    string truepath = Win32API.GetRealAppPath(file.FullName);
                    if(File.Exists(truepath) || Directory.Exists(truepath))
                    {
                        FileInfo fileInfo = new FileInfo(truepath);
                        string shortfilename = fileInfo.Name;
                        if (shortfilename == "")
                        {
                            shortfilename = fileInfo.FullName;
                        }

                        AddNew(shortfilename, Dock.GetSourceFromIcon(Dock.GetSystemIcon(truepath)), truepath);
                    } else
                    {
                        x--;
                    }
                    
                }
                
                x++;
            }
            string epath = Environment.GetEnvironmentVariable("windir") + "\\explorer.exe";
            AddNew(AppLanguage.GetDialogByLocale(AppLanguage.Dialog.OpenInExplorer, Dock.locale), Dock.GetSourceFromIcon(Dock.GetSystemIcon(epath)), "explorer");

            Loaded += (a, b) =>
            {
                TranslateTransform offsetTransform = new TranslateTransform();
                DoubleAnimation anim = new DoubleAnimation
                {
                    From = 30,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new QuarticEase()
                };
                offsetTransform.BeginAnimation(TranslateTransform.YProperty, anim);
                MainD.RenderTransform = offsetTransform;
                MainD.BeginAnimation(Grid.OpacityProperty, Animations.OpacityAnimation(0, 1));

                NativeMethods.EnableBlur(this);
            };
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            CloseApp();
        }
    }
}
