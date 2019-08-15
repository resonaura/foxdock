using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Color = System.Windows.Media.Color;

namespace FoxDock
{
    class Animations
    {
        public static System.Drawing.Color GetDominantColor(Bitmap bmp)
        {
            //Used for tally
            int r = 0;
            int g = 0;
            int b = 0;

            int total = 0;

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    System.Drawing.Color clr = bmp.GetPixel(x, y);

                    r += clr.R;
                    g += clr.G;
                    b += clr.B;

                    total++;
                }
            }

            //Calculate average
            r /= total;
            g /= total;
            b /= total;

            return System.Drawing.Color.FromArgb(r, g, b);
        }
        
        public static void DominantBGAnimate(System.Drawing.Color dominant, Border App_bg, Tooltip tooltip, Border WhiteOverlay, Border BlackOverlay)
        {
            try
            {
                BrushAnimation brushAnimation = new BrushAnimation
                {
                    From = App_bg.Background,
                    To = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, dominant.R, dominant.G, dominant.B)),
                    Duration = TimeSpan.FromSeconds(0.5)
                };
                Timeline.SetDesiredFrameRate(brushAnimation, 60);
                App_bg.BeginAnimation(Border.BackgroundProperty, brushAnimation);

                int r = dominant.R + 10;
                int g = dominant.G + 10;
                int b = dominant.B + 10;

                if (r > 255)
                {
                    g = g - (r - 255);
                    b = b - (r - 255);
                    if (g < 0) g = 0;
                    if (b < 0) b = 0;
                    r = 255;
                }
                if (g > 255)
                {
                    r = r - (g - 255);
                    b = b - (g - 255);
                    if (r < 0) r = 0;
                    if (b < 0) b = 0;
                    g = 255;
                }
                if (b > 255)
                {
                    r = r - (b - 255);
                    g = g - (b - 255);
                    if (r < 0) r = 0;
                    if (g < 0) g = 0;
                    b = 255;
                }

                BrushAnimation brushAnimation2 = new BrushAnimation
                {
                    From = App_bg.Background,
                    To = new SolidColorBrush(System.Windows.Media.Color.FromArgb(250, (byte)r, (byte)g, (byte)b)),
                    Duration = TimeSpan.FromSeconds(0.5)
                };
                Timeline.SetDesiredFrameRate(brushAnimation2, 60);
                tooltip.app_hint.BeginAnimation(Label.BackgroundProperty, brushAnimation2);

                BrushAnimation brushAnimation3 = new BrushAnimation
                {
                    From = App_bg.Background,
                    To = new SolidColorBrush(System.Windows.Media.Color.FromArgb(30, dominant.R, dominant.G, dominant.B)),
                    Duration = TimeSpan.FromSeconds(0.5)
                };
                Timeline.SetDesiredFrameRate(brushAnimation3, 60);
                double bright = dominant.GetBrightness();

                double black_per = (0.5 - bright) / 0.5;
                if (black_per < 0) black_per = 0;

                double white_per = 1 - black_per;

                double black_opacity = 1 * white_per;
                double white_opacity = 1 * black_per;

                /*var uiSettings = new System.Windows.UI.ViewManagement.UISettings();
                var color = uiSettings.getColorValue(
                Windows.UI.ViewManagement.UIColorType.background
                );*/

                DoubleAnimation op1 = Animations.OpacityAnimation(BlackOverlay.Opacity, black_opacity);
                BlackOverlay.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, op1);

                DoubleAnimation op2 = Animations.OpacityAnimation(WhiteOverlay.Opacity, white_opacity);
                WhiteOverlay.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, op2);

                //MainContextMenu.BeginAnimation(Label.BackgroundProperty, brushAnimation3);
            }
            catch
            {
                Debug.WriteLine("Dominate bg error...");
            }



        }

        public string theme = String.Empty;
        public static void Break(UIElement element)
        {
            DoubleAnimation myAnimation = new DoubleAnimation();
            // Initialize animation
            

            // To start
            element.BeginAnimation(Window.OpacityProperty, myAnimation);

            // To stop and keep the current value of the animated property
            myAnimation.BeginTime = null;
            element.BeginAnimation(Window.OpacityProperty, myAnimation);
        }
        public static void ThemeAnimate(string theme, Border App_bg, Tooltip tooltip, Border WhiteOverlay, Border BlackOverlay, List<DockIcon> combined, DockBackground dockBackground, HintBackground hintBackground, ContextMenu MainContextMenu, ResourceDictionary Resources, Cache cache)
        {
            double black_opacity = 0;
            double white_opacity = 0;
            double accent_opacity = 0;

            int hint_opacity = (int)(cache.hm_trans * 255);

            switch (dockBackground)
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
            switch(hintBackground)
            {
                case HintBackground.Auto:
                    if (theme == "0")
                    {
                        tooltip.app_hint.Background = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 24, 24, 24));
                        tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    }
                    if (theme == "1")
                    {
                        tooltip.app_hint.Background = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 255, 255, 255));
                        tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                    }
                    break;
                case HintBackground.Black:
                    tooltip.app_hint.Background = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 24, 24, 24));
                    tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    break;
                case HintBackground.Gray:
                    tooltip.app_hint.Background = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 48, 48, 48));
                    tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    break;
                case HintBackground.White:
                    tooltip.app_hint.Background = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 255, 255, 255));
                    tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                    break;
                case HintBackground.Accent:
                    SolidColorBrush accent_brush = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, SystemParameters.WindowGlassColor.R, SystemParameters.WindowGlassColor.G, SystemParameters.WindowGlassColor.B));
                    tooltip.app_hint.Background = accent_brush;
                    tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    break;

            }
            string hcolor = "White";
            switch (cache.IndicatorColor)
            {
                case IndicatorColor.Auto:
                    if (theme == "0")
                    {
                        hcolor = "White";
                    }
                    if (theme == "1")
                    {
                        hcolor = "Black";
                    }
                break;
                case IndicatorColor.Black:
                    hcolor = "Black";
                    break;
                case IndicatorColor.Gray:
                    hcolor = "Gray";
                    break;
                case IndicatorColor.White:
                    hcolor = "White";
                    break;
                case IndicatorColor.Accent:
                    hcolor = "Accent";
                    break;
            }
            foreach (DockIcon ic in combined)
            {
                ic.HighlightColor = hcolor;
            }
            switch(cache.MenuColor)
            {
                case MenuColor.Auto:
                    if(theme == "0")
                    {
                        MainContextMenu.Background = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                        MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                        foreach (UIElement item in MainContextMenu.Items)
                        {
                            MenuItem mi = item as MenuItem;
                            if (mi != null)
                                mi.Template = (ControlTemplate)Resources["DarkCoolMenuItem"];
                        }
                    }
                    if (theme == "1")
                    {
                        MainContextMenu.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                        MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(24, 24, 24));

                        foreach (UIElement item in MainContextMenu.Items)
                        {
                            MenuItem mi = item as MenuItem;
                            if (mi != null)
                                mi.Template = (ControlTemplate)Resources["WhiteCoolMenuItem"];
                        }
                    }
                    break;
                case MenuColor.Black:
                    MainContextMenu.Background = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                    MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                    foreach (UIElement item in MainContextMenu.Items)
                    {
                        MenuItem mi = item as MenuItem;
                        if (mi != null)
                            mi.Template = (ControlTemplate)Resources["DarkCoolMenuItem"];
                    }
                    break;
                case MenuColor.Gray:
                    MainContextMenu.Background = new SolidColorBrush(Color.FromRgb(48, 48, 48));
                    MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                    foreach (UIElement item in MainContextMenu.Items)
                    {
                        MenuItem mi = item as MenuItem;
                        if (mi != null)
                            mi.Template = (ControlTemplate)Resources["DarkCoolMenuItem"];
                    }
                    break;
                case MenuColor.White:
                    MainContextMenu.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(24, 24, 24));

                    foreach (UIElement item in MainContextMenu.Items)
                    {
                        MenuItem mi = item as MenuItem;
                        if (mi != null)
                            mi.Template = (ControlTemplate)Resources["WhiteCoolMenuItem"];
                    }
                    break;
                case MenuColor.Accent:
                    MainContextMenu.Background = SystemParameters.WindowGlassBrush;
                    MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                    foreach (UIElement item in MainContextMenu.Items)
                    {
                        MenuItem mi = item as MenuItem;
                        if (mi != null)
                            mi.Template = (ControlTemplate)Resources["DarkCoolMenuItem"];
                    }
                    break;
            }

            
            DoubleAnimation op1 = Animations.OpacityAnimation(BlackOverlay.Opacity, black_opacity);
            BlackOverlay.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, op1);

            DoubleAnimation op2 = Animations.OpacityAnimation(WhiteOverlay.Opacity, white_opacity);
            WhiteOverlay.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, op2);

            DoubleAnimation op3 = Animations.OpacityAnimation(App_bg.Opacity, accent_opacity);
            App_bg.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, op3);
        }
        public static DoubleAnimation OpacityAnimation(double from, double to, double duration = 0.5)
        {
            DoubleAnimation anim = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(duration),
                EasingFunction = new SineEase()
            };
            Timeline.SetDesiredFrameRate(anim, 60);

            return anim;
        }
    }
}
