// Ported from the MIT-licensed reference implementation:
//   https://github.com/lizipeng0013/kgg-dec.mirror
//   Copyright (c) 2024 LSR
// The original licence text is reproduced in THIRD-PARTY-NOTICES.md at the repository root.

using System.Text;

namespace ZonyLrcTools.Common.MusicDecryption.Kugou
{
    /// <summary>
    /// 酷狗／QQ 音乐的 ekey 解码器。
    /// </summary>
    internal static class EkeyDecoder
    {
        /// <summary>
        /// V2 前缀，解码后即 "QQMusic EncV2,Key:"。
        /// </summary>
        private const string V2Prefix = "UVFNdXNpYyBFbmNWMixLZXk6";

        private static readonly byte[] V2Key1 =
        [
            0x33, 0x38, 0x36, 0x5A, 0x4A, 0x59, 0x21, 0x40, 0x23, 0x2A, 0x24, 0x25, 0x5E, 0x26, 0x29, 0x28
        ];

        private static readonly byte[] V2Key2 =
        [
            0x2A, 0x2A, 0x23, 0x21, 0x28, 0x23, 0x24, 0x25, 0x26, 0x5E, 0x61, 0x31, 0x63, 0x5A, 0x2C, 0x54
        ];

        /// <summary>
        /// 将数据库中的 ekey 字符串还原为 QMC2 使用的原始密钥。
        /// </summary>
        /// <returns>原始密钥；解码失败时为空数组。</returns>
        public static byte[] Decode(string ekey) => Decode(ekey, out _);

        /// <summary>
        /// 将数据库中的 ekey 字符串还原为 QMC2 使用的原始密钥。
        /// </summary>
        /// <param name="ekey">数据库中的密钥。</param>
        /// <param name="invalidCharCount">Base64 中的非法字符数量，空白不计入。</param>
        /// <returns>原始密钥；解码失败时为空数组。</returns>
        public static byte[] Decode(string ekey, out int invalidCharCount)
        {
            invalidCharCount = 0;
            if (string.IsNullOrEmpty(ekey))
            {
                return [];
            }

            if (ekey.StartsWith(V2Prefix, StringComparison.Ordinal))
            {
                var raw = Encoding.Latin1.GetBytes(ekey[V2Prefix.Length..]);
                var inner = TcTea.CbcDecrypt(raw, TcTea.ToRoundKeys(V2Key1));
                if (inner.Length == 0)
                {
                    return [];
                }

                inner = TcTea.CbcDecrypt(inner, TcTea.ToRoundKeys(V2Key2));
                if (inner.Length == 0)
                {
                    return [];
                }

                return DecodeV1(Encoding.Latin1.GetString(inner), out invalidCharCount);
            }

            return DecodeV1(ekey, out invalidCharCount);
        }

        private static byte[] DecodeV1(string ekey, out int invalidCharCount)
        {
            var result = Base64Decode(ekey, out invalidCharCount);
            if (result.Length < 8)
            {
                return [];
            }

            var teaKey = new[]
            {
                0x69005600u | ((uint)result[0] << 16) | result[1],
                0x46003800u | ((uint)result[2] << 16) | result[3],
                0x2b002000u | ((uint)result[4] << 16) | result[5],
                0x15000b00u | ((uint)result[6] << 16) | result[7]
            };

            var decrypted = TcTea.CbcDecrypt(result[8..], teaKey);
            if (decrypted.Length == 0)
            {
                return [];
            }

            var output = new byte[8 + decrypted.Length];
            result.AsSpan(0, 8).CopyTo(output);
            decrypted.CopyTo(output.AsSpan(8));
            return output;
        }

        private static byte[] Base64Decode(string input, out int invalidCount)
        {
            var normalized = new char[input.Length + 3];
            var length = 0;
            invalidCount = 0;
            foreach (var value in input)
            {
                if (value is ' ' or '\r' or '\n' or '\t')
                {
                    continue;
                }

                if (value is not (>= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9'
                    or '+' or '/' or '-' or '_' or '='))
                {
                    invalidCount++;
                    continue;
                }

                normalized[length++] = value switch
                {
                    '-' => '+',
                    '_' => '/',
                    _ => value
                };
            }

            if (invalidCount != 0 || length == 0 || length % 4 == 1)
            {
                return [];
            }

            // 兼容省略末尾填充的密钥，由标准解码器校验填充位置。
            while (length % 4 != 0)
            {
                normalized[length++] = '=';
            }

            var padding = normalized[length - 1] == '=' ? 1 : 0;
            if (normalized[length - 2] == '=')
            {
                padding++;
            }

            var output = new byte[length / 4 * 3 - padding];
            return Convert.TryFromBase64Chars(normalized.AsSpan(0, length), output, out _) ? output : [];
        }
    }
}
