using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mew.Core.Assets
{
    public class SceneLoader : IAsyncDisposable
    {
        private List<ISceneHandle> SceneHandles { get; } = new();
        private List<ISceneHandle> LoadingHandles { get; } = new();
        private bool Disposed { get; set; }
        public float Progression
            => LoadingHandles.Any()
                ? LoadingHandles.Sum(x => x.Progress) / LoadingHandles.Count
                : 1f;

        public async ValueTask<Scene> LoadAsync(UnifiedScene unifiedScene, CancellationToken cancellationToken = default)
        {
            var handle = UnifiedSceneLoader.LoadAsync(unifiedScene);
            LoadingHandles.Add(handle);
            var scene = await handle.GetScene(cancellationToken);

            if (Disposed)
            {
                await UnloadAsyncInternal(handle);
            }
            else if (cancellationToken.IsCancellationRequested)
            {
                await UnloadAsyncInternal(handle);
                throw new TaskCanceledException();
            }

            LoadingHandles.Remove(handle);
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
            await Task.WhenAll(SceneHandles.Select(x =>
                    UnifiedSceneLoader.UnloadAsync(x).AsTask()));
            SceneHandles.Clear();
        }

        public async ValueTask DisposeAsync()
        {
            Disposed = true;
            await UnloadAllAsync();
        }
    }
}
