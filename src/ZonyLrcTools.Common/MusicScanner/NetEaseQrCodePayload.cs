namespace ZonyLrcTools.Common.MusicScanner;

/// <summary>
/// 网易云音乐扫码登录的二维码数据。
/// </summary>
/// <param name="Content">二维码指向的 URL，GUI 侧用它渲染二维码图片。</param>
/// <param name="UniKey">本次登录的唯一 Key，用于轮询登录状态。</param>
/// <param name="AsciiGraphic">ASCII 形态的二维码，CLI 在终端里直接打印它。</param>
public sealed record NetEaseQrCodePayload(string Content, string UniKey, string AsciiGraphic);

/// <summary>
/// 网易云音乐扫码登录的状态。
/// </summary>
public enum NetEaseLoginState
{
    /// <summary>二维码已生成，等待用户扫码。</summary>
    Waiting,

    /// <summary>用户已扫码，等待在手机上确认。</summary>
    ScanConfirmed,

    /// <summary>登录成功。</summary>
    Succeeded,

    /// <summary>二维码已过期，需要重新获取。</summary>
    Expired
}
