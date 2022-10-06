namespace ZonyLrcTools.Common.Infrastructure.IO
{
    public static class FileStreamExtensions
    {
        /// <summary>
        /// 将字节数据通过缓冲区的形式，写入到文件当中。
        /// </summary>
        /// <param name="fileStream">需要写入数据的文件流。</param>
        /// <param name="data">等待写入的数据。</param>
        /// <param name="bufferSize">缓冲区大小。</param>
        public static async Task WriteBytesToFileAsync(this FileStream fileStream, byte[] data, int bufferSize = 1024)
        {
            await using (fileStream)
            {
                var count = data.Length / 1024;
                var modCount = data.Length % 1024;
                if (count <= 0)
                {
                    await fileStream.WriteAsync(data, 0, modCount);
                }
                else
                {
                    for (var i = 0; i < count; i++)
                    {
                        await fileStream.WriteAsync(data, i * 1024, 1024);
                    }

                    if (modCount != 0)
                    {
                        await fileStream.WriteAsync(data, count * 1024, modCount);
                    }
                }

                await fileStream.FlushAsync();
            }
        }
    }
}