using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ZonyLrcTools.Cli.Config;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Cli.Infrastructure.Tag
{
    /// <inheritdoc cref="ZonyLrcTools.Cli.Infrastructure.Tag.IBlockWordDictionary" />
    public class BlockWordDictionary : IBlockWordDictionary, ISingletonDependency
    {
        private readonly ToolOptions _options;

        private readonly Lazy<Dictionary<string, string>> _wordsDictionary;

        public BlockWordDictionary(IOptions<ToolOptions> options)
        {
            _options = options.Value;

            _wordsDictionary = new Lazy<Dictionary<string, string>>(() =>
            {
                var jsonData = File.ReadAllText(_options.BlockWordOptions.BlockWordDictionaryFile);
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);
            });
        }

        public string GetValue(string key)
        {
            if (_wordsDictionary.Value.TryGetValue(key, out var value))
            {
                return value;
            }

            return null;
        }
    }
}