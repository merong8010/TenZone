using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class PopupRanking : Popup
{
    public class RankingListWithMyRank
    {
        public List<RankingList.Data> topRanks;
        public RankingList.Data myRank;
    }
    [SerializeField]
    private TabGroup levelTabsPortrait;
    [SerializeField]
    private TabGroup levelTabsLandscape;

    [SerializeField]
    private TabGroup typeTabsPortrait;
    [SerializeField]
    private TabGroup typeTabsLandscape;

    [SerializeField]
    private RankingList rankingListPortrait;
    [SerializeField]
    private RankingList rankingListLandscape;
    [SerializeField]
    private RankingListItem myRankItemPortrait;
    [SerializeField]
    private RankingListItem myRankItemLandscape;

    private bool isInit = false;

    private int currentLevelIdx;
    private int currentTypeIdx;
    [SerializeField]
    private GameObject loadingObj;
    [SerializeField]
    private Text stateText;

    private void Initialize()
    {
        if (isInit) return;
        isInit = true;

        levelTabsPortrait.Init(0, idx =>
        {
            currentLevelIdx = idx;
            Refresh();
        });

        levelTabsLandscape.Init(0, idx =>
        {
            currentLevelIdx = idx;
            Refresh();
        });

        typeTabsPortrait.Init(0, idx =>
        {
            currentTypeIdx = idx;
            Refresh();
        });
        typeTabsLandscape.Init(0, idx =>
        {
            currentTypeIdx = idx;
            Refresh();
        });
    }

    public override void Open()
    {
        Initialize();
        base.Open();
    }

    public override void Refresh()
    {
        base.Refresh();
        loadingObj.SetActive(true);
        stateText.text = "Loading";
        rankingListPortrait.gameObject.SetActive(false);
        rankingListLandscape.gameObject.SetActive(false);
        myRankItemPortrait.gameObject.SetActive(false);
        myRankItemLandscape.gameObject.SetActive(false);

        string date = currentTypeIdx == 0 ? GameManager.Instance.dateTime.Value.ToDateText() : "ALL";

        FirebaseManager.Instance.GetRankingFromServer(FirebaseManager.Instance.UserId, result =>
        {
            loadingObj.SetActive(false);
            if (result == null)
            {
                stateText.text = TextManager.Get("FailLoadRank");
                return;
            }
            stateText.text = string.Empty;

            if (result.topRanks != null)
            {
                rankingListPortrait.gameObject.SetActive(true);
                rankingListPortrait.UpdateList(result.topRanks.Select(x=> (RankingList.Data)x).ToList());

                rankingListLandscape.gameObject.SetActive(true);
                rankingListLandscape.UpdateList(result.topRanks.Select(x => (RankingList.Data)x).ToList());
            }

            if(result.myRank != null)
            {
                myRankItemPortrait.gameObject.SetActive(true);
                myRankItemPortrait.SetData(result.myRank);
                myRankItemLandscape.gameObject.SetActive(true);
                myRankItemLandscape.SetData(result.myRank);
            }
        }, date, 50, (GameData.PuzzleLevel)(currentLevelIdx + 1));
    }
}
