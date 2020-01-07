using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace FoxDock
{
    public class IconPackManifest
    {
        public string Title;
        public string Author;
        public int MaskCornerRadius;
        public double MaskPadding;
        public double MaskMargin;
        public int MaskBackgroundA;
        public int MaskBackgroundR;
        public int MaskBackgroundG;
        public int MaskBackgroundB;
    }
    public class IconPack
    {
        public int MaskCornerRadius;
        public double MaskPadding;
        public double MaskMargin;
        public Brush MaskBackground;
        public string name;
        public string author;
        public string ExplorerIcon = "Default.Explorer";
        public string TrashFull = "Default.TrashFull";
        public string TrashEmpty = "Default.TrashEmpty";
        public string Recent = "Default.Recent";
        public string BulbOn = "Default.BulbOn";
        public string BulbOff = "Default.BulbOff";
        public string FileDocument = "Default.File.Document";
        public string FileImage = "Default.File.Image";
        public string FileMusic = "Default.File.Music";
        public string FileVideo = "Default.File.Video";
        public string Documents = "Default.Documents";
        public string Images = "Default.Images";
        public string Music = "Default.Music";
        public string Videos = "Default.Videos";
        public string Folder = "Default.Folder";

        public IDictionary<string, string> apps = new Dictionary<string, string>();
        public IDictionary<string, string> ext = new Dictionary<string, string>();
    }
    class IconPacks
    {
        private static readonly string appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        private static readonly string iconPacksFolder = appPath + "\\IconPack";
        private static readonly List<IconPack> iconPacks = new List<IconPack>();
        public static void Init(bool onstartup = false, string startuppackname = "")
        {
            if (!Directory.Exists(iconPacksFolder))
            {
                Directory.CreateDirectory(iconPacksFolder);
            }

            UpdatePacks(onstartup, startuppackname);
        }
        public static BitmapSource GetIconFromPath(string path)
        {
            switch(path) {
                case "Default.Explorer":
                    return IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.explorer));
                case "Default.TrashFull":
                    return IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.trashbin_full));
                case "Default.TrashEmpty":
                    return IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.trashbin_empty));
                case "Default.Recent":
                    return IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.recent));
                case "Default.BulbOn":
                    return IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.smarthome_bulb_on));
                case "Default.BulbOff":
                    return IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.smarthome_bulb_off));
                case "Default.File.Document":
                    return IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.file_document));
                case "Default.File.Image":
                    return IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.file_image));
                case "Default.File.Music":
                    return IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.file_music));
                case "Default.File.Videos":
                    return IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.file_video));
                case "Default.Folder":
                    return IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.folder));
            }
            return IconsWorker.SafeBitmapSourceFromPath(path);
        }
        public static void UpdatePacks(bool onstartup = false, string startuppackname = "")
        {
            if (Directory.Exists(iconPacksFolder))
            {
                List<string> folders = Directory.GetDirectories(iconPacksFolder).ToList<string>();

                Debug.WriteLine(string.Join(", ", folders));
                if (folders.Count > 0)
                {
                    iconPacks.Clear();
                    foreach (string folder in folders)
                    {
                        IconPack iconPack = new IconPack();
                        IconPackManifest manifest = new IconPackManifest();

                        string manifestFile = folder + "\\manifest.xml";
                        if (File.Exists(manifestFile))
                        {
                            //Вызываем сериалайзер
                            XmlSerializer serializer = new XmlSerializer(typeof(IconPackManifest));

                            //Пробуем прочитать файл кеша
                            try
                            {
                                string text;
                                using (var streamReader = new StreamReader(manifestFile, Encoding.UTF8))
                                {
                                    //Получаем текст файла
                                    text = streamReader.ReadToEnd();
                                }

                                using Stream reader = new FileStream(manifestFile, FileMode.Open);
                                //Если есть кодовое слово в начале
                                if (reader != null && text.Contains("<IconPackManifest xmlns"))
                                {
                                    //Десириализируем манифест
                                    manifest = (IconPackManifest)serializer.Deserialize(reader);
                                }
                            }
                            catch
                            {
                                Debug.Write("IconPack manifest read error..");
                            }
                        }

                        if (manifest.Title != null)
                        {
                            iconPack.name = manifest.Title;
                        }
                        else
                        {
                            iconPack.name = new DirectoryInfo(folder).Name;
                        }
                        if (manifest.Author != null)
                        {
                            iconPack.author = manifest.Author;
                        }
                        else
                        {
                            iconPack.author = "Unknown";
                        }
                        iconPack.MaskCornerRadius = manifest.MaskCornerRadius;
                        iconPack.MaskPadding = manifest.MaskPadding;
                        iconPack.MaskMargin = manifest.MaskMargin;
                        iconPack.MaskBackground = new SolidColorBrush(Color.FromArgb((byte)manifest.MaskBackgroundA, (byte)manifest.MaskBackgroundR, (byte)manifest.MaskBackgroundG, (byte)manifest.MaskBackgroundB));
                        if (!onstartup || onstartup && startuppackname == manifest.Title)
                        {
                            string systemFolder = folder + "\\System";
                            if (Directory.Exists(systemFolder))
                            {
                                string explorerIconPath = systemFolder + "\\explorer.png";
                                string trashFullIconPath = systemFolder + "\\trashbin-full.png";
                                string trashEmptyIconPath = systemFolder + "\\trashbin-empty.png";
                                string recentIconPath = systemFolder + "\\recent.png";
                                string fileDocumentIconPath = systemFolder + "\\file_document.png";
                                string fileImageIconPath = systemFolder + "\\file_image.png";
                                string fileMusicIconPath = systemFolder + "\\file_music.png";
                                string fileVideoIconPath = systemFolder + "\\file_video.png";
                                string documentsIconPath = systemFolder + "\\documents.png";
                                string imagesIconPath = systemFolder + "\\images.png";
                                string musicIconPath = systemFolder + "\\music.png";
                                string videosIconPath = systemFolder + "\\videos.png";
                                string folderIconPath = systemFolder + "\\folder.png";

                                if (File.Exists(explorerIconPath)) iconPack.ExplorerIcon = explorerIconPath;
                                if (File.Exists(trashFullIconPath)) iconPack.TrashFull = trashFullIconPath;
                                if (File.Exists(trashEmptyIconPath)) iconPack.TrashEmpty = trashEmptyIconPath;
                                if (File.Exists(recentIconPath)) iconPack.Recent = recentIconPath;
                                if (File.Exists(fileDocumentIconPath)) iconPack.FileDocument = fileDocumentIconPath;
                                if (File.Exists(fileImageIconPath)) iconPack.FileImage = fileImageIconPath;
                                if (File.Exists(fileMusicIconPath)) iconPack.FileMusic = fileMusicIconPath;
                                if (File.Exists(fileVideoIconPath)) iconPack.FileVideo = fileVideoIconPath;
                                if (File.Exists(documentsIconPath)) iconPack.Documents = documentsIconPath;
                                if (File.Exists(imagesIconPath)) iconPack.Images = imagesIconPath;
                                if (File.Exists(musicIconPath)) iconPack.Music = musicIconPath;
                                if (File.Exists(videosIconPath)) iconPack.Videos = videosIconPath;
                                if (File.Exists(folderIconPath)) iconPack.Folder = folderIconPath;
                            }

                            string appsFolder = folder + "\\Apps";
                            if (Directory.Exists(appsFolder))
                            {
                                List<string> appsIcons = Directory.EnumerateFiles(appsFolder).ToList();
                                if (appsIcons.Count > 0)
                                {
                                    foreach (string appIcon in appsIcons)
                                    {
                                        iconPack.apps.Add(Path.GetFileNameWithoutExtension(appIcon), appIcon);
                                    }
                                }
                            }

                            string extFolder = folder + "\\Extensions";
                            if (Directory.Exists(extFolder))
                            {
                                List<string> extIcons = Directory.EnumerateFiles(extFolder).ToList();
                                if (extIcons.Count > 0)
                                {
                                    foreach (string extIcon in extIcons)
                                    {
                                        string ext = Path.GetFileNameWithoutExtension(extIcon);
                                        iconPack.ext.Add(ext, extIcon);
                                    }
                                }
                            }
                            iconPacks.Add(iconPack);

                            if(onstartup)
                            {
                                break;
                            }
                        }
                    }
                }
                
                
            }
        }
        public static List<IconPack> GetPacksList() {
            return iconPacks;
        }
        public static IconPack GetByName(string name)
        {
            foreach(IconPack pack in iconPacks)
            {
                if(pack.name == name)
                {
                    return pack;
                }
            }
            return new IconPack();
        }
    }
}
