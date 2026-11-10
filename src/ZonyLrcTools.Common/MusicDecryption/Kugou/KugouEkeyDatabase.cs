// Ported from the MIT-licensed reference implementation:
//   https://github.com/lizipeng0013/kgg-dec.mirror
//   Copyright (c) 2024 LSR
// The original licence text is reproduced in THIRD-PARTY-NOTICES.md at the repository root.

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace ZonyLrcTools.Common.MusicDecryption.Kugou
{
    /// <summary>
    /// 酷狗客户端本地数据库（KGMusicV3.db）的读取器。
    /// <remarks>
    /// 数据库按 1KB 分页做了 AES-128-CBC 加密，每页的密钥与 IV 由主密钥和页号派生。
    /// 解密后可以读到 <c>ShareFileItems</c> 表里的 ekey，那是 .kgg 文件真正用的解密密钥。
    /// </remarks>
    /// </summary>
    internal static class KugouEkeyDatabase
    {
        /// <summary>
        /// .kgg 文件头的长度，音频数据从这个位置之后开始。
        /// </summary>
        internal const int KggHeaderSize = 0x400;

        /// <summary>
        /// .kgg 文件的 16 字节魔数。
        /// </summary>
        internal static readonly byte[] KggMagic =
        [
            0x7C, 0xD5, 0x32, 0xEB, 0x86, 0x02, 0x7F, 0x4B,
            0xA8, 0xAF, 0xA6, 0x8E, 0x0F, 0xFF, 0x99, 0x14
        ];

        private const int PageSize = 0x400;
        private const uint PageKeySalt = 0x546C4173;
        private const uint IvMultiplier = 0x7FFFFF07;
        private const uint IvDivisor = 0xce26;
        private const uint IvBase = 0x9EF4;

        /// <summary>
        /// 酷狗客户端内置的主密钥。
        /// </summary>
        private static readonly byte[] MasterKey =
        [
            0x1d, 0x61, 0x31, 0x45, 0xb2, 0x47, 0xbf, 0x7f,
            0x3d, 0x18, 0x96, 0x72, 0x14, 0x4f, 0xe4, 0xbf
        ];

        private static readonly byte[] SqliteHeader =
        [
            (byte)'S', (byte)'Q', (byte)'L', (byte)'i', (byte)'t', (byte)'e', (byte)' ',
            (byte)'f', (byte)'o', (byte)'r', (byte)'m', (byte)'a', (byte)'t', (byte)' ', (byte)'3', 0
        ];

        /// <summary>
        /// 从数据库里读出所有非空的 ekey。
        /// </summary>
        /// <returns>音频哈希到 ekey 的映射；文件不存在或解密失败时返回空字典。</returns>
        public static IReadOnlyDictionary<string, string> ReadEncryptionKeys(string databasePath)
        {
            return ReadEncryptionKeys(databasePath, out _);
        }

        /// <summary>
        /// 从数据库里读出所有非空的 ekey，并在失败时给出可读的原因。
        /// <remarks>
        /// 读取失败的原因有多种（文件不存在 / 不是酷狗数据库 / 主密钥不匹配 / 表结构缺失 / 权限不足），
        /// 这些在 UI 上的处置完全不同，因此不能再把所有异常压成「没有密钥」。
        /// </remarks>
        /// </summary>
        /// <param name="databasePath">数据库文件路径。</param>
        /// <param name="failureReason">读取失败时的原因说明；成功时为 null。</param>
        public static IReadOnlyDictionary<string, string> ReadEncryptionKeys(string databasePath, out string? failureReason)
        {
            failureReason = null;
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(databasePath))
            {
                failureReason = "没有指定酷狗数据库路径。";
                return result;
            }

            if (!File.Exists(databasePath))
            {
                failureReason = $"数据库文件不存在：{databasePath}";
                return result;
            }

            byte[] plain;
            try
            {
                plain = DecryptDatabase(databasePath);
            }
            catch (Exception ex)
            {
                failureReason = $"数据库解密失败（文件可能不是酷狗 KGMusicV3.db，或版本不受支持）：{ex.Message}";
                return result;
            }

            if (plain.Length == 0)
            {
                failureReason = $"数据库解密后内容为空，格式不像酷狗 KGMusicV3.db：{databasePath}";
                return result;
            }

            // 直接从内存解析，避免明文密钥落盘和并发清理临时文件的竞争。
            var pinned = GCHandle.Alloc(plain, GCHandleType.Pinned);
            try
            {
                using var connection = new SqliteConnection("Data Source=:memory:;Pooling=False");
                connection.Open();
                var error = SQLitePCL.raw.sqlite3_deserialize(connection.Handle, "main",
                    pinned.AddrOfPinnedObject(), plain.Length, plain.Length, SQLitePCL.raw.SQLITE_DESERIALIZE_READONLY);
                if (error != SQLitePCL.raw.SQLITE_OK)
                {
                    throw new SqliteException("无法读取解密后的酷狗数据库。", error);
                }

                using var command = connection.CreateCommand();
                command.CommandText = "select EncryptionKeyId, EncryptionKey from ShareFileItems where EncryptionKey != ''";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) || reader.IsDBNull(1))
                    {
                        continue;
                    }

                    var keyId = reader.GetString(0);
                    var ekey = reader.GetString(1);
                    if (keyId.Length > 0 && ekey.Length > 0)
                    {
                        result[keyId] = ekey;
                    }
                }
            }
            catch (Exception ex)
            {
                failureReason = $"数据库内容读取失败（表 ShareFileItems 可能不存在）：{ex.Message}";
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            finally
            {
                pinned.Free();
            }

            return result;
        }

        /// <summary>
        /// 把整个加密数据库解密成明文的 SQLite 字节流。
        /// </summary>
        private static byte[] DecryptDatabase(string databasePath)
        {
            using var source = new FileStream(databasePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var sourceLength = source.Length;
            if (sourceLength == 0 || sourceLength > Array.MaxLength || sourceLength % PageSize != 0)
            {
                return [];
            }

            var length = (int)sourceLength;
            var pageCount = length / PageSize;
            var plain = new byte[length];
            var headerBuffer = new byte[8];

            // 复用同一个 Aes 实例：每页只是换 Key，避免 16000+ 次 Aes.Create() 的对象构造开销。
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;

            for (var pageIndex = 1; pageIndex <= pageCount; pageIndex++)
            {
                var target = plain.AsSpan((pageIndex - 1) * PageSize, PageSize);
                source.ReadExactly(target);

                DerivePageKeyIv((uint)pageIndex, out var key, out var iv);

                if (pageIndex == 1)
                {
                    // 首页前 16 字节是明文文件头，真正从 0x10 开始加密；
                    // 解密前要把 0x08..0x10 复制到 0x10 处作为 CBC 的密文块。
                    target.Slice(0x10, 8).CopyTo(headerBuffer);
                    target.Slice(0x08, 8).CopyTo(target[0x10..]);

                    AesCbcDecryptInPlace(aes, target[0x10..], key, iv);

                    if (!target.Slice(0x10, 8).SequenceEqual(headerBuffer))
                    {
                        return [];
                    }

                    SqliteHeader.CopyTo(target);
                }
                else
                {
                    AesCbcDecryptInPlace(aes, target, key, iv);
                }
            }

            return plain;
        }

        private static void AesCbcDecryptInPlace(Aes aes, Span<byte> buffer, byte[] key, byte[] iv)
        {
            aes.Key = key;

            // 两个坑都要显式避开：
            // 1) 参数顺序是 TryDecryptCbc(ciphertext, iv, destination, out written, padding)，
            //    写成 (buffer, buffer, iv, ...) 会把 buffer 自己当 IV、把真正的 IV 覆盖掉；
            // 2) padding 的默认值是 PKCS7 而不是实例上的 aes.Padding，分页数据是整块无填充的，
            //    不传 PaddingMode.None 就会在每页末尾抛「Padding is invalid」。
            if (!aes.TryDecryptCbc(buffer, iv, buffer, out var written, PaddingMode.None) ||
                written != buffer.Length)
            {
                throw new CryptographicException("酷狗数据库分页解密失败。");
            }
        }

        /// <summary>
        /// 按页号派生出该页使用的 AES 密钥与 IV。
        /// </summary>
        private static void DerivePageKeyIv(uint pageNumber, out byte[] key, out byte[] iv)
        {
            var buffer = new byte[0x18];
            MasterKey.CopyTo(buffer, 0);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(0x10), pageNumber);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(0x14), PageKeySalt);

            key = MD5.HashData(buffer);

            // IV 由一个线性同余式的四次迭代结果拼成。
            var value = pageNumber + 1;
            for (var i = 0; i < 16; i += 4)
            {
                unchecked
                {
                    var quotient = value / IvDivisor;
                    var product = IvMultiplier * quotient;
                    var next = IvBase * value - product;
                    if ((next & 0x8000_0000) != 0)
                    {
                        next += IvMultiplier;
                    }

                    value = next;
                    BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(i), value);
                }
            }

            iv = MD5.HashData(buffer.AsSpan(0, 0x10));
        }

        /// <summary>
        /// 酷狗客户端数据库的常见位置。
        /// </summary>
        public static IEnumerable<string> EnumerateCandidatePaths()
        {
            var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!string.IsNullOrEmpty(roaming))
            {
                yield return Path.Combine(roaming, "KuGou8", "KGMusicV3.db");
                yield return Path.Combine(roaming, "KuGou", "KGMusicV3.db");
            }

            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!string.IsNullOrEmpty(localAppData))
            {
                yield return Path.Combine(localAppData, "KuGou8", "KGMusicV3.db");
                yield return Path.Combine(localAppData, "KuGou", "KGMusicV3.db");
            }
        }

        /// <summary>
        /// 找到第一个存在的数据库路径。
        /// </summary>
        public static string? FindDatabasePath()
        {
            foreach (var path in EnumerateCandidatePaths())
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// 让用户能看懂的错误信息。
        /// </summary>
        public static string BuildMissingDatabaseMessage()
        {
            var builder = new StringBuilder();
            builder.Append("未找到酷狗客户端数据库 KGMusicV3.db，无法获取解密密钥。");
            builder.Append("请确认已安装酷狗音乐并至少登录过一次。已尝试的路径：");
            foreach (var path in EnumerateCandidatePaths())
            {
                builder.Append(' ').Append(path).Append(';');
            }

            return builder.ToString();
        }
    }
}
