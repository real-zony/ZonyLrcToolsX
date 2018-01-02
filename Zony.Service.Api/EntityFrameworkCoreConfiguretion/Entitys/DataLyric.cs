using System;

namespace Zony.Service.Api.EntityFrameworkCoreConfiguretion.Entitys
{
    public class DataLyric
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public DateTime CreateTime { get; set; }
        public string LyricText { get; set; }
    }
}