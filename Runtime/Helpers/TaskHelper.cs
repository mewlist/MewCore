using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#if USE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace Mew.Core.TaskHelpers
{
    public static class TaskHelper
    {
#if USE_UNITASK
        public static async UniTask NextFrame(CancellationToken ct = default)
        {
            await UniTask.NextFrame(ct);
        }
#else
        public static async Task NextFrame(CancellationToken ct = default)
        {
            await TaskHelperInternal.NextFrame(ct);
        }
#endif

        public static async ValueTask<T[]> WhenAll<T>(params ValueTask<T>[] tasks)
        {
            var results = new T[tasks.Length];
            for (var i = 0; i < tasks.Length; i++)
                results[i] = await tasks[i].ConfigureAwait(false);
            return results;
        }

        public static async ValueTask WhenAll(params ValueTask[] tasks)
        {
            foreach (var t in tasks)
                await t.ConfigureAwait(false);
        }
    }
}