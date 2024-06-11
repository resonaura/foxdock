using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FoxDock
{
    /// <summary>
    /// Логика взаимодействия для IconPacksListItem.xaml
    /// </summary>
    public partial class IconPacksListItem : UserControl
    {
        public static readonly DependencyProperty Source1Property = DependencyProperty.Register("Source1", typeof(BitmapSource), typeof(IconPacksListItem));
        public static readonly DependencyProperty Source2Property = DependencyProperty.Register("Source2", typeof(BitmapSource), typeof(IconPacksListItem));
        public static readonly DependencyProperty Source3Property = DependencyProperty.Register("Source3", typeof(BitmapSource), typeof(IconPacksListItem));
        public static readonly DependencyProperty Source4Property = DependencyProperty.Register("Source4", typeof(BitmapSource), typeof(IconPacksListItem));
        public static readonly DependencyProperty TxtProperty = DependencyProperty.Register("Txt", typeof(string), typeof(IconPacksListItem));
        public static readonly DependencyProperty AuthorProperty = DependencyProperty.Register("Author", typeof(string), typeof(IconPacksListItem));

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IconPacksListItem));
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }
        void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(IconPacksListItem.ClickEvent);
            RaiseEvent(newEventArgs);
        }
        void OnClick()
        {
            RaiseClickEvent();
        }

        public BitmapSource Source1
        {
            get { return (BitmapSource)GetValue(Source1Property); }
            set
            {
                SetValue(Source1Property, value);
            }
        }
        public BitmapSource Source2
        {
            get { return (BitmapSource)GetValue(Source2Property); }
            set
            {
                SetValue(Source2Property, value);
            }
        }
        public BitmapSource Source3
        {
            get { return (BitmapSource)GetValue(Source3Property); }
            set
            {
                SetValue(Source3Property, value);
            }
        }
        public BitmapSource Source4
        {
            get { return (BitmapSource)GetValue(Source4Property); }
            set
            {
                SetValue(Source4Property, value);
            }
        }
        public string Txt
        {
            get { return (string)GetValue(TxtProperty); }
            set
            {
                SetValue(TxtProperty, value);
            }
        }
        public string Author
        {
            get { return (string)GetValue(AuthorProperty); }
            set
            {
                SetValue(AuthorProperty, value);
            }
        }

        public IconPacksListItem()
        {
            InitializeComponent();
            this.DataContext = this;
            PreviewMouseLeftButtonUp += (sender, args) => OnClick();
        }
    }
}
