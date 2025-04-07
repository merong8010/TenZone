using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class ListItem<T> : MonoBehaviour where T : class
{
	protected T data;
	public RectTransform rect
	{
		get => (RectTransform)transform;
	}
	public T GetData()
	{
		return data;
	}

	[SerializeField]
	private UnityEngine.UI.Text[] fontUpdateTexts;
	//private Locale lastUpdateLocale;

	public virtual void SetData(T data)
	{
		//if (lastUpdateLocale != TextManager.Instance.locale)
		//{
		//	if (fontUpdateTexts != null && fontUpdateTexts.Length > 0)
		//	{
		//		for (int i = 0; i < fontUpdateTexts.Length; i++)
		//		{
		//			fontUpdateTexts[i].font = TextManager.Instance.GetFont();
		//		}
		//	}
		//	lastUpdateLocale = TextManager.Instance.locale;
		//}
		if (scroll == null) {
			scroll = GetComponentInParent<CustomList<T>> ();
		}
		this.data = data;

	}

	protected CustomList<T>.ClickEvent clickEvent;
	protected CustomList<T>.ClickCountEvent clickCountEvent;

	public virtual void SetEvent(CustomList<T>.ClickEvent clickEvent)
	{
		this.clickEvent = clickEvent;
	}
	public virtual void SetCountEvent(CustomList<T>.ClickCountEvent clickEvent)
	{
		this.clickCountEvent = clickEvent;
	}

	protected bool isActive;

    public virtual void Active(bool active)
    {
        isActive = active;
    }

	protected CustomList<T> scroll;


    public virtual void ClickItem()
    {
		SendScrollFocus();
		if (clickEvent != null)
			clickEvent.Invoke (data);
    }

	public virtual void ClickCountItem(int count)
	{
		SendScrollFocus();
		if (clickCountEvent != null)
			clickCountEvent.Invoke(data, count);
	}

	protected void SendScrollFocus()
	{
		if (scroll == null)
		{
			scroll = GetComponentInParent<CustomList<T>>();
		}
		if ( scroll != null ) scroll.Focus (this);
	}

    [SerializeField]
	protected GameObject focusObj;

	public virtual void Focus(ListItem<T> focusItem)
	{
		if( focusObj != null ) focusObj.SetActive (focusItem == this);
	}

    public virtual void Focus(T data)
    {
        if (focusObj != null) focusObj.SetActive(GetData() == data);
    }

    public virtual void Focus(bool focus)
    {
        if (focusObj != null) focusObj.SetActive(focus);
    }

	[SerializeField]
	protected GameObject checkObj;
	public virtual void Check(List<T> checkItems)
    {
		if (checkObj != null)
		{
			checkObj.SetActive(false);
			for(int i = 0; i < checkItems.Count; i++)
            {
				if(checkItems[i].Equals(GetData()))
                {
					checkObj.SetActive(true);
					break;
                }
            }
			//checkObj.SetActive(GetData().Equals(checkItem));
		}
    }

}

