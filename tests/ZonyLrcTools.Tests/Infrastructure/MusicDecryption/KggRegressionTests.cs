using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ZonyLrcTools.Common.MusicDecryption;
using ZonyLrcTools.Common.MusicDecryption.Kugou;

namespace ZonyLrcTools.Tests.Infrastructure.MusicDecryption
{
    /// <summary>
    /// 使用独立合成数据验证 KGG 解密边界，不代表真实客户端样本。
    /// </summary>
    public sealed class KggRegressionTests : IDisposable
    {
        private const string Ekey = "AQIDBAUGBwg0ENpQsPOCXXtClrT99x6WMpfYT1U/+p8=";
        private readonly string _directory = Directory.CreateTempSubdirectory("zony-kgg-tests-").FullName;

        /// <summary>
        /// 无填充 Base64 的末组三个字符不能丢掉第二个字节。
        /// </summary>
        [Fact]
        public void Decode_UnpaddedEkey_PreservesEveryKeyByte()
        {
            var result = EkeyDecoder.Decode(Ekey.TrimEnd('='), out var invalidCount);

            Assert.Equal(Enumerable.Range(1, 16).Select(value => (byte)value), result);
            Assert.Equal(0, invalidCount);
        }

        /// <summary>
        /// 填充之后还有数据的 Base64 是损坏密钥，不能静默忽略。
        /// </summary>
        [Fact]
        public void Decode_RejectsDataAfterPadding()
        {
            Assert.Empty(EkeyDecoder.Decode(Ekey + "AAAA"));
        }

        /// <summary>
        /// QMC 查表算法使用相同位移量左右移位，不是循环移位。
        /// </summary>
        [Fact]
        public void MapCipher_UsesReferenceMask()
        {
            var cipher = Qmc2Cipher.Create([1, 2, 3, 4, 5, 6, 7, 8]);
            var data = new byte[8];

            cipher!.Decrypt(data, 0);

            Assert.Equal(new byte[] { 29, 65, 192, 65, 29, 65, 192, 65 }, data);
        }

        /// <summary>
        /// 合成密文必须还原为预先确定的音频字节。
        /// </summary>
        [Fact]
        public async Task ConvertAsync_DecryptsSyntheticFile()
        {
            var input = CreateKgg();
            var result = await KggMusicDecryptor.ConvertAsync(input, new KeyProvider());

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal("fLaCtest-payload"u8.ToArray(), File.ReadAllBytes(result.OutputFilePath!));
            Assert.Single(Directory.GetFiles(_directory, "*.flac"));
            Assert.Empty(Directory.GetFiles(_directory, "*.part"));
        }

        /// <summary>
        /// 同名文件不论长度是否相同，都不能覆盖或冒充本次成功结果。
        /// </summary>
        [Theory]
        [InlineData(16)]
        [InlineData(3)]
        public async Task ConvertAsync_PreservesExistingOutput(int length)
        {
            var input = CreateKgg();
            var output = Path.ChangeExtension(input, ".flac");
            var original = Enumerable.Repeat((byte)0x42, length).ToArray();
            File.WriteAllBytes(output, original);

            var result = await KggMusicDecryptor.ConvertAsync(input, new KeyProvider());

            Assert.False(result.IsSuccess);
            Assert.Equal(original, File.ReadAllBytes(output));
        }

        /// <summary>
        /// 旧版 KGM 与 KGG 魔数相同，不能将旧格式误送到 KGG 解密器。
        /// </summary>
        [Theory]
        [InlineData(2u)]
        [InlineData(3u)]
        public void IsKggFile_DoesNotCaptureLegacyKgm(uint version)
        {
            var path = CreateKgg();
            var data = File.ReadAllBytes(path);
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x14), version);
            File.WriteAllBytes(path, data);

            Assert.False(KggMusicDecryptor.IsKggFile(path));
            Assert.False(new KugouKeyProvider().IsKggFile(path));
            Assert.Null(new KugouKeyProvider().ReadAudioHash(path));
        }

        /// <summary>
        /// 不能覆盖另一个任务或程序的临时文件。
        /// </summary>
        [Fact]
        public async Task ConvertAsync_PreservesUnownedPartialFile()
        {
            var input = CreateKgg();
            var partialPath = Path.ChangeExtension(input, ".flac") + ".part";
            File.WriteAllText(partialPath, "other conversion");

            var result = await KggMusicDecryptor.ConvertAsync(input, new KeyProvider());

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal("other conversion", File.ReadAllText(partialPath));
        }

        /// <summary>
        /// 音频偏移不能指向头部或文件末尾之外。
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1023u)]
        [InlineData(uint.MaxValue)]
        public async Task ConvertAsync_RejectsInvalidAudioOffset(uint offset)
        {
            var input = CreateKgg(offset);

            var result = await KggMusicDecryptor.ConvertAsync(input, new KeyProvider());

            Assert.False(result.IsSuccess);
            Assert.Single(Directory.GetFiles(_directory));
        }

        /// <summary>
        /// 损坏的音频头不能默认为 MP3 并报告成功。
        /// </summary>
        [Fact]
        public async Task ConvertAsync_RejectsUnknownAudioHeader()
        {
            var input = CreateKgg();
            var bytes = File.ReadAllBytes(input);
            bytes[1024] ^= 0xFF;
            File.WriteAllBytes(input, bytes);

            var result = await KggMusicDecryptor.ConvertAsync(input, new KeyProvider());

            Assert.False(result.IsSuccess);
            Assert.Single(Directory.GetFiles(_directory));
        }

        /// <summary>
        /// 查找密钥期间取消后，不应继续发布输出文件。
        /// </summary>
        [Fact]
        public async Task ConvertAsync_CancellationDoesNotPublishOutput()
        {
            var input = CreateKgg();
            using var cancellation = new CancellationTokenSource();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                KggMusicDecryptor.ConvertAsync(input, new KeyProvider(cancellation.Cancel), cancellation.Token));

            Assert.Single(Directory.GetFiles(_directory));
        }

        /// <summary>
        /// 删除本测试创建的数据。
        /// </summary>
        public void Dispose() => Directory.Delete(_directory, true);

        private string CreateKgg(uint offset = 1024)
        {
            var encryptedAudio = Convert.FromHexString("59CEA1024B24B3F612F2A138532EA1E6");
            var data = new byte[1024 + encryptedAudio.Length];
            Convert.FromHexString("7CD532EB86027F4BA8AFA68E0FFF9914").CopyTo(data, 0);
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x10), offset);
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x14), 5);
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x44), 32);
            "0123456789abcdef0123456789abcdef"u8.CopyTo(data.AsSpan(0x48));
            encryptedAudio.CopyTo(data, 1024);
            var path = Path.Combine(_directory, "song.kgg");
            File.WriteAllBytes(path, data);
            return path;
        }

        private sealed class KeyProvider(Action? onFind = null) : IKugouKeyProvider
        {
            public string DatabasePath => "synthetic-key-source";
            public void SetDatabasePath(string? path) => throw new NotSupportedException();
            public KugouDatabaseInfo Probe() => new(true, DatabasePath, 1, "合成密钥");
            public void Invalidate() => throw new NotSupportedException();
            public bool IsKggFile(string filePath) => throw new NotSupportedException();
            public string? ReadAudioHash(string filePath) => throw new NotSupportedException();
            public string? FindKey(string audioHash)
            {
                onFind?.Invoke();
                return audioHash == "0123456789abcdef0123456789abcdef" ? Ekey : null;
            }
        }
    }
}
