using System.Threading;
using System.Threading.Tasks;

namespace Mew.Core.TaskHelpers
{
    public static class TaskHelper
    {
        public static async Task NextFrame(CancellationToken ct = default)
        {
            await TaskHelperInternal.NextFrame(ct);
        }
    }
}