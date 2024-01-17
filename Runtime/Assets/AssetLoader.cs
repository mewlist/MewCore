#if USE_MEW_CORE_ASSETS
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Mew.Core.Assets
{
    public class AssetLoader : IDisposable
    {
        private ConcurrentBag<AsyncOperationHandle> Handles { get; } = new();

        public async ValueTask<T> LoadAsync<T>(object key, CancellationToken ct)
            where T: Object
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            await handle.Task;
            if (ct.IsCancellationRequested)
            {
                Addressables.Release(handle);
                return null;
            }
            Handles.Add(handle);
            return handle.Result;
        }

        public void Dispose()
        {
            foreach (var handle in Handles)
                Addressables.Release(handle);
            Handles.Clear();
        }
    }
}
#endif