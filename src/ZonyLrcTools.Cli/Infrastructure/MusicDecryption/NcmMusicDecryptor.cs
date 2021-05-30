using System.Threading.Tasks;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Cli.Infrastructure.MusicDecryption
{
    public class NcmMusicDecryptor : IMusicDecryptor, ITransientDependency
    {
        public Task<byte[]> Convert(byte[] sourceBytes)
        {
            throw new System.NotImplementedException();
        }
    }
}