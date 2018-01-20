using System;

namespace Zony.Lib.Infrastructures.Threading.Extensions
{
    public static class LockExtensions
    {
        public static void Locking<T>(this T source, Action<T> action) where T : class
        {
            lock (source)
            {
                action(source);
            }
        }
    }
}
