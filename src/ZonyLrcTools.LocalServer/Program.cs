using System.Diagnostics;
using System.Reflection;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.LocalServer.EventBus;

#region Main Flow

var app = RegisterAndConfigureServices();
await ListenServices();

#endregion

#region Configure Services

async Task ListenServices()
{
    await new SuperSocketListener().ListenAsync();
    await app?.RunAsync()!;
}

WebApplication? RegisterAndConfigureServices()
{
    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.ConfigureKestrel(k => k.ListenAnyIP(50002));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.BeginAutoDependencyInject<Program>();

    var insideApp = builder.Build();
    insideApp.UseSpaStaticFiles(new StaticFileOptions
    {
        RequestPath = "",
        FileProvider = new Microsoft.Extensions.FileProviders
            .ManifestEmbeddedFileProvider(
                Assembly.GetExecutingAssembly(), "UiStaticResources"
            )
    });

    insideApp.MapControllers();
#if !DEBUG
        insideApp.Lifetime.ApplicationStarted.Register(OpenBrowser);
#endif

    return insideApp;
}

void OpenBrowser()
{
    const string url = "http://localhost:50002/index.html";

    if (OperatingSystem.IsWindows())
    {
        Process.Start("explorer.exe", url);
    }
    else if (OperatingSystem.IsMacOS())
    {
        Process.Start("open", url);
    }
    else if (OperatingSystem.IsLinux())
    {
        Process.Start("xdg-open", url);
    }
}

#endregion