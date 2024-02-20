#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Mew.Core.Tasks
{
    public class TaskQueue : IDisposable
    {
        private readonly List<TaskQueueAwaitable> queue = new();
        private readonly ICollection queueAsCollection;
        private bool updateProcessing;
        private bool taskProcessing;
        private int processingPriority;
        private CancellationTokenSource? cts;
        private CancellationToken disposeCt;
        protected string loopId = string.Empty;

        private object SyncRoot => queueAsCollection.SyncRoot;

        public TaskQueueLimitType LimitType { get; }
        /// <summary>
        /// Max size of queue.
        /// </summary>
        public int MaxSize { get; }

        /// <summary>
        /// Count of running or waiting tasks.
        /// </summary>
        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return queue.Count + (taskProcessing ? 1 : 0);
                }
            }
        }

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
            queueAsCollection = queue;
            if (string.IsNullOrEmpty(loopId)) MewLoop.Add(Update);
            else MewLoop.Add(loopId, Update);
        }

        /// <summary>
        /// Set cancellation token to dispose TaskQueue.
        /// </summary>
        /// <param name="ct">Dispose TaskQueue when ct is cancelled.</param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void DisposeWith(CancellationToken ct)
        {
            if (Disposed) throw new ObjectDisposedException($"TaskQueue[{loopId}] is disposed");
            disposeCt = ct;
            if (ct.IsCancellationRequested)
                Dispose();
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

                            if (index < 0 && taskProcessing && processingPriority >= priority)
                            {
                                if (taskProcessing) CancelCurrent();
                                break;
                            }
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
            disposeCt = CancellationToken.None;
            lock (SyncRoot)
            {
                queue.Clear();
            }
            Disposed = true;
        }

        ~TaskQueue()
        {
            Dispose();
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
        }

        private void Update()
        {
            if (disposeCt != CancellationToken.None && disposeCt.IsCancellationRequested)
            {
                Dispose();
                return;
            }

            if (updateProcessing) return;
            lock (SyncRoot)
                if (queue.Count == 0) return;

            UpdateInternal();
        }

        private async void UpdateInternal()
        {
            updateProcessing = true;

            while (true)
            {
                using var taskCts = new CancellationTokenSource();
                TaskQueueAwaitable task;

                lock (SyncRoot)
                {
                    task = queue[0];
                    queue.RemoveAt(0);
                    taskProcessing = true;
                    processingPriority = task.Priority;
                }

                cts = disposeCt != CancellationToken.None
                    ? CancellationTokenSource.CreateLinkedTokenSource(taskCts.Token, disposeCt)
                    : taskCts;

                await task.Invoke(cts.Token);

                cts?.Dispose();
                cts = null;

                lock (SyncRoot) if (queue.Count == 0) break;
            }

            taskProcessing = false;
            processingPriority = int.MaxValue;
            updateProcessing = false;
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