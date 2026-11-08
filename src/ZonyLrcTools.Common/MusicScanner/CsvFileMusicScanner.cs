using System.Text;
using Ude;
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
    public async Task<List<MusicInfo>> GetMusicInfoFromCsvFileAsync(string csvFilePath, string outputDirectory,
        string pattern)
    {
        var encoding = DetectFileEncoding(csvFilePath);

        var csvFileContent = await File.ReadAllTextAsync(csvFilePath, encoding);

        // 不能只按 Environment.NewLine 切分：在 Windows 上读到的是 "\r\n"，而用户从网盘、
        // 版本的表格导出、Git 里签出的 CSV 往往是 "\n"，那样整个文件会被当成一行，
        // 跳过表头之后就是空的，界面上表现为"解析了但一首歌都没有"。
        var csvFileLines = csvFileContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        return csvFileLines.Skip(1)
            .Where(line => line.Contains(',')) // 跳过空行与 VCS 合并产生的杂行。
            .Select(line => GetMusicInfoFromCsvFileLine(line, outputDirectory, pattern))
            .ToList();
    }

    private MusicInfo GetMusicInfoFromCsvFileLine(string csvFileLine, string outputDirectory, string pattern)
    {
        var csvFileLineItems = csvFileLine.Split(',');
        var name = csvFileLineItems[0].Trim();
        var artist = csvFileLineItems.Length > 1 ? csvFileLineItems[1].Trim() : string.Empty;
        var fakeFilePath = Path.Combine(outputDirectory, pattern.Replace("{Name}", name).Replace("{Artist}", artist));
        var musicInfo = new MusicInfo(fakeFilePath, name, artist);
        return musicInfo;
    }

    private Encoding DetectFileEncoding(string filePath)
    {
        using var fileStream = File.OpenRead(filePath);
        var detector = new CharsetDetector();
        detector.Feed(fileStream);
        detector.DataEnd();

        if (detector.Charset != null)
        {
            return Encoding.GetEncoding(detector.Charset);
        }

        return Encoding.Default;
    }
}