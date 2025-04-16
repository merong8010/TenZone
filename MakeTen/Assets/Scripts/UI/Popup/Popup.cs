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
        rt.localScale = Vector3.zero;
        rt.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        //Vector2 origin = rt.anchoredPosition;
        //rt.anchoredPosition = new Vector2(origin.x, origin.y - 1000);
        //rt.DOAnchorPosY(origin.y, 0.5f).SetEase(Ease.InOutElastic);
    }

    public virtual void Close()
    {
        UIManager.Instance.ClosePopup(this);
    }

    public virtual void Refresh()
    {
        if(portraitObj != null) portraitObj?.SetActive(Util.GetDeviceOrientation() == DeviceOrientation.Portrait);
        if(landscapeObj != null) landscapeObj?.SetActive(Util.GetDeviceOrientation() == DeviceOrientation.Landscape);
    }
}
