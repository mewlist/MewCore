#if USE_MEW_CORE_ASSETS
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Mew.Core.Assets
{
    public static class AddressablesHelper
    {
        public static async ValueTask<SceneInstance> LoadSceneAsync(SceneAssetReference sceneAssetReference)
        {
            var sceneInstance = await LoadSceneAsync(sceneAssetReference, new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D));
            return sceneInstance;
        }

        public static async ValueTask<SceneInstance> LoadSceneAsync(SceneAssetReference sceneAssetReference, LoadSceneParameters loadSceneParameters)
        {
            var handle = Addressables.LoadSceneAsync(sceneAssetReference, loadSceneParameters);
            return await handle.Task;
        }
    }
}
#endif