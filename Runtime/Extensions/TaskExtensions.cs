using System;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mew.Core.Extensions
{
    public static class TaskExtensions
    {
        public static void Forget(this Task task)
        {
            ForgetInternal(task);
        }

        public static void Forget(this ValueTask task)
        {
            ForgetInternal(task);
        }

        private static async void ForgetInternal(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static async void ForgetInternal(ValueTask task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}