using System.Threading.Tasks;

namespace Mew.Core.TaskHelpers
{
    public static class TaskHelper
    {
        public static async Task NextFrame()
        {
#if UNITY_2023_2_OR_NEWER
            await TaskHelperInternal.NextFrame();
#else
            await TaskHelperLegacyInternal.NextFrame();
#endif
        }
    }
}