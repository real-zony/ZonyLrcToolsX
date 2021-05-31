using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;
using ZonyLrcTools.Cli.Infrastructure.MusicDecryption;

namespace ZonyLrcTools.Tests.Infrastructure.MusicDecryption
{
    public class NcmMusicDecryptorTests : TestBase
    {
        [Fact]
        public async Task ConvertMusic_Test()
        {
            var decryptor = ServiceProvider.GetRequiredService<IMusicDecryptor>();

            await using var fs = File.Open(Path.Combine(Directory.GetCurrentDirectory(), "MusicFiles", "Loren Gray - Queen.ncm"), FileMode.Open);
            using var reader = new BinaryReader(fs);
            var response = await decryptor.ConvertMusic(reader.ReadBytes((int) fs.Length));

            var musicFilePath = Path.Combine(Directory.GetCurrentDirectory(),
                "MusicFiles",
                $"Loren Gray - Queen.{((JObject) response.ExtensionObjects["JSON"]).SelectToken("$.format").Value<string>()}");

            if (File.Exists(musicFilePath))
            {
                File.Delete(musicFilePath);
            }

            await using var musicFileStream = File.Create(musicFilePath);
            await musicFileStream.WriteAsync(response.Data);
            await musicFileStream.FlushAsync();
        }
    }
}