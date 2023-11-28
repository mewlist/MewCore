#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Mew.Core.Tasks
{
    public class TaskQueue : IDisposable
    {
        private readonly List<TaskQueueAwaitable> queue = new();
        private TaskAwaiter? awaiter;
        private CancellationTokenSource? cts;
        private CancellationToken? disposeCt;

        protected string loopId = string.Empty;

        private object SyncRoot => ((ICollection)queue).SyncRoot;

        public TaskQueueLimitType LimitType { get; }
        public int MaxSize { get; }

        /// <summary>
        /// Count of running or waiting tasks.
        /// </summary>
        public int Count => (queue?.Count ?? 0) + (awaiter.HasValue ? 1 : 0);

        /// <summary>
        /// true if started.
        /// </summary>
        public bool Started { get; private set; }

        /// <summary>
        /// true if disposed.
        /// </summary>
        public bool Disposed { get; private set; }

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
            if (Started) Stop();
            disposeCt = ct;
            if (string.IsNullOrEmpty(loopId)) MewLoop.Add(Update);
            else MewLoop.Add(loopId, Update);
            Started = true;
        }

        private void Stop()
        {
            if (Disposed) throw new ObjectDisposedException($"TaskQueue[{loopId}] is disposed");
            CancelCurrent();
            lock (SyncRoot)
            {
                queue.Clear();
            }
            if (string.IsNullOrEmpty(loopId)) MewLoop.Remove(Update);
            else MewLoop.Remove(loopId, Update);
            Started = false;
        }

        /// <summary>
        /// Enqueue task.
        /// </summary>
        /// <param name="func"></param>
        /// <param name="priority">Low number is prior. default is 0</param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Enqueue(TaskAction func, int priority = 0)
        {
            EnqueueAsync(func, priority);
        }

        /// <summary>
        /// Enqueue task.
        /// </summary>
        /// <param name="func"></param>
        /// <param name="priority">Low number is prior. default is 0</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public TaskQueueAwaitable EnqueueAsync(TaskAction func, int priority = 0)
        {
            if (Disposed) throw new ObjectDisposedException("TaskQueue is disposed");
            if (!Started) throw new InvalidOperationException("TaskQueue is not started. Call Start() before enqueue.");
            var newTask = new TaskQueueAwaitable(func, priority);
            var flood = Count >= MaxSize;

            lock (SyncRoot)
            {
                var at = queue.FindIndex(x => x.Priority > newTask.Priority);
                if (at == -1) queue.Add(newTask);
                else queue.Insert(at, newTask);

                switch (LimitType)
                {
                    case TaskQueueLimitType.None: break;
                    case TaskQueueLimitType.SwapLast:
                        if (flood)
                        {
                            var index = queue.FindLastIndex(x =>
                                x.Priority >= priority && x != newTask);
                            index = index >= 0 ? index : queue.Count - 1;
                            var task = queue[index];
                            task.Cancel();
                            queue.RemoveAt(index);
                        }

                        break;
                    case TaskQueueLimitType.Discard:
                        if (flood)
                        {
                            var index = queue.Count - 1;
                            var task = queue[index];
                            task.Cancel();
                            queue.RemoveAt(index);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return newTask;
            }
        }

        public void Dispose()
        {
            if (Disposed) return;
            Stop();
            disposeCt = null;
            lock (SyncRoot)
            {
                queue.Clear();
            }
            Disposed = true;
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

            if (awaiter.HasValue) return;

            TaskQueueAwaitable task;

            lock (SyncRoot)
            {
                if (!queue.Any()) return;

                task = queue.First();
                queue.RemoveAt(0);
            }

            var taskCts = new CancellationTokenSource();
            cts = disposeCt.HasValue
                ? CancellationTokenSource.CreateLinkedTokenSource(taskCts.Token, disposeCt.Value)
                : taskCts;
            var invokedTask = task.Invoke(cts.Token);
            awaiter = invokedTask.GetAwaiter();
            await invokedTask.ContinueWith(_ => awaiter = null, taskCts.Token);
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