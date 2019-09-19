using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
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
            Bitmap temp = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
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

        /// <summary>
        /// Функция получения значка по пути
        /// </summary>
        /// <param name="path">Путь к файлу/папке</param>
        /// <returns></returns>
        public static BitmapSource SourceFromPath(string path)
        {
            //Тут всё почти так же, как и в предыдущей функции. Мне лень описывать)
            try
            {
                if(Directory.Exists(path))
                {
                    Bitmap source = Properties.Resources.folder;

                    string myDocuments = API.Shell32.GetSFPath(Environment.SpecialFolder.MyDocuments);
                    string commonDocuments = API.Shell32.GetSFPath(Environment.SpecialFolder.CommonDocuments);
                    string myMusic = API.Shell32.GetSFPath(Environment.SpecialFolder.MyMusic);
                    string commonMusic = API.Shell32.GetSFPath(Environment.SpecialFolder.CommonMusic);
                    string myPictures = API.Shell32.GetSFPath(Environment.SpecialFolder.MyPictures);
                    string commonPictures = API.Shell32.GetSFPath(Environment.SpecialFolder.CommonPictures);
                    string myVideos = API.Shell32.GetSFPath(Environment.SpecialFolder.MyVideos);
                    string commonVideos = API.Shell32.GetSFPath(Environment.SpecialFolder.CommonVideos);

                    if (path == myDocuments || path == commonDocuments) source = Properties.Resources.documents;
                    if (path == myMusic || path == commonMusic) source = Properties.Resources.music;
                    if (path == myPictures || path == commonPictures) source = Properties.Resources.images;
                    if (path == myVideos || path == commonVideos) source = Properties.Resources.videos;

                    return GetSourceFromBitmap(source);
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
                                return GetSourceFromBitmap(Properties.Resources.file_image);
                            case "audio":
                                return GetSourceFromBitmap(Properties.Resources.file_music);
                            case "video":
                                return GetSourceFromBitmap(Properties.Resources.file_video);
                            case "application":
                                string extension = Path.GetExtension(path).ToUpper();
                                if (extension != ".EXE" && extension != ".LNK")
                                {
                                    return GetSourceFromBitmap(Properties.Resources.file_document);
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
        /// <summary>
        /// Функция для расчёта формулы Рыбьего Глаза
        /// </summary>
        /// <param name="p">Процент</param>
        /// <returns>Результат</returns>
        public static float FishEye(float p)
        {
            return -(p * (p - 2));
        }
        /// <summary>
        /// Локика рыбьего глаза для значков
        /// </summary>
        /// <param name="x">Смещение</param>
        public static void FishEyeForIcons(float x, Dock dock)
        {
            //Комбинируем пользовательские значки и виджеты
            List<DockIcon> combined = new List<DockIcon>();
            foreach (DockIcon di in dock.MainPanel.Children)
            {
                combined.Add(di);
            }

            foreach (DockIcon di in dock.AIcons.Children)
            {
                combined.Add(di);
            }

            //Считаем ширину мнимой линии
            float width = (combined.Count) * (dock.size + (float)combined.First().Margin.Left + dock.size + (float)combined.First().Margin.Right);
            if (width < 300)
            {
                width = 300;
            }

            //Создаём большой массив с точкой мнимой линии
            float[] big_array = new float[(int)width];

            //Дальше немного эльфийской магии (или мне просто лень комментировать)
            int eye_size = 800;
            for (int i = 0; i < eye_size; i++)
            {
                float m_val = 0;
                int peak = 0;
                if (i < eye_size / 2)
                {
                    peak = i;
                }
                else
                {
                    peak = eye_size - i;
                }
                float percent = (float)peak / ((float)eye_size / 2);
                float eye_result = FishEye(percent);
                //..Debug.WriteLine(peak);

                m_val = eye_size / 2 * eye_result;

                int index = (int)(i + width * x + dock.size + 5 - eye_size / 2);

                if (index < width && index >= 0)
                {
                    big_array[index] = m_val;
                }
            }
            float[] single_array = new float[(int)combined.Count];

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

                    if (newsize >= dock.fe_max_size - 1)
                    {
                        dock.fe_max_size = newsize;
                        if (!dock.isHovered && dock.fe_max_size_el != i)
                        {
                            //Img_MouseEnterDo(combined[i]);
                            if (i < dock.MainPanel.Children.Count)
                            {
                                ContextMenuTools.SetContextIcon((DockIcon)dock.MainPanel.Children[i], dock);
                                dock.context_icon = dock.MainPanel.Children[i];
                            }
                            dock.fe_max_size_el = i;
                        }
                        else
                        {
                            if (!dock.isHovered)
                            {
                                //Img_MouseMoveDo(combined[fe_max_size_el]);
                                if (i < dock.MainPanel.Children.Count)
                                {
                                    dock.context_icon = dock.MainPanel.Children[dock.fe_max_size_el];
                                }
                            }
                        }
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
            }
        }
    }
}
