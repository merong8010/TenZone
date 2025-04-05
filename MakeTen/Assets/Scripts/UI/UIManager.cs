using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class UIManager : Singleton<UIManager>
{
    private List<Popup> popups = new List<Popup>();
    //private Dictionary<T, Popup> popups = new Dictionary<string, GameObject>();
    private Stack<Popup> popupStack = new Stack<Popup>(); // 열린 팝업 순서 관리
    [SerializeField]
    private Transform popupParent;

    protected override void Awake()
    {
        base.Awake();
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

        popupStack.Push(popup);

        return (T)popup;
    }

    public void ClosePopup<T>() where T : Popup
    {
        if (popupStack.Count > 0)
        {
            List<Popup> popupList = new List<Popup>();
            while(popupStack.TryPop(out Popup popup))
            {
                if(popup.GetType() != typeof(T))
                {
                    popupList.Add(popup);
                }
                else
                {
                    if (popupList.Count > 0)
                    {
                        for (int i = 0; i < popupList.Count; i++)
                        {
                            popupStack.Push(popupList[i]);
                        }
                    }
                    popup.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    public void ClosePopup(Popup close)
    {
        if (popupStack.Count > 0)
        {
            List<Popup> popupList = new List<Popup>();
            while (popupStack.TryPop(out Popup popup))
            {
                if (popup.GetType() != close.GetType())
                {
                    popupList.Add(popup);
                }
                else
                {
                    if (popupList.Count > 0)
                    {
                        for (int i = 0; i < popupList.Count; i++)
                        {
                            popupStack.Push(popupList[i]);
                        }
                    }
                    popup.gameObject.SetActive(false);
                    break;
                }
            }
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
            Popup popup = popupStack.Pop();
            popup.gameObject.SetActive(false);
        }
    }

    // 📌 모든 팝업 닫기
    public void CloseAllPopups()
    {
        while (popupStack.Count > 0)
        {
            Popup popup = popupStack.Pop();
            popup.gameObject.SetActive(false);
        }
    }
    [SerializeField]
    private GameObject loadingObj;
    [SerializeField]
    private UnityEngine.UI.Text loadingText;

    public void Loading(string message = "Loading")
    {
        loadingObj.SetActive(true);
        loadingText.text = message;
    }

    public void CloseLoading()
    {
        loadingObj.SetActive(false);
    }
}
