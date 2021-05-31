using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Cli.Infrastructure.MusicDecryption
{
    /// <summary>
    /// NCM 音乐转换器，用于将 NCM 格式的音乐转换为可播放的格式。
    /// </summary>
    public class NcmMusicDecryptor : IMusicDecryptor, ITransientDependency
    {
        protected readonly byte[] AesCoreKey = {0x68, 0x7A, 0x48, 0x52, 0x41, 0x6D, 0x73, 0x6F, 0x35, 0x6B, 0x49, 0x6E, 0x62, 0x61, 0x78, 0x57};
        protected readonly byte[] AesModifyKey = {0x23, 0x31, 0x34, 0x6C, 0x6A, 0x6B, 0x5F, 0x21, 0x5C, 0x5D, 0x26, 0x30, 0x55, 0x3C, 0x27, 0x28};

        public async Task<DecryptionResult> ConvertMusic(byte[] sourceBytes)
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

            var coreKeyBytes = GetBytesByOffset(DecryptAes128Ecb(AesCoreKey, keyBytes), 17);

            var modifyDataBytes = new byte[streamReader.ReadInt32()];
            stream.Read(modifyDataBytes);
            for (int i = 0; i < modifyDataBytes.Length; i++)
            {
                modifyDataBytes[i] ^= 0x63;
            }

            var decryptBase64Bytes = Convert.FromBase64String(Encoding.UTF8.GetString(GetBytesByOffset(modifyDataBytes, 22)));
            var decryptModifyData = DecryptAes128Ecb(AesModifyKey, decryptBase64Bytes);

            var musicInfoJson = JObject.Parse(Encoding.UTF8.GetString(GetBytesByOffset(decryptModifyData, 6)));

            // CRC 校验
            stream.Seek(4, SeekOrigin.Current);
            stream.Seek(5, SeekOrigin.Current);

            GetAlbumImageBytes(stream, streamReader);

            var sBox = BuildKeyBox(coreKeyBytes);
            return new DecryptionResult(GetMusicBytes(sBox, stream).ToArray())
            {
                ExtensionObjects = new Dictionary<string, object>
                {
                    {"JSON", musicInfoJson}
                }
            };
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

        /// <summary>
        /// RC4 加密，生成 KeyBox。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private byte[] BuildKeyBox(byte[] key)
        {
            byte[] box = new byte[256];
            for (int i = 0; i < 256; ++i)
            {
                box[i] = (byte) i;
            }

            byte keyLength = (byte) key.Length;
            byte c;
            byte lastByte = 0;
            byte keyOffset = 0;
            byte swap;

            for (int i = 0; i < 256; ++i)
            {
                swap = box[i];
                c = (byte) ((swap + lastByte + key[keyOffset++]) & 0xff);

                if (keyOffset >= keyLength)
                {
                    keyOffset = 0;
                }

                box[i] = box[c];
                box[c] = swap;
                lastByte = c;
            }

            return box;
        }

        /// <summary>
        /// 获得歌曲的专辑图像信息。
        /// </summary>
        /// <param name="stream">原始文件流。</param>
        /// <param name="streamReader">二进制读取器。</param>
        private byte[] GetAlbumImageBytes(Stream stream, BinaryReader streamReader)
        {
            var imgLength = streamReader.ReadInt32();

            if (imgLength <= 0)
            {
                return null;
            }

            var imgBuffer = streamReader.ReadBytes(imgLength);

            return imgBuffer;
        }

        /// <summary>
        /// 获得歌曲的完整数据。
        /// </summary>
        /// <param name="sBox"></param>
        /// <param name="stream">原始文件流。</param>
        private MemoryStream GetMusicBytes(byte[] sBox, Stream stream)
        {
            var n = 0x8000;
            var memoryStream = new MemoryStream();

            while (true)
            {
                var tb = new byte[n];
                var result = stream.Read(tb);
                if (result <= 0) break;

                for (int i = 0; i < n; i++)
                {
                    var j = (byte) ((i + 1) & 0xff);
                    tb[i] ^= sBox[sBox[j] + sBox[(sBox[j] + j) & 0xff] & 0xff];
                }

                memoryStream.Write(tb);
            }

            memoryStream.Flush();

            return memoryStream;
        }
    }
}