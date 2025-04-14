using UnityEngine;
using DG.Tweening;

public class Popup : MonoBehaviour
{
    [SerializeField]
    protected GameObject portraitObj;
    [SerializeField]
    protected GameObject landscapeObj;
    [SerializeField]
    protected SafeArea safeArea;
    [SerializeField]
    protected RectTransform rt;

    protected virtual void Awake()
    {
        if (safeArea == null) safeArea = GetComponentInChildren<SafeArea>();
        if (safeArea != null) safeArea.refreshAction = Refresh;
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
        Vector2 origin = rt.anchoredPosition;
        rt.anchoredPosition = new Vector2(origin.x, origin.y - 1000);
        rt.DOAnchorPosY(origin.y, 0.5f);
    }

    public virtual void Close()
    {
        UIManager.Instance.ClosePopup(this);
    }

    public virtual void Refresh()
    {
        portraitObj?.SetActive(Input.deviceOrientation == DeviceOrientation.Portrait);
        landscapeObj?.SetActive(Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight);
    }
}
