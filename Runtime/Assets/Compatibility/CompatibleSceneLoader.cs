using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
using UnityEngine;
using UnityEngine.SceneManagement;

#if USE_UNITASK
using Cysharp.Threading.Tasks;
#endif

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Mew.Core.Assets
{
    internal static class CompatibleSceneLoader
    {
        internal static async ValueTask<SceneHandle> LoadSceneAsync(UnifiedScene unifiedScene, LoadSceneParameters parameters)
        {
#if UNITY_2023_2_OR_NEWER || USE_UNITASK
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
#if UNITY_2023_2_OR_NEWER || USE_UNITASK
            await SceneManager.UnloadSceneAsync(sceneHandle.Scene);
#else
            var asyncOp = SceneManager.UnloadSceneAsync(sceneHandle.Scene);
            while (!asyncOp.isDone) await TaskHelper.NextFrame();
#endif
        }

#if UNITY_EDITOR
        public static async Task<ISceneHandle> LoadEditorSceneAsync(UnifiedScene unifiedScene, LoadSceneParameters parameters)
        {

#if UNITY_2023_2_OR_NEWER || USE_UNITASK
            await EditorSceneManager.LoadSceneAsyncInPlayMode(unifiedScene.EditorScenePath , parameters ) ;
#else
            var asyncOp = EditorSceneManager.LoadSceneAsyncInPlayMode(unifiedScene.EditorScenePath , parameters ) ;
            while (!asyncOp.isDone) await TaskHelper.NextFrame();
#endif
            var loadedScene = SceneManager.GetSceneAt(SceneManager.loadedSceneCount - 1);
            return new SceneHandle(loadedScene);
        }
#endif
    }
}