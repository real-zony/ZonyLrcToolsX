using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using SystemConvert = System.Convert;

namespace Zony.Lib.NCMConverter.Convert
{
    public class NCMConverter
    {
        /// <summary>
        /// 转换 NCM 文件为 Flac/MP3 文件
        /// </summary>
        /// <param name="filePath">NCM 文件路径</param>
        /// <returns>转换状态</returns>
        public NCMConverterEnum ProcessFile(string filePath)
        {
            using (var fs = File.Open(filePath, FileMode.Open))
            {
                // 校验是否是网易云加密的 NCM 文件
                if (NCMExtenstion.ReadInt32(fs) != 0x4e455443)
                {
                    return NCMConverterEnum.Invalid;
                }

                if (NCMExtenstion.ReadInt32(fs) != 0x4d414446)
                {
                    return NCMConverterEnum.Invalid;
                }

                // 读取密钥
                NCMExtenstion.Seek(fs,2);
                var keyBytes = NCMExtenstion.ReadBytes(fs, NCMExtenstion.ReadInt32(fs));

                for (int i = 0; i < keyBytes.Length; i++)
                {
                    keyBytes[i] ^= 0x64;
                }

                // 减去 "neteasecloudmusic" 字符串之后的数据即为密钥数据
                var deKeyDataBytes = NCMExtenstion.GetBytesByOffset(NCMExtenstion.DecryptAes128Ecb(Encoding.UTF8.GetBytes(NCMConveterConsts.AesCoreKeyString), keyBytes), 17);

                var modifyDataBytes = NCMExtenstion.ReadBytes(fs, NCMExtenstion.ReadInt32(fs));
                for (int i = 0; i < modifyDataBytes.Length; i++)
                {
                    modifyDataBytes[i] ^= 0x63;
                }

                var decryptBase64 = SystemConvert.FromBase64String(Encoding.UTF8.GetString(NCMExtenstion.GetBytesByOffset(modifyDataBytes, 22)));
                var decryptModifyDataBytes = NCMExtenstion.DecryptAes128Ecb(Encoding.UTF8.GetBytes(NCMConveterConsts.AesModifyKeyBytes), decryptBase64);

                // 获取 NCM 文件的歌曲文件信息 Json
                var musicJson = JObject.Parse(Encoding.UTF8.GetString(NCMExtenstion.GetBytesByOffset(decryptModifyDataBytes, 6)));
                var fileExtension = musicJson.SelectToken("$.format").Value<string>();

                // 跳过 CRC 校验
                NCMExtenstion.Seek(fs,9);

                // 跳过专辑图像数据
                var imageBytes = new byte[NCMExtenstion.ReadInt32(fs)];
                if (imageBytes.Length != 0)
                {
                    fs.Read(imageBytes, 0, imageBytes.Length);
                }

                var box = NCMExtenstion.BuildKeyBox(deKeyDataBytes);

                var n = 0x8000;
                using (var outputFile = File.Create(Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}.{fileExtension}")))
                {
                    while (true)
                    {
                        var tb = new byte[n];
                        var result = fs.Read(tb, 0, n);
                        if(result <= 0) break;

                        for (int i = 0; i < n; i++)
                        {
                            var j = (byte) ((i + 1) & 0xff);
                            tb[i] ^= box[box[j] + box[(box[j] + j) & 0xff] & 0xff];
                        }

                        outputFile.Write(tb,0,n);
                    }

                    outputFile.Flush();
                }

                fs.Close();
                return NCMConverterEnum.Success;
            }
        }
    }
}