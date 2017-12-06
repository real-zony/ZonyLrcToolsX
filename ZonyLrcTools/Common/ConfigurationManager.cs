using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using ZonyLrcTools.Common.Interfaces;

namespace ZonyLrcTools.Common
{
    public sealed class ConfigurationManager : IConfigurationManager
    {
        public ConfigurationModel ConfigModel { get; set; }
        /// <summary>
        /// 默认配置文件路径
        /// </summary>
        public readonly string ConfigurationFileName = $@"{Environment.CurrentDirectory}\config.json";

        public void LoadConfiguration()
        {
            LoadConfiguration(ConfigurationFileName);
        }

        public void LoadConfiguration(string filePath)
        {
            if (!File.Exists(filePath))
            {
                ConfigModel = InitializeDefaultConfigurationModel();
                return;
            }

            FileStream _file = File.Open(filePath, FileMode.Open);
            using (StreamReader _reader = new StreamReader(_file))
            {
                ConfigModel = JsonConvert.DeserializeObject<ConfigurationModel>(_reader.ReadToEnd());
            }
        }

        public void SaveConfiguration()
        {
            SaveConfiguration(ConfigurationFileName);
        }

        public void SaveConfiguration(string filePath)
        {
            using (FileStream _file = File.Open(filePath, FileMode.OpenOrCreate))
            {
                _file.SetLength(0);

                StreamWriter _sr = new StreamWriter(_file);
                _sr.Write(JsonConvert.SerializeObject(ConfigModel));
            }
        }

        private ConfigurationModel InitializeDefaultConfigurationModel()
        {
            return new ConfigurationModel()
            {
                EncodingName = "utf-8",
                DownloadThreadNumber = 4,
                IsIgnoreExitsFile = true,
                IsCheckUpdate = true,
                IsAgree = false,
                ExtensionsName = new List<string>() { "*.mp3", "*.ape", "*.flac", "*.m4a" }
            };
        }
    }
}
