using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using ZonyLrcTools.Api.Database;

namespace ZonyLrcTools.Api;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(k => k.ListenAnyIP(10086));

        builder.Services.AddFastEndpoints();
        builder.Services.RegisterDatabaseServices();

        var app = builder.Build();
        app.UseFastEndpoints();
        await app.RunAsync();
    }
}