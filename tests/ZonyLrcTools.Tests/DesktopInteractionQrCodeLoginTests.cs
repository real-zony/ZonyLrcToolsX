using Xunit;
using ZonyLrcTools.Common.MusicScanner;
using ZonyLrcTools.Desktop.ViewModels;

namespace ZonyLrcTools.Tests;

/// <summary>
/// 扫码登录对话框的状态与关闭行为测试。
/// </summary>
public class DesktopInteractionQrCodeLoginTests
{
    /// <summary>
    /// 重复收到窗口关闭通知时只取消一次登录。
    /// </summary>
    [Fact]
    public void NotifyClosed_CancelsLoginOnlyOnce()
    {
        var viewModel = new QrCodeLoginViewModel();
        var cancellationCount = 0;
        viewModel.Cancelled += (_, _) => cancellationCount++;

        viewModel.NotifyClosed();
        viewModel.NotifyClosed();

        Assert.Equal(1, cancellationCount);
        Assert.False(viewModel.IsCompleted);
    }

    /// <summary>
    /// 登录成功导致的窗口关闭不应触发取消。
    /// </summary>
    [Fact]
    public void Complete_ClosesDialogWithoutCancellingLogin()
    {
        var viewModel = new QrCodeLoginViewModel();
        var cancellationCount = 0;
        var closeRequestCount = 0;
        viewModel.Cancelled += (_, _) => cancellationCount++;
        viewModel.CloseRequested += (_, _) =>
        {
            closeRequestCount++;
            viewModel.NotifyClosed();
        };

        viewModel.Complete();

        Assert.True(viewModel.IsCompleted);
        Assert.Equal(1, closeRequestCount);
        Assert.Equal(0, cancellationCount);
    }

    /// <summary>
    /// 已关闭的窗口不再接受迟到的登录结果。
    /// </summary>
    [Fact]
    public void NotifyClosed_IgnoresLateLoginCallbacks()
    {
        var viewModel = new QrCodeLoginViewModel();
        var statusBeforeClosing = viewModel.StatusText;
        var closeRequestCount = 0;
        viewModel.CloseRequested += (_, _) => closeRequestCount++;
        viewModel.NotifyClosed();

        viewModel.SetState(NetEaseLoginState.Succeeded);
        viewModel.SetError("迟到的网络错误");
        viewModel.Complete();

        Assert.False(viewModel.IsCompleted);
        Assert.Equal(statusBeforeClosing, viewModel.StatusText);
        Assert.Equal(0, closeRequestCount);
    }

    /// <summary>
    /// 二维码过期后不应继续显示等待扫码。
    /// </summary>
    [Fact]
    public void SetState_ExpiredStopsShowingWaitingStatus()
    {
        var viewModel = new QrCodeLoginViewModel();
        viewModel.SetState(NetEaseLoginState.Waiting);
        var waitingStatus = viewModel.StatusText;

        viewModel.SetState(NetEaseLoginState.Expired);

        Assert.NotEqual(waitingStatus, viewModel.StatusText);
        Assert.False(viewModel.IsCompleted);
    }
}
