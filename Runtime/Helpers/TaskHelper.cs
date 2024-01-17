using System.Threading.Tasks;
using UnityEngine;

namespace Mew.Core.TaskHelpers
{
    public static class TaskHelper
    {
        public static async Task NextFrame()
        {
#if UNITY_2023_2_OR_NEWER
            AwaitableCompletionSource awaitableCompletionSource = new();
            MewLoop.Add<MewUnityUpdate>(OnUpdate);
            await awaitableCompletionSource.Awaitable;
            MewLoop.Remove<MewUnityUpdate>(OnUpdate);
            return;

            void OnUpdate() => awaitableCompletionSource.TrySetResult();
#else
            TaskCompletionSource<bool> taskCompletionSource = new();
            MewLoop.Add<MewUnityUpdate>(OnUpdate);
            await taskCompletionSource.Task;
            MewLoop.Remove<MewUnityUpdate>(OnUpdate);
            return;

            void OnUpdate() => taskCompletionSource.TrySetResult(true);
#endif
        }
    }
}