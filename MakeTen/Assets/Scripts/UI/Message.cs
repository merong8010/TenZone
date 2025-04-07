using UnityEngine;
using UnityEngine.UI;
using System;

public class Message : MonoBehaviour
{
    public enum Type
    {
        Simple,
        Confirm,
        Ask,
    }

    [SerializeField]
    private Text messageText;
    [SerializeField]
    private Text yesText;
    [SerializeField]
    private Text noText;

    [SerializeField]
    private GameObject confirmButton;
    [SerializeField]
    private GameObject yesButton;
    [SerializeField]
    private GameObject noButton;

    private Action<bool> callback;
    public void Show(Type type, string message, string yes = "yes", string no = "no", Action<bool> callback = null)
    {
        gameObject.SetActive(true);

        messageText.text = message;
        confirmButton.SetActive(type == Type.Confirm);
        yesButton.SetActive(type == Type.Ask);
        noButton.SetActive(type == Type.Ask);

        yesText.text = TextManager.Get(yes);
        noText.text = TextManager.Get(no);

        this.callback = callback;
    }

    public void ClickConfirm()
    {
        if (callback != null) callback.Invoke(true);
        gameObject.SetActive(false);
    }

    public void ClickYes()
    {
        if (callback != null) callback.Invoke(true);
        gameObject.SetActive(false);
    }

    public void ClickNo()
    {
        if (callback != null) callback.Invoke(false);
        gameObject.SetActive(false);
    }
}
