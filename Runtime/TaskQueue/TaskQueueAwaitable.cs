#nullable enable
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Mew.Core.Tasks
{
    public class TaskQueueAwaitable
    {
        private readonly TaskCompletionSource<bool> taskCompletionSource;
        private readonly TaskAction taskAction;

        private Action<Exception>? onException;

        public int Priority { get; }

        public TaskAwaiter<bool> GetAwaiter() => taskCompletionSource.Task.GetAwaiter();

        public TaskQueueAwaitable(TaskAction func, int priority)
        {
            taskCompletionSource = new TaskCompletionSource<bool>();
            taskAction = func;
            Priority = priority;
        }

        public TaskQueueAwaitable OnException(Action<Exception> action)
        {
            onException = action;
            return this;
        }

        public async Task Invoke(CancellationToken ct)
        {
            try
            {
                await taskAction.Invoke(ct);
                taskCompletionSource.SetResult(true);
            }
            catch (Exception e)
            {
                if (onException is not null)
                    onException.Invoke(e);
                taskCompletionSource.SetException(e);
            }
        }

        public IEnumerator ToEnumerator()
        {
            var awaiter = GetAwaiter();
            while (!awaiter.IsCompleted) yield return null;
        }

        public static implicit operator Task(TaskQueueAwaitable awaitable)
        {
            return awaitable.taskCompletionSource.Task;
        }

        public void Cancel()
        {
            taskCompletionSource.SetResult(false);
        }
    }
}