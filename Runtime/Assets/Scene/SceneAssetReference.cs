#if USE_MEW_CORE_ASSETS
using System;
using UnityEngine.AddressableAssets;

namespace Mew.Core.Assets
{
    [Serializable]
    public class SceneAssetReference : AssetReference {

        public SceneAssetReference(string guid) : base(guid) {}
        public override bool ValidateAsset(string path) { return path.EndsWith(".unity"); }
    }
}
#endif
