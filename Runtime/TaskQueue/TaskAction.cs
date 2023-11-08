#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace Mew.Core.Tasks
{
    public delegate Task TaskAction(CancellationToken ct);
}