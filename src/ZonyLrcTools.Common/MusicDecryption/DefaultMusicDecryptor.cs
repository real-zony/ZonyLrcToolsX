using MusicDecrypto.Library;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Logging;

namespace ZonyLrcTools.Common.MusicDecryption
{
    /// <inheritdoc cref="ZonyLrcTools.Common.MusicDecryption.IMusicDecryptor" />
    public class DefaultMusicDecryptor : IMusicDecryptor, ITransientDependency
    {
        private readonly IWarpLogger _warpLogger;

        public DefaultMusicDecryptor(IWarpLogger warpLogger)
        {
            _warpLogger = warpLogger;
        }

        public async Task<DecryptionResult> ConvertMusicAsync(string filePath)
        {
            try
            {
                await using var buffer = new MarshalMemoryStream();
                await using var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                buffer.SetLengthWithPadding(file.Length);
                await file.CopyToAsync(buffer);

                using var decrypto = DecryptoFactory.Create(buffer, Path.GetFileName(filePath), message => { });
                var outFileName = (await decrypto.DecryptAsync()).NewName;
                var outFilePath = Path.Combine(Path.GetDirectoryName(filePath)!, outFileName);

                if (!File.Exists(outFilePath))
                {
                    await using var outFile = new FileStream(outFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    await buffer.CopyToAsync(outFile);
                }

                return DecryptionResult.Success(outFilePath);
            }
            catch (Exception e)
            {
                return DecryptionResult.Failed(e.Message);
            }
        }
    }
}