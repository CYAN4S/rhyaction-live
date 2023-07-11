using System;
using UnityEngine;

namespace Core
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance is null)
                    throw new Exception($"There is no {typeof(T).Name}!");
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance is null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }
    }
}