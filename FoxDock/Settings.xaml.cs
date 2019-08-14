using Microsoft.Win32;
using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace FoxDock
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public static AppLanguage.Locale locale = AppLanguage.GetSystemLocale();

        public Settings()
        {
            RoutedEventHandler handler = null;
            handler = (s, e) =>
            {
                Loaded -= handler;
            };
            Loaded += handler;
            Activated += Settings_Activated;
            InitializeComponent();

            //Локализация заголовка Настроек
            SettingsHeader.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsShort, locale);
            this.Title = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsShort, locale);

            //Локализация вкладок Настроек
            t_1_text.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.SettingsHomeTab, locale);
            t_2_text.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.SettingsPerfomanceTab, locale);
            t_3_text.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.SettingsCustomizeTab, locale);

            //Локализация подписей Настроек
            DockSettingsStartupLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsStartupLabel, locale);
            DockSettingsDisableBlurLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsDisableBlurLabel, locale);
            DockSettingsEnableStarDustLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsEnableStarDustLabel, locale);
            DockSettingsPanelScaleLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsPanelScaleLabel, locale);
            DockSettingsBackgroundOpacityLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsBackgroundOpacityLabel, locale);
            DockSettingsDisplayDockOnTopLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsDisplayDockOnTopLabel, locale);
            DockSettingsAutoHideLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsAutoHideLabel, locale);
            DockSettingsBGMColorLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsBGMColorLabel, locale);
            DockSettingsBGHColorLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsBGHColorLabel, locale);

            //Грузим кеш и делаем активным нужный цвет фона
            MainWindow.cache = CacheOperations.LoadCache(MainWindow.cache);
            object sender = new object();

            switch (MainWindow.cache.background)
            {
                case DockBackground.Auto:
                    sender = BGMAuto;
                    break;
                case DockBackground.Black:
                    sender = BGMBlack;
                    break;
                case DockBackground.Gray:
                    sender = BGMGray;
                    break;
                case DockBackground.White:
                    sender = BGMWhite;
                    break;
                case DockBackground.Accent:
                    sender = BGMAccent;
                    break;
            }
            BGActiveAnimation(sender, BGM_Colors.Children);
            switch (MainWindow.cache.hintBackground)
            {
                case HintBackground.Auto:
                    sender = BGHAuto;
                    break;
                case HintBackground.Black:
                    sender = BGHBlack;
                    break;
                case HintBackground.Gray:
                    sender = BGHGray;
                    break;
                case HintBackground.White:
                    sender = BGHWhite;
                    break;
                case HintBackground.Accent:
                    sender = BGHAccent;
                    break;
            }
            BGActiveAnimation(sender, BGH_Colors.Children);

        }

        private void Settings_Activated(object sender, EventArgs e)
        {
        }



        private void MoveWindow(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            }
            catch
            {

            }

        }
        private void UpdateMenuBySender(object sender)
        {
            HomeTabButton.IsEnabled = true;
            PerfomanceTabButton.IsEnabled = true;
            CustomizeTabButton.IsEnabled = true;

            HomeTabButton.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            PerfomanceTabButton.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            CustomizeTabButton.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));

            DropShadowEffect noglow = new DropShadowEffect();
            noglow.ShadowDepth = 0;
            noglow.Color = Color.FromArgb(0, 255, 255, 255);
            noglow.Opacity = 0;

            t_1_text.Effect = noglow;
            t_2_text.Effect = noglow;
            t_3_text.Effect = noglow;

            t_1_icon.Effect = noglow;
            t_2_icon.Effect = noglow;
            t_3_icon.Effect = noglow;

            DropShadowEffect glow = new DropShadowEffect();
            glow.ShadowDepth = 0;
            glow.Color = Color.FromArgb(255, 255, 255, 255);
            glow.Opacity = .5;

            Button cur = (sender as Button);

            SolidColorBrush accent = AccentColors.ImmersiveSystemAccentBrush as SolidColorBrush;

            if (accent != null)
            {
                cur.Background = accent;
            }



            if (cur != null)
            {
                cur.IsEnabled = false;
                switch (cur.Name)
                {
                    case "HomeTabButton":
                        t_1_icon.Effect = glow;
                        t_1_text.Effect = glow;
                        break;
                    case "PerfomanceTabButton":
                        t_2_icon.Effect = glow;
                        t_2_text.Effect = glow;
                        break;
                    case "CustomizeTabButton":
                        t_3_icon.Effect = glow;
                        t_3_text.Effect = glow;
                        break;
                }
            }

        }
        private void changeTab(int index)
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = SettingsTabs.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new SineEase()
            };
            myDoubleAnimation.Completed += (s, e) =>
            {
                SettingsTabs.SelectedIndex = index;

                DoubleAnimation myDoubleAnimation2 = new DoubleAnimation
                {
                    From = SettingsTabs.Opacity,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.2),
                    EasingFunction = new SineEase()
                };

                SettingsTabs.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, myDoubleAnimation2);
            };
            SettingsTabs.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, myDoubleAnimation);
        }
        private void HomeTabButton_Click(object sender, RoutedEventArgs e)
        {
            changeTab(0);
            UpdateMenuBySender(sender);
        }
        private void PerfomanceTabButton_Click(object sender, RoutedEventArgs e)
        {
            changeTab(1);
            UpdateMenuBySender(sender);
        }
        private void CustomizeTabButton_Click(object sender, RoutedEventArgs e)
        {
            changeTab(2);
            UpdateMenuBySender(sender);
        }
        public static void AddApplicationToStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("FoxDock", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
            }
        }
        public static void RemoveApplicationFromStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue("FoxDock", false);
            }
        }
        private void StartupToggle_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.cache.runAtStartup = true;
            CacheOperations.StoreCache(MainWindow.cache);
            //Debug.WriteLine(MainWindow.cache.runAtStartup);
            RemoveApplicationFromStartup();
            AddApplicationToStartup();
        }

        private void StartupToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindow.cache.runAtStartup = false;
            CacheOperations.StoreCache(MainWindow.cache);
            //Debug.WriteLine(MainWindow.cache.runAtStartup);
            RemoveApplicationFromStartup();
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void DisableBlur_Checked(object sender, RoutedEventArgs e)
        {
            //NativeMethods.DisableBlur(this);

            if (window != null)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    NativeMethods.DisableBlur(window);
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }

            MainWindow.cache.disableBlur = true;
            CacheOperations.StoreCache(MainWindow.cache);
        }

        private void DisableBlur_Unchecked(object sender, RoutedEventArgs e)
        {
            //NativeMethods.EnableBlur(this);

            if (window != null)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    NativeMethods.EnableBlur(window);
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }

            MainWindow.cache.disableBlur = false;
            CacheOperations.StoreCache(MainWindow.cache);
        }

        private void StarDustEnable_Checked(object sender, RoutedEventArgs e)
        {
            if (window != null)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    window.StarDust.Visibility = Visibility.Visible;
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }

            MainWindow.cache.enableStarDust = true;
            CacheOperations.StoreCache(MainWindow.cache);
        }

        private void StarDustEnable_Unchecked(object sender, RoutedEventArgs e)
        {
            if (window != null)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    window.StarDust.Visibility = Visibility.Hidden;
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }

            MainWindow.cache.enableStarDust = false;
            CacheOperations.StoreCache(MainWindow.cache);
        }

        private void MButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Button cur = (sender as Button);

            cur.Opacity = 1;
        }

        private void MButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Button cur = (sender as Button);

            cur.Opacity = 1;
        }
        public void Toggle_Loaded_Do(CheckBox target)
        {
            SolidColorBrush accent = AccentColors.ImmersiveSystemAccentBrush as SolidColorBrush;

            SolidColorBrush alt_br = new SolidColorBrush(Color.FromRgb(accent.Color.R, accent.Color.G, accent.Color.B));
            alt_br.Opacity = 0;

            SolidColorBrush result_brush = new SolidColorBrush();

            if (target.IsChecked == true)
                result_brush = accent;
            else
                result_brush = alt_br;

            BrushAnimation brushAnimation = new BrushAnimation
            {
                From = target.Background,
                To = result_brush,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            try
            {
                target.BeginAnimation(CheckBox.BackgroundProperty, brushAnimation);
            }
            catch
            {

            }

        }
        public void Toggle_Loaded(object sender, RoutedEventArgs e)
        {
            CheckBox target = (sender as CheckBox);

            Toggle_Loaded_Do(target);
        }

        private void EnableTopmostToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (window != null)
            {
                Task.Factory.StartNew(() =>
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        WindowAPI.SendToTop(window);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                });
            }
            MainWindow.cache.enableTopmost = true;
            CacheOperations.StoreCache(MainWindow.cache);
        }

        private void EnableTopmostToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (window != null)
            {
                Task.Factory.StartNew(() =>
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        WindowAPI.SendToBack(window);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                });
            }
            MainWindow.cache.enableTopmost = false;
            CacheOperations.StoreCache(MainWindow.cache);
        }
        public MainWindow window;
        public bool ds = false;
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!MainWindow.lock_slider)
            {
                if (window != null)
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        window.App_full_bg.Opacity = e.NewValue;
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                }
                if (!ds)
                {
                    ds = true;
                }
                else
                {
                    MainWindow.cache.bg_trans = e.NewValue;
                    CacheOperations.StoreCache(MainWindow.cache);
                }

            }

        }

        private void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!MainWindow.lock_slider)
            {
                if (window != null)
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        window.size = (int)(MainWindow.defsize * e.NewValue);

                        //Комбинируем основные значки с виджетами
                        List<DockIcon> combined = new List<DockIcon>();
                        foreach (DockIcon di in window.MainPanel.Children)
                        {
                            combined.Add(di);
                        }
                        foreach (DockIcon di in window.AIcons.Children)
                        {
                            combined.Add(di);
                        }

                        foreach (DockIcon img in combined)
                        {
                            DoubleAnimation da = new DoubleAnimation
                            {
                                From = img.Size,
                                To = window.size,
                                Duration = TimeSpan.FromMilliseconds(100),
                                EasingFunction = new SineEase()
                            };

                            img.BeginAnimation(DockIcon.SizeProperty, da);
                        }
                        int i = 0;

                        double new_h = window.size + window.size / 2.5;
                        double new_top = System.Windows.SystemParameters.PrimaryScreenHeight - new_h;

                        window.UpdateDockWidth();
                        window.animateHChange(new_top, new_h);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                    MainWindow.cache.scaleFactor = e.NewValue;
                    CacheOperations.StoreCache(MainWindow.cache);
                }


            }
        }
        private void UpdateDockBG()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                window.AutoWallUI();
            }));
        }
        private void BGActiveAnimation(object sender, UIElementCollection borders)
        {
            ThicknessAnimation activeAnimation = new ThicknessAnimation
            {
                From = new Thickness(2, 2, 2, 2),
                To = new Thickness(5, 5, 5, 5),
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new SineEase()
            };
            ThicknessAnimation inactiveAnimation = new ThicknessAnimation
            {
                From = new Thickness(2, 2, 2, 2),
                To = new Thickness(2, 2, 2, 2),
                Duration = TimeSpan.FromMilliseconds(0),
                EasingFunction = new SineEase()
            };

            BrushAnimation activeColorAnimation = new BrushAnimation
            {
                From = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                To = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                Duration = TimeSpan.FromMilliseconds(150)
            };
            BrushAnimation inactiveColorAnimation = new BrushAnimation
            {
                From = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                To = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                Duration = TimeSpan.FromMilliseconds(0),
            };


            foreach (Border border in borders)
            {
                border.BeginAnimation(BorderThicknessProperty, inactiveAnimation);
                border.BeginAnimation(BorderBrushProperty, inactiveColorAnimation);
                border.Opacity = .5;
            }
            (sender as Border).BeginAnimation(BorderThicknessProperty, activeAnimation);
            (sender as Border).BeginAnimation(BorderBrushProperty, activeColorAnimation);
            (sender as Border).Opacity = 1;
        }
        private void AHToggle_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.cache.dockAutoHide = true;
            CacheOperations.StoreCache(MainWindow.cache);
        }

        private void AHToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindow.cache.dockAutoHide = false;
            CacheOperations.StoreCache(MainWindow.cache);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.MoveWindow(sender, e);
        }

        private void BGMAuto_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.cache.background = DockBackground.Auto;
            CacheOperations.StoreCache(MainWindow.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGM_Colors.Children);
        }

        private void BGMBlack_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.cache.background = DockBackground.Black;
            CacheOperations.StoreCache(MainWindow.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGM_Colors.Children);
        }

        private void BGMGray_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.cache.background = DockBackground.Gray;
            CacheOperations.StoreCache(MainWindow.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGM_Colors.Children);
        }

        private void BGMWhite_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.cache.background = DockBackground.White;
            CacheOperations.StoreCache(MainWindow.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGM_Colors.Children);
        }

        private void SettingsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BGMAccent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.cache.background = DockBackground.Accent;
            CacheOperations.StoreCache(MainWindow.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGM_Colors.Children);
        }

        private void BGHAuto_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.cache.hintBackground = HintBackground.Auto;
            CacheOperations.StoreCache(MainWindow.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGH_Colors.Children);
        }
        private void BGHBlack_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.cache.hintBackground = HintBackground.Black;
            CacheOperations.StoreCache(MainWindow.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGH_Colors.Children);
        }
        private void BGHGray_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.cache.hintBackground = HintBackground.Gray;
            CacheOperations.StoreCache(MainWindow.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGH_Colors.Children);
        }

        private void BGHWhite_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.cache.hintBackground = HintBackground.White;
            CacheOperations.StoreCache(MainWindow.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGH_Colors.Children);
        }

        private void BGHAccent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.cache.hintBackground = HintBackground.Accent;
            CacheOperations.StoreCache(MainWindow.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGH_Colors.Children);
        }

        
    }
}
