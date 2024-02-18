using System;
using System.Threading;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
using UnityEngine.SceneManagement;

#if USE_MEW_CORE_ASSETS
using UnityEngine.AddressableAssets;
#endif

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Mew.Core.Assets
{
    public static class UnifiedSceneLoader
    {
        public static async Task<ISceneHandle> LoadAsync(UnifiedScene unifiedScene, CancellationToken cancellationToken)
        {
            var parameters = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.None);
            ISceneHandle handle;

#if USE_MEW_CORE_ASSETS
            if (unifiedScene.SceneResourceLocation is not null)
            {
                var h = Addressables.LoadSceneAsync(unifiedScene.SceneResourceLocation, parameters);
                handle = new SceneInstanceHandle(h);
            }
            else if (unifiedScene.SceneAssetReference is not null && unifiedScene.SceneAssetReference.RuntimeKeyIsValid())
            {
                var h = Addressables.LoadSceneAsync(unifiedScene.SceneAssetReference, parameters);
                handle = new SceneInstanceHandle(h);
            }
            else if (unifiedScene.IsAddressablesSceneKey)
            {
                var h = Addressables.LoadSceneAsync(unifiedScene.AddressablesSceneKey.RuntimeKey, parameters);
                handle = new SceneInstanceHandle(h);
            }
            else
#endif
            if (unifiedScene.SceneReference is not null && unifiedScene.SceneReference.IsValid)
            {
                handle = await CompatibleSceneLoader.LoadSceneAsync(unifiedScene, parameters);
            }
#if UNITY_EDITOR
            // for test use
            else if (!string.IsNullOrEmpty(unifiedScene.EditorScenePath))
            {
                var asyncOp = EditorSceneManager.LoadSceneAsyncInPlayMode(unifiedScene.EditorScenePath , parameters ) ;
                while (!asyncOp.isDone)
                    await TaskHelper.NextFrame();
                var loadedScene = SceneManager.GetSceneAt(SceneManager.loadedSceneCount - 1);
                handle = new SceneHandle(loadedScene);
            }
#endif
            else
            {
                throw new ArgumentException("SceneReference is not valid.");
            }

            return handle;
        }

        public static async ValueTask UnloadAsync(ISceneHandle targetHandle)
        {
            if (targetHandle is null)
                throw new ArgumentException("Scene is not loaded.");

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
                        await CompatibleSceneLoader.UnloadSceneAsync(sceneHandle);
                    break;
                }
            }
        }
    }
}