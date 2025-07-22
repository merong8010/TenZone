using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;

public class UIManager : Singleton<UIManager>
{
    [Header("Popup Settings")]
    [SerializeField] private Transform popupParent;

    // --- 개선점 1: 명확한 이름으로 변경 및 캐시/스택 분리 ---
    // 모든 생성된 팝업 인스턴스를 보관 (메모리 캐시)
    private List<Popup> popupInstanceCache = new List<Popup>();
    // 현재 "열려있는" 팝업만 순서대로 관리 (UI 스택)
    private List<Popup> openPopupStack = new List<Popup>();
    // 닫기 콜백 관리
    private Dictionary<Popup, Action> closeCallbacks = new Dictionary<Popup, Action>();

    // --- 개선점 2: 프리팹 미리 로딩(Pre-loading) 시스템 ---
    private Dictionary<Type, GameObject> popupPrefabs = new Dictionary<Type, GameObject>();
    private const string PopupPath = "Prefabs/UI/Popups/";

    /// <summary>
    /// 게임 시작 시 팝업 프리팹들을 미리 로드하여 딕셔너리에 저장합니다.
    /// Resources.Load를 게임 플레이 중에 호출하는 것을 방지하여 성능을 향상시킵니다.
    /// </summary>
    /// <param name="popupTypes">미리 로드할 팝업들의 타입 배열</param>
    public void InitializePopups(params Type[] popupTypes)
    {
        foreach (var type in popupTypes)
        {
            if (!typeof(Popup).IsAssignableFrom(type)) continue;

            string path = $"{PopupPath}{type.Name}";
            GameObject prefab = Resources.Load<GameObject>(path);

            if (prefab != null)
            {
                popupPrefabs[type] = prefab;
            }
            else
            {
                Debug.LogWarning($"[UIManager] InitializePopups: Prefab for '{type.Name}' not found at path '{path}'");
            }
        }
    }

    /// <summary>
    /// 지정된 타입의 팝업을 엽니다.
    /// </summary>
    public T Open<T>(Action callback = null) where T : Popup
    {
        // 이미 생성된 인스턴스가 있는지 확인
        Popup popup = popupInstanceCache.FirstOrDefault(x => x.GetType() == typeof(T));

        if (popup == null)
        {
            // --- 개선점 3: 안정성 강화를 위한 예외 처리 ---
            if (!popupPrefabs.ContainsKey(typeof(T)))
            {
                Debug.LogError($"[UIManager] Prefab for '{typeof(T).Name}' is not pre-loaded. Call InitializePopups first.");
                return null;
            }

            GameObject prefab = popupPrefabs[typeof(T)];
            GameObject popupObj = Instantiate(prefab, popupParent);
            popup = popupObj.GetComponent<T>();

            if (popup == null)
            {
                Debug.LogError($"[UIManager] Prefab '{prefab.name}' does not have the required component '{typeof(T).Name}'.");
                Destroy(popupObj);
                return null;
            }

            popupInstanceCache.Add(popup);
        }

        // 팝업 열기 로직
        popup.transform.SetAsLastSibling(); // 가장 위로 올리기
        popup.Open(); // 팝업 자체의 Open 로직 실행

        if (!openPopupStack.Contains(popup))
        {
            openPopupStack.Add(popup);
        }

        // 콜백 등록
        if (callback != null)
        {
            closeCallbacks[popup] = callback;
        }

        return (T)popup;
    }

    /// <summary>
    /// 현재 열려있는 팝업 중 특정 타입의 팝업을 가져옵니다.
    /// </summary>
    public T Get<T>() where T : Popup
    {
        return openPopupStack.FirstOrDefault(x => x.GetType() == typeof(T)) as T;
    }

    /// <summary>
    /// 가장 마지막에 열린 팝업을 닫습니다.
    /// </summary>
    public void ClosePopup()
    {
        // --- 개선점 4: 로직 일관성 확보 및 버그 수정 ---
        // 가장 마지막 팝업을 찾아 상세 로직이 구현된 다른 ClosePopup 메소드에 넘겨줍니다.
        // 이렇게 하면 코드 중복을 피하고 콜백 미호출 버그를 해결할 수 있습니다.
        if (openPopupStack.Count > 0)
        {
            ClosePopup(openPopupStack.Last());
        }
    }

    /// <summary>
    /// 특정 팝업 인스턴스를 닫습니다. (핵심 로직)
    /// </summary>
    public void ClosePopup(Popup popupToClose)
    {
        if (popupToClose != null && openPopupStack.Contains(popupToClose))
        {
            popupToClose.gameObject.SetActive(false);
            // 스택에서 제거
            openPopupStack.Remove(popupToClose);

            // 콜백이 있으면 실행하고 제거
            if (closeCallbacks.ContainsKey(popupToClose))
            {
                closeCallbacks[popupToClose]?.Invoke();
                closeCallbacks.Remove(popupToClose);
            }
        }
    }

    /// <summary>
    /// 특정 타입의 팝업을 모두 닫습니다.
    /// </summary>
    public void ClosePopup<T>() where T : Popup
    {
        // 역순으로 순회해야 Remove 시 컬렉션 변경 에러가 발생하지 않습니다.
        for (int i = openPopupStack.Count - 1; i >= 0; i--)
        {
            if (openPopupStack[i] is T)
            {
                ClosePopup(openPopupStack[i]);
            }
        }
    }

    /// <summary>
    /// 열려있는 모든 팝업을 닫습니다.
    /// </summary>
    public void CloseAllPopups()
    {
        // 역순으로 순회
        for (int i = openPopupStack.Count - 1; i >= 0; i--)
        {
            ClosePopup(openPopupStack[i]);
        }
    }

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingObj;
    [SerializeField] private CanvasGroup loadingGroup;
    [SerializeField] private UnityEngine.UI.Text loadingText;

    private Tween loadingTweener;

    public void Loading(string message = "Loading", float fadeInDuration = 0.5f, Action onFadeInComplete = null, float delay = 1f, float fadeOutDuration = 0.5f, Action onFadeOutComplete = null)
    {
        loadingTweener?.Kill();
        loadingText.text = message;
        loadingObj.SetActive(true);

        // --- 개선점 5: DOTween.Sequence를 사용한 가독성 높은 로직 ---
        loadingTweener = DOTween.Sequence()
            .Append(loadingGroup.DOFade(1f, fadeInDuration))
            .AppendCallback(() => onFadeInComplete?.Invoke())
            .AppendInterval(delay)
            .Append(loadingGroup.DOFade(0f, fadeOutDuration))
            .OnComplete(() =>
            {
                onFadeOutComplete?.Invoke();
                loadingObj.SetActive(false);
            });
    }

    public void CloseLoading(float fadeOutDuration = 0.5f)
    {
        loadingTweener?.Kill();
        loadingTweener = loadingGroup.DOFade(0f, fadeOutDuration)
            .OnComplete(() => loadingObj.SetActive(false));
    }

    // 기타 필드 및 메소드는 그대로 유지
    [Header("Other UI Components")]
    [SerializeField] private GameObject bg;
    public Message Message;

    public void ShowBG(bool show)
    {
        bg.SetActive(show);
    }

    public void Refresh()
    {
        openPopupStack.LastOrDefault()?.Refresh();
    }
}