using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
/// <summary>
/// Advertise Manager
/// </summary>
public class ADManager : Singleton<ADManager>
{
    //private const string maxSdkKey = "1f7NNgB9E4w-7idGBBcbq3wvbmFFonHpQPBp4NzJRuITbwqn6QMwUHq423KqqbdF_q24B0OdugBTb0EhQbcwuQ";
    //private const string adUnitId = "";
    private const string bannerId = "88aec1eaee4f191d";
    private const string interId = "b3c26fbbfa31f8ea";
#if UNITY_ANDROID 
    private const string rewardId = "67ba5712196753af";
#elif UNITY_IOS
    private const string rewardId = "e2c6a320f0083a99";
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

    public bool IsFree;

    protected override void Awake()
    {
        base.Awake();
    }

    //    private bool isInitialize = false;

    //    public void Initialize()
    //    {
    //        if (isInitialize) return;
    //        isInitialize = true;

    //        Debug.Log("ADManager.Initialize");
    //        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
    //        {
    //            InitializeRewardedAds();
    //        };

    //        MaxSdk.InitializeSdk();
    //    }

    //    //private AdvertiseCallback currentRewardCallback;
    //    public void ShowRewardVideo(int id, AdvertiseCallback callback)
    //    {
    //        this.callback = callback;
    //        currentRewardId = id;

    //        if (DataManager.IsAdsRemoved())
    //        {
    //            CompleteVideo();
    //            return;
    //        }
    //#if UNITY_EDITOR
    //        UISystem.Instance.Open<PopupDialogBox>().Init("Advertise TEST", "this is Advertise TEST!\nDo you want reward?", TextManager.Get("yes"), TextManager.Get("no"),
    //            delegate (bool yes)
    //            {
    //                if (yes) CompleteVideo();
    //                else CancelVideo();
    //            });
    //#else
    //         if (!GameManager.Instance.CurrentServer.isAdvertiseTest)
    //         {
    //            if (MaxSdk.IsRewardedAdReady(rewardId))
    //            {
    //                BackendChatManagerV2.Instance.Dispose();
    //                MaxSdk.ShowRewardedAd(rewardId);
    //            }
    //            else
    //            {
    //                UISystem.Instance.Open<PopupDialogBox>().Init(TextManager.Get("notice"), TextManager.Get("no_ads_retry"), TextManager.Get("yes"));
    //                LoadRewardedAd();
    //            }
    //         }
    //         else
    //         {
    //            UISystem.Instance.Open<PopupDialogBox>().Init("Advertise TEST", "this is Advertise TEST!\nDo you want reward?", TextManager.Get("yes"), TextManager.Get("no"),
    //            delegate (bool yes)
    //            {
    //                if (yes) CompleteVideo();
    //                else CancelVideo();
    //            });
    //         }
    //#endif

    //        //#endif
    //    }

    //    private int currentRewardId;

    //    public void CompleteVideo()
    //    {
    //        if (callback != null)
    //        {
    //            callback.Invoke(Result.SUCCESS);
    //            callback = null;

    //            StartCoroutine(BackendChatManagerV2.Instance.Init());
    //            DataManager.Instance.QuestAction(Chart.QuestType.WATCH_AD, 1);
    //        }
    //    }

    //    public void CancelVideo()
    //    {
    //        if (callback != null)
    //        {
    //            callback.Invoke(Result.FAIL);
    //            callback = null;
    //        }
    //    }


    //    int retryAttempt;

    //    public void InitializeRewardedAds()
    //    {
    //        Debug.Log("InitializeRewardedAds");
    //        // Attach callback
    //        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
    //        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
    //        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
    //        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
    //        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
    //        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
    //        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
    //        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

    //        LoadRewardedAd();
    //    }

    //    private void LoadRewardedAd()
    //    {
    //        MaxSdk.LoadRewardedAd(rewardId);
    //    }

    //    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    //    {

    //        Debug.Log("OnRewardedAdLoadedEvent   adUnitId : " + adUnitId);
    //        // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.

    //        // Reset retry attempt
    //        retryAttempt = 0;
    //    }

    //    private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    //    {
    //        Debug.Log("OnRewardedAdLoadFailedEvent adUnitId : " + adUnitId + " | errorInfo.Message : " + errorInfo.Message);
    //        // Rewarded ad failed to load 
    //        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).

    //        retryAttempt++;
    //        double retryDelay = Math.Pow(2, Math.Min(6, retryAttempt));

    //        Invoke("LoadRewardedAd", (float)retryDelay);
    //    }

    //    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    //    {
    //        Debug.Log("OnRewardedAdFailedToDisplayEvent  adUnitId : " + adUnitId + " | errorInfo : " + adInfo.NetworkName);
    //        // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
    //        //LoadRewardedAd();

    //        //CompleteVideo();
    //    }

    //    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    //    {
    //        Debug.Log("OnRewardedAdFailedToDisplayEvent  adUnitId : " + adUnitId + " | errorInfo : " + errorInfo.Message);
    //        // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
    //        LoadRewardedAd();
    //        CancelVideo();
    //    }

    //    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    //    private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    //    {
    //        // Rewarded ad is hidden. Pre-load the next ad
    //        LoadRewardedAd();
    //    }

    //    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    //    {
    //        Debug.Log("OnRewardedAdReceiveRewardEvent   " + adUnitId + " || reward : " + reward + "  || " + reward.Amount + " || " + adInfo.NetworkName);
    //        CompleteVideo();
    //        // The rewarded ad displayed and the user should receive the reward.
    //    }

    //    private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    //    {
    //        Debug.Log("OnRewardedAdRevenuePaidEvent   " + adUnitId + "  || " + adInfo);
    //        //OnRewardedAdRevenuePaidEvent
    //        // Ad revenue paid. Use this callback to track user revenue.
    //    }
}




//using UnityEngine;
//using UnityEngine.Advertisements;

//public class AdManager : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
//{
//    public static AdManager Instance;

//    [Header("Ad Unit IDs")]
//    [SerializeField] private string androidGameId = "YOUR_ANDROID_GAME_ID";
//    [SerializeField] private string iosGameId = "YOUR_IOS_GAME_ID";

//    [SerializeField] private string interstitialAdUnitId = "Interstitial_Android";
//    [SerializeField] private string rewardedAdUnitId = "Rewarded_Android";

//    private string gameId;
//    private bool testMode = true;

//    void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//            InitializeAds();
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    void InitializeAds()
//    {
//#if UNITY_ANDROID
//        gameId = androidGameId;
//#elif UNITY_IOS
//        gameId = iosGameId;
//#endif
//        Advertisement.Initialize(gameId, testMode);
//    }

//    #region 전면 광고
//    public void ShowInterstitialAd()
//    {
//        if (Advertisement.IsReady(interstitialAdUnitId))
//        {
//            Advertisement.Show(interstitialAdUnitId, this);
//        }
//        else
//        {
//            Debug.Log("전면 광고 준비 안됨");
//            Advertisement.Load(interstitialAdUnitId, this);
//        }
//    }
//    #endregion

//    #region 보상형 광고
//    public void ShowRewardedAd(System.Action onSuccess)
//    {
//        if (Advertisement.IsReady(rewardedAdUnitId))
//        {
//            Advertisement.Show(rewardedAdUnitId, new ShowOptions
//            {
//                resultCallback = result =>
//                {
//                    if (result == ShowResult.Finished)
//                    {
//                        Debug.Log("보상형 광고 성공!");
//                        onSuccess?.Invoke();
//                    }
//                    else
//                    {
//                        Debug.LogWarning("광고 중단 또는 실패");
//                    }
//                }
//            });
//        }
//        else
//        {
//            Debug.Log("보상형 광고 준비 안됨");
//            Advertisement.Load(rewardedAdUnitId, this);
//        }
//    }
//    #endregion

//    #region 광고 콜백 (선택)
//    public void OnUnityAdsAdLoaded(string adUnitId)
//    {
//        Debug.Log($"{adUnitId} 광고 로드 완료");
//    }

//    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
//    {
//        Debug.LogError($"광고 로드 실패: {adUnitId} - {error.ToString()} - {message}");
//    }

//    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
//    {
//        Debug.Log($"{adUnitId} 광고 종료 상태: {showCompletionState}");
//    }

//    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
//    {
//        Debug.LogError($"광고 표시 실패: {adUnitId} - {message}");
//    }

//    public void OnUnityAdsShowStart(string adUnitId) { }
//    public void OnUnityAdsShowClick(string adUnitId) { }
//    #endregion
//}
