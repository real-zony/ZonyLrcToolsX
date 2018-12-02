using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Zony.Lib.NCMConverter.Convert
{
    public static class NCMExtenstion
    {
        /// <summary>
        /// 从文件流当中读取 4 字节的数据并将其转换为 Int32 类型
        /// </summary>
        /// <param name="fs">待读取的文件流</param>
        /// <returns>读取到的 Int32 值</returns>
        public static int ReadInt32(FileStream fs)
        {
            var buffer = new byte[4];
            fs.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer,0);
        }
        
        /// <summary>
        /// 设置当前文件流的位置
        /// </summary>
        /// <param name="fs">待设置的文件流</param>
        /// <param name="offset">偏移量</param>
        public static void Seek(FileStream fs,int offset)
        {
            fs.Seek(offset, SeekOrigin.Current);
        }

        /// <summary>
        /// 从文件流中读取指定字节的数据并返回
        /// </summary>
        /// <param name="fs">待读取的文件流</param>
        /// <param name="length">读取的长度</param>
        /// <returns></returns>
        public static byte[] ReadBytes(FileStream fs, int length)
        {
            var buffer = new byte[length];
            fs.Read(buffer, 0, length);
            return buffer;
        }

        /// <summary>
        /// 从源字节组的指定位置截取指定长度的字节数据，并生成新的字节数组
        /// </summary>
        /// <param name="srcBytes">待截取的源字节流</param>
        /// <param name="offset">偏移量</param>
        /// <param name="length">要截取的长度</param>
        /// <returns>截取完成的字节数组</returns>
        public static byte[] GetBytesByOffset(byte[] srcBytes,int offset = 0,int length = 0)
        {
            if (length == 0)
            {
                var zeroBytes = new byte[srcBytes.Length - offset];
                Array.Copy(srcBytes,offset,zeroBytes,0,srcBytes.Length - offset);
                return zeroBytes;
            }

            var resultBytes = new byte[length];
            Array.Copy(srcBytes, 0, resultBytes, 0,length);
            return resultBytes;
        }

        /// <summary>
        /// 使用 Key 解密 AES 加密后的数据
        /// </summary>
        /// <param name="keyBytes">密钥的字节数据</param>
        /// <param name="data">待解密的数据</param>
        /// <returns>解密成功后的数据</returns>
        public static byte[] DecryptAes128Ecb(byte[] keyBytes, byte[] data)
        {
            var aes = Aes.Create();
            if (aes != null)
            {
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.ECB;
                using (var decryptor = aes.CreateDecryptor(keyBytes, null))
                {
                    byte[] result = decryptor.TransformFinalBlock(data, 0, data.Length);
                    return result;
                }
            }

            throw new NullReferenceException("无法创建 AES 工具类.");
        }

        public static byte[] BuildKeyBox(byte[] key)
        {
            var box = new byte[256];
            for (int i = 0; i < 256; ++i)
            {
                box[i] = (byte) i;
            }

            byte c = 0;
            byte lastByte = 0;
            byte keyOffset = 0;
            byte swap;

            for(int i=0;i <256;++i)
            {
                swap = box[i];
                c = (byte) ((swap + lastByte + key[keyOffset++]) & 0xff);

                if (keyOffset >= key.Length) keyOffset = 0;

                box[i] = box[c];
                box[c] = swap;
                lastByte = c;
            }

            return box;
        }

        /// <summary>
        /// 搜索指定目录下的 NCM 文件
        /// </summary>
        /// <param name="dirPath">待搜索的目录</param>
        /// <returns>搜索完成的结果</returns>
        public static List<string> FindNCMFiles(string dirPath)
        {
            var result = new List<string>();
            SearchFile(result, dirPath);
            return result;
        }

        private static void SearchFile(List<string> files, string dirPath)
        {
            try
            {
                foreach (var file in Directory.GetFiles(dirPath, "*.ncm"))
                {
                    files.Add(file);
                }

                foreach (var directory in Directory.GetDirectories(dirPath))
                {
                    SearchFile(files,directory);
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
