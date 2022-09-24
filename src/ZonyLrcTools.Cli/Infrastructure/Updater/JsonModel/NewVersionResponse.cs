using System;
using System.Collections.Generic;

namespace ZonyLrcTools.Cli.Infrastructure.Updater.JsonModel;

public class NewVersionResponse
{
    public Version NewVersion { get; set; }

    public string NewVersionDescription { get; set; }

    public List<NewVersionItem> Items { get; set; }

    public DateTime UpdateTime { get; set; }
}