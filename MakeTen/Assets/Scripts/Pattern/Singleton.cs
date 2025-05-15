using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour {

	private static T _instance;
	public static T Instance
	{
		get {
			if (_instance == null) {
				_instance = FindAnyObjectByType(typeof(T)) as T;
				Debug.Log($"Instance {typeof(T)}");
				if (_instance == null) {
					T targetInstance = Resources.Load<T>(new System.Text.StringBuilder().Append("Prefabs/").Append(typeof(T).ToString()).ToString());
					if (targetInstance != null)
					{
						_instance = Instantiate(targetInstance);
						_instance.gameObject.name = _instance.GetType().FullName;
					}
				}
				if(_instance == null)
                {
					_instance = new GameObject(typeof(T).FullName).AddComponent<T>();
                }
			}
			return _instance;
		}
	}
	public bool DontDestroy;
	protected virtual void Awake()
	{
		if( _instance == null ) _instance = this as T;
		if (DontDestroy) DontDestroyOnLoad(this.gameObject);
	}

	public static bool HasInstance {
		get {
			return !IsDestroyed;
		}
	}

	public static bool IsDestroyed {
		get {
			if(_instance == null) {
				return true;
			} else {
				return false;
			}
		}
	}

	protected virtual void OnDestroy () {
		_instance = null;
	}

	protected virtual void OnApplicationQuit () {
		_instance = null;
	}
}
