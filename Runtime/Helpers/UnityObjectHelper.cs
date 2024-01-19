﻿using System;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
using Unity.Collections;
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

            using var instanceIds = new NativeArray<int>( new [] { instance.gameObject.GetInstanceID() }, Allocator.Temp);
            SceneManager.MoveGameObjectsToScene(instanceIds, scene);

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

        public static async ValueTask DestroyAsync(Object target)
        {
            Object.Destroy(target);
            if (target is GameObject go)
                while (go) await TaskHelper.NextFrame();
            else if (target is Component component)
                while (component) await TaskHelper.NextFrame();
        }
    }
}