#if UNITY_2023_2_OR_NEWER
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Mew.Core.TaskHelpers
{
    internal static class TaskHelperInternal
    {
        private static readonly Stack<AwaitableCompletionSource> Pool = new();
        private static readonly Queue<AwaitableCompletionSource> Queued = new();
        private static readonly List<AwaitableCompletionSource> Running = new();
        private static bool registered;

        public static async Task NextFrame()
        {
            if (!registered)
            {
                MewLoop.Add<MewUnityUpdate>(OnUpdate);
                registered = true;
            }
            if (Pool.Count == 0)
                Pool.Push(new AwaitableCompletionSource());
            var awaitableCompletionSource = Pool.Pop();
            awaitableCompletionSource.Reset();
            Queued.Enqueue(awaitableCompletionSource);
            await awaitableCompletionSource.Awaitable;
            return;

            void OnUpdate()
            {
                while (Queued.Count > 0)
                    Running.Add(Queued.Dequeue());

                foreach (var completionSource in Running)
                {
                    completionSource.TrySetResult();
                    Pool.Push(completionSource);
                }
                Running.Clear();
            }
        }
    }
}
#endif
