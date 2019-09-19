using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FoxDock
{
    /// <summary>
    /// Класс для реализации перемещения иконок между собой
    /// </summary>
    class IconsMove
    {
        /// <summary>
        /// Функция для проверки соприкосновения двух элементов
        /// </summary>
        /// <param name="el1">Элемент 1</param>
        /// <param name="el2">Элемент 2</param>
        /// <param name="e">Аргументы мыши</param>
        /// <returns></returns>
        public static bool HitTest(UIElement el1, UIElement el2, MouseEventArgs e)
        {
            //Получаем координаты мыши
            System.Windows.Point pt = e.GetPosition(el1);

            //Проверяем соприкосновение
            HitTestResult result = VisualTreeHelper.HitTest(el2, pt);

            if (result != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Логика перемещения значков в доке
        /// </summary>
        /// <param name="item_index">Исходный элемент (индекс)</param>
        /// <param name="ditem_index">Конечный элемент (индекс)</param>
        /// <param name="images">Все значки</param>
        /// <returns>Новые значки</returns>
        public static List<DockIcon> MoveImg(int item_index, int ditem_index, List<DockIcon> images)
        {
            //Создаём два массива слева и справа значка
            List<DockIcon> left = new List<DockIcon>();
            List<DockIcon> right = new List<DockIcon>();

            //Создаём переменную для счётчика в foreach
            int i = 0;

            //Добавляем элемент с перемещаемым значков в массив правого
            right.Add(images[item_index]);

            //Удаляем перемещаемый значок из всех элеменов
            images.Remove(images[item_index]);

            //Прохидимся по всем значкам
            foreach (DockIcon img in images)
            {
                //Если текущий индекс больше или равен индексу замещаемого значка
                if (i >= ditem_index)
                {
                    //Добавляем в правый массив
                    right.Add(img);
                }
                else //Иначе
                {
                    //Добавляем в левый массив
                    left.Add(img);
                }
                i++; //+1 к индексу
            }

            //Очищаем все значки массива
            images.Clear();

            //Добавляем левые и правые значки в массив
            images.AddRange(left);
            images.AddRange(right);

            return images; //Вовращаем конечный массив
        }

        /// <summary>
        /// Логика перемещения строки в массиве (работает так же как перемещение значков)
        /// </summary>
        /// <param name="item_index">Исходный элемент (индекс)</param>
        /// <param name="ditem_index">Конечный элемент (индекс)</param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static List<string> MoveString(int item_index, int ditem_index, List<string> elements)
        {
            //Не вижу смысла комментировать, так как всё это описано в другой функции
            List<string> left = new List<string>();
            List<string> right = new List<string>();

            int i = 0;
            right.Add(elements[item_index]);
            elements.RemoveAt(item_index);

            foreach (string el in elements)
            {
                if (i >= ditem_index)
                {
                    right.Add(el);
                }
                else
                {
                    left.Add(el);
                }
                i++;
            }

            elements = new List<string>();
            elements.AddRange(left);
            elements.AddRange(right);

            return elements;
        }
    }
}
