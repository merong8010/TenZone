using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class PopupRanking : Popup
{
    public class RankingListWithMyRank
    {
        public List<RankingList.PointData> topRanks;
        public RankingList.PointData myRank;
    }
    [SerializeField]
    private TabGroup levelTabs;
    [SerializeField]
    private TabGroup typeTabs;

    [SerializeField]
    private RankingList rankingList;
    [SerializeField]
    private RankingListItem myRankItem;

    private bool isInit = false;

    private int currentLevelIdx;
    private int currentTypeIdx;

    [SerializeField]
    private Text stateText;

    private void Initialize()
    {
        if (isInit) return;
        isInit = true;

        levelTabs.Init(0, idx =>
        {
            currentLevelIdx = idx;
            Refresh();
        });

        typeTabs.Init(0, idx =>
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

        stateText.text = "Loading";
        rankingList.gameObject.SetActive(false);
        myRankItem.gameObject.SetActive(false);

        string date = currentTypeIdx == 0 ? GameManager.Instance.dateTime.Value.ToDateText() : "ALL";

        FirebaseManager.Instance.GetRankingFromServer(DataManager.Instance.userData.id, result =>
        {
            if (result == null)
            {
                stateText.text = TextManager.Get("FailLoadRank");
                return;
            }
            stateText.text = string.Empty;

            if (result.topRanks != null)
            {
                rankingList.gameObject.SetActive(true);
                rankingList.UpdateList(result.topRanks.Select(x=> (RankingList.Data)x).ToList());
            }

            if(result.myRank != null)
            {
                myRankItem.gameObject.SetActive(true);
                myRankItem.SetData(result.myRank);
            }
        }, date, 50, (PuzzleManager.Level)(currentLevelIdx + 1));
    }
}
