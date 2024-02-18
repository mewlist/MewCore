#if !UNITY_2023_2_OR_NEWER
using System.Threading.Tasks;

namespace Mew.Core.TaskHelpers
{
    internal static class TaskHelperLegacyInternal
    {
        public static async Task NextFrame()
        {
            TaskCompletionSource<bool> taskCompletionSource = new();
            MewLoop.Add<MewUnityUpdate>(OnUpdate);
            await taskCompletionSource.Task;
            MewLoop.Remove<MewUnityUpdate>(OnUpdate);
            return;

            void OnUpdate() => taskCompletionSource.TrySetResult(true);
        }
    }
}
#endif