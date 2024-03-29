﻿using UnityEditor;
using UnityEngine;

namespace Mew.Core.Assets
{
    [CustomPropertyDrawer(typeof(UnifiedScene))]
    public class UnifiedScenePropertyDrawer : PropertyDrawer
    {
        private const string SceneAssetReferenceKBackingField = "<SceneAssetReference>k__BackingField";
        private const string SceneReferenceKBackingField = "<SceneReference>k__BackingField";
        private const string AddressableSceneKeyKBackingField = "<AddressablesSceneKey>k__BackingField";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
#if USE_MEW_CORE_ASSETS
            var sceneAssetReferenceProperty = property.FindPropertyRelative(SceneAssetReferenceKBackingField);
            var addressableSceneKeyProperty = property.FindPropertyRelative(AddressableSceneKeyKBackingField);
#endif
            var sceneReferenceProperty = property.FindPropertyRelative(SceneReferenceKBackingField);

#if USE_MEW_CORE_ASSETS
            var sceneAssetReference = sceneAssetReferenceProperty.boxedValue as SceneAssetReference;
            var addressableSceneKey = addressableSceneKeyProperty.boxedValue as AddressablesSceneKey;
#endif
            var sceneReference = sceneReferenceProperty.boxedValue as SceneReference;

#if USE_MEW_CORE_ASSETS
            var isAssetReference = !string.IsNullOrEmpty(sceneAssetReference.AssetGUID);
            var isAddressableSceneKey = addressableSceneKey is not null;
#else
            const bool isAssetReference = false;
            const bool isAddressableSceneKey = false;
#endif
            var isSceneReference = sceneReference.IsValid;

            var drawer = new LayoutDrawer();
            var labelText = (isAssetReference || isAddressableSceneKey)
                ? $"{label.text} (Addressables)"
                : isSceneReference
                    ? $"{label.text} : (Build Settings)"
                    : $"{label.text} : Select scene";
            var customLabel = new GUIContent(labelText);
            var originalColor = GUI.color;
            if (!isAssetReference && !isSceneReference && !isAddressableSceneKey)
                GUI.color = Color.yellow;
            drawer.DrawLabel(position, customLabel);

            EditorGUI.indentLevel++;

#if USE_MEW_CORE_ASSETS
            if (isAssetReference)
            {
                drawer.DrawProperty(position, sceneAssetReferenceProperty);
            }
            else if (isAddressableSceneKey)
            {
                drawer.DrawProperty(position, addressableSceneKeyProperty);
            }
            else if (isSceneReference)
            {
                drawer.DrawProperty(position, sceneReferenceProperty);
            }
            else
            {
                drawer.DrawProperty(position, sceneAssetReferenceProperty);
                drawer.DrawProperty(position, sceneReferenceProperty);
                drawer.DrawProperty(position, addressableSceneKeyProperty);
            }
#else
            drawer.DrawProperty(position, sceneReferenceProperty);
#endif

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
            var addressableSceneKeyProperty = property.FindPropertyRelative(AddressableSceneKeyKBackingField);
#if USE_MEW_CORE_ASSETS
            var sceneAssetReference = sceneAssetReferenceProperty.boxedValue as SceneAssetReference;
            var addressableSceneKey = addressableSceneKeyProperty.boxedValue as AddressablesSceneKey;
#endif
            var sceneReference = sceneReferenceProperty.boxedValue as SceneReference;

            var fixedHeight = EditorGUI.GetPropertyHeight(SerializedPropertyType.String, label);

            if (sceneReference.IsValid)
            {
                return fixedHeight +
                       EditorGUI.GetPropertyHeight(sceneReferenceProperty);
            }
#if USE_MEW_CORE_ASSETS
            else if (!string.IsNullOrEmpty(sceneAssetReference.AssetGUID))
            {
                return fixedHeight +
                       EditorGUI.GetPropertyHeight(sceneAssetReferenceProperty);
            }
            else if (addressableSceneKey is not null)
            {
                return fixedHeight +
                       EditorGUI.GetPropertyHeight(addressableSceneKeyProperty);
            }
#endif
            else
            {
#if USE_MEW_CORE_ASSETS
                return fixedHeight +
                       EditorGUI.GetPropertyHeight(sceneAssetReferenceProperty) +
                       EditorGUI.GetPropertyHeight(sceneReferenceProperty) +
                       EditorGUI.GetPropertyHeight(addressableSceneKeyProperty);
#else
                return fixedHeight +
                       EditorGUI.GetPropertyHeight(sceneReferenceProperty);
#endif
            }
        }
    }
}