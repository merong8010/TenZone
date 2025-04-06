using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] {typeof(T)} 인스턴스가 이미 파괴되었습니다. 새로 생성되지 않습니다.");
                return null;
            }

            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance = FindFirstObjectByType<T>();

                    // 🎯 에디터 모드에서도 동작하게 설정
#if UNITY_EDITOR
                    if (!Application.isPlaying && _instance == null)
                    {
                        _instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
                    }
#endif

                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).ToString());
                        _instance = singletonObject.AddComponent<T>();
                        //DontDestroyOnLoad(singletonObject);
                    }
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            //DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}
