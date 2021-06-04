using Shouldly;
using Xunit;
using ZonyLrcTools.Cli.Infrastructure.Tag;

namespace ZonyLrcTools.Tests.Infrastructure.Tag
{
    public class BlockWordDictionaryTests : TestBase
    {
        [Fact]
        public void GetValue_Test()
        {
            var dictionary = GetService<IBlockWordDictionary>();
            var result = dictionary.GetValue("fuckking");
            
            result.ShouldBe("***kking");
        }
    }
}