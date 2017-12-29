using TextEncoding = System.Text.Encoding;

namespace ZonyLrcTools.Encoders
{
    public class GeneralEncoder : IEncoder
    {
        public string EncodePageName { get; set; }

        public byte[] Encoding(string sourceStr)
        {
            var _encoding = TextEncoding.GetEncoding(EncodePageName);
            var _sourceBytes = TextEncoding.UTF8.GetBytes(sourceStr);
            return TextEncoding.Convert(TextEncoding.UTF8, _encoding, _sourceBytes);
        }

        public byte[] Encoding(byte[] sourceBytes)
        {
            var _encoding = TextEncoding.GetEncoding(EncodePageName);
            return TextEncoding.Convert(TextEncoding.UTF8, _encoding, sourceBytes);
        }

        public byte[] Encoding(byte[] sourceBytes, TextEncoding sourceEncode)
        {
            var _encoding = TextEncoding.GetEncoding(EncodePageName);
            return TextEncoding.Convert(sourceEncode, _encoding, sourceBytes);
        }
    }
}
