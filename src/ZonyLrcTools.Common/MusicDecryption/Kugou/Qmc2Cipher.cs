// Ported from the MIT-licensed reference implementation:
//   https://github.com/lizipeng0013/kgg-dec.mirror
//   Copyright (c) 2024 LSR
// The original licence text is reproduced in THIRD-PARTY-NOTICES.md at the repository root.

namespace ZonyLrcTools.Common.MusicDecryption.Kugou
{
    /// <summary>
    /// QMC2 加密流的解密器基类，负责按偏移量流式解出明文。
    /// </summary>
    internal abstract class Qmc2Cipher
    {
        /// <summary>
        /// 首段的长度，这一段的密钥派生方式与其他段不同。
        /// </summary>
        protected const int FirstSegmentSize = 0x80;

        /// <summary>
        /// 其余段落的长度。
        /// </summary>
        protected const int OtherSegmentSize = 0x1400;

        /// <summary>
        /// 解密数据流中的一个字节。
        /// </summary>
        /// <param name="cipher">密文中的该字节。</param>
        /// <param name="offset">该字节在完整音频流中的偏移量。</param>
        public abstract byte Decrypt(byte cipher, long offset);

        /// <summary>
        /// 就地把一段数据解密。
        /// <remarks>
        /// 批量接口，避免逐字节走虚函数调用；几十 MB 的文件靠逐字节会慢到不可接受。
        /// </remarks>
        /// </summary>
        /// <param name="data">待解密的密文，解密结果原地写回。</param>
        /// <param name="offset">这段数据在完整音频流中的起始偏移量。</param>
        public virtual void Decrypt(Span<byte> data, long offset)
        {
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = Decrypt(data[i], offset + i);
            }
        }

        /// <summary>
        /// 根据解码后的 ekey 长度选择具体的实现。
        /// </summary>
        public static Qmc2Cipher? Create(byte[] key)
        {
            if (key.Length == 0)
            {
                return null;
            }

            return key.Length < 300 ? new Qmc2MapCipher(key) : new Qmc2Rc4Cipher(key);
        }
    }

    /// <summary>
    /// 短密钥（长度小于 300）使用的查表型解密器。
    /// </summary>
    internal sealed class Qmc2MapCipher : Qmc2Cipher
    {
        private const int MapIndexOffset = 71214;
        private const int MapKeySize = 128;
        private const int MapIndexBoundary = 0x7FFF;

        private readonly byte[] _key;

        /// <summary>
        /// 根据短密钥生成查找表。
        /// </summary>
        public Qmc2MapCipher(byte[] key)
        {
            _key = new byte[MapKeySize];
            var length = key.Length;
            for (var i = 0; i < MapKeySize; i++)
            {
                var j = (int)(((long)i * i + MapIndexOffset) % length);
                var shift = (j + 4) % 8;
                var value = key[j];
                _key[i] = (byte)((value << shift) | (value >> shift));
            }
        }

        /// <inheritdoc />
        public override byte Decrypt(byte cipher, long offset)
        {
            var index = offset <= MapIndexBoundary ? offset : offset % MapIndexBoundary;
            return (byte)(cipher ^ _key[index % MapKeySize]);
        }

        /// <inheritdoc />
        public override void Decrypt(Span<byte> data, long offset)
        {
            for (var i = 0; i < data.Length; i++)
            {
                var position = offset + i;
                var index = position <= MapIndexBoundary ? position : position % MapIndexBoundary;
                data[i] ^= _key[index % MapKeySize];
            }
        }
    }

    /// <summary>
    /// 长密钥使用的 RC4 派生流解密器。
    /// </summary>
    internal sealed class Qmc2Rc4Cipher : Qmc2Cipher
    {
        /// <summary>
        /// 每段从密钥流中取用的前置长度上限。
        /// </summary>
        private const int SegmentSkipMask = 0x1FF;

        /// <summary>
        /// 密钥流的长度，比普通段多出 512 字节用于起始偏移。
        /// </summary>
        private const int KeyStreamSize = OtherSegmentSize + 512;

        private readonly byte[] _key;
        private readonly byte[] _keyStream = new byte[KeyStreamSize];
        private readonly uint _keyHash;

        /// <summary>
        /// 根据长密钥生成密钥流。
        /// </summary>
        public Qmc2Rc4Cipher(byte[] key)
        {
            _key = key;
            _keyHash = ComputeKeyHash(key);
            DeriveKeyStream();
        }

        /// <inheritdoc />
        public override byte Decrypt(byte cipher, long offset)
        {
            if (offset < FirstSegmentSize)
            {
                // 首段的索引直接由当前偏移推导，且随偏移逐字节变化。
                var seed = _key[(int)(offset % _key.Length)];
                var index = GetSegmentKey(_keyHash, offset, seed) % _key.Length;
                return (byte)(cipher ^ _key[index]);
            }

            var segmentIndex = offset / OtherSegmentSize;
            var segmentOffset = offset % OtherSegmentSize;
            var skipLength = GetSegmentKey(_keyHash, segmentIndex, _key[segmentIndex % _key.Length]) & SegmentSkipMask;
            return (byte)(cipher ^ _keyStream[skipLength + segmentOffset]);
        }

        /// <inheritdoc />
        public override void Decrypt(Span<byte> data, long offset)
        {
            // 首段与常规段的算法不同，先按边界切一刀。
            if (offset < FirstSegmentSize)
            {
                var length = (int)Math.Min(data.Length, FirstSegmentSize - offset);
                for (var i = 0; i < length; i++)
                {
                    var seed = _key[(int)((offset + i) % _key.Length)];
                    var index = GetSegmentKey(_keyHash, offset + i, seed) % _key.Length;
                    data[i] ^= _key[index];
                }

                offset += length;
                data = data[length..];
            }

            while (data.Length > 0)
            {
                var segmentIndex = offset / OtherSegmentSize;
                var segmentOffset = (int)(offset % OtherSegmentSize);
                var skipLength = GetSegmentKey(_keyHash, segmentIndex, _key[segmentIndex % _key.Length]) & SegmentSkipMask;
                var length = Math.Min(data.Length, OtherSegmentSize - segmentOffset);

                var stream = _keyStream.AsSpan((int)skipLength + segmentOffset, length);
                for (var i = 0; i < length; i++)
                {
                    data[i] ^= stream[i];
                }

                offset += length;
                data = data[length..];
            }
        }

        /// <summary>
        /// 把密钥压成一个 32 位的乘法哈希，遇到溢出就提前结束。
        /// </summary>
        private static uint ComputeKeyHash(ReadOnlySpan<byte> key)
        {
            var hash = 1u;
            foreach (var item in key)
            {
                if (item == 0)
                {
                    continue;
                }

                var next = hash * item;
                if (next <= hash)
                {
                    break;
                }

                hash = next;
            }

            return hash;
        }

        /// <summary>
        /// 按段号与种子计算出该段在密钥或密钥流中的起始位置。
        /// <remarks>
        /// 中间结果可能远超 int 上限，必须用 long 承载，否则会截断出错位。
        /// </remarks>
        /// </summary>
        private static long GetSegmentKey(uint hash, long segmentId, int seed)
        {
            if (seed == 0)
            {
                return 0;
            }

            return (long)(hash / (double)(seed * (segmentId + 1)) * 100.0);
        }

        /// <summary>
        /// 用 RC4 的 KSA 生成密钥流。
        /// </summary>
        private void DeriveKeyStream()
        {
            var length = _key.Length;
            var state = new byte[length];
            for (var i = 0; i < length; i++)
            {
                state[i] = (byte)i;
            }

            // 这里的 i / j 必须是宽于 byte 的整数，长度 512 时 byte 会回绕。
            var j = 0;
            for (var i = 0; i < length; i++)
            {
                j = (j + state[i] + _key[i]) % length;
                (state[i], state[j]) = (state[j], state[i]);
            }

            var a = 0;
            var b = 0;
            for (var i = 0; i < KeyStreamSize; i++)
            {
                a = (a + 1) % length;
                b = (b + state[a]) % length;
                (state[a], state[b]) = (state[b], state[a]);
                _keyStream[i] = state[(state[a] + state[b]) % length];
            }
        }
    }
}
