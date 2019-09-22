using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace FoxDock
{
    public class IconPackManifest
    {
        public string Title;
        public string Author;
    }
    public class IconPack
    {
        public string name;
        public string author;
        public BitmapSource ExplorerIcon = IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.explorer));
        public BitmapSource TrashFull = IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.trashbin_full));
        public BitmapSource TrashEmpty = IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.trashbin_empty));
        public BitmapSource Recent = IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.recent));
        public BitmapSource FileDocument = IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.file_document));
        public BitmapSource FileImage = IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.file_image));
        public BitmapSource FileMusic = IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.file_music));
        public BitmapSource FileVideo = IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.file_video));
        public BitmapSource Documents = IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.documents));
        public BitmapSource Images = IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.images));
        public BitmapSource Music = IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.music));
        public BitmapSource Videos = IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.videos));
        public BitmapSource Folder = IconsWorker.GetSourceFromBitmap(IconsWorker.Optimize(FoxDock.Properties.Resources.folder));
        public IDictionary<string, BitmapSource> apps = new Dictionary<string, BitmapSource>();
    }
    class IconPacks
    {
        private static readonly string appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        private static readonly string iconPacksFolder = appPath + "\\IconPack";
        private static readonly List<IconPack> iconPacks = new List<IconPack>();
        public static void Init()
        {
            if (!Directory.Exists(iconPacksFolder))
            {
                Directory.CreateDirectory(iconPacksFolder);
            }

            UpdatePacks();
        }
        public static void UpdatePacks()
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
                        if(File.Exists(manifestFile))
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

                                using (Stream reader = new FileStream(manifestFile, FileMode.Open))
                                {
                                    //Если есть кодовое слово в начале
                                    if (reader != null && text.Contains("<IconPackManifest xmlns"))
                                    {
                                        //Десириализируем манифест
                                        manifest = (IconPackManifest)serializer.Deserialize(reader);
                                    }
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
                        } else
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

                            if (File.Exists(explorerIconPath)) iconPack.ExplorerIcon = IconsWorker.SafeBitmapSourceFromPath(explorerIconPath);
                            if (File.Exists(trashFullIconPath)) iconPack.TrashFull = IconsWorker.SafeBitmapSourceFromPath(trashFullIconPath);
                            if (File.Exists(trashEmptyIconPath)) iconPack.TrashEmpty = IconsWorker.SafeBitmapSourceFromPath(trashEmptyIconPath);
                            if (File.Exists(recentIconPath)) iconPack.Recent = IconsWorker.SafeBitmapSourceFromPath(recentIconPath);
                            if (File.Exists(fileDocumentIconPath)) iconPack.FileDocument = IconsWorker.SafeBitmapSourceFromPath(fileDocumentIconPath);
                            if (File.Exists(fileImageIconPath)) iconPack.FileImage = IconsWorker.SafeBitmapSourceFromPath(fileImageIconPath);
                            if (File.Exists(fileMusicIconPath)) iconPack.FileMusic = IconsWorker.SafeBitmapSourceFromPath(fileMusicIconPath);
                            if (File.Exists(fileVideoIconPath)) iconPack.FileVideo = IconsWorker.SafeBitmapSourceFromPath(fileVideoIconPath);
                            if (File.Exists(documentsIconPath)) iconPack.Documents = IconsWorker.SafeBitmapSourceFromPath(documentsIconPath);
                            if (File.Exists(imagesIconPath)) iconPack.Images = IconsWorker.SafeBitmapSourceFromPath(imagesIconPath);
                            if (File.Exists(musicIconPath)) iconPack.Music = IconsWorker.SafeBitmapSourceFromPath(musicIconPath);
                            if (File.Exists(videosIconPath)) iconPack.Videos = IconsWorker.SafeBitmapSourceFromPath(videosIconPath);
                            if (File.Exists(folderIconPath)) iconPack.Folder = IconsWorker.SafeBitmapSourceFromPath(folderIconPath);
                        }

                        string appsFolder = folder + "\\Apps";
                        if (Directory.Exists(appsFolder))
                        {
                            List<string> appsIcons = Directory.EnumerateFiles(appsFolder).ToList();
                            if(appsIcons.Count > 0)
                            {
                                foreach(string appIcon in appsIcons)
                                {
                                    iconPack.apps.Add(Path.GetFileNameWithoutExtension(appIcon), IconsWorker.SafeBitmapSourceFromPath(appIcon));
                                } 
                            }
                        }
                        iconPacks.Add(iconPack);
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
