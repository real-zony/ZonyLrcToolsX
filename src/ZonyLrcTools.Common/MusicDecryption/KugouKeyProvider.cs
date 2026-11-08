using System.Buffers.Binary;
using System.Text;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Common.MusicDecryption
{
    /// <inheritdoc cref="ZonyLrcTools.Common.MusicDecryption.IKugouKeyProvider" />
    public class KugouKeyProvider : IKugouKeyProvider, ISingletonDependency
    {
        private const int HeaderSize = Kugou.KugouEkeyDatabase.KggHeaderSize;
        private const int AudioHashOffset = 0x48;
        private const int AudioHashLength = 0x20;

        private readonly object _syncRoot = new();
        private readonly Dictionary<string, string> _keys = new(StringComparer.OrdinalIgnoreCase);

        private string? _manualPath;
        private string? _databasePath;
        private string? _loadedPath;
        private string? _failureReason;
        private bool _isLoaded;

        public string? DatabasePath
        {
            get
            {
                lock (_syncRoot)
                {
                    EnsureLoaded();
                    return _databasePath;
                }
            }
        }

        public void SetDatabasePath(string? path)
        {
            lock (_syncRoot)
            {
                var normalized = string.IsNullOrWhiteSpace(path) ? null : path;
                if (string.Equals(normalized, _manualPath, StringComparison.OrdinalIgnoreCase))
                {
                    // 路径没变就不作废缓存，避免无意义地重新解密 16 MB 数据库。
                    return;
                }

                _manualPath = normalized;
                Invalidate();
            }
        }

        /// <summary>
        /// 探测当前可用的酷狗数据库。
        /// <remarks>
        /// 结果会被缓存：整库解密需要几十毫秒，而真正查一次密钥只要 0.1 毫秒。
        /// 只有在 <see cref="SetDatabasePath"/> 改变了路径、或显式调用 <see cref="Invalidate"/> 时才会重新读取。
        /// </remarks>
        /// </summary>
        public KugouDatabaseInfo Probe()
        {
            lock (_syncRoot)
            {
                EnsureLoaded();

                if (_databasePath == null)
                {
                    var message = _manualPath != null
                        ? $"指定的酷狗数据库不可用：{_failureReason ?? _manualPath}"
                        : Kugou.KugouEkeyDatabase.BuildMissingDatabaseMessage();

                    return new KugouDatabaseInfo(false, null, 0, message);
                }

                return _keys.Count == 0
                    ? new KugouDatabaseInfo(false, _databasePath, 0,
                        $"已找到酷狗数据库，但里面没有任何解密密钥：{_databasePath}")
                    : new KugouDatabaseInfo(true, _databasePath, _keys.Count,
                        $"已读取 {_keys.Count} 个解密密钥：{_databasePath}");
            }
        }

        /// <summary>
        /// 让下一次访问重新读取数据库（例如用户在酷狗里新下载了歌曲）。
        /// </summary>
        public void Invalidate()
        {
            lock (_syncRoot)
            {
                _isLoaded = false;
                _keys.Clear();
                _databasePath = null;
                _loadedPath = null;
                _failureReason = null;
            }
        }

        public bool IsKggFile(string filePath) => ReadAudioHash(filePath) != null;

        public string? ReadAudioHash(string filePath)
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                var header = new byte[HeaderSize];
                if (stream.ReadAtLeast(header, HeaderSize, throwOnEndOfStream: false) != HeaderSize)
                {
                    return null;
                }

                if (!header.AsSpan(0, 16).SequenceEqual(Kugou.KugouEkeyDatabase.KggMagic))
                {
                    return null;
                }

                var hashLength = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(0x44));
                if (hashLength == 0 || hashLength > AudioHashLength)
                {
                    return null;
                }

                return Encoding.ASCII.GetString(header, AudioHashOffset, (int)hashLength);
            }
            catch
            {
                return null;
            }
        }

        public string? FindKey(string audioHash)
        {
            if (string.IsNullOrEmpty(audioHash))
            {
                return null;
            }

            lock (_syncRoot)
            {
                EnsureLoaded();
                return _keys.TryGetValue(audioHash, out var ekey) && !string.IsNullOrEmpty(ekey) ? ekey : null;
            }
        }

        private void EnsureLoaded()
        {
            if (_isLoaded)
            {
                return;
            }

            _isLoaded = true;
            _keys.Clear();
            _databasePath = null;
            _loadedPath = null;
            _failureReason = null;

            var path = _manualPath;
            if (path == null)
            {
                path = Kugou.KugouEkeyDatabase.FindDatabasePath();
                if (path == null)
                {
                    return;
                }
            }
            else if (!File.Exists(path))
            {
                _failureReason = "文件不存在";
                return;
            }

            var keys = Kugou.KugouEkeyDatabase.ReadEncryptionKeys(path, out var failureReason);
            if (keys.Count == 0)
            {
                _failureReason = failureReason ?? "没有读到任何密钥";

                // 手动指定的文件读不出密钥，就不要把它当成可用数据库。
                if (_manualPath != null)
                {
                    return;
                }
            }

            _databasePath = path;
            _loadedPath = path;
            foreach (var pair in keys)
            {
                _keys[pair.Key] = pair.Value;
            }
        }
    }
}
