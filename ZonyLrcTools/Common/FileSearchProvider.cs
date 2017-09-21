using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ZonyLrcTools.Common
{
    public class FileSearchProvider
    {
        private readonly ISettingManager m_settingManager;

        public FileSearchProvider(ISettingManager settingManager)
        {
            m_settingManager = settingManager;
        }

        public async List<string> FindFiles(string directoryPath)
        {
            await Task.Run(() =>
            {
                var _result = new List<string>();



                return new List<string>();
            });
        }
    }
}
