using System;

namespace Zony.Lib.Plugin.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PluginInfoAttribute : Attribute
    {
        /// <summary>
        /// 插件作者
        /// </summary>
        public string Author { get; private set; }
        /// <summary>
        /// 插件名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 插件版本
        /// </summary>
        public Version Version { get; private set; }
        /// <summary>
        /// 插件相关网址
        /// </summary>
        public string Url { get; private set; }
        /// <summary>
        /// 插件的描述信息
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// 初始化插件信息
        /// </summary>
        /// <param name="name">插件名称</param>
        /// <param name="author">插件作者</param>
        /// <param name="version">插件版本</param>
        /// <param name="url">插件相关网址</param>
        /// <param name="description">插件的描述信息</param>
        public PluginInfoAttribute(string name, string author, string version, string url, string description)
        {
            Author = author;
            Name = name;
            Url = url;
            Version = new Version(version);
            Description = description;
        }
    }
}