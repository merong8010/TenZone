using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro; // UI 표시용

public class ObjectPooler : Singleton<ObjectPooler>
{
    [System.Serializable]
    public class Pool
    {
        public string tag;        // 오브젝트 태그
        public string prefabPath; // 프리팹 경로 (Resources 폴더 기준)
        public GameObject prefab; // 직접 등록하는 프리팹
        public int size;          // 초기 생성 개수
        public int maxPoolSize = 50; // 최대 풀 크기
        public float cleanupDelay = 10f; // 미사용 오브젝트 삭제 주기 (초)
        public float optimizeInterval = 30f; // 메모리 최적화 실행 주기
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, Pool> poolSettings;
    private Dictionary<string, List<GameObject>> activeObjects;

    public TMP_Text poolStatusText; // 현재 풀 상태를 표시할 UI 텍스트
    public bool isReady = false;
    protected override void Awake()
    {
        base.Awake();
        
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolSettings = new Dictionary<string, Pool>();
        activeObjects = new Dictionary<string, List<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            List<GameObject> activeList = new List<GameObject>();

            GameObject prefabToUse = pool.prefab;
            if (prefabToUse == null && !string.IsNullOrEmpty(pool.prefabPath))
            {
                prefabToUse = Resources.Load<GameObject>(pool.prefabPath);
                if (prefabToUse == null)
                {
                    Debug.LogError($"[ObjectPooler] '{pool.prefabPath}' 경로에서 프리팹을 찾을 수 없습니다.");
                    continue;
                }
            }

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(prefabToUse);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
            poolSettings.Add(pool.tag, pool);
            activeObjects.Add(pool.tag, activeList);

            StartCoroutine(CleanupPool(pool.tag, pool.cleanupDelay));
            StartCoroutine(OptimizeMemory(pool.tag, pool.optimizeInterval));
        }
        isReady = true;
    }

    /// <summary>
    /// Resources 폴더에서 프리팹을 동적으로 로딩하여 풀 생성
    /// </summary>
    private T CreateDynamicPool<T>(string path, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, float autoReturnTime) where T : MonoBehaviour
    {
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"[ObjectPooler] '{path}' 경로에서 프리팹을 찾을 수 없습니다.");
            return null;
        }

        Pool newPool = new Pool { tag = path, prefabPath = path, prefab = prefab, size = 5, maxPoolSize = 20, cleanupDelay = 10f };
        pools.Add(newPool);
        poolSettings[path] = newPool;
        poolDictionary[path] = new Queue<GameObject>();
        activeObjects[path] = new List<GameObject>();

        for (int i = 0; i < newPool.size; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(parent);
            poolDictionary[path].Enqueue(obj);
        }

        StartCoroutine(CleanupPool(path, newPool.cleanupDelay));

        return Get<T>(path, parent, position, scale, rotation, autoReturnTime);
    }

    /// <summary>
    /// 태그 또는 경로 기반으로 오브젝트 가져오기 (부족하면 자동 확장)
    /// </summary>
    public T Get<T>(string tagOrPath, Transform parent = null, Vector3 position = default(Vector3), Vector3 scale = default(Vector3), Quaternion rotation = default(Quaternion), float autoReturnTime = 0f) where T : MonoBehaviour
    {
        if (!pools.Exists(x => x.tag == tagOrPath)) return null;
        string tag = tagOrPath;
        if (!poolDictionary.ContainsKey(tag))
        {
            return CreateDynamicPool<T>(tagOrPath, parent, position, scale,  rotation, autoReturnTime);
        }

        
        if(poolDictionary[tag].Count == 0)
        {
            if(!ExpandPool(tag))
            {
                return null;
            }
        }

        GameObject obj = poolDictionary[tag].Dequeue();
        obj.SetActive(true);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = position;
        obj.transform.localScale = scale == default(Vector3) ? Vector3.one : scale;
        obj.transform.rotation = rotation;

        activeObjects[tag].Add(obj);
        UpdatePoolStatusUI();

        if (autoReturnTime > 0f)
        {
            StartCoroutine(AutoReturnObject(tag, obj, autoReturnTime));
        }

        T component = obj.GetComponent<T>();
        if (component == null)
        {
            Debug.LogWarning($"The pooled object with tag {tag} does not have component of type {typeof(T)}.");
        }

        return component;
    }

    /// <summary>
    /// 오브젝트를 풀에 반환하기
    /// </summary>
    public void ReturnObject(string tag, GameObject obj)
    {
        if(obj == null) return;
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[ObjectPooler] {tag} 태그의 오브젝트 풀이 존재하지 않습니다.");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
        activeObjects[tag].Remove(obj);
        UpdatePoolStatusUI();
    }

    /// <summary>
    /// 부족할 경우 자동 확장
    /// </summary>
    private bool ExpandPool(string tag)
    {
        if (!poolSettings.ContainsKey(tag))
        {
            Debug.LogWarning($"[ObjectPooler] {tag} 태그의 오브젝트 설정을 찾을 수 없습니다.");
            return false;
        }

        Pool pool = poolSettings[tag];

        if (poolDictionary[tag].Count + activeObjects[tag].Count >= pool.maxPoolSize)
        {
            Debug.LogWarning($"[ObjectPooler] {tag} 풀의 최대 크기({pool.maxPoolSize})에 도달하여 확장할 수 없습니다.");
            return false;
        }

        
        GameObject obj = Instantiate(pool.prefab);
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        poolDictionary[tag].Enqueue(obj);
        return true;
    }

    /// <summary>
    /// 일정 시간 후 오브젝트 자동 반환
    /// </summary>
    private IEnumerator AutoReturnObject(string tag, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnObject(tag, obj);
    }

    /// <summary>
    /// 일정 시간 동안 사용되지 않은 오브젝트 삭제
    /// </summary>
    private IEnumerator CleanupPool(string tag, float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);

            if (!poolDictionary.ContainsKey(tag)) continue;

            Queue<GameObject> newQueue = new Queue<GameObject>();
            foreach (var obj in poolDictionary[tag])
            {
                if (!obj.activeSelf)
                {
                    Destroy(obj);
                }
                else
                {
                    newQueue.Enqueue(obj);
                }
            }

            poolDictionary[tag] = newQueue;
            UpdatePoolStatusUI();
        }
    }

    /// <summary>
    /// 메모리 자동 최적화: 오래된 오브젝트 정리
    /// </summary>
    private IEnumerator OptimizeMemory(string tag, float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            if (!poolDictionary.ContainsKey(tag)) continue;

            Pool pool = poolSettings[tag];

            if (poolDictionary[tag].Count > pool.size)
            {
                int excess = poolDictionary[tag].Count - pool.size;
                for (int i = 0; i < excess; i++)
                {
                    GameObject obj = poolDictionary[tag].Dequeue();
                    Destroy(obj);
                }
            }

            UpdatePoolStatusUI();
        }
    }

    /// <summary>
    /// 현재 풀 상태를 UI에 표시
    /// </summary>
    private void UpdatePoolStatusUI()
    {
        if (poolStatusText == null) return;

        string status = "Object Pool Status:\n";
        foreach (var pool in pools)
        {
            int activeCount = activeObjects[pool.tag].Count;
            int availableCount = poolDictionary[pool.tag].Count;
            status += $"{pool.tag}: Active {activeCount} / Available {availableCount} / Max {pool.maxPoolSize}\n";
        }

        poolStatusText.text = status;
    }
}
