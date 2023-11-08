#nullable enable
namespace Mew.Core.Tasks
{
    public class TaskWithPriority
    {
        public TaskAction Func { get; }
        public int Priority { get; }

        public TaskWithPriority(TaskAction func, int priority)
        {
            Func = func;
            Priority = priority;
        }
    }
}