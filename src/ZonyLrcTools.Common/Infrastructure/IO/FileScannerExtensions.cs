namespace ZonyLrcTools.Common.Infrastructure.IO;

public static class FileScannerExtensions
{
    public static async Task<IEnumerable<string>> ScanMusicFilesAsync(this IFileScanner fileScanner,
        string dirPath,
        IEnumerable<string> extensions)
    {
        var files = (await fileScanner.ScanAsync(dirPath, extensions))
            .SelectMany(t => t.FilePaths)
            .ToList();

        return files;
    }
}