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
        /// 按魔数和加密版本判断是否为 KGG，避免误判共用魔数的旧版 KGM。
        /// </summary>
        public static bool IsKggFile(string filePath)
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                Span<byte> buffer = stackalloc byte[0x18];
                return stream.ReadAtLeast(buffer, buffer.Length, throwOnEndOfStream: false) == buffer.Length
                       && buffer[..Magic.Length].SequenceEqual(Magic)
                       && BinaryPrimitives.ReadUInt32LittleEndian(buffer[0x14..]) == ExpectedEncodingVersion;
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
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
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
            if (offsetToAudio < HeaderSize || offsetToAudio > input.Length - 4)
            {
                return DecryptionResult.Failed("kgg 音频偏移超出有效范围。");
            }

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
                return DecryptionResult.Failed(keyProvider?.Probe().Message ?? KugouEkeyDatabase.BuildMissingDatabaseMessage());
            }

            var ekey = ResolveEkey(keyProvider, audioHash);
            cancellationToken.ThrowIfCancellationRequested();
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
            if (extension == null)
            {
                return DecryptionResult.Failed("解密后的音频格式无法识别，文件或密钥可能已损坏。");
            }

            input.Seek(offsetToAudio, SeekOrigin.Begin);

            var outputPath = Path.Combine(
                Path.GetDirectoryName(filePath)!,
                $"{Path.GetFileNameWithoutExtension(filePath)}.{extension}");

            // 期望的输出字节数，用于校验写盘是否完整。
            var expectedLength = input.Length - offsetToAudio;

            if (File.Exists(outputPath))
            {
                return DecryptionResult.Failed($"输出文件已存在，未覆盖：{outputPath}");
            }

            // 每次转换独占临时文件，仅在完整写入后发布，且不覆盖已有输出。
            var tempPath = outputPath + $".{Guid.NewGuid():N}.part";
            var ownsTempFile = false;
            try
            {
                await using (var output = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                                 BufferSize, true))
                {
                    ownsTempFile = true;
                    var buffer = new byte[BufferSize];
                    var offset = 0L;
                    while (true)
                    {
                        var read = await input.ReadAsync(buffer, cancellationToken);
                        if (read == 0)
                        {
                            break;
                        }

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

                cancellationToken.ThrowIfCancellationRequested();
                File.Move(tempPath, outputPath);
            }
            finally
            {
                if (ownsTempFile)
                {
                    TryDelete(tempPath);
                }
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
                // 保留原始转换结果或异常。
            }
        }

        private static bool HasDatabase(IKugouKeyProvider? keyProvider)
        {
            return keyProvider != null ? keyProvider.DatabasePath != null : KugouEkeyDatabase.FindDatabasePath() != null;
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

        private static string? DetectExtension(ReadOnlySpan<byte> magic)
        {
            if (magic.SequenceEqual("fLaC"u8))
            {
                return "flac";
            }

            if (magic.SequenceEqual("OggS"u8))
            {
                return "ogg";
            }

            return magic[..3].SequenceEqual("ID3"u8)
                   || (magic[0] == 0xFF && (magic[1] & 0xE0) == 0xE0 && (magic[1] & 0x06) != 0)
                ? "mp3"
                : null;
        }
    }
}
