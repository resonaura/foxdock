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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FoxDock
{
    /// <summary>
    /// Interaction logic for Dock.xaml
    /// </summary>
    public partial class Dock : UserControl
    {
        //Подключаем кеш
        public static Cache cache = new Cache();

        //Основные таймеры
        private readonly System.Timers.Timer mainTimer = new System.Timers.Timer();

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
        public Dock()
        {
            InitializeComponent();



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

            //Выполняем логику размера значков
            if (!isInitedAS)
            {
                size = (int)(defsize * cache.scaleFactor);
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
            //try { API.WindowsManager.window = this; } catch { ConsoleLog("Ошибка задания основного окна для WindowAPI"); }


            ExplorerIcon.Source = IconPacks.GetIconFromPath(iPack.ExplorerIcon);
            //Получаем значки Проводника и Корзины и задаём их для соответствующих виджетов на Доке
            //ExplorerIcon.Source = IconsWorker.GetSourceFromIcon(IconsWorker.GetSystemIcon(FileTools.GetExplorerPath()));

            //RecentIcon.Source = IconsWorker.GetSourceFromIcon(IconsWorker.GetSystemIcon(FileTools.GetRecentsPath()));
            RecentIcon.Source = IconPacks.GetIconFromPath(iPack.Recent);

            TrashIcon.Source = API.Shell32.TrashCount() > 0 ? fullTrashIcon : emptyTrashIcon;

            //Получаем высоту панели задач

            //Обработчик события успешной загрузки дока
            void handler(object s, RoutedEventArgs e)
            {

                if (cache.enableStarDust)
                {
                    StarDust.Visibility = Visibility.Visible;
                }
            }

            Loaded += handler;
        }
    }
}
