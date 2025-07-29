using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.Purchasing;

public class PopupSettings : Popup
{
    [SerializeField]
    private Toggle bgmToggle;
    [SerializeField]
    private Toggle effectToggle;
    [SerializeField]
    private Toggle lowSpecToggle;
    [SerializeField]
    private Toggle pushToggle;
    [SerializeField]
    private Toggle hapticToggle;
    [SerializeField]
    private Toggle portraitToggle;
    [SerializeField]
    private Toggle rotateToggle;

    public override void Open()
    {
        Initialize();
        base.Open();
        bgmToggle.isOn = OptionManager.Instance.Get(OptionManager.Type.BGM);
        effectToggle.isOn = OptionManager.Instance.Get(OptionManager.Type.EFFECT);
        lowSpecToggle.isOn = OptionManager.Instance.Get(OptionManager.Type.LOW_SPEC);
        pushToggle.isOn = OptionManager.Instance.Get(OptionManager.Type.PUSH);
        hapticToggle.isOn = OptionManager.Instance.Get(OptionManager.Type.HAPTIC);
        portraitToggle.isOn = OptionManager.Instance.Get(OptionManager.Type.PORTRAIT);
        rotateToggle.isOn = OptionManager.Instance.Get(OptionManager.Type.ROTATE_SCREEN);
    }

    private bool isInit = false;
    private void Initialize()
    {
        if (isInit) return;
        isInit = true;

        bgmToggle.onValueChanged.AddListener(isOn => BGM(isOn));
        effectToggle.onValueChanged.AddListener(isOn => Effect(isOn));
        lowSpecToggle.onValueChanged.AddListener(isOn => LowSpec(isOn));
        pushToggle.onValueChanged.AddListener(isOn => Push(isOn));
        hapticToggle.onValueChanged.AddListener(isOn => Haptic(isOn));
        portraitToggle.onValueChanged.AddListener(isOn => Portrait(isOn));
        rotateToggle.onValueChanged.AddListener(isOn => Rotate(isOn));
        idText.text = FirebaseManager.Instance.UserId;
    }

    private void BGM(bool isOn)
    {
        OptionManager.Instance.Set(OptionManager.Type.BGM, isOn);
        SoundManager.Instance.SetBgmMute(isOn);
    }

    private void Effect(bool isOn)
    {
        OptionManager.Instance.Set(OptionManager.Type.EFFECT, isOn);
        SoundManager.Instance.SetEffectMute(isOn);
    }

    private void LowSpec(bool isOn)
    {
        OptionManager.Instance.Set(OptionManager.Type.LOW_SPEC, isOn);
        //SoundManager.Instance.SetEffectMute(isOn);
    }

    private void Push(bool isOn)
    {
        OptionManager.Instance.Set(OptionManager.Type.PUSH, isOn);
        //SoundManager.Instance.SetEffectMute(isOn);
    }

    private void Haptic(bool isOn)
    {
        OptionManager.Instance.Set(OptionManager.Type.HAPTIC, isOn);
        //SoundManager.Instance.SetEffectMute(isOn);
    }

    private void Portrait(bool isOn)
    {
        OptionManager.Instance.Set(OptionManager.Type.PORTRAIT, isOn);
        GameManager.Instance.SetScreenRoate(isOn, OptionManager.Instance.Get(OptionManager.Type.ROTATE_SCREEN));
    }

    private void Rotate(bool isOn)
    {
        OptionManager.Instance.Set(OptionManager.Type.ROTATE_SCREEN, isOn);
        GameManager.Instance.SetScreenRoate(OptionManager.Instance.Get(OptionManager.Type.PORTRAIT), isOn);
    }


    [SerializeField]
    private Text idText;

    [SerializeField]
    private GameObject loginObjs;
    [SerializeField]
    private GameObject logoutObj;
    [SerializeField]
    private GameObject loginedGoogle;
    [SerializeField]
    private GameObject loginedApple;
    //[SerializeField]
    //private GameObject loginedEmail;

    public override void Refresh()
    {
        base.Refresh();
        idText.text = FirebaseManager.Instance.UserId;
        if(FirebaseManager.Instance.authType == FirebaseManager.AuthenticatedType.None)
        {
            loginObjs.SetActive(true);
            logoutObj.SetActive(false);
            loginedGoogle.SetActive(false);
            loginedApple.SetActive(false);
            //loginedEmail.SetActive(false);
        }
        else
        {
            loginObjs.SetActive(false);
            logoutObj.SetActive(true);
            loginedGoogle.SetActive(FirebaseManager.Instance.authType == FirebaseManager.AuthenticatedType.Google);
            loginedApple.SetActive(FirebaseManager.Instance.authType == FirebaseManager.AuthenticatedType.Apple);
            //loginedEmail.SetActive(FirebaseManager.Instance.authType == FirebaseManager.AuthenticatedType.Email);
        }
        //loginStatus.text = DataManager.Instance.userData.authType.ToString();
        //loginObjs.SetActive(DataManager.Instance.userData.authType == FirebaseManager.AuthenticatedType.None);
        //logoutObj.SetActive(DataManager.Instance.userData.authType != FirebaseManager.AuthenticatedType.None);
    }

    public void ClickCopy()
    {
        UniClipboard.SetText(idText.text);
        UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("Copied_ID"));
    }

    public void ClickGoogleLogin()
    {
        FirebaseManager.Instance.StartGoogleLogin();
    }

    public void ClickAppleLogin()
    {
        //FirebaseManager.Instance.StartGoogleLogin();
    }

    public void ClickLogout()
    {
        FirebaseManager.Instance.LogOut();
    }

    public void ClickQA()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("mailto:qa.tenzone@gmail.com").Append("?subject=").Append(TextManager.Get("QA")).Append("&body=").Append(TextManager.Get("QA")).AppendLine().AppendLine().Append("ID : ").Append(idText.text).AppendLine().AppendLine().Append(TextManager.Get("QA_Contents"));
        Application.OpenURL(sb.ToString());
    }

    [SerializeField]
    private GameObject languageObj;
    [SerializeField]
    private TabGroup languageTab;
    private int languageIdx;
    public void ClickLanguage()
    {
        languageObj.SetActive(true);
        string countryCode = PlayerPrefs.GetString("Locale", Util.GetCountryCode());
        languageIdx = 0;
        switch(countryCode)
        {
            case "KR":
                languageIdx = 1;
                break;
            case "JP":
                languageIdx = 2;
                break;
            case "TW":
                languageIdx = 3;
                break;
        }
        languageTab.Init(languageIdx, idx =>
        {
            languageIdx = idx;
        });
    }

    public void ClickLanguageConfirm()
    {
        PlayerPrefs.SetString("Locale", ((TextManager.Locale)languageIdx).ToString());
        TextManager.LoadDatas(((TextManager.Locale)languageIdx).ToString(), DataManager.Instance.Get<GameData.Language>());
        CloseLanguage();
        Refresh();
        TextSetter[] ts = GetComponentsInChildren<TextSetter>();
        for(int i = 0; i < ts.Length; i++)
        {
            ts[i].Refresh();
        }
        TextSetter[] mainTs = MainManager.Instance.GetComponentsInChildren<TextSetter>();
        for (int i = 0; i < mainTs.Length; i++)
        {
            mainTs[i].Refresh();
        }
    }

    public void CloseLanguage()
    {
        languageObj.SetActive(false);
    }

    [SerializeField]
    private GameObject emailLoginObj;
    [SerializeField]
    private InputField emailInput;
    [SerializeField]
    private InputField passwordInput;

    public void ClickMailLogin()
    {
        emailLoginObj.SetActive(true);
    }

    public void StartMailLogin()
    {
        if(!emailInput.text.IsValidEmail())
        {
            UIManager.Instance.Message.Show(Message.Type.Simple, "invalid email");
            return;
        }
        if(passwordInput.text.Length < 6)
        {
            UIManager.Instance.Message.Show(Message.Type.Simple, "need password length 6 or more");
            return;
        }

        FirebaseManager.Instance.SignInWithEmail(emailInput.text, passwordInput.text);
        CloseMailLogin();
    }

    public void CloseMailLogin()
    {
        emailLoginObj.SetActive(false);
    }
}
