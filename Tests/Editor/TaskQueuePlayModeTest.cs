#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

namespace Mew.Core.Tasks.Tests
{
    public class TaskQueuePlayModeTest
    {
        [UnityTest]
        public IEnumerator SwapLastTest()
        {
            var taskQueue = new TaskQueue(TaskQueueLimitType.SwapLast, 2);
            var result = new List<int>();
            taskQueue.Start();
            AddTestTask(taskQueue, result, result: 0, priority: 0);
            AddTestTask(taskQueue, result, result: 1, priority: 0);
            AddTestTask(taskQueue, result, result: 2, priority: 0);
            while (taskQueue.Any()) yield return null;
            Assert.IsTrue(result.SequenceEqual(new List<int>{ 0, 2 }));
            taskQueue.Dispose();
        }

        [UnityTest]
        public IEnumerator SwapLastHighPriorityTest()
        {
            var taskQueue = new TaskQueue(TaskQueueLimitType.SwapLast, 2);
            var result = new List<int>();
            taskQueue.Start();
            AddTestTask(taskQueue, result, result: 0, priority: 0);
            AddTestTask(taskQueue, result, result: 1, priority: 0);
            AddTestTask(taskQueue, result, result: 2, priority: -1);
            while (taskQueue.Any()) yield return null;
            Assert.IsTrue(result.SequenceEqual(new List<int>{ 2, 0 }));
            taskQueue.Dispose();
        }

        [UnityTest]
        public IEnumerator SwapLastLowPriorityTest()
        {
            var taskQueue = new TaskQueue(TaskQueueLimitType.SwapLast, 2);
            var result = new List<int>();
            taskQueue.Start();
            AddTestTask(taskQueue, result, result: 0, priority: -1);
            AddTestTask(taskQueue, result, result: 1, priority: -1);
            AddTestTask(taskQueue, result, result: 2, priority: 0);
            while (taskQueue.Any()) yield return null;
            Assert.IsTrue(result.SequenceEqual(new List<int>{ 0, 1 }));
            taskQueue.Dispose();
        }

        [UnityTest]
        public IEnumerator DiscardTest()
        {
            var taskQueue = new TaskQueue(TaskQueueLimitType.Discard, 2);
            var result = new List<int>();
            taskQueue.Start();
            AddTestTask(taskQueue, result, result: 0, priority: 0);
            AddTestTask(taskQueue, result, result: 1, priority: 0);
            AddTestTask(taskQueue, result, result: 2, priority: 0);
            while (taskQueue.Any()) yield return null;
            Assert.IsTrue(result.SequenceEqual(new List<int>{ 0, 1 }));
            taskQueue.Dispose();
        }

        [UnityTest]
        public IEnumerator DiscardHighPriorityTest()
        {
            var taskQueue = new TaskQueue(TaskQueueLimitType.Discard, 2);
            var result = new List<int>();
            taskQueue.Start();
            AddTestTask(taskQueue, result, result: 0, priority: 0);
            AddTestTask(taskQueue, result, result: 1, priority: 0);
            AddTestTask(taskQueue, result, result: 2, priority: -1);
            while (taskQueue.Any()) yield return null;
            Assert.IsTrue(result.SequenceEqual(new List<int>{ 2, 0 }));
            taskQueue.Dispose();
        }

        [UnityTest]
        public IEnumerator DiscardLowPriorityTest()
        {
            var taskQueue = new TaskQueue(TaskQueueLimitType.Discard, 2);
            var result = new List<int>();
            taskQueue.Start();
            AddTestTask(taskQueue, result, result: 0, priority: -1);
            AddTestTask(taskQueue, result, result: 1, priority: -1);
            AddTestTask(taskQueue, result, result: 2, priority: 0);
            while (taskQueue.Any()) yield return null;
            Assert.IsTrue(result.SequenceEqual(new List<int>{ 0, 1 }));
            taskQueue.Dispose();
        }

        [UnityTest]
        public IEnumerator DisposeTest()
        {
            var taskQueue = new TaskQueue();
            var result = new List<int>();
            taskQueue.Start();
            AddTestTask(taskQueue, result, result: 0, priority: 0);
            AddTestTask(taskQueue, result, result: 1, priority: 0);
            AddTestTask(taskQueue, result, result: 2, priority: 0);
            while (taskQueue.Count >= 2) yield return null;
            taskQueue.Dispose();
            while (taskQueue.Any()) yield return null;
            Assert.IsTrue(result.SequenceEqual(new List<int>{ 0, 1 }));
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(result.SequenceEqual(new List<int>{ 0, 1 }));
            Assert.IsTrue(taskQueue.Disposed);
        }

        [UnityTest]
        public IEnumerator DisposeByCancellationTokenTest()
        {
            var cts = new CancellationTokenSource();
            var taskQueue = new TaskQueue();
            var result = new List<int>();
            taskQueue.Start(cts.Token);
            AddTestTask(taskQueue, result, result: 0, priority: 0);
            AddTestTask(taskQueue, result, result: 1, priority: 0);
            AddTestTask(taskQueue, result, result: 2, priority: 0);
            while (taskQueue.Count >= 2) yield return null;
            cts.Cancel();
            while (taskQueue.Any()) yield return null;
            Assert.IsTrue(result.SequenceEqual(new List<int>{ 0, 1 }));
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(result.SequenceEqual(new List<int>{ 0, 1 }));
            Assert.IsTrue(taskQueue.Disposed);
        }

        [UnityTest]
        public IEnumerator TaskAwaiterTest()
        {
            var cts = new CancellationTokenSource();
            var taskQueue = new TaskQueue();
            var result = new List<int>();
            taskQueue.Start(cts.Token);
            yield return AddTestTask(taskQueue, result, result: 0, priority: 0).ToEnumerator();
            Assert.AreEqual(0, taskQueue.Count);
            yield return AddTestTask(taskQueue, result, result: 1, priority: 0).ToEnumerator();
            Assert.AreEqual(0, taskQueue.Count);
            yield return AddTestTask(taskQueue, result, result: 2, priority: 0).ToEnumerator();

            // TODO: Discard されたタスクの await できる？
            Assert.AreEqual(0, taskQueue.Count);
            Assert.IsTrue(result.SequenceEqual(new List<int>{ 0, 1, 2 }));
            taskQueue.Dispose();
        }

        [UnityTest]
        public IEnumerator TaskAwaiterDiscardTest()
        {
            var cts = new CancellationTokenSource();
            var taskQueue = new TaskQueue(TaskQueueLimitType.Discard, 2);
            var result = new List<int>();
            taskQueue.Start(cts.Token);

            var tasks = new List<IEnumerator>
            {
                AddTestTask(taskQueue, result, result: 0, priority: 0).ToEnumerator(),
                AddTestTask(taskQueue, result, result: 1, priority: 0).ToEnumerator(),
                AddTestTask(taskQueue, result, result: 2, priority: 0).ToEnumerator()
            };
            var toDiscard = tasks[2];

            yield return toDiscard;
            foreach (var enumerator in tasks)
                yield return enumerator;

            Assert.AreEqual(0, taskQueue.Count);
            Assert.IsTrue(result.SequenceEqual(new List<int>{ 0, 1 }));
            taskQueue.Dispose();
        }

        [UnityTest]
        public IEnumerator TaskAwaiterSwapTest()
        {
            var cts = new CancellationTokenSource();
            var taskQueue = new TaskQueue(TaskQueueLimitType.SwapLast, 2);
            var result = new List<int>();
            taskQueue.Start(cts.Token);

            var tasks = new List<IEnumerator>
            {
                AddTestTask(taskQueue, result, result: 0, priority: 0).ToEnumerator(),
                AddTestTask(taskQueue, result, result: 1, priority: 0).ToEnumerator(),
                AddTestTask(taskQueue, result, result: 2, priority: 0).ToEnumerator()
            };
            var toDiscard = tasks[1];

            yield return toDiscard;
            foreach (var enumerator in tasks)
                yield return enumerator;

            Assert.AreEqual(0, taskQueue.Count);
            Assert.IsTrue(result.SequenceEqual(new List<int>{ 0, 2 }));
            taskQueue.Dispose();
        }

        [UnityTest]
        public IEnumerator TaskExceptionTest()
        {
            var cts = new CancellationTokenSource();
            var taskQueue = new TaskQueue();
            var result = new List<int>();
            taskQueue.Start(cts.Token);

            var exceptionCount = 0;
            var tasks = new List<TaskQueueAwaitable>
            {
                AddThrowTask(taskQueue, priority: 0).OnException(_ => exceptionCount++),
                AddThrowTask(taskQueue, priority: 0).OnException(_ => exceptionCount++),
                AddThrowTask(taskQueue, priority: 0).OnException(_ => exceptionCount++)
            };

            var task = WaitForAllTaskComplete();
            while (!task.IsCompleted)
                yield return null;

            Assert.AreEqual(0, taskQueue.Count);
            Assert.AreEqual(3, exceptionCount);
            Assert.IsTrue(result.SequenceEqual(new List<int>()));
            taskQueue.Dispose();
            yield break;

            async Task WaitForAllTaskComplete()
            {
                foreach (var awaitable in tasks)
                {
                    try { await awaitable; }
                    catch {  }
                }
            }
        }

        private static TaskQueueAwaitable AddTestTask(TaskQueue taskQueue, ICollection<int> resultList, int result, int priority)
        {
            return taskQueue.EnqueueAsync(async ct =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(0.1f), ct);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                resultList.Add(result);
            }, priority);
        }

        private static TaskQueueAwaitable AddThrowTask(TaskQueue taskQueue, int priority)
        {
            return taskQueue.EnqueueAsync(async ct =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(0.1f), ct);
                    throw new Exception("Exception");
                }
                catch (TaskCanceledException)
                {
                }
            }, priority);
        }
    }
}