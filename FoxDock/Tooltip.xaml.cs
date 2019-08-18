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
        }
    }
}
