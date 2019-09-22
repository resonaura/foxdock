using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace FoxDock
{
    /// <summary>
    /// Класс для работы с кешем
    /// </summary>
    public static class CacheOperations
    {
        /// <summary>
        /// Функция для загрузки кеша
        /// </summary>
        /// <param name="cache">Кеш</param>
        /// <returns></returns>
        public static Cache LoadCache(Cache cache)
        {
            //Получаем путь к кешу
            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string cacheFile = appPath + "//Cache//UserSets.xml";

            //Проверяем наличие файла
            bool exists = System.IO.File.Exists(cacheFile);

            //Если существует
            if (exists)
            {
                //Вызываем сериалайзер
                XmlSerializer serializer = new XmlSerializer(typeof(Cache));

                //Пробуем прочитать файл кеша
                try
                {
                    string text;
                    using (var streamReader = new StreamReader(cacheFile, Encoding.UTF8))
                    {
                        //Получаем текст файла
                        text = streamReader.ReadToEnd();
                    }

                    using (Stream reader = new FileStream(cacheFile, FileMode.Open))
                    {
                        //Если есть кодовое слово в начале
                        if (reader != null && text.Contains("<Cache xmlns"))
                        {
                            //Десириализируем кеш
                            cache = (Cache)serializer.Deserialize(reader);
                        }
                    }
                }
                catch
                {
                    Debug.Write("Cache read error.");
                }
                return cache; //Вовзращаем кеш
            }
            else
            {
                return cache;
            }
        }
        /// <summary>
        /// Функция для сохранения кеша
        /// </summary>
        /// <param name="cache">Кеш</param>
        public static void StoreCache(Cache cache)
        {
            //Инициализируем сериалайзер
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(Cache));

            //Получаем путь к папке кеша
            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string subPath = appPath + "//Cache";

            //Проверяем наличие папки
            bool exists = System.IO.Directory.Exists(subPath);

            //Если не сущесвует
            if (!exists)
            {
                System.IO.Directory.CreateDirectory(subPath); //Создаём папку для кеша
            }

            //Пробуем записать кеш
            try
            {
                FileStream file = File.Create(subPath + "//UserSets.xml"); //Открываем файлстрим
                writer.Serialize(file, cache); //Сериализируем и пишем в файл
                file.Close(); //Закрываем файлстрим
            }
            catch
            {
                Debug.Write("Cache write error.");
            }
        }
    }
}
