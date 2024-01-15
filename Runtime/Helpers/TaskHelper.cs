using System.Threading.Tasks;
using UnityEngine;

namespace Mew.Core.TaskHelpers
{
    public static class TaskHelper
    {
        public static async Task NextFrame()
        {
            AwaitableCompletionSource awaitableCompletionSource = new();
            MewLoop.Add<MewUnityUpdate>(() => awaitableCompletionSource.TrySetResult());
            await awaitableCompletionSource.Awaitable;
        }
    }
}