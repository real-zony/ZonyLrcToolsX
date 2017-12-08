using System.Collections.Generic;
using Zony.Lib.Plugin.Models;
using Zony.Lib.Infrastructures.Dependency;

namespace ZonyLrcTools.Common
{
    public class GlobalContext : ISingletonDependency
    {
        public Dictionary<string, List<string>> Musics { get; set; }

        public List<MusicInfoModel> MusicInfos { get; set; }
    }
}