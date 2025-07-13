#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace Mew.Core.Tasks
{
    public delegate ValueTask TaskAction(CancellationToken ct);
}