namespace ZonyLrcTools.Common.MusicDecryption
{
    /// <summary>
    /// 酷狗数据库的探测结果。
    /// </summary>
    /// <param name="IsAvailable">数据库是否可用（文件存在且能解出密钥）。</param>
    /// <param name="DatabasePath">实际生效的数据库路径；不可用时为 null。</param>
    /// <param name="KeyCount">能从数据库里读出的密钥条目数量。</param>
    /// <param name="Message">给用户看的说明文字。</param>
    public sealed record KugouDatabaseInfo(bool IsAvailable, string? DatabasePath, int KeyCount, string Message);

    /// <summary>
    /// .kgg 解密前置条件的查询入口。
    /// <remarks>
    /// .kgg 的音频用 QMC2 加密，而每个文件自己的 ekey 不在文件里，
    /// 需要从酷狗客户端本地的 KGMusicV3.db 里按音频哈希查出来。
    /// GUI 需要提前知道“库在不在、这个文件有没有密钥”，才能在用户点开始之前给出提示。
    /// </remarks>
    /// </summary>
    public interface IKugouKeyProvider
    {
        /// <summary>
        /// 当前生效的数据库路径；没有可用数据库时为 null。
        /// </summary>
        string? DatabasePath { get; }

        /// <summary>
        /// 手动指定数据库路径；<c>null</c> 或空字符串表示恢复自动探测。
        /// </summary>
        void SetDatabasePath(string? path);

        /// <summary>
        /// 探测当前可用的数据库。
        /// <remarks>
        /// 实现应当缓存结果：整库解密需要几十毫秒，而查一次密钥只要零点几毫秒。
        /// </remarks>
        /// </summary>
        KugouDatabaseInfo Probe();

        /// <summary>
        /// 作废缓存，让下一次访问重新读取数据库。
        /// <remarks>
        /// 用户在酷狗里新下载了歌曲、或换了一份数据库文件之后需要调用，否则读到的还是旧快照。
        /// </remarks>
        /// </summary>
        void Invalidate();

        /// <summary>
        /// 按魔数和加密版本判断文件是否为 KGG，不看扩展名。
        /// </summary>
        bool IsKggFile(string filePath);

        /// <summary>
        /// 读出 .kgg 文件头里的音频哈希；不是合法的 .kgg 时返回 null。
        /// </summary>
        string? ReadAudioHash(string filePath);

        /// <summary>
        /// 取出该音频哈希对应的 ekey；没有时返回 null。
        /// </summary>
        string? FindKey(string audioHash);
    }
}
