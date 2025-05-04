using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Text;
using System.Collections;
using System;
using System.Linq;

public class HUD : Singleton<HUD>
{
    [Header("Main")]
    [SerializeField]
    private AnimationRect mainRect;

    [SerializeField]
    private TMPro.TextMeshProUGUI levelText;
    [SerializeField]
    private TMPro.TextMeshProUGUI expText;
    [SerializeField]
    private Image expBar;

    [SerializeField]
    private Text nameText;

    [SerializeField]
    private TMPro.TextMeshProUGUI heartCount;
    [SerializeField]
    private TMPro.TextMeshProUGUI heartChargeRemainTime;

    [Header("Puzzle")]
    [SerializeField]
    private AnimationRect puzzleRect;
    [SerializeField]
    private TMPro.TextMeshProUGUI pointText;
    [SerializeField]
    private TMPro.TextMeshProUGUI timeText;
    [SerializeField]
    private Image timeBar;
    //[SerializeField]
    //private GoodsDisplay shuffleDisplay;
    //[SerializeField]
    //private GoodsDisplay autoBreakDisplay;

    private IDisposable disposable;

    public void UpdateUserData(UserData data)
    {
        levelText.text = $"Lv.{data.level}";
        if(DataManager.Instance.Get<GameData.UserLevel>().ToList().Exists(x => x.level == data.level + 1))
        {
            int max = DataManager.Instance.Get<GameData.UserLevel>().SingleOrDefault(x => x.level == data.level + 1).exp;
            expText.text = data.exp.ToProgressText(DataManager.Instance.Get<GameData.UserLevel>().SingleOrDefault(x => x.level == data.level + 1).exp);
            expBar.fillAmount = (float)data.exp / max;
        }
        else
        {
            expText.text = "MAX";
            expBar.fillAmount = 1f;
        }

        nameText.text = data.nickname;
        heartCount.text = data.Heart.ToString();

        if(disposable == null)
        {
            disposable = GameManager.Instance.reactiveTime.Subscribe(x =>
            {
                //Debug.Log(data.Heart);
                //if(data.Heart >= DataManager.Instance.MaxHeart)
                //{
                //    heartChargeRemainTime.text = "MAX";
                //}
                //else
                //{
                //    int passedSec = (int)(x.ToTick() - DataManager.Instance.userData.lastHeartTime);
                //    heartChargeRemainTime.text = (DataManager.Instance.HeartChargeTime - passedSec).ToTimeText();
                //}
                UpdateHeart();
            });
        }
    }

    public void UpdateHeart()
    {
        heartCount.text = DataManager.Instance.userData.Heart.ToString();
        if (DataManager.Instance.userData.Heart >= DataManager.Instance.MaxHeart)
        {
            heartChargeRemainTime.text = "MAX";
        }
        else
        {
            int passedSec = (int)(GameManager.Instance.dateTime.Value.ToTick() - DataManager.Instance.userData.lastHeartTime);
            heartChargeRemainTime.text = (DataManager.Instance.HeartChargeTime - passedSec).ToTimeText();
        }
        //int passedSec = (int)(GameManager.Instance.dateTime.Value.ToTick() - DataManager.Instance.userData.lastHeartTime);
        //heartChargeRemainTime.text = (DataManager.Instance.HeartChargeTime - passedSec).ToTimeText();
    }

    public void Initialize(ReactiveProperty<int> pointProperty)
    {
        pointProperty.Subscribe(x => { pointText.text = new StringBuilder().Append("point : ").Append(x).ToString(); });
        GameManager.Instance.reactiveTime.Subscribe(x =>
        {
            if(x.Ticks <= PuzzleManager.Instance.finishTime.Ticks)
            {
                TimeSpan timeSpan = (PuzzleManager.Instance.finishTime - x);
                timeText.text = string.Format("{0}:{1:00}.{2}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 100);
                timeBar.fillAmount = (float)timeSpan.TotalSeconds / PuzzleManager.Instance.CurrentGameTime;
            }
        });
    }

    public void ClickGameStart()
    {
        UIManager.Instance.Open<PopupLevelSelect>();
    }

    public void ClickSetting()
    {
        UIManager.Instance.Open<PopupSettings>();
    }

    public void ClickNickname()
    {
        UIManager.Instance.Open<PopupNickname>();
    }

    public void ClickMail()
    {
        UIManager.Instance.Open<PopupMail>();
    }

    public void ClickRanking()
    {
        UIManager.Instance.Open<PopupRanking>();
    }

    public void ClickShop()
    {
        UIManager.Instance.Open<PopupShop>();
    }
    public void ClickAttendance()
    {
        UIManager.Instance.Open<PopupAttendance>();
    }

    public void ClickCheat()
    {
        UIManager.Instance.Open<PopupCheat>();
        //DataManager.Instance.userData.ChargeHeart();
    }

    public void ClickPause()
    {
        PuzzleManager.Instance.IsPause = true;
        UIManager.Instance.Message.Show(Message.Type.Ask, TextManager.Get("PuzzleQuit"), callback : (yes) =>
        {
            if (yes) GameManager.Instance.GoScene(GameManager.Scene.Main);
            else PuzzleManager.Instance.IsPause = false;
        });
    }

    public void ClickShuffle()
    {
        //PuzzleManager.Instance.IsPause = true;
        UIManager.Instance.Message.Show(Message.Type.Ask, TextManager.Get("PuzzleQuit"), callback: (yes) =>
        {
            if (yes) PuzzleManager.Instance.Shuffle();
            //PuzzleManager.Instance.IsPause = false;
        });
        
    }

    [SerializeField]
    private Image searchCoolImage;
    public void ClickSearch()
    {
        PuzzleManager.Instance.Search();
    }

    public void StartSearchCool(DateTime coolFinish, float max)
    {
        StartCoroutine(CheckSearchCool(coolFinish, max));
    }

    private IEnumerator CheckSearchCool(DateTime coolFinish, float max)
    {
        while(coolFinish.Ticks > GameManager.Instance.dateTime.Value.Ticks)
        {
            searchCoolImage.fillAmount = ((coolFinish.Ticks - GameManager.Instance.dateTime.Value.Ticks) / 10000000f) / max;
            yield return Yielders.EndOfFrame;
        }
    }

    [SerializeField]
    private Image explodeCoolImage;
    public void ClickExplode()
    {
        PuzzleManager.Instance.Explode();
    }

    public void StartExplodeCool(DateTime coolFinish, float max)
    {
        StartCoroutine(CheckExplodeCool(coolFinish, max));
    }

    private IEnumerator CheckExplodeCool(DateTime coolFinish, float max)
    {
        while (coolFinish.Ticks > GameManager.Instance.dateTime.Value.Ticks)
        {
            explodeCoolImage.fillAmount = ((coolFinish.Ticks - GameManager.Instance.dateTime.Value.Ticks) / 10000000f) / max;
            yield return Yielders.EndOfFrame;
        }
    }

    public void UpdateScene(GameManager.Scene scene)
    {
        UIManager.Instance.ShowBG(scene != GameManager.Scene.Puzzle);
        mainRect.Show(scene == GameManager.Scene.Main);
        puzzleRect.Show(scene == GameManager.Scene.Puzzle);
    }

    public void ShowMain(bool show)
    {
        mainRect.Show(show);
    }

    public void ShowPuzzle(bool show)
    {
        puzzleRect.Show(show);
    }

    public void ShowAddSeconds(float sec)
    {
        ObjectPooler.Instance.Get<Effect>("add_seconds", timeBar.transform.parent, timeBar.transform.localPosition, autoReturnTime: 1f).SetText($"+{sec}s");
    }

    [SerializeField]
    private string id;
    [SerializeField]
    private int scoreMin;
    [SerializeField]
    private int scoreMax;
    [SerializeField]
    private string[] countryCodes;
    public void TestSubmit()
    {
        for (int i = 0; i < 50; i++)
        {
            int pointEasy = UnityEngine.Random.Range(50, 150);
            int pointNormal = UnityEngine.Random.Range(50, 150);
            int pointHard = UnityEngine.Random.Range(50, 150);
            int pointExpert = UnityEngine.Random.Range(50, 150);
            string id = $"Rank_{i}";
            string nickname = $"Player_{Util.GenerateRandomCode()}";
            string countryCode = countryCodes[UnityEngine.Random.Range(0, countryCodes.Length)];
            string date = GameManager.Instance.dateTime.Value.ToDateText();
            int level = UnityEngine.Random.Range(10, 90);
            FirebaseManager.Instance.SubmitScore(PuzzleManager.Level.Easy, FirebaseManager.KEY.RANKING_ALL, id, nickname, level, pointEasy, countryCode);
            FirebaseManager.Instance.SubmitScore(PuzzleManager.Level.Easy, date, id, nickname, level, pointEasy, countryCode);
            FirebaseManager.Instance.SubmitScore(PuzzleManager.Level.Normal, FirebaseManager.KEY.RANKING_ALL, id, nickname, level, pointNormal, countryCode);
            FirebaseManager.Instance.SubmitScore(PuzzleManager.Level.Normal, date, id, nickname, level, pointNormal, countryCode);
            FirebaseManager.Instance.SubmitScore(PuzzleManager.Level.Hard, FirebaseManager.KEY.RANKING_ALL, id, nickname, level, pointHard, countryCode);
            FirebaseManager.Instance.SubmitScore(PuzzleManager.Level.Hard, date, id, nickname, level, pointHard, countryCode);
            FirebaseManager.Instance.SubmitScore(PuzzleManager.Level.Expert, FirebaseManager.KEY.RANKING_ALL, id, nickname, level, pointExpert, countryCode);
            FirebaseManager.Instance.SubmitScore(PuzzleManager.Level.Expert, date, id, nickname, level, pointExpert, countryCode);
        }

    }

    
}
