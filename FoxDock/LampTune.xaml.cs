using FoxDock.API;
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
    /// Логика взаимодействия для LampTune.xaml
    /// </summary>
    public partial class LampTune : Window
    {
        private readonly SmartHomeDevice device;
        public LampTune(SmartHomeDevice d)
        {
            InitializeComponent();
            device = d;

            Task.Factory.StartNew(async () =>
            {
                int bright = await device.GetBright();
                int temp = await device.GetTemp();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    BrightSlider.Value = bright;
                    TempSlider.Value = temp;
                });
            });
            //Ставим прозрачность основной сетки в ноль
            MainGrid.Opacity = 0;

            //Функция для выполнения стартовой анимации
            void handler(object s, RoutedEventArgs e)
            {
                //Создаём и выполняем анимацию прозрачности
                DoubleAnimation opacityAnimation = new DoubleAnimation
                {
                    From = MainGrid.Opacity,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new BackEase()
                };
                MainGrid.BeginAnimation(Grid.OpacityProperty, opacityAnimation);

                //Создаём и выполняем анимацию перемещения
                ThicknessAnimation topAnimation = new ThicknessAnimation
                {
                    From = new Thickness(0, 50, 0, 0),
                    To = new Thickness(0, 0, 0, 0),
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new BackEase()
                };
                TunePanel.BeginAnimation(StackPanel.MarginProperty, topAnimation);
            }
            Loaded += handler;
        
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            //Создаём и выполняем анимацию прозрачности
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = MainGrid.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new BackEase()
            };
            MainGrid.BeginAnimation(StackPanel.OpacityProperty, opacityAnimation);

            //Создаём и выполняем анимацию перемещения
            ThicknessAnimation topAnimation = new ThicknessAnimation
            {
                From = new Thickness(0, 0, 0, 0),
                To = new Thickness(0, 50, 0, 0),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new SineEase()
            };
            //По завершению закрываем окно диалога
            topAnimation.Completed += (x, ev) =>
            {
                this.Close();
            };
            TunePanel.BeginAnimation(StackPanel.MarginProperty, topAnimation);
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            device.SetBright((int)((sender as Slider).Value));
        }

        private void TempSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            device.SetTemp((int)((sender as Slider).Value));
        }
    }
}
