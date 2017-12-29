using TextEncoding = System.Text.Encoding;

namespace ZonyLrcTools.Encoders
{
    public class ANSIEncoder : IEncoder
    {
        public string EncodePageName { get; set; } = string.Empty;

        public byte[] Encoding(string sourceStr)
        {
            return TextEncoding.Convert(TextEncoding.UTF8, TextEncoding.Default, TextEncoding.UTF8.GetBytes(sourceStr));
        }

        public byte[] Encoding(byte[] sourceBytes)
        {
            return TextEncoding.Convert(TextEncoding.UTF8, TextEncoding.Default, sourceBytes);
        }

        public byte[] Encoding(byte[] sourceBytes, TextEncoding sourceEncode)
        {
            return TextEncoding.Convert(sourceEncode, TextEncoding.Default, sourceBytes);
        }
    }
}