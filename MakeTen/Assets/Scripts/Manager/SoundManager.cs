using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField]
    private AudioListener audioListener;
    [SerializeField]
    private AudioSource bgmSource;
    [SerializeField]
    private AudioSource effectSource;

    [SerializeField]
    private AudioClip clickClip;

    private float bgmVolume = 1f;
    private float effectVolume = 1f;

    private Dictionary<string, AudioClip> audioPooler = new Dictionary<string, AudioClip>();

    private const string prePathBGM = "Sounds/BGM/";
    private const string prePathEffect = "Sounds/SFX/";

    protected override void Awake()
    {
        base.Awake();

        SetBgmMute(OptionManager.Instance.Get(OptionManager.Type.BGM));
        SetEffectMute(OptionManager.Instance.Get(OptionManager.Type.EFFECT));

        //SetVolumeBGM(OptionManager.Instance.GetVolume(OptionManager.Type.BGM));
        //SetVolumeEffect(OptionManager.Instance.GetVolume(OptionManager.Type.EFFECT));
    }

    private bool bgmMute;
    private bool effectMute;

    public void SetBgmMute(bool on)
    {
        bgmMute = !on;
        bgmSource.volume = bgmMute ? 0f : bgmVolume;
        if (!bgmMute && !string.IsNullOrEmpty(currentBgmPath)) PlayBGM(currentBgmPath, true);
    }

    public void SetEffectMute(bool on)
    {
        effectMute = !on;
        effectSource.volume = effectMute ? 0f : effectVolume;
    }

    private AudioClip GetAudioClip(string path)
    {
        if (!audioPooler.ContainsKey(path))
        {
            AudioClip clip = Resources.Load<AudioClip>(path);
            audioPooler.Add(path, clip);
        }

        return audioPooler[path];
    }

    private string currentBgmPath;
    public void PlayBGM(string path, bool force = false)
    {
        if (!force && currentBgmPath == path)
        {
            return;
        }
        currentBgmPath = path;
        if (bgmMute || bgmVolume == 0f) return;

        AudioClip audioClip = GetAudioClip(prePathBGM + path);
        if (audioClip != null)
        {
            bgmSource.clip = audioClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlayEffect(string path)
    {
        if (effectMute || effectVolume == 0f) return;
        AudioClip audioClip = GetAudioClip(prePathEffect + path);
        if (audioClip != null)
        {
            effectSource.PlayOneShot(audioClip);
        }
    }

    public void PlayEffect(GameObject go, string path)
    {
        if (effectMute || effectVolume == 0f) return;

        if (go == null)
        {
            PlayEffect(path);
            return;
        }

        AudioSource source = go.GetComponent<AudioSource>();
        if (source == null)
        {
            source = go.AddComponent<AudioSource>();
        }

        AudioClip audioClip = GetAudioClip(prePathEffect + path);
        if (audioClip != null)
        {
            source.PlayOneShot(audioClip);
        }
    }

    [SerializeField]
    private string buttonClipPath;

    public void PlayClick(GameObject go)
    {
        //PlayEffect(go, buttonClipPath);
    }

    public void PlayClick()
    {
        if (effectMute || effectVolume == 0f) return;
        effectSource.PlayOneShot(clickClip);
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (effectMute || effectVolume == 0f) return;
            effectSource.PlayOneShot(clickClip);
        }
    }
}