// Ported from the MIT-licensed reference implementation:
//   https://github.com/lizipeng0013/kgg-dec.mirror
//   Copyright (c) 2024 LSR
// The original licence text is reproduced in THIRD-PARTY-NOTICES.md at the repository root.

using System.Buffers.Binary;
using System.Text;

namespace ZonyLrcTools.Common.MusicDecryption.Kugou
{
    /// <summary>
    /// 酷狗 .kgg 文件的解密器。
    /// <remarks>
    /// .kgg 的音频主体用 QMC2 加密，而每个文件自己的 ekey 并不在文件里，
    /// 需要从酷狗客户端本地的 KGMusicV3.db 中按音频哈希查出来。
    /// 因此这个格式无法脱离酷狗客户端单独解密。
    /// </remarks>
    /// </summary>
    internal static class KggMusicDecryptor
    {
        private const int HeaderSize = KugouEkeyDatabase.KggHeaderSize;
        private const int BufferSize = 1024 * 1024;
        private const uint ExpectedEncodingVersion = 5;
        private const uint ExpectedHashLength = 0x20;

        /// <summary>
        /// .kgg 文件的 16 字节魔数。
        /// </summary>
        private static byte[] Magic => KugouEkeyDatabase.KggMagic;

        /// <summary>
        /// 判断文件是否真的是 .kgg 格式（按魔数而非扩展名）。
        /// </summary>
        public static bool IsKggFile(string filePath)
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                Span<byte> buffer = stackalloc byte[16];
                return stream.ReadAtLeast(buffer, 16, throwOnEndOfStream: false) == 16
                       && buffer.SequenceEqual(Magic);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 解密 .kgg 文件。
        /// </summary>
        /// <param name="filePath">待解密的文件路径。</param>
        /// <param name="keyProvider">密钥来源；为 null 时使用自动探测到的酷狗数据库。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>解密结果；失败时带有可读的错误说明。</returns>
        public static async Task<DecryptionResult> ConvertAsync(string filePath,
            IKugouKeyProvider? keyProvider,
            CancellationToken cancellationToken)
        {
            if (!File.Exists(filePath))
            {
                return DecryptionResult.Failed($"文件不存在: {filePath}");
            }

            await using var input = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, true);

            var header = new byte[HeaderSize];
            if (await input.ReadAtLeastAsync(header, HeaderSize, throwOnEndOfStream: false, cancellationToken) !=
                HeaderSize)
            {
                return DecryptionResult.Failed("文件过小，不是有效的 kgg 文件。");
            }

            if (!header.AsSpan(0, Magic.Length).SequenceEqual(Magic))
            {
                return DecryptionResult.Failed("文件头校验失败，不是有效的 kgg 文件。");
            }

            var offsetToAudio = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(0x10));
            var encodingVersion = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(0x14));
            if (encodingVersion != ExpectedEncodingVersion)
            {
                return DecryptionResult.Failed($"不支持的 kgg 加密版本 0x{encodingVersion:X2}，目前只支持 0x05。");
            }

            var hashLength = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(0x44));
            if (hashLength != ExpectedHashLength)
            {
                return DecryptionResult.Failed($"kgg 音频哈希长度异常（期望 0x20，实际 0x{hashLength:X2}）。");
            }

            var audioHash = Encoding.ASCII.GetString(header, 0x48, (int)hashLength);

            if (!HasDatabase(keyProvider))
            {
                return DecryptionResult.Failed(KugouEkeyDatabase.BuildMissingDatabaseMessage());
            }

            var ekey = ResolveEkey(keyProvider, audioHash);
            if (string.IsNullOrEmpty(ekey))
            {
                return DecryptionResult.Failed(
                    $"在酷狗数据库中没有找到该文件的解密密钥（音频哈希 {audioHash}）。" +
                    "通常表示这首歌尚未在酷狗客户端里播放或下载过。");
            }

            var qmcKey = EkeyDecoder.Decode(ekey, out var invalidCharCount);
            if (invalidCharCount > 0)
            {
                return DecryptionResult.Failed(
                    $"酷狗数据库里的 ekey 已损坏（有 {invalidCharCount} 个非法 Base64 字符），无法解码。" +
                    "通常表示数据库文件被写坏了，可以尝试重新登录酷狗客户端刷新该记录。");
            }

            var cipher = Qmc2Cipher.Create(qmcKey);
            if (cipher == null)
            {
                return DecryptionResult.Failed("ekey 解码失败，无法构造解密器。");
            }

            input.Seek(offsetToAudio, SeekOrigin.Begin);

            // 先解密开头 4 字节用于判断真实格式，再回去完整解密一遍。
            var magic = new byte[4];
            if (await input.ReadAtLeastAsync(magic, 4, throwOnEndOfStream: false, cancellationToken) != 4)
            {
                return DecryptionResult.Failed("kgg 音频数据为空。");
            }

            cipher.Decrypt(magic, 0);
            var extension = DetectExtension(magic);
            input.Seek(offsetToAudio, SeekOrigin.Begin);

            var outputPath = Path.Combine(
                Path.GetDirectoryName(filePath)!,
                $"{Path.GetFileNameWithoutExtension(filePath)}.{extension}");

            // 期望的输出字节数，用于校验写盘是否完整。
            var expectedLength = input.Length - offsetToAudio;

            // 目标已存在且大小正确就不再覆盖，避免重复解密白费时间。
            // 只要大小对不上（例如上次被中途取消留下的半截文件），就重新解密覆盖。
            if (File.Exists(outputPath) && new FileInfo(outputPath).Length == expectedLength)
            {
                return DecryptionResult.Success(outputPath);
            }

            // 先写临时文件，成功后再改名。中途取消或被强杀都不会污染正式输出文件名。
            var tempPath = outputPath + ".part";
            try
            {
                await using (var output = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None,
                                 BufferSize, true))
                {
                    var buffer = new byte[BufferSize];
                    long offset = 0;
                    int read;
                    while ((read = await input.ReadAsync(buffer, cancellationToken)) > 0)
                    {
                        cipher.Decrypt(buffer.AsSpan(0, read), offset);
                        await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                        offset += read;
                    }

                    await output.FlushAsync(cancellationToken);

                    if (offset != expectedLength)
                    {
                        throw new IOException($"解密写入的字节数不匹配（期望 {expectedLength}，实际 {offset}）。");
                    }
                }

                File.Move(tempPath, outputPath, overwrite: true);
            }
            catch
            {
                TryDelete(tempPath);
                throw;
            }

            return DecryptionResult.Success(outputPath);
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // 清理失败不影响主流程，下次解密会覆盖这个临时文件。
            }
        }

        private static bool HasDatabase(IKugouKeyProvider? keyProvider)
        {
            return keyProvider?.DatabasePath != null || KugouEkeyDatabase.FindDatabasePath() != null;
        }

        /// <summary>
        /// 取出音频哈希对应的 ekey；优先走外部提供的密钥来源。
        /// </summary>
        public static string? ResolveEkey(IKugouKeyProvider? keyProvider, string audioHash)
        {
            if (keyProvider != null)
            {
                return keyProvider.FindKey(audioHash);
            }

            var databasePath = KugouEkeyDatabase.FindDatabasePath();
            if (databasePath == null)
            {
                return null;
            }

            var keys = KugouEkeyDatabase.ReadEncryptionKeys(databasePath);
            return keys.TryGetValue(audioHash, out var ekey) ? ekey : null;
        }

        private static string DetectExtension(ReadOnlySpan<byte> magic)
        {
            if (magic.SequenceEqual("fLaC"u8))
            {
                return "flac";
            }

            if (magic.SequenceEqual("OggS"u8))
            {
                return "ogg";
            }

            return "mp3";
        }
    }
}
