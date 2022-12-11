namespace ZonyLrcTools.Common.MusicDecryption
{
    public sealed class DecryptionResult
    {
        public bool IsSuccess { get; set; } = false;

        public string? OutputFilePath { get; set; } = default;

        public string? ErrorMessage { get; set; } = default;

        public static DecryptionResult Failed(string errorMessage)
        {
            return new DecryptionResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }

        public static DecryptionResult Success(string outputFilePath)
        {
            return new DecryptionResult
            {
                IsSuccess = true,
                OutputFilePath = outputFilePath
            };
        }
    }
}