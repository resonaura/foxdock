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
        
        public static void DominantBGAnimate(System.Drawing.Color dominant, Dock dock)
        {
            try
            {
                BrushAnimation brushAnimation = new BrushAnimation
                {
                    From = dock.App_bg.Background,
                    To = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, dominant.R, dominant.G, dominant.B)),
                    Duration = TimeSpan.FromSeconds(0.5)
                };
                Timeline.SetDesiredFrameRate(brushAnimation, 60);
                dock.App_bg.BeginAnimation(Border.BackgroundProperty, brushAnimation);

                int r = dominant.R + 10;
                int g = dominant.G + 10;
                int b = dominant.B + 10;

                if (r > 255)
                {
                    g -= (r - 255);
                    b -= (r - 255);
                    if (g < 0)
                    {
                        g = 0;
                    }

                    if (b < 0)
                    {
                        b = 0;
                    }

                    r = 255;
                }
                if (g > 255)
                {
                    r -= (g - 255);
                    b -= (g - 255);
                    if (r < 0)
                    {
                        r = 0;
                    }

                    if (b < 0)
                    {
                        b = 0;
                    }

                    g = 255;
                }
                if (b > 255)
                {
                    r -= (b - 255);
                    g -= (b - 255);
                    if (r < 0)
                    {
                        r = 0;
                    }

                    if (g < 0)
                    {
                        g = 0;
                    }

                    b = 255;
                }

                BrushAnimation brushAnimation2 = new BrushAnimation
                {
                    From = dock.App_bg.Background,
                    To = new SolidColorBrush(System.Windows.Media.Color.FromArgb(250, (byte)r, (byte)g, (byte)b)),
                    Duration = TimeSpan.FromSeconds(0.5)
                };
                Timeline.SetDesiredFrameRate(brushAnimation2, 60);
                dock.tooltip.app_hint.BeginAnimation(Label.BackgroundProperty, brushAnimation2);

                BrushAnimation brushAnimation3 = new BrushAnimation
                {
                    From = dock.App_bg.Background,
                    To = new SolidColorBrush(System.Windows.Media.Color.FromArgb(30, dominant.R, dominant.G, dominant.B)),
                    Duration = TimeSpan.FromSeconds(0.5)
                };
                Timeline.SetDesiredFrameRate(brushAnimation3, 60);
                double bright = dominant.GetBrightness();

                double black_per = (0.5 - bright) / 0.5;
                if (black_per < 0)
                {
                    black_per = 0;
                }

                double white_per = 1 - black_per;

                double black_opacity = 1 * white_per;
                double white_opacity = 1 * black_per;

                /*var uiSettings = new System.Windows.UI.ViewManagement.UISettings();
                var color = uiSettings.getColorValue(
                Windows.UI.ViewManagement.UIColorType.background
                );*/

                DoubleAnimation op1 = Animations.SingleAnimation(dock.BlackOverlay.Opacity, black_opacity);
                dock.BlackOverlay.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, op1);

                DoubleAnimation op2 = Animations.SingleAnimation(dock.WhiteOverlay.Opacity, white_opacity);
                dock.WhiteOverlay.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, op2);

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
        public static void ThemeAnimate(string theme, Dock dock)
        {
            double black_opacity = 0;
            double white_opacity = 0;
            double accent_opacity = 0;

            int hint_opacity = (int)(Dock.cache.hm_trans * 255);

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
            switch(Dock.cache.hintBackground)
            {
                case HintBackground.Auto:
                    if (theme == "0")
                    {
                        dock.tooltip.app_hint.Background = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 24, 24, 24));
                        dock.tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    }
                    if (theme == "1")
                    {
                        dock.tooltip.app_hint.Background = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 255, 255, 255));
                        dock.tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                    }
                    break;
                case HintBackground.Black:
                    dock.tooltip.app_hint.Background = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 24, 24, 24));
                    dock.tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    break;
                case HintBackground.Gray:
                    dock.tooltip.app_hint.Background = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 48, 48, 48));
                    dock.tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    break;
                case HintBackground.White:
                    dock.tooltip.app_hint.Background = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 255, 255, 255));
                    dock.tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                    break;
                case HintBackground.Accent:
                    SolidColorBrush accent_brush = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, SystemParameters.WindowGlassColor.R, SystemParameters.WindowGlassColor.G, SystemParameters.WindowGlassColor.B));
                    dock.tooltip.app_hint.Background = accent_brush;
                    dock.tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    break;

            }
            string hcolor = "White";
            switch (Dock.cache.IndicatorColor)
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
            foreach (DockIcon ic in Dock.GetCombined(dock.MainPanel.Children, dock.AIcons.Children))
            {
                ic.HighlightColor = hcolor;
            }
            switch(Dock.cache.MenuColor)
            {
                case MenuColor.Auto:
                    if(theme == "0")
                    {
                        dock.MainContextMenu.Background = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                        dock.MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                        foreach (UIElement item in dock.MainContextMenu.Items)
                        {
                            if (item is MenuItem mi)
                            {
                                mi.Template = (ControlTemplate)dock.Resources["DarkCoolMenuItem"];
                            }
                        }
                    }
                    if (theme == "1")
                    {
                        dock.MainContextMenu.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                        dock.MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(24, 24, 24));

                        foreach (UIElement item in dock.MainContextMenu.Items)
                        {
                            if (item is MenuItem mi)
                            {
                                mi.Template = (ControlTemplate)dock.Resources["WhiteCoolMenuItem"];
                            }
                        }
                    }
                    break;
                case MenuColor.Black:
                    dock.MainContextMenu.Background = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                    dock.MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                    foreach (UIElement item in dock.MainContextMenu.Items)
                    {
                        if (item is MenuItem mi)
                        {
                            mi.Template = (ControlTemplate)dock.Resources["DarkCoolMenuItem"];
                        }
                    }
                    break;
                case MenuColor.Gray:
                    dock.MainContextMenu.Background = new SolidColorBrush(Color.FromRgb(48, 48, 48));
                    dock.MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                    foreach (UIElement item in dock.MainContextMenu.Items)
                    {
                        if (item is MenuItem mi)
                        {
                            mi.Template = (ControlTemplate)dock.Resources["DarkCoolMenuItem"];
                        }
                    }
                    break;
                case MenuColor.White:
                    dock.MainContextMenu.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    dock.MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(24, 24, 24));

                    foreach (UIElement item in dock.MainContextMenu.Items)
                    {
                        if (item is MenuItem mi)
                        {
                            mi.Template = (ControlTemplate)dock.Resources["WhiteCoolMenuItem"];
                        }
                    }
                    break;
                case MenuColor.Accent:
                    dock.MainContextMenu.Background = SystemParameters.WindowGlassBrush;
                    dock.MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                    foreach (UIElement item in dock.MainContextMenu.Items)
                    {
                        if (item is MenuItem mi)
                        {
                            mi.Template = (ControlTemplate)dock.Resources["DarkCoolMenuItem"];
                        }
                    }
                    break;
            }

            
            DoubleAnimation op1 = Animations.SingleAnimation(dock.BlackOverlay.Opacity, black_opacity);
            dock.BlackOverlay.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, op1);

            DoubleAnimation op2 = Animations.SingleAnimation(dock.WhiteOverlay.Opacity, white_opacity);
            dock.WhiteOverlay.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, op2);

            DoubleAnimation op3 = Animations.SingleAnimation(dock.App_bg.Opacity, accent_opacity);
            dock.App_bg.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, op3);
        }
        public static DoubleAnimation SingleAnimation(double from, double to, double duration = 0.5)
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
