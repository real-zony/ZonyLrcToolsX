namespace ZonyLrcTools.Common.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// 替换文件名当中的无效路径
        /// </summary>
        /// <param name="fileName">要替换的文件名称</param>
        /// <returns></returns>
        public static string ReplaceBadCharOfFileName(this string fileName)
        {
            string str = fileName;
            str = str.Replace("\\", string.Empty);
            str = str.Replace("/", string.Empty);
            str = str.Replace(":", string.Empty);
            str = str.Replace("*", string.Empty);
            str = str.Replace("?", string.Empty);
            str = str.Replace("\"", string.Empty);
            str = str.Replace("<", string.Empty);
            str = str.Replace(">", string.Empty);
            str = str.Replace("|", string.Empty);
            str = str.Replace(" ", string.Empty);    //前面的替换会产生空格,最后将其一并替换掉
            return str;
        }
    }
}