using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;

public class ADManager : Singleton<ADManager>, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
#if UNITY_ANDROID
    private string gameId = "5838019";
    private string bannerId = "Banner_Android";
    private string rewardedId = "Rewarded_Android";
#elif UNITY_IOS
    private string gameId = "5838018";
    private string bannerId = "Banner_iOS";
    private string rewardedId = "Rewarded_iOS";
#endif

    public enum Result
    {
        FAIL = 0,
        SUCCESS = 1,
    }

    public delegate void AdvertiseCallback(Result result);

    private AdvertiseCallback callback;

    private class AdvertiseViewTime
    {
        public Dictionary<int, long> viewTime = new Dictionary<int, long>();
        public AdvertiseViewTime()
        {

        }

        public AdvertiseViewTime(Dictionary<int, long> showTime)
        {
            foreach (KeyValuePair<int, long> pair in showTime)
            {
                viewTime.Add(pair.Key, pair.Value);
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        //Initialize();
    }

    private bool isInit = false;
    private bool isWaitShowReward = false;
    private bool isWaitShowBanner = false;

    private bool isLoadedReward = false;
    private bool isLoadedBanner = false;
    private Action<bool> showCallback;

    public void Initialize()
    {
        Debug.Log($"ADManager.Initialize isVIP : {DataManager.Instance.userData.isVIP} | Advertisement.isInitialized : {Advertisement.isInitialized} | Advertisement.isSupported: {Advertisement.isSupported}");
        if (DataManager.Instance.userData.isVIP) return;
        if (isInit) return;
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        //if (!Advertisement.isInitialized)
        {
#if RELEASE
            Advertisement.Initialize(gameId, false, this);
#else
            Advertisement.Initialize(gameId, true, this);
#endif
            //isWaitShowBanner = true;
            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        }
    }

    public void ShowReward(Action<bool> callback)
    {
        if(DataManager.Instance.userData.isVIP)
        {
            callback?.Invoke(true);
            return;
        }

        showCallback = callback;
        isWaitShowReward = true;
        if(!isLoadedReward)
        {
            Advertisement.Load(rewardedId, this);
            return;
        }

        Advertisement.Show(rewardedId, this);
    }

    public void ShowBanner()
    {
        if (DataManager.Instance.userData.isVIP)
        {
            return;
        }
        isWaitShowBanner = true;
        if (!isLoadedBanner)
        {
            //Advertisement.Banner.Load(bannerId, this);
            //Advertisement.Banner.Load(bannerId);
            MainThreadDispatcher.Instance.Enqueue(() => { Advertisement.Banner.Load(bannerId); });
            return;
        }
        MainThreadDispatcher.Instance.Enqueue(() => { Advertisement.Banner.Show(bannerId); });
        //Advertisement.Banner.Show(bannerId);
    }

    public void HideBanner(bool isDestroy = true)
    {
        Advertisement.Banner.Hide(isDestroy);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("OnInitializationComplete");
        isInit = true;
        StartCoroutine(DelayedAdLoad());
        //MainThreadDispatcher.Instance.Enqueue(() =>
        //{
        //    Advertisement.Banner.Load(bannerId);
        //    Advertisement.Load(rewardedId, this);
        //});
        //Advertisement.Banner.Load(bannerId);
        //Advertisement.Load(rewardedId, this);
    }

    IEnumerator DelayedAdLoad()
    {
        yield return null; // 한 프레임 대기 (메인 루프 완전 준비)
        Advertisement.Banner.Load(bannerId);
        Advertisement.Load(rewardedId, this);
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log("OnInitializationFailed "+error+" | "+message);
        MainThreadDispatcher.Instance.Enqueue(() =>
        {
            UIManager.Instance.Message.Show(Message.Type.Confirm, error.ToString(), callback: confirm =>
            {
                Initialize();
            });
        });
        
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        MainThreadDispatcher.Instance.Enqueue(() =>
        {
            if (placementId == bannerId)
            {
                isLoadedBanner = true;
                if (isWaitShowBanner)
                {
                    Advertisement.Banner.Show(bannerId);
                }
            }
            else
            {
                isLoadedReward = true;
                if (isWaitShowReward)
                {
                    Advertisement.Show(rewardedId, this);
                }
            }
        });
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        MainThreadDispatcher.Instance.Enqueue(() =>
        {
            Advertisement.Load(placementId, this);
            if (isWaitShowReward && placementId == rewardedId)
            {
                UIManager.Instance.Message.Show(Message.Type.Confirm, string.Format("RetryLoadAds", error.ToString(), message), callback: confirm =>
                {
                    if (confirm)
                    {
                        Advertisement.Load(rewardedId, this);
                    }
                    else
                    {
                        if (showCallback != null) showCallback.Invoke(false);
                    }

                });
            }
        });
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        MainThreadDispatcher.Instance.Enqueue(() =>
        {
            if (isWaitShowReward && placementId == rewardedId)
            {
                UIManager.Instance.Message.Show(Message.Type.Confirm, string.Format("RetryLoadAds", error.ToString(), message), callback: confirm =>
                {
                    if (confirm)
                    {
                        Advertisement.Show(rewardedId, this);
                    }
                    else
                    {
                        if (showCallback != null) { showCallback.Invoke(false); }
                    }
                });
            }
        });
        
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        if (placementId == rewardedId) isWaitShowReward = false;
        else isWaitShowBanner = false;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        throw new NotImplementedException();
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        MainThreadDispatcher.Instance.Enqueue(() =>
        {
            if (placementId == rewardedId)
            {
                if (showCallback != null) showCallback.Invoke(true);
                showCallback = null;
                Advertisement.Load(rewardedId, this);
            }
            else if (placementId == bannerId)
            {
                Advertisement.Load(bannerId, this);
            }
        });
        
        //throw new NotImplementedException();
    }

    public float GetEstimatedBannerHeight()
    {
        float dpi = Screen.dpi;
        if (dpi == 0) dpi = 160; // dpi 정보가 없는 경우 기본값

        float bannerDp = 50; // 일반적 배너 높이
        float bannerPx = bannerDp * (dpi / 160f);

        return bannerPx;
    }
}





