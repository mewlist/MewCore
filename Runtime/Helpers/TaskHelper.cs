using System.Threading.Tasks;
using UnityEngine;

namespace Mew.Core.TaskHelpers
{
    public static class TaskHelper
    {
        public static async Task NextFrame()
        {
            AwaitableCompletionSource awaitableCompletionSource = new();
            MewLoop.Add<MewUnityUpdate>(OnUpdate);
            await awaitableCompletionSource.Awaitable;
            MewLoop.Remove<MewUnityUpdate>(OnUpdate);
            return;

            void OnUpdate() => awaitableCompletionSource.TrySetResult();
        }
    }
}