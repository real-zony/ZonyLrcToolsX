using MusicDecrypto.Library;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Logging;
using ZonyLrcTools.Common.MusicDecryption.Kugou;

namespace ZonyLrcTools.Common.MusicDecryption
{
    /// <inheritdoc cref="ZonyLrcTools.Common.MusicDecryption.IMusicDecryptor" />
    public class DefaultMusicDecryptor : IMusicDecryptor, ITransientDependency
    {
        private readonly IWarpLogger _warpLogger;
        private readonly IKugouKeyProvider? _kugouKeyProvider;

        /// <summary>
        /// 创建音乐解密器。
        /// </summary>
        public DefaultMusicDecryptor(IWarpLogger warpLogger, IKugouKeyProvider? kugouKeyProvider = null)
        {
            _warpLogger = warpLogger;
            _kugouKeyProvider = kugouKeyProvider;
        }

        /// <inheritdoc />
        public async Task<DecryptionResult> ConvertMusicAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // .kgg 的密钥存放在酷狗客户端数据库里，MusicDecrypto 覆盖不到这条链路。
                if (KggMusicDecryptor.IsKggFile(filePath))
                {
                    return await KggMusicDecryptor.ConvertAsync(filePath, _kugouKeyProvider, cancellationToken);
                }

                await using var buffer = new MarshalMemoryStream();
                await using var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                buffer.SetLengthWithPadding(file.Length);
                await file.CopyToAsync(buffer, cancellationToken);

                using var decrypto = DecryptoFactory.Create(buffer, Path.GetFileName(filePath), message => { });
                var outFileName = (await decrypto.DecryptAsync()).NewName;
                cancellationToken.ThrowIfCancellationRequested();
                var outFilePath = Path.Combine(Path.GetDirectoryName(filePath)!, outFileName);

                if (File.Exists(outFilePath))
                {
                    return DecryptionResult.Failed($"输出文件已存在，未覆盖：{outFilePath}");
                }

                var tempPath = outFilePath + $".{Guid.NewGuid():N}.part";
                var ownsTempFile = false;
                try
                {
                    await using (var outFile = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        ownsTempFile = true;
                        buffer.Position = 0;
                        await buffer.CopyToAsync(outFile, cancellationToken);
                        await outFile.FlushAsync(cancellationToken);
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    File.Move(tempPath, outFilePath);
                }
                finally
                {
                    if (ownsTempFile)
                    {
                        try
                        {
                            File.Delete(tempPath);
                        }
                        catch (IOException)
                        {
                            // 保留原始转换结果或异常。
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // 保留原始转换结果或异常。
                        }
                    }
                }

                return DecryptionResult.Success(outFilePath);
            }
            catch (OperationCanceledException)
            {
                // 用户主动取消，交给调用方区分处理。
                throw;
            }
            catch (Exception e)
            {
                return DecryptionResult.Failed(e.Message);
            }
        }
    }
}
