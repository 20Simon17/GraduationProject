#if UNITY_NETWORKING
using Unity.Netcode;
#endif
using UnityEngine;
using UnityEngine.Serialization;

namespace SR.Shared
{
    /// <summary>
    ///     A static instance is similar to a singleton, but instead of destroying any new
    ///     instances, it overrides the current instance. This is handy for resetting the state
    ///     and saves you doing it manually.
    /// </summary>
    public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance;
        private static bool _autoCreateIfMissing = false;

        public static bool HasInstance => _instance != null;
        public static T TryGetInstance() => _instance;
        
        [Tooltip("If true, Instance getter will auto-create a GameObject with this component if missing.")]

        public static T Instance
        {
            get
            {
                if (_instance != null) 
                    return _instance;
                
                _instance = FindAnyObjectByType<T>(FindObjectsInactive.Exclude);
                
                if (_instance != null || !_autoCreateIfMissing) 
                    return _instance;
                
                var go = new GameObject(typeof(T).Name + " (Auto)")
                {
                    hideFlags = HideFlags.DontSave
                };
                _instance = go.AddComponent<T>();
#if UNITY_EDITOR
                Debug.Log($"[StaticInstance] Auto-created {typeof(T).Name}");
#endif
                
                return _instance;
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _instance = null;

        /// <summary>
        /// Initialize the singleton.
        /// When overriding in derived classes, always call base.Awake() first
        /// to ensure proper singleton initialization.
        /// </summary>
        protected virtual void Awake() => InitializeSingleton();

        protected virtual void InitializeSingleton()
        {
            if (!Application.isPlaying) return;
            if (_instance == null) _instance = this as T;
            else if (_instance != this) _instance = this as T; // StaticInstance overrides pointer, never destroys
        }
        
        protected virtual void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
        /// <summary>
        ///     Enable or disable auto-creation of the singleton instance if missing.
        /// </summary>
        protected static void EnableAutoCreate(bool enable = true) => _autoCreateIfMissing = enable;

    }

    /// <summary>
    ///     This transforms the static instance into a basic singleton.
    ///     This will destroy any new versions created, leaving the original instance intact.
    /// </summary>
    public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Initialize the singleton.
        /// When overriding in derived classes, always call base.Awake() first
        /// to ensure proper singleton initialization.
        /// </summary>
        protected override void InitializeSingleton()
        {
            if (!Application.isPlaying) return;
            
            if (_instance != null && _instance != this)
            {
                // Destroy only the duplicate component’s GO to avoid ghost instances
                Destroy(gameObject);
                return;
            }

            _instance = this as T;
        }
    }

    /// <summary>
    ///     A persistent version of the singleton. This will survive through scene loads.
    ///     Perfect for system classes which require stateful, persistent data. Or audio sources
    ///     where music plays through loading screens, etc.
    /// </summary>
    public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        /// <summary>
        /// When true, the GameObject will be unparented during initialization
        /// </summary>
        public bool autoUnparentOnAwake = true;

        /// <summary>
        /// Initialize the persistent singleton.
        /// When overriding in derived classes, always call base.Awake() first
        /// to ensure proper singleton initialization.
        /// </summary>
        protected override void InitializeSingleton()
        {
            if (!Application.isPlaying) return;
            
            if (autoUnparentOnAwake) transform.SetParent(null, true);

            // Make persistent before assigning to handle scene swaps
            DontDestroyOnLoad(gameObject);

            base.InitializeSingleton();
        }
    }

    /// <summary>
    /// Regulator singleton will destroy any older components of the same type it finds on awake
    /// Ensures that only the most recently created instance survives
    /// </summary>
    public abstract class RegulatorSingleton<T> : StaticInstance<T> where T : MonoBehaviour
    {
        public float InitializationTime { get; private set; }

        /// <summary>
        /// Initialize the regulator singleton.
        /// When overriding in derived classes, always call base.Awake() first
        /// to ensure proper singleton initialization.
        /// </summary>
        protected override void InitializeSingleton()
        {
            if (!Application.isPlaying) return;

            InitializationTime = Time.time;
            DontDestroyOnLoad(gameObject);

            // Find and destroy older instances
            var all = FindObjectsByType<T>(FindObjectsSortMode.None);
            foreach (var comp in all)
            {
                if (comp == this) continue;
                var reg = comp.GetComponent<RegulatorSingleton<T>>();
                if (reg != null && reg.InitializationTime < InitializationTime)
                {
                    // Prefer destroying only the component’s GO if you truly want exclusivity
                    Destroy(comp.gameObject);
                }
            }

            _instance = this as T;
        }
    }

#if UNITY_NETWORKING
    /// <summary>
    ///     Server-authoritative singleton for NGO.
    ///     Do not auto-create on clients. Instance is set on OnNetworkSpawn/Despawn.
    /// </summary>
    public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        protected static T _instance;
        
        public static bool HasInstance => _instance != null;
        public static T TryGetInstance() => _instance;

        public static T Instance => _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _instance = null;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            // The server spawns this object; clients receive it.
            _instance = (T)this;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (_instance == this) _instance = null;
        }

        /// <summary>
        /// Utility to create/spawn the singleton on the server.
        /// </summary>
        protected static T CreateAndSpawnServerInstance(string name = null)
        {
            if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer)
            {
                Debug.LogError($"[{typeof(T).Name}] CreateAndSpawnServerInstance called without server authority.");
                return null;
            }

            if (_instance) return _instance;

            var go = new GameObject(name ?? typeof(T).Name);
            var netObj = go.AddComponent<NetworkObject>();
            var comp = go.AddComponent<T>();

            netObj.Spawn(true); // Visible to clients
            return comp;
        }
    }
#endif
}
