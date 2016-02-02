namespace Microsoft.Content.Build.DoxygenMigration.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public static class TaskUtility
    {
        /// <summary>
        /// Provide parallel version for ForEach
        /// </summary>
        /// <typeparam name="T">The type for the enumerable</typeparam>
        /// <param name="source">The enumerable to control the foreach loop</param>
        /// <param name="body">The task body</param>
        /// <param name="maxParallelism">The max parallelism allowed</param>
        /// <returns>The task</returns>
        public static async Task ForEachInParallelAsync<T>(this IEnumerable<T> source, Func<T, Task> body, int maxParallelism)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }

            using (var semaphore = new SemaphoreSlim(maxParallelism))
            {
                // warning "access to disposed closure" around "semaphore" could be ignored as it is inside Task.WhenAll
                await Task.WhenAll(from s in source select ForEachCoreAsync(body, semaphore, s));
            }
        }

        private static async Task ForEachCoreAsync<T>(Func<T, Task> body, SemaphoreSlim semaphore, T s)
        {
            await semaphore.WaitAsync();
            try
            {
                await body(s);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Provide parallel version for ForEach
        /// </summary>
        /// <typeparam name="T">The type for the enumerable</typeparam>
        /// <param name="source">The enumerable to control the foreach loop</param>
        /// <param name="body">The task body</param>
        /// <returns>The task</returns>
        /// <remarks>The max parallelism is 64</remarks>
        public static Task ForEachInParallelAsync<T>(this IEnumerable<T> source, Func<T, Task> body)
        {
            return ForEachInParallelAsync(source, body, 64);
        }
    }
}
