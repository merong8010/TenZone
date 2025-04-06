using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Text;
using System.Collections;

public class HUD : Singleton<HUD>
{
    [SerializeField]
    private Text heartCount;
    [SerializeField]
    private Text heartChargeRemainTime;

    [SerializeField]
    private Text pointText;
    [SerializeField]
    private Text timeText;

    private Coroutine timeCoroutine;
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
    public void Initialize(ReactiveProperty<int> pointProperty, ReactiveProperty<float> timePropoerty)
    {
        pointProperty.Subscribe(x => { pointText.text = new StringBuilder().Append("point : ").Append(x).ToString(); });
        timePropoerty.Subscribe(x => { timeText.text = new StringBuilder().Append("time : ").Append(Mathf.RoundToInt(x)).ToString(); });
    }

    public void ClickRanking()
    {
        UIManager.Instance.Open<PopupRanking>();
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
        for(int i = 0; i < 50; i++)
        {
            FirebaseManager.Instance.TestSubmitScore(id + "_" + i, Random.Range(scoreMin, scoreMax), countryCodes[Random.Range(0, countryCodes.Length)]);
        }
        
    }
}
