using System;
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
        public const string ConfigurationFileName = "config.json";

        public void LoadConfiguration()
        {
            FileStream _file = File.Open(Environment.CurrentDirectory + $@"\{ConfigurationFileName}", FileMode.OpenOrCreate);
            using (StreamReader _reader = new StreamReader(_file))
            {
                string _jsonStr = _reader.ReadToEnd();
            }
        }

        public void LoadConfiguration(string filePath)
        {
            throw new NotImplementedException();
        }

        public void SaveConfiguration()
        {
            throw new NotImplementedException();
        }

        public void SaveConfiguration(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
