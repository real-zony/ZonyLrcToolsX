using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using ZonyLrcTools.Common.MusicDecryption;

namespace ZonyLrcTools.Tests.Infrastructure.MusicDecryption
{
    public class NcmMusicDecryptorTests : TestBase
    {
        [Fact]
        public async Task ConvertMusicAsync_Test()
        {
            var decryptor = ServiceProvider.GetRequiredService<IMusicDecryptor>();
            var ncmFilePath = Path.Combine(Directory.GetCurrentDirectory(), "MusicFiles", "Loren Gray - Queen.ncm");

            var result = await decryptor.ConvertMusicAsync(ncmFilePath);
            result.IsSuccess.ShouldBeTrue();
        }
    }
}