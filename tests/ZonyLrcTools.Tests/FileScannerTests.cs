using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using QRCoder;
using Shouldly;
using Xunit;
using ZonyLrcTools.Common.Infrastructure.IO;

namespace ZonyLrcTools.Tests
{
    public class FileScannerTests : TestBase
    {
        [Fact]
        public async Task ScanAsync_Test()
        {
            var tempMusicFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Temp.mp3");
            var fs = File.Create(tempMusicFilePath);
            fs.Close();

            var fileScanner = ServiceProvider.GetRequiredService<IFileScanner>();
            var result = await fileScanner.ScanAsync(
                Path.GetDirectoryName(tempMusicFilePath),
                new[] { "*.mp3", "*.flac" });

            result.Count.ShouldBe(2);
            result.First(e => e.ExtensionName == ".mp3").FilePaths.Count.ShouldNotBe(0);

            File.Delete(tempMusicFilePath);
        }

        [Fact]
        public void TestConsole()
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode("https://y.music.163.com/m/login?codekey=2f0da1d0-759e-478b-9153-35058b3", QRCodeGenerator.ECCLevel.L);
            AsciiQRCode qrCode = new AsciiQRCode(qrCodeData);
            string qrCodeAsAsciiArt = qrCode.GetGraphic(1, drawQuietZones: false);
        }
    }
}