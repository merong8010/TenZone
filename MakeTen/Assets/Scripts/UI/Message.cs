using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

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

    private class Data
    {
        public Type type;
        public string message;
        public Action<bool> callback;
    }

    private Stack<Data> stack = new Stack<Data>();
    private Data data = null;
    private void Show(Data data)
    {
        Show(data.type, data.message, callback: data.callback);
    }
    public void Show(Type type, string message, string yes = "yes", string no = "no", Action<bool> callback = null)
    {
        if(gameObject.activeSelf && data != null)
        {
            stack.Push(data);
        }
        gameObject.SetActive(true);

        messageText.text = message;
        confirmButton.SetActive(type == Type.Confirm);
        yesButton.SetActive(type == Type.Ask);
        noButton.SetActive(type == Type.Ask);

        yesText.text = TextManager.Get(yes);
        noText.text = TextManager.Get(no);
        data = new Data();
        data.type = type;
        data.message = message;
        data.callback = callback;
        this.callback = callback;
    }

    public void ClickConfirm()
    {
        gameObject.SetActive(false);
        data = null;
        if (stack.Count > 0) Show(stack.Pop());

        if (callback != null) callback.Invoke(true);
    }

    public void ClickYes()
    {
        gameObject.SetActive(false);
        data = null;
        if (stack.Count > 0) Show(stack.Pop());

        if (callback != null) callback.Invoke(true);
    }

    public void ClickNo()
    {
        gameObject.SetActive(false);
        data = null;
        if (stack.Count > 0) Show(stack.Pop());

        if (callback != null) callback.Invoke(false);
    }
}
