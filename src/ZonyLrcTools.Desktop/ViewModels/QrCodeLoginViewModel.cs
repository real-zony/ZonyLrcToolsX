using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using QRCoder;
using ZonyLrcTools.Common.MusicScanner;
using ZonyLrcTools.Desktop.Infrastructure.Localization;

namespace ZonyLrcTools.Desktop.ViewModels;

/// <summary>
/// 网易云扫码登录对话框。
/// </summary>
public partial class QrCodeLoginViewModel : ObservableObject
{
    private readonly IUILocalizationService? _localization;
    private bool _isClosed;

    [ObservableProperty]
    private Bitmap? _qrCodeImage;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private bool _hasImage;

    /// <summary>
    /// 请求关闭登录对话框。
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// 登录完成前关闭对话框时触发。
    /// </summary>
    public event EventHandler? Cancelled;

    /// <summary>
    /// 是否已登录成功。
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>登录窗口标题。</summary>
    public string LoginTitle => _localization?["NetEase_LoginTitle"] ?? "Sign in to NetEase Cloud Music";
    /// <summary>扫码登录提示。</summary>
    public string LoginTip => _localization?["NetEase_LoginTip"] ?? "Scan the QR code with the NetEase Cloud Music app on your phone.";
    /// <summary>取消按钮文案。</summary>
    public string CancelText => _localization?["Common_Cancel"] ?? "Cancel";

    /// <summary>
    /// 创建扫码登录对话框状态。
    /// </summary>
    public QrCodeLoginViewModel(IUILocalizationService? localization = null)
    {
        _localization = localization;
        _statusText = WaitText;
    }

    private string WaitText => _localization?["NetEase_LoginWaiting"] ?? "Waiting for you to scan...";
    private string ScannedText => _localization?["NetEase_LoginScanned"] ?? "Scanned, please confirm on your phone.";
    private string SuccessText => _localization?["NetEase_LoginSucceeded"] ?? "Signed in.";

    /// <summary>
    /// 显示扫描器提供的二维码。
    /// </summary>
    public void SetPayload(NetEaseQrCodePayload payload)
    {
        if (_isClosed) return;

        var previousImage = QrCodeImage;
        QrCodeImage = RenderQrCode(payload.Content);
        HasImage = QrCodeImage != null;
        previousImage?.Dispose();
        StatusText = HasImage
            ? WaitText
            : _localization?["NetEase_LoginFailed"] ?? "Login did not complete. Close this dialog and try again.";
    }

    /// <summary>
    /// 更新扫码登录状态。
    /// </summary>
    public void SetState(NetEaseLoginState state)
    {
        if (_isClosed) return;

        StatusText = state switch
        {
            NetEaseLoginState.ScanConfirmed => ScannedText,
            NetEaseLoginState.Succeeded => SuccessText,
            NetEaseLoginState.Expired => _localization?["NetEase_LoginExpired"] ?? "The QR code expired. Close this dialog and try again.",
            _ => WaitText
        };
    }

    /// <summary>
    /// 显示登录错误。
    /// </summary>
    public void SetError(string message)
    {
        if (_isClosed) return;

        StatusText = message;
    }

    /// <summary>
    /// 标记登录完成并关闭对话框。
    /// </summary>
    public void Complete()
    {
        if (_isClosed) return;

        IsCompleted = true;
        RequestClose();
    }

    /// <summary>
    /// 请求关闭对话框。
    /// </summary>
    public void RequestClose()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 窗口关闭后停止登录并释放二维码。
    /// </summary>
    public void NotifyClosed()
    {
        if (_isClosed) return;

        _isClosed = true;
        var image = QrCodeImage;
        QrCodeImage = null;
        HasImage = false;
        image?.Dispose();

        if (!IsCompleted)
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }

    private static Bitmap? RenderQrCode(string content)
    {
        try
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new PngByteQRCode(data);
            using var stream = new MemoryStream(qrCode.GetGraphic(12));
            return new Bitmap(stream);
        }
        catch (Exception)
        {
            // 二维码无法生成时，由界面显示登录失败提示。
            return null;
        }
    }
}
