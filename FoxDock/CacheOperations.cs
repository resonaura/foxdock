using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FoxDock
{
    public static class CacheOperations
    {
        public static Cache LoadCache(Cache cache)
        {
            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string cacheFile = appPath + "//Cache//UserSets.xml";

            bool exists = System.IO.File.Exists(cacheFile);

            if (exists)
            {

                XmlSerializer serializer = new XmlSerializer(typeof(Cache));

                try
                {
                    string text;
                    using (var streamReader = new StreamReader(cacheFile, Encoding.UTF8))
                    {
                        text = streamReader.ReadToEnd();
                    }

                    using (Stream reader = new FileStream(cacheFile, FileMode.Open))
                    {
                        if (reader != null && text.Contains("<Cache xmlns"))
                            cache = (Cache)serializer.Deserialize(reader);
                    }
                }
                catch
                {
                    Debug.Write("Cache read error.");
                }
                return cache;
            }
            else
            {
                return cache;
            }
        }
        public static void StoreCache(Cache cache)
        {
            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(typeof(Cache));

            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);


            string subPath = appPath + "//Cache";

            bool exists = System.IO.Directory.Exists(subPath);

            if (!exists)
                System.IO.Directory.CreateDirectory(subPath);

            try
            {
                System.IO.FileStream file = System.IO.File.Create(subPath + "//UserSets.xml");
                writer.Serialize(file, cache);
                file.Close();
            }
            catch
            {
                Debug.Write("Cache write error.");
            }
        }
    }
}
