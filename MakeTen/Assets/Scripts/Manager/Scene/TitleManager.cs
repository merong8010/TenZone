using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class TitleManager : Singleton<TitleManager>
{
    //[SerializeField]
    //private GameObject loginObj;
    [SerializeField]
    private GameObject tabPlayObj;

    [SerializeField]
    private Text statusText;
   

    protected override void Awake()
    {
        base.Awake();
        GameManager.Instance.Init();
    }

    

    public void SetStatus(string text, bool showLogins = false, bool showTap = false)
    {
        statusText.text = text;
        //loginObj.SetActive(showLogins);
        tabPlayObj.SetActive(showTap);
    }

    public void ClickGuest()
    {

    }

    public void ClickGoogle()
    {

    }

    public void ClickApple()
    { 

    }

    public void GameStart()
    {
        GameManager.Instance.GoScene(GameManager.Scene.Main);
    }
}
