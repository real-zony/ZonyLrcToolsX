using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using ZonyLrcTools.Common.MusicDecryption;

namespace ZonyLrcTools.Tests.Infrastructure.MusicDecryption
{
    /// <summary>
    /// 使用真实 NCM 样本验证解密。
    /// </summary>
    public class NcmMusicDecryptorTests : TestBase
    {
        /// <summary>
        /// 每次使用独立目录，避免历史输出影响解密结果。
        /// </summary>
        [Fact]
        public async Task ConvertMusicAsync_DecryptsNcmAsync()
        {
            var directory = Directory.CreateTempSubdirectory("zony-ncm-tests-").FullName;
            try
            {
                var decryptor = ServiceProvider.GetRequiredService<IMusicDecryptor>();
                var ncmFilePath = Path.Combine(directory, "Loren Gray - Queen.ncm");
                File.Copy(Path.Combine(AppContext.BaseDirectory, "MusicFiles", "Loren Gray - Queen.ncm"), ncmFilePath);

                var result = await decryptor.ConvertMusicAsync(ncmFilePath);
                result.IsSuccess.ShouldBeTrue(result.ErrorMessage);
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}