using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Color = System.Windows.Media.Color;

namespace FoxDock
{
    /// <summary>
    /// Класс для упрощения анимаций
    /// </summary>
    class Animations
    {
        /// <summary>
        /// Функция для остановки анимации прозрачности у элемента
        /// </summary>
        /// <param name="element"></param>
        public static void Break(UIElement element)
        {
            DoubleAnimation myAnimation = new DoubleAnimation();
            // Initialize animation
            

            // To start
            element.BeginAnimation(Window.OpacityProperty, myAnimation);

            // To stop and keep the current value of the animated property
            myAnimation.BeginTime = null;
            element.BeginAnimation(Window.OpacityProperty, myAnimation);
        }
        /// <summary>
        /// Изменение цветов интерфейса в зависимости от настроек Дока
        /// </summary>
        /// <param name="theme">Тема</param>
        /// <param name="dock">Док</param>
        public static void ThemeAnimate(string theme, Dock dock)
        {
            //Переменные для значений прозрачности разных оверлеев
            double black_opacity = 0;
            double white_opacity = 0;
            double accent_opacity = 0;

            //Получаем прозрачность подсказки
            int hint_opacity = (int)(Dock.cache.hm_trans * 255);

            //В зависимости от кеша задаём прозрачность оверлеев
            switch (Dock.cache.background)
            {
                case DockBackground.Auto: //Если фон Дока должен задаваться в зависимости от темы
                    if (theme == "0") //Если тёмная тема
                    {
                        black_opacity = 1; //Делаем чёрный оверлей видимым
                    }
                    if (theme == "1") //Если светлая тема
                    {
                        white_opacity = 1; //Делаем белый оверлей видимым
                    }
                    break;
                case DockBackground.Black: //Если фон дока должен быть чёрным
                    black_opacity = 1; //Делаем чёрный оверлей видимым
                    break;
                case DockBackground.White: //Если фон дока должен быть белым
                    white_opacity = 1; //Делаем белый оверлей видимым
                    break;
                case DockBackground.Gray: //Если фон дока должен быть серым
                    black_opacity = 0.7; //Задаём прозрачность чёрного фона как 70%
                    white_opacity = 0.3; //Задаём прозрачность белого фона как 30%
                    break;
                case DockBackground.Accent: //Если фон дока должен быть акцентом
                    accent_opacity = 1; //Делаем акцентный оверлей видимым
                    break;
            }

            //В зависимости от кеша задаём фон и цвет текста подсказки
            switch(Dock.cache.hintBackground)
            {
                case HintBackground.Auto: //Если оформление подсказки должно задаваться в зависимости от темы
                    if (theme == "0") //Если тёмная тема
                    {
                        dock.tooltip.app_hint.Background = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 24, 24, 24));
                        dock.tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    }
                    if (theme == "1") //Если светлая тема
                    {
                        dock.tooltip.app_hint.Background = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 255, 255, 255));
                        dock.tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                    }
                    break;
                case HintBackground.Black: //Если оформление подсказки должно быть чёрным
                    dock.tooltip.app_hint.Background = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 24, 24, 24));
                    dock.tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    break;
                case HintBackground.Gray: //Если оформление подсказки должно быть серым
                    dock.tooltip.app_hint.Background = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 48, 48, 48));
                    dock.tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    break;
                case HintBackground.White: //Если оформление подсказки должно быть белым
                    dock.tooltip.app_hint.Background = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, 255, 255, 255));
                    dock.tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                    break;
                case HintBackground.Accent://Если оформление подсказки должно зависеть от акцентного цвета системы
                    SolidColorBrush accent_brush = new SolidColorBrush(Color.FromArgb((byte)hint_opacity, SystemParameters.WindowGlassColor.R, SystemParameters.WindowGlassColor.G, SystemParameters.WindowGlassColor.B));
                    dock.tooltip.app_hint.Background = accent_brush;
                    dock.tooltip.app_hint.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    break;

            }
            string hcolor = "White"; //По умолчанию цвет индикатора задаём как белый

            //В зависимости от кеша задаём цвет индикатора
            switch (Dock.cache.IndicatorColor)
            {
                case IndicatorColor.Auto: //Если цвет индикатора должен зависеть от темы
                    if (theme == "0") //Если тёмная тема
                    {
                        hcolor = "White"; //Задаём цвет индикатора как белый
                    }
                    if (theme == "1") //Если светлая тема
                    {
                        hcolor = "Black"; //Задаём цвет индикатора как чёрный
                    }
                break;
                case IndicatorColor.Black: //Если цвет индикатора должен быть чёрным
                    hcolor = "Black"; //Задаём цвет индикатора как чёрный
                    break;
                case IndicatorColor.Gray: //Если цвет индикатора должен быть серый
                    hcolor = "Gray"; //Задаём цвет индикатора как серый
                    break;
                case IndicatorColor.White: //Если цвет индикатора должен быть белым
                    hcolor = "White"; //Задаём цвет индикатора как белый
                    break;
                case IndicatorColor.Accent: //Если цвет индикатора должен зависеть от цвета темы системы
                    hcolor = "Accent"; //Задаём цвет индикатора как акцентный
                    break;
            }

            //Проходимся по всем основным иконкам и виджетам
            foreach (DockIcon ic in Dock.GetCombined(dock.MainPanel.Children, dock.AIcons.Children))
            {
                ic.HighlightColor = hcolor; //Задаём цвет
            }

            //В зависимости от кеша задаём цвет меню
            switch (Dock.cache.MenuColor)
            {
                case MenuColor.Auto: //Если цвет меню должен зависеть от темы
                    if(theme == "0") //Если тёмная тема
                    {
                        //Задаём тёмную тему меню
                        dock.MainContextMenu.Background = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                        dock.MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                        foreach (UIElement item in dock.MainContextMenu.Items)
                        {
                            if (item is MenuItem mi)
                            {
                                mi.Template = (ControlTemplate)dock.Resources["DarkCoolMenuItem"];
                            }
                        }
                    }
                    if (theme == "1") //Если светлая тема
                    {
                        //Задаём светлую тему меню
                        dock.MainContextMenu.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                        dock.MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(24, 24, 24));

                        foreach (UIElement item in dock.MainContextMenu.Items)
                        {
                            if (item is MenuItem mi)
                            {
                                mi.Template = (ControlTemplate)dock.Resources["WhiteCoolMenuItem"];
                            }
                        }
                    }
                    break;
                case MenuColor.Black: //Если цвет меню должен быть чёрными
                    //Задаём чёрную тему меню
                    dock.MainContextMenu.Background = new SolidColorBrush(Color.FromRgb(24, 24, 24));
                    dock.MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                    foreach (UIElement item in dock.MainContextMenu.Items)
                    {
                        if (item is MenuItem mi)
                        {
                            mi.Template = (ControlTemplate)dock.Resources["DarkCoolMenuItem"];
                        }
                    }
                    break;
                case MenuColor.Gray: //Если цвет меню должен быть серым
                    //Задаём серую тему меню
                    dock.MainContextMenu.Background = new SolidColorBrush(Color.FromRgb(48, 48, 48));
                    dock.MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                    foreach (UIElement item in dock.MainContextMenu.Items)
                    {
                        if (item is MenuItem mi)
                        {
                            mi.Template = (ControlTemplate)dock.Resources["DarkCoolMenuItem"];
                        }
                    }
                    break;
                case MenuColor.White: //Если цвет меню должен быть белым
                    //Задаём белую тему меню
                    dock.MainContextMenu.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    dock.MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(24, 24, 24));

                    foreach (UIElement item in dock.MainContextMenu.Items)
                    {
                        if (item is MenuItem mi)
                        {
                            mi.Template = (ControlTemplate)dock.Resources["WhiteCoolMenuItem"];
                        }
                    }
                    break;
                case MenuColor.Accent: //Если цвет меню должен быть акцентным
                    //Задаём акцентную тему меню
                    dock.MainContextMenu.Background = SystemParameters.WindowGlassBrush;
                    dock.MainContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                    foreach (UIElement item in dock.MainContextMenu.Items)
                    {
                        if (item is MenuItem mi)
                        {
                            mi.Template = (ControlTemplate)dock.Resources["DarkCoolMenuItem"];
                        }
                    }
                    break;
            }

            //Выполняем анимацию прозрачности оверлеев
            DoubleAnimation op1 = Animations.SingleAnimation(dock.BlackOverlay.Opacity, black_opacity);
            dock.BlackOverlay.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, op1);

            DoubleAnimation op2 = Animations.SingleAnimation(dock.WhiteOverlay.Opacity, white_opacity);
            dock.WhiteOverlay.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, op2);

            DoubleAnimation op3 = Animations.SingleAnimation(dock.App_bg.Opacity, accent_opacity);
            dock.App_bg.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, op3);
        }
        /// <summary>
        /// Функция для создания простейшей анимации
        /// </summary>
        /// <param name="from">Начальное значение</param>
        /// <param name="to">Конечное значение</param>
        /// <param name="duration">Длительность анимации</param>
        /// <returns>Анимация</returns>
        public static DoubleAnimation SingleAnimation(double from, double to, double duration = 0.5)
        {
            DoubleAnimation anim = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(duration),
                EasingFunction = new CubicEase()
            };
            Timeline.SetDesiredFrameRate(anim, 60);

            return anim;
        }
    }
}
