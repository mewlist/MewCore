using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Mew.Core.TaskHelpers
{
    public static class TaskHelper
    {
#if UNITY_2023_2_OR_NEWER
        private static readonly Stack<AwaitableCompletionSource> pool = new();
        private static readonly Queue<AwaitableCompletionSource> queued = new();
        private static readonly List<AwaitableCompletionSource> running = new();
        private static bool registered;
#endif

        public static async Task NextFrame()
        {
#if UNITY_2023_2_OR_NEWER
            if (!registered)
            {
                MewLoop.Add<MewUnityUpdate>(OnUpdate);
                registered = true;
            }
            if (pool.Count == 0)
                pool.Push(new AwaitableCompletionSource());
            var awaitableCompletionSource = pool.Pop();
            awaitableCompletionSource.Reset();
            queued.Enqueue(awaitableCompletionSource);
            await awaitableCompletionSource.Awaitable;
            return;

            void OnUpdate()
            {
                while (queued.Count > 0)
                    running.Add(queued.Dequeue());

                foreach (var completionSource in running)
                {
                    completionSource.TrySetResult();
                    pool.Push(completionSource);
                }
                running.Clear();
            }
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