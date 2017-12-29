using System;
using TextEncoding = System.Text.Encoding;

namespace ZonyLrcTools.Encoders
{
    public class UTF_8BOMEncoder : IEncoder
    {
        public string EncodePageName { get; set; } = string.Empty;

        public byte[] Encoding(string sourceStr)
        {
            byte[] _sourceBytes = TextEncoding.UTF8.GetBytes(sourceStr);
            return GenerateBytes(_sourceBytes);
        }

        public byte[] Encoding(byte[] sourceBytes)
        {
            return GenerateBytes(sourceBytes);
        }

        public byte[] Encoding(byte[] sourceBytes, TextEncoding sourceEncode)
        {
            byte[] _convertBytes = TextEncoding.Convert(sourceEncode, TextEncoding.UTF8, sourceBytes);
            return GenerateBytes(_convertBytes);
        }

        /// <summary>
        /// 将转换好的编码在其头部增加 3 字节 BOM 标识
        /// </summary>
        /// <param name="sourceBytes">转换好的数据字节</param>
        private byte[] GenerateBytes(byte[] sourceBytes)
        {
            byte[] _tmpData = new byte[sourceBytes.Length + 3];
            _tmpData[0] = 0xef;
            _tmpData[1] = 0xbb;
            _tmpData[2] = 0xbf;

            Array.Copy(sourceBytes, 0, _tmpData, 3, sourceBytes.Length);
            return _tmpData;
        }
    }
}