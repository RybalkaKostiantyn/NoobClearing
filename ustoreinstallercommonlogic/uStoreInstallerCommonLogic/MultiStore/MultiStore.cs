using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace uStoreInstallerCommonLogic
{
    public class MultiStore
    {
        [JsonProperty("pathToUStore")]
        public string PathToUStore { get; set; }

        [JsonProperty("server")]
        public string Server { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("pwd")]
        public string Pwd { get; set; }

        /// <summary>
        /// Loads MultiStore settings from the first json file from the current directory
        /// </summary>
        /// <returns></returns>
        public static List<MultiStore> LoadFromJson()
        {
            var di = new DirectoryInfo(Directory.GetCurrentDirectory());
            var jsConfFile = di.GetFiles("*.json").FirstOrDefault();

            if (jsConfFile == null)
                throw new MultiStoreException("Could not find a json config file by path: " + di.FullName);

            var json = File.ReadAllText(jsConfFile.FullName);
            var stores = JsonConvert.DeserializeObject<List<MultiStore>>(json.Replace(@"\", @"\\"));
            stores.ForEach(_ =>
            {
                if (!Directory.Exists(_.PathToUStore))
                    throw new MultiStoreException("Could not find a part of the path '" + _.PathToUStore + "'.");
            });

            return stores;
        }
    }
}