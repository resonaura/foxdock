using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace FoxDock
{
    /// <summary>
    /// Логика взаимодействия для DockIcon.xaml
    /// </summary>
    public partial class DockIcon : UserControl
    {
        //Задаём основные свойства
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(BitmapSource), typeof(DockIcon));
        public static readonly DependencyProperty HighlightProperty = DependencyProperty.Register("Highlight", typeof(bool), typeof(DockIcon));
        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(double), typeof(DockIcon));
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(DockIcon));
        public static readonly DependencyProperty HighlightColorProperty = DependencyProperty.Register("HighlightColor", typeof(string), typeof(DockIcon));
        public static readonly DependencyProperty NotifyCountProperty = DependencyProperty.Register("NotifyCount", typeof(string), typeof(DockIcon));
        public static readonly DependencyProperty IsHoveredProperty = DependencyProperty.Register("IsHovered", typeof(bool), typeof(DockIcon));
        public static readonly DependencyProperty MaskCornerRadiusProperty = DependencyProperty.Register("MaskCornerRadius", typeof(int), typeof(DockIcon));
        public static readonly DependencyProperty MaskPaddingProperty = DependencyProperty.Register("MaskPadding", typeof(double), typeof(DockIcon));
        public static readonly DependencyProperty MaskMarginProperty = DependencyProperty.Register("MaskMargin", typeof(double), typeof(DockIcon));
        public static readonly DependencyProperty MaskBackgroundProperty = DependencyProperty.Register("MaskBackground", typeof(Brush), typeof(DockIcon));
        //Задаём основные события
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

        /// <summary>
        /// Функция для свойства кол-ва уведомлений
        /// </summary>
        public string NotifyCount
        {
            get { return (string)GetValue(NotifyCountProperty); }
            set
            {
                if (value != "0" && NotifyCounter.Opacity == 0)
                {
                    //NotifyCounter.BeginAnimation(OpacityProperty, Animations.SingleAnimation(NotifyCounter.Opacity, 1, .3));
                } else if(value == "0" && NotifyCounter.Opacity == 1)
                {
                    //NotifyCounter.BeginAnimation(OpacityProperty, Animations.SingleAnimation(NotifyCounter.Opacity, 0, .3));
                }
                NotifyLabel.Content = value;
                SetValue(NotifyCountProperty, value);
            }
        }
        /// <summary>
        /// Функция для свойства источника иконки
        /// </summary>
        public BitmapSource Source
        {
            get { return (BitmapSource)GetValue(SourceProperty); }
            set {
                SetValue(SourceProperty, value);
            }
        }
        /// <summary>
        /// Функция для свойства цвета хайлайта
        /// </summary>
        public string HighlightColor
        {
            get { return (string)GetValue(HighlightColorProperty); }
            set {
                switch(value)
                {
                    case "White":
                        HighlightDot.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                        break;
                    case "Gray":
                        HighlightDot.Background = new SolidColorBrush(Color.FromRgb(45, 45, 45));
                        break;
                    case "Black":
                        HighlightDot.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                        break;
                    case "Accent":
                        HighlightDot.Background = SystemParameters.WindowGlassBrush;
                        break;
                }
                
                SetValue(HighlightColorProperty, value);
            }
        }
        public int MaskCornerRadius
        {
            get { return (int)GetValue(MaskCornerRadiusProperty); }
            set
            {
                SetValue(MaskCornerRadiusProperty, value);
            }
        }
        public double MaskPadding
        {
            get { return (double)GetValue(MaskPaddingProperty); }
            set
            {
                SetValue(MaskPaddingProperty, value);
            }
        }
        public double MaskMargin
        {
            get { return (double)GetValue(MaskMarginProperty); }
            set
            {
                SetValue(MaskMarginProperty, value);
            }
        }
        public Brush MaskBackground
        {
            get { return (Brush)GetValue(MaskBackgroundProperty); }
            set
            {
                SetValue(MaskBackgroundProperty, value);
            }
        }
        /// <summary>
        /// Функция для свойства хайлайта
        /// </summary>
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
        /// <summary>
        /// Функция для свойства размера иконки дока
        /// </summary>
        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }
        /// <summary>
        /// Функция для свойства подсказки
        /// </summary>
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set
            {
                SetValue(LabelProperty, value);
            }
        }
        public bool IsHovered
        {
            get { return (bool)GetValue(IsHoveredProperty); }
        }
        /// <summary>
        /// Функция инициализации иконки Дока
        /// </summary>
        public DockIcon()
        {
            InitializeComponent();
            PreviewMouseLeftButtonUp += (sender, args) => OnClick();
            this.DataContext = this;
            this.MouseEnter += (a, b) =>
            {
                SetValue(IsHoveredProperty, true);
            };
            this.MouseLeave += (c, d) =>
            {
                SetValue(IsHoveredProperty, false);
            };

        }
    }
}
