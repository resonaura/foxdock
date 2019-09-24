using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

namespace FoxDock
{
    class IconsWorker
    {
        /// <summary>
        /// Функция обрезки пустого пространства из битмапа
        /// </summary>
        /// <param name="bmp">Изначальный рисунок</param>
        /// <returns>Конечный рисунок</returns>
        private static Bitmap CropWhiteSpace(Bitmap bmp)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            int white = 0xffffff;

            bool allWhiteRow(int r)
            {
                for (int i = 0; i < w; ++i)
                    if ((bmp.GetPixel(i, r).ToArgb() & white) != white)
                        return false;
                return true;
            }

            bool allWhiteColumn(int c)
            {
                for (int i = 0; i < h; ++i)
                    if ((bmp.GetPixel(c, i).ToArgb() & white) != white)
                        return false;
                return true;
            }

            int topmost = 0;
            for (int row = 0; row < h; ++row)
            {
                if (!allWhiteRow(row))
                    break;
                topmost = row;
            }

            int bottommost = 0;
            for (int row = h - 1; row >= 0; --row)
            {
                if (!allWhiteRow(row))
                    break;
                bottommost = row;
            }

            int leftmost = 0, rightmost = 0;
            for (int col = 0; col < w; ++col)
            {
                if (!allWhiteColumn(col))
                    break;
                leftmost = col;
            }

            for (int col = w - 1; col >= 0; --col)
            {
                if (!allWhiteColumn(col))
                    break;
                rightmost = col;
            }

            if (rightmost == 0) rightmost = w; // As reached left
            if (bottommost == 0) bottommost = h; // As reached top.

            int croppedWidth = rightmost - leftmost;
            int croppedHeight = bottommost - topmost;

            if (croppedWidth == 0) // No border on left or right
            {
                leftmost = 0;
                croppedWidth = w;
            }

            if (croppedHeight == 0) // No border on top or bottom
            {
                topmost = 0;
                croppedHeight = h;
            }

            try
            {
                var target = new Bitmap(croppedWidth, croppedHeight);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(bmp,
                      new RectangleF(0, 0, croppedWidth, croppedHeight),
                      new RectangleF(leftmost, topmost, croppedWidth, croppedHeight),
                      GraphicsUnit.Pixel);
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(
                  string.Format("Values are topmost={0} btm={1} left={2} right={3} croppedWidth={4} croppedHeight={5}", topmost, bottommost, leftmost, rightmost, croppedWidth, croppedHeight),
                  ex);
            }
        }

        /// <summary>
        /// Функция для заменты прозрачности на определённый цвет
        /// </summary>
        /// <param name="bitmap">Изначальный рисунок</param>
        /// <param name="color">Конечный рисунок</param>
        /// <returns></returns>
        static Bitmap Process(Bitmap bitmap, Color color)
        {
            Bitmap temp = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(temp);
            g.Clear(color);
            g.DrawImage(bitmap, Point.Empty);
            return temp;
        }
        /// <summary>
        /// Функция получения списка Shell Image
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static API.Shell32.IImageList GetShellList(int id)
        {
            Guid riid = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
            API.Shell32.SHGetImageList(id, ref riid, out API.Shell32.IImageList ppv);

            return ppv;
        }
        /// <summary>
        /// Функция получения Shell иконки
        /// </summary>
        /// <param name="i">Индекс</param>
        /// <returns>Иконка</returns>
        public static Icon GetShellIcon(int i)
        {
            try
            {
                API.Shell32.IImageList ppv = GetShellList(4);
                IntPtr picon = IntPtr.Zero;
                int flags = 0;

                ppv.GetIcon(i, flags, ref picon);
                if (picon != null && System.Drawing.Icon.FromHandle(picon) != null)
                {
                    Icon icon = (Icon)System.Drawing.Icon.FromHandle(picon).Clone();
                    double realwidth = CropWhiteSpace(Process(icon.ToBitmap(), Color.FromArgb(255, 255, 255, 255))).Width;
                    if (realwidth <= 100)
                    {
                        API.Shell32.IImageList ppv_low = GetShellList(2);
                        IntPtr picon2 = IntPtr.Zero;

                        ppv_low.GetIcon(i, flags, ref picon2);
                        if (picon2 != null && System.Drawing.Icon.FromHandle(picon2) != null)
                        {
                            icon = (Icon)System.Drawing.Icon.FromHandle(picon2).Clone();
                        }
                    }

                    return icon;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                //Если таки ошибка - выводим её в консоль
                Debug.WriteLine(ex.ToString());
            }
            return null;
        }
        public static void UpdateDockIcons(Dock dock, int index = -1)
        {
            if(dock.MainPanel.Children.Count > 0)
            {
                int i = 0;
                foreach(DockIcon icon in dock.MainPanel.Children)
                {
                    if(index == -1 || (index != -1 && i == index))
                    {
                        if (i < Dock.cache.dock_apps_path.Count)
                        {
                            icon.Source.Freeze();
                            icon.Source = SourceFromPath(Dock.cache.dock_apps_path[i], dock.iPack);
                        }
                    }
                    
                    i++;
                }
            }
        }
        /// <summary>
        /// Функция получения значка по пути
        /// </summary>
        /// <param name="path">Путь к файлу/папке</param>
        /// <returns></returns>
        public static BitmapSource SourceFromPath(string path, IconPack iPack)
        {
            //Тут всё почти так же, как и в предыдущей функции. Мне лень описывать)
            try
            {
                if(Directory.Exists(path))
                {
                    BitmapSource source = iPack.Folder;

                    string myDocuments = API.Shell32.GetSFPath(Environment.SpecialFolder.MyDocuments);
                    string commonDocuments = API.Shell32.GetSFPath(Environment.SpecialFolder.CommonDocuments);
                    string myMusic = API.Shell32.GetSFPath(Environment.SpecialFolder.MyMusic);
                    string commonMusic = API.Shell32.GetSFPath(Environment.SpecialFolder.CommonMusic);
                    string myPictures = API.Shell32.GetSFPath(Environment.SpecialFolder.MyPictures);
                    string commonPictures = API.Shell32.GetSFPath(Environment.SpecialFolder.CommonPictures);
                    string myVideos = API.Shell32.GetSFPath(Environment.SpecialFolder.MyVideos);
                    string commonVideos = API.Shell32.GetSFPath(Environment.SpecialFolder.CommonVideos);

                    if (path == myDocuments || path == commonDocuments) source = iPack.Documents;
                    if (path == myMusic || path == commonMusic) source = iPack.Music;
                    if (path == myPictures || path == commonPictures) source = iPack.Images;
                    if (path == myVideos || path == commonVideos) source = iPack.Videos;

                    return source;
                }
                if(File.Exists(path))
                {
                    string mimeType = MimeMapping.GetMimeMapping(path);
                    if(mimeType != "")
                    {
                        string type = mimeType.Split('/')[0];
                        switch(type)
                        {
                            case "image":
                                return iPack.FileImage;
                            case "audio":
                                return iPack.FileMusic;
                            case "video":
                                return iPack.FileVideo;
                            case "application":
                                string extension = Path.GetExtension(path).ToUpper();
                                if (extension != ".EXE" && extension != ".LNK")
                                {
                                    return GetSourceFromBitmap(Properties.Resources.file_document);
                                } else if(extension == ".EXE")
                                {
                                    string filename = Path.GetFileNameWithoutExtension(path).ToLower();

                                    if(iPack.apps.ContainsKey(filename))
                                    {
                                        if(iPack.apps[filename] != null)
                                        {
                                            return iPack.apps[filename];
                                        } 
                                    }
                                }
                                break;

                        }
                    }

                    

                }
                API.Shell32.SHFILEINFO psfi = new API.Shell32.SHFILEINFO();
                int dwFileAttributes = 2048;
                API.Shell32.SHGFI uFlags = API.Shell32.SHGFI.SHGFI_SYSICONINDEX;
                if (API.Shell32.SHGetFileInfo(path, dwFileAttributes, out psfi, (uint)Marshal.SizeOf((object)psfi), uFlags) == 0)
                {
                    return (BitmapSource)null;
                }

                int i = psfi.iIcon;

                return GetSourceFromIcon(GetShellIcon(i));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return (BitmapSource)null;
        }

        /// <summary>
        /// Функция получения значка Корзины
        /// </summary>
        /// <returns>Значок</returns>
        public static Icon GetTrashIcon(bool isFull)
        {
            //Снова неведомая херня с взаимодействием с Win32 API. Писал в состоянии алкогольного опьянения...
            //На всякий случай, чтобы не еб#нуло ошибку всю логику помещаем в try, catch
            try
            {
                int i = 31;
                if (isFull)
                {
                    i = 32;
                }
                return GetShellIcon(i);
            }
            catch (Exception ex)
            {
                //Если таки ошибка - выводим её в консоль
                Debug.WriteLine(ex.ToString());
            }
            return null;

        }
        /// <summary>
        /// Функция получения источника изображения из рисунка
        /// </summary>
        /// <param name="bitmap">Рисунок</param>
        /// <returns>Источник для изображения</returns>
        public static BitmapSource GetSourceFromBitmap(Bitmap bitmap)
        {
            BitmapSource result = null;
            if (bitmap != null)
            {
                IntPtr hbmp = bitmap.GetHbitmap();
                result = Imaging.CreateBitmapSourceFromHBitmap(hbmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                API.GDI32.DeleteObject(hbmp);
            }
            return result;
        }
        /// <summary>
        /// Функция для получения BitmapSource из Icon
        /// </summary>
        /// <param name="icon">Icon</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource GetSourceFromIcon(Icon icon)
        {
            BitmapSource result = null;
            if (icon != null)
            {
                result = GetSourceFromBitmap(Optimize(icon.ToBitmap()));
            }
            return result;
        }
        /// <summary>
        /// Функция для изменения размера рисунка
        /// </summary>
        /// <param name="bmp">Рисунок</param>
        /// <param name="width">Ширина</param>
        /// <param name="height">Высота</param>
        /// <returns>Конечный рисунок</returns>
        private static Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }

            return result;
        }
        /// <summary>
        /// Функция для оптимизации рисунка
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap Optimize(Bitmap bmp)
        {
            return ResizeBitmap(bmp, 128, 128);
        }
        public static BitmapSource SafeBitmapSourceFromPath(string path)
        {
            using (Bitmap bitmap = new Bitmap(path))
            {
                BitmapSource source = GetSourceFromBitmap(Optimize(bitmap));
                return source;
            }
        }
        //=ЕСЛИ(И(x >= -p; x <= p);((-ABS(x) + p/2) / p/2 + 0.25)*2;0)
        private static double TriangleCalc(double i, double x, double p)
        {
            
            double rX = p - x + p + i;
            if (rX >= -p && rX <= p)
            {
                return ((-Math.Abs(rX) + p / 2) / p / 2 + 0.25) * 2;
            } else
            {
                return 0;
            }
        }
        /// <summary>
        /// Локика рыбьего глаза для значков
        /// </summary>
        /// <param name="x">Смещение</param>
        public static void FishEyeForIcons(double x, Dock dock, List<DockIcon> combined)
        {
            //Считаем ширину мнимой линии
            int width = (combined.Count) * (int)(dock.size + combined.First().Margin.Left + dock.size + combined.First().Margin.Right);
            if (width < 300)
            {
                width = 300;
            }

            //Создаём большой массив с точкой мнимой линии
            double[] big_array = new double[(int)width];

            //Дальше немного эльфийской магии (или мне просто лень комментировать)
            int eye_size = 800;
            for (int i = 0; i < eye_size; i++)
            {
                double m_val = 0;
                int peak = i < eye_size / 2 ? i : eye_size - i;
                double eye_result = peak / ((double)eye_size / 2);
                //..Debug.WriteLine(peak);

                m_val = eye_size / 2 * eye_result;

                int index = (int)(i + (width * x) + dock.size + 5 - (eye_size / 2));

                if (index < width && index >= 0)
                {
                    big_array[index] = m_val;
                }
            }
            double[] single_array = new double[combined.Count];

            double max_s = 0;
            int max_i = 0;
            if (!dock.panelIconsAnimating)
            {
                for (int i = 0; i < combined.Count; i++)
                {
                    int m = (int)(width / combined.Count) * (i + 1);
                    if (m >= big_array.Length)
                    {
                        m = big_array.Length - 1;
                    }

                    if (m < 0)
                    {
                        m = 0;
                    }

                    single_array[i] = big_array[m];

                    DockIcon image = combined[i] as DockIcon;
                    double newsize = dock.size * (big_array[m] / eye_size * 0.3 + 1);

                    if(newsize > max_s)
                    {
                        max_i = i;
                        max_s = newsize;
                    }

                    if (!dock.panelIconsAnimated)
                    {
                        DoubleAnimation doubleAnimation = new DoubleAnimation
                        {
                            From = image.Size,
                            To = newsize,
                            Duration = TimeSpan.FromMilliseconds(100),
                            EasingFunction = new SineEase(),
                            FillBehavior = FillBehavior.Stop
                        };
                        doubleAnimation.Completed += (a, e) =>
                        {
                            dock.panelIconsAnimated = true;
                            dock.panelIconsAnimating = false;
                        };
                        image.BeginAnimation(DockIcon.SizeProperty, doubleAnimation);
                        dock.panelIconsAnimating = true;
                    }
                    else
                    {

                        if (image.Size != newsize)
                        {
                            image.Size = newsize;
                        }
                    }
                }

                if(max_i != dock.fe_max_size)
                {
                    if (max_i < dock.MainPanel.Children.Count)
                    {
                        dock.context_icon = dock.MainPanel.Children[max_i];
                    }
                    dock.fe_max_size_el = max_i;
                }

            }
        }

        public static List<DockIcon> GetCombined(UIElementCollection uI1, UIElementCollection uL2)
        {
            List<DockIcon> combined = new List<DockIcon>();
            foreach (DockIcon di in uI1)
            {
                combined.Add(di);
            }
            foreach (DockIcon di in uL2)
            {
                combined.Add(di);
            }
            return combined;
        }
    }
}
