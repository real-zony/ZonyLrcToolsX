using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using ZonyLrcTools.Common.Interfaces;

namespace ZonyLrcTools.Common
{
    public class ConfigurationManager : IConfigurationManager
    {
        public ConfigurationModel ConfigModel { get; private set; }
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
                using (StreamWriter _sr = new StreamWriter(_file))
                {
                    _sr.Write(JsonConvert.SerializeObject(ConfigModel));
                }
            }
        }

        /// <summary>
        /// 默认设置初始化
        /// </summary>
        /// <returns></returns>
        private ConfigurationModel InitializeDefaultConfigurationModel()
        {
            return new ConfigurationModel()
            {
                EncodingName = "UTF-8",
                DownloadThreadNumber = 1,
                IsReplaceLyricFile = true,
                IsCheckUpdate = true,
                IsAgree = false,
                ExtensionsName = new List<string>() { "*.mp3", "*.ape", "*.flac", "*.m4a" },
                ProxyIP = string.Empty,
                ProxyPort = 0,
                PluginOptions = new Dictionary<string, Dictionary<string, object>>() { { "Zony.Lib.NetEase", new Dictionary<string, object>() { { "ReplaceLF", false } } } }
            };
        }
    }
}