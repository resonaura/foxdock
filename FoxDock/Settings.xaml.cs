using Microsoft.Win32;
using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
    /// Логика взаимодействия для xaml
    /// </summary>
    public partial class Settings : Window
    {
        
        public SolidColorBrush UpdateColorInBrush(SolidColorBrush source, Color color)
        {
            SolidColorBrush result = new SolidColorBrush
            {
                Color = color,
                Opacity = source.Opacity
            };

            return result;
        }

        public static AppLanguage.Locale locale = AppLanguage.GetSystemLocale();

        public Settings(Dock dock)
        {
            InitializeComponent();
            window = dock;

            var theme = "0";

            try
            {
                //Получаем из реестра тему
                var wpReg = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", false);
                theme = wpReg.GetValue("SystemUsesLightTheme").ToString();

                //Закрываем работу с реестра
                wpReg.Close();
            }
            catch
            {
                //Some code
            }

            
            if(theme == "1")
            {
                this.Foreground = new SolidColorBrush(Colors.Black);
                BgPanel1.Fill = new SolidColorBrush(Color.FromArgb(70, 0, 0, 0));
                BgPanel2.Fill = new SolidColorBrush(Color.FromArgb(255, 210, 210, 210));
                AcrylicWindow.SetTintColor(this, Colors.White);
                AcrylicWindow.SetFallbackColor(this, Color.FromArgb(255, 200, 200, 200));
                UpdateMenuBySender(HomeTabButton);
            }
            Activated += Settings_Activated;
            

            this.DataContext = this;

            //Локализация заголовка Настроек
            SettingsHeader.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsShort, locale);
            this.Title = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsShort, locale);

            //Локализация вкладок Настроек
            t_1_text.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.SettingsHomeTab, locale);
            t_2_text.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.SettingsPerfomanceTab, locale);
            t_3_text.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.SettingsCustomizeTab, locale);
            t_4_text.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.SettingsIconPacksTab, locale);
            t_5_text.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.SettingsHelpTab, locale);
            t_6_text.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.SettingsAboutTab, locale);

            //Локализация подписей Настроек
            DockSettingsStartupLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsStartupLabel, locale);
            DockSettingsDisableBlurLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsDisableBlurLabel, locale);
            DockSettingsEnableStarDustLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsEnableStarDustLabel, locale);
            DockSettingsSmartDisableLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsSmartDisableLabel, locale);
            DockSettingsPanelScaleLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsPanelScaleLabel, locale);
            DockSettingsBackgroundOpacityLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsBackgroundOpacityLabel, locale);
            DockSettingsHintOpacityLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsHintOpacityLabel, locale);
            DockSettingsDisplayDockOnTopLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsDisplayDockOnTopLabel, locale);
            DockSettingsAutoHideLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsAutoHideLabel, locale);
            DockSettingsBGMColorLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsBGMColorLabel, locale);
            DockSettingsBGHColorLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsBGHColorLabel, locale);
            DockSettingsBGIColorLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsBGIColorLabel, locale);
            DockSettingsBGNColorLabel.Text = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsBGNColorLabel, locale);
            DockSettingsNoHelp.Content = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettingsNoHelp, locale);

            //Грузим кеш и делаем активным нужный цвет фона
            Dock.cache = CacheOperations.LoadCache(Dock.cache);
            object sender = new object();

            //Задаём значение всем параметрам настроек
            StartupToggle.IsChecked = Dock.cache.runAtStartup;
            DisableBlurToggle.IsChecked = Dock.cache.disableBlur;
            StarDustEnableToggle.IsChecked = Dock.cache.enableStarDust;
            SmartDisableToggle.IsChecked = Dock.cache.smart_disable;
            EnableTopmostToggle.IsChecked = Dock.cache.enableTopmost;
            AHToggle.IsChecked = Dock.cache.dockAutoHide;
            Trans_bar.Value = Dock.cache.bg_trans;
            Hint_trans_bar.Value = Dock.cache.hm_trans;
            ScaleSlider.Value = Dock.cache.scaleFactor;

            //Выполняем логику для слайдеров настроек
            Toggle_Loaded_Do(StartupToggle);
            Toggle_Loaded_Do(DisableBlurToggle);
            Toggle_Loaded_Do(StarDustEnableToggle);
            Toggle_Loaded_Do(SmartDisableToggle);
            Toggle_Loaded_Do(AHToggle);
            Toggle_Loaded_Do(EnableTopmostToggle);

            
            

            switch (Dock.cache.background)
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
            switch (Dock.cache.hintBackground)
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
            switch (Dock.cache.IndicatorColor)
            {
                case IndicatorColor.Auto:
                    sender = BGIAuto;
                    break;
                case IndicatorColor.Black:
                    sender = BGIBlack;
                    break;
                case IndicatorColor.Gray:
                    sender = BGIGray;
                    break;
                case IndicatorColor.White:
                    sender = BGIWhite;
                    break;
                case IndicatorColor.Accent:
                    sender = BGIAccent;
                    break;
            }
            BGActiveAnimation(sender, BGI_Colors.Children);
            switch (Dock.cache.MenuColor)
            {
                case MenuColor.Auto:
                    sender = BGNAuto;
                    break;
                case MenuColor.Black:
                    sender = BGNBlack;
                    break;
                case MenuColor.Gray:
                    sender = BGNGray;
                    break;
                case MenuColor.White:
                    sender = BGNWhite;
                    break;
                case MenuColor.Accent:
                    sender = BGNAccent;
                    break;
            }
            BGActiveAnimation(sender, BGN_Colors.Children);



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
            IconPackTabButton.IsEnabled = true;
            AboutTabButton.IsEnabled = true;
            HelpTabButton.IsEnabled = true;

            HomeTabButton.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            PerfomanceTabButton.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            CustomizeTabButton.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            IconPackTabButton.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            AboutTabButton.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            HelpTabButton.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));

            DropShadowEffect noglow = new DropShadowEffect
            {
                ShadowDepth = 0,
                Color = Color.FromArgb(0, 255, 255, 255),
                Opacity = 0
            };

            t_1_text.Effect = noglow;
            t_2_text.Effect = noglow;
            t_3_text.Effect = noglow;
            t_4_text.Effect = noglow;
            t_5_text.Effect = noglow;
            t_6_text.Effect = noglow;

            t_1_icon.Effect = noglow;
            t_2_icon.Effect = noglow;
            t_3_icon.Effect = noglow;
            t_4_icon.Effect = noglow;
            t_5_icon.Effect = noglow;
            t_6_icon.Effect = noglow;

            t_1_text.Foreground = this.Foreground;
            t_2_text.Foreground = this.Foreground;
            t_3_text.Foreground = this.Foreground;
            t_4_text.Foreground = this.Foreground;
            t_5_text.Foreground = this.Foreground;
            t_6_text.Foreground = this.Foreground;

            t_1_icon.Foreground = this.Foreground;
            t_2_icon.Foreground = this.Foreground;
            t_3_icon.Foreground = this.Foreground;
            t_4_icon.Foreground = this.Foreground;
            t_5_icon.Foreground = this.Foreground;
            t_6_icon.Foreground = this.Foreground;

            DropShadowEffect glow = new DropShadowEffect
            {
                ShadowDepth = 0,
                Color = Color.FromArgb(255, 255, 255, 255),
                Opacity = .5
            };

            Button cur = (sender as Button);


            if (AccentColors.ImmersiveSystemAccentBrush is SolidColorBrush accent)
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
                        t_1_icon.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        t_1_text.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        break;
                    case "PerfomanceTabButton":
                        t_2_icon.Effect = glow;
                        t_2_text.Effect = glow;
                        t_2_icon.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        t_2_text.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        break;
                    case "CustomizeTabButton":
                        t_3_icon.Effect = glow;
                        t_3_text.Effect = glow;
                        t_3_icon.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        t_3_text.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        break;
                    case "IconPackTabButton":
                        t_4_icon.Effect = glow;
                        t_4_text.Effect = glow;
                        t_4_icon.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        t_4_text.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        break;
                    case "HelpTabButton":
                        t_5_icon.Effect = glow;
                        t_5_text.Effect = glow;
                        t_5_icon.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        t_5_text.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        break;
                    case "AboutTabButton":
                        t_6_icon.Effect = glow;
                        t_6_text.Effect = glow;
                        t_6_icon.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        t_6_text.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        break;
                }
            }

        }
        private void ChangeTab(int index)
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
            ChangeTab(0);
            UpdateMenuBySender(sender);
        }
        private void PerfomanceTabButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeTab(1);
            UpdateMenuBySender(sender);
        }
        private void CustomizeTabButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeTab(2);
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
            Dock.cache.runAtStartup = true;
            CacheOperations.StoreCache(Dock.cache);
            //Debug.WriteLine(MainWindow.cache.runAtStartup);
            RemoveApplicationFromStartup();
            AddApplicationToStartup();
        }

        private void StartupToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            Dock.cache.runAtStartup = false;
            CacheOperations.StoreCache(Dock.cache);
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
                    API.Acryl.DisableBlur(window);
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }

            Dock.cache.disableBlur = true;
            CacheOperations.StoreCache(Dock.cache);
        }

        private void DisableBlur_Unchecked(object sender, RoutedEventArgs e)
        {
            //NativeMethods.EnableBlur(this);

            if (window != null)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    API.Acryl.EnableBlur(window);
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }

            Dock.cache.disableBlur = false;
            CacheOperations.StoreCache(Dock.cache);
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

            Dock.cache.enableStarDust = true;
            CacheOperations.StoreCache(Dock.cache);
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

            Dock.cache.enableStarDust = false;
            CacheOperations.StoreCache(Dock.cache);
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

            SolidColorBrush alt_br = new SolidColorBrush(Color.FromRgb(accent.Color.R, accent.Color.G, accent.Color.B))
            {
                Opacity = 0
            };

            SolidColorBrush result_brush;

            if (target.IsChecked == true)
            {
                result_brush = accent;
            }
            else
            {
                result_brush = alt_br;
            }

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
            catch { }

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
                        API.WindowsManager.SendToTop(window);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                });
            }
            Dock.cache.enableTopmost = true;
            CacheOperations.StoreCache(Dock.cache);
        }

        private void EnableTopmostToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (window != null)
            {
                Task.Factory.StartNew(() =>
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        API.WindowsManager.SendToBack(window);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                });
            }
            Dock.cache.enableTopmost = false;
            CacheOperations.StoreCache(Dock.cache);
        }
        public Dock window;
        public bool ds = false;
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Dock.lock_slider)
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
                    Dock.cache.bg_trans = e.NewValue;
                    CacheOperations.StoreCache(Dock.cache);
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
            Dock.cache.dockAutoHide = true;
            CacheOperations.StoreCache(Dock.cache);
        }

        private void AHToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            Dock.cache.dockAutoHide = false;
            CacheOperations.StoreCache(Dock.cache);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.MoveWindow(sender, e);
        }

        private void BGMAuto_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.background = DockBackground.Auto;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGM_Colors.Children);
        }

        private void BGMBlack_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.background = DockBackground.Black;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGM_Colors.Children);
        }

        private void BGMGray_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.background = DockBackground.Gray;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGM_Colors.Children);
        }

        private void BGMWhite_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.background = DockBackground.White;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGM_Colors.Children);
        }

        private void SettingsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BGMAccent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.background = DockBackground.Accent;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGM_Colors.Children);
        }

        private void BGHAuto_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.hintBackground = HintBackground.Auto;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGH_Colors.Children);
        }
        private void BGHBlack_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.hintBackground = HintBackground.Black;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGH_Colors.Children);
        }
        private void BGHGray_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.hintBackground = HintBackground.Gray;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGH_Colors.Children);
        }

        private void BGHWhite_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.hintBackground = HintBackground.White;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGH_Colors.Children);
        }

        private void BGHAccent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.hintBackground = HintBackground.Accent;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGH_Colors.Children);
        }

        private void Hint_trans_bar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Dock.lock_slider)
            {
                if (!ds)
                {
                    ds = true;
                }
                else
                {
                    Dock.cache.hm_trans = e.NewValue;
                    CacheOperations.StoreCache(Dock.cache);
                    UpdateDockBG();
                }

            }
        }

        private void BGIAuto_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.IndicatorColor = IndicatorColor.Auto;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGI_Colors.Children);
        }

        private void BGIBlack_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.IndicatorColor = IndicatorColor.Black;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGI_Colors.Children);
        }

        private void BGIGray_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.IndicatorColor = IndicatorColor.Gray;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGI_Colors.Children);
        }

        private void BGIWhite_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.IndicatorColor = IndicatorColor.White;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGI_Colors.Children);
        }

        private void BGIAccent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.IndicatorColor = IndicatorColor.Accent;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGI_Colors.Children);
        }

        private void BGNAuto_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.MenuColor = MenuColor.Auto;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGN_Colors.Children);
        }

        private void BGNBlack_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.MenuColor = MenuColor.Black;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGN_Colors.Children);
        }

        private void BGNGray_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.MenuColor = MenuColor.Gray;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGN_Colors.Children);
        }

        private void BGNWhite_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.MenuColor = MenuColor.White;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGN_Colors.Children);
        }

        private void BGNAccent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dock.cache.MenuColor = MenuColor.Accent;
            CacheOperations.StoreCache(Dock.cache);
            UpdateDockBG();
            BGActiveAnimation(sender, BGN_Colors.Children);
        }

        private void SmartDisableToggle_Checked(object sender, RoutedEventArgs e)
        {
            Dock.cache.smart_disable = true;
            CacheOperations.StoreCache(Dock.cache);
        }

        private void SmartDisableToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            Dock.cache.smart_disable = false;
            CacheOperations.StoreCache(Dock.cache);
        }
        private void ChangeDockSize(double dsize)
        {
            if (!Dock.lock_slider)
            {
                if (window != null)
                {

                    Slider slider = ScaleSlider;
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        window.size = (int)(Dock.defsize * dsize);

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

                        double new_h = window.size + window.size / 2.5;
                        double new_top = System.Windows.SystemParameters.PrimaryScreenHeight - new_h;

                        window.UpdateDockWidth();
                        window.AnimateHChange(new_top, new_h);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                    Dock.cache.scaleFactor = dsize;
                    CacheOperations.StoreCache(Dock.cache);
                }


            }
        }
        private void ScaleSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            ChangeDockSize(ScaleSlider.Value);
        }

        private void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.OldValue == ScaleSlider.Minimum && e.NewValue == ScaleSlider.Maximum || e.OldValue == ScaleSlider.Maximum && e.NewValue == ScaleSlider.Minimum)
            {
                ChangeDockSize(e.NewValue);
            }
        }
        private bool IPacksUpdated = false;
        private void IconPackTabButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeTab(3);
            UpdateMenuBySender(sender);
            if (IPacksUpdated) return;
            IPSpinner.Visibility = Visibility.Visible;
            IPSpinner.BeginAnimation(OpacityProperty, Animations.SingleAnimation(0, 1));
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IconPacks.UpdatePacks();
                    if (Dock.cache.iconPackName == "" || (Dock.cache.iconPackName != "" && Dock.cache.iconPackName == "Clean"))
                    {
                        SolidColorBrush fr = this.Foreground as SolidColorBrush;
                        DefaultIconPack.Background = new SolidColorBrush(Color.FromArgb(50, fr.Color.R, fr.Color.G, fr.Color.B));
                        DefaultIconPack.IsEnabled = false;
                    }
                    if (IconPacks.GetPacksList().Count > 0)
                    {

                        IconPacksListItem ditm = DefaultIconPack;
                        IconPacksList.Children.Clear();
                        IconPacksList.Children.Add(ditm);
                        foreach (IconPack ipack in IconPacks.GetPacksList())
                        {
                            IconPacksListItem iconPacksListItem = new IconPacksListItem
                            {
                                Txt = ipack.name,
                                Author = ipack.author,
                                Source1 = ipack.ExplorerIcon,
                                Source2 = ipack.Recent,
                                Source3 = ipack.TrashEmpty,
                                Source4 = ipack.TrashFull,
                                Foreground = DefaultIconPack.Foreground
                            };
                            iconPacksListItem.Click += DefaultIconPack_Click;
                            if (Dock.cache.iconPackName != "")
                            {
                                if (Dock.cache.iconPackName == ipack.name)
                                {
                                    SolidColorBrush fr = this.Foreground as SolidColorBrush;
                                    iconPacksListItem.Background = new SolidColorBrush(Color.FromArgb(50, fr.Color.R, fr.Color.G, fr.Color.B));
                                    iconPacksListItem.IsEnabled = false;
                                }
                            }

                            IconPacksList.Children.Add(iconPacksListItem);
                        }
                        DoubleAnimation fadeOutSpinner = Animations.SingleAnimation(1, 0);
                        fadeOutSpinner.Completed += (x, y) =>
                        {
                            IPSpinner.Visibility = Visibility.Hidden;
                            IPacksUpdated = true;
                        };
                        IPSpinner.BeginAnimation(OpacityProperty, fadeOutSpinner);
                    }
                });
                
            });
        }

        private void DefaultIconPack_Click(object sender, RoutedEventArgs e)
        {
            if (sender is IconPacksListItem ditem)
            {
                foreach (IconPacksListItem item in IconPacksList.Children)
                {
                    item.IsEnabled = true;
                    item.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                }
                SolidColorBrush fr = this.Foreground as SolidColorBrush;
                ditem.Background = new SolidColorBrush(Color.FromArgb(50, fr.Color.R, fr.Color.G, fr.Color.B));
                ditem.IsEnabled = false;

                string name = ditem.Txt;
                if(name != "")
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        IconPack iconPack = IconPacks.GetByName(name);
                        window.iPack = iconPack;
                        window.fullTrashIcon = iconPack.TrashFull;
                        window.emptyTrashIcon = iconPack.TrashEmpty;
                        window.TrashIcon.Source = API.Shell32.TrashCount() > 0 ? window.fullTrashIcon : window.emptyTrashIcon;
                        window.RecentIcon.Source = iconPack.Recent;
                        window.ExplorerIcon.Source = iconPack.ExplorerIcon;

                        IconsWorker.UpdateDockIcons(window);
                    }));
                    Dock.cache.iconPackName = name;
                    CacheOperations.StoreCache(Dock.cache);
                }
            }
        }

        private void AboutTabButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeTab(5);
            UpdateMenuBySender(sender);
        }

        private void HelpTabButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeTab(4);
            UpdateMenuBySender(sender);
        }
    }
}
