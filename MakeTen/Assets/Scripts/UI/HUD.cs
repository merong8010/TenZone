using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Text;
using System.Collections;
using System;
using System.Linq;

public class HUD : Singleton<HUD>
{
    [SerializeField]
    private Text levelText;
    [SerializeField]
    private Text expText;
    [SerializeField]
    private Text nameText;

    [SerializeField]
    private Text heartCount;
    [SerializeField]
    private Text heartChargeRemainTime;

    [SerializeField]
    private Text pointText;
    [SerializeField]
    private Text timeText;

    private Coroutine timeCoroutine;

    public void UpdateUserData(UserData data)
    {
        levelText.text = $"Lv.{data.level}";
        if(DataManager.Instance.Get<GameData.UserLevel>().ToList().Exists(x => x.level == data.level + 1))
        {
            expText.text = data.exp.ToProgressText(DataManager.Instance.Get<GameData.UserLevel>().SingleOrDefault(x => x.level == data.level + 1).exp);
        }
        else
        {
            expText.text = "MAX";
        }

        nameText.text = data.nickname;

        heartCount.text = $"Heart : {data.Heart}";

        int remain = (int)(data.nextHeartChargeTime - GameManager.Instance.dateTime.Value.ToTick());
        timeText.text = remain.ToTimeText();

        if (timeCoroutine != null) StopCoroutine(timeCoroutine);
        if (remain > 0) timeCoroutine = StartCoroutine(CheckRemain(remain));
        else heartChargeRemainTime.text = "MAX";
    }

    public void UpdateHeart()
    {
        heartCount.text = DataManager.Instance.userData.Heart.ToString();

        int remain = (int)(DataManager.Instance.userData.nextHeartChargeTime - GameManager.Instance.dateTime.Value.ToTick());
        timeText.text = remain.ToTimeText();

        if (timeCoroutine != null) StopCoroutine(timeCoroutine);
        if (remain > 0) timeCoroutine = StartCoroutine(CheckRemain(remain));
        else heartChargeRemainTime.text = "MAX";
    }

    private IEnumerator CheckRemain(int remain)
    {
        while(remain > 0)
        {
            yield return Yielders.Get(1f);
            remain -= 1;
            heartChargeRemainTime.text = remain.ToTimeText();
        }

        UpdateHeart();
    }

    private bool isInit = false;
    public void Initialize(ReactiveProperty<int> pointProperty)
    {
        pointProperty.Subscribe(x => { pointText.text = new StringBuilder().Append("point : ").Append(x).ToString(); });
        GameManager.Instance.reactiveTime.Subscribe(x =>
        {
            if(x.Ticks <= PuzzleManager.Instance.finishTime.Ticks)
            {
                TimeSpan timeSpan = (PuzzleManager.Instance.finishTime - x);
                timeText.text = string.Format("{0}:{1:00}.{2}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 100);
            }
            
        });
        //timePropoerty.Subscribe(x => { timeText.text = new StringBuilder().Append("time : ").Append(Mathf.RoundToInt(x)).ToString(); });
    }

    public void ClickRanking()
    {
        UIManager.Instance.Open<PopupRanking>();
    }

    public void ClickCharge()
    {
        DataManager.Instance.userData.ChargeHeart();
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
            FirebaseManager.Instance.TestSubmitScore(PuzzleManager.Level.Easy, FirebaseManager.KEY.RANKING_ALL, $"USER_{i}", $"USER_{i}", UnityEngine.Random.Range(50, 150), UnityEngine.Random.Range(0, 50000), countryCodes[UnityEngine.Random.Range(0, countryCodes.Length)]);
            FirebaseManager.Instance.TestSubmitScore(PuzzleManager.Level.Easy, GameManager.Instance.dateTime.Value.ToDateText(), $"USER_{i}", $"USER_{i}", UnityEngine.Random.Range(50, 150), UnityEngine.Random.Range(0, 50000), countryCodes[UnityEngine.Random.Range(0, countryCodes.Length)]);

            FirebaseManager.Instance.TestSubmitScore(PuzzleManager.Level.Normal, FirebaseManager.KEY.RANKING_ALL, $"USER_{i}", $"USER_{i}", UnityEngine.Random.Range(50, 150), UnityEngine.Random.Range(0, 50000), countryCodes[UnityEngine.Random.Range(0, countryCodes.Length)]);
            FirebaseManager.Instance.TestSubmitScore(PuzzleManager.Level.Normal, GameManager.Instance.dateTime.Value.ToDateText(), $"USER_{i}", $"USER_{i}", UnityEngine.Random.Range(50, 150), UnityEngine.Random.Range(0, 50000), countryCodes[UnityEngine.Random.Range(0, countryCodes.Length)]);

            FirebaseManager.Instance.TestSubmitScore(PuzzleManager.Level.Hard, FirebaseManager.KEY.RANKING_ALL, $"USER_{i}", $"USER_{i}", UnityEngine.Random.Range(50, 150), UnityEngine.Random.Range(0, 50000), countryCodes[UnityEngine.Random.Range(0, countryCodes.Length)]);
            FirebaseManager.Instance.TestSubmitScore(PuzzleManager.Level.Hard, GameManager.Instance.dateTime.Value.ToDateText(), $"USER_{i}", $"USER_{i}", UnityEngine.Random.Range(50, 150), UnityEngine.Random.Range(0, 50000), countryCodes[UnityEngine.Random.Range(0, countryCodes.Length)]);

            FirebaseManager.Instance.TestSubmitScore(PuzzleManager.Level.Expert, FirebaseManager.KEY.RANKING_ALL, $"USER_{i}", $"USER_{i}", UnityEngine.Random.Range(50, 150), UnityEngine.Random.Range(0, 50000), countryCodes[UnityEngine.Random.Range(0, countryCodes.Length)]);
            FirebaseManager.Instance.TestSubmitScore(PuzzleManager.Level.Expert, GameManager.Instance.dateTime.Value.ToDateText(), $"USER_{i}", $"USER_{i}", UnityEngine.Random.Range(50, 150), UnityEngine.Random.Range(0, 50000), countryCodes[UnityEngine.Random.Range(0, countryCodes.Length)]);
        }

    }
}
