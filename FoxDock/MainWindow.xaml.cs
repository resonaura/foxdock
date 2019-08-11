using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.IO.Ports;
using System.IO;
using Microsoft.Win32;
using System.Xml.Serialization;
using System.Drawing;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Windows.Threading;
using System.Drawing.Drawing2D;
using System.Windows.Media.Effects;
using Point = System.Windows.Point;
using Shell32;
using Path = System.IO.Path;
using Color = System.Windows.Media.Color;
using System.ComponentModel;

namespace FoxDock
{
    public partial class MainWindow : Window
    {
        //Подключаем кеш
        public static Cache cache = new Cache();

        //Основные таймеры
        private System.Timers.Timer mainTimer = new System.Timers.Timer();
        private System.Timers.Timer mouseTimer = new System.Timers.Timer();

        //Инициализируем окна
        private Tooltip tooltip = new Tooltip();
        private Settings settings;
        private Dialog dialog;

        //Основные переменные
        public WinStates winStates = new WinStates();
        public static bool lock_slider = true;
        public bool isInitedAS = false;
        public int taskbar_g = 0;
        public double dpiY = 1;
        public static int defsize = 56;
        public int size = (int)(defsize * cache.scaleFactor);
        public bool lockSizeChange = false;
        private string lastTheme = string.Empty;
        private bool isDown;
        private UIElement down_icon;
        private UIElement context_icon;
        private bool isDrop = false;
        private bool dockHidden = false;
        private bool move_lock = false;
        private bool apprunned = false;
        private double oldX = 0;
        private double mouseSpeed = 0;
        private bool isHovered = false;
        private bool startup_animation_completed = false;
        private bool AbsIconDrag = false;
        private bool Draggable_icon_an = true;
        private double fe_max_size = 0;
        private int fe_max_size_el = 0;
        private bool panelIconsAnimated = false;
        private bool panelIconsAnimating = false;
        private DockIcon dr_ic = null;
        private bool movingToTrash = false;

        //Необходимые константы
        public const int SPI_SETDESKWALLPAPER = 20;
        public const int WM_SETTINGCHANGE = 0x001A;
        public const int WM_SYSCOMMAND = 0x0112;
        public const int WM_THEMECHANGED = 0x031A;
        public const int SC_MINIMIZE = 0xF020;
        public const int WM_WININICHANGE = 0x001A;

        /// <summary>
        /// Инициализация дока
        /// </summary>
        public MainWindow()
        {
            InitializeComponent(); //Инициализируем все компоненты

            if (settings == null) settings = new Settings(); //Инициализируем окно настроек, если оно не инициализированно

            //Для защиты от вылета используем try,catch
            try
            {
                WindowAPI.window = this;

                Process[] explorer_p = Process.GetProcessesByName("explorer");
                string explorer_name = explorer_p[0].MainModule.FileVersionInfo.FileDescription;
                ExplorerIcon.Label = explorer_name;
            }
            catch
            {
                consoleLog("Ошибка получения имени проводника...");
            }

            //Получаем значки Проводника и Корзины и задаём их для соответствующих виджетов на Доке
            string epath = Environment.GetEnvironmentVariable("windir") + "\\explorer.exe";
            consoleLog(epath);
            ExplorerIcon.Source = Imaging.CreateBitmapSourceFromHBitmap(GetSystemIcon(epath).ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            TrashIcon.Source = Imaging.CreateBitmapSourceFromHBitmap(GetTrashIcon().ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            //Получаем высоту панели задач
            int taskbar = GetTaskBarH();
            taskbar_g = taskbar;
            
            //Обработчик события успешной загрузки дока
            RoutedEventHandler handler = null;
            handler = (s, e) =>
            {
                //Получаем DPI
                PresentationSource source = PresentationSource.FromVisual(this);
                double dpiY = 1;
                if (source != null)
                {
                    dpiY = source.CompositionTarget.TransformToDevice.M22;
                }

                //Прячем окно подсказки
                tooltip.Hide();
                
                //Убираем событие
                Loaded -= handler;

                //Загружаем кеш
                cache = CacheOperations.LoadCache(cache);

                //Выполняем необходимые действия в зависимости от кеша
                if (cache.disableBlur == false)
                    NativeMethods.EnableBlur(this);
                if (cache.enableStarDust)
                    StarDust.Visibility = Visibility.Visible;

                //Делаем слайдеры настроек активными в зависимости от значений из кеша
                settings.DisableBlurToggle.IsChecked = cache.disableBlur;
                settings.StarDustEnableToggle.IsChecked = cache.enableStarDust;
                settings.EnableTopmostToggle.IsChecked = cache.enableTopmost;
                settings.AHToggle.IsChecked = cache.dockAutoHide;
                settings.Trans_bar.Value = cache.bg_trans;
                settings.ScaleSlider.Value = cache.scaleFactor;
                settings.StartupToggle.IsChecked = cache.runAtStartup;

                //Получаем размер значков в зависимости от масштаба из настроек
                size = (int)(defsize * cache.scaleFactor);

                //Получаем высоту дока и его положение по вертикали
                double new_h = size + size / 2.5;
                this.Height = new_h;
                double new_top = System.Windows.SystemParameters.PrimaryScreenHeight - new_h;
                animateHChange(new_top, new_h);

                //Адаптивный фон дока
                AutoWallUI(true);

                //Применяем логику для кнопки блокировки дока в контекстном меню
                DockLockUpdateUI();

                //Задаём прозрачность фона в зависимости от значения кеша
                App_full_bg.Opacity = cache.bg_trans;

                //Задаём Framerate для всех анимаций
                Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 60 });

                
                //Выполняем стартовую анимацию появления дока
                double top = System.Windows.SystemParameters.PrimaryScreenHeight - this.Height - taskbar_g;
                DoubleAnimation myDoubleAnimation = new DoubleAnimation
                {
                    From = top + this.Height + taskbar_g,
                    To = top,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new PowerEase(),

                };
                Timeline.SetDesiredFrameRate(myDoubleAnimation, 60);
                myDoubleAnimation.Completed += StartUpAnimation_Completed;
                this.BeginAnimation(Window.TopProperty, myDoubleAnimation);
                this.BeginAnimation(OpacityProperty, Animations.OpacityAnimation(0, 1));
            };
            Loaded += handler;

            //Не помню, что эта фигня делает, но она вроде очень нужна
            WindowAPI.MakeWin();

            //Адаптивный фон дока
            AutoWallUI();

            //Событие изменения свойства WindowState
            this.StateChanged += MainWindow_StateChanged;

            //Даём доступ к основному окну в окне Настроек
            settings.window = this;

            //Логика в случае того, если док запущен второй раз
            var exists = System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1;
            if (exists)
            {
                WindowAPI.ShowDesktop(); //Переводим пользователя на Рабочий Стол
                Close(); //Закрываем текущую копию Дока
                tooltip.Close(); //Закрываем окно Подсказки
                settings.Close(); //Закрываем окно настроек

                Environment.Exit(0); //Убиваем процесс
            }

            //Запускаем основной таймер
            mainTimer.Interval = 2000;
            mainTimer.Elapsed += MainTimer_Tick;
            mainTimer.Start();

            //Запускаем таймер для автоматического появления/скрытия Дока в режиме Поверх Всех Окон
            mouseTimer.Interval = 2000;
            mouseTimer.Elapsed += MouseTimer_Elapsed;
            mouseTimer.Start();

            //Загружаем кеш
            cache = CacheOperations.LoadCache(cache);

            //Выполняем логику размера значков
            if (!isInitedAS)
            {
                size = (int)(defsize * cache.scaleFactor);
                settings.ScaleSlider.Value = cache.scaleFactor;
                isInitedAS = true;
            }

            //Добавляем значки из кеша на Док
            if (cache.dock_apps_path != null)
            {
                foreach (string path in cache.dock_apps_path)
                {
                    addIconToPanel(path);
                }
            }

            //Разблокируем слайдер
            lock_slider = false;
        }

        /// <summary>
        /// Функция получения высоты Панели Задач, если она расположена снизу
        /// </summary>
        /// <returns>Высота</returns>
        public int GetTaskBarH()
        {
            WindowAPI.TaskBarLocation location = WindowAPI.GetTaskBarLocation(); //Получаем положение Панели Задач
            if (location == WindowAPI.TaskBarLocation.BOTTOM) //Если она снизу
            {
                return Application.Current.Dispatcher.Invoke(() => (int)(WpfScreen.GetScreenFrom(this).DeviceBounds.Bottom - WpfScreen.GetScreenFrom(this).WorkingArea.Bottom));  //Возвращаем высоту Панели Задач
            } else
            {
                return 0; //Возращаем 0 (да-да, я кеп)
            }
        }

        /// <summary>
        /// Отобразить кнопку блокировки дока в контекстном меню в зависимости от значения кеша
        /// </summary>
        public void DockLockUpdateUI()
        {
            if (cache.dockLock)
            {
                LockDockButton.Header = "Unlock Dock";
                LockDockIcon.Text = "\uE785";
            }
            else
            {
                LockDockButton.Header = "Lock Dock";
                LockDockIcon.Text = "\uE72E";
            }
        }
        
        /// <summary>
        /// Логика таймера положения мыши в режиме Поверх Всех Окон (Topmost)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {

            double y = WindowAPI.GetMousePosition().Y / dpiY; //Получаем положение мыши по Y

            //Если режим Topmost активен
            if (cache.enableTopmost)
            {
                //Получаем положение дока по вертикале относительно экрана
                double top = System.Windows.SystemParameters.PrimaryScreenHeight - (size + size / 2.5) - taskbar_g;

                //Если курсор находится в триггер-зоне экрана
                if (y >= System.Windows.SystemParameters.PrimaryScreenHeight - 20)
                {

                    if (dockHidden)
                        SafeInvoke(() => ShowDock()); //Отображаем Док
                }
                else //Если курсор находится вне триггер-зоны
                {
                    if (y < System.Windows.SystemParameters.PrimaryScreenHeight - (System.Windows.SystemParameters.PrimaryScreenHeight - top))
                    {
                        //Если пользователь находится на Рабочем Столе
                        if (WindowAPI.IsOnDesktop())
                        {
                            if (dockHidden)
                                SafeInvoke(() => ShowDock()); //Отображаем док
                        }
                        else //В обратном случае
                        {
                            if (!dockHidden && cache.dockAutoHide)
                            {
                                SafeInvoke(() => HideDock()); //Скрываем док
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Обработчик события изменения параметра WindowState главного окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (!cache.enableTopmost) //Если не включен режим Поверх Всех Окон
                WindowAPI.SendToBack(this); //Отправляем Док на задний план
        }
        

        //Логика обработки событий изменения Настроек (тут твориться какая-то неведомая херня, которую я стырил с StackOverflow, но вроде оно работает как надо)
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //Короче. Если типо происходит какое-то событие в системе и это событие - изменение настроек системы, то выполняем логику адаптивного фона
            //Это нужно для того, чтобы если юзверь сменит тему в системе Док подстроился под эту тему.
            //От така фігня, малята

            if (msg == WM_SETTINGCHANGE)
            {
                AutoWallUI();
            }

            return IntPtr.Zero;
        }
        /// <summary>
        /// Логика добавления значка в Док
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        private void addIconToPanel(string path)
        {
            //Создаём иконку
            object icn = new object();
            Icon icon = icn as Icon;

            //Пробуем получить значок этого файла или папки (вообще похер)
            try
            {
                icon = GetSystemIcon(path); //Получаем значок
            }
            catch (Exception ex) //Если словили ошибку
            {
                icon = null; //Убиваем значок самым жестоким способом...
                Debug.WriteLine(ex.Message + " - ошибка получения значка приложения"); //Выводим в консоль сообщение об ошибке
            }

            //Если значок ещё живой (а вдруг?)
            if (icon != null)
            {
                Bitmap bitmap = icon.ToBitmap(); //Переводим его в битмап


                //Создаём новый DockIcon и присваиваем ему все события
                DockIcon dockIcon = new DockIcon();
                dockIcon.Source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                dockIcon.MouseDown += Img_MouseDown;
                dockIcon.MouseEnter += Img_MouseEnter;
                dockIcon.MouseLeave += Img_MouseLeave;
                dockIcon.MouseMove += Img_MouseMove;
                dockIcon.MouseUp += Img_MouseUp;

                //Получаем свободный индекс для добавления нового элемента
                int index = MainPanel.Children.Count;
                MainPanel.Children.Insert(index, dockIcon);

                //В новом потоке спустя 300 мс. обновляем ширину дока
                Task.Factory.StartNew(() =>
                {
                    System.Threading.Thread.Sleep(300);
                    SafeInvoke(() => UpdateDockWidth());
                });


            }
        }
        /// <summary>
        /// Выполнение логики передвиженыя мыши по иконке
        /// </summary>
        /// <param name="sender"></param>
        private void Img_MouseMoveDo(object sender)
        {
            DockIcon image = sender as DockIcon; //Получаем значок из сендера

            Label label = tooltip.app_hint; //Делаем особый клон текущей подсказки 

            //Подстраиваем ширину клона в зависимости от содержания
            label.Measure(new System.Windows.Size(Double.PositiveInfinity, Double.PositiveInfinity));
            label.Arrange(new Rect(label.DesiredSize));
            
            //Получаем ширину клона
            double real_hint_width = label.ActualWidth;

            //Получаем смещение
            DockIcon uIElement = image;
            var element_Visual_Relative = uIElement.TransformToVisual((Visual)Content);
            System.Windows.Point offset = element_Visual_Relative.Transform(new System.Windows.Point(0, 0));
            var offsetX = offset.X;
            
            //Высчитываем смещение подсказки по левому краю
            double left = offsetX + (image.Size) / 2 - (real_hint_width / 2) + 30 + 5;

            //Изменяем позицию и размер подсказки
            animateHint(left, 0, 0);
        }
        /// <summary>
        /// Обработчик события перемещения мыши по иконке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Img_MouseMove(object sender, MouseEventArgs e)
        {
            Img_MouseMoveDo(sender);
        }
        
        /// <summary>
        /// Функция на случай того если определённая программа запущена
        /// </summary>
        /// <param name="image">Значок программы</param>
        private void ifAppRunned(DockIcon image)
        {
            if (!move_lock)
            {
                image.Highlight = true;
            }

        }
        /// <summary>
        /// Функция на случай того если определённая программа не запущена
        /// </summary>
        /// <param name="image">Значок программы</param>
        private void ifNotAppRunned(DockIcon image)
        {
            if (!move_lock)
            {
                image.Highlight = false;
            }
        }

        /// <summary>
        /// Функция поиска подстроки в строке
        /// </summary>
        /// <param name="substr">Подстрока</param>
        /// <param name="str">Строка</param>
        /// <returns></returns>
        private bool substrInStr(string substr, string str)
        {
            return str.IndexOf(substr) > -1;
        }
        /// <summary>
        /// Функция получения названия исполняемого файла из пути
        /// </summary>
        /// <param name="path">Путь</param>
        /// <returns>Название исполняемого файла</returns>
        private string appFromPath(string path)
        {
            string app_name = System.IO.Path.GetFileNameWithoutExtension(path); //Получаем файлнейм

            

            //Возвращаем правильный файлнейм
            return app_name;
        }
        /// <summary>
        /// Проверка того запущено ли приложение
        /// </summary>
        /// <param name="path">Путь</param>
        /// <returns>Результат</returns>
        private bool CheckIfAppRunned(string path)
        {
            string app_path = getRealAppPath(path);
            string app_name = appFromPath(app_path);

            return System.Diagnostics.Process.GetProcessesByName(app_name).Length >= 1;
        }
        /// <summary>
        /// Функция уничтожения процесса
        /// </summary>
        /// <param name="path">Путь</param>
        private void killProcess(string path)
        {
            string app_path = getRealAppPath(path);
            string app_name = appFromPath(app_path);

            System.Diagnostics.Process.GetProcessesByName(app_name)[0].Kill();
        }
        /// <summary>
        /// Логика проверки активности приложений из Дока
        /// </summary>
        private void AppsActiveLogic()
        {
            try
            {
                int i = 0;
                foreach (string path in cache.dock_apps_path)
                {

                    var already_runned = CheckIfAppRunned(path);

                    try
                    {
                        SafeInvoke(() =>
                        {
                            if (already_runned)
                            {
                                if (i < MainPanel.Children.Count && i >= 0) ifAppRunned(MainPanel.Children[i] as DockIcon);
                            }
                            else
                            {
                                if (i < MainPanel.Children.Count && i >= 0) ifNotAppRunned(MainPanel.Children[i] as DockIcon);
                            }
                        });

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message + " beda #1");
                    }


                    i++;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString() + " beda #2");
            }
        }
        /// <summary>
        /// Функция отображения Док бара
        /// </summary>
        public void ShowDock()
        {

            double top = System.Windows.SystemParameters.PrimaryScreenHeight - this.Height - taskbar_g;
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = top + this.Height + taskbar_g,
                To = top,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new PowerEase(),

            };
            myDoubleAnimation.Completed += (x, y) =>
            {
                lockSizeChange = false;
                dockHidden = false;
            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation, 60);
            this.BeginAnimation(Window.TopProperty, myDoubleAnimation);
            this.BeginAnimation(OpacityProperty, Animations.OpacityAnimation(this.Opacity, 1));
        }
        /// <summary>
        /// Функция скрытия Док бара
        /// </summary>
        public void HideDock()
        {
            dockHidden = true;
            lockSizeChange = true;
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = this.Top,
                To = this.Top + this.Height + taskbar_g,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new PowerEase(),

            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation, 60);
            this.BeginAnimation(Window.TopProperty, myDoubleAnimation);
            this.BeginAnimation(OpacityProperty, Animations.OpacityAnimation(this.Opacity, 0));
        }
        /// <summary>
        /// Фунция безопасного инвока
        /// </summary>
        /// <param name="act"></param>
        private void SafeInvoke(Action act)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(act);
            }
            catch
            {
                consoleLog("Invoke error");
            }
        }
        
        /// <summary>
        /// Логика основного таймера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainTimer_Tick(object sender, EventArgs e)
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows(); //Получаем все окна проводника

            //В зависимости от кол-ва окон проводника отображаем или скрываем хайлайт под виджетом проводника
            if(shellWindows.Count > 0)
            {
                SafeInvoke(() => ExplorerIcon.Highlight = true);
            } else
            {
                SafeInvoke(() => ExplorerIcon.Highlight = false);
            }

            //Переменная для того, чтобы обозначить существования корзины среди окон проводника
            bool rb_matches = false;

            //Проходимся по всем окнам проводника
            foreach (SHDocVw.InternetExplorer ie in shellWindows)
            {
                //Если есть в кармане пачка... Ой, не то пальто... Кхм. Если текущее окно - окно корзины задаём значение переменной на положительное
                if(ie.LocationName == "Recycle Bin" || ie.LocationName == "Корзина" || ie.LocationName == "Кошик")
                {
                    rb_matches = true;
                }
            }

            //Если есть корзина
            if(rb_matches)
            {
                SafeInvoke(() => TrashIcon.Highlight = true); //Делаем хайлайт активным
            } else //Иначе
            {
                SafeInvoke(() => TrashIcon.Highlight = false); //Делаем хайлайт неактивным
            }

            //Пытаемся получить и задать значок Корзины
            try
            {
                Application.Current.Dispatcher.Invoke(() => TrashIcon.Source = Imaging.CreateBitmapSourceFromHBitmap(GetTrashIcon().ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
            }
            catch
            {
                consoleLog("Ошибка получения и задания значка Корзины");
            }

            //Получаем высоту Панели Задач в случае того, если она расположена снизу
            int taskbar = GetTaskBarH();
            taskbar_g = taskbar;

            //Если нету блокировки движений
            if (!move_lock)
            {
                //Выполняем логику активности приложений в новом потоке
                Task.Factory.StartNew(() =>
                {
                    AppsActiveLogic();
                });

                //Пробуем анимировать положение Дока по вертикали
                try
                {
                    SafeInvoke(() => animateHChange(System.Windows.SystemParameters.PrimaryScreenHeight - this.Height, this.Height));
                }
                catch (Exception ex)
                {
                    //В случае ошибки - выводим её в консоль
                    Debug.WriteLine(ex.Message + " beda #3");
                }
            }
            
            //Если стартовая анимация выполнена и не заблокировано изменение размера, то выполняем логику автоматической позиции Дока и подсказки
            try
            {
                if (startup_animation_completed && !lockSizeChange)
                    SafeInvoke(() => AutoTooltipAndDockPosition());
            }
            catch
            {
                consoleLog("Ошибка расчёта автоматической позиции Дока и подсказки");
            }

        }
        /// <summary>
        /// Функция автоматической позиции Дока и подсказки
        /// </summary>
        public void AutoTooltipAndDockPosition()
        {
            if (!startup_animation_completed) return; //Если стартовая анимация не выполнена - останавливаем выполнение функции

            //Высчитываем положение подсказки
            double top = System.Windows.SystemParameters.PrimaryScreenHeight - this.Height - taskbar_g;
            tooltip.Top = top - tooltip.Height;

            //Меняем текущее положение окна по вертикали
            DoubleAnimation fastda = new DoubleAnimation
            {
                From = this.Top,
                To = top,
                Duration = TimeSpan.FromMilliseconds(0)
            };
            if (!dockHidden)
                this.BeginAnimation(TopProperty, fastda);
        }
        /// <summary>
        /// Функция адаптивного фона
        /// </summary>
        /// <param name="upd"></param>
        public void AutoWallUI(bool upd = false)
        {
            Task.Factory.StartNew(() =>
            {
                //Получаем из реестра тему
                var wpReg = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", false);
                var theme = wpReg.GetValue("SystemUsesLightTheme").ToString();

                //Закрываем работу с реестра
                wpReg.Close();

                SafeInvoke(() =>
                {
                    //Комбинируем основные значки с виджетами
                    List<DockIcon> combined = new List<DockIcon>();
                    foreach (DockIcon di in MainPanel.Children)
                    {
                        combined.Add(di);
                    }
                    foreach (DockIcon di in AIcons.Children)
                    {
                        combined.Add(di);
                    }

                    //Выполняем стандартную анимацию анимации темы
                    Animations.ThemeAnimate(theme, App_bg, tooltip, WhiteOverlay, BlackOverlay, combined);
                });
            });
        }

        /// <summary>
        /// Функция логирования в консоль
        /// </summary>
        /// <param name="cdd">Объект</param>
        public void consoleLog(object cdd)
        {
            Debug.WriteLine(cdd);
        }

        /// <summary>
        /// Функция получения значка Корзины
        /// </summary>
        /// <returns>Значок</returns>
        private static Icon GetTrashIcon()
        {
            //Снова неведомая херня с взаимодействием с Win32 API. Писал в состоянии алкогольного опьянения...
            //На всякий случай, чтобы не еб#нуло ошибку всю логику помещаем в try, catch
            try
            {
                Win32E.SHFILEINFO psfi = new Win32E.SHFILEINFO();
                
                Guid riid = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
                Win32E.IImageList ppv;
                Win32E.SHGetImageList(4, ref riid, out ppv);
                IntPtr picon = IntPtr.Zero;
                int flags = 0;

                Shell shell = new Shell();
                Folder recycleBin = shell.NameSpace(10);
                int itemsCount = recycleBin.Items().Count;

                int i = 31;
                if(itemsCount > 0)
                {
                    i = 32;
                }

                ppv.GetIcon(i, flags, ref picon);
                Icon icon = (Icon)System.Drawing.Icon.FromHandle(picon).Clone();
                Win32E.DestroyIcon(psfi.hIcon);
                return icon;
            }
            catch (Exception ex)
            {
                //Если таки ошибка - выводим её в консоль
                Debug.WriteLine(ex.Message + " beda #4");
            }
            return (Icon)null;
        }
        /// <summary>
        /// Функция получения значка по пути
        /// </summary>
        /// <param name="path">Путь к файлу/папке</param>
        /// <returns></returns>
        private static Icon GetSystemIcon(string path)
        {
            //Тут всё почти так же, как и в предыдущей функции. Мне лень описывать)
            try
            {
                Win32E.SHFILEINFO psfi = new Win32E.SHFILEINFO();
                int dwFileAttributes = 2048;
                Win32E.SHGFI uFlags = Win32E.SHGFI.SHGFI_SYSICONINDEX;
                if (Win32E.SHGetFileInfo(path, dwFileAttributes, out psfi, (uint)Marshal.SizeOf((object)psfi), uFlags) == 0)
                    return (Icon)null;
                int i = psfi.iIcon;
                Guid riid = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
                Win32E.IImageList ppv;
                Win32E.SHGetImageList(4, ref riid, out ppv);
                IntPtr picon = IntPtr.Zero;
                int flags = 0;
                ppv.GetIcon(i, flags, ref picon);
                Icon icon = (Icon)System.Drawing.Icon.FromHandle(picon).Clone();
                Win32E.DestroyIcon(psfi.hIcon);
                return icon;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " beda #4");
            }
            return (Icon)null;
        }
        
        /// <summary>
        /// Функция обработки успешного перетаскивания на Док
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_Drop(object sender, DragEventArgs e)
        {
            if (cache.dockLock || movingToTrash) return; //Если не включена блокировка значков или идёт перемещение в корзину
            isDrop = false; //Задаём отрицательное значение переменной, которая сигнализирует о том, что происходит перетаскивание
            
            //Если на Док перетащили файлы/папки, а не какую-то иную фигню
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                try
                {
                    //Получаем файлы/папки
                    string[] s = (string[])e.Data.GetData(DataFormats.FileDrop);
                    
                    //Проходимся по всем объектам
                    foreach (string fn in s)
                    {
                        //Получаем название объекта
                        string lname = System.IO.Path.GetFileNameWithoutExtension(fn);

                        //Если такого объекта уже нету в Доке
                        if (cache.dock_apps_path.IndexOf(fn) == -1)
                        {
                            //Добавляем в кеш и на панель
                            cache.dock_apps.Add(lname);
                            cache.dock_apps_path.Add(fn);
                            CacheOperations.StoreCache(cache);

                            addIconToPanel(fn);
                        }

                    }
                }
                catch (Exception ex)
                {
                    //Выводим сообщение об ошибке
                    Debug.WriteLine(ex.Message + " beda #5");
                }
            }
        }

        /// <summary>
        /// Проверка файла на то, является ли он ярлыком
        /// </summary>
        /// <param name="path">Путь к ярлыку</param>
        /// <returns></returns>
        public bool IsLink(string path)
        {

            string pathOnly = System.IO.Path.GetDirectoryName(path);
            string filenameOnly = System.IO.Path.GetFileName(path);

            
            Type shellAppType = Type.GetTypeFromProgID("Shell.Application");

            Object shell = Activator.CreateInstance(shellAppType);
            Folder folder = (Shell32.Folder)shellAppType.InvokeMember("NameSpace",
            System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { pathOnly });

            FolderItem folderItem = folder.ParseName(filenameOnly);

            if (folderItem != null)
            {
                return folderItem.IsLink;
            }
            return false; // not found
        }
        /// <summary>
        /// Получение исходного пути ярлыка
        /// </summary>
        /// <param name="shortcutFilename">Путь к ярлыку</param>
        /// <returns></returns>
        public static string GetShortcutTarget(string shortcutFilename)
        {
            
            string pathOnly = System.IO.Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = System.IO.Path.GetFileName(shortcutFilename);

            if(File.Exists(Path.GetTempPath() + "\\" + filenameOnly))
            {
                pathOnly = Path.GetTempPath();
            } else
            {
                File.Copy(shortcutFilename, Path.GetTempPath() + "\\" + filenameOnly);
                pathOnly = Path.GetTempPath();
            }

            Type shellAppType = Type.GetTypeFromProgID("Shell.Application");

            Object shell = Activator.CreateInstance(shellAppType);
            Folder folder = (Shell32.Folder)shellAppType.InvokeMember("NameSpace",
            System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { pathOnly });

            Shell32.FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                if (folderItem.IsLink)
                {
                    try
                    {
                        Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
                        return link.Path;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message + " beda #7");
                    }

                }
                return shortcutFilename;
            }
            return string.Empty;  // not found
            
            
        }
        /// <summary>
        /// Получение пути к приложению
        /// </summary>
        /// <param name="path">Путь</param>
        /// <returns>Путь</returns>
        public string getRealAppPath(string path)
        {
            if (IsLink(path)) //Если путь - ярлык
            {
                return GetShortcutTarget(path); //Получаем путь из ярлыка
            }
            else
            {
                return path; //В противном случае - возвращаем - путь который был
            }

        }
        /// <summary>
        /// Обработка события Mouse_Up значка дока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Img_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DockIcon img = sender as DockIcon; //Получаем текущий значок

            int current_index = MainPanel.Children.IndexOf(img); //Получаем его индекс

            if (down_icon != null && isDown && !isDrop) //Если нету перетаскивания на Док и до этого левая кнопка мыши была зажата
            {
                //Если зажатый значок был тем же, что и текущий
                if (down_icon == img)
                {
                    //Проучаем путь к приложению
                    string app_path = getRealAppPath(cache.dock_apps_path[current_index]);
                    string app_name = appFromPath(app_path);

                    //Проверяем запущено ли оно
                    var already_runned = System.Diagnostics.Process.GetProcessesByName(app_name).Count() >= 1;

                    //Переводим прозрачность основной панели в нормальное состояние
                    MainPanel.Opacity = 1;
                    apprunned = true;

                    //Для защиты от вылета затачиваем весь следующий код в try, catch
                    try
                    {
                        //Если приложение запущено
                        if (already_runned)
                        {
                            //Получаем связанные процессы
                            Process[] process = System.Diagnostics.Process.GetProcessesByName(app_name);

                            //Получаем их количество
                            int proc_c = process.Length;

                            //Переменная для хранения кол-ва реальных окон
                            int real_windows = 0;
                            
                            //Проходимся по всем связанным процессам
                            for (int i = 0; i < proc_c; i++)
                            {
                                //Получаем текущий
                                Process proc = process[i];

                                //Если основного окна не существует либо приложение - проводник
                                if (proc.MainWindowHandle == IntPtr.Zero || app_name == "explorer")
                                {
                                    //Если процесс последний и кол-во окон - 0 или приложение - проводник
                                    if (i == proc_c - 1 && real_windows == 0 || app_name == "explorer")
                                        //Запускаем приложение по-новой
                                        System.Diagnostics.Process.Start(cache.dock_apps_path[current_index]);
                                }
                                else //В противном случае
                                {
                                    //+1 к реальным окнам
                                    real_windows++;

                                    //Если окно скрыто
                                    if (WindowAPI.IsIconic(proc.MainWindowHandle))
                                    {
                                        //Показываем и делаем его активным
                                        WindowAPI.SetForegroundWindow(proc.MainWindowHandle);
                                        WindowAPI.ShowWindowAsync(proc.MainWindowHandle, 9);

                                    }
                                    else //Если окно открыто
                                    {
                                        //Скрываем его
                                        WindowAPI.ShowWindowAsync(proc.MainWindowHandle, WindowAPI.SW_MINIMIZE);

                                    }
                                }
                            }
                        }
                        else //Если приложение не запущено
                        {
                            //Запускаем его
                            System.Diagnostics.Process.Start(cache.dock_apps_path[current_index]);
                        }

                    }
                    catch (Exception ex)
                    {
                        //Если совсем пипец - выводим сообщение об этом в консоль
                        Debug.WriteLine(ex.Message + " beda #8");
                    }
                }
            }
            //Переводим переменную активности нажатия в отрицательное состояние
            isDown = false;

            //Выполняем анимацию прозрачности, чтобы вернуть её в нормальное состояние для значка
            img.BeginAnimation(DockIcon.OpacityProperty, Animations.OpacityAnimation(img.Opacity, 1, 0.2));

        }
        /// <summary>
        /// Функция задания активного значка для контекстного меню
        /// </summary>
        /// <param name="img">Значок</param>
        public void SetContextIcon(DockIcon img)
        {
            //Отображаем пункт контекстного меню, отвечающий за удаление значка из Дока
            RemoveFromDockButton.Opacity = 1;
            RemoveFromDockButton.IsEnabled = true;

            //Получаем имя и путь текущего значка
            string current_name = cache.dock_apps[MainPanel.Children.IndexOf(img)];
            string current_path = cache.dock_apps_path[MainPanel.Children.IndexOf(img)];

            //Задаём кнопке закрытия проги в контекстном меню нужное имя
            CloseSomeAppButton.Header = "Close " + current_name;

            //Проверяем запущено ли приложение
            bool apprunned = CheckIfAppRunned(current_path);

            //В зависимости от результата делаем активной/неактивной кнопку закрытия приложения
            if (apprunned)
            {
                CloseSomeAppButton.IsEnabled = true;
            }
            else
            {
                CloseSomeAppButton.IsEnabled = false;
            }

            //Задаём текущий значок как контекстный
            context_icon = img;
        }
        
        /// <summary>
        /// Обработчик события нажатия на значок Дока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            apprunned = false; //Приводим индикацию запущенности приложения в исходное состояние

            //Если нажатие было левой клавишей мыши
            if (e.LeftButton == MouseButtonState.Pressed)
                isDown = true; //Обозначаем нажатие

            //Получаем значок из сендера
            DockIcon img = sender as DockIcon;

            //Если была нажата правая клавиша
            if (e.RightButton == MouseButtonState.Pressed)
            {
                //Делаем значок контекстным
                SetContextIcon(img);
            }

            //Обозначаем, что именно наш значок был зажат
            down_icon = img;
            
            //Делаем значок полупрозрачным
            img.BeginAnimation(DockIcon.OpacityProperty, Animations.OpacityAnimation(img.Opacity, 0.5, 0.2));
        }
        
        /// <summary>
        /// Логика обработки события выхода курсора за рамки иконки
        /// </summary>
        /// <param name="sender"></param>
        private void Img_MouseLeaveDo(object sender)
        {
            isHovered = false; //Обозначаем, что ни один значок не наведён
        }
        /// <summary>
        /// Обработка события выхода курсора за рамки иконки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Img_MouseLeave(object sender, MouseEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                SafeInvoke(() => Img_MouseLeaveDo(sender));
            });
        }
        /// <summary>
        /// Применяем изменение размера
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="e"></param>
        /// <param name="dur"></param>
        private void animateSizeChange(int start, int end, DockIcon e, double dur = 0.1)
        {
            e.Size = end;
        }
        /// <summary>
        /// Анимация изменения ширины
        /// </summary>
        /// <param name="start">Бывшее значение</param>
        /// <param name="end">Новое значение</param>
        /// <param name="e">Окно</param>
        public void animateWChange(int start, int end, Window e)
        {
            if (lockSizeChange) return;
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = start,
                To = end,
                Duration = TimeSpan.FromSeconds(0),
            };
            e.BeginAnimation(Window.WidthProperty, myDoubleAnimation);
        }
        /// <summary>
        /// Анимация изменения высоты и смещения относительно верха экрана
        /// </summary>
        /// <param name="top">Смещение</param>
        /// <param name="height">Высота</param>
        public void animateHChange(double top, double height)
        {
            if (lockSizeChange) return;
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = this.Top,
                To = top,
                Duration = TimeSpan.FromSeconds(0)
            };
            DoubleAnimation myDoubleAnimation2 = new DoubleAnimation
            {
                From = this.Height,
                To = height,
                Duration = TimeSpan.FromSeconds(0)
            };
            if (startup_animation_completed)
            {
                this.BeginAnimation(Window.TopProperty, myDoubleAnimation);
            }

            this.BeginAnimation(Window.HeightProperty, myDoubleAnimation2);

        }
        /// <summary>
        /// Коплексная нимация подсказки
        /// </summary>
        /// <param name="left_pos">Позиция по левому краю</param>
        /// <param name="top_pos">Позиция по верху</param>
        /// <param name="dur">Длительность анимации</param>
        private void animateHint(double left_pos, double top_pos, double dur = 0.1)
        {
            ThicknessAnimation thicknessAnimation = new ThicknessAnimation
            {
                From = tooltip.app_hint.Margin,
                To = new Thickness(left_pos, top_pos, 0, 0),
                Duration = TimeSpan.FromSeconds(dur),
                EasingFunction = new SineEase()
            };

            Timeline.SetDesiredFrameRate(thicknessAnimation, 100);

            tooltip.app_hint.BeginAnimation(Window.MarginProperty, thicknessAnimation);
        }
        /// <summary>
        /// Получения направления мыши
        /// </summary>
        /// <returns></returns>
        private string getMouseDirection()
        {
            string xDirection = string.Empty;

            if (oldX < System.Windows.Forms.Cursor.Position.X - this.Left)
            {
                xDirection = "right";
            }
            else
            {
                xDirection = "left";
            }
            mouseSpeed = System.Windows.Forms.Cursor.Position.X - this.Left - oldX;
            if (xDirection == "left") mouseSpeed = -mouseSpeed;
            oldX = System.Windows.Forms.Cursor.Position.X - this.Left;
            return xDirection;
        }
        /// <summary>
        /// Логика обработки события наведения на значок
        /// </summary>
        /// <param name="sender"></param>
        private void Img_MouseEnterDo(object sender)
        {
            if (!move_lock)
            {
                //Получаем текущий значок
                DockIcon img = sender as DockIcon;

                //Получаем текущий индекс
                int current_index = MainPanel.Children.IndexOf(img);

                string current_label = string.Empty; //Переменная для текущей подсказки

                if (current_index != -1) //Если индекс определён
                {
                    current_label = cache.dock_apps[current_index]; //Получаем текущую подсказку
                }
                else
                {
                    current_label = img.Label; //В противном случае берём подсказку из свойства объекта
                }

                //Если объекте подсказки текст не сходится с текстом подсказки из значка
                if ((string)tooltip.app_hint.Content != current_label)
                {
                    //Обновляем текст в объекте подсказки
                    tooltip.app_hint.Content = current_label;

                    //Запускаем анимацию прозрачности подсказки
                    tooltip.app_hint.BeginAnimation(OpacityProperty, Animations.OpacityAnimation(0, 1, 0.2));
                }
                //Отображаем подсказку
                tooltip.app_hint.Visibility = Visibility.Visible;

                //Если индекс существует
                if (current_index != -1)
                {
                    //Задаем текущий значок как контекстный
                    context_icon = img;
                }
                
                //Создаём клон подсказки
                Label label = tooltip.app_hint;

                //Подстраиваем ширину клона под содержание
                label.Measure(new System.Windows.Size(Double.PositiveInfinity, Double.PositiveInfinity));
                label.Arrange(new Rect(label.DesiredSize));

                //Получаем ширину из клона
                double real_hint_width = label.ActualWidth;

                //Получаем смещение
                DockIcon uIElement = img;
                var element_Visual_Relative = uIElement.TransformToVisual((Visual)Content);
                System.Windows.Point offset = element_Visual_Relative.Transform(new System.Windows.Point(0, 0));
                var offsetX = offset.X;
                
                //Получаем смещение по левому краю для подсказки
                double left = offsetX + (img.Size) / 2 - (real_hint_width / 2) + 30 + 5;
                
                animateHint(left, 0, 0);
            }
        }
        /// <summary>
        /// Обработка события наведения на значок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Img_MouseEnter(object sender, MouseEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                isHovered = true;
                SafeInvoke(() => Img_MouseEnterDo(sender));
            });
        }
        /// <summary>
        /// Обработка появления перетаскивания на Док
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_Drop_Enter(object sender, DragEventArgs e)
        {
            //Если есть данные нужного формата (а не фигня всякая)
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //Если значки дока не заблокированы
                if(!cache.dockLock)
                {
                    e.Effects = DragDropEffects.Copy;
                    isDrop = true;
                } else //В противном случае
                {
                    e.Effects = DragDropEffects.None;
                }
                
            }
        }
        /// <summary>
        /// Функция логики закрытия Дока
        /// </summary>
        private void ExitDock()
        {
            //Останавливаем таймеры
            mainTimer.Stop();
            mouseTimer.Stop();

            //Блокируем док
            move_lock = true;

            //Делаем окно неактивным
            this.IsEnabled = false;

            //Создаём анимацию закрытия Дока
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = this.Top,
                To = this.Top + this.Height + taskbar_g,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new PowerEase(),

            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation, 60);
            myDoubleAnimation.Completed += CloseAnimation_Completed;
            myDoubleAnimation.RemoveRequested += CloseAnimation_Completed;

            //Закрытие окон подсказки и настроек
            tooltip.Close();
            settings.Close();

            //Блокируем изменение размера
            lockSizeChange = true;

            //Анимируем закрытие Дока
            this.BeginAnimation(Window.TopProperty, myDoubleAnimation);
            this.BeginAnimation(OpacityProperty, Animations.OpacityAnimation(this.Opacity, 0));
        }
        /// <summary>
        /// Обработка события нажатия кнопки закрытия Дока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExitDock();
        }
        /// <summary>
        /// Обработка события завершения анимации перед закрытием
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseAnimation_Completed(object sender, EventArgs e)
        {
            Close();
            Environment.Exit(0);
        }

        /// <summary>
        /// Обработка события закрытия окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            //Обозначаем то, что значки панели не анимированны
            panelIconsAnimated = false;

            //Очищяем значок переноса
            Draggable_icon.Source = null;
            dr_ic = null;

            //Если нету блокировки движения значков Дока
            if (!move_lock)
            {
                //Комбинируем пользовательские значки с виджетами
                List<DockIcon> combined = new List<DockIcon>();
                foreach (DockIcon di in MainPanel.Children)
                {
                    combined.Add(di);
                }
                foreach (DockIcon di in AIcons.Children)
                {
                    combined.Add(di);
                }
                
                //Проходимся по всем значкам из комбинированных
                foreach (DockIcon img_cur in combined)
                {
                    //Если текущий значок существует
                    if (img_cur != null)
                    {
                        //Анимируем его в нормальное состояние и размер
                        img_cur.BeginAnimation(DockIcon.OpacityProperty, Animations.OpacityAnimation(img_cur.Opacity, 1, 0.2));
                        DoubleAnimation doubleAnimation = new DoubleAnimation
                        {
                            From = img_cur.Size,
                            To = size,
                            Duration = TimeSpan.FromMilliseconds(200),
                            EasingFunction = new SineEase()
                        };
                        doubleAnimation.Completed += (x, y) =>
                        {
                            tooltip.Hide();
                        };
                        img_cur.BeginAnimation(DockIcon.SizeProperty, doubleAnimation);
                    }
                }
            }
            //Если есть зажатая иконка
            if (down_icon != null && isDown && !apprunned && !cache.dockLock)
            {
                //Запрашиваем её удаление
                DockIcon dimg = down_icon as DockIcon;
                RemoveFromDock(dimg);
                MainPanel.Opacity = 1;
            }
            //Очищаем подсказку
            tooltip.app_hint.Content = string.Empty;
            
            //Анимируем подсказку
            animateHint(tooltip.app_hint.Margin.Left - 100, -1, 0);
            tooltip.app_hint.Visibility = Visibility.Hidden;

            foreach (DockIcon img_cur in MainPanel.Children)
            {
                //Если текущий значок существует - восстанавливаем его оригинальный размер
                if (img_cur != null)
                {
                    animateSizeChange((int)img_cur.Size, (int)(size), img_cur);
                }
            }
        }
        public void UpdateDockWidth()
        {
            //Получаем суммарное количество значков
            int summary_icons_count = (MainPanel.Children.Count + AIcons.Children.Count);

            //Считаем необходимые значения для получения ширины дока
            double summary_icons_width = summary_icons_count * size;
            double summary_icons_margin = summary_icons_count * 10;
            double separator_size_and_margin = 2 + 20;
            double free_space = size / 2;

            //Дополнительная логика для особых вариантов размера
            double addt = 0;
            if (size < 56) addt = size / 2;
            if (size < 53) addt = size;

            //Считаем ширину Дока
            double new_width = summary_icons_width + summary_icons_margin + separator_size_and_margin + free_space + addt;
            if (new_width < 100) new_width = 100;

            //Задаём ширину Дока
            this.Width = new_width;
        }
        /// <summary>
        /// Функция запроса удаления значка из дока
        /// </summary>
        /// <param name="image">Значок</param>
        private void RemoveFromDock(DockIcon image)
        {
            //Создаём диалог
            if (dialog == null) dialog = new Dialog("Are you sure you want to remove this item from Dock?");

            //Если диалог существует
            if (dialog != null)
            {
                //Делаем значок полупрозрачным
                image.BeginAnimation(StackPanel.OpacityProperty, Animations.OpacityAnimation(image.Opacity, 0.2, 0.3));

                //Для безопасности выполняем код в конструкции try, catch
                try
                {
                    dialog.Show(); //Отображаем диалог
                    dialog.onResult += () => //Если есть результат диалога
                    {
                        //Если диалог всё ещё существует
                        if (dialog != null)
                        {
                            //Если пользователь согласился
                            if (dialog.result == true)
                            {
                                //Получаем индекс текущего значка
                                int dindex = MainPanel.Children.IndexOf(image);

                                //Если индекс находится в рамках массива значков
                                if (dindex < cache.dock_apps.Count)
                                    cache.dock_apps.RemoveAt(dindex);
                                if (dindex < cache.dock_apps_path.Count)
                                    cache.dock_apps_path.RemoveAt(dindex);

                                //Сохраняем кеш
                                CacheOperations.StoreCache(cache);

                                //Удаляем текущий значок
                                MainPanel.Children.Remove(image);

                                //Спустя 300 мс. обновляем ширину Дока
                                Task.Factory.StartNew(() =>
                                {
                                    System.Threading.Thread.Sleep(300);
                                    SafeInvoke(() => UpdateDockWidth());
                                });

                            }
                            else //Если пользователь отказался
                            {
                                //Делаем всё как было
                                image.BeginAnimation(StackPanel.OpacityProperty, Animations.OpacityAnimation(image.Opacity, 1, 0.3));
                            }
                            isDown = false; //Отменяем нажатие
                        }
                        //Если диалог всё ещё существует
                        if (dialog != null)
                        {
                            //Закрываем диалог
                            dialog.CloseDialog();
                        }
                        //Убиваем диалог
                        dialog = null;
                    };
                }
                catch
                {
                    Debug.WriteLine("Ошибка удаления значка");
                }

            }
        }
        /// <summary>
        /// Обработка события отмены перетаскивания на Док
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            isDrop = false;
        }
        /// <summary>
        /// Обработка события изменения размера окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Расчёт смещения Дока по левому краю и задание его Доку
            double left = SystemParameters.PrimaryScreenWidth / 2 - e.NewSize.Width / 2;
            this.Left = left;

            //Задаём смещение и ширину окну Подсказки
            tooltip.Left = left - 30;
            tooltip.Width = e.NewSize.Width + 60;

            //Считаем положение по верхнему краю для Дока
            double new_h = size + size / 2.5;
            double top = System.Windows.SystemParameters.PrimaryScreenHeight - new_h - taskbar_g;

            //Если стартовая анимация была выполнена
            if (startup_animation_completed)
            {
                DoubleAnimation fastda = new DoubleAnimation
                {
                    From = this.Top,
                    To = top,
                    Duration = TimeSpan.FromMilliseconds(0)
                };
                this.BeginAnimation(TopProperty, fastda);
            }

            tooltip.Top = top - tooltip.Height;

        }

        private void StartUpAnimation_Completed(object sender, EventArgs e)
        {
            startup_animation_completed = true;
        }
        /// <summary>
        /// Обработка события нажатия на кнопку Настроек
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            //Если есть уже окно настроек
            try
            {
                //Отображаем окно настроек
                settings.Show();
                settings.Activate();
            }
            catch //Если его нету
            {
                //Создаём новый экземпляр и отображаем
                settings = new Settings();
                settings.Show();
                settings.Activate();
            }
            //Задаём значение всем параметрам настроек
            cache = CacheOperations.LoadCache(cache);
            settings.StartupToggle.IsChecked = cache.runAtStartup;
            settings.DisableBlurToggle.IsChecked = cache.disableBlur;
            settings.StarDustEnableToggle.IsChecked = cache.enableStarDust;
            settings.EnableTopmostToggle.IsChecked = cache.enableTopmost;
            settings.AHToggle.IsChecked = cache.dockAutoHide;
            settings.Trans_bar.Value = cache.bg_trans;
            settings.ScaleSlider.Value = cache.scaleFactor;

            //Выполняем логику для слайдеров настроек
            settings.Toggle_Loaded_Do(settings.StartupToggle);
            settings.Toggle_Loaded_Do(settings.DisableBlurToggle);
            settings.Toggle_Loaded_Do(settings.StarDustEnableToggle);

        }
        /// <summary>
        /// Логика нажатия кнопки удаления значка в контекстном меню
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveFromDockButton_Click(object sender, RoutedEventArgs e)
        {
            //Получаем контекстный значок
            DockIcon context_img = context_icon as DockIcon;

            //Если он существует
            if (context_img != null)
            {
                //Запрашиваем удаление
                RemoveFromDock(context_img);
            }
        }
        /// <summary>
        /// Логика нажатия кнопки перезапуска Дока в меню
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            //Останавливаем все таймеры
            mainTimer.Stop();
            mouseTimer.Stop();

            //Блокируем Док
            move_lock = true;

            //Делаем Док не активным
            this.IsEnabled = false;

            //Инициализируем анимацию закрытия Дока
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = this.Top,
                To = this.Top + this.Height + taskbar_g,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new PowerEase(),

            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation, 60);
            myDoubleAnimation.Completed += (s, es) =>
            {
                System.Windows.Forms.Application.Restart();
                System.Windows.Application.Current.Shutdown();
            };
            myDoubleAnimation.RemoveRequested += (s, es) =>
            {
                System.Windows.Forms.Application.Restart();
                System.Windows.Application.Current.Shutdown();
            };

            //Закрываем все 
            tooltip.Close();
            settings.Close();

            //Анимируем
            this.BeginAnimation(Window.TopProperty, myDoubleAnimation);
            this.BeginAnimation(OpacityProperty, Animations.OpacityAnimation(this.Opacity, 0));
        }
        /// <summary>
        /// Логика нажатия мыши на переносной значок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Draggable_icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Активизируем переменную перетаскивания
            if (e.LeftButton == MouseButtonState.Pressed)
                AbsIconDrag = true;
        }
        //Функция проверки соприкосновения двух элементов
        private bool hitTest(UIElement el1, UIElement el2, MouseEventArgs e)
        {
            //Мне лень всё это переводить

            // Retrieve the coordinate of the mouse position.
            System.Windows.Point pt = e.GetPosition(el1);

            // Perform the hit test against a given portion of the visual object tree.
            HitTestResult result = VisualTreeHelper.HitTest(el2, pt);

            if (result != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Логика перемещения значков в доке
        /// </summary>
        /// <param name="item_index">Исходный элемент (индекс)</param>
        /// <param name="ditem_index">Конечный элемент (индекс)</param>
        /// <param name="images">Все значки</param>
        /// <returns>Новые значки</returns>
        private List<DockIcon> MoveImg(int item_index, int ditem_index, List<DockIcon> images)
        {
            //Создаём два массива слева и справа значка
            List<DockIcon> left = new List<DockIcon>();
            List<DockIcon> right = new List<DockIcon>();

            //Создаём переменную для счётчика в foreach
            int i = 0;

            //Добавляем элемент с перемещаемым значков в массив правого
            right.Add(images[item_index]);

            //Удаляем перемещаемый значок из всех элеменов
            images.Remove(images[item_index]);

            //Прохидимся по всем значкам
            foreach (DockIcon img in images)
            {
                //Если текущий индекс больше или равен индексу замещаемого значка
                if (i >= ditem_index)
                {
                    //Добавляем в правый массив
                    right.Add(img);
                }
                else //Иначе
                {
                    //Добавляем в левый массив
                    left.Add(img);
                }
                i++; //+1 к индексу
            }

            //Очищаем все значки массива
            images.Clear();

            //Добавляем левые и правые значки в массив
            images.AddRange(left);
            images.AddRange(right);

            return images; //Вовращаем конечный массив
        }

        /// <summary>
        /// Логика перемещения строки в массиве (работает так же как перемещение значков)
        /// </summary>
        /// <param name="item_index">Исходный элемент (индекс)</param>
        /// <param name="ditem_index">Конечный элемент (индекс)</param>
        /// <param name="elements"></param>
        /// <returns></returns>
        private List<string> MoveString(int item_index, int ditem_index, List<string> elements)
        {
            //Не вижу смысла комментировать, так как всё это описано в другой функции
            List<string> left = new List<string>();
            List<string> right = new List<string>();

            int i = 0;
            right.Add(elements[item_index]);
            elements.RemoveAt(item_index);

            foreach (string el in elements)
            {
                if (i >= ditem_index)
                {
                    right.Add(el);
                }
                else
                {
                    left.Add(el);
                }
                i++;
            }

            elements = new List<string>();
            elements.AddRange(left);
            elements.AddRange(right);

            return elements;
        }
        /// <summary>
        /// Событие MouseUp переносной иконки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Draggable_icon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AbsIconDrag = false; //Отменяем активность перетаскивания
            isDown = false; //Отменяем нажатость значка

            //Подготавливаемся к перемещению
            List<DockIcon> allElements = new List<DockIcon>();
            List<DockIcon> oAllElements = new List<DockIcon>();

            //Проходимся по всем элементам Дока
            foreach (UIElement cur in MainPanel.Children)
            {
                //Получаем текущий элемент дока и представляем его как значок
                DockIcon ci = cur as DockIcon;

                //Если текущий значок существует
                if (ci != null)
                {
                    //Добавляем в оба рабочих массива
                    allElements.Add(ci);
                    oAllElements.Add(ci);
                }

            }
            //Подготавливаем переменную для блокировки цикла
            bool lock_cycle = false;

            //Если элемент был перетащён в Корзину
            if (hitTest(TrashIcon, Draggable_icon, e))
            {
                //Запрашиваем удаление
                if (dr_ic != null)
                    RemoveFromDock(dr_ic);
                MainPanel.Opacity = 1;
            }
            //Проходимся по всем значкам из первого массива
            foreach (DockIcon cur in allElements)
            {
                //Если есть касание переносной иконки с текущей и цикл не заблокирован
                if (hitTest(cur, Draggable_icon, e) && !lock_cycle)
                {
                    //Получаем индексы обоих значков
                    int cur_index = MainPanel.Children.IndexOf(cur);
                    int down_index = MainPanel.Children.IndexOf(down_icon);

                    //Если текущий индекс не равен -1
                    if (cur_index != -1)
                    {
                        //Перетаскиваем значок
                        oAllElements = MoveImg(down_index, cur_index, oAllElements);

                        //Перетаскиваем строки в кеше
                        cache.dock_apps = MoveString(down_index, cur_index, cache.dock_apps);
                        cache.dock_apps_path = MoveString(down_index, cur_index, cache.dock_apps_path);

                        //Сохраняем кеш
                        CacheOperations.StoreCache(cache);

                        MainPanel.Opacity = 1;

                        //Удаляем старые значки
                        foreach (DockIcon img in allElements)
                        {
                            MainPanel.Children.Remove(img);
                        }

                        //Добавляем новый
                        foreach (DockIcon img in oAllElements)
                        {
                            MainPanel.Children.Add(img);
                        }

                        //Анимация исчезновения значка перетаскивания
                        DoubleAnimation myDoubleAnimation1 = new DoubleAnimation
                        {
                            From = Draggable_icon.Opacity,
                            To = 0,
                            Duration = TimeSpan.FromSeconds(0.3),
                            EasingFunction = new SineEase()
                        };
                        Timeline.SetDesiredFrameRate(myDoubleAnimation1, 30);
                        myDoubleAnimation1.Completed += (a, es) =>
                        {
                            Draggable_icon.Source = null;
                            dr_ic = null;
                        };
                        Draggable_icon.BeginAnimation(DockIcon.OpacityProperty, myDoubleAnimation1);

                        //Блокируем цикл
                        lock_cycle = true;

                        //Спустя 100 мс. разблокируем перемещение Дока
                        Task.Factory.StartNew(() =>
                        {
                            System.Threading.Thread.Sleep(100);
                            move_lock = false;
                        });

                    }
                }
            }

        }

        /// <summary>
        /// Функция клонирования значка
        /// </summary>
        /// <param name="source">Исходный значок</param>
        /// <returns>Конечный значок</returns>
        private DockIcon CloneIcon(DockIcon source)
        {
            DockIcon cloneimg = new DockIcon();
            cloneimg.Source = source.Source;
            cloneimg.Size = size;
            cloneimg.Height = size;
            cloneimg.MouseEnter += Img_MouseEnter;
            cloneimg.MouseLeave += Img_MouseLeave;
            cloneimg.MouseDown += Img_MouseDown;
            cloneimg.MouseUp += Img_MouseUp;
            cloneimg.MouseMove += Img_MouseMove;

            return cloneimg;
        }
        /// <summary>
        /// Локика рыбьего глаза для значков
        /// </summary>
        /// <param name="x">Смещение</param>
        private void fishEyeForIcons(double x)
        {
            //Комбинируем пользовательские значки и виджеты
            List<DockIcon> combined = new List<DockIcon>();
            foreach (DockIcon di in MainPanel.Children)
            {
                combined.Add(di);
            }
            foreach (DockIcon di in AIcons.Children)
            {
                combined.Add(di);
            }

            //Считаем ширину мнимой линии
            double width = (combined.Count) * 80;
            if (width < 300) width = 300;

            //Создаём большой массив с точкой мнимой линии
            double[] big_array = new double[(int)width];

            //Дальше немного эльфийской магии (или мне просто лень комментировать)
            int end = 300;
            for (int i = 0; i < end; i++)
            {
                double m_val = 0;
                if (i < end / 2)
                {
                    m_val = i;
                }
                else
                {
                    m_val = end - (i);
                }

                int index = (int)(i + width * x - end / 2);

                if (index < width && index >= 0)
                {
                    big_array[index] = m_val / 3;
                }
            }

            double[] single_array = new double[(int)combined.Count];

            if (!panelIconsAnimating)
            {
                for (int i = 0; i < combined.Count; i++)
                {
                    int m = (int)(width / combined.Count) * (i + 1) - 20;
                    if (m >= big_array.Length)
                        m = big_array.Length - 1;
                    if (m < 0) m = 0;
                    single_array[i] = big_array[m];

                    DockIcon image = combined[i] as DockIcon;
                    double newsize = size * (big_array[m] / 50 / 5 + 1);

                    if (newsize >= fe_max_size - 1)
                    {
                        fe_max_size = newsize;


                        if (!isHovered && fe_max_size_el != i)
                        {
                            //Img_MouseEnterDo(combined[i]);
                            if (i < MainPanel.Children.Count)
                            {
                                SetContextIcon((DockIcon)MainPanel.Children[i]);
                                context_icon = MainPanel.Children[i];
                            }


                            fe_max_size_el = i;
                        }
                        else
                        {
                            if (!isHovered)
                            {
                                //Img_MouseMoveDo(combined[fe_max_size_el]);
                                if (i < MainPanel.Children.Count)
                                {
                                    context_icon = MainPanel.Children[fe_max_size_el];
                                }
                            }
                        }
                    }

                    if (!panelIconsAnimated)
                    {

                        DoubleAnimation doubleAnimation = new DoubleAnimation
                        {
                            From = image.Size,
                            To = newsize,
                            Duration = TimeSpan.FromMilliseconds(100),
                            EasingFunction = new SineEase()
                        };
                        doubleAnimation.Completed += (a, e) =>
                        {
                            panelIconsAnimated = true;
                            panelIconsAnimating = false;

                        };
                        if (image.Size != newsize)
                        {
                            image.BeginAnimation(DockIcon.SizeProperty, doubleAnimation);
                        }

                        panelIconsAnimating = true;
                    }
                    else
                    {
                        DoubleAnimation doubleAnimation = new DoubleAnimation
                        {
                            From = image.Size,
                            To = newsize,
                            Duration = TimeSpan.FromMilliseconds(0),
                            EasingFunction = new SineEase()
                        };
                        if (image.Size != newsize)
                        {
                            image.BeginAnimation(DockIcon.SizeProperty, doubleAnimation);
                        }
                    }
                }


            }
        }
        /// <summary>
        /// Логика перемещения мыши по окну
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            //Получаем относительные координаты мыши и вызываем рыбьий глаз (магггииииияяяя)
            double gl_x = e.GetPosition(DockMain).X;
            double x = gl_x - Draggable_icon.Width / 2;
            double y = e.GetPosition(DockMain).Y - Draggable_icon.Height / 2;
            fishEyeForIcons(gl_x / DockMain.Width);
            
            //Если был зажат какой-либо значок
            if ((isDown || AbsIconDrag))
            {
                //Если нету блокировки значков дока
                if (!cache.dockLock)
                {
                    //Получаем зажатый значок
                    DockIcon dicon = down_icon as DockIcon;

                    //Инициализируем перемещение
                    Draggable_icon.Source = dicon.Source;
                    dr_ic = dicon;

                    //Смещаем зажатую иконку
                    Draggable_icon.Margin = new Thickness(x, y, 0, 0);

                    //Задаём прозрачность основной панели
                    MainPanel.Opacity = .8;

                    //Если
                    if (Draggable_icon_an)
                    {
                        Draggable_icon_an = false;

                        //Вызываем анимацию перетаскиваемой иконки
                        Draggable_icon.BeginAnimation(DockIcon.OpacityProperty, Animations.OpacityAnimation(Draggable_icon.Opacity, 1, 0.1));
                    }
                }

            }
            else //Если нету зажатия
            {
                if (!cache.dockLock)
                {
                    //Скрываем значок перетаскивания
                    DoubleAnimation myDoubleAnimation1 = new DoubleAnimation
                    {
                        From = Draggable_icon.Opacity,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.1),
                        EasingFunction = new SineEase()
                    };
                    myDoubleAnimation1.Completed += (a, es) =>
                    {
                        Draggable_icon.Source = null;
                        dr_ic = null;
                        Draggable_icon_an = true;
                    };
                    Timeline.SetDesiredFrameRate(myDoubleAnimation1, 30);
                    Draggable_icon.BeginAnimation(DockIcon.OpacityProperty, myDoubleAnimation1);
                }
            }
            try
            {
                //Отображаем подсказку
                if (!lockSizeChange)
                    tooltip.Show();
            }
            catch
            {
                consoleLog("Tooltip show error");
            }
        }
        /// <summary>
        /// Обработка события нажатия мыши на окне
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Если зажата правая клавиша мыши
            if (e.RightButton == MouseButtonState.Pressed)
            {
                //Если контекстного значка нету
                if (context_icon == null)
                {
                    //Скрываем кнопку удаления значка из контекстного меню
                    RemoveFromDockButton.Opacity = .5;
                    RemoveFromDockButton.IsEnabled = false;
                }
            }
        }
        /// <summary>
        /// Логика открытия новой копии приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenNewButton_Click(object sender, RoutedEventArgs e)
        {
            DockIcon context_img = context_icon as DockIcon;

            if (context_img != null)
            {
                int current_index = MainPanel.Children.IndexOf(context_img);
                Process.Start(cache.dock_apps_path[current_index]);
            }
        }
        /// <summary>
        /// Логика нажатия кнопки закрытия приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseSomeAppButton_Click(object sender, RoutedEventArgs e)
        {
            //Получаем имя приложения
            string current_name = cache.dock_apps[MainPanel.Children.IndexOf(context_icon)];

            //Создам диалог
            if (dialog == null) dialog = new Dialog("Are you sure you want to close " + current_name + "?");
            if (dialog != null)
            {
                try
                {
                    //Отображаем его
                    dialog.Show();
                    dialog.onResult += () =>
                    {
                        if (dialog != null)
                        {
                            if (dialog.result == true) //Если юзверь согласился закрыть прогу
                            {
                                //Получаем путь приложения
                                string current_path = cache.dock_apps_path[MainPanel.Children.IndexOf(context_icon)];

                                //Узнаём запущено ли оно
                                bool apprunned = CheckIfAppRunned(current_path);

                                //Если запущено
                                if (apprunned)
                                {
                                    //Закрываем прогу
                                    killProcess(current_path);
                                }
                            }
                            dialog.CloseDialog(); //Закрываем диалог
                        }

                        dialog = null;
                    };
                }
                catch
                {
                    consoleLog("Где-то что-то пошло не так...");
                }
            }
        }
        /// <summary>
        /// Логика события активации основной части дока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DockMain_Activated(object sender, EventArgs e)
        {
            //Если не включен режим Поверх Всех Окон, то отправляем Док на задний план
            if (!cache.enableTopmost)
                WindowAPI.SendToBack(this);
        }
        /// <summary>
        /// Логика нажатия кнопки блокировки Дока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LockDockButton_Click(object sender, RoutedEventArgs e)
        {
            cache = CacheOperations.LoadCache(cache);
            cache.dockLock = !cache.dockLock;
            DockLockUpdateUI();
            CacheOperations.StoreCache(cache);
        }
        /// <summary>
        /// Логика генерации контекстного меню
        /// </summary>
        /// <param name="items">Элементы</param>
        /// <returns></returns>
        private ContextMenu GenerateContextMenu(List<object> items)
        {
            ContextMenu res = new ContextMenu();
            res.Margin = MainContextMenu.Margin;
            res.Style = MainContextMenu.Style;
            res.ItemTemplate = (DataTemplate)Resources["MenuItemStyle"];
            res.Background = MainContextMenu.Background;
            res.Effect = MainContextMenu.Effect;
            //res.ItemTemplate = MainContextMenu.ItemTemplate;
            
            foreach(object item in items)
            {
                res.Items.Add(item);
            }
            
            
            return res;
        }
        /// <summary>
        /// Генерируем элемент меню
        /// </summary>
        /// <param name="icon">Значок</param>
        /// <param name="text">Заголовок</param>
        /// <param name="func">Результат</param>
        /// <returns></returns>
        private MenuItem GenerateMenuItem(string icon, string text, Func<int> func)
        {
            MenuItem item = new MenuItem();

            item.Style = CloseSomeAppButton.Style;
            item.CommandParameter = CloseSomeAppButton.CommandParameter;
            item.Template = (ControlTemplate)Resources["DarkCoolMenuItem"];
            item.Padding = CloseSomeAppButton.Padding;
            item.Background = CloseSomeAppButton.Background;
            item.Foreground = CloseSomeAppButton.Foreground;

            TextBlock ti = new TextBlock();
            ti.Text = icon;
            ti.FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets");
            ti.FontSize = 14;
            ti.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            
            ti.VerticalAlignment = CloseSomeAppButton.VerticalAlignment;
            
            item.Icon = ti;
            item.Header = text;

            if (func != null)
            {
                item.Click += (s, e) =>
                {
                    func();
                };
            }
                
            return item;
        }
        
        /// <summary>
        /// События появления переноса в Корзину вне Дока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrashIcon_DragEnter(object sender, DragEventArgs e)
        {
            movingToTrash = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Move;

            }
        }
        /// <summary>
        /// События завершения переноса в Корзину вне Дока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrashIcon_Drop(object sender, DragEventArgs e)
        {
            
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                try
                {
                    string[] s = (string[])e.Data.GetData(DataFormats.FileDrop);


                    foreach (string fn in s)
                    {
                        
                        Shell shell = new Shell();
                        Folder RecyclingBin = shell.NameSpace(10);

                        RecyclingBin.MoveHere(fn);

                        TrashIcon.Source = Imaging.CreateBitmapSourceFromHBitmap(GetTrashIcon().ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                        Task.Factory.StartNew(() =>
                        {
                            Thread.Sleep(300);
                            movingToTrash = false;
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + " beda #5");
                }
            }
        }
        /// <summary>
        /// Функция генерации разделителя
        /// </summary>
        /// <returns></returns>
        private Separator GenerateSeparator()
        {
            Separator separator = new Separator();
            separator.Height = 2;
            separator.Background = new SolidColorBrush(Color.FromRgb(45, 45, 45));

            return separator;
        }
        /// <summary>
        /// Функция клонирования элемента контекстного меню
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private MenuItem CloneMenuItem(MenuItem source)
        {
            MenuItem result = new MenuItem();

            if(source != null)
            {


                result.Style = source.Style;
                result.CommandParameter = source.CommandParameter;
                result.Template = (ControlTemplate)Resources["DarkCoolMenuItem"];
                result.Padding = source.Padding;
                result.Background = source.Background;
                result.Foreground = source.Foreground;

                TextBlock ti = new TextBlock();
                ti.Text = (source.Icon as TextBlock).Text;
                ti.FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets");
                ti.FontSize = 14;
                ti.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                ti.VerticalAlignment = CloseSomeAppButton.VerticalAlignment;

                result.Icon = ti;
                result.Header = source.Header;

                
                
                switch(source.Name)
                {
                    case "CloseSomeAppButton":
                        result.Click += CloseSomeAppButton_Click;
                        break;
                    case "OpenNewButton":
                        result.Click += OpenNewButton_Click;
                        break;
                    case "RemoveFromDockButton":
                        result.Click += RemoveFromDockButton_Click;
                        break;
                    case "LockDockButton":
                        result.Click += LockDockButton_Click;
                        break;
                    case "SettingsButton":
                        result.Click += SettingsButton_Click;
                        break;
                    case "RestartButton":
                        result.Click += RestartButton_Click;
                        break;
                    case "ExitButton":
                        result.Click += MenuItem_Click;
                        break;
                }

            }

            return result;
        }
        /// <summary>
        /// Логика нажатия на кнопку Проводника
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExplorerIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DockIcon ic = sender as DockIcon;
            List<object> items = new List<object>();

            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = ic.Opacity,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new SineEase(),

            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation, 30);
            ic.BeginAnimation(DockIcon.OpacityProperty, myDoubleAnimation);

            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();

            if(e.ChangedButton == MouseButton.Left)
            {
                switch (shellWindows.Count)
                {
                    case 0:
                        System.Diagnostics.Process.Start("explorer");
                        return;
                    case 1:
                        foreach (SHDocVw.InternetExplorer ie in shellWindows)
                        {

                            IntPtr intPtr = new IntPtr(ie.HWND);
                            if (WindowAPI.IsIconic(intPtr))
                            {
                                WindowAPI.SetForegroundWindow(intPtr);
                                WindowAPI.ShowWindowAsync(intPtr, 9);

                            }
                            else
                            {

                                WindowAPI.ShowWindowAsync(intPtr, WindowAPI.SW_MINIMIZE);

                            }
                        }
                        return;
                }
            }
            

            string filename;
            
            foreach (SHDocVw.InternetExplorer ie in shellWindows)
            {
                filename = Path.GetFileNameWithoutExtension(ie.FullName).ToLower();

                
                if (filename.Equals("explorer"))
                {
                    // Save the location off to your application
                    IntPtr intPtr = new IntPtr(ie.HWND);
                    items.Add(GenerateMenuItem("\uE8B7", ie.LocationName, () =>
                    {
                        WindowAPI.SetForegroundWindow(intPtr);
                        WindowAPI.ShowWindowAsync(intPtr, 9);
                        return 1;
                    }));

                    // Setup a trigger for when the user navigates
                    //ie.NavigateComplete2 += new SHDocVw.DWebBrowserEvents2_NavigateComplete2EventHandler(handlerMethod);
                }
                
            }
            if(shellWindows.Count > 0)
                items.Add(GenerateSeparator());
            items.Add(GenerateMenuItem("\uE8A7", "Open new", () =>
            {
                System.Diagnostics.Process.Start("explorer");
                return 1;
            }));
            items.Add(GenerateMenuItem("\uE8BB", "Close all", () =>
            {
                foreach (SHDocVw.InternetExplorer ie in shellWindows)
                {
                    ie.Quit();
                }
                return 1;
            }));
            items.Add(GenerateSeparator());
            items.Add(CloneMenuItem(LockDockButton));
            items.Add(GenerateSeparator());
            items.Add(CloneMenuItem(SettingsButton));
            items.Add(CloneMenuItem(RestartButton));
            items.Add(CloneMenuItem(ExitButton));


            ContextMenu contextMenu = GenerateContextMenu(items);


            contextMenu.PlacementTarget = ic;
            contextMenu.IsOpen = true;
            e.Handled = true;
        }
        /// <summary>
        /// Логика нажатия на Корзину
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrashIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DockIcon ic = sender as DockIcon;
            List<object> items = new List<object>();

            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = ic.Opacity,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new SineEase(),

            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation, 30);
            ic.BeginAnimation(DockIcon.OpacityProperty, myDoubleAnimation);

            if (e.ChangedButton == MouseButton.Left)
            {
                SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
                bool rb_matches = false;
                SHDocVw.InternetExplorer lastIe = null;
                foreach (SHDocVw.InternetExplorer ie in shellWindows)
                {
                    if (ie.LocationName == "Recycle Bin" || ie.LocationName == "Корзина" || ie.LocationName == "Кошик")
                    {
                        rb_matches = true;
                        lastIe = ie;
                    }
                }
                if(rb_matches)
                {
                    if(lastIe != null)
                    {
                        IntPtr intPtr = new IntPtr(lastIe.HWND);
                        if (WindowAPI.IsIconic(intPtr))
                        {
                            WindowAPI.SetForegroundWindow(intPtr);
                            WindowAPI.ShowWindowAsync(intPtr, 9);

                        }
                        else
                        {

                            WindowAPI.ShowWindowAsync(intPtr, WindowAPI.SW_MINIMIZE);

                        }
                    }
                    
                    
                } else
                {
                    System.Diagnostics.Process.Start("explorer.exe", "shell:RecycleBinFolder");
                }
                
                
                return;
            }
            items.Add(GenerateMenuItem("\uE8A7", "Open Recycle Bin", () =>
            {
                System.Diagnostics.Process.Start("explorer.exe", "shell:RecycleBinFolder");
                return 1;
            }));
            items.Add(GenerateMenuItem("\uE74D", "Clear Recycle Bin", () =>
            {
                WindowAPI.SHEmptyRecycleBin(IntPtr.Zero, null, WindowAPI.RecycleFlag.SHERB_NOSOUND);
                TrashIcon.Source = Imaging.CreateBitmapSourceFromHBitmap(GetTrashIcon().ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                return 1;
            }));
            items.Add(GenerateSeparator());
            items.Add(CloneMenuItem(LockDockButton));
            items.Add(GenerateSeparator());
            items.Add(CloneMenuItem(SettingsButton));
            items.Add(CloneMenuItem(RestartButton));
            items.Add(CloneMenuItem(ExitButton));


            ContextMenu contextMenu = GenerateContextMenu(items);


            contextMenu.PlacementTarget = ic;
            contextMenu.IsOpen = true;
            e.Handled = true;
        }
        /// <summary>
        /// Логика старта зажатия виджета Проводника/Корзины
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExplorerIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DockIcon ic = sender as DockIcon;
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = ic.Opacity,
                To = 0.5,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new SineEase(),

            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation, 30);
            ic.BeginAnimation(DockIcon.OpacityProperty, myDoubleAnimation);
        }
    }
}
