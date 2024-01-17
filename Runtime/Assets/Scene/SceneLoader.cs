using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
using UnityEngine;
using UnityEngine.SceneManagement;

#if USE_MEW_CORE_ASSETS
using UnityEngine.AddressableAssets;
#endif

namespace Mew.Core.Assets
{
    public class SceneLoader : IAsyncDisposable
    {
        private List<ISceneHandle> SceneHandles { get; } = new();
        private bool Disposed { get; set; }

        public async ValueTask<Scene> LoadAsync(UnifiedScene unifiedScene, CancellationToken cancellationToken = default)
        {
            var parameters = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
            ISceneHandle handle;

#if USE_MEW_CORE_ASSETS
            if (unifiedScene.SceneResourceLocation is not null)
            {
                var h = Addressables.LoadSceneAsync(unifiedScene.SceneResourceLocation, parameters);
                handle = new SceneInstanceHandle(h);
            }
            else if (unifiedScene.SceneAssetReference is not null)
            {
                var h = Addressables.LoadSceneAsync(unifiedScene.SceneAssetReference, parameters);
                handle = new SceneInstanceHandle(h);
            }
            else
#endif
            {
#if UNITY_2023_2_OR_NEWER
                await SceneManager.LoadSceneAsync(unifiedScene.SceneReference, parameters);
#else
                var asyncOp = SceneManager.LoadSceneAsync(unifiedScene.SceneReference, parameters);
                while (!asyncOp.isDone)
                    await TaskHelper.NextFrame();
#endif
                var loadedScene = SceneManager.GetSceneAt(SceneManager.loadedSceneCount - 1);
                handle = new SceneHandle(loadedScene);
            }

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
            await UnloadAsyncInternal(targetHandle);
        }

        private async ValueTask UnloadAsyncInternal(ISceneHandle targetHandle)
        {
            if (targetHandle is null)
                throw new ArgumentException("Scene is not loaded.");

            SceneHandles.Remove(targetHandle);

            switch (targetHandle)
            {
#if USE_MEW_CORE_ASSETS
                case SceneInstanceHandle sceneInstanceHandle:
                {
                    var handle = Addressables.UnloadSceneAsync(
                        sceneInstanceHandle.Handle.Result,
                        UnloadSceneOptions.UnloadAllEmbeddedSceneObjects,
                        true);
                    await handle.Task;
                    break;
                }
#endif
                case SceneHandle sceneHandle:
                {
                    if (sceneHandle.Scene.isLoaded)
                    {
#if UNITY_2023_2_OR_NEWER
                        await SceneManager.UnloadSceneAsync(sceneHandle.Scene);
#else
                        var asyncOp = SceneManager.UnloadSceneAsync(sceneHandle.Scene);
                        while (!asyncOp.isDone)
                            await TaskHelper.NextFrame();
#endif

                    }
                    break;
                }
            }
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
