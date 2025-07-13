#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Time = UnityEngine.Time;

namespace Mew.Core.Tasks
{
    public class TaskInterval : IDisposable
    {
        private float lastTime;
        private float elapsed;
        private ValueTaskAwaiter? awaiter;
        private CancellationTokenSource? cts;
        private CancellationToken? disposeCt;
        private Action<Exception>? onException;

        private Stopwatch Stopwatch { get; } = new();
        private bool Disposed { get; set; }


        private Action? SyncAction { get; }
        private TaskAction? AsyncAction { get; }
        public IntervalTimerType IntervalTimerType { get; }
        public LagProcessType LagProcessType { get; }
        public TimeSpan Interval { get; }
        public string LoopId { get; }


        /// <summary>
        /// Create TaskInterval.
        /// </summary>
        /// <param name="intervalMs"></param>
        /// <param name="action"></param>
        /// <param name="intervalTimerType"></param>
        /// <param name="lagProcessType"></param>
        /// <returns></returns>
        public static TaskInterval Create(double intervalMs, Action action, IntervalTimerType intervalTimerType = IntervalTimerType.UnityTime, LagProcessType lagProcessType = LagProcessType.Skip)
        {
            return Create(TimeSpan.FromMilliseconds(intervalMs), action, intervalTimerType, lagProcessType);
        }

        /// <summary>
        /// Create TaskInterval.
        /// </summary>
        /// <param name="intervalMs"></param>
        /// <param name="action"></param>
        /// <param name="intervalTimerType"></param>
        /// <param name="lagProcessType"></param>
        /// <returns></returns>
        public static TaskInterval Create(double intervalMs, TaskAction action, IntervalTimerType intervalTimerType = IntervalTimerType.UnityTime, LagProcessType lagProcessType = LagProcessType.Skip)
        {
            return Create(TimeSpan.FromMilliseconds(intervalMs), action, intervalTimerType, lagProcessType);
        }

        /// <summary>
        /// Create TaskInterval.
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="intervalTimerType"></param>
        /// <param name="lagProcessType"></param>
        /// <returns></returns>
        public static TaskInterval Create(TimeSpan interval, Action action, IntervalTimerType intervalTimerType = IntervalTimerType.UnityTime, LagProcessType lagProcessType = LagProcessType.Skip)
        {
            return new TaskInterval(interval, action, intervalTimerType, lagProcessType, string.Empty);
        }

        /// <summary>
        /// Create TaskInterval.
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="intervalTimerType"></param>
        /// <param name="lagProcessType"></param>
        /// <returns></returns>
        public static TaskInterval Create(TimeSpan interval, TaskAction action, IntervalTimerType intervalTimerType = IntervalTimerType.UnityTime, LagProcessType lagProcessType = LagProcessType.Skip)
        {
            return new TaskInterval(interval, action, intervalTimerType, lagProcessType, string.Empty);
        }

        protected TaskInterval(TimeSpan interval, Action action, IntervalTimerType intervalTimerType, LagProcessType lagProcessType, string loopId)
        {
            Interval = interval;
            SyncAction = action;
            IntervalTimerType = intervalTimerType;
            LagProcessType = lagProcessType;
            LoopId = loopId;
        }

        protected TaskInterval(TimeSpan interval, TaskAction action, IntervalTimerType intervalTimerType, LagProcessType lagProcessType, string loopId)
        {
            Interval = interval;
            AsyncAction = action;
            IntervalTimerType = intervalTimerType;
            LagProcessType = lagProcessType;
            LoopId = loopId;
        }

        /// <summary>
        /// Run action every interval.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="ct">Dispose TaskInterval when ct is cancelled.</param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Start(CancellationToken? ct = null)
        {
            if (Disposed) throw new ObjectDisposedException($"TaskInterval is disposed");

            Stop();

            disposeCt = ct;
            if (string.IsNullOrEmpty(LoopId)) MewLoop.Add(Update);
            else MewLoop.Add(LoopId, Update);

            Stopwatch.Start();
        }

        /// <summary>
        /// Progress timer manually.
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Tick(float elapsedTime)
        {
            if (Disposed) throw new ObjectDisposedException($"TaskInterval is disposed");

            if (IntervalTimerType != IntervalTimerType.ManualUpdate)
                throw new InvalidOperationException("TaskInterval.Tick() is called, but ProcessType is not ManualUpdate");

            elapsed += elapsedTime;
        }

        public TaskInterval OnException(Action<Exception> action)
        {
            onException = action;
            return this;
        }

        public void Dispose()
        {
            Stop();
            Disposed = true;
        }

        private void Stop()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
            awaiter = null;
            Stopwatch.Stop();
            Stopwatch.Reset();

            if (string.IsNullOrEmpty(LoopId)) MewLoop.Remove(Update);
            else MewLoop.Remove(LoopId, Update);
        }

        private void Update()
        {
            if (disposeCt.HasValue && disposeCt.Value.IsCancellationRequested)
            {
                Dispose();
                return;
            }

            UpdateTime();

            if (awaiter.HasValue)
            {
                if (!awaiter.Value.IsCompleted) return;
                cts?.Cancel();
                cts?.Dispose();
                cts = null;
                awaiter = null;
            }

            var interval = (float)Interval.TotalSeconds;
            if (elapsed < interval) return;
            elapsed -= interval;

            var flood = elapsed > interval;
            if (flood)
            {
                switch (LagProcessType)
                {
                    case LagProcessType.Skip: elapsed = interval; break;
                    case LagProcessType.Flood: break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            Invoke();
        }

        private async void Invoke()
        {
            try
            {
                if (SyncAction is not null)
                {
                    SyncAction.Invoke();
                }

                if (AsyncAction is not null)
                {
                    var taskCts = new CancellationTokenSource();
                    cts = disposeCt.HasValue
                        ? CancellationTokenSource.CreateLinkedTokenSource(taskCts.Token, disposeCt.Value)
                        : taskCts;
                    var task = AsyncAction(cts.Token);
                    awaiter = task.GetAwaiter();
                    await task;
                }
            }
            catch (Exception e)
            {
                if (onException is null)
                    throw;
                onException.Invoke(e);
            }
        }

        private void UpdateTime()
        {
            switch (IntervalTimerType)
            {
                case IntervalTimerType.SystemTime: ProcessBySystemTime(); break;
                case IntervalTimerType.UnityTime: ProcessByUnityTime(); break;
                case IntervalTimerType.UnityUnscaledTime: ProcessByUnityUnscaledTime(); break;
                case IntervalTimerType.ManualUpdate: break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessBySystemTime()
        {
            var currentTime = Stopwatch.ElapsedMilliseconds / 1000f;
            var elapsedTime = currentTime - lastTime;
            lastTime = currentTime;
            elapsed += elapsedTime;
        }

        private void ProcessByUnityTime()
        {
            elapsed += Time.deltaTime;
        }

        private void ProcessByUnityUnscaledTime()
        {
            elapsed += Time.unscaledDeltaTime;
        }
    }

    /// <inheritdoc cref="TaskInterval"/>
    public class TaskInterval<T> : TaskInterval
    {
        private TaskInterval(TimeSpan interval, Action action, IntervalTimerType intervalTimerType, LagProcessType lagProcessType, string loopId)
            : base(interval, action, intervalTimerType, lagProcessType, loopId)
        { }
        private TaskInterval(TimeSpan interval, TaskAction action, IntervalTimerType intervalTimerType, LagProcessType lagProcessType, string loopId)
            : base(interval, action, intervalTimerType, lagProcessType, loopId)
        { }

        /// <summary>
        /// <inheritdoc cref="TaskInterval.Create(double, Action, IntervalTimerType, LagProcessType)" />
        /// </summary>
        /// <param name="intervalMs"></param>
        /// <param name="action"></param>
        /// <param name="intervalTimerType"></param>
        /// <param name="lagProcessType"></param>
        /// <returns></returns>
        public new static TaskInterval Create(double intervalMs, Action action, IntervalTimerType intervalTimerType = IntervalTimerType.UnityTime, LagProcessType lagProcessType = LagProcessType.Skip)
        {
            return Create(TimeSpan.FromMilliseconds(intervalMs), action, intervalTimerType, lagProcessType);
        }

        /// <summary>
        /// <inheritdoc cref="TaskInterval.Create(double, TaskAction, IntervalTimerType, LagProcessType)" />
        /// </summary>
        /// <param name="intervalMs"></param>
        /// <param name="action"></param>
        /// <param name="intervalTimerType"></param>
        /// <param name="lagProcessType"></param>
        /// <returns></returns>
        public new static TaskInterval Create(double intervalMs, TaskAction action, IntervalTimerType intervalTimerType = IntervalTimerType.UnityTime, LagProcessType lagProcessType = LagProcessType.Skip)
        {
            return Create(TimeSpan.FromMilliseconds(intervalMs), action, intervalTimerType, lagProcessType);
        }

        /// <summary>
        /// <inheritdoc cref="TaskInterval.Create(TimeSpan, Action, IntervalTimerType, LagProcessType)" />
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="intervalTimerType"></param>
        /// <param name="lagProcessType"></param>
        /// <returns></returns>
        public new static TaskInterval Create(TimeSpan interval, Action action, IntervalTimerType intervalTimerType = IntervalTimerType.UnityTime, LagProcessType lagProcessType = LagProcessType.Skip)
        {
            var loopId = MewLoop.LoopId<T>();
            return new TaskInterval<T>(interval, action, intervalTimerType, lagProcessType, loopId);
        }

        /// <summary>
        /// <inheritdoc cref="TaskInterval.Create(TimeSpan, TaskAction, IntervalTimerType, LagProcessType)" />
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="intervalTimerType"></param>
        /// <param name="lagProcessType"></param>
        /// <returns></returns>
        public new static TaskInterval Create(TimeSpan interval, TaskAction action, IntervalTimerType intervalTimerType = IntervalTimerType.UnityTime, LagProcessType lagProcessType = LagProcessType.Skip)
        {
            var loopId = MewLoop.LoopId<T>();
            return new TaskInterval<T>(interval, action, intervalTimerType, lagProcessType, loopId);
        }

    }
}