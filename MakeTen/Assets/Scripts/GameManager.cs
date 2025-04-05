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
        UIManager.Instance.Loading();
        _currentTime = null;
        FetchOnlineTime();
    }

    private void Pause()
    {
        _currentTime = null;
    }

    private DateTime? _currentTime = null; // 시간 저장 (정상적으로 가져오지 못하면 null)

    public DateTime? dateTime => _currentTime != null ? _currentTime.Value : null;
    
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
                    //Debug.Log($"✅ Google 시간 동기화 성공: {_currentTime}");
                }
                else
                {
                    //Debug.LogWarning("❌ Google 서버 응답에 시간 정보 없음.");
                    _currentTime = null;
                    FetchOnlineTime();
                }
            }
            else
            {
                //Debug.LogError($"❌ Google 시간 요청 실패: {request.error}");
                _currentTime = null;
                FetchOnlineTime();
            }
        }

        //UIManager.Instance.CloseLoading();
    }

    private double lastCheckTime;

    private void Update()
    {
        if(_currentTime != null)
        {
            double flow = Time.realtimeSinceStartupAsDouble - lastCheckTime;
            if (flow >= 1)
            {
                _currentTime = _currentTime.Value.AddSeconds(flow);
                lastCheckTime = Time.realtimeSinceStartupAsDouble;

                Debug.Log(dateTime.ToString());
            }

            
        }
    }
}
