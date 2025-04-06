using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static UnityEditor.Progress;

public class CustomList<T> : MonoBehaviour where T : class
{
	public delegate void ClickEvent(T data);
    public delegate void ClickCountEvent(T data, int count);
    protected List<ListItem<T>> listItems = new List<ListItem<T>>();
    [Header("Grid")]
	[SerializeField]
	protected GridLayoutGroup grid;
    [Header("None Grid")]
    [SerializeField]
    protected Transform listParent;
    [SerializeField]
    private Vector2 itemSize;
    [SerializeField]
    private Vector2 itemSpace;
    [SerializeField]
    private int itemCountInRow;

    [SerializeField]
	protected GameObject itemObj;

    [SerializeField]
    protected ScrollRect scroll;

    [SerializeField]
    private bool isContentSizeFit;
    [SerializeField]
    private bool isContentSizeFitHorizontal;

    public List<T> GetDatas()
    {
        List<ListItem<T>> list = GetList();
        List<T> data = new List<T>();
        for(int i = 0;i < list.Count; i++)
        {
            data.Add(list[i].GetData());
        }
        return data;
    }

    public void SetScroll(float val)
    {
        if( scroll == null )
        {
            scroll = GetComponent<ScrollRect>();
        }
        if( scroll == null )
        {
            scroll = GetComponentInParent<ScrollRect>();
        }
        if (scroll != null)
        {
            if (scroll.verticalScrollbar != null) scroll.verticalScrollbar.value = val;
            if (scroll.horizontalScrollbar != null) scroll.horizontalScrollbar.value = val;

        }
    }

    public void MoveTo(int index)
    {
        if (scroll == null)
        {
            return;
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
        
        scroll.velocity = Vector2.zero;
        index = System.Math.Clamp(index, 0, scroll.content.childCount);
        var axis = scroll.vertical ? 1 : 0;
        var content = scroll.content.rect.size[axis];
        var viewport = scroll.viewport.rect.size[axis];
        var pos = -((RectTransform)scroll.content.GetChild(index)).anchoredPosition[axis] - viewport * 0.5f;
//         scroll.verticalNormalizedPosition = 1f - Mathf.Clamp(pos, 0f, content - viewport) / (content - viewport);
        var position = scroll.content.anchoredPosition;
        position[axis] = Mathf.Clamp(pos, 0f, content - viewport);
        scroll.content.anchoredPosition = position;
    }
    
	protected ClickEvent clickEvent;

	public void SetEvent(ClickEvent clickEvent)
	{
		this.clickEvent = clickEvent;
		for (int i = 0; i < listItems.Count; i++) {
			listItems [i].SetEvent (clickEvent);
		}
	}

    protected ClickCountEvent clickCountEvent;
    public void SetCountEvent(ClickCountEvent clickEvent)
    {
        this.clickCountEvent = clickEvent;
        for (int i = 0; i < listItems.Count; i++)
        {
            listItems[i].SetCountEvent(clickCountEvent);
        }
    }

    public List<ListItem<T>> GetList()
	{
		return listItems.GetRange (0,currentCount);
	}
	protected int currentCount;
    [SerializeField]
    private bool isDebug;

    public void Clear()
    {
        if( listParent.childCount <= 0 )
        {
            return;
        }
        for(int i = listParent.childCount-1; i >= 0; i--)
        {
            Destroy(listParent.GetChild(i).gameObject);
        }
        listItems.Clear();
    }
    public virtual void UpdateList(List<T> datas)
    {
        if (datas == null) return;

        if (grid == null)
        {
            //ListItem<T>[] childs = listParent.GetComponentsInChildren<ListItem<T>>();
            //if(childs.Length > listItems.Count)
            //{
            //    for(int i = 0; i < childs.Length; i++)
            //    {
            //        if(litItems[i])
            //    }
            //}
            if (listParent.childCount > listItems.Count)
            {
                for (int i = 0; i < listParent.childCount; i++)
                {
                    ListItem<T> item = listParent.GetChild(i).GetComponent<ListItem<T>>();
                    if (item != null && !listItems.Contains(item))
                    {
                        if (this.clickEvent != null)
                        {
                            item.SetEvent(this.clickEvent);
                        }
                        listItems.Add(item);
                    }
                }
            }
        }
        else
        {
            if (grid.transform.childCount > listItems.Count)
            {
                for (int i = 0; i < grid.transform.childCount; i++)
                {
                    ListItem<T> item = grid.transform.GetChild(i).GetComponent<ListItem<T>>();
                    if (!listItems.Contains(item))
                    {
                        if (this.clickEvent != null)
                        {
                            item.SetEvent(this.clickEvent);
                        }
                        listItems.Add(item);
                    }
                }
            }
        }
        currentCount = datas.Count;

        for (int i = 0; i < datas.Count; i++)
        {
            if (listItems.Count <= i)
            {
                GameObject go = Instantiate(itemObj, grid == null ? listParent : grid.transform);
                go.name = new StringBuilder().Append(itemObj.name).Append("_").Append(i).ToString();
                if (grid == null)
                {
                    ((RectTransform)go.transform).sizeDelta = itemSize;
                    if (itemCountInRow > 0)
                    {
                        Vector3 itemPosition = new Vector3((i % itemCountInRow), (i / itemCountInRow), 0f);
                        Vector3 vertice = new Vector3(1f, -1f, 1f);
                        ((RectTransform)go.transform).anchoredPosition = itemPosition * (itemSize + itemSpace) * vertice;
                    }
                }

                ListItem<T> item = go.GetComponent<ListItem<T>>();
                listItems.Add(item);
                if (this.clickEvent != null)
                {
                    item.SetEvent(this.clickEvent);
                }
                if (this.clickCountEvent != null)
                {
                    item.SetCountEvent(this.clickCountEvent);
                }
            }

            listItems[i].gameObject.SetActive(true);
            listItems[i].SetData(datas[i]);
        }

        if (datas.Count < listItems.Count)
        {
            for (int i = datas.Count; i < listItems.Count; i++)
            {
                listItems[i].gameObject.SetActive(false);
            }
        }
        //if(isContentSizeFit)
        //{
        //    grid.SetRectSizeHeight(datas.Count, itemCountInRow);
        //}
        //else
        //{
        //    if (isContentSizeFitHorizontal) grid.SetRectSizeWidth(datas.Count);
        //}
    } 

    public virtual void UpdateList(T[] datas)
    {
        if (datas == null) return;

        if (grid == null)
        {
            if (listParent.childCount > listItems.Count)
            {
                for (int i = 0; i < listParent.childCount; i++)
                {
                    ListItem<T> item = listParent.GetChild(i).GetComponent<ListItem<T>>();
                    if (!listItems.Contains(item))
                    {
                        if (this.clickEvent != null)
                        {
                            item.SetEvent(this.clickEvent);
                        }
                        if (this.clickCountEvent != null)
                        {
                            item.SetCountEvent(this.clickCountEvent);
                        }
                        listItems.Add(item);
                        
                    }
                }
            }
        }
        else
        {
            if (grid.transform.childCount > listItems.Count)
            {
                for (int i = 0; i < grid.transform.childCount; i++)
                {
                    ListItem<T> item = grid.transform.GetChild(i).GetComponent<ListItem<T>>();
                    if (!listItems.Contains(item))
                    {
                        if (this.clickEvent != null)
                        {
                            item.SetEvent(this.clickEvent);
                        }
                        if (this.clickCountEvent != null)
                        {
                            item.SetCountEvent(this.clickCountEvent);
                        }
                        listItems.Add(item);
                    }
                }
            }
        }

        currentCount = datas.Length;

        for (int i = 0; i < datas.Length; i++)
        {
            if (listItems.Count <= i)
            {
                GameObject go = Instantiate(itemObj, grid == null ? listParent : grid.transform);
                go.name = new StringBuilder().Append(itemObj.name).Append("_").Append(i).ToString();
                ListItem<T> item = go.GetComponent<ListItem<T>>();
                listItems.Add(item);
                if (this.clickEvent != null)
                {
                    item.SetEvent(this.clickEvent);
                }
                if (this.clickCountEvent != null)
                {
                    item.SetCountEvent(this.clickCountEvent);
                }
            }
            listItems[i].gameObject.SetActive(true);
            listItems[i].SetData(datas[i]);
        }

        if (datas.Length < listItems.Count)
        {
            for (int i = datas.Length; i < listItems.Count; i++)
            {
                listItems[i].gameObject.SetActive(false);
            }
        }

        //if (isContentSizeFit)
        //{
        //    grid.SetRectSizeHeight(datas.Length, itemCountInRow);
        //}
        //else
        //{
        //    if (isContentSizeFitHorizontal) grid.SetRectSizeWidth(datas.Length);
        //}

        //if (layoutGroup != null) layoutGroup.UpdateLayout();
    }

    public void UpdateCheck(List<T> datas)
    {
        for(int i = 0; i < listItems.Count; i++)
        {
            listItems[i].Check(datas);
        }
    }

    public void Active(bool active)
    {
        for( int i = 0; i < listItems.Count; i++ )
        {
            listItems[i].Active(active);
        }
    }

    public int currentIdx
    {
        get;
        private set;
    }

	public void Focus(int idx)
	{
        currentIdx = idx;
		if (listItems.Count > idx) {
			for (int i = 0; i < listItems.Count; i++) {
				listItems [i].Focus (idx >= 0 ? listItems[idx] : null);
			}
		}
	}
	public void Focus(ListItem<T> target)
	{
		for (int i = 0; i < listItems.Count; i++) {
            if(listItems[i] == target)
            {
                currentIdx = i;
            }
			listItems [i].Focus (target);
		}
	}

    public void Focus(T data)
    {
        for (int i = 0; i < listItems.Count; i++)
        {
            if( listItems[i].GetData() == data)
            {
                currentIdx = i;
            }
            listItems[i].Focus(data);
        }
    }

    public void Focus(bool focus)
    {
        for (int i = 0; i < listItems.Count; i++)
        {
            listItems[i].Focus(focus);
        }
    }

    public ListItem<T> GetItem(int idx)
    {
        if (listItems.Count > idx)
            return listItems[idx];
        return null;
    }

    public ListItem<T> GetItem(T data)
    {
        return listItems.SingleOrDefault(x => x.GetData() == data);
    }

    public int GetIdx(T data)
    {
        for (int i = 0; i < listItems.Count; i++)
        {
            if (listItems[i].GetData() == data)
            {
                return i;
            }
        }
        return -1;
    }
}

