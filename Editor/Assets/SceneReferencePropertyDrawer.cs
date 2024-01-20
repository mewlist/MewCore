using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mew.Core.Assets
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferencePropertyDrawer : PropertyDrawer
    {
        private SceneAsset FindSceneAsset(string sceneName)
        {
            var targetScene = EditorBuildSettings.scenes.FirstOrDefault(x
                => Path.GetFileNameWithoutExtension(x.path) == sceneName);
            return targetScene != null
                ? targetScene.enabled
                    ? AssetDatabase.LoadAssetAtPath(targetScene.path, typeof(SceneAsset)) as SceneAsset
                    : null
                : null;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const string fieldName = "<SceneName>k__BackingField";
            var serializedProperty = property.FindPropertyRelative(fieldName);
            var sceneName = property.FindPropertyRelative(fieldName).stringValue;
            var sceneAsset = FindSceneAsset(sceneName);
            sceneAsset = (SceneAsset) EditorGUI.ObjectField(position, label, sceneAsset, typeof(SceneAsset), false);

            if (!string.IsNullOrEmpty(serializedProperty.stringValue) && sceneAsset == null)
            {
                serializedProperty.stringValue = string.Empty;
            }
            else if (sceneAsset is not null && sceneAsset.name != serializedProperty.stringValue)
            {
                if (FindSceneAsset(sceneAsset.name) is null)
                {
                    Debug.LogWarning($"Scene {sceneAsset.name} is not found in build settings.");
                    serializedProperty.stringValue = string.Empty;
                }
                serializedProperty.stringValue = sceneAsset.name;
            }
        }
    }
}