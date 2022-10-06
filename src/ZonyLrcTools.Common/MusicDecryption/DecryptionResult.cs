namespace ZonyLrcTools.Common.MusicDecryption
{
    public class DecryptionResult
    {
        public byte[] Data { get; protected set; }

        public Dictionary<string, object> ExtensionObjects { get; set; }

        public DecryptionResult(byte[] data)
        {
            Data = data;
        }
    }
}