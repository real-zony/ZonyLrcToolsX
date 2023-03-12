using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace ZonyLrcTools.Common.Infrastructure.Encryption;

/// <summary>
/// 提供网易云音乐 API 的相关加密方法。
/// </summary>
/// <remarks>
/// 加密方法参考以下开源项目:
/// 1. https://github.com/jitwxs/163MusicLyrics/blob/master/MusicLyricApp/Api/Music/NetEaseMusicNativeApi.cs
/// 2. https://github.com/mos9527/pyncm/blob/ad0a84b2ed5f1affa9890d5f54f6170c2cf99bbb/pyncm/utils/crypto.py#L53
/// </remarks>
public static class NetEaseMusicEncryptionHelper
{
    public const string Modulus =
        "00e0b509f6259df8642dbc35662901477df22677ec152b5ff68ace615bb7b725152b3ab17a876aea8a5aa76d2e417629ec4ee341f56135fccf695280104e0312ecbda92557c93870114af6c9d05c4f7f0c3685b7a46bee255932575cce10b424d813cfe4875d3e82047b97ddef52741d546b8e289dc6935b3ece0462db0a22b8e7";

    public const string Nonce = "0CoJUm6Qyw8W8jud";
    public const string PubKey = "010001";
    public const string Vi = "0102030405060708";
    public static readonly byte[] ID_XOR_KEY_1 = "3go8&$8*3*3h0k(2)2"u8.ToArray();

    public static string RsaEncode(string text)
    {
        var srText = new string(text.Reverse().ToArray());
        var a = BCHexDec(BitConverter.ToString(Encoding.Default.GetBytes(srText)).Replace("-", string.Empty));
        var b = BCHexDec(PubKey);
        var c = BCHexDec(Modulus);
        var key = BigInteger.ModPow(a, b, c).ToString("x");
        key = key.PadLeft(256, '0');

        return key.Length > 256 ? key.Substring(key.Length - 256, 256) : key;
    }

    public static BigInteger BCHexDec(string hex)
    {
        var dec = new BigInteger(0);
        var len = hex.Length;

        for (var i = 0; i < len; i++)
        {
            dec += BigInteger.Multiply(new BigInteger(Convert.ToInt32(hex[i].ToString(), 16)),
                BigInteger.Pow(new BigInteger(16), len - i - 1));
        }

        return dec;
    }

    public static string AesEncode(string secretData, string secret = "TA3YiYCfY2dDJQgg")
    {
        byte[] encrypted;
        var iv = Encoding.UTF8.GetBytes(Vi);

        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(secret);
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            using (var encryptor = aes.CreateEncryptor())
            {
                using (var stream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cryptoStream))
                        {
                            sw.Write(secretData);
                        }

                        encrypted = stream.ToArray();
                    }
                }
            }
        }

        return Convert.ToBase64String(encrypted);
    }

    public static string CreateSecretKey(int length)
    {
        const string str = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var sb = new StringBuilder(length);
        var rnd = new Random();

        for (var i = 0; i < length; ++i)
        {
            sb.Append(str[rnd.Next(0, str.Length)]);
        }

        return sb.ToString();
    }

    public static string CloudMusicDllEncode(string deviceId)
    {
        var xored = new byte[deviceId.Length];
        for (var i = 0; i < deviceId.Length; i++)
        {
            xored[i] = (byte)(deviceId[i] ^ ID_XOR_KEY_1[i % ID_XOR_KEY_1.Length]);
        }

        using (var md5 = MD5.Create())
        {
            var digest = md5.ComputeHash(xored);
            return Convert.ToBase64String(digest);
        }
    }
}