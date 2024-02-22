using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
using UnityEngine;
using UnityEngine.SceneManagement;

#if USE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Mew.Core.Assets
{
    internal static class CompatibleSceneLoader
    {
        internal static async ValueTask UnloadSceneAsync(SceneHandle sceneHandle)
        {
#if UNITY_2023_2_OR_NEWER || USE_UNITASK
            await SceneManager.UnloadSceneAsync(sceneHandle.Scene);
#else
            var asyncOp = SceneManager.UnloadSceneAsync(sceneHandle.Scene);
            while (!asyncOp.isDone) await TaskHelper.NextFrame();
#endif
        }
    }
}