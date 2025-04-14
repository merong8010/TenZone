using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OptionManager : Singleton<OptionManager>
{
    public enum Type
    {
        NONE = 0,
        BGM = 1,
        EFFECT = 2,
        LOW_SPEC = 3, //sleep mode
        PUSH = 4,
        HAPTIC = 5,
        MAX,
    }

    public bool Get(Type type)
    {
        Init();
        if (options.ContainsKey(type))
            return options[type] == 1;
        return false;
    }

    public void Set(Type type, bool on)
    {
        PlayerPrefs.SetInt(type.ToString(), on ? 1 : 0);
        if (options.ContainsKey(type)) options[type] = on ? 1 : 0;
        switch (type)
        {
            case Type.BGM:
                SoundManager.Instance.SetBgmMute(on);
                break;
            case Type.EFFECT:
                SoundManager.Instance.SetEffectMute(on);
                break;
            case Type.PUSH:
                break;
        }
    }

    private bool isInit = false;

    private void Init()
    {
        if (isInit) return;
        isInit = true;

        for (int i = 0; i < defaultSettings.Count; i++)
        {
            if(!PlayerPrefs.HasKey(defaultSettings[i].type.ToString()))
            {
                if(defaultSettings[i].floatVal > 0f)
                {
                    PlayerPrefs.SetFloat(defaultSettings[i].type.ToString(), defaultSettings[i].floatVal);
                }
                else
                {
                    PlayerPrefs.SetInt(defaultSettings[i].type.ToString(), defaultSettings[i].boolVal ? 1 : 0);
                }
                options.Add(defaultSettings[i].type, defaultSettings[i].floatVal > 0f ? defaultSettings[i].floatVal : defaultSettings[i].boolVal ? 1f : 0f);
            }
            else
            {
                if(PlayerPrefs.GetFloat(defaultSettings[i].type.ToString()) > 0f)
                {
                    options.Add(defaultSettings[i].type, PlayerPrefs.GetFloat(defaultSettings[i].type.ToString()));
                }
                else
                {
                    options.Add(defaultSettings[i].type, PlayerPrefs.GetInt(defaultSettings[i].type.ToString()));
                }
            }
            
        }
    }

    private Dictionary<Type, float> options = new Dictionary<Type, float>();
    protected override void Awake()
    {
        base.Awake();

        Init();
    }

    [SerializeField]
    private List<DefaultSetting> defaultSettings;
    [System.Serializable]
    private class DefaultSetting
    {
        public Type type;
        public bool boolVal;
        public float floatVal;
    }
}