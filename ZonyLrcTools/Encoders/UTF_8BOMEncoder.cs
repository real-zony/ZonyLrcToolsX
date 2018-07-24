using System;
using TextEncoding = System.Text.Encoding;

namespace ZonyLrcTools.Encoders
{
    public class UTF_8BOMEncoder : IEncoder
    {
        public string EncodePageName { get; set; } = string.Empty;

        public byte[] Encoding(string sourceStr)
        {
            byte[] sourceBytes = TextEncoding.UTF8.GetBytes(sourceStr);
            return GenerateBytes(sourceBytes);
        }

        public byte[] Encoding(byte[] sourceBytes)
        {
            return GenerateBytes(sourceBytes);
        }

        public byte[] Encoding(byte[] sourceBytes, TextEncoding sourceEncode)
        {
            byte[] convertBytes = TextEncoding.Convert(sourceEncode, TextEncoding.UTF8, sourceBytes);
            return GenerateBytes(convertBytes);
        }

        /// <summary>
        /// 将转换好的编码在其头部增加 3 字节 BOM 标识
        /// </summary>
        /// <param name="sourceBytes">转换好的数据字节</param>
        private byte[] GenerateBytes(byte[] sourceBytes)
        {
            byte[] tmpData = new byte[sourceBytes.Length + 3];
            tmpData[0] = 0xef;
            tmpData[1] = 0xbb;
            tmpData[2] = 0xbf;

            Array.Copy(sourceBytes, 0, tmpData, 3, sourceBytes.Length);
            return tmpData;
        }
    }
}