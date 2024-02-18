using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mew.Core.TaskHelpers
{
    public static class TaskHelperInternal
    {
        private static readonly Stack<MewCompletionSource> Pool = new();
        private static readonly Queue<MewCompletionSource> Queued = new();
        private static readonly List<MewCompletionSource> Running = new();
        private static bool registered;

        public static async Task NextFrame(CancellationToken ct = default)
        {
            if (!registered)
            {
                MewLoop.Add<MewUnityUpdate>(OnUpdate);
                registered = true;
            }

            if (Pool.Count == 0) Pool.Push(new MewCompletionSource());

            var awaitableCompletionSource = Pool.Pop();
            awaitableCompletionSource.Reset();
            Queued.Enqueue(awaitableCompletionSource);

            await awaitableCompletionSource.Awaitable;

            ct.ThrowIfCancellationRequested();
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
