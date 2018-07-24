using Zony.Lib.Infrastructures.Dependency;

namespace ZonyLrcTools.Encoders.Provider
{
    public interface IEncoderProvider : ISingletonDependency
    {
        /// <summary>
        /// 根据编码页名称获得编码器
        /// </summary>
        /// <param name="encodePageName">编码页名称</param>
        /// <returns>获得的编码器</returns>
        IEncoder GetEncoder(string encodePageName);
    }
}