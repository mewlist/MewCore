#nullable enable
using System.Collections;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Task = System.Threading.Tasks.Task;

namespace Mew.Core.Tasks.Tests
{
    public class TaskIntervalPlayModeTest
    {
        [UnityTest]
        public IEnumerator BasicTest()
        {
            var result = 0;
            var cts = new CancellationTokenSource();
            var startTime = Time.time;
            TaskInterval
                .Create(100, () => { result++; })
                .Start(cts.Token);

            while(Time.time - startTime < 1.05f)
                yield return null;

            cts.Cancel();
            cts.Dispose();
            Assert.AreEqual(10, result);

            yield return new WaitForSeconds(1f);
            
            Assert.AreEqual(10, result);
        }

        [UnityTest]
        public IEnumerator FixedUpdateTest()
        {
            var result = 0;
            var cts = new CancellationTokenSource();
            var startTime = Time.fixedTime;
            TaskInterval<MewUnityEarlyUpdate>
                .Create(100, () => { result++; })
                .Start(cts.Token);

            while(Time.fixedTime - startTime < 1.05f)
                yield return null;

            cts.Cancel();
            cts.Dispose();
            Assert.AreEqual(10, result);

            yield return new WaitForSeconds(1f);
            
            Assert.AreEqual(10, result);
        }

        [UnityTest]
        public IEnumerator SkipTaskWhenFloodTest()
        {
            var result = 0;
            var nextTrigger = false;
            var cts = new CancellationTokenSource();
            var taskInterval = TaskInterval<MewManualUpdate>.Create(
                100,
                async ct =>
                {
                    result++;
                    while(!nextTrigger)
                        await Task.Yield();
                },
                IntervalTimerType.ManualUpdate,
                LagProcessType.Skip);

            
            taskInterval.Start(cts.Token);
            taskInterval.Tick(0.11f);
            MewLoop.Update<MewManualUpdate>();
            Assert.AreEqual(1, result);
            
            // task awaiting
            taskInterval.Tick(0.11f);
            MewLoop.Update<MewManualUpdate>();
            Assert.AreEqual(1, result);

            // task takes long time and flood
            taskInterval.Tick(1f);

            // complete task
            nextTrigger = true;
            yield return new WaitForSeconds(0.1f);

            MewLoop.Update<MewManualUpdate>();
            Assert.AreEqual(2, result);
            
            cts.Cancel();
            cts.Dispose();
            yield return new WaitForSeconds(0.1f);
            taskInterval.Tick(1f);
            Assert.AreEqual(2, result);
            yield return null;
        }
    }
}