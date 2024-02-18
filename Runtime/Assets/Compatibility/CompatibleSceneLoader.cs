using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mew.Core.Assets
{
    internal static class CompatibleSceneLoader
    {
        internal static async ValueTask<SceneHandle> LoadSceneAsync(UnifiedScene unifiedScene, LoadSceneParameters parameters)
        {
#if UNITY_2023_2_OR_NEWER
            await SceneManager.LoadSceneAsync(unifiedScene.SceneReference, parameters);
#else
                var asyncOp = SceneManager.LoadSceneAsync(unifiedScene.SceneReference, parameters);
                while (!asyncOp.isDone) await TaskHelper.NextFrame();
#endif
            var loadedScene = SceneManager.GetSceneAt(SceneManager.loadedSceneCount - 1);
            return new SceneHandle(loadedScene);
        }

        internal static async ValueTask UnloadSceneAsync(SceneHandle sceneHandle)
        {
#if UNITY_2023_2_OR_NEWER
            await SceneManager.UnloadSceneAsync(sceneHandle.Scene);
#else
            var asyncOp = SceneManager.UnloadSceneAsync(sceneHandle.Scene);
            while (!asyncOp.isDone) await TaskHelper.NextFrame();
#endif
        }
    }
}