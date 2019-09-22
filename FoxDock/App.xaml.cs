using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FoxDock
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            SourceChord.FluentWPF.ResourceDictionaryEx.GlobalTheme = SourceChord.FluentWPF.ElementTheme.Dark;

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                var fileName = args[1];
                if (File.Exists(fileName))
                {
                    var extension = Path.GetExtension(fileName);
                    if (extension == ".icp")
                    {
                        MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure you want to install this icon pack?", "Install", System.Windows.MessageBoxButton.YesNo);
                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            string appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                            string iconPackFolder = appPath + "\\IconPack";
                            try
                            {
                                ZipFile.ExtractToDirectory(fileName, iconPackFolder);
                                MessageBox.Show("IconPack install successfully!");
                            }
                            catch
                            {
                                MessageBox.Show("IconPack is not valid!");
                            }
                            
                            
                        }
                    }
                    Environment.Exit(0); //Убиваем процесс
                }
            }
        }
    }
}
