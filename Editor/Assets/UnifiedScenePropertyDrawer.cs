using UnityEditor;
using UnityEngine;

namespace Mew.Core.Assets
{
    [CustomPropertyDrawer(typeof(UnifiedScene))]
    public class UnifiedScenePropertyDrawer : PropertyDrawer
    {
        private const string SceneAssetReferenceKBackingField = "<SceneAssetReference>k__BackingField";
        private const string SceneReferenceKBackingField = "<SceneReference>k__BackingField";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sceneAssetReferenceProperty = property.FindPropertyRelative(SceneAssetReferenceKBackingField);
            var sceneReferenceProperty = property.FindPropertyRelative(SceneReferenceKBackingField);
            var sceneAssetReference = sceneAssetReferenceProperty.boxedValue as SceneAssetReference;
            var sceneReference = sceneReferenceProperty.boxedValue as SceneReference;

            var drawer = new LayoutDrawer();
            var isAssetReference = !string.IsNullOrEmpty(sceneAssetReference.AssetGUID);
            var isSceneReference = sceneReference.IsValid;

            var labelText = isAssetReference
                ? $"{label.text} (Addressables)"
                : isSceneReference
                    ? $"{label.text} : (Build Settings)"
                    : $"{label.text} : Select scene";
            var customLabel = new GUIContent(labelText);
            var originalColor = GUI.color;
            if (!isAssetReference && !isSceneReference)
                GUI.color = Color.yellow;
            drawer.DrawLabel(position, customLabel);

            EditorGUI.indentLevel++;
            if (isAssetReference)
            {
                drawer.DrawProperty(position, sceneAssetReferenceProperty);
            }
            else if (isSceneReference)
            {
                drawer.DrawProperty(position, sceneReferenceProperty);
            }
            else
            {
                drawer.DrawProperty(position, sceneAssetReferenceProperty);
                drawer.DrawProperty(position, sceneReferenceProperty);
            }
            EditorGUI.indentLevel--;
            if (!isAssetReference && !isSceneReference)
                GUI.color = originalColor;
        }

        private class LayoutDrawer
        {
            private float y = 0f;

            public void DrawProperty(Rect position, SerializedProperty property)
            {
                var rect = position;
                rect.y += y;
                rect.height = EditorGUI.GetPropertyHeight(property);;
                EditorGUI.PropertyField(rect, property);
                y += rect.height;
            }

            public void DrawLabel(Rect position, GUIContent text)
            {
                var rect = position;
                rect.y += y;
                rect.height = EditorGUI.GetPropertyHeight(SerializedPropertyType.String, text);
                GUI.Label(rect, text);
                y += rect.height;
            }
        }

        private void DrawProperty(Rect position, SerializedProperty property, GUIContent label)
        {
            var rect = position;
            EditorGUI.PropertyField(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var sceneAssetReferenceProperty = property.FindPropertyRelative(SceneAssetReferenceKBackingField);
            var sceneReferenceProperty = property.FindPropertyRelative(SceneReferenceKBackingField);
            var sceneAssetReference = sceneAssetReferenceProperty.boxedValue as SceneAssetReference;
            var sceneReference = sceneReferenceProperty.boxedValue as SceneReference;

            var fixedHeight = EditorGUI.GetPropertyHeight(SerializedPropertyType.String, label);

            if (!string.IsNullOrEmpty(sceneAssetReference.AssetGUID))
            {
                return fixedHeight +
                       EditorGUI.GetPropertyHeight(sceneAssetReferenceProperty);
            }
            else if (sceneReference.IsValid)
            {
                return fixedHeight +
                       EditorGUI.GetPropertyHeight(sceneReferenceProperty);
            }
            else
            {
                return fixedHeight +
                       EditorGUI.GetPropertyHeight(sceneAssetReferenceProperty) +
                       EditorGUI.GetPropertyHeight(sceneReferenceProperty);
            }
        }
    }
}