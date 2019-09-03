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

            DialogLabel.Text = text;
            MainGrid.Opacity = 0;
            void handler(object s, RoutedEventArgs e)
            {
                if (!Dock.cache.disableBlur)
                {
                    Acryl.EnableBlur(this);
                }

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
            }

            Loaded += handler;

            
        }
        public delegate void DialogEvent();
        public bool result = false;
        public event DialogEvent OnResult;

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            result = false;
            OnResult();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            result = true;
            OnResult();
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
