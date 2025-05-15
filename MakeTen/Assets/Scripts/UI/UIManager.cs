using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using DG.Tweening;

public class UIManager : Singleton<UIManager>
{
    private List<Popup> popups = new List<Popup>();
    //private Dictionary<T, Popup> popups = new Dictionary<string, GameObject>();
    private List<Popup> popupStack = new List<Popup>(); // 열린 팝업 순서 관리
    [SerializeField]
    private Transform popupParent;

    [SerializeField]
    private Transform blockParent;
    [SerializeField]
    private Vector2Int blockCounts;
    [SerializeField]
    private Vector2 blockSize;
    [SerializeField]
    private Vector2 blockGap;

    protected override void Awake()
    {
        base.Awake();
    }

    private const string PopupPath = "Prefabs/UI/Popups/";

    private Dictionary<Popup, Action> closeCallback = new Dictionary<Popup, Action>();
    public T Open<T>(Action callback = null) where T : Popup
    {
        Popup popup = null;
        if(popups.Exists(x => x.GetType() == typeof(T)))
        {
            popup = popups.FirstOrDefault(x => x.GetType() == typeof(T));
            
        }
        else
        {
            GameObject popupObj = Instantiate(Resources.Load<GameObject>(new StringBuilder().Append(PopupPath).Append(typeof(T).Name).ToString()), popupParent);
            popup = popupObj.GetComponent<T>();
            popups.Add(popup);
        }

        if (popup == null) return null;
        popup.Open();

        popupStack.Add(popup);
        if(callback != null) closeCallback.Add(popup, callback);
        return (T)popup;
    }

    public T Get<T>() where T: Popup
    {
        if(popupStack.Exists(x => x.GetType() == typeof(T)))
        {
            return (T)popupStack.FirstOrDefault(x => x.GetType() == typeof(T));
        }
        return null;
    }

    public void ClosePopup<T>() where T : Popup
    {
        if (popupStack.Count > 0)
        {
            if (popupStack.Exists(x => x.GetType() == typeof(T)))
            {
                foreach (Popup item in popupStack.Where(x => x.GetType() == typeof(T)))
                {
                    item.gameObject.SetActive(false);
                    if (closeCallback.ContainsKey(item) && closeCallback[item] != null)
                    {
                        closeCallback[item].Invoke();
                        closeCallback.Remove(item);
                    }
                }
                popupStack.RemoveAll(x => x.GetType() == typeof(T));
            }
        }
    }

    public void ClosePopup(Popup close)
    {
        if (popupStack.Count > 0 && popupStack.Contains(close))
        {
            close.gameObject.SetActive(false);
            popupStack.Remove(close);
            if (closeCallback.ContainsKey(close) && closeCallback[close] != null)
            {
                closeCallback[close].Invoke();
                closeCallback.Remove(close);
            }
            //List<Popup> popupList = new List<Popup>();
            //while (popupStack.TryPop(out Popup popup))
            //{
            //    if (popup.GetType() != close.GetType())
            //    {
            //        popupList.Add(popup);
            //    }
            //    else
            //    {
            //        if (popupList.Count > 0)
            //        {
            //            for (int i = 0; i < popupList.Count; i++)
            //            {
            //                popupStack.Push(popupList[i]);
            //            }
            //        }
            //        popup.gameObject.SetActive(false);
            //        break;
            //    }
            //}
        }
    }

    //// 📌 팝업을 등록 (Start에서 수동 등록 or 동적 생성 가능)
    //public void RegisterPopup(string popupName, GameObject popup)
    //{
    //    if (!popups.ContainsKey(popupName))
    //    {
    //        popups.Add(popupName, popup);
    //        popup.SetActive(false);
    //    }
    //}

    //// 📌 팝업 열기
    //public void ShowPopup(string popupName)
    //{
    //    if (popups.ContainsKey(popupName))
    //    {
    //        GameObject popup = popups[popupName];
    //        popup.SetActive(true);
    //        popupStack.Push(popup); // 스택에 추가
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"[PopupManager] {popupName} 팝업이 등록되지 않았습니다.");
    //    }
    //}
    [SerializeField]
    private CanvasScalerAutoAdjuster scalerAuto;
    public void Refresh()
    {
        //scalerAuto.Refresh();
        popupStack.LastOrDefault()?.Refresh();
    }
    // 📌 팝업 닫기
    public void ClosePopup()
    {
        if (popupStack.Count > 0)
        {
            Popup popup = popupStack.Last();
            popup.gameObject.SetActive(false);
            popupStack.Remove(popup);
        }
    }

    // 📌 모든 팝업 닫기
    public void CloseAllPopups()
    {
        foreach (Popup popup in popupStack)
        {
            popup.gameObject.SetActive(false);
        }
        popupStack.Clear();
    }
    
    [SerializeField]
    private GameObject loadingObj;
    [SerializeField]
    private CanvasGroup loadingGroup;

    [SerializeField]
    private UnityEngine.UI.Image loadingBG;
    [SerializeField]
    private UnityEngine.UI.Text loadingText;
    [SerializeField]
    private GameObject bg;

    public void ShowBG(bool show)
    {
        bg.SetActive(show);

    }
    private Tweener loadingTweener;
    public void Loading(string message = "Loading", float bgAlpha = 1f, float fadeOutDuration = 0.5f, float fadeDelay = 1f, float fadeInDuration = 0.5f, Action callback = null, Action completeCallback = null)
    {
        loadingObj.SetActive(true);
        loadingTweener?.Kill();
        loadingGroup.alpha = 0f;
        loadingTweener = DOTween.To(() => loadingGroup.alpha, x => loadingGroup.alpha = x, bgAlpha, fadeOutDuration).OnComplete(() =>
        {
            callback?.Invoke();
            if(fadeDelay > 0f || completeCallback != null)
            {
                loadingTweener = DOTween.To(() => loadingGroup.alpha, x => loadingGroup.alpha = x, 0f, fadeInDuration).SetDelay(fadeDelay).OnComplete(() => loadingObj.SetActive(false)).OnComplete(() =>
                {
                    completeCallback?.Invoke();
                    loadingObj.SetActive(false);
                });
            }
        });
        
        loadingText.text = message;
    }

    public void CloseLoading(float fadeInDuration = 0.5f, float fadeDelay = 0f, Action completeCallback = null)
    {
        loadingTweener = DOTween.To(() => loadingGroup.alpha, x => loadingGroup.alpha = x, 0f, fadeInDuration).SetDelay(fadeDelay).OnComplete(() => loadingObj.SetActive(false)).OnComplete(() =>
        {
            completeCallback?.Invoke();
            loadingObj.SetActive(false);
        });
    }


    public void RefreshMain()
    {

    }

    public Message Message;
}
