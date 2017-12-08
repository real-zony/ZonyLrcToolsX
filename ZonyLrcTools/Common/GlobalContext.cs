using System.Collections.Generic;
using System.Collections.Concurrent;
using Zony.Lib.Plugin.Models;
using Zony.Lib.Infrastructures.Dependency;

namespace ZonyLrcTools.Common
{
    public class GlobalContext : ISingletonDependency
    {
        public Dictionary<string, List<string>> Musics { get; set; }

        public ConcurrentBag<MusicInfoModel> MusicInfos { get; set; }
    }
}