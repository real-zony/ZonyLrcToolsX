using System;

namespace ZonyLrcTools.Common.Interfaces
{
    public class EncodingLyricProvider : IEncodingLyricProvider
    {
        private readonly IConfigurationManager m_configMgr;

        public EncodingLyricProvider(IConfigurationManager configMgr)
        {
            m_configMgr = configMgr;
        }

        public string EncodeText(string sourceStr)
        {
            throw new NotImplementedException();
        }

        public string EncodeText(byte[] sourceStr)
        {
            throw new NotImplementedException();
        }
    }
}
