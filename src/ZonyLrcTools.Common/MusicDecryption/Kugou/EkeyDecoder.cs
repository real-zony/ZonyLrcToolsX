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
        private static readonly byte[] V2Prefix = "UVFNdXNpYyBFbmNWMixLZXk6"u8.ToArray();

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
        /// <param name="invalidCharCount">
        /// Base64 段里被忽略的非法字符数量。Base64 解码本身是宽容的（会静默映射成 0），
        /// 这里把它统计出来，让上层能把「密钥损坏」和「没有密钥」区分开。
        /// </param>
        /// </summary>
        /// <returns>原始密钥；解码失败时为空数组。</returns>
        public static byte[] Decode(string ekey, out int invalidCharCount)
        {
            invalidCharCount = 0;
            if (string.IsNullOrEmpty(ekey))
            {
                return [];
            }

            var raw = Encoding.Latin1.GetBytes(ekey);
            if (raw.Length >= V2Prefix.Length && raw.AsSpan(0, V2Prefix.Length).SequenceEqual(V2Prefix))
            {
                var inner = TcTea.CbcDecrypt(raw[V2Prefix.Length..], TcTea.ToRoundKeys(V2Key1));
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
            var result = Base64Decode(Encoding.Latin1.GetBytes(ekey), out invalidCharCount);
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

        private static readonly byte[] Base64Table =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"u8.ToArray();

        /// <summary>
        /// 宽容的 Base64 解码，兼容 URL-safe 变体与不规范的长度。
        /// <param name="invalidCount">输入中不属于 Base64 的字符数量（空白字符会被跳过，不计入）。</param>
        /// </summary>
        private static byte[] Base64Decode(byte[] input, out int invalidCount)
        {
            var reverse = new byte[256];
            var isKnown = new bool[256];
            for (var i = 0; i < Base64Table.Length; i++)
            {
                reverse[Base64Table[i]] = (byte)i;
                isKnown[Base64Table[i]] = true;
            }

            // URL-safe 变体。
            reverse[(byte)'-'] = 62;
            reverse[(byte)'_'] = 63;
            isKnown[(byte)'-'] = true;
            isKnown[(byte)'_'] = true;
            isKnown[(byte)'='] = true;

            // 空白字符按「忽略」处理，其余未知字符计入 invalidCount，交给调用方判定为损坏。
            isKnown[(byte)' '] = true;
            isKnown[(byte)'\r'] = true;
            isKnown[(byte)'\n'] = true;
            isKnown[(byte)'\t'] = true;

            invalidCount = 0;
            var cleaned = new byte[input.Length];
            var cleanedLength = 0;
            foreach (var value in input)
            {
                if (value is (byte)' ' or (byte)'\r' or (byte)'\n' or (byte)'\t')
                {
                    continue;
                }

                if (!isKnown[value])
                {
                    invalidCount++;
                    continue;
                }

                cleaned[cleanedLength++] = value;
            }

            input = cleaned.AsSpan(0, cleanedLength).ToArray();

            var output = new byte[(input.Length + 3) / 4 * 3];
            var outOffset = 0;

            // 解一组 4 个字符；返回 true 表示遇到了填充，可以结束。
            bool DecodeBlock(ReadOnlySpan<byte> block)
            {
                var a = reverse[block[0]];
                var b = reverse[block[1]];
                var c = reverse[block[2]];
                var d = reverse[block[3]];

                output[outOffset++] = (byte)((a << 2) | (b >> 4));

                if (block[2] == '=')
                {
                    return true;
                }

                output[outOffset++] = (byte)((b << 4) | (c >> 2));

                if (block[3] == '=')
                {
                    return true;
                }

                output[outOffset++] = (byte)((c << 6) | d);
                return false;
            }

            var fullLength = input.Length - input.Length % 4;
            for (var i = 0; i < fullLength; i += 4)
            {
                if (DecodeBlock(input.AsSpan(i, 4)))
                {
                    return output.AsSpan(0, outOffset).ToArray();
                }
            }

            var remainder = input.Length % 4;
            if (remainder != 0)
            {
                var tail = new byte[4];
                tail[0] = input[fullLength];
                tail[1] = remainder > 1 ? input[fullLength + 1] : (byte)'=';
                tail[2] = (byte)'=';
                tail[3] = (byte)'=';
                DecodeBlock(tail);
            }

            return output.AsSpan(0, outOffset).ToArray();
        }
    }
}
