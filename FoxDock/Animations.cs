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

                double black_opacity = 0.7 * white_per;
                double white_opacity = 0.7 * black_per;

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
        public static void ThemeAnimate(string theme, Border App_bg, Tooltip tooltip, Border WhiteOverlay, Border BlackOverlay, List<DockIcon> combined)
        {
            double black_opacity = 0;
            double white_opacity = 0;

            if(theme == "0")
            {
                black_opacity = 0.5;
                white_opacity = 0;
                tooltip.app_hint.Background = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }
            if(theme == "1")
            {
                black_opacity = 0;
                white_opacity = 0.5;
                tooltip.app_hint.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(24, 24, 24));
            }
            foreach(DockIcon ic in combined)
            {
                if (theme == "0")
                {
                    ic.Theme = "Dark";
                }
                if(theme == "1")
                {
                    ic.Theme = "Light";
                }
            }
            DoubleAnimation op1 = Animations.OpacityAnimation(BlackOverlay.Opacity, black_opacity);
            BlackOverlay.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, op1);

            DoubleAnimation op2 = Animations.OpacityAnimation(WhiteOverlay.Opacity, white_opacity);
            WhiteOverlay.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, op2);
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
