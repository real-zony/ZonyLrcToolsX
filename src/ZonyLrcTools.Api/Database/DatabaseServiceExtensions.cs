using FreeSql;
using Microsoft.Extensions.DependencyInjection;

namespace ZonyLrcTools.Api.Database;

public static class DatabaseServiceExtensions
{
    public static IServiceCollection RegisterDatabaseServices(this IServiceCollection services)
    {
        var freeSqlBuilder = new FreeSqlBuilder()
            .UseConnectionString(DataType.Sqlite, "Data Source=ZonyLrcTools.db;Pooling=true;Max Pool Size=10;")
            .UseAutoSyncStructure(true);

        var freeSql = freeSqlBuilder.Build();
        services.AddSingleton(freeSql);

        return services;
    }
}