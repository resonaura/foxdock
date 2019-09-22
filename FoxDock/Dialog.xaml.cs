using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace FoxDock
{
    /// <summary>
    /// Логика взаимодействия для диалога
    /// </summary>
    public partial class Dialog : Window
    {
        public static AppLanguage.Locale locale = AppLanguage.GetSystemLocale(); //Получаем текущую локаль
        /// <summary>
        /// Функция создания нового диалога
        /// </summary>
        /// <param name="text">Текст диалога</param>
        /// <param name="isRenameWindow">Окно переиминования</param>
        /// <param name="oldName">Старое имя файла</param>
        public Dialog(string text, bool isRenameWindow = false, string oldName = "")
        {
            InitializeComponent(); //Инициализируем компоненты

            //Если окно диалога не должно служить для переименования
            if(!isRenameWindow)
            {
                //Задаём подписи для кнопок и скрываем TextBox для переименования
                NoButton.Content = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.ConfNo, locale);
                YesButton.Content = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.ConfYes, locale);
                RenameBox.Visibility = Visibility.Collapsed;
            } else //Если таки должно
            {
                //Задаём подписи для кнопок, отображаем TextBox для переименования и задаём текст
                NoButton.Content = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.ConfCancel, locale);
                YesButton.Content = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.ConfSave, locale);
                RenameBox.Visibility = Visibility.Visible;
                RenameBox.Text = oldName;
                DialogLabel.Padding = new Thickness(10);
            }
            
            //Задаём текст диалога
            DialogLabel.Text = text;

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
                DialogPanel.BeginAnimation(StackPanel.MarginProperty, topAnimation);
            }
            Loaded += handler;
        }
        public delegate void DialogEvent();
        public bool result = false;
        public event DialogEvent OnResult;

        /// <summary>
        /// Обработчик события нажатия кнопки "Нет"/"Отмена"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            result = false;
            OnResult();
        }
        /// <summary>
        /// Обработка события нажатия кнопки "Да"/"Сохранить"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            result = true;
            OnResult();
        }
        /// <summary>
        /// Функция закрытия диалога
        /// </summary>
        public void CloseDialog()
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
            DialogPanel.BeginAnimation(StackPanel.MarginProperty, topAnimation);
        }
    }
}
