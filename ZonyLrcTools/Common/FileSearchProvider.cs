using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZonyLrcTools.Common
{
    public class FileSearchProvider
    {
        private readonly ISettingManager m_settingManager;

        public FileSearchProvider(ISettingManager settingManager)
        {
            m_settingManager = settingManager;
        }

        public async List<string> FindFiles()
        {
            await Task.Run(() =>
            {
                return new List<string>();
            });
        }
    }
}
