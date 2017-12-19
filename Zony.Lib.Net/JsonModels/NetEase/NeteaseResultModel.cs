using System.Collections.Generic;

namespace Zony.Lib.Net.JsonModels.NetEase
{
    public class NetEaseResultModel
    {
        public NetEaseInnerResultModel result { get; set; }
        public string code { get; set; }
    }

    public class NetEaseInnerResultModel
    {
        public List<NeteaseSongModel> tracks { get; set; }
    }
}
