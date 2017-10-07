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

        public async List<string> FindFiles(string directoryPath)
        {
            return await Task.Run<List<string>>(() =>
            {
                return new List<string>();
            });
        }

        public Task<List<string>> FindFilesAsync(string diretcoryPath)
        {
            throw new System.NotImplementedException();
        }
    }
}
