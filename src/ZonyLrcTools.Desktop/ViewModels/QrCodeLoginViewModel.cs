using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using QRCoder;
using ZonyLrcTools.Common.MusicScanner;
using ZonyLrcTools.Desktop.Infrastructure.Localization;

namespace ZonyLrcTools.Desktop.ViewModels;

/// <summary>
/// Drives the NetEase QR-code login dialog.
/// </summary>
/// <remarks>
/// Two events cross the dialog boundary in opposite directions: <see cref="CloseRequested"/>
/// lets the login flow close the window once it is done, and <see cref="Cancelled"/> fires when
/// the user closed the window first, so the caller can stop polling instead of waiting forever.
/// </remarks>
public partial class QrCodeLoginViewModel : ObservableObject
{
    private readonly IUILocalizationService? _localization;

    [ObservableProperty]
    private Bitmap? _qrCodeImage;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private bool _hasImage;

    /// <summary>
    /// Raised when the login flow wants the dialog closed.
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Raised when the user closed the dialog before logging in.
    /// </summary>
    public event EventHandler? Cancelled;

    /// <summary>
    /// True once the login succeeded; the dialog stays closable but no longer counts as a cancel.
    /// </summary>
    public bool IsCompleted { get; private set; }

    public string LoginTitle => _localization?["NetEase_LoginTitle"] ?? "Sign in to NetEase Cloud Music";
    public string LoginTip => _localization?["NetEase_LoginTip"] ?? "Scan the QR code with the NetEase Cloud Music app on your phone.";
    public string CancelText => _localization?["Common_Cancel"] ?? "Cancel";

    public QrCodeLoginViewModel(IUILocalizationService? localization = null)
    {
        _localization = localization;
        _statusText = WaitText;
    }

    private string WaitText => _localization?["NetEase_LoginWaiting"] ?? "Waiting for you to scan...";
    private string ScannedText => _localization?["NetEase_LoginScanned"] ?? "Scanned, please confirm on your phone.";
    private string SuccessText => _localization?["NetEase_LoginSucceeded"] ?? "Signed in.";

    /// <summary>
    /// Render the QR payload the scanner produced.
    /// </summary>
    public void SetPayload(NetEaseQrCodePayload payload)
    {
        QrCodeImage = RenderQrCode(payload.Content);
        HasImage = QrCodeImage != null;
        StatusText = WaitText;
    }

    /// <summary>
    /// Reflect a login-state change reported by the scanner.
    /// </summary>
    public void SetState(NetEaseLoginState state)
    {
        StatusText = state switch
        {
            NetEaseLoginState.ScanConfirmed => ScannedText,
            NetEaseLoginState.Succeeded => SuccessText,
            _ => WaitText
        };
    }

    /// <summary>
    /// Push an error message into the dialog.
    /// </summary>
    public void SetError(string message)
    {
        StatusText = message;
    }

    /// <summary>
    /// Mark the flow as finished and close the dialog.
    /// </summary>
    public void Complete()
    {
        IsCompleted = true;
        RequestClose();
    }

    /// <summary>
    /// Ask the dialog to close.
    /// </summary>
    public void RequestClose()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called by the dialog when its window closed, for whatever reason.
    /// </summary>
    public void NotifyClosed()
    {
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
            var pngBytes = new PngByteQRCode(data).GetGraphic(12);

            return new Bitmap(new MemoryStream(pngBytes));
        }
        catch (Exception)
        {
            // A render failure shows up as the placeholder, not as a crash.
            return null;
        }
    }
}
