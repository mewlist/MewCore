using System;
using System.Threading;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mew.Core.UnityObjectHelpers
{
    public static class UnityObjectHelper
    {
        public static T InstantiateComponentInSceneRoot<T>(Scene scene)
        {
            return (T)InstantiateComponentInSceneRoot(typeof(T), scene);
        }

        public static object InstantiateComponentInSceneRoot(Type targetType, Scene scene)
        {
            Assert.IsTrue(targetType.IsSubclassOf(typeof(MonoBehaviour)));

            var go = new GameObject(targetType.Name);
            var instance = go.AddComponent(targetType);
            SceneManager.MoveGameObjectToScene(instance.gameObject, scene);
            return instance;
        }

        public static object InstantiateComponentOnGameObject(Type targetType, GameObject on)
        {
            Assert.IsTrue(targetType.IsSubclassOf(typeof(MonoBehaviour)));

            return on.AddComponent(targetType);
        }

        public static object InstantiateComponentUnderTransform(Type targetType, Transform under, bool worldPositionStays)
        {
            Assert.IsTrue(targetType.IsSubclassOf(typeof(MonoBehaviour)));

            var go = new GameObject(targetType.Name);
            var instance = go.AddComponent(targetType);
            go.transform.SetParent(under, worldPositionStays: worldPositionStays);

            return instance;
        }

        public static async ValueTask DestroyAsync(Object target, CancellationToken ct = default)
        {
            Object.Destroy(target);
            switch (target)
            {
                case GameObject go:
                    while (go) await TaskHelper.NextFrame(ct);
                    break;
                case Component component:
                    while (component) await TaskHelper.NextFrame(ct);
                    break;
            }
        }
    }
}