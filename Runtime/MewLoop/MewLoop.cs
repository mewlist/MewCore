#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mew.Core
{
    public class MewLoop
    {
        private static readonly ConcurrentDictionary<string, MewLoopDelegateCollection> DelegateCollections = new();
        private static readonly MewLoopDelegateCollection DefaultDelegateCollection = new();

        private static string? DefaultId { get; set; }

        private static ConcurrentDictionary<Type, string> loopIdMap = new();

        public static string LoopId<T>()
        {
            var type = typeof(T);
            if (loopIdMap.TryGetValue(type, out var id))
                return id;
            id = type.Name;
            loopIdMap[type] = id;
            return id;
        }

        /// <summary>
        /// Register delegate collection for id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="delegateCollection"></param>
        public static void Register(string id, MewLoopDelegateCollection delegateCollection)
        {
            DelegateCollections[id] = delegateCollection;
        }

        /// <summary>
        /// Register delegate collection for type T.
        /// </summary>
        /// <param name="delegateCollection"></param>
        /// <typeparam name="T"></typeparam>
        public static void Register<T>()
        {
            var id = LoopId<T>();
            if (DelegateCollections.ContainsKey(id))
                throw new Exception($"DelegateCollection with id {id} is already registered.");

            if (DefaultId == id)
                DelegateCollections[id] = DefaultDelegateCollection;
            else
                DelegateCollections[id] = new MewLoopDelegateCollection();
        }

        /// <summary>
        /// Add update callback function to default delegate collection.
        /// Default delegate collection is set through SetDefaultCollection().
        /// </summary>
        /// <param name="updateFunction"></param>
        /// <exception cref="NullReferenceException"></exception>
        public static void Add(MewLoopDelegateCollection.UpdateFunction updateFunction)
        {
            DefaultDelegateCollection.Add(updateFunction);
        }

        /// <summary>
        /// Add update callback function to delegate collection for type T.
        /// </summary>
        /// <param name="updateFunction"></param>
        /// <typeparam name="T"></typeparam>
        public static void Add<T>(MewLoopDelegateCollection.UpdateFunction updateFunction)
        {
            Add(LoopId<T>(), updateFunction);
        }

        /// <summary>
        /// Add update callback function to delegate collection for id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateFunction"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        public static void Add(string id, MewLoopDelegateCollection.UpdateFunction updateFunction)
        {
            if (!DelegateCollections.ContainsKey(id))
                throw new KeyNotFoundException($"DelegateCollection with id {DefaultId} is not found.");
            DelegateCollections[id].Add(updateFunction);
        }

        /// <summary>
        /// Remove update callback function from default delegate collection.
        /// </summary>
        /// <param name="updateFunction"></param>
        /// <exception cref="NullReferenceException"></exception>
        public static void Remove(MewLoopDelegateCollection.UpdateFunction updateFunction)
        {
            DefaultDelegateCollection.Remove(updateFunction);
        }

        /// <summary>
        /// Remove update callback function from delegate collection for type T.
        /// </summary>
        /// <param name="updateFunction"></param>
        /// <typeparam name="T"></typeparam>
        public static void Remove<T>(MewLoopDelegateCollection.UpdateFunction updateFunction)
        {
            Remove(LoopId<T>(), updateFunction);
        }

        /// <summary>
        /// Remove update callback function from delegate collection for id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateFunction"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        public static void Remove(string id, MewLoopDelegateCollection.UpdateFunction updateFunction)
        {
            if (!DelegateCollections.ContainsKey(id))
                throw new KeyNotFoundException($"DelegateCollection with id {DefaultId} is not found.");
            DelegateCollections[id].Remove(updateFunction);
        }

        /// <summary>
        /// Invoke update callback functions in default delegate collection.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public static void Update()
        {
            if (string.IsNullOrEmpty(DefaultId))
                throw new NullReferenceException("DefaultId is null or empty.");
            Update(DefaultId);
        }

        /// <summary>
        /// Invoke update callback functions in delegate collection for type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Update<T>()
        {
            Update(LoopId<T>());
        }

        /// <summary>
        /// Invoke update callback functions in delegate collection for id.
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        public static void Update(string id)
        {
            if (!DelegateCollections.ContainsKey(id))
                throw new KeyNotFoundException($"DelegateCollection with id {DefaultId} is not found.");
            DelegateCollections[id].Invoke();
        }

        /// <summary>
        /// Set default delegate collection id as T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void SetDefaultCollection<T>()
        {
            SetDefaultCollection(LoopId<T>());
        }

        /// <summary>
        /// Set default delegate collection id.
        /// </summary>
        /// <param name="id"></param>
        public static void SetDefaultCollection(string? id)
        {
            DefaultId = id;
        }
    }
}