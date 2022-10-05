using System.Diagnostics;
using ZonyLrcTools.LocalServer;

var app = RegisterAndConfigureServices();
await ListenServices();

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

    var insideApp = builder.Build();
    insideApp.UseSpaStaticFiles(new StaticFileOptions
    {
        RequestPath = "",
        FileProvider = new Microsoft.Extensions.FileProviders
            .ManifestEmbeddedFileProvider(
                typeof(Program).Assembly, "UiStaticResources"
            )
    });

    insideApp.UseAuthorization();
    insideApp.MapControllers();
    insideApp.Lifetime.ApplicationStarted.Register(OpenBrowser);

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