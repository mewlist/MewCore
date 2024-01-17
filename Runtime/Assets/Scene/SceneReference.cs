using System;
using UnityEngine;

namespace Mew.Core.Assets
{
    [Serializable]
    public class SceneReference
    {
        [field: SerializeField] public string SceneName { get; set; }

        public bool IsValid => !string.IsNullOrEmpty(SceneName);

        public static implicit operator string(SceneReference sceneObject)
            => sceneObject.SceneName;

        public static implicit operator SceneReference(string sceneName)
            => new() { SceneName = sceneName };
    }
}