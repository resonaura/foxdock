using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.IO;
using System.Diagnostics;
using System.Windows.Threading;
using Point = System.Windows.Point;
using Shell32;
using Path = System.IO.Path;
using FoxDock.API;

namespace FoxDock
{
    public partial class Dock : Window
    {
        //Подключаем кеш
        public static Cache cache = new Cache();

        //Основные таймеры
        private readonly System.Timers.Timer mainTimer = new System.Timers.Timer();

        //Инициализируем окна
        public Tooltip tooltip;
        private Settings settings;
        private Dialog dialog;
        
        #region Основные переменные
        public static bool lock_slider = true;
        private readonly bool isInitedAS = false;
        private readonly int taskbar_g = 0;
        public double dpiX = 1;
        private double dpiY = 1;
        public static int defsize = 53;
        public int size = (int)(defsize * cache.scaleFactor);
        private bool lockSizeChange = false;
        public static AppLanguage.Locale locale = AppLanguage.GetSystemLocale();
        private bool isDown;
        private UIElement down_icon;
        public UIElement context_icon;
        private bool isDrop = false;
        private bool dockHidden = false;
        private bool apprunned = false;
        public bool isHovered = false;
        private bool startup_animation_completed = false;
        private bool AbsIconDrag = false;
        private bool Draggable_icon_an = true;
        public double fe_max_size = -1;
        public int fe_max_size_el = 0;
        public bool panelIconsAnimated = false;
        public bool panelIconsAnimating = false;
        private DockIcon dr_ic = null;
        private bool movingToTrash = false;
        private SHDocVw.ShellWindows shellWindows;
        private bool isMouseOnTheDock = false;
        private int inactiveSeconds = 0;
        private List<string> short_app_names = new List<string>();
        public IconPack iPack = new IconPack();
        
        //private readonly BitmapSource fullTrashIcon = IconsWorker.GetSourceFromIcon(IconsWorker.GetTrashIcon(true));
        //private readonly BitmapSource emptyTrashIcon = IconsWorker.GetSourceFromIcon(IconsWorker.GetTrashIcon(false));
        public BitmapSource fullTrashIcon = IconsWorker.GetSourceFromBitmap(FoxDock.Properties.Resources.trashbin_full);
        public BitmapSource emptyTrashIcon = IconsWorker.GetSourceFromBitmap(FoxDock.Properties.Resources.trashbin_empty);
        private Point lastMousePosition = new Point();
        #endregion

        #region Константы
        public const int SPI_SETDESKWALLPAPER = 20;
        public const int WM_SETTINGCHANGE = 0x001A;
        public const int WM_SYSCOMMAND = 0x0112;
        public const int WM_THEMECHANGED = 0x031A;
        public const int SC_MINIMIZE = 0xF020;
        public const int WM_WININICHANGE = 0x001A;
        public const int WM_DISPLAYCHANGE = 0x007e;
        #endregion

        private List<SmartHomeDevice> smartHomeDevices = new List<SmartHomeDevice>();
        /// <summary>
        /// Инициализация дока
        /// </summary>
        public Dock()
        {
            InitializeComponent(); //Инициализируем все компоненты

            
            //Загружаем кеш
            cache = CacheOperations.LoadCache(cache);

            IconPacks.Init(true, cache.iconPackName);
            API.FileAssociations.EnsureAssociationsSet();

            if (cache.iconPackName != "")
            {
                iPack = IconPacks.GetByName(cache.iconPackName);
            }
            emptyTrashIcon = IconPacks.GetIconFromPath(iPack.TrashEmpty);
            fullTrashIcon = IconPacks.GetIconFromPath(iPack.TrashFull);

            if (settings == null)
            {
                settings = new Settings(this); //Инициализируем окно настроек, если оно не инициализированно
            }
            if(tooltip == null)
            {
                tooltip = new Tooltip();
            }

            //Выполняем логику размера значков
            if (!isInitedAS)
            {
                size = (int)(defsize * cache.scaleFactor);
                settings.ScaleSlider.Value = cache.scaleFactor;
                isInitedAS = true;
            }

            //Применяем локализацию для кнопок меню
            ExitButton.Header = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.ExitDock, locale);
            RestartButton.Header = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.RestartDock, locale);
            SettingsButton.Header = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DockSettings, locale);
            LockDockButton.Header = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.LockDock, locale);
            RemoveFromDockButton.Header = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.RemoveFromDock, locale);
            RenameButton.Header = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.RenameIcon, locale);
            OpenNewButton.Header = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.OpenNew, locale);

            //Применяем локализацию для виджетов
            ExplorerIcon.Label = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.Explorer, locale);
            TrashIcon.Label = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.RecycleBin, locale);
            RecentIcon.Label = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.RecentFiles, locale);

            //Для защиты от вылета используем try,catch
            try { API.WindowsManager.window = this; } catch { ConsoleLog("Ошибка задания основного окна для WindowAPI"); }


            ExplorerIcon.Source = IconPacks.GetIconFromPath(iPack.ExplorerIcon);
            //Получаем значки Проводника и Корзины и задаём их для соответствующих виджетов на Доке
            //ExplorerIcon.Source = IconsWorker.GetSourceFromIcon(IconsWorker.GetSystemIcon(FileTools.GetExplorerPath()));

            //RecentIcon.Source = IconsWorker.GetSourceFromIcon(IconsWorker.GetSystemIcon(FileTools.GetRecentsPath()));
            RecentIcon.Source = IconPacks.GetIconFromPath(iPack.Recent);

            TrashIcon.Source = API.Shell32.TrashCount() > 0 ? fullTrashIcon : emptyTrashIcon;

            //Получаем высоту панели задач
            int taskbar = API.WindowsManager.GetTaskBarH(this);
            taskbar_g = taskbar;

            //Обработчик события успешной загрузки дока
            void handler(object s, RoutedEventArgs e)
            {
                Task.Factory.StartNew(async () =>
                {
                    smartHomeDevices = await SmartHome.Discover();

                    if (smartHomeDevices.Count > 0)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            tooltip.app_hint.Visibility = Visibility.Hidden;
                        });
                    }

                    foreach (SmartHomeDevice device in smartHomeDevices)
                    {
                        try
                        {
                            bool isturnedon = await device.IsTurnedOn();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                DockIcon dockIcon = new DockIcon();
                            
                                if (device.type == DeviceType.Yeelight)
                                {
                                    dockIcon.Source = isturnedon ? IconPacks.GetIconFromPath(iPack.BulbOn) : IconPacks.GetIconFromPath(iPack.BulbOff);
                                }
                                dockIcon.Label = device.model;
                            
                                dockIcon.MouseEnter += DockIcon_MouseEnter;
                                dockIcon.MouseLeave += DockIcon_MouseLeave;
                                dockIcon.MouseMove += DockIcon_MouseMove;
                                dockIcon.MouseUp += BulbIcon_MouseUp;


                                SHIcons.Children.Insert(0, dockIcon);
                                
                                UpdateDockWidth();
                                StabilizeIcons();
                            });
                            
                            Debug.WriteLine(device.model);
                        }
                        catch(Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                        
                    }
                    
                    if(smartHomeDevices.Count > 0)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            SHIcons.Visibility = Visibility.Visible;
                            SHSeparator.Visibility = Visibility.Visible;
                        });
                        Thread.Sleep(300);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            tooltip.app_hint.Visibility = Visibility.Visible;
                        });
                    }
                });
                //Получаем DPI
                double[] dpi = API.WindowsManager.GetDPI(this);
                dpiY = dpi[1]; dpiX = dpi[0];

                //Убираем событие
                Loaded -= handler;

                WindowInteropHelper wndHelper = new WindowInteropHelper(this);

                int exStyle = (int)API.Win32.GetWindowLong(wndHelper.Handle, (int)API.Win32.GetWindowLongFields.GWL_EXSTYLE);

                exStyle |= (int)API.Win32.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
                API.Win32.SetWindowLong(wndHelper.Handle, (int)API.Win32.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

                //Загружаем кеш
                cache = CacheOperations.LoadCache(cache);

                //Выполняем необходимые действия в зависимости от кеша
                if (cache.disableBlur == false)
                {
                    API.Acryl.EnableBlur(this);
                }

                if (cache.enableStarDust)
                {
                    StarDust.Visibility = Visibility.Visible;
                }

                //Получаем высоту дока и его положение по вертикали
                double new_h = size + size / 2.0;
                Height = new_h;
                double new_top = System.Windows.SystemParameters.PrimaryScreenHeight - new_h;
                AnimateHChange(new_top, new_h);

                //Адаптивный фон дока
                AutoWallUI();

                //Применяем логику для кнопки блокировки дока в контекстном меню
                DockLockUpdateUI();

                //Задаём прозрачность фона в зависимости от значения кеша
                App_full_bg.Opacity = cache.bg_trans;

                //Задаём Framerate для всех анимаций
                Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 60 });

                //Выполняем стартовую анимацию появления дока
                double top = System.Windows.SystemParameters.PrimaryScreenHeight - this.Height - taskbar_g;
                isMouseOnTheDock = true;
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
                this.BeginAnimation(OpacityProperty, Animations.SingleAnimation(0, 1));

                tooltip.Show();
            }

            Loaded += handler;

            //Не помню, что эта фигня делает, но она вроде очень нужна
            API.WindowsManager.MakeWin();

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
                API.WindowsManager.ShowDesktop(); //Переводим пользователя на Рабочий Стол
                Close(); //Закрываем текущую копию Дока
                tooltip.Close(); //Закрываем окно Подсказки
                settings.Close(); //Закрываем окно настроек
                Environment.Exit(0); //Убиваем процесс
            }

            //Запускаем основной таймер
            mainTimer.Interval = 1000;
            mainTimer.Elapsed += MainTimer_Tick;
            mainTimer.Start();

            //Добавляем значки из кеша на Док
            if (cache.dock_apps_path != null)
            {
                foreach (string path in cache.dock_apps_path) { short_app_names.Add(Path.GetFileNameWithoutExtension(path)); AddIconToPanel(path); }
                StabilizeIcons(); UpdateDockWidth();
            }

            //Разблокируем слайдер
            lock_slider = false;
        }

        private void DockIcon_MouseMove(object sender, MouseEventArgs e)
        {
            if (!cache.enable_fish_eye)
            {
                if (sender is DockIcon img)
                {
                    //Обновляем текст в объекте подсказки
                    //tooltip.app_hint.BeginAnimation(Label.OpacityProperty, Animations.SingleAnimation(1, 0, 0));
                    //Получаем текущий индекс
                    int current_index = MainPanel.Children.IndexOf(img);

                    string current_label;
                    if (current_index != -1) //Если индекс определён
                    {
                        current_label = cache.dock_apps[current_index]; //Получаем текущую подсказку
                    }
                    else
                    {
                        current_label = img.Label; //В противном случае берём подсказку из свойства объекта
                    }

                    if (current_label != tooltip.app_hint.Content.ToString())
                    {
                        tooltip.app_hint.Content = current_label;
                    }


                    Label label = tooltip.app_hint; //Делаем особый клон текущей подсказки 

                    //Подстраиваем ширину клона в зависимости от содержания
                    label.Measure(new System.Windows.Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    label.Arrange(new Rect(label.DesiredSize));

                    //Получаем ширину клона
                    double real_hint_width = label.ActualWidth;


                    var element_Visual_Relative = img.TransformToVisual((Visual)Content);
                    System.Windows.Point offset = element_Visual_Relative.Transform(new System.Windows.Point(0, 0));
                    var offsetX = offset.X;
                    tooltip.hintTrans.X = offsetX + (img.Size) / 2 - (real_hint_width / 2) + 30 + 5;
                }
            }
        }

        /// <summary>
        /// Отобразить кнопку блокировки дока в контекстном меню в зависимости от значения кеша
        /// </summary>
        public void DockLockUpdateUI()
        {
            if (cache.dockLock) {
                LockDockButton.Header = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.UnlockDock, locale);
                LockDockIcon.Text = "\uE785";
            }
            else
            {
                LockDockButton.Header = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.LockDock, locale);
                LockDockIcon.Text = "\uE72E";
            }
        }
        
        /// <summary>
        /// Обработчик события изменения параметра WindowState главного окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (!cache.enableTopmost)
            {
                API.WindowsManager.SendToBack(this); //Если не включен режим Поверх Всех Окон отправляем Док на задний план
            }
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
            switch(msg)
            {
                case WM_SETTINGCHANGE:
                    AutoWallUI();
                    break;
                case WM_DISPLAYCHANGE:
                    this.Width = 0;
                    UpdateDockWidth();
                    break;
            }

            return IntPtr.Zero;
        }
        /// <summary>
        /// Логика добавления значка в Док
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        private void AddIconToPanel(string path)
        {
            //Создаём иконку
            Dictionary<bool, BitmapSource> d = IconsWorker.SourceFromPath(path, iPack);
            BitmapSource source = d.Values.First();
            bool maskneeded = d.Keys.First();
            if (source != null)
            {
                //Создаём новый DockIcon и присваиваем ему все события
                DockIcon dockIcon = new DockIcon
                {
                    Source = source
                };
                if (maskneeded)
                {
                    dockIcon.MaskCornerRadius = iPack.MaskCornerRadius;
                    dockIcon.MaskBackground = iPack.MaskBackground;
                    dockIcon.MaskPadding = iPack.MaskPadding * size / defsize;
                    dockIcon.MaskMargin = iPack.MaskMargin * size/defsize;
                }

                dockIcon.MouseDown += DockIcon_MouseDown;
                dockIcon.MouseEnter += DockIcon_MouseEnter;
                dockIcon.MouseLeave += DockIcon_MouseLeave;
                dockIcon.MouseUp += DockIcon_MouseUp;
                dockIcon.MouseMove += DockIcon_MouseMove;

                //Получаем свободный индекс для добавления нового элемента
                int index = MainPanel.Children.Count;
                MainPanel.Children.Insert(index, dockIcon);

                //В новом потоке обновляем ширину дока
                Task.Factory.StartNew(() =>
                {
                    System.Threading.Thread.Sleep(0);
                    SafeInvoke(() =>
                    {
                        UpdateDockWidth();
                        lastCombined = IconsWorker.GetIcons(DockIcons);
                    });
                });
            }
        }
        
        /// <summary>
        /// Функция на случай того если определённая программа запущена
        /// </summary>
        /// <param name="image">Значок программы</param>
        private void IfAppRunned(DockIcon image, string windowname = "")
        {
            if (!image.Highlight)
            {
                image.Highlight = true;
            }
            //Получаем путь и имя приложения
            string current_path = cache.dock_apps_path[MainPanel.Children.IndexOf(image)];
            string current_name = Path.GetFileNameWithoutExtension(current_path).ToLower();

            if(current_name == "telegram")
            {
                string count_str = API.Win32.GetTelegramNotifyCount(windowname);
                int count = int.Parse(count_str);
                if(count > 99)
                {
                    count_str = ".." + Convert.ToInt32(count_str.AsEnumerable().Last().ToString());
                }
                image.NotifyCount = count_str;
            }

        }
        /// <summary>
        /// Функция на случай того если определённая программа не запущена
        /// </summary>
        /// <param name="image">Значок программы</param>
        private void IfNotAppRunned(DockIcon image)
        {
            if (image.Highlight)
            {
                image.Highlight = false;
            }
        }
        /// <summary>
        /// Функция уничтожения процесса
        /// </summary>
        /// <param name="path">Путь</param>
        private void KillProcess(string path)
        {
            Process.GetProcessesByName(FileTools.AppFromPath(path))[0].Kill();
        }
        /// <summary>
        /// Логика проверки активности приложений из Дока
        /// </summary>
        private void AppsActiveLogic()
        {
            try
            {
                Array allProcesses = Process.GetProcesses().ToArray();
                List<int> indexes = new List<int>();
                
                int index = -1;
                foreach (Process p in allProcesses)
                {
                    string pname = p.ProcessName;
                    index = short_app_names.IndexOf(pname);
                    
                    if (index != -1)
                    {
                        indexes.Add(index);
                        
                        Application.Current.Dispatcher.Invoke(() => 
                        {
                            string window_title = "";

                            if(pname.ToLower() == "telegram")
                            {
                                window_title = p.MainWindowTitle;
                            }
                            IfAppRunned(MainPanel.Children[index] as DockIcon, window_title);
                        });
                    }
                }
                for(int i = 0; i < cache.dock_apps_path.Count; i++)
                {
                    if(indexes.IndexOf(i) == -1)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            IfNotAppRunned(MainPanel.Children[i] as DockIcon);
                        });
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Apps active logic error.." + ex.ToString());
            }
        }
        /// <summary>
        /// Функция отображения Док бара
        /// </summary>
        public void ShowDock()
        {
            isMouseOnTheDock = true;
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
            this.BeginAnimation(OpacityProperty, Animations.SingleAnimation(this.Opacity, 1));
        }
        /// <summary>
        /// Функция скрытия Док бара
        /// </summary>
        public void HideDock()
        {
            dockHidden = true;
            lockSizeChange = true;
            isMouseOnTheDock = true;
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = this.Top,
                To = this.Top + this.Height + taskbar_g,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new PowerEase(),
            };
            myDoubleAnimation.Completed += (x, y) =>
            {
                isMouseOnTheDock = false;
            };
            Timeline.SetDesiredFrameRate(myDoubleAnimation, 60);
            this.BeginAnimation(Window.TopProperty, myDoubleAnimation);
            this.BeginAnimation(OpacityProperty, Animations.SingleAnimation(this.Opacity, 0));
        }
        /// <summary>
        /// Фунция безопасного инвока
        /// </summary>
        /// <param name="act"></param>
        private void SafeInvoke(Action act)
        {
            try { Application.Current.Dispatcher.Invoke(act); } catch { ConsoleLog("Invoke error"); }
        }

        /// <summary>
        /// Логика режима Поверх Всех Окон
        /// </summary>
        private void TopmostTimerLogic()
        {
            //Получаем положение дока по вертикале относительно экрана
            double top = System.Windows.SystemParameters.PrimaryScreenHeight - (size + size / 2) - taskbar_g;
            double y = API.WindowsManager.GetMousePosition().Y / dpiY; //Получаем положение мыши по Y
            
            //Если курсор находится в триггер-зоне экрана
            if (y >= System.Windows.SystemParameters.PrimaryScreenHeight - 20)
            {
                if (dockHidden)
                {
                    MainTimerLogic();
                    SafeInvoke(() => ShowDock()); //Отображаем Док
                }
            }
            else //Если курсор находится вне триггер-зоны
            {
                if (y < System.Windows.SystemParameters.PrimaryScreenHeight - (System.Windows.SystemParameters.PrimaryScreenHeight - top))
                {
                    //Если пользователь находится на Рабочем Столе
                    if (API.WindowsManager.IsOnDesktop())
                    {
                        if (dockHidden)
                        {
                            SafeInvoke(() => ShowDock()); //Отображаем док
                        }
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
        
        /// <summary>
        /// Логика значка Корзины в зависимости от наличия в Корзине элементов
        /// </summary>
        private void TrashIconLogic()
        {
            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Loaded,
                new Action(() => {
                    try
                    {
                        int trash_count = API.Shell32.TrashCount();
                        string trash_count_string = trash_count.ToString();
                        if(trash_count > 9)
                        {
                            trash_count_string = "9+";
                        }
                        TrashIcon.NotifyCount = trash_count_string;
                        if (trash_count > 0)
                        {
                            TrashIcon.Source = fullTrashIcon;
                        }
                        else
                        {
                            TrashIcon.Source = emptyTrashIcon;
                        }
                    }
                    catch
                    {
                        Debug.WriteLine("Ошибка получения и задания значка Корзины");
                    }
                })
            );
        }
        /// <summary>
        /// Логика хайлайтов Проводника и Корзины
        /// </summary>
        private void ExplorerAndTrashHighlightLogic()
        {
            shellWindows = new SHDocVw.ShellWindows(); //Получаем все окна проводника

            //В зависимости от кол-ва окон проводника отображаем или скрываем хайлайт под виджетом проводника
            if (shellWindows.Count > 0)
            {
                SafeInvoke(() => ExplorerIcon.Highlight = true);
            }
            else
            {
                SafeInvoke(() => ExplorerIcon.Highlight = false);
            }

            //Переменная для того, чтобы обозначить существования корзины среди окон проводника
            bool rb_matches = false;

            //Проходимся по всем окнам проводника
            foreach (SHDocVw.InternetExplorer ie in shellWindows)
            {
                //Если есть в кармане пачка... Ой, не то пальто... Кхм. Если текущее окно - окно корзины задаём значение переменной на положительное
                if (ie.LocationName == "Recycle Bin" || ie.LocationName == "Корзина" || ie.LocationName == "Кошик")
                {
                    rb_matches = true;
                }
            }
            //Если есть корзина
            SafeInvoke(() => TrashIcon.Highlight = rb_matches); //Делаем хайлайт активным в зависимости от наличия Корзины
        }
        private int timer_tick_p = 0;
        private async void MainTimerLogic()
        {
            timer_tick_p++;
            if (timer_tick_p >= 2)
            {
                foreach (SmartHomeDevice d in smartHomeDevices)
                {
                    int index = smartHomeDevices.IndexOf(d);
                    
                    if(index >= 0 && index < Application.Current.Dispatcher.Invoke(() => SHIcons.Children.Count))
                    {
                        DockIcon ic = Application.Current.Dispatcher.Invoke(() => SHIcons.Children[index] as DockIcon);
                        bool laststate = d.state;

                        if (await d.IsTurnedOn())
                        {
                            if (laststate != true)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    ic.Source = IconPacks.GetIconFromPath(iPack.BulbOn);
                                });
                            }
                        }
                        else
                        {
                            if (laststate != false)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    ic.Source = IconPacks.GetIconFromPath(iPack.BulbOff);
                                });
                            }
                        }
                    }
                }
                timer_tick_p = 0;
            }

            ExplorerAndTrashHighlightLogic(); //Выполняем логику хайлайтов Проводника и Корзины
            TrashIconLogic(); //Пытаемся получить и задать значок Корзины

            //Выполняем логику активности приложений в новом потоке
            await Task.Factory.StartNew(() =>
            {
                AppsActiveLogic();
            });

            //Пробуем изменять положение Дока по вертикали (нужно для того, чтобы при перемещении панели задач и других действиях док становился на нужное место)
            try
            {
                SafeInvoke(() => AnimateHChange(System.Windows.SystemParameters.PrimaryScreenHeight - this.Height, this.Height));
            }
            catch (Exception ex)
            {
                //В случае ошибки - выводим её в консоль
                Debug.WriteLine(ex.Message + " beda #3");
            }

            //Если стартовая анимация выполнена и не заблокировано изменение размера, то выполняем логику автоматической позиции Дока и подсказки
            try
            {
                if (startup_animation_completed && !lockSizeChange)
                {
                    SafeInvoke(() => AutoTooltipAndDockPosition());
                }
            }
            catch
            {
                ConsoleLog("Ошибка расчёта автоматической позиции Дока и подсказки");
            }
        }
        /// <summary>
        /// Логика основного таймера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainTimer_Tick(object sender, EventArgs e)
        {
            
            
            //Если режим Topmost активен
            if (cache.enableTopmost)
            {
                TopmostTimerLogic();
            }
            if (isMouseOnTheDock) return;

            //Если режим умной блокировки активен
            if (cache.smart_disable)
            {
                Point current_mouse_position = API.WindowsManager.GetMousePosition();
                inactiveSeconds++;
                if (lastMousePosition == current_mouse_position)
                {
                    if (inactiveSeconds > 10)
                    {
                        return;
                    }
                }
                else
                {
                    inactiveSeconds = 0;
                    lastMousePosition = current_mouse_position;
                }
            }

            if (!API.WindowsManager.IsOnDesktop()) return;
            Task.Factory.StartNew(() =>
            {
                MainTimerLogic();
            });

        }
        /// <summary>
        /// Функция автоматической позиции Дока и подсказки
        /// </summary>
        public void AutoTooltipAndDockPosition()
        {
            if (!startup_animation_completed)
            {
                return; //Если стартовая анимация не выполнена - останавливаем выполнение функции
            }

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
            {
                this.BeginAnimation(TopProperty, fastda);
            }
        }

        /// <summary>
        /// Функция адаптивного фона
        /// </summary>
        /// <param name="upd"></param>
        public void AutoWallUI()
        {
            Task.Factory.StartNew(() =>
            {
                string theme = API.Win32.GetSysTheme();

                SafeInvoke(() =>
                {
                    //Комбинируем основные значки с виджетами
                    List<DockIcon> combined = IconsWorker.GetIcons(DockIcons);

                    //Выполняем стандартную анимацию анимации темы
                    Animations.ThemeAnimate(theme, this);
                });
            });
        }

        /// <summary>
        /// Функция логирования в консоль
        /// </summary>
        /// <param name="cdd">Объект</param>
        public void ConsoleLog(object cdd)
        {
            Debug.WriteLine(cdd);
        }
        
        /// <summary>
        /// Функция обработки успешного перетаскивания на Док
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_Drop(object sender, DragEventArgs e)
        {
            isDrop = false; //Задаём отрицательное значение переменной, которая сигнализирует о том, что происходит перетаскивание
            if (cache.dockLock || movingToTrash)
            {
                return; //Если не включена блокировка значков или идёт перемещение в корзину
            }

            Debug.WriteLine(string.Join(", ", e.Data.GetFormats()));


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
                            if(File.Exists(fn))
                            {
                                string reapath = FileTools.GetRealAppPath(fn);
                                cache.dock_apps.Add(lname);
                                cache.dock_apps_path.Add(reapath);
                                short_app_names.Add(Path.GetFileNameWithoutExtension(reapath));
                                CacheOperations.StoreCache(cache);

                                AddIconToPanel(fn);
                                IconsWorker.UpdateDockIcons(this);
                            }
                            
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
        /// Обработка события Mouse_Up значка дока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DockIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DockIcon img = sender as DockIcon; //Получаем текущий значок

            int current_index = MainPanel.Children.IndexOf(img); //Получаем его индекс

            if (down_icon != null && isDown && !isDrop) //Если нету перетаскивания на Док и до этого левая кнопка мыши была зажата
            {
                //Если зажатый значок был тем же, что и текущий
                if (down_icon == img)
                {
                    //Проучаем путь к приложению
                    string app_path = cache.dock_apps_path[current_index];
                    string app_name = FileTools.AppFromPath(app_path);

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
                                    {
                                        string path = cache.dock_apps_path[current_index];
                                        ProcessStartInfo _processStartInfo = new ProcessStartInfo
                                        {
                                            WorkingDirectory = Path.GetDirectoryName(path),
                                            FileName = Path.GetFileName(path)
                                        };
                                        //Запускаем приложение по-новой
                                        System.Diagnostics.Process.Start(_processStartInfo);
                                        img.Highlight = true;
                                    }
                                }
                                else //В противном случае
                                {
                                    //+1 к реальным окнам
                                    real_windows++;

                                    //Если окно скрыто
                                    if (API.WindowsManager.IsIconic(proc.MainWindowHandle))
                                    {
                                        //Показываем и делаем его активным
                                        API.WindowsManager.SetForegroundWindow(proc.MainWindowHandle);
                                        API.WindowsManager.ShowWindowAsync(proc.MainWindowHandle, 9);

                                    }
                                    else //Если окно открыто
                                    {
                                        //Скрываем его
                                        API.WindowsManager.ShowWindowAsync(proc.MainWindowHandle, API.WindowsManager.SW_MINIMIZE);

                                    }
                                }
                            }
                        }
                        else //Если приложение не запущено
                        {
                            //Запускаем его
                            string path = cache.dock_apps_path[current_index];
                            ProcessStartInfo _processStartInfo = new ProcessStartInfo
                            {
                                WorkingDirectory = Path.GetDirectoryName(path),
                                FileName = Path.GetFileName(path)
                            };
                            //Запускаем приложение по-новой
                            System.Diagnostics.Process.Start(_processStartInfo);
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
            img.BeginAnimation(DockIcon.OpacityProperty, Animations.SingleAnimation(img.Opacity, 1, 0.2));

        }
        
        /// <summary>
        /// Обработчик события нажатия на значок Дока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DockIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            apprunned = false; //Приводим индикацию запущенности приложения в исходное состояние

            //Если нажатие было левой клавишей мыши
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isDown = true; //Обозначаем нажатие
            }

            //Получаем значок из сендера
            DockIcon img = sender as DockIcon;

            //Если была нажата правая клавиша
            if (e.RightButton == MouseButtonState.Pressed)
            {
                //Делаем значок контекстным
                ContextMenuTools.SetContextIcon(img, this);
            }

            //Обозначаем, что именно наш значок был зажат
            down_icon = img;
            
            //Делаем значок полупрозрачным
            img.BeginAnimation(DockIcon.OpacityProperty, Animations.SingleAnimation(img.Opacity, 0.5, 0.2));
        }
        
        /// <summary>
        /// Логика обработки события выхода курсора за рамки иконки
        /// </summary>
        /// 
        private void DockIcon_MouseLeaveDo()
        {
            isHovered = false; //Обозначаем, что ни один значок не наведён
        }
        /// <summary>
        /// Обработка события выхода курсора за рамки иконки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DockIcon_MouseLeave(object sender, MouseEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                SafeInvoke(() => DockIcon_MouseLeaveDo());
            });
        }
        /// <summary>
        /// Применяем изменение размера
        /// </summary>
        /// <param name="end"></param>
        /// <param name="e"></param>
        /// 
        /// 
        private void AnimateSizeChange(int end, DockIcon e)
        {
            e.Size = end;
        }
        /// <summary>
        /// Анимация изменения ширины
        /// </summary>
        /// <param name="start">Бывшее значение</param>
        /// <param name="end">Новое значение</param>
        /// <param name="e">Окно</param>
        public void AnimateWChange(int start, int end, Window e)
        {
            if (lockSizeChange)
            {
                return;
            }

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
        public void AnimateHChange(double top, double height)
        {
            if (lockSizeChange)
            {
                return;
            }

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
        /// Обработка события наведения на значок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DockIcon_MouseEnter(object sender, MouseEventArgs e)
        {
            isHovered = true;

            if (!cache.enable_fish_eye)
            {
                if (sender is DockIcon img)
                {
                    //Обновляем текст в объекте подсказки
                    //tooltip.app_hint.BeginAnimation(Label.OpacityProperty, Animations.SingleAnimation(1, 0, 0));
                    //Получаем текущий индекс
                    int current_index = MainPanel.Children.IndexOf(img);

                    string current_label;
                    if (current_index != -1) //Если индекс определён
                    {
                        current_label = cache.dock_apps[current_index]; //Получаем текущую подсказку
                    }
                    else
                    {
                        current_label = img.Label; //В противном случае берём подсказку из свойства объекта
                    }

                    if (current_label != tooltip.app_hint.Content.ToString())
                    {
                        tooltip.app_hint.Content = current_label;
                    }


                    Label label = tooltip.app_hint; //Делаем особый клон текущей подсказки 

                    //Подстраиваем ширину клона в зависимости от содержания
                    label.Measure(new System.Windows.Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    label.Arrange(new Rect(label.DesiredSize));

                    //Получаем ширину клона
                    double real_hint_width = label.ActualWidth;


                    var element_Visual_Relative = img.TransformToVisual((Visual)Content);
                    System.Windows.Point offset = element_Visual_Relative.Transform(new System.Windows.Point(0, 0));
                    var offsetX = offset.X;
                    tooltip.hintTrans.X = offsetX + (img.Size) / 2 - (real_hint_width / 2) + 30 + 5;
                }
            }
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
            this.BeginAnimation(OpacityProperty, Animations.SingleAnimation(this.Opacity, 0));
        }
        /// <summary>
        /// Обработка события нажатия кнопки закрытия Дока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CloseDock_Click(object sender, RoutedEventArgs e)
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
        /// Функция для возвращения значков в норму
        /// </summary>
        private void StabilizeIcons(bool opacityOnly = false)
        {
            //Комбинируем пользовательские значки с виджетами
            List<DockIcon> combined = IconsWorker.GetIcons(DockIcons);

            //Проходимся по всем значкам из комбинированных
            foreach (DockIcon img_cur in combined)
            {
                //Если текущий значок существует
                if (img_cur != null)
                {
                    //Анимируем его в нормальное состояние и размер
                    img_cur.BeginAnimation(DockIcon.OpacityProperty, Animations.SingleAnimation(img_cur.Opacity, 1, 0.2));
                    if (opacityOnly) return;
                    DoubleAnimation doubleAnimation = new DoubleAnimation
                    {
                        From = img_cur.Size,
                        To = size,
                        Duration = TimeSpan.FromMilliseconds(200),
                        EasingFunction = new SineEase(),
                        FillBehavior = FillBehavior.Stop
                    };
                    doubleAnimation.Completed += (x, y) =>
                    {
                        panelIconsAnimating = false;
                        img_cur.Size = size;
                    };
                    tooltip.app_hint.BeginAnimation(Label.OpacityProperty, Animations.SingleAnimation(1, 0, .2));
                    img_cur.BeginAnimation(DockIcon.SizeProperty, doubleAnimation);
                }
            }
        }

        /// <summary>
        /// Обработка события закрытия окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dock_MouseLeave(object sender, MouseEventArgs e)
        {
            //Обозначаем то, что значки панели не анимированны
            panelIconsAnimated = false;

            //Очищяем значок переноса
            Draggable_icon.Source = null;
            dr_ic = null;

            isMouseOnTheDock = false;

            //Если нету блокировки движения значков Дока
            StabilizeIcons();
            //Если есть зажатая иконка
            if (down_icon != null && isDown && !apprunned && !cache.dockLock)
            {
                //Запрашиваем её удаление
                DockIcon dimg = down_icon as DockIcon;
                RemoveFromDock(dimg);
                MainPanel.Opacity = 1;
            }
            

            foreach (DockIcon img_cur in MainPanel.Children)
            {
                //Если текущий значок существует - восстанавливаем его оригинальный размер
                if (img_cur != null)
                {
                    AnimateSizeChange((int)(size), img_cur);
                }
            }
        }
        public void UpdateDockWidth()
        {
            //Получаем суммарное количество значков
            int summary_icons_count = (int)IconsWorker.GetDockIconsCount(DockIcons);

            //Считаем необходимые значения для получения ширины дока
            double summary_icons_width = summary_icons_count * size;
            double summary_icons_margin = summary_icons_count * 10;
            double separator_size_and_margin = (int)IconsWorker.GetSeparatorsWidth(DockIcons);
            double free_space = size;

            //Дополнительная логика для особых вариантов размера
            double addt = 0;
            if (size < 56)
            {
                addt = size / 2;
            }

            if (size < 53)
            {
                addt = size;
            }

            //Считаем ширину Дока
            double new_width = summary_icons_width + summary_icons_margin + separator_size_and_margin + free_space + addt;
            if (new_width < 100)
            {
                new_width = 100;
            }

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
            if (dialog == null)
            {
                dialog = new Dialog(AppLanguage.GetDialogByLocale(AppLanguage.Dialog.ConfRemove, locale));
            }

            //Если диалог существует
            if (dialog != null)
            {
                //Делаем значок полупрозрачным
                image.BeginAnimation(StackPanel.OpacityProperty, Animations.SingleAnimation(image.Opacity, 0.2, 0.3));

                //Для безопасности выполняем код в конструкции try, catch
                try
                {
                    dialog.Show(); //Отображаем диалог
                    dialog.OnResult += () => //Если есть результат диалога
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
                                {
                                    cache.dock_apps.RemoveAt(dindex);
                                }

                                if (dindex < cache.dock_apps_path.Count)
                                {
                                    cache.dock_apps_path.RemoveAt(dindex);
                                    short_app_names.RemoveAt(dindex);
                                }

                                //Сохраняем кеш
                                CacheOperations.StoreCache(cache);

                                //Удаляем текущий значок
                                MainPanel.Children.Remove(image);

                                //Спустя 30 мс. обновляем ширину Дока
                                Task.Factory.StartNew(() =>
                                {
                                    System.Threading.Thread.Sleep(30);
                                    SafeInvoke(() =>
                                    {
                                        UpdateDockWidth();
                                        lastCombined = IconsWorker.GetIcons(DockIcons);
                                    });
                                });

                            }
                            else //Если пользователь отказался
                            {
                                //Делаем всё как было
                                image.BeginAnimation(StackPanel.OpacityProperty, Animations.SingleAnimation(image.Opacity, 1, 0.3));
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

        private void RenameIcon(int index)
        {
            //Создаём диалог
            if (dialog == null)
            {
                dialog = new Dialog(AppLanguage.GetDialogByLocale(AppLanguage.Dialog.ConfRename, locale), true, cache.dock_apps[index]);
            }

            //Если диалог существует
            if (dialog != null)
            {
                //Для безопасности выполняем код в конструкции try, catch
                try
                {
                    dialog.Show(); //Отображаем диалог
                    dialog.OnResult += () => //Если есть результат диалога
                    {
                        //Если диалог всё ещё существует
                        if (dialog != null)
                        {
                            //Если пользователь сохранил изменения
                            if (dialog.result == true)
                            {
                                
                                cache.dock_apps[index] = dialog.RenameBox.Text;
                                //Сохраняем кеш
                                CacheOperations.StoreCache(cache);
                            }
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
            tooltip.Width = e.NewSize.Width + 300;

            //Считаем положение по верхнему краю для Дока
            double new_h = size + size / 2.0;
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
            isMouseOnTheDock = false;
        }
        /// <summary>
        /// Обработка события нажатия на кнопку Настроек
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SettingsButton_Click(object sender, RoutedEventArgs e)
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
                settings = new Settings(this);
                settings.Show();
                settings.Activate();
            }
        }
        /// <summary>
        /// Логика нажатия кнопки удаления значка в контекстном меню
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RemoveFromDockButton_Click(object sender, RoutedEventArgs e)
        {
            //Если существует контекстный значок
            if (context_icon is DockIcon context_img)
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
        public void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            //Останавливаем все таймеры
            mainTimer.Stop();

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
            this.BeginAnimation(OpacityProperty, Animations.SingleAnimation(this.Opacity, 0));
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
            {
                AbsIconDrag = true;
            }
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
                //Если текущий значок существует
                if (cur is DockIcon ci)
                {
                    //Добавляем в оба рабочих массива
                    allElements.Add(ci);
                    oAllElements.Add(ci);
                }
            }
            //Подготавливаем переменную для блокировки цикла
            bool lock_cycle = false;

            //Если элемент был перетащён в Корзину
            if (IconsMove.HitTest(TrashIcon, Draggable_icon, e))
            {
                //Запрашиваем удаление
                if (dr_ic != null)
                {
                    RemoveFromDock(dr_ic);
                }

                MainPanel.Opacity = 1;
            }
            //Проходимся по всем значкам из первого массива
            foreach (DockIcon cur in allElements)
            {
                //Если есть касание переносной иконки с текущей и цикл не заблокирован
                if (IconsMove.HitTest(cur, Draggable_icon, e) && !lock_cycle)
                {
                    //Получаем индексы обоих значков
                    int cur_index = MainPanel.Children.IndexOf(cur);
                    int down_index = MainPanel.Children.IndexOf(down_icon);

                    //Если текущий индекс не равен -1
                    if (cur_index != -1)
                    {
                        //Перетаскиваем значок
                        oAllElements = IconsMove.MoveImg(down_index, cur_index, oAllElements);

                        //Перетаскиваем строки в кеше
                        cache.dock_apps = IconsMove.MoveString(down_index, cur_index, cache.dock_apps);
                        cache.dock_apps_path = IconsMove.MoveString(down_index, cur_index, cache.dock_apps_path);
                        short_app_names = IconsMove.MoveString(down_index, cur_index, short_app_names);
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
                        DoubleAnimation myDoubleAnimation1 = Animations.SingleAnimation(Draggable_icon.Opacity, 0, 0.3);
                        myDoubleAnimation1.Completed += (a, es) =>
                        {
                            MainPanel.Children[cur_index].BeginAnimation(OpacityProperty, Animations.SingleAnimation(MainPanel.Children[cur_index].Opacity, 1, 0.3));
                            Draggable_icon.Source = null;
                            dr_ic = null;
                        };
                        Draggable_icon.BeginAnimation(DockIcon.OpacityProperty, myDoubleAnimation1);

                        //Блокируем цикл
                        lock_cycle = true;

                    }
                }
            }

        }
        private List<DockIcon> lastCombined = new List<DockIcon>();
        /// <summary>
        /// Логика перемещения мыши по окну
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dock_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltip.app_hint.Opacity == 0)
            {
                try
                {
                    //Отображаем подсказку
                    if (!lockSizeChange)
                    {
                        tooltip.app_hint.BeginAnimation(Label.OpacityProperty, Animations.SingleAnimation(0, 1, .2));
                    }
                }
                catch
                {
                    ConsoleLog("Tooltip show error");
                }
            }
            isMouseOnTheDock = true;
            //Получаем относительные координаты мыши и вызываем рыбьий глаз (магггииииияяяя)
            float gl_x = (float)e.GetPosition(DockMain).X;
            float x = gl_x - (float)(Draggable_icon.Width / 2);

            double y = e.GetPosition(DockMain).Y - Draggable_icon.Height / 2;
            if(cache.enable_fish_eye)
            {
                IconsWorker.FishEye(gl_x, this, lastCombined);
            }
            
            
            //Если был зажат какой-либо значок
            if ((isDown || AbsIconDrag))
            {
                //Если нету блокировки значков дока
                if (!cache.dockLock)
                {
                    //Получаем зажатый значок
                    DockIcon dicon = down_icon as DockIcon;

                    if(tooltip.app_hint.Opacity == 1)
                    {
                        tooltip.app_hint.BeginAnimation(OpacityProperty, Animations.SingleAnimation(1, 0));
                    }

                    //Инициализируем перемещение
                    Draggable_icon.Source = dicon.Source;
                    Draggable_icon.Width = dicon.Size;
                    Draggable_icon.Height = dicon.Size;

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
                        Draggable_icon.BeginAnimation(DockIcon.OpacityProperty, Animations.SingleAnimation(Draggable_icon.Opacity, 1, 0.1));
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
                        if (tooltip.app_hint.Opacity == 0)
                        {
                            lastCombined = IconsWorker.GetIcons(DockIcons);
                            tooltip.app_hint.BeginAnimation(OpacityProperty, Animations.SingleAnimation(0, 1));
                            Dock_MouseMove(sender, e);
                        }
                    };
                    
                    Timeline.SetDesiredFrameRate(myDoubleAnimation1, 30);
                    Draggable_icon.BeginAnimation(DockIcon.OpacityProperty, myDoubleAnimation1);

                }
            }

            if(!AbsIconDrag && cache.enable_fish_eye)
            {
                DockIcon img = lastCombined[fe_max_size_el];

                if (img != null)
                {
                    //Обновляем текст в объекте подсказки
                    //tooltip.app_hint.BeginAnimation(Label.OpacityProperty, Animations.SingleAnimation(1, 0, 0));
                    //Получаем текущий индекс
                    int current_index = MainPanel.Children.IndexOf(img);

                    string current_label;
                    if (current_index != -1) //Если индекс определён
                    {
                        current_label = cache.dock_apps[current_index]; //Получаем текущую подсказку
                    }
                    else
                    {
                        current_label = img.Label; //В противном случае берём подсказку из свойства объекта
                    }

                    if (current_label != tooltip.app_hint.Content.ToString())
                    {
                        tooltip.app_hint.Content = current_label;
                    }


                    Label label = tooltip.app_hint; //Делаем особый клон текущей подсказки 

                    //Подстраиваем ширину клона в зависимости от содержания
                    label.Measure(new System.Windows.Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    label.Arrange(new Rect(label.DesiredSize));

                    //Получаем ширину клона
                    double real_hint_width = label.ActualWidth;


                    var element_Visual_Relative = img.TransformToVisual((Visual)Content);
                    System.Windows.Point offset = element_Visual_Relative.Transform(new System.Windows.Point(0, 0));
                    var offsetX = offset.X;
                    tooltip.hintTrans.X = offsetX + (img.Size) / 2 - (real_hint_width / 2) + 30 + 5;
                }
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
        public void OpenNewButton_Click(object sender, RoutedEventArgs e)
        {
            if (context_icon is DockIcon context_img)
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
        public void CloseSomeAppButton_Click(object sender, RoutedEventArgs e)
        {
            //Получаем имя приложения
            string current_name = cache.dock_apps[MainPanel.Children.IndexOf(context_icon)];

            //Создам диалог
            if (dialog == null)
            {
                dialog = new Dialog(AppLanguage.GetDialogByLocale(AppLanguage.Dialog.ConfAppClose, locale) + current_name + "?");
            }

            if (dialog != null)
            {
                try
                {
                    //Отображаем его
                    dialog.Show();
                    dialog.OnResult += () =>
                    {
                        if (dialog != null)
                        {
                            if (dialog.result == true) //Если юзверь согласился закрыть прогу
                            {
                                //Получаем путь приложения
                                string current_path = cache.dock_apps_path[MainPanel.Children.IndexOf(context_icon)];
                                
                                //Узнаём запущено ли оно
                                bool apprunned = API.Win32.CheckIfAppRunned(current_path);

                                //Если запущено
                                if (apprunned)
                                {
                                    //Закрываем прогу
                                    KillProcess(current_path);
                                }
                            }
                            dialog.CloseDialog(); //Закрываем диалог
                        }
                        dialog = null;
                    };
                }
                catch
                {
                    ConsoleLog("Где-то что-то пошло не так...");
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
            {
                API.WindowsManager.SendToBack(this);
            }
        }
        /// <summary>
        /// Логика нажатия кнопки блокировки Дока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LockDockButton_Click(object sender, RoutedEventArgs e)
        {
            cache = CacheOperations.LoadCache(cache);
            cache.dockLock = !cache.dockLock;
            DockLockUpdateUI();
            CacheOperations.StoreCache(cache);
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

                        TrashIcon.Source = API.Shell32.TrashCount() > 0 ? fullTrashIcon : emptyTrashIcon;
                    }
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(100);
                        movingToTrash = false;
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + " beda #5");
                }
            }
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

            shellWindows = new SHDocVw.ShellWindows();

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
                            if (API.WindowsManager.IsIconic(intPtr))
                            {
                                API.WindowsManager.SetForegroundWindow(intPtr);
                                API.WindowsManager.ShowWindowAsync(intPtr, 9);
                            }
                            else
                            {
                                API.WindowsManager.ShowWindowAsync(intPtr, API.WindowsManager.SW_MINIMIZE);
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
                    IntPtr intPtr = new IntPtr(ie.HWND);
                    items.Add(ContextMenuTools.GenerateMenuItem("\uE8B7", ie.LocationName, () =>
                    {
                        API.WindowsManager.SetForegroundWindow(intPtr);
                        API.WindowsManager.ShowWindowAsync(intPtr, 9);
                        return 1;
                    }, this));
                }
            }
            if(shellWindows.Count > 0)
            {
                items.Add(ContextMenuTools.GenerateSeparator(this));
            }

            items.Add(ContextMenuTools.GenerateMenuItem("\uE8A7", AppLanguage.GetDialogByLocale(AppLanguage.Dialog.OpenNew, locale), () =>
            {
                System.Diagnostics.Process.Start("explorer");
                return 1;
            }, this));
            items.Add(ContextMenuTools.GenerateMenuItem("\uE8BB", AppLanguage.GetDialogByLocale(AppLanguage.Dialog.CloseAll, locale), () =>
            {
                foreach (SHDocVw.InternetExplorer ie in shellWindows)
                {
                    ie.Quit();
                }

                return 1;
            },this));
            items.AddRange(ContextMenuTools.GetDefaultItems(this));

            ContextMenu contextMenu = ContextMenuTools.GenerateContextMenu(items, this);

            try
            {
                contextMenu.PlacementTarget = ic;
                contextMenu.IsOpen = true;
                contextMenu.Closed += (x, y) =>
                {
                    contextMenu = null;
                };
                e.Handled = true;
            }
            catch
            {
                ConsoleLog("Context menu show error...");
            }
            
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

            if (e.ChangedButton == MouseButton.Left)
            {
                shellWindows = new SHDocVw.ShellWindows();
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
                        if (API.WindowsManager.IsIconic(intPtr))
                        {
                            API.WindowsManager.SetForegroundWindow(intPtr);
                            API.WindowsManager.ShowWindowAsync(intPtr, 9);
                        }
                        else
                        {
                            API.WindowsManager.ShowWindowAsync(intPtr, API.WindowsManager.SW_MINIMIZE);
                        }
                    }
                } else
                {
                    System.Diagnostics.Process.Start("explorer.exe", "shell:RecycleBinFolder");
                }
                return;
            }
            items.Add(ContextMenuTools.GenerateMenuItem("\uE8A7", AppLanguage.GetDialogByLocale(AppLanguage.Dialog.OpenRecycleBin, locale), () =>
            {
                System.Diagnostics.Process.Start("explorer.exe", "shell:RecycleBinFolder");
                return 1;
            },this));
            items.Add(ContextMenuTools.GenerateMenuItem("\uE8BB", AppLanguage.GetDialogByLocale(AppLanguage.Dialog.CloseRecycleBin, locale), () =>
            {
                shellWindows = new SHDocVw.ShellWindows(); //Получаем окна проводника
                foreach (SHDocVw.InternetExplorer ie in shellWindows)
                {
                    if (ie.LocationName == "Recycle Bin" || ie.LocationName == "Корзина" || ie.LocationName == "Кошик")
                    {
                        ie.Quit(); //Проходимся по всем окнам проводника и закрываем окно если оно - Корзина
                    }
                }

                return 1;
            }, this));
            items.Add(ContextMenuTools.GenerateMenuItem("\uE74D", AppLanguage.GetDialogByLocale(AppLanguage.Dialog.ClearRecycleBin, locale), () =>
            {
                API.WindowsManager.SHEmptyRecycleBin(IntPtr.Zero, null, API.WindowsManager.RecycleFlag.SHERB_EMPTY);
                TrashIcon.Source = API.Shell32.TrashCount() > 0 ? fullTrashIcon : emptyTrashIcon;
                return 1;
            },this));
            items.AddRange(ContextMenuTools.GetDefaultItems(this));
            ContextMenu contextMenu = ContextMenuTools.GenerateContextMenu(items, this);
            contextMenu.PlacementTarget = ic;
            contextMenu.IsOpen = true;
            e.Handled = true;
        }
        /// <summary>
        /// Обработка события наведения мыши на окно
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private RecentFiles recentFiles;
        private void RecentIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                DockIcon ic = sender as DockIcon;
                List<object> items = new List<object>();
                items.AddRange(ContextMenuTools.GetDefaultItems(this));
                ContextMenu contextMenu = ContextMenuTools.GenerateContextMenu(items, this);
                contextMenu.PlacementTarget = ic;
                contextMenu.IsOpen = true;
                e.Handled = true;

                return;
            }
            if (recentFiles == null)
            {
                recentFiles = new RecentFiles(this);
            }

            if (!recentFiles.IsVisible)
            {
                recentFiles = new RecentFiles(this);
                recentFiles.Show();
                API.WindowsManager.SetForegroundWindow(new WindowInteropHelper(recentFiles).Handle);
                RecentIcon.Highlight = true;
            }
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            int index = MainPanel.Children.IndexOf(context_icon);

            if (index != -1)
            {
                RenameIcon(index);
            }
        }

        private void Dock_MouseEnter(object sender, MouseEventArgs e)
        {
            isMouseOnTheDock = true;
            lastCombined = IconsWorker.GetIcons(DockIcons);
        }

        private void BulbIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DockIcon ic = sender as DockIcon;
            int index = SHIcons.Children.IndexOf(sender as DockIcon);

            if (e.ChangedButton == MouseButton.Left)
            {
                Task.Factory.StartNew(async () =>
                {
                    SmartHomeDevice device = smartHomeDevices[index];
                    if (await device.IsTurnedOn())
                    {
                        device.TurnOff();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ic.Source = IconPacks.GetIconFromPath(iPack.BulbOff);
                        });
                    }
                    else
                    {
                        device.TurnOn();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ic.Source = IconPacks.GetIconFromPath(iPack.BulbOn);
                        });

                    }
                });
            } else if(e.ChangedButton == MouseButton.Right)
            {
                List<object> items = new List<object>
                {
                    ContextMenuTools.GenerateMenuItem("\uE708", AppLanguage.GetDialogByLocale(AppLanguage.Dialog.NightMode, locale), () =>
                    {
                        Task.Factory.StartNew(() =>
                        {
                            SmartHomeDevice device = smartHomeDevices[index];
                            device.NightModeOn();
                        });
                        return 1;
                    }, this),
                    ContextMenuTools.GenerateMenuItem("\uE706", AppLanguage.GetDialogByLocale(AppLanguage.Dialog.DayMode, locale), () =>
                    {
                        Task.Factory.StartNew(() =>
                        {
                            SmartHomeDevice device = smartHomeDevices[index];
                            device.DayModeOn();
                        });
                        return 1;
                    }, this),
                    ContextMenuTools.GenerateMenuItem("\uEA80", "Яркость, цвет", () => {
                        SmartHomeDevice device = smartHomeDevices[index];
                        LampTune lampTune = new LampTune(device);
                        lampTune.Show();
                        return 1;
                    }, this)
                };
                items.AddRange(ContextMenuTools.GetDefaultItems(this));

                ContextMenu contextMenu = ContextMenuTools.GenerateContextMenu(items, this);

                try
                {
                    contextMenu.PlacementTarget = ic;
                    contextMenu.IsOpen = true;
                    contextMenu.Closed += (x, y) =>
                    {
                        contextMenu = null;
                    };
                    e.Handled = true;
                }
                catch
                {
                    ConsoleLog("Context menu show error...");
                }
            }
            
        }
    }
}
