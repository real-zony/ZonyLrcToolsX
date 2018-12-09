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

        /// <summary>
        /// 从默认配置文件加载设置
        /// </summary>
        public void LoadConfiguration()
        {
            LoadConfiguration(ConfigurationFileName);
        }

        /// <summary>
        /// 加载设置
        /// </summary>
        /// <param name="filePath">设置文件路径</param>
        public void LoadConfiguration(string filePath)
        {
            if (!File.Exists(filePath))
            {
                ConfigModel = InitializeDefaultConfigurationModel();
                return;
            }

            FileStream file = File.Open(filePath, FileMode.Open);
            using (StreamReader reader = new StreamReader(file))
            {
                ConfigModel = JsonConvert.DeserializeObject<ConfigurationModel>(reader.ReadToEnd());
            }
        }

        /// <summary>
        /// 保存设置到默认配置文件
        /// </summary>
        public void SaveConfiguration()
        {
            SaveConfiguration(ConfigurationFileName);
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="filePath">设置文件路径</param>
        public void SaveConfiguration(string filePath)
        {
            using (FileStream file = File.Open(filePath, FileMode.OpenOrCreate))
            {
                file.SetLength(0);
                using (StreamWriter sr = new StreamWriter(file))
                {
                    sr.Write(JsonConvert.SerializeObject(ConfigModel));
                }
            }
        }

        /// <summary>
        /// 默认设置初始化
        /// </summary>
        /// <returns></returns>
        private ConfigurationModel InitializeDefaultConfigurationModel()
        {
            return new ConfigurationModel
            {
                EncodingName = "UTF-8",
                DownloadThreadNumber = 1,
                IsReplaceLyricFile = true,
                IsCheckUpdate = true,
                IsAgree = false,
                ExtensionsName = new List<string> { "*.mp3", "*.ape", "*.flac", "*.m4a" },
                ProxyIP = string.Empty,
                ProxyPort = 0,
                PluginOptions = new Dictionary<string, Dictionary<string, object>>
                {
                    {
                        "Zony.Lib.NetEase", new Dictionary<string, object>
                        {
                            { "ReplaceLF", false },
                            { "Inline", true }
                        } 
                    }
                }
            };
        }
    }
}