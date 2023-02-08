using ZonyLrcTools.Common.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Common.MusicScanner;

public class CsvFileMusicScanner : ITransientDependency
{
    public async Task<List<MusicInfo>> GetMusicInfoFromCsvFileAsync(string csvFilePath, ManualDownloadOptions options)
    {
        var csvFileContent = await File.ReadAllTextAsync(csvFilePath);
        var csvFileLines = csvFileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        return csvFileLines.Skip(1).Select(line => GetMusicInfoFromCsvFileLine(line, options)).ToList();
    }

    private MusicInfo GetMusicInfoFromCsvFileLine(string csvFileLine, ManualDownloadOptions options)
    {
        var csvFileLineItems = csvFileLine.Split(',');
        var name = csvFileLineItems[0];
        var artist = csvFileLineItems[1];
        var fakeFilePath = Path.Combine(options.OutputDirectory, options.OutputFileNamePattern.Replace("{Name}", name).Replace("{Artist}", artist));
        var musicInfo = new MusicInfo(fakeFilePath, name, artist);
        return musicInfo;
    }
}