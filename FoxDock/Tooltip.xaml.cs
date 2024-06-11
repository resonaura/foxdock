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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FoxDock
{
    /// <summary>
    /// Логика взаимодействия для Tooltip.xaml
    /// </summary>
    public partial class Tooltip : Window
    {
        public static AppLanguage.Locale locale = AppLanguage.GetSystemLocale();
        public Tooltip()
        {
            InitializeComponent();

            this.Title = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.Tooltip, locale);
            this.Loaded += Tooltip_Loaded;
        }

        private void Tooltip_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)API.Win32.GetWindowLong(wndHelper.Handle, (int)API.Win32.GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)API.Win32.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            API.Win32.SetWindowLong(wndHelper.Handle, (int)API.Win32.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }
    }
}
