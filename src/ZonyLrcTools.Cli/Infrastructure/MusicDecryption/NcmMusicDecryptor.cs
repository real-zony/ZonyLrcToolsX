using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;
using ZonyLrcTools.Cli.Infrastructure.Exceptions;

namespace ZonyLrcTools.Cli.Infrastructure.MusicDecryption
{
    /// <summary>
    /// NCM 音乐转换器，用于将 NCM 格式的音乐转换为可播放的格式。
    /// </summary>
    public class NcmMusicDecryptor : IMusicDecryptor, ITransientDependency
    {
        protected readonly byte[] AesCoreKey = {0x68, 0x7A, 0x48, 0x52, 0x41, 0x6D, 0x73, 0x6F, 0x35, 0x6B, 0x49, 0x6E, 0x62, 0x61, 0x78, 0x57};
        protected readonly byte[] AesModifyKey = {0x23, 0x31, 0x34, 0x6C, 0x6A, 0x6B, 0x5F, 0x21, 0x5C, 0x5D, 0x26, 0x30, 0x55, 0x3C, 0x27, 0x28};

        public Task<byte[]> Convert(byte[] sourceBytes)
        {
            var stream = new MemoryStream(sourceBytes);
            var streamReader = new BinaryReader(stream);

            var lengthBytes = new byte[4];
            lengthBytes = streamReader.ReadBytes(4);
            if (BitConverter.ToInt32(lengthBytes) != 0x4e455443)
            {
                throw new Exception();
            }

            lengthBytes = streamReader.ReadBytes(4);
            if (BitConverter.ToInt32(lengthBytes) != 0x4d414446)
            {
                throw new Exception();
            }

            stream.Seek(2, SeekOrigin.Current);
            stream.Read(lengthBytes);

            var keyBytes = new byte[BitConverter.ToInt32(lengthBytes)];
            stream.Read(keyBytes);

            // 对已经加密的数据进行异或操作。
            for (int i = 0; i < keyBytes.Length; i++)
            {
                keyBytes[i] ^= 0x64;
            }

            var deKeyDataBytes = GetBytesByOffset(DecryptAes128Ecb(AesCoreKey, keyBytes), 17);

            var modifyDataBytes = new byte[streamReader.ReadInt32()];
            stream.Read(modifyDataBytes);
            for (int i = 0; i < modifyDataBytes.Length; i++)
            {
                modifyDataBytes[i] ^= 0x63;
            }

            throw new System.NotImplementedException();
        }

        private byte[] GetBytesByOffset(byte[] srcBytes, int offset = 0)
        {
            var resultBytes = new byte[srcBytes.Length - offset];
            Array.Copy(srcBytes, offset, resultBytes, 0, srcBytes.Length - offset);
            return resultBytes;
        }

        private byte[] DecryptAes128Ecb(byte[] keyBytes, byte[] data)
        {
            var aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.ECB;
            using var decryptor = aes.CreateDecryptor(keyBytes, null);
            var result = decryptor.TransformFinalBlock(data, 0, data.Length);

            return result;
        }
    }
}