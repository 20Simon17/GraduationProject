using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null && Application.isPlaying)
            {
                GameObject go = new GameObject("Singleton_" + typeof(T).Name);
                //DontDestroyOnLoad(go);
                _instance = go.AddComponent<T>();
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"Multiple instances of {typeof(T)} found. Destroying duplicate.");
            Destroy(gameObject);
        }
        else
        {
            _instance = this as T;
        }
    }
    
    //TODO: Fix the error upon exiting play
}