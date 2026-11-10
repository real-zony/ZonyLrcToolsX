using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;
using YamlDotNet.RepresentationModel;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Desktop.Infrastructure.Configuration;
using ZonyLrcTools.Desktop.ViewModels;

namespace ZonyLrcTools.Tests.Infrastructure;

/// <summary>设置文件无损修改的回归测试。</summary>
public sealed class DesktopSettingsPersistenceTests
{
    /// <summary>会话与数据库路径使用生成的 JSON 元数据往返，敏感会话文件限制权限。</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RoundTripsStateUsingGeneratedMetadataAsync(bool isSession)
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "state.json");
        try
        {
            var assembly = typeof(NetEaseSession).Assembly;
            var contextType = assembly.GetType("ZonyLrcTools.Desktop.Infrastructure.Configuration.DesktopStateJsonContext")!;
            var context = (JsonSerializerContext)contextType.GetProperty("Default")!.GetValue(null)!;
            var value = isSession
                ? (object)new NetEaseSession { Cookie = "test-cookie", CsrfToken = "test-token" }
                : new KugouDatabaseSetting { DatabasePath = "/music/My Database.db" };
            var typeInfo = context.GetTypeInfo(value.GetType())!;
            var writer = assembly.GetType("ZonyLrcTools.Desktop.Infrastructure.Configuration.DesktopStateFile")!;
            var method = writer.GetMethod("WriteAsync", BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(value.GetType());
            await (Task)method.Invoke(null, new object[]
            {
                path, value, typeInfo, isSession, CancellationToken.None
            })!;

            var loaded = JsonSerializer.Deserialize(await File.ReadAllTextAsync(path), typeInfo);
            if (isSession)
            {
                var session = loaded.ShouldBeOfType<NetEaseSession>();
                session.Cookie.ShouldBe("test-cookie");
                session.CsrfToken.ShouldBe("test-token");
            }
            else
            {
                loaded.ShouldBeOfType<KugouDatabaseSetting>().DatabasePath.ShouldBe("/music/My Database.db");
            }
            if (isSession && !OperatingSystem.IsWindows())
            {
                File.GetUnixFileMode(path).ShouldBe(UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
            Directory.GetFiles(directory, "*.tmp").ShouldBeEmpty();
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    /// <summary>更改其他设置不能覆盖手工配置的换行符。</summary>
    [Fact]
    public void KeepsCustomLineBreakWhenChangingProxy()
    {
        var options = new GlobalOptions
        {
            Provider = new ProviderOptions
            {
                Lyric = new LyricsOptions
                {
                    Config = new GlobalLyricsConfigOptions { LineBreak = string.Empty, FileEncoding = "ascii" },
                    Plugin = Array.Empty<LyricsProviderOptions>()
                }
            }
        };
        var viewModel = new SettingsViewModel(new MemorySettingsStore(options));
        viewModel.IsProxyEnabled = true;
        options.Provider.Lyric.Config.LineBreak.ShouldBe(string.Empty);
        viewModel.AvailableEncodings.ShouldContain("ascii");
    }

    /// <summary>恢复默认值同时恢复下载器顺序。</summary>
    [Fact]
    public async Task ResetRestoresProviderOrderAsync()
    {
        var options = new GlobalOptions
        {
            Provider = new ProviderOptions
            {
                Lyric = new LyricsOptions
                {
                    Config = new GlobalLyricsConfigOptions(),
                    Plugin = new[]
                    {
                        new LyricsProviderOptions { Name = "QQ", Priority = 1 },
                        new LyricsProviderOptions { Name = "NetEase", Priority = -1 }
                    }
                }
            }
        };
        var viewModel = new SettingsViewModel(new MemorySettingsStore(options));
        await viewModel.ResetToDefaultsCommand.ExecuteAsync(null);
        viewModel.LyricsProviders[0].Name.ShouldBe("NetEase");
        viewModel.LyricsProviders[0].Priority.ShouldBe(1);
        viewModel.LyricsProviders[0].IsEnabled.ShouldBeTrue();
        viewModel.ProxyPort.ShouldBe(4780);
    }

    private sealed class MemorySettingsStore(GlobalOptions options) : ISettingsStore
    {
        public GlobalOptions Options { get; } = options;
        public string ConfigFilePath => "unused.yaml";
        public Task<SettingsSaveResult> SaveAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(SettingsSaveResult.Success(ConfigFilePath));
    }

    /// <summary>合法的 YAML 写法不应导致未知字段和注释丢失。</summary>
    [Theory]
    [InlineData("# keep\nnetworkOptions: {isEnable: false, ip: localhost, port: 80}\ncustom: keep\n", "networkOptions.port", "8080")]
    [InlineData("# keep\nprovider:\n  tag:\n    blockWord:\n      filePath: My Words.json # dictionary\ncustom: keep\n", "provider.tag.blockWord.filePath", "New Words.json")]
    [InlineData("# keep\nnetworkOptions:\n  isEnable: false\n  ip: localhost\ncustom: keep\n", "networkOptions.port", "8080")]
    [InlineData("# keep\nnetworkOptions: {isEnable: false, ip: localhost}\ncustom: keep\n", "networkOptions.port", "8080")]
    [InlineData("# keep\nnetworkOptions: {}\ncustom: keep\n", "networkOptions.port", "8080")]
    [InlineData("# keep\nnetworkOptions: {ip: localhost,}\ncustom: keep\n", "networkOptions.port", "8080")]
    public async Task PatchesValidYamlWithoutDiscardingUnmanagedValuesAsync(string yaml, string key, string value)
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, yaml);
            using var store = new SettingsStore(Options.Create(new GlobalOptions()));
            var method = typeof(SettingsStore).GetMethod("TryPatchInPlaceAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var result = await (Task<string?>)method.Invoke(store, new object[]
            {
                path, new Dictionary<string, string> { [key] = value }, CancellationToken.None
            })!;

            result.ShouldNotBeNull();
            result.ShouldContain("# keep");
            var stream = new YamlStream();
            stream.Load(new StringReader(result!));
            var mapping = (YamlMappingNode)stream.Documents[0].RootNode;
            ((YamlScalarNode)mapping.Children[new YamlScalarNode("custom")]).Value.ShouldBe("keep");
            var node = (YamlNode)mapping;
            foreach (var part in key.Split('.'))
            {
                node = ((YamlMappingNode)node).Children[new YamlScalarNode(part)];
            }
            ((YamlScalarNode)node).Value.ShouldBe(value);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>无效文件必须保留，不能用内存配置覆盖。</summary>
    [Fact]
    public async Task RefusesMalformedYamlAsync()
    {
        var path = Path.GetTempFileName();
        try
        {
            const string original = "networkOptions: [broken\n# keep";
            await File.WriteAllTextAsync(path, original);
            using var store = new SettingsStore(Options.Create(new GlobalOptions()));
            var method = typeof(SettingsStore).GetMethod("TryPatchInPlaceAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
            await Should.ThrowAsync<Exception>(async () => await (Task<string?>)method.Invoke(store, new object[]
            {
                path, new Dictionary<string, string> { ["networkOptions.port"] = "8080" }, CancellationToken.None
            })!);
            (await File.ReadAllTextAsync(path)).ShouldBe(original);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
