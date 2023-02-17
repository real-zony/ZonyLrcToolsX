using System.Security.Cryptography;
using System.Text;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Common.MusicScanner;

public class NetEaseMusicSongListMusicScanner : ITransientDependency
{
}

public interface INetEaseMusicSongListMusicScanner
{
}

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Reference github links:
/// https://github.com/Quandong-Zhang/easynet-playlist-downloader/blob/main/main.py
/// https://github.com/mos9527/pyncm/blob/master/pyncm/apis/login.py
/// </remarks>
public class NetEaseMusicSessionManager : INetEaseMusicSongListMusicScanner, ISingletonDependency
{
}

public class NetEaseCrypto
{
    private readonly (long, long) WEAPI_RSA_PUBKEY = (
        Convert.ToInt64(
            "00e0b509f6259df8642dbc35662901477df22677ec152b5ff68ace615bb7b725152b3ab17a876aea8a5aa76d2e417629ec4ee341f56135fccf695280104e0312ecbda92557c93870114af6c9d05c4f7f0c3685b7a46bee255932575cce10b424d813cfe4875d3e82047b97ddef52741d546b8e289dc6935b3ece0462db0a22b8e7",
            16),
        Convert.ToInt64("10001", 16) // textbook rsa without padding
    );

    private const string Base62 = "PJArHa0dpwhvMNYqKnTbitWfEmosQ9527ZBx46IXUgOzD81VuSFyckLRljG3eC";
    private const string Base64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

    private const string WEAPI_AES_KEY = "0CoJUm6Qyw8W8jud"; // cbc
    private const string WEAPI_AES_IV = "0102030405060708"; // cbc
    private const string LINUXAPI_AES_KEY = "rFgB&h#%2?^eDg:Q"; // ecb
    private const string EAPI_DIGEST_SALT = "nobody%(url)suse%(text)smd5forencrypt";
    private const string EAPI_DATA_SALT = "%(url)s-36cd479b6b5-%(text)s-36cd479b6b5-%(digest)s";
    private const string EAPI_AES_KEY = "e82ckenh8dichen8"; // ecb

    public string WeApiEncrypt(string @params, string aesKey2 = null)
    {
        aesKey2 ??= RandomString(16);
        // 1st go,encrypt the text with aes_key and aes_iv.
        @params = AesEncrypt(@params, WEAPI_AES_KEY, WEAPI_AES_IV, CipherMode.CBC);
        // 2nd go,encrypt the ENCRYPTED text again,with the 2nd key and aes_iv.
        @params = AesEncrypt(@params, aesKey2, WEAPI_AES_IV);
        // 3rd go,generate RSA encrypted encSecKey.
        var encSecKey = HexDigest(RsaEncrypt(aesKey2, WEAPI_RSA_PUBKEY.Item1, WEAPI_RSA_PUBKEY.Item2));
        return $"params={@params}&encSecKey={encSecKey}";
    }

    public string AesEncrypt(string data, string key, string iv, CipherMode mode = CipherMode.CBC)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Encoding.UTF8.GetBytes(iv);
            aes.Mode = mode;
            aes.Padding = PaddingMode.PKCS7;
            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                var plainBytes = System.Text.Encoding.UTF8.GetBytes(Pkcs7Pad(data));
                var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                return Convert.ToBase64String(encryptedBytes);
            }
        }
    }

    private string Pkcs7Pad(string data, int bs = 16)
    {
        var padding = bs - data.Length % bs;
        return data + new string((char)padding, padding);
    }

    private string RandomString(int length, string chars = Base62)
    {
        var random = new Random();
        return string.Join("", Enumerable.Range(0, length).Select(_ => chars[random.Next(chars.Length)]));
    }

    private string HexDigest(byte[] data)
    {
        return string.Join("", data.Select(b => b.ToString("X2")));
    }

    static byte[] RsaEncrypt(string data, long n, long e, bool reverse = true)
    {
        var m = reverse ? data.Reverse() : data;
        var mBytes = Encoding.UTF8.GetBytes(string.Join("", m));
        var mHex = BitConverter.ToString(mBytes).Replace("-", string.Empty);
        var mInt = int.Parse(mHex, System.Globalization.NumberStyles.HexNumber);
        var r = (int)Math.Pow(mInt, e) % n;
        return BitConverter.GetBytes(r);
    }

    static string HexCompose(string hex)
    {
        return BitConverter.ToString(Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray());
    }
}