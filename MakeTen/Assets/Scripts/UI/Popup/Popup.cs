using UnityEngine;

public class Popup : MonoBehaviour
{
    public virtual void Open()
    {

    }

    public virtual void Close()
    {
        UIManager.Instance.ClosePopup(this);
    }
}
