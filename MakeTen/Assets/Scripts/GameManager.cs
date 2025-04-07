using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using System.Text;
using System;
using System.Collections;
using UnityEngine.Networking;

public class GameManager : Singleton<GameManager>
{
    protected override void Awake()
    {
        base.Awake();

        System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo("en-US");
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        StartCoroutine(Initialize());
    }

    private void OnApplicationFocus(bool focus)
    {
        if(focus)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if(pause)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    private void Resume()
    {
        //UIManager.Instance.Loading();
        //_currentTime = null;
        //FetchOnlineTime();
    }

    private void Pause()
    {
        //_currentTime = null;
    }

    private DateTime? _currentTime = null; // 시간 저장 (정상적으로 가져오지 못하면 null)

    public DateTime? dateTime => _currentTime != null ? _currentTime.Value : null;

    public ReactiveProperty<DateTime> reactiveTime = new ReactiveProperty<DateTime>();
    
    // ✅ 구글 서버에서 UTC 시간 가져오기
    public void FetchOnlineTime()
    {
        StartCoroutine(GetOnlineTime());
    }

    private IEnumerator GetOnlineTime()
    {
        using (UnityWebRequest request = UnityWebRequest.Get("https://www.google.com"))
        {
            request.SetRequestHeader("Cache-Control", "no-cache");
            request.SetRequestHeader("Pragma", "no-cache");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // HTTP 헤더에서 Date 정보 추출
                string dateHeader = request.GetResponseHeader("Date");

                if (!string.IsNullOrEmpty(dateHeader))
                {
                    _currentTime = DateTime.Parse(dateHeader).ToUniversalTime();
                    lastCheckTime = Time.realtimeSinceStartupAsDouble;

                    UIManager.Instance.CloseLoading();
                }
                else
                {
                    _currentTime = null;
                    FetchOnlineTime();
                }
            }
            else
            {
                _currentTime = null;
                FetchOnlineTime();
            }
        }
    }

    private double lastCheckTime;

    private void Update()
    {
        if(_currentTime != null)
        {
            _currentTime = _currentTime.Value.AddSeconds(Time.realtimeSinceStartupAsDouble - lastCheckTime);
            lastCheckTime = Time.realtimeSinceStartupAsDouble;
            reactiveTime.Value = _currentTime.Value;
        }
    }

    private IEnumerator Initialize()
    {
        FetchOnlineTime();
        yield return new WaitUntil(() => _currentTime != null);
        yield return new WaitUntil(() => FirebaseManager.Instance.IsReady);
        DataManager.Instance.LoadGameDatas();
        yield return new WaitUntil(() => DataManager.Instance.IsLoadComplete);

        UIManager.Instance.Main.Refresh();
        HUD.Instance.UpdateHeart();
    }
}
