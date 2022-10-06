namespace ZonyLrcTools.Common.Infrastructure.Extensions
{
    /// <summary>
    /// Linq 相关的扩展方法。
    /// </summary>
    public static class LinqHelper
    {
        /// <summary>
        /// 使用 Lambda 的形式遍历指定的迭代器。
        /// </summary>
        /// <param name="items">等待遍历的迭代器实例。</param>
        /// <param name="action">遍历时需要执行的操作。</param>
        public static void Foreach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }
    }
}