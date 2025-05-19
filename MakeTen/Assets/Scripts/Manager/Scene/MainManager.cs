using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System;
using UniRx;

public class MainManager : Singleton<MainManager>
{
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


    [SerializeField]
    private Transform blockParent;
    [SerializeField]
    private Vector2Int blockCounts;
    [SerializeField]
    private Vector2 blockSize;
    [SerializeField]
    private Vector2 blockGap;

    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(InitBG());
        UpdateUserData(DataManager.Instance.userData);
        UpdateHeart();
    }

    private IEnumerator InitBG()
    {
        yield return new WaitUntil(() => ObjectPooler.Instance.isReady);
        Vector2 blockStartPos = blockStartPos = new Vector2(-(blockSize.x + blockGap.x) * (blockCounts.x - 1) * 0.5f, -(blockSize.y + blockGap.y) * (blockCounts.y - 1) * 0.5f);
        for (int x = 0; x < blockCounts.x; x++)
        {
            for (int y = 0; y < blockCounts.y; y++)
            {
                Block blockObj = ObjectPooler.Instance.Get<Block>("block_title", blockParent, blockStartPos + new Vector2((blockSize.x + blockGap.x) * x, (blockSize.y + blockGap.y) * y), Vector3.one);
                blockObj.SetSize(blockSize);
                blockObj.InitRandom();
            }
        }
    }

    public void Refresh()
    {
        //loginStatusText.text = DataManager.Instance.userData.authType.ToString();

        //googleLoginButton.SetActive(DataManager.Instance.userData.authType == FirebaseManager.AuthenticatedType.None);
        //logoutButton.SetActive(DataManager.Instance.userData.authType != FirebaseManager.AuthenticatedType.None);
    }

    //public void GameStart()
    //{
    //    UIManager.Instance.Open<PopupLevelSelect>();
    //}

    //public void GoogleLogin()
    //{
    //    FirebaseManager.Instance.StartGoogleLogin();
    //}

    //public void ClickLogOut()
    //{
    //    FirebaseManager.Instance.LogOut();
    //}

    private IDisposable disposable;

    public void UpdateUserData(UserData data)
    {
        levelText.text = $"Lv.{data.level}";
        if (DataManager.Instance.Get<GameData.UserLevel>().ToList().Exists(x => x.level == data.level + 1))
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

        if (disposable == null)
        {
            disposable = GameManager.Instance.reactiveTime.Subscribe(x =>
            {
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
}
