using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxDock
{
    public class AppLanguage
    {
        //Основные перечисления для удобства

        /// <summary>
        /// Язык
        /// </summary>
        public enum Locale
        {
            EN,
            RU,
            UA
        }
        /// <summary>
        /// Диалоги
        /// </summary>
        public enum Dialog
        {
            ExitDock,
            RestartDock,
            DockSettings,
            DockSettingsShort,
            LockDock,
            UnlockDock,
            RemoveFromDock,
            OpenNew,
            CloseSomeApp,
            CloseAll,
            Explorer,
            RecycleBin,
            OpenRecycleBin,
            CloseRecycleBin,
            ClearRecycleBin,
            ConfAppClose,
            ConfRemove,
            ConfRename,
            ConfNo,
            ConfYes,
            ConfCancel,
            ConfSave,
            SettingsHomeTab,
            SettingsPerfomanceTab,
            SettingsCustomizeTab,
            DockSettingsStartupLabel,
            DockSettingsDisableBlurLabel,
            DockSettingsEnableStarDustLabel,
            DockSettingsPanelScaleLabel,
            DockSettingsBackgroundOpacityLabel,
            DockSettingsHintOpacityLabel,
            DockSettingsDisplayDockOnTopLabel,
            DockSettingsAutoHideLabel,
            DockSettingsBGMColorLabel,
            DockSettingsBGHColorLabel,
            DockSettingsBGIColorLabel,
            DockSettingsBGNColorLabel,
            DockSettingsSmartDisableLabel,
            Tooltip,
            RecentFiles,
            OpenInExplorer,
            RenameIcon
        }

        //Основные функции

        /// <summary>
        /// Функция получения языка системы
        /// </summary>
        /// <returns>Язык</returns>
        public static Locale GetSystemLocale()
        {
            CultureInfo ci = CultureInfo.InstalledUICulture;

            string lang_full = ci.TwoLetterISOLanguageName;
            switch (lang_full)
            {
                case "en":
                    return Locale.EN;
                case "ru":
                    return Locale.RU;
                case "uk":
                    return Locale.UA;
            }
            return Locale.EN;
        }
        /// <summary>
        /// Функция для получения определённого диалога в зависимости от языка
        /// </summary>
        /// <param name="di">Диалог</param>
        /// <param name="locale">Язык</param>
        /// <returns></returns>
        public static string GetDialogByLocale(Dialog di, Locale locale)
        {
            return DICT[locale][di];
        }

        //Основные словари
        private readonly static Dictionary<Dialog, string> EN_DICT = new Dictionary<Dialog, string>()
        {
            [Dialog.ExitDock] = "Exit",
            [Dialog.RestartDock] = "Restart Dock",
            [Dialog.DockSettings] = "Dock settings",
            [Dialog.DockSettingsShort] = "Settings",
            [Dialog.LockDock] = "Lock Dock",
            [Dialog.UnlockDock] = "Unlock Dock",
            [Dialog.RemoveFromDock] = "Remove from Dock",
            [Dialog.OpenNew] = "Open new",
            [Dialog.CloseSomeApp] = "Close",
            [Dialog.CloseAll] = "Close all",
            [Dialog.Explorer] = "Explorer",
            [Dialog.RecycleBin] = "Recycle Bin",
            [Dialog.OpenRecycleBin] = "Open Recycle Bin",
            [Dialog.CloseRecycleBin] = "Close Recycle Bin",
            [Dialog.ClearRecycleBin] = "Clear Recycle Bin",
            [Dialog.ConfAppClose] = "Are you sure you want to close ",
            [Dialog.ConfRemove] = "Are you sure you want to remove this item from Dock?",
            [Dialog.ConfRename] = "Rename icon:",
            [Dialog.ConfNo] = "No",
            [Dialog.ConfYes] = "Yes",
            [Dialog.ConfCancel] = "Cancel",
            [Dialog.ConfSave] = "Save",
            [Dialog.SettingsHomeTab] = "Home",
            [Dialog.SettingsPerfomanceTab] = "Perfomance",
            [Dialog.SettingsCustomizeTab] = "Customize",
            [Dialog.DockSettingsStartupLabel] = "Run FoxDock at Windows startup",
            [Dialog.DockSettingsDisableBlurLabel] = "Disable blur",
            [Dialog.DockSettingsEnableStarDustLabel] = "Enable StarDust",
            [Dialog.DockSettingsPanelScaleLabel] = "Panel scale:",
            [Dialog.DockSettingsBackgroundOpacityLabel] = "Dock background opacity:",
            [Dialog.DockSettingsHintOpacityLabel] = "Hint background opacity:",
            [Dialog.DockSettingsDisplayDockOnTopLabel] = "Display Dock on top:",
            [Dialog.DockSettingsAutoHideLabel] = "Auto hide Dock if it's on top:",
            [Dialog.DockSettingsBGMColorLabel] = "Background color:",
            [Dialog.DockSettingsBGHColorLabel] = "Hint color:",
            [Dialog.DockSettingsBGIColorLabel] = "Indicator color:",
            [Dialog.DockSettingsBGNColorLabel] = "Menu color:",
            [Dialog.DockSettingsSmartDisableLabel] = "Smart disable background tasks:",
            [Dialog.Tooltip] = "Tooltip",
            [Dialog.RecentFiles] = "Recent items",
            [Dialog.OpenInExplorer] = "Open in Explorer",
            [Dialog.RenameIcon] = "Rename icon"
        };
        private static readonly Dictionary<Dialog, string> RU_DICT = new Dictionary<Dialog, string>()
        {
            [Dialog.ExitDock] = "Выйти",
            [Dialog.RestartDock] = "Перезапустить Док-бар",
            [Dialog.DockSettings] = "Настройки Док-бара",
            [Dialog.DockSettingsShort] = "Настройки",
            [Dialog.LockDock] = "Заблокировать значки",
            [Dialog.UnlockDock] = "Разблокировать значки",
            [Dialog.RemoveFromDock] = "Убрать из Док-бара",
            [Dialog.OpenNew] = "Открыть новое окно",
            [Dialog.CloseSomeApp] = "Закрыть",
            [Dialog.CloseAll] = "Закрыть все окна",
            [Dialog.Explorer] = "Проводник",
            [Dialog.RecycleBin] = "Корзина",
            [Dialog.OpenRecycleBin] = "Открыть Корзину",
            [Dialog.CloseRecycleBin] = "Закрыть Корзину",
            [Dialog.ClearRecycleBin] = "Очистить Корзину",
            [Dialog.ConfAppClose] = "Вы действительно хотите закрыть ",
            [Dialog.ConfRemove] = "Вы уверены, что хотите удалить этот элемент из Док-бара?",
            [Dialog.ConfRename] = "Переименовать значок:",
            [Dialog.ConfNo] = "Нет",
            [Dialog.ConfYes] = "Да",
            [Dialog.ConfCancel] = "Отменить",
            [Dialog.ConfSave] = "Сохранить",
            [Dialog.SettingsHomeTab] = "Главная",
            [Dialog.SettingsPerfomanceTab] = "Производительность",
            [Dialog.SettingsCustomizeTab] = "Персонализация",
            [Dialog.DockSettingsStartupLabel] = "Запускать FoxDock при запуске Windows",
            [Dialog.DockSettingsDisableBlurLabel] = "Отключить размытие",
            [Dialog.DockSettingsEnableStarDustLabel] = "Включить StarDust",
            [Dialog.DockSettingsPanelScaleLabel] = "Размер значков:",
            [Dialog.DockSettingsBackgroundOpacityLabel] = "Непрозрачность фона док-бара:",
            [Dialog.DockSettingsHintOpacityLabel] = "Непрозрачность фона подсказки:",
            [Dialog.DockSettingsDisplayDockOnTopLabel] = "Отображать Док-бар поверх всех окон:",
            [Dialog.DockSettingsAutoHideLabel] = "Автоматически скрывать Док-бар если он поверх всех окон:",
            [Dialog.DockSettingsBGMColorLabel] = "Цвет фона:",
            [Dialog.DockSettingsBGHColorLabel] = "Цвет подсказки:",
            [Dialog.DockSettingsBGIColorLabel] = "Цвет индикатора:",
            [Dialog.DockSettingsBGNColorLabel] = "Цвет меню:",
            [Dialog.DockSettingsSmartDisableLabel] = "Умное отключение фоновых задач:",
            [Dialog.Tooltip] = "Подсказка",
            [Dialog.RecentFiles] = "Недавние документы",
            [Dialog.OpenInExplorer] = "Открыть в Проводнике",
            [Dialog.RenameIcon] = "Переименовать значок"
        };
        private static readonly Dictionary<Dialog, string> UA_DICT = new Dictionary<Dialog, string>()
        {
            [Dialog.ExitDock] = "Вийти",
            [Dialog.RestartDock] = "Перезавантажити Док-бар",
            [Dialog.DockSettings] = "Налаштування Док-бару",
            [Dialog.DockSettingsShort] = "Налаштування",
            [Dialog.LockDock] = "Заблокувати значки",
            [Dialog.UnlockDock] = "Розблокувати значки",
            [Dialog.RemoveFromDock] = "Видалити з Док-бару",
            [Dialog.OpenNew] = "Відкрити нове вікно",
            [Dialog.CloseSomeApp] = "Закрити",
            [Dialog.CloseAll] = "Закрити всі вікна",
            [Dialog.Explorer] = "Провідник",
            [Dialog.RecycleBin] = "Кошик",
            [Dialog.OpenRecycleBin] = "Відкрити Кошик",
            [Dialog.CloseRecycleBin] = "Закрити Кошик",
            [Dialog.ClearRecycleBin] = "Очистити Кошик",
            [Dialog.ConfAppClose] = "Ви дійсно хочете закрити ",
            [Dialog.ConfRemove] = "Ви впевнені, що хочете видалити цей елемент з Док-бару?",
            [Dialog.ConfRename] = "Перейменувати значок:",
            [Dialog.ConfNo] = "Ні",
            [Dialog.ConfYes] = "Так",
            [Dialog.ConfCancel] = "Відмінити",
            [Dialog.ConfSave] = "Зберегти",
            [Dialog.SettingsHomeTab] = "Головна",
            [Dialog.SettingsPerfomanceTab] = "Продуктивність",
            [Dialog.SettingsCustomizeTab] = "Персоналізація",
            [Dialog.DockSettingsStartupLabel] = "Запускати FoxDock під час запуску Windows",
            [Dialog.DockSettingsDisableBlurLabel] = "Вимкнути розмиття",
            [Dialog.DockSettingsEnableStarDustLabel] = "Ввімкнути StarDust",
            [Dialog.DockSettingsPanelScaleLabel] = "Розмір значків:",
            [Dialog.DockSettingsBackgroundOpacityLabel] = "Непрозорість фону док-бару:",
            [Dialog.DockSettingsHintOpacityLabel] = "Непрозорість фону підсказки:",
            [Dialog.DockSettingsDisplayDockOnTopLabel] = "Відображати Док-бар поверх всіх вікон:",
            [Dialog.DockSettingsAutoHideLabel] = "Автоматично приховувати Док-бар якщо він поверх всіх вікон:",
            [Dialog.DockSettingsBGMColorLabel] = "Колір фону:",
            [Dialog.DockSettingsBGHColorLabel] = "Колір підсказки:",
            [Dialog.DockSettingsBGIColorLabel] = "Колір індикатору:",
            [Dialog.DockSettingsBGNColorLabel] = "Колір меню:",
            [Dialog.DockSettingsSmartDisableLabel] = "Розумне відключення фонових завдань:",
            [Dialog.Tooltip] = "Підсказка",
            [Dialog.RecentFiles] = "Недавні документи",
            [Dialog.OpenInExplorer] = "Відкрити у Провіднику",
            [Dialog.RenameIcon] = "Перейменувати значок"
        };

        //Комбинированный словарь
        private static readonly Dictionary<Locale, Dictionary<Dialog, string>> DICT = new Dictionary<Locale, Dictionary<Dialog, string>> {
            [Locale.EN] = EN_DICT,
            [Locale.RU] = RU_DICT,
            [Locale.UA] = UA_DICT
        };
        
    }
}
