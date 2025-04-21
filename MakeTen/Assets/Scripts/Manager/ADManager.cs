using System.Collections;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Advertisements;
//using Advertise
/// <summary>
/// Advertise Manager
/// </summary>
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
    //Organization Core ID
    //14569505087296

    //Game ID
    //iOS 5838018

    //Android 5838019

    //Monetization Stats API Key
    //92d5c848e7f474cb5d7dee72b51d8b4018ca74a6d0aff2428280e01cd8cef5ea

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
    private Action rewardCallback;

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
            isWaitShowBanner = true;
        }
    }

    public void ShowReward(Action callback)
    {
        if(DataManager.Instance.userData.isVIP)
        {
            callback?.Invoke();
            return;
        }

        rewardCallback = callback;
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
            Advertisement.Load(bannerId, this);
            return;
        }

        Advertisement.Show(bannerId, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("OnInitializationComplete");
        isInit = true;
        Advertisement.Load(bannerId, this);
        Advertisement.Load(rewardedId, this);
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log("OnInitializationFailed "+error+" | "+message);
        UIManager.Instance.Message.Show(Message.Type.Confirm, error.ToString(), callback: confirm =>
        {
            Initialize();
        });
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        if(placementId == bannerId)
        {
            isLoadedReward = true;
            if (isWaitShowBanner)
            {
                Advertisement.Show(bannerId, this);
            }
        }
        else
        {
            isLoadedReward = true;
            if(isWaitShowReward)
            {
                Advertisement.Show(rewardedId, this);
            }
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Advertisement.Load(placementId, this);
        if(isWaitShowReward && placementId == rewardedId)
        {
            UIManager.Instance.Message.Show(Message.Type.Confirm, string.Format("RetryLoadAds", error.ToString(), message), callback: confirm =>
            {
                if (confirm)
                    Advertisement.Load(rewardedId, this);
            });
        }
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        if (isWaitShowReward && placementId == rewardedId)
        {
            UIManager.Instance.Message.Show(Message.Type.Confirm, string.Format("RetryLoadAds", error.ToString(), message), callback: confirm =>
            {
                if (confirm)
                    Advertisement.Show(rewardedId, this);
            });
        }
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
        if(placementId == rewardedId)
        {
            if (rewardCallback != null) rewardCallback.Invoke();
            rewardCallback = null;
            Advertisement.Load(rewardedId, this);
        }
        else if(placementId == bannerId)
        {
            Advertisement.Load(bannerId, this);
        }
        //throw new NotImplementedException();
    }
}





