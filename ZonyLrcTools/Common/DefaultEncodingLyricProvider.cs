using System.Text;
using ZonyLrcTools.Common.Interfaces;

namespace ZonyLrcTools.Common
{
    public class DefaultEncodingLyricProvider : IEncodingLyricProvider
    {
        private readonly IConfigurationManager m_configMgr;

        public DefaultEncodingLyricProvider(IConfigurationManager configMgr)
        {
            m_configMgr = configMgr;
        }

        public byte[] EncodeText(string sourceStr)
        {
            return Encoding.UTF8.GetBytes(sourceStr);
        }

        public byte[] EncodeText(byte[] sourceStr)
        {
            return sourceStr;
        }
    }
}
