using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

#if USE_MEW_CORE_ASSETS
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif

namespace Mew.Core.Assets
{
    public class SceneLocationFinder
    {
        private readonly List<UnifiedScene> scenes = new();
        public string AddressablesKey { get; set; }

        public async Task Initialize()
        {
            scenes.AddRange(GetSceneAssets());
            scenes.AddRange(await GetSceneAssetReferences());
        }

        public UnifiedScene FindByScene(Scene scene)
        {
            foreach (var unifiedScene in scenes)
            {
#if USE_MEW_CORE_ASSETS
                if (unifiedScene.IsSceneAssetReference)
                {
                    if (unifiedScene.SceneAssetReference.editorAsset.name == scene.name)
                        return unifiedScene;
                }
                else if (unifiedScene.IsSceneResourceLocation)
                {
                    if (unifiedScene.SceneResourceLocation.InternalId.EndsWith($"/{scene.name}.unity"))
                        return unifiedScene;
                }
#endif
                if (unifiedScene.IsSceneReference)
                {
                    if (unifiedScene.SceneReference.SceneName == scene.name)
                        return unifiedScene;
                }
            }
            return null;
        }

        private static IEnumerable<UnifiedScene> GetSceneAssets()
        {
            var unifiedScenes = new List<UnifiedScene>();
            for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var unifiedScene = new UnifiedScene
                {
                    SceneReference = new SceneReference
                        { SceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)) }
                };
                unifiedScenes.Add(unifiedScene);
            }
            return unifiedScenes;
        }

        private async ValueTask<List<UnifiedScene>> GetSceneAssetReferences()
        {
#if USE_MEW_CORE_ASSETS
            var handle = Addressables.LoadResourceLocationsAsync(AddressablesKey, typeof(SceneInstance));
            var resourceLocations = await handle.Task;
            return resourceLocations
                .Select(x => new UnifiedScene { SceneResourceLocation = x })
                .ToList();
#else
            return await Task.FromResult(new List<UnifiedScene>());
#endif
        }
    }
}