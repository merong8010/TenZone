using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InfiniteScroll<T> : MonoBehaviour where T : ListItem<T>
{
    public ScrollRect scrollRect;
    public GameObject itemPrefab;
    public int poolSize = 10;
    public List<T> dataList;

    private Queue<T> itemPool = new Queue<T>();
    private RectTransform content;
    private float itemHeight;
    private int topIndex = 0;

    private bool isInit = false;
    private void Initialize()
    {
        if (isInit) return;
        isInit = true;

        content = scrollRect.content;
        itemHeight = ((RectTransform)itemPrefab.transform).sizeDelta.y;

        // 초기화
        for (int i = 0; i < poolSize; i++)
        {
            var go = Instantiate(itemPrefab, content);
            var item = go.GetComponent<T>();
            itemPool.Enqueue(item);
        }

        UpdateVisibleItems();
        scrollRect.onValueChanged.AddListener(_ => OnScroll());
    }
    void Start()
    {
        Initialize();
    }

    void OnScroll()
    {
        float scrollY = content.anchoredPosition.y;
        int newTopIndex = Mathf.FloorToInt(scrollY / itemHeight);

        if (newTopIndex != topIndex)
        {
            topIndex = newTopIndex;
            UpdateVisibleItems();
        }
    }

    void UpdateVisibleItems()
    {
        int visibleCount = Mathf.Min(poolSize, dataList.Count - topIndex);

        foreach (Transform child in content)
            child.gameObject.SetActive(false);

        for (int i = 0; i < visibleCount; i++)
        {
            var item = itemPool.Dequeue();
            item.SetData(dataList[topIndex + i]);
            item.transform.SetSiblingIndex(i);
            item.gameObject.SetActive(true);
            itemPool.Enqueue(item);
        }

        float contentHeight = dataList.Count * itemHeight;
        content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);
    }
}
