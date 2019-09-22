using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FoxDock
{
    class ContextMenuTools
    {
        /// <summary>
        /// Функция генерации разделителя
        /// </summary>
        /// <returns></returns>
        public static Separator GenerateSeparator(Dock dock)
        {
            Separator separator = new Separator
            {
                Height = 2,
                Margin = new Thickness(5),
                Opacity = .2,
                Background = dock.MainContextMenu.Foreground
            };
            return separator;
        }

        /// <summary>
        /// Функция клонирования элемента контекстного меню
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static MenuItem CloneMenuItem(MenuItem source, Dock dock)
        {
            MenuItem result = new MenuItem();
            if (source != null)
            {
                TextBlock ti = new TextBlock
                {
                    Text = (source.Icon as TextBlock).Text,
                    FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"),
                    FontSize = 14,
                    Foreground = dock.CloseSomeAppButton.Foreground,
                    VerticalAlignment = dock.CloseSomeAppButton.VerticalAlignment
                };
                result = new MenuItem
                {
                    Style = source.Style,
                    CommandParameter = source.CommandParameter,
                    Template = dock.CloseSomeAppButton.Template,
                    Padding = source.Padding,
                    Background = source.Background,
                    Foreground = source.Foreground,
                    Icon = ti,
                    Header = source.Header
                };

                switch (source.Name)
                {
                    case "CloseSomeAppButton":
                        result.Click += dock.CloseSomeAppButton_Click;
                        break;
                    case "OpenNewButton":
                        result.Click += dock.OpenNewButton_Click;
                        break;
                    case "RemoveFromDockButton":
                        result.Click += dock.RemoveFromDockButton_Click;
                        break;
                    case "LockDockButton":
                        result.Click += dock.LockDockButton_Click;
                        break;
                    case "SettingsButton":
                        result.Click += dock.SettingsButton_Click;
                        break;
                    case "RestartButton":
                        result.Click += dock.RestartButton_Click;
                        break;
                    case "ExitButton":
                        result.Click += dock.CloseDock_Click;
                        break;
                }

            }

            return result;
        }

        /// <summary>
        /// Логика генерации контекстного меню
        /// </summary>
        /// <param name="items">Элементы</param>
        /// <returns></returns>
        public static ContextMenu GenerateContextMenu(List<object> items, Dock dock)
        {
            ContextMenu res = new ContextMenu
            {
                Margin = dock.MainContextMenu.Margin,
                Style = dock.MainContextMenu.Style,
                ItemTemplate = (DataTemplate)dock.Resources["MenuItemStyle"],
                Background = dock.MainContextMenu.Background,
                Effect = dock.MainContextMenu.Effect,
            };
            foreach (object item in items)
            {
                res.Items.Add(item);
            }

            return res;
        }
        /// <summary>
        /// Генерируем элемент меню
        /// </summary>
        /// <param name="icon">Значок</param>
        /// <param name="text">Заголовок</param>
        /// <param name="func">Результат</param>
        /// <returns></returns>
        public static MenuItem GenerateMenuItem(string icon, string text, Func<int> func, Dock dock)
        {
            TextBlock ti = new TextBlock
            {
                Text = icon,
                FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"),
                FontSize = 14,
                Foreground = dock.CloseSomeAppButton.Foreground,
                VerticalAlignment = dock.CloseSomeAppButton.VerticalAlignment
            };
            MenuItem item = new MenuItem
            {
                Style = dock.CloseSomeAppButton.Style,
                CommandParameter = dock.CloseSomeAppButton.CommandParameter,
                Template = dock.CloseSomeAppButton.Template,
                Padding = dock.CloseSomeAppButton.Padding,
                Background = dock.CloseSomeAppButton.Background,
                Foreground = dock.CloseSomeAppButton.Foreground,
                Icon = ti,
                Header = text
            };
            item.Click += (s, e) => { func(); };
            return item;
        }

        /// <summary>
        /// Функция задания активного значка для контекстного меню
        /// </summary>
        /// <param name="img">Значок</param>
        
        public static void SetContextIcon(DockIcon img, Dock dock)
        {
            //Отображаем пункт контекстного меню, отвечающий за удаление значка из Дока
            dock.RemoveFromDockButton.Opacity = 1;
            dock.RemoveFromDockButton.IsEnabled = true;

            //Получаем имя и путь текущего значка
            string current_name = Dock.cache.dock_apps[dock.MainPanel.Children.IndexOf(img)];
            string current_path = Dock.cache.dock_apps_path[dock.MainPanel.Children.IndexOf(img)];

            //Задаём кнопке закрытия проги в контекстном меню нужное имя
            dock.CloseSomeAppButton.Header = AppLanguage.GetDialogByLocale(AppLanguage.Dialog.CloseSomeApp, Dock.locale) + " " + current_name;

            //Проверяем запущено ли приложение
            bool apprunned = API.Win32.CheckIfAppRunned(current_path);

            //В зависимости от результата делаем активной/неактивной кнопку закрытия приложения
            if (apprunned)
            {
                dock.CloseSomeAppButton.IsEnabled = true;
            }
            else
            {
                dock.CloseSomeAppButton.IsEnabled = false;
            }

            //Задаём текущий значок как контекстный
            dock.context_icon = img;
        }
        /// <summary>
        /// Функция для получения стандартных элементов контекстного меню
        /// </summary>
        /// <param name="dock">Док</param>
        /// <returns></returns>
        public static List<object> GetDefaultItems(Dock dock)
        {
            List<object> items = new List<object>
            {
                GenerateSeparator(dock),
                CloneMenuItem(dock.LockDockButton, dock),
                GenerateSeparator(dock),
                CloneMenuItem(dock.SettingsButton, dock),
                CloneMenuItem(dock.RestartButton, dock),
                CloneMenuItem(dock.ExitButton, dock)
            };
            return items;
        }
    }
}
