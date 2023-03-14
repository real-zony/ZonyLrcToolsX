using ZonyLrcTools.Common.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Common.MusicScanner;

/// <summary>
/// 基于 CSV 文件的音乐信息扫描器。
/// </summary>
public class CsvFileMusicScanner : ITransientDependency
{
    /// <summary>
    /// 从 Csv 文件中获取需要下载的歌曲信息。
    /// </summary>
    /// <param name="csvFilePath">CSV 文件的路径。</param>
    /// <param name="outputDirectory">歌词文件的输出目录。</param>
    /// <param name="pattern">输出的歌词文件格式，默认是 "{Artist} - {Title}.lrc" 的形式。</param>
    public async Task<List<MusicInfo>> GetMusicInfoFromCsvFileAsync(string csvFilePath, string outputDirectory, string pattern)
    {
        var csvFileContent = await File.ReadAllTextAsync(csvFilePath);
        var csvFileLines = csvFileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        return csvFileLines.Skip(1).Select(line => GetMusicInfoFromCsvFileLine(line, outputDirectory, pattern)).ToList();
    }

    private MusicInfo GetMusicInfoFromCsvFileLine(string csvFileLine, string outputDirectory, string pattern)
    {
        var csvFileLineItems = csvFileLine.Split(',');
        var name = csvFileLineItems[0];
        var artist = csvFileLineItems[1];
        var fakeFilePath = Path.Combine(outputDirectory, pattern.Replace("{Name}", name).Replace("{Artist}", artist));
        var musicInfo = new MusicInfo(fakeFilePath, name, artist);
        return musicInfo;
    }
}