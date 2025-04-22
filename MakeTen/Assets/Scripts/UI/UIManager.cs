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

        StartCoroutine(InitBG());
    }

    private IEnumerator InitBG()
    {
        yield return new WaitUntil(() => ObjectPooler.Instance.isReady);
        Vector2 blockStartPos = blockStartPos = new Vector2(-(blockSize.x + blockGap.x) * (blockCounts.x - 1) * 0.5f, -(blockSize.y + blockGap.y) * (blockCounts.y - 1) * 0.5f);
        Debug.Log("blockStartPos " + blockStartPos);
        for (int x = 0; x < blockCounts.x; x++)
        {
            for (int y = 0; y < blockCounts.y; y++)
            {
                Block blockObj = ObjectPooler.Instance.Get<Block>("block_title", blockParent, blockStartPos + new Vector2((blockSize.x + blockGap.x) * x, (blockSize.y + blockGap.y) * y), Vector3.one);
                blockObj.SetSize(blockSize);
                blockObj.InitRandom();
            }
        }
    }

    private const string PopupPath = "Prefabs/UI/Popups/";

    public T Open<T>() where T : Popup
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
                }
                popupStack.RemoveAll(x => x.GetType() == typeof(T));
            }
            //List<Popup> popupList = new List<Popup>();
            //while(popupStack.TryPop(out Popup popup))
            //{
            //    if(popup.GetType() != typeof(T))
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

    public void ClosePopup(Popup close)
    {
        if (popupStack.Count > 0 && popupStack.Contains(close))
        {
            close.gameObject.SetActive(false);
            popupStack.Remove(close);
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
        //while (popupStack.Count > 0)
        //{
        //    Popup popup = popupStack.Pop();
        //    popup.gameObject.SetActive(false);
        //}
    }
    [SerializeField]
    private GameObject loadingObj;
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
    public void Loading(string message = "Loading", float bgAlpha = 1f, float fadeDuration = 0.5f, float fadeDelay = 1f, Action callback = null, Action completeCallback = null)
    {
        loadingObj.SetActive(true);
        loadingTweener?.Kill();
        loadingTweener = DOTween.ToAlpha(() => loadingBG.color, x => loadingBG.color = x, bgAlpha, fadeDuration).OnComplete(() =>
        {
            callback?.Invoke();
            if(fadeDelay > 0f)
            {
                loadingTweener = DOTween.ToAlpha(() => loadingBG.color, x => loadingBG.color = x, 0f, fadeDuration).SetDelay(fadeDelay).OnComplete(() => loadingObj.SetActive(false)).OnComplete(() =>
                {
                    completeCallback?.Invoke();
                    loadingObj.SetActive(false);
                });
            }
            
        });
        
        loadingText.text = message;
    }

    public void CloseLoading()
    {
        loadingObj.SetActive(false);
    }

    public Main Main;

    //public void ShowMain(bool isShow)
    //{
    //    Main.gameObject.SetActive(isShow);
    //}

    public void RefreshMain()
    {

    }

    public Message Message;
    public Title Title;
}
