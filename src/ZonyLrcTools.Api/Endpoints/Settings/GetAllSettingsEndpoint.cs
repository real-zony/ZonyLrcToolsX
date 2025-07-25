using FastEndpoints;
using ZonyLrcTools.Api.Domain.Settings;
using ZonyLrcTools.Api.Endpoints.Settings.DTOs;

namespace ZonyLrcTools.Api.Endpoints.Settings;

public class GetAllSettingsEndpoint(IFreeSql freeSql) : EndpointWithoutRequest<GetAllSettingsResponse>
{
    public override void Configure()
    {
        Get("/api/settings");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var setting = await freeSql.Select<Setting>()
            .ToListAsync(ct);

        if (setting.Count == 0)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(new GetAllSettingsResponse
        {
            IsAutoCheckUpdate = true
        }, ct);
    }
}