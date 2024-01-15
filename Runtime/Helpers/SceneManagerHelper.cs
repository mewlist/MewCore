using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Mew.Core.SceneHelpers
{
    public static class SceneManagerHelper
    {
        public static List<Scene> GetLoadedScenes()
        {
            var loadedSceneCount = SceneManager.loadedSceneCount;
            var loadedScenes = new List<Scene>();
            for (var i=0; i<loadedSceneCount; i++)
                loadedScenes.Add(SceneManager.GetSceneAt(i));
            return loadedScenes;
        }
    }
}