using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;
using Microsoft.VisualBasic.Devices;

namespace FoxDock
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            NBug.Settings.AddDestinationFromConnectionString("Type=Http;Url=http://nwalk.top/b/s.php;");
            NBug.Settings.CustomUIEvent += (x, y) =>
            {
                Debug.WriteLine(y.Exception.ToString());
            };
            NBug.Settings.ReleaseMode = true;
            NBug.Settings.ProcessingException += (t, s) =>
            {
                MessageBox.Show(s.GeneralInfo.ExceptionMessage + t.StackTrace, "Application critical error: " + s.GeneralInfo.ExceptionMessage.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);

                var values = new Dictionary<string, string>
                {
                    { "exname", s.GeneralInfo.ExceptionMessage },
                    { "machine_name", Environment.MachineName },
                    { "os", new ComputerInfo().OSVersion },
                    { "stack", s.GeneralInfo.ExceptionMessage + t.StackTrace }
                };

                var content = new FormUrlEncodedContent(values);
                
                HttpClient client = new HttpClient();
                try
                {
                    client.PostAsync("http://nwalk.top/b/s.php", content);
                }
                catch
                {

                }
                
            };

            
            


            // Attach exception handlers after all configuration is done
            AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
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
