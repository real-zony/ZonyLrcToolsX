// Ported from the MIT-licensed reference implementation:
//   https://github.com/lizipeng0013/kgg-dec.mirror
//   Copyright (c) 2024 LSR
// The original licence text is reproduced in THIRD-PARTY-NOTICES.md at the repository root.

using System.Buffers.Binary;

namespace ZonyLrcTools.Common.MusicDecryption.Kugou
{
    /// <summary>
    /// 腾讯 TEA（TC-TEA）加解密实现，用于解析酷狗的 ekey 密文。
    /// <remarks>
    /// 这里只实现了 QMC2 所需的部分：单块 ECB 解密与带双 IV 链的 CBC 解密。
    /// 算法来源于酷狗客户端的实际行为，与 MusicDecrypto 内置的格式无关。
    /// </remarks>
    /// </summary>
    internal static class TcTea
    {
        private const int Rounds = 16;
        private const uint Delta = 0x9e3779b9;

        /// <summary>
        /// 16 轮累加后的 sum 值，等价于 <c>Rounds * Delta</c> 在 uint32 下的溢出结果。
        /// </summary>
        private const uint ExpectedSum = 0xE3779B90;

        private const int BlockSize = 8;
        private const int FixedSaltLength = 2;
        private const int ZeroPaddingLength = 7;
        private const int HeaderSize = BlockSize * 2;

        private static uint SingleRound(uint value, uint sum, uint key1, uint key2)
        {
            return ((value << 4) + key1) ^ (value + sum) ^ ((value >> 5) + key2);
        }

        /// <summary>
        /// TEA 单块解密（ECB）。
        /// </summary>
        public static ulong DecryptBlock(ulong value, ReadOnlySpan<uint> key)
        {
            var y = (uint)(value >> 32);
            var z = (uint)value;
            var sum = ExpectedSum;

            for (var i = 0; i < Rounds; i++)
            {
                z -= SingleRound(y, sum, key[2], key[3]);
                y -= SingleRound(z, sum, key[0], key[1]);
                sum -= Delta;
            }

            return ((ulong)y << 32) | z;
        }

        /// <summary>
        /// TC-TEA 的 CBC 解密，带自定义的头部跳过与零填充校验。
        /// <remarks>
        /// 参数与返回值都用数组而非 Span，是因为内部要用局部函数保存解密状态。
        /// </remarks>
        /// </summary>
        /// <returns>解密后的明文；若校验失败则返回空数组。</returns>
        public static byte[] CbcDecrypt(byte[] cipher, uint[] key)
        {
            // 由于填充方案的特殊性，密文至少要两个块长。
            if (cipher.Length % BlockSize != 0 || cipher.Length < HeaderSize)
            {
                return [];
            }

            var iv1 = 0UL;
            var iv2 = 0UL;
            var inOffset = 0;
            var header = new byte[HeaderSize];

            // 解密一轮，把结果写到 target 指定的 8 字节位置。
            void DecryptRound(byte[] target, int targetOffset)
            {
                var iv1Next = BinaryPrimitives.ReadUInt64BigEndian(cipher.AsSpan(inOffset));
                var iv2Next = DecryptBlock(iv1Next ^ iv2, key);
                var plain = iv2Next ^ iv1;

                iv1 = iv1Next;
                iv2 = iv2Next;
                BinaryPrimitives.WriteUInt64BigEndian(target.AsSpan(targetOffset), plain);
                inOffset += BlockSize;
            }

            DecryptRound(header, 0);
            DecryptRound(header, BlockSize);

            var headerSkipLength = 1 + (header[0] & 7) + FixedSaltLength;
            var realPlainLength = cipher.Length - headerSkipLength - ZeroPaddingLength;
            if (realPlainLength <= 0)
            {
                return [];
            }

            var result = new byte[realPlainLength];

            // 先拷出头部块里剩余的明文。
            var copyLength = Math.Min(HeaderSize - headerSkipLength, realPlainLength);
            Array.Copy(header, headerSkipLength, result, 0, copyLength);
            var outOffset = copyLength;

            if (realPlainLength != copyLength)
            {
                for (var i = cipher.Length - BlockSize * 3; i != 0; i -= BlockSize)
                {
                    DecryptRound(header, 0);
                    Array.Copy(header, 0, result, outOffset, BlockSize);
                    outOffset += BlockSize;
                }

                // 最后一个块只取首字节，且必须落在 header 的第二个块位置，供下方零填充校验使用。
                DecryptRound(header, BlockSize);
                result[outOffset] = header[BlockSize];
            }

            // 校验零填充：末尾 8 字节左移一位后必须为 0。
            var verify = BinaryPrimitives.ReadUInt64BigEndian(header.AsSpan(BlockSize)) << 8;
            return verify != 0 ? [] : result;
        }

        /// <summary>
        /// 将 16 字节的密钥按大端拆成 4 个 uint32，供 TEA 使用。
        /// </summary>
        public static uint[] ToRoundKeys(byte[] key)
        {
            var result = new uint[4];
            for (var i = 0; i < 4; i++)
            {
                result[i] = BinaryPrimitives.ReadUInt32BigEndian(key.AsSpan(i * 4));
            }

            return result;
        }
    }
}
