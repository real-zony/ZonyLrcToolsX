using TextEncoding = System.Text.Encoding;

namespace ZonyLrcTools.Encoders
{
    public class GeneralEncoder : IEncoder
    {
        public string EncodePageName { get; set; }

        public byte[] Encoding(string sourceStr)
        {
            var encoding = TextEncoding.GetEncoding(EncodePageName);
            var sourceBytes = TextEncoding.UTF8.GetBytes(sourceStr);
            return TextEncoding.Convert(TextEncoding.UTF8, encoding, sourceBytes);
        }

        public byte[] Encoding(byte[] sourceBytes)
        {
            var encoding = TextEncoding.GetEncoding(EncodePageName);
            return TextEncoding.Convert(TextEncoding.UTF8, encoding, sourceBytes);
        }

        public byte[] Encoding(byte[] sourceBytes, TextEncoding sourceEncode)
        {
            var encoding = TextEncoding.GetEncoding(EncodePageName);
            return TextEncoding.Convert(sourceEncode, encoding, sourceBytes);
        }
    }
}
