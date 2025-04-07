using UnityEngine;

public class Popup : MonoBehaviour
{
    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        UIManager.Instance.ClosePopup(this);
    }
}
