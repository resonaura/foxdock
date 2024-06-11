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
        public static void UpdateDockIcons(DockWindow dock, int index = -1)
        {
            /*
            if(dock.MainPanel.Children.Count > 0)
            {
                int i = 0;
                foreach(DockIcon icon in dock.MainPanel.Children)
                {
                    if(index == -1 || (index != -1 && i == index))
                    {
                        if (i < DockWindow.cache.dock_apps_path.Count)
                        {
                            icon.Source.Freeze();
                            Dictionary<bool, BitmapSource> d = SourceFromPath(DockWindow.cache.dock_apps_path[i], dock.iPack);
                            icon.Source = d.Values.First();
                            bool maskneeded = d.Keys.First();
                            if(maskneeded)
                            {
                                icon.MaskCornerRadius = dock.iPack.MaskCornerRadius;
                                icon.MaskBackground = dock.iPack.MaskBackground;
                                icon.MaskPadding = dock.iPack.MaskPadding * dock.size / DockWindow.defsize;
                                icon.MaskMargin = dock.iPack.MaskMargin * dock.size / DockWindow.defsize;
                            } else
                            {
                                icon.MaskCornerRadius = 0;
                                icon.MaskBackground = new SolidColorBrush();
                                icon.MaskPadding = 0;
                                icon.MaskMargin = 0;
                            }
                        }
                    }
                    
                    i++;
                }
            }
            */
        }
        /// <summary>
        /// Функция получения значка по пути
        /// </summary>
        /// <param name="path">Путь к файлу/папке</param>
        /// <returns></returns>
        public static Dictionary<bool, BitmapSource> SourceFromPath(string path, IconPack iPack)
        {
            Dictionary<bool, BitmapSource> result = new Dictionary<bool, BitmapSource>();
            //Тут всё почти так же, как и в предыдущей функции. Мне лень описывать)
            try
            {
                if(Directory.Exists(path))
                {
                    BitmapSource source = IconPacks.GetIconFromPath(iPack.Folder);

                    string myDocuments = API.Shell32.GetSFPath(Environment.SpecialFolder.MyDocuments);
                    string commonDocuments = API.Shell32.GetSFPath(Environment.SpecialFolder.CommonDocuments);
                    string myMusic = API.Shell32.GetSFPath(Environment.SpecialFolder.MyMusic);
                    string commonMusic = API.Shell32.GetSFPath(Environment.SpecialFolder.CommonMusic);
                    string myPictures = API.Shell32.GetSFPath(Environment.SpecialFolder.MyPictures);
                    string commonPictures = API.Shell32.GetSFPath(Environment.SpecialFolder.CommonPictures);
                    string myVideos = API.Shell32.GetSFPath(Environment.SpecialFolder.MyVideos);
                    string commonVideos = API.Shell32.GetSFPath(Environment.SpecialFolder.CommonVideos);

                    if (path == myDocuments || path == commonDocuments) source = IconPacks.GetIconFromPath(iPack.Documents);
                    if (path == myMusic || path == commonMusic) source = IconPacks.GetIconFromPath(iPack.Music);
                    if (path == myPictures || path == commonPictures) source = IconPacks.GetIconFromPath(iPack.Images);
                    if (path == myVideos || path == commonVideos) source = IconPacks.GetIconFromPath(iPack.Videos);

                    result.Add(false, source);
                    return result;
                }
                if(File.Exists(path))
                {
                    string extension = Path.GetExtension(path).ToLower().Replace(".", string.Empty);
                    if (iPack.ext.ContainsKey(extension))
                    {
                        if (iPack.ext[extension] != null)
                        {
                            result.Add(false, IconPacks.GetIconFromPath(iPack.ext[extension]));
                            return result;
                        }
                    }
                    string mimeType = MimeMapping.GetMimeMapping(path);
                    if(mimeType != "")
                    {
                        string type = mimeType.Split('/')[0];
                        switch(type)
                        {
                            case "image":
                                result.Add(false, IconPacks.GetIconFromPath(iPack.FileImage));
                                return result;
                            case "audio":
                                result.Add(false, IconPacks.GetIconFromPath(iPack.FileMusic));
                                return result;
                            case "video":
                                result.Add(false, IconPacks.GetIconFromPath(iPack.FileVideo));
                                return result;
                            case "application":
                                
                                if (extension != "exe" && extension != "lnk")
                                {
                                    result.Add(false, IconPacks.GetIconFromPath(iPack.FileDocument));
                                    return result;
                                } else if(extension == "exe")
                                {
                                    string filename = Path.GetFileNameWithoutExtension(path).ToLower();

                                    if(iPack.apps.ContainsKey(filename))
                                    {
                                        if(iPack.apps[filename] != null)
                                        {
                                            result.Add(false, IconPacks.GetIconFromPath(iPack.apps[filename]));
                                            return result;
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
                    result.Add(true, null);
                    return result;
                }

                int i = psfi.iIcon;

                result.Add(true, GetSourceFromIcon(GetShellIcon(i)));
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            result.Add(true, null);
            return result;
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
            using Bitmap bitmap = new Bitmap(path);
            BitmapSource source = GetSourceFromBitmap(Optimize(bitmap));
            return source;
        }
        private static double FishEyeCalc(int i, double k, int size)
        {
            double rX = -k + 1 + i;
            return rX >= -size && rX <= size ? ((-Math.Abs(rX) + size / 2) / size / 2 + 0.25) * 2 : 0;
        }
        /// <summary>
        /// Локика рыбьего глаза для значков
        /// </summary>
        /*/// <param name="x">Смещение</param>
        public static void FishEye(double x, DockWindow dock, List<DockIcon> combined)
        {
            int width = (int)(dock.Width);
            int eye_size = 50;

            double max_s = 0;
            int max_i = 0;

            if (!dock.panelIconsAnimating)
            {
                for (int i = 0; i < combined.Count; i++)
                {
                    if (combined[i] is DockIcon ic)
                    {
                        int ic_start_x = (int)(width / combined.Count + ic.Margin.Left + ic.Margin.Right) * i;
                        ic_start_x += (int)dock.Margin.Left;


                        double zoom = 0.3;
                        double newsize = dock.size * (1 + zoom * FishEyeCalc(ic_start_x, x - (dock.size * 1 + zoom) / 2, eye_size));

                        if (newsize > max_s)
                        {
                            max_i = i;
                            max_s = newsize;
                        }

                        if (!dock.panelIconsAnimated)
                        {
                            DoubleAnimation doubleAnimation = new DoubleAnimation
                            {
                                From = ic.Size,
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
                            ic.BeginAnimation(DockIcon.SizeProperty, doubleAnimation);
                            dock.panelIconsAnimating = true;
                        }
                        else
                        {

                            if (ic.Size != newsize)
                            {
                                ic.Size = newsize;
                            }
                        }
                    }
                }

                if (max_i != dock.fe_max_size)
                {
                    if (max_i < dock.MainPanel.Children.Count)
                    {
                        dock.context_icon = dock.MainPanel.Children[max_i];
                    }
                    dock.fe_max_size_el = max_i;
                }
            }
        }
        */
        public static List<DockIcon> GetIcons(StackPanel DockIcons)
        {
            List<DockIcon> combined = new List<DockIcon>();
            
            foreach(UIElement uIElement in DockIcons.Children)
            {
                if (uIElement is StackPanel sp)
                {
                    foreach (UIElement element in sp.Children)
                    {
                        if (element is DockIcon di)
                        {
                            combined.Add(di);
                        }
                    }
                }
            }

            return combined;
        }
        public static double CalcSeparatorOffset(DockIcon icon, StackPanel DockIcons)
        {
            double offset = 0;
            foreach (UIElement uIElement in DockIcons.Children)
            {
                if(uIElement is DockSeparator separator)
                {
                    offset = separator.ActualWidth + separator.Margin.Left + separator.Margin.Right;
                } else if (uIElement is StackPanel sp)
                {
                    foreach (UIElement element in sp.Children)
                    {
                        if (element == icon) return offset; 
                    }
                }
            }
            return offset;
        }
        public static double GetSeparatorsWidth(StackPanel DockIcons)
        {
            double offset = 0;
            foreach (UIElement uIElement in DockIcons.Children)
            {
                if (uIElement is DockSeparator separator)
                {
                    offset = separator.ActualWidth + separator.Margin.Left + separator.Margin.Right + 5; 
                }
            }
            return offset;
        }
        public static double GetDockIconsCount(StackPanel DockIcons)
        {
            double result = 0;
            foreach (UIElement uIElement in DockIcons.Children)
            {
                if (uIElement is StackPanel sp)
                {
                    foreach (UIElement element in sp.Children)
                    {
                        if (element is DockIcon di)
                        {
                            result++;
                        }
                    }
                }
            }
            return result;
        }
    }
}
