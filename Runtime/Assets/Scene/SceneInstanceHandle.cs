#if USE_MEW_CORE_ASSETS
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Mew.Core.Assets
{
    public class SceneInstanceHandle : ISceneHandle
    {
        public AsyncOperationHandle<SceneInstance> Handle { get; }
        public float Progress => Handle.PercentComplete;
        public bool Completed => Handle.IsDone;

        public SceneInstanceHandle(AsyncOperationHandle<SceneInstance> asyncOperationHandle)
        {
            Handle = asyncOperationHandle;
        }

        public async ValueTask<Scene> GetScene(CancellationToken ct)
        {
            if (!Handle.IsDone)
                await Handle.Task;

            if (ct.IsCancellationRequested)
            {
                await UnifiedSceneLoader.UnloadAsync(this);
                ct.ThrowIfCancellationRequested();
            }

            return Handle.Result.Scene;
        }

        public bool Equals(Scene other)
        {
            if (!Handle.IsDone)
                throw new OperationException("SceneInstanceHandle is not completed.");
            return Handle.Result.Scene == other;
        }

        public static implicit operator AsyncOperationHandle<SceneInstance>(SceneInstanceHandle handle)
        {
            return handle.Handle;
        }
    }
}
#endif