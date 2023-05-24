using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Cli.Infrastructure.Tag
{
    /// <inheritdoc cref="ZonyLrcTools.Cli.Infrastructure.Tag.IBlockWordDictionary" />
    public class BlockWordDictionary : IBlockWordDictionary, ISingletonDependency
    {
        private readonly GlobalOptions _options;

        private readonly Lazy<Dictionary<string, string>> _wordsDictionary;

        public BlockWordDictionary(IOptions<GlobalOptions> options)
        {
            _options = options.Value;

            _wordsDictionary = new Lazy<Dictionary<string, string>>(() =>
            {
                var jsonData = File.ReadAllText(_options.Provider.Tag.BlockWord.FilePath);
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData) ?? throw new InvalidOperationException("屏蔽词字典文件格式错误。");
            });
        }

        public string? GetValue(string key)
        {
            if (_wordsDictionary.Value.TryGetValue(key, out var value))
            {
                return value;
            }

            return null;
        }
    }
}