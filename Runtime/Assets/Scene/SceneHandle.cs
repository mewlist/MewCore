﻿using System.Threading;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mew.Core.Assets
{
    public class SceneHandle : ISceneHandle
    {
        public AsyncOperation AsyncOp { get; }
        public Scene Scene { get; private set; }
        public float Progress => AsyncOp.progress;
        public bool Completed => AsyncOp.isDone;

        public SceneHandle(AsyncOperation asyncOp)
        {
            AsyncOp = asyncOp;
            AsyncOp.completed += OnCompleted;
        }

        public SceneHandle(Scene scene)
        {
            Scene = scene;
        }

        private void OnCompleted(AsyncOperation operation)
        {
            AsyncOp.completed -= OnCompleted;
            Scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        }

        public async ValueTask<Scene> GetScene(CancellationToken ct)
        {
            if (!AsyncOp.isDone)
            {
#if UNITY_2023_2_OR_NEWER
                await AsyncOp;
#else
                while (!AsyncOp.isDone) await TaskHelper.NextFrame(ct);
#endif
            }

            // not pass cancellation token to SceneManager.UnloadSceneAsync
            if (!Scene.IsValid()) await TaskHelper.NextFrame();

            if (ct.IsCancellationRequested)
            {
                await UnifiedSceneLoader.UnloadAsync(this);
                ct.ThrowIfCancellationRequested();
            }
            return Scene;
        }

        public bool Equals(Scene other)
        {
            return Scene.Equals(other);
        }
    }
}
