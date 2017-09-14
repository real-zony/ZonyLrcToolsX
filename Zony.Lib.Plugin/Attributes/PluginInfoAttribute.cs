using System;

namespace Zony.Lib.Plugin.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PluginInfoAttribute : Attribute
    {
        public string Author { get; private set; }
        public string Name { get; private set; }
        public Version Version { get; private set; }
        public string Url { get; private set; }
        public string Description { get; private set; }

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