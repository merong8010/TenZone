using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InfiniteScroll<T> : MonoBehaviour where T : class
{
    [SerializeField]
    protected RectTransform content;
    [SerializeField]
    protected GameObject itemPrefab;
    [SerializeField]
    protected int poolSize = 10;
    [SerializeField]
    private Rect padding;
    [SerializeField]
    private Vector2 gap;

    protected List<T> dataList;

    protected List<ListItem<T>> items = new List<ListItem<T>>();
    //protected RectTransform content;
    protected float itemHeight;
    protected int topIndex = 0;

    protected bool isInit = false;
    private void Init()
    {
        Debug.Log($"gameObject.name : {gameObject.name} | {isInit}");
        if (isInit) return;
        isInit = true;

        //content = scrollRect.content;
        itemHeight = ((RectTransform)itemPrefab.transform).sizeDelta.y + gap.y;
        Debug.Log("Initialize " + poolSize);
        // 초기화
        for (int i = 0; i < poolSize; i++)
        {
            var go = Instantiate(itemPrefab, content);
            var item = go.GetComponent<ListItem<T>>();
            //item.SetData(dataList[i]);
            item.rect.anchoredPosition = new Vector2(padding.x, -padding.y -(i * itemHeight));
            items.Add(item);
        }

        //scrollRect.onValueChanged.AddListener(_ => OnScroll());
    }
    //void Start()
    //{
    //    Initialize();
    //}

    public void UpdateList(List<T> datas)
    {
        Debug.Log("UpdateList!!");
        Debug.Log("UpdateList" + datas.Count);
        dataList = datas;
        Debug.Log("UpdateList" + dataList.Count+" | "+ content);
        //Debug.Log("UpdateList" + dataList.Count + " | " + scrollRect.content);

        //content = scrollRect.content;
        //Debug.Log("content" + content);
        itemHeight = ((RectTransform)itemPrefab.transform).sizeDelta.y + gap.y;
        Debug.Log("Initialize " + poolSize);
        // 초기화
        for (int i = 0; i < poolSize; i++)
        {
            var go = Instantiate(itemPrefab, content);
            var item = go.GetComponent<ListItem<T>>();
            //item.SetData(dataList[i]);
            item.rect.anchoredPosition = new Vector2(padding.x, -padding.y - (i * itemHeight));
            items.Add(item);
        }

        //Init();
        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetData(dataList[i]);
        }
        UpdateVisibleItems();
    }

    void OnScroll()
    {
        float scrollY = content.anchoredPosition.y;
        int newTopIndex = Mathf.FloorToInt(scrollY / itemHeight);
        if(newTopIndex < 0) newTopIndex = 0;
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
            var item = items[i];
            item.SetData(dataList[topIndex + i]);
            item.rect.anchoredPosition = new Vector2(padding.x, -padding.y-((i+topIndex) * itemHeight));
            
            item.gameObject.SetActive(true);
        }

        float contentHeight = dataList.Count * itemHeight;
        content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);
    }
}
