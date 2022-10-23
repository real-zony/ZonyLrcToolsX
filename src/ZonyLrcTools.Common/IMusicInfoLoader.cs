namespace ZonyLrcTools.Common;

public interface IMusicInfoLoader
{
    Task<List<MusicInfo?>> LoadAsync(string dirPath,
        int parallelCount = 2,
        CancellationToken cancellationToken = default);

    Task<List<MusicInfo?>> LoadAsync(IReadOnlyCollection<string> filePaths,
        int parallelCount = 2,
        CancellationToken cancellationToken = default);
}