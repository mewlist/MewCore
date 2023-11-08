#nullable enable
namespace Mew.Core.Tasks
{
    public enum TaskQueueLimitType
    {
        None,
        /// <summary>
        /// Discard last less-or-equal priority task. Then enqueue.
        /// If all tasks have higher priority, discard new task. 
        /// </summary>
        SwapLast,
        /// <summary>
        /// Keep queue and discard new task.
        /// </summary>
        Discard
    }
}