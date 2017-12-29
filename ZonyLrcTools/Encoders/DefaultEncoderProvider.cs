namespace ZonyLrcTools.Encoders
{
    public class DefaultEncoderProvider : IEncoderProvider
    {
        public IEncoder GetEncoder(string encodePageName)
        {
            if (encodePageName == "UTF-8 BOM") return new UTF_8BOMEncoder();
            if (encodePageName == "ANSI") return new ANSIEncoder();
            return new GeneralEncoder() { EncodePageName = encodePageName };
        }
    }
}