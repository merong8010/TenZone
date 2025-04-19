using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

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
        idText.text = DataManager.Instance.userData.id.ToString();
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
    private TextMeshProUGUI idText;

    [SerializeField]
    private TextMeshProUGUI loginStatus;
    [SerializeField]
    private GameObject loginObjs;
    [SerializeField]
    private GameObject logoutObj;

    public override void Refresh()
    {
        base.Refresh();
        loginStatus.text = DataManager.Instance.userData.authType.ToString();
        loginObjs.SetActive(DataManager.Instance.userData.authType == FirebaseManager.AuthenticatedType.None);
        logoutObj.SetActive(DataManager.Instance.userData.authType != FirebaseManager.AuthenticatedType.None);
    }

    public void ClickGoogleLogin()
    {
        FirebaseManager.Instance.StartGoogleLogin();
    }
}
