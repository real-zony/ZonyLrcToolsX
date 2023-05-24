using Shouldly;
using Xunit;
using ZonyLrcTools.Common.Infrastructure.Exceptions;

namespace ZonyLrcTools.Tests.Infrastructure.Exceptions
{
    public class ErrorCodeHelperTests : TestBase
    {
        [Fact]
        public void LoadMessage_Test()
        {
            ErrorCodeHelper.LoadErrorMessage();

            ErrorCodeHelper.ErrorMessages.ShouldNotBeNull();
            ErrorCodeHelper.ErrorMessages.Count.ShouldBe(17);
        }

        [Fact]
        public void GetMessage_Test()
        {
            ErrorCodeHelper.LoadErrorMessage();

            ErrorCodeHelper.GetMessage(ErrorCodes.DirectoryNotExist).ShouldBe("需要扫描的目录不存在，请确认路径是否正确。");
        }
    }
}