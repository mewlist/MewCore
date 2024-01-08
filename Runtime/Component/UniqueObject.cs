using System;
using UnityEngine;

namespace Mew.Core.Components
{
    public abstract class UniqueObject<T> : MonoBehaviour
        where T: MonoBehaviour
    {
        protected static T instance;

        public static T Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = FindFirstObjectByType<T>();
                if (instance != null) return instance;
                throw new NullReferenceException("No UniqueObject instance found.");
            }
        }

        protected virtual void Awake()
        {
            if (instance == null) instance = GetComponent<T>();
            else if (instance != this) Destroy(gameObject);
        }

        protected void Destroy()
        {
            instance = null;
            if (this) Destroy(gameObject);
        }
    }
}