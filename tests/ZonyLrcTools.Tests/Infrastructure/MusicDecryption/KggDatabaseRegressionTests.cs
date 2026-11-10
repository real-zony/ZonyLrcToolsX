using System;
using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using Xunit;
using ZonyLrcTools.Common.MusicDecryption;
using ZonyLrcTools.Common.MusicDecryption.Kugou;

namespace ZonyLrcTools.Tests.Infrastructure.MusicDecryption
{
    /// <summary>
    /// 使用合成加密 SQLite 数据验证密钥读取，不依赖真实酷狗数据库。
    /// </summary>
    public sealed class KggDatabaseRegressionTests : IDisposable
    {
        private readonly string _directory = Directory.CreateTempSubdirectory("zony-kgg-db-tests-").FullName;

        /// <summary>
        /// 通过 SQLite 查询完整的合成加密数据库，并保持原文件不变。
        /// </summary>
        [Fact]
        public void ReadEncryptionKeys_ReadsEncryptedDatabaseWithoutChangingSource()
        {
            var path = CreateEncryptedDatabase("keys;test.db");
            var original = File.ReadAllBytes(path);

            var keys = KugouEkeyDatabase.ReadEncryptionKeys(path, out var failureReason);

            Assert.Null(failureReason);
            Assert.Equal("synthetic-ekey", keys["audio-hash"]);
            Assert.Equal(original, File.ReadAllBytes(path));
            Assert.Single(Directory.GetFiles(_directory));
        }

        /// <summary>
        /// 区分大小写的文件系统上，切换路径大小写必须重新加载数据库。
        /// </summary>
        [Fact]
        public void SetDatabasePath_ReloadsDistinctCaseSensitivePath()
        {
            var lowerPath = Path.Combine(_directory, "keys.db");
            var upperPath = Path.Combine(_directory, "KEYS.db");
            File.WriteAllText(lowerPath, "invalid database");
            if (File.Exists(upperPath))
            {
                return;
            }

            CreateEncryptedDatabase("KEYS.db");
            var provider = new KugouKeyProvider();
            provider.SetDatabasePath(lowerPath);
            Assert.False(provider.Probe().IsAvailable);

            provider.SetDatabasePath(upperPath);

            Assert.True(provider.Probe().IsAvailable);
            Assert.Equal("synthetic-ekey", provider.FindKey("audio-hash"));
        }

        /// <summary>
        /// 删除本测试创建的数据。
        /// </summary>
        public void Dispose() => Directory.Delete(_directory, true);

        private string CreateEncryptedDatabase(string name)
        {
            var path = Path.Combine(_directory, name);
            var connectionString = new SqliteConnectionStringBuilder { DataSource = path, Pooling = false }.ToString();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA page_size=1024; CREATE TABLE ShareFileItems (EncryptionKeyId TEXT, EncryptionKey TEXT);"
                                      + "INSERT INTO ShareFileItems VALUES ('audio-hash', 'synthetic-ekey');";
                command.ExecuteNonQuery();
            }

            var bytes = File.ReadAllBytes(path);
            using var aes = Aes.Create();
            var derivation = new byte[24];
            for (var page = 1; page <= bytes.Length / 1024; page++)
            {
                Convert.FromHexString("1D613145B247BF7F3D189672144FE4BF").CopyTo(derivation, 0);
                BinaryPrimitives.WriteUInt32LittleEndian(derivation.AsSpan(16), (uint)page);
                BinaryPrimitives.WriteUInt32LittleEndian(derivation.AsSpan(20), 0x546C4173);
                aes.Key = MD5.HashData(derivation);
                var value = (uint)page + 1;
                for (var index = 0; index < 16; index += 4)
                {
                    value = unchecked(0x9EF4 * value - 0x7FFFFF07 * (value / 0xCE26));
                    if ((value & 0x80000000) != 0)
                    {
                        value = unchecked(value + 0x7FFFFF07);
                    }

                    BinaryPrimitives.WriteUInt32LittleEndian(derivation.AsSpan(index), value);
                }

                var iv = MD5.HashData(derivation.AsSpan(0, 16));
                var offset = (page - 1) * 1024;
                if (page == 1)
                {
                    var header = bytes.AsSpan(16, 8).ToArray();
                    var encrypted = aes.EncryptCbc(bytes.AsSpan(16, 1008), iv, PaddingMode.None);
                    encrypted.CopyTo(bytes, 16);
                    encrypted.AsSpan(0, 8).CopyTo(bytes.AsSpan(8));
                    header.CopyTo(bytes, 16);
                }
                else
                {
                    var encrypted = aes.EncryptCbc(bytes.AsSpan(offset, 1024), iv, PaddingMode.None);
                    encrypted.CopyTo(bytes, offset);
                }
            }

            File.WriteAllBytes(path, bytes);
            return path;
        }
    }
}
