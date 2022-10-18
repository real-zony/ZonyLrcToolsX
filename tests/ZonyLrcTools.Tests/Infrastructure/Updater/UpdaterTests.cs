using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using ZonyLrcTools.Common.Updater;
using ZonyLrcTools.Common.Updater.JsonModel;

namespace ZonyLrcTools.Tests.Infrastructure.Updater;

public class UpdaterTests : TestBase
{
    private readonly IUpdater _updater;

    public UpdaterTests()
    {
        _updater = GetService<IUpdater>();
    }

    [Fact]
    public async Task CheckUpdateAsync()
    {
        var response = new NewVersionResponse
        {
            NewVersion = new Version(0, 0, 1, 49),
            NewVersionDescription = "这里是新版本描述",
            UpdateTime = DateTime.Now,
            Items = new List<NewVersionItem>
            {
                new NewVersionItem
                {
                    ItemType = NewVersionItemType.Important,
                    Url = "https://github.com/real-zony/ZonyLrcToolsX/releases/tag/ZonyLrcToolsX_Alpha.2022092449"
                }
            }
        };

        var responseString = JsonConvert.SerializeObject(response);
        responseString.ShouldNotBeNull();
        
        await _updater.CheckUpdateAsync();
    }
}