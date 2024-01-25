﻿using System;
using UnityEngine;
#if USE_MEW_CORE_ASSETS
using UnityEngine.ResourceManagement.ResourceLocations;
#endif

namespace Mew.Core.Assets
{
    [Serializable]
    public class UnifiedScene
    {
#if USE_MEW_CORE_ASSETS
        [field: SerializeField] public SceneAssetReference SceneAssetReference { get; set; }
        public IResourceLocation SceneResourceLocation { get; set; }
#endif
        [field: SerializeField] public SceneReference SceneReference { get; set; }

#if UNITY_EDITOR
        [field: SerializeField] public string EditorScenePath { get; set; }
#endif

        public bool IsSceneReference => SceneReference?.IsValid ?? false;
        public bool IsSceneAssetReference
        {
            get
            {
#if USE_MEW_CORE_ASSETS
                return SceneAssetReference != null && SceneAssetReference.RuntimeKeyIsValid();
#else
                return false;
#endif
            }
        }

        public bool IsSceneResourceLocation
        {
            get
            {
#if USE_MEW_CORE_ASSETS
                return SceneResourceLocation != null;
#else
                return false;
#endif
            }
        }

        private bool IsValidInternal => IsSceneReference || IsSceneAssetReference || IsSceneResourceLocation;
#if UNITY_EDITOR
            public bool IsValid => IsValidInternal || !string.IsNullOrEmpty(EditorScenePath);
#else
        public bool IsValid => IsValidInternal;
#endif
    }
}