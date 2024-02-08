#if USE_MEW_CORE_ASSETS
using UnityEngine;

namespace Mew.Core.Assets
{
    [CreateAssetMenu(menuName = "MewCore/AddressableSceneKey", fileName = "AddressableSceneKey", order = 0)]
    public class AddressablesSceneKey : ScriptableObject
    {
       [field: SerializeField] public string RuntimeKey { get; set; }
       public AddressablesSceneKey(string runtimeKey) { RuntimeKey = runtimeKey; }
    }
}
#endif