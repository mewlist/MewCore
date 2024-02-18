using System;
using System.Threading.Tasks;

#if UNITY_2023_2_OR_NEWER
using UnityEngine;
#endif

namespace Mew.Core
{
    public class MewCompletionSource
    {
#if UNITY_2023_2_OR_NEWER
        private readonly AwaitableCompletionSource taskCompletionSource = new();
        public Awaitable Awaitable => taskCompletionSource.Awaitable;
#else
        private readonly AwaitableCompletionCompatibleSource taskCompletionSource = new();
        public Task Awaitable => taskCompletionSource.Awaitable;
#endif

        public void SetResult()
        {
            if (!TrySetResult())
                throw new InvalidOperationException("Can't raise completion of the same Awaitable twice");
        }

        public void SetCanceled()
        {
            if (!TrySetCanceled())
                throw new InvalidOperationException("Can't raise completion of the same Awaitable twice");
        }

        public void SetException(Exception exception)
        {
            if (!TrySetException(exception))
                throw new InvalidOperationException("Can't raise completion of the same Awaitable twice");
        }

        public bool TrySetResult() => taskCompletionSource.TrySetResult();
        public bool TrySetCanceled() => taskCompletionSource.TrySetCanceled();
        public bool TrySetException(Exception exception) => taskCompletionSource.TrySetException(exception);
        public void Reset() => taskCompletionSource.Reset();
    }
}