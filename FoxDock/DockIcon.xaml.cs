using System;
using System.Collections.Generic;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FoxDock
{
    /// <summary>
    /// Логика взаимодействия для DockIcon.xaml
    /// </summary>
    public partial class DockIcon : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(BitmapSource), typeof(DockIcon));
        public static readonly DependencyProperty HighlightProperty = DependencyProperty.Register("Highlight", typeof(bool), typeof(DockIcon));
        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(double), typeof(DockIcon));
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(DockIcon));
        public static readonly DependencyProperty ThemeProperty = DependencyProperty.Register("Theme", typeof(string), typeof(DockIcon));

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DockIcon));

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(DockIcon.ClickEvent);
            RaiseEvent(newEventArgs);
        }

        void OnClick()
        {
            RaiseClickEvent();
        }

        

        public BitmapSource Source
        {
            get { return (BitmapSource)GetValue(SourceProperty); }
            set {
                SetValue(SourceProperty, value);
            }
        }
        public string Theme
        {
            get { return (string)GetValue(ThemeProperty); }
            set {
                if(value == "Dark")
                {
                    HighlightDot.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                } else if(value == "Light")
                {
                    HighlightDot.Background = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                }
                
                SetValue(ThemeProperty, value);
            }
        }
        public bool Highlight
        {
            get { return (bool)GetValue(HighlightProperty); }
            set {
                if (value == true)
                {
                    if(HighlightDot.Opacity != 1)
                    {
                        DoubleAnimation fadeInAnimation = new DoubleAnimation
                        {
                            From = HighlightDot.Opacity,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.2),
                            EasingFunction = new SineEase()
                        };
                        HighlightDot.BeginAnimation(Border.OpacityProperty, fadeInAnimation);
                    }                    
                }
                else
                {
                    if (HighlightDot.Opacity != 0)
                    {
                        DoubleAnimation fadeOutAnimation = new DoubleAnimation
                        {
                            From = HighlightDot.Opacity,
                            To = 0,
                            Duration = TimeSpan.FromSeconds(0.2),
                            EasingFunction = new SineEase()
                        };
                        HighlightDot.BeginAnimation(Border.OpacityProperty, fadeOutAnimation);
                    }
                }
                SetValue(HighlightProperty, value);

                
            }
        }
        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set
            {
                SetValue(LabelProperty, value);
            }
        }
        public DockIcon()
        {
            InitializeComponent();
            PreviewMouseLeftButtonUp += (sender, args) => OnClick();
            this.DataContext = this;
        }
    }
}
