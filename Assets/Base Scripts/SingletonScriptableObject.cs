using UnityEngine;

namespace Base_Scripts
{
    public abstract class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<T>(typeof(T).Name);
                    if (instance == null)
                    {
                        Debug.LogError($"Singleton ScriptableObject of type {typeof(T).Name} not found in Resources.");
                    }
                }
                return instance;
            }
        }
    }
}