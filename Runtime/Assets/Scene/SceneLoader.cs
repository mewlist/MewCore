using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Mew.Core.Assets
{
    public class SceneLoader : IAsyncDisposable
    {
        private List<ISceneHandle> SceneHandles { get; } = new();
        private bool Disposed { get; set; }

        public async ValueTask<Scene> LoadAsync(UnifiedScene unifiedScene, CancellationToken cancellationToken = default)
        {
            var handle = await UnifiedSceneLoader.LoadAsync(unifiedScene, cancellationToken);
            var scene = await handle.GetScene();

            if (Disposed)
            {
                await UnloadAsyncInternal(handle);
            }
            else if (cancellationToken.IsCancellationRequested)
            {
                await UnloadAsyncInternal(handle);
                throw new TaskCanceledException();
            }

            SceneHandles.Add(handle);
            return scene;
        }

        public async ValueTask UnloadAsync(Scene scene)
        {
            var targetHandle = SceneHandles.FirstOrDefault(x => x.Equals(scene));
            if (targetHandle is null)
                await UnloadAsyncInternal(new SceneHandle(scene));
            else
                await UnloadAsyncInternal(targetHandle);
        }

        private async ValueTask UnloadAsyncInternal(ISceneHandle targetHandle)
        {
            SceneHandles.Remove(targetHandle);
            await UnifiedSceneLoader.UnloadAsync(targetHandle);
        }

        public async ValueTask UnloadAllAsync()
        {
            var toRemove = SceneHandles.ToArray();
            await Task.WhenAll(
                toRemove.Select(x =>
                    UnloadAsyncInternal(x).AsTask()));
        }

        public async ValueTask DisposeAsync()
        {
            Disposed = true;
            await UnloadAllAsync();
        }
    }
}
