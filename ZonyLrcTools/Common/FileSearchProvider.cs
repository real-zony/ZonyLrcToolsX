using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZonyLrcTools.Common
{
    public class FileSearchProvider : ISearchProvider
    {
        private readonly ISettingManager m_settingManager;

        public FileSearchProvider(ISettingManager settingManager)
        {
            m_settingManager = settingManager;
        }

        public List<string> FindFiles(string directoryPath)
        {
            return null;
        }

        public async Task<List<string>> FindFilesAsync(string diretcoryPath)
        {

            return await Task.Run(() =>
            {
                return new List<string>();
            });
        }
    }
}
