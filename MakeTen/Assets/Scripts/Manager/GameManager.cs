using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using System.Text;
using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    protected override void Awake()
    {
        base.Awake();

        System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo("en-US");
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        //StartCoroutine(Initialize());
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
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
        FetchOnlineTime();
    }

    private void Pause()
    {
        //_currentTime = null;
    }

    private DateTime? _currentTime = null; // 시간 저장 (정상적으로 가져오지 못하면 null)
    public DateTime? dateTime => _currentTime != null ? _currentTime.Value : null;
    public ReactiveProperty<DateTime> reactiveTime = new ReactiveProperty<DateTime>();
    public GameData.GameLevel currentLevel;
    private Scene currentScene;
    public bool isUse10Seconds;
    // ✅ 구글 서버에서 UTC 시간 가져오기
    public void FetchOnlineTime()
    {
        //_currentTime = null;
        UIManager.Instance.Loading(delay: 0f);
        StartCoroutine(GetOnlineTime());
    }

    public bool isOffline = false;

    private IEnumerator GetOnlineTime()
    {
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            UIManager.Instance.CloseLoading();
            UIManager.Instance.Message.Show(Message.Type.Ask, TextManager.Get("noInternet"), callback: confirm =>
            {
                if(confirm)
                {
                    _currentTime = DateTime.UtcNow;
                    isOffline = true;
                }
                else
                {
                    FetchOnlineTime();
                }
            });
            yield break;
        }
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
        if (_currentTime != null)
        {
            _currentTime = _currentTime.Value.AddSeconds(Time.realtimeSinceStartupAsDouble - lastCheckTime);
            lastCheckTime = Time.realtimeSinceStartupAsDouble;
            reactiveTime.Value = _currentTime.Value;
        }
    }

    private bool isInit = false;
    public void Init()
    {
        if (isInit) return;
        isInit = true;
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        GoScene(Scene.Title);
        FetchOnlineTime();
        TitleManager.Instance.SetStatus("Check Time");
        yield return new WaitUntil(() => _currentTime != null);
        TitleManager.Instance.SetStatus("Check Server State");
        yield return new WaitUntil(() => FirebaseManager.Instance.IsReady || isOffline);
        TitleManager.Instance.SetStatus("Check Game Datas");
        DataManager.Instance.LoadGameDatas();
        yield return new WaitUntil(() => DataManager.Instance.IsLoadComplete);
        TitleManager.Instance.SetStatus("", showTap: true);
        IAPManager.Instance.TryInitialize();
        ADManager.Instance.Initialize();
    }

    public enum Scene
    {
        Title,
        Main,
        Puzzle,
    }
    public void GoScene(Scene scene, GameData.GameLevel level = null, bool use10Seconds = false)
    {
        //UIManager.Instance.Loading(callback: () =>
        //{
        //    HUD.Instance.ShowMain(false);
        //    HUD.Instance.ShowPuzzle(false);
        //    switch (scene)
        //    {
        //        case Scene.Title:
        //            PuzzleManager.Instance.gameObject.SetActive(false);
        //            UIManager.Instance.Main.gameObject.SetActive(false);
        //            break;
        //        case Scene.Main:
        //            UIManager.Instance.Title.gameObject.SetActive(false);
        //            PuzzleManager.Instance.gameObject.SetActive(false);
        //            UIManager.Instance.Main.gameObject.SetActive(true);
        //            break;
        //        case Scene.Puzzle:
        //            PuzzleManager.Instance.gameObject.SetActive(true);
        //            PuzzleManager.Instance.ClearBlocks();
        //            UIManager.Instance.Main.gameObject.SetActive(false);
        //            break;
        //    }
        //}, completeCallback: () =>
        //{
        //    switch (scene)
        //    {
        //        case Scene.Title:
        //            //HUD.Instance.UpdateScene(Scene.Title);
        //            break;
        //        case Scene.Main:
        //            HUD.Instance.ShowMain(true);
        //            UIManager.Instance.Refresh();
        //            ADManager.Instance.ShowBanner();
        //            break;
        //        case Scene.Puzzle:
        //            HUD.Instance.ShowPuzzle(true);
        //            PuzzleManager.Instance.GameStart(level, use10Seconds);
        //            ADManager.Instance.ShowBanner();
        //            break;
        //    }

        //});
        
        isUse10Seconds = use10Seconds;
        currentLevel = level;
        //StartCoroutine(GoScene(scene));
        UIManager.Instance.Loading(onFadeInComplete: () =>
        {
            SceneManager.LoadScene((int)scene);
            switch(currentScene)
            {
                case Scene.Title:
                    
                    break;
                case Scene.Main:
                    MainManager.Instance.ReturnBlockObj();
                    break;
                case Scene.Puzzle:
                    PuzzleManager.Instance.ReturnBlockObj();
                    break;
            }

            if(currentScene == Scene.Title && scene == Scene.Main)
            {
                UIManager.Instance.InitializePopups(
                    typeof(PopupAttendance),
                    typeof(PopupCheat),
                    typeof(PopupLevelSelect),
                    typeof(PopupMail),
                    typeof(PopupNickname),
                    typeof(PopupRanking),
                    typeof(PopupResult),
                    typeof(PopupReward),
                    typeof(PopupSettings),
                    typeof(PopupShop)
                );
            }
        }, onFadeOutComplete: () =>
        {
            currentScene = scene;
            switch (currentScene)
            {
                case Scene.Title:

                    break;
                case Scene.Main:
                    if (DataManager.Instance.userData.IsAttendanceRewardable)
                    {
                        UIManager.Instance.Open<PopupAttendance>();
                    }
                    break;
                case Scene.Puzzle:
                    break;
            }
        });
    }

    //private IEnumerator GoScene(Scene scene)
    //{
    //    UIManager.Instance.Loading(callback: () =>
    //    {
    //        SceneManager.LoadScene((int)scene);
    //    }, completeCallback:()=>
    //    {
            
    //    });
        
        
    //}

    public void SetScreenRoate(bool isPortrait, bool isRotate)
    {
        if(isPortrait)
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = isRotate;
            Screen.autorotateToLandscapeRight = isRotate;
        }
        else
        {
            Screen.orientation = Screen.orientation == ScreenOrientation.Portrait ? ScreenOrientation.LandscapeLeft : Screen.orientation;
            Screen.autorotateToPortrait = isRotate;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }
    }
}
