using System.Threading;

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
    }
}