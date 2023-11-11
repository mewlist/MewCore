#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Mew.Core.Tasks
{
    public class TaskQueue : IDisposable
    {
        private TaskAwaiter? awaiter;
        private CancellationTokenSource? cts;
        private CancellationToken? disposeCt;
        private List<TaskWithPriority>? queue = new();

        protected string loopId = string.Empty;


        public TaskQueueLimitType LimitType { get; }
        public int MaxSize { get; }

        /// <summary>
        /// Count of running or waiting tasks.
        /// </summary>
        public int Count => (queue?.Count ?? 0) + (awaiter.HasValue ? 1 : 0);

        /// <summary>
        /// true if disposed.
        /// </summary>
        public bool Disposed => queue == null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskQueueLimitType"></param>
        /// <param name="maxSize"></param>
        public TaskQueue(TaskQueueLimitType taskQueueLimitType = TaskQueueLimitType.None, int maxSize = 0)
        {
            if (taskQueueLimitType != TaskQueueLimitType.None && maxSize < 1)
                throw new ArgumentOutOfRangeException(nameof(maxSize), "maxSize must be greater than 0");

            LimitType = taskQueueLimitType;
            MaxSize = maxSize;
        }

        /// <summary>
        /// Start TaskQueue.
        /// </summary>
        /// <param name="ct">Dispose TaskQueue when ct is cancelled.</param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Start(CancellationToken? ct = null)
        {
            if (Disposed) throw new ObjectDisposedException($"TaskQueue[{loopId}] is disposed");
            Stop();
            disposeCt = ct;
            if (string.IsNullOrEmpty(loopId)) MewLoop.Add(Update);
            else MewLoop.Add(loopId, Update);
        }

        private void Stop()
        {
            if (queue is null) throw new ObjectDisposedException($"TaskQueue[{loopId}] is disposed");
            CancelCurrent();
            queue.Clear();
            if (string.IsNullOrEmpty(loopId)) MewLoop.Remove(Update);
            else MewLoop.Remove(loopId, Update);
        }

        public void Enqueue(TaskAction func, int priority = 0)
        {
            if (queue is null) throw new ObjectDisposedException("TaskQueue is disposed");
            var newTask = new TaskWithPriority(func, priority);
            var flood = Count >= MaxSize;

            queue.Add(newTask);
            queue = queue.OrderBy(x => -x.Priority).ToList();

            switch (LimitType)
            {
                case TaskQueueLimitType.None: break;
                case TaskQueueLimitType.SwapLast:
                    if (flood)
                    {
                        var index = queue.FindLastIndex(x =>
                            x.Priority <= priority && x != newTask);
                        if (index >= 0) queue.RemoveAt(index);
                        else queue.RemoveAt(queue.Count - 1);
                    }
                    break;
                case TaskQueueLimitType.Discard:
                    if (flood) queue.RemoveAt(queue.Count - 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Dispose()
        {
            if (Disposed) return;
            Stop();
            disposeCt = null;
            queue = null;
        }

        public bool Any()
        {
            return Count > 0;
        }

        private void CancelCurrent()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
            awaiter = null;
        }

        private async void Update()
        {
            if (disposeCt is { IsCancellationRequested: true })
            {
                Dispose();
                return;
            }

            if (awaiter.HasValue)
            {
                if (awaiter.Value.IsCompleted) CancelCurrent();
                else return;
            }

            if (queue is null) return;
            if (queue.Count == 0) return;

            var task = queue.First();
            queue.RemoveAt(0);
            var taskCts = new CancellationTokenSource();
            cts = disposeCt.HasValue
                ? CancellationTokenSource.CreateLinkedTokenSource(taskCts.Token, disposeCt.Value)
                : taskCts;
            var invokedTask = task.Func.Invoke(cts.Token); 
            awaiter = invokedTask.GetAwaiter();
            await invokedTask;
        }

        public async Task WaitForEmptyAsync()
        {
            while (Count > 0) await Task.Yield();
        }
    }

    /// <summary>
    /// TaskQueue for specific loop.
    /// </summary>
    /// <typeparam name="T">Loop Timing Type</typeparam>
    public class TaskQueue<T> : TaskQueue
    {
        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public TaskQueue() : base()
        { loopId = MewLoop.LoopId<T>(); }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        /// <param name="taskQueueLimitType"></param>
        /// <param name="maxSize"></param>
        public TaskQueue(TaskQueueLimitType taskQueueLimitType, int maxSize)
            : base(taskQueueLimitType, maxSize)
        { loopId = MewLoop.LoopId<T>(); }
    }
}