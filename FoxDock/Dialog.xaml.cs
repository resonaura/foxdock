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
using System.Windows.Shapes;

namespace FoxDock
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Dialog : Window
    {
        public static AppLanguage.Locale locale = AppLanguage.GetSystemLocale();
        public Dialog(string text)
        {
            InitializeComponent();

            NoButton.Content = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.ConfNo, locale);
            YesButton.Content = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.ConfYes, locale);

            DialogLabel.Content = text;
            MainGrid.Opacity = 0;
            RoutedEventHandler handler = null;
            handler = (s, e) =>
            {
                if(!MainWindow.cache.disableBlur)
                    NativeMethods.EnableBlur(this);

                DoubleAnimation opacityAnimation = new DoubleAnimation
                {
                    From = MainGrid.Opacity,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new BackEase()
                };
                MainGrid.BeginAnimation(Grid.OpacityProperty, opacityAnimation);


                ThicknessAnimation topAnimation = new ThicknessAnimation
                {
                    From = new Thickness(0, 50, 0, 0),
                    To = new Thickness(0, 0, 0, 0),
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new BackEase()
                };
                DialogPanel.BeginAnimation(StackPanel.MarginProperty, topAnimation);
            };
            Loaded += handler;

            
        }
        public delegate void DialogEvent();
        public bool result = false;
        public event DialogEvent onResult;

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            result = false;
            onResult();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            result = true;
            onResult();
        }

        public void CloseDialog()
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = MainGrid.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new BackEase()
            };
            MainGrid.BeginAnimation(StackPanel.OpacityProperty, opacityAnimation);

            ThicknessAnimation topAnimation = new ThicknessAnimation
            {
                From = new Thickness(0, 0, 0, 0),
                To = new Thickness(0, 50, 0, 0),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new SineEase()
            };
            
            topAnimation.Completed += (x, ev) =>
            {
                this.Close();
            };
            DialogPanel.BeginAnimation(StackPanel.MarginProperty, topAnimation);
        }
    }
}
