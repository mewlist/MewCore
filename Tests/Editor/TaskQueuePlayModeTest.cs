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
            AddTestTask(taskQueue, result, result: 2, priority: 1);
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
            AddTestTask(taskQueue, result, result: 0, priority: 1);
            AddTestTask(taskQueue, result, result: 1, priority: 1);
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
            AddTestTask(taskQueue, result, result: 2, priority: 1);
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
            AddTestTask(taskQueue, result, result: 0, priority: 1);
            AddTestTask(taskQueue, result, result: 1, priority: 1);
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

        private static void AddTestTask(TaskQueue taskQueue, ICollection<int> resultList, int result, int priority)
        {
            taskQueue.Enqueue(async ct =>
            {
                await Task.Delay(TimeSpan.FromSeconds(0.1f), ct);
                resultList.Add(result);
            }, priority);
        }
    }
}