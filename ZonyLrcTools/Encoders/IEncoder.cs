using System.Text;
using Zony.Lib.Infrastructures.Dependency;

namespace ZonyLrcTools.Encoders
{
    /// <summary>
    /// 编码器
    /// </summary>
    public interface IEncoder : ITransientDependency
    {
        /// <summary>
        /// 将字符串转换为指定编码格式的 Bytes 
        /// </summary>
        /// <param name="sourceStr">需要转换的字符串</param>
        byte[] Encoding(string sourceStr);
        /// <summary>
        /// 将源字节组转换为指定编码格式的 Bytes
        /// </summary>
        /// <param name="sourceBytes">源字节组</param>
        byte[] Encoding(byte[] sourceBytes);
        /// <summary>
        /// 将源字节组转换为指定编码格式的 Bytes
        /// </summary>
        /// <param name="sourceBytes">源字节组</param>
        /// <param name="sourceEncode">源字节组编码</param>
        byte[] Encoding(byte[] sourceBytes, Encoding sourceEncode);

        /// <summary>
        /// 要使用的编码页
        /// </summary>
        string EncodePageName { get; set; }
    }
}