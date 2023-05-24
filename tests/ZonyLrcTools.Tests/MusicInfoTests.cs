using Shouldly;
using Xunit;
using ZonyLrcTools.Common;

namespace ZonyLrcTools.Tests;

public class MusicInfoTests
{
    [Fact]
    public void InvalidFilePathTest()
    {
        var musicInfo = new MusicInfo("C:\\Users\\Zony\\Music\\[ZonyLrcTools]:? - 01. 你好.mp3", "你好", "Zony");
        musicInfo.FilePath.ShouldBe(@"C:\Users\Zony\Music\[ZonyLrcTools] - 01. 你好.mp3");
    }
}