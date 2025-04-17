using UnityEngine;
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
        base.Open();
        Initialize();
        Refresh();

        //FirebaseManager.Instance.GetRankingFromServer(DataManager.Instance.userData.id, result =>
        //{
        //    rankingList.UpdateList(result.topRanks.Select(x => (RankingList.Data)x).ToList());
        //    myRankItem.SetData(result.myRank);
        //}, date:currentTypeIdx == 0 ? GameManager.Instance.dateTime.Value.ToDateText() : "ALL", limit : 50, (PuzzleManager.Level)currentLevelIdx);
        //FirebaseManager.Instance.GetTopScores(100, datas =>
        //{
        //    rankingList.UpdateList(datas);

        //    //string[] countryCodes = datas.Select(x => x.countryCode).Distinct().ToArray();
        //    //foreach (string countryCode in countryCodes)
        //    //{
        //    //    DataManager.Instance.GetFlags(countryCode, flag =>
        //    //    {
        //    //        //List<RankingListItem> items = (List<RankingListItem>)rankingList.GetList().Where(x => x.GetData().countryCode == countryCode);
        //    //        //for (int i = 0; i < items.Count; i++)
        //    //        //{
        //    //        //    items[i].UpdateFlag(flag);
        //    //        //}
        //    //        rankingList.UpdateFlags(countryCode, flag);
        //    //    });
        //    //}
        //});

    }

    public override void Refresh()
    {
        base.Refresh();
        Debug.Log("Refresh");
        string date = currentTypeIdx == 0 ? GameManager.Instance.dateTime.Value.ToDateText() : "ALL";

        //UIManager.Instance.Loading("Loading Rank");
        FirebaseManager.Instance.GetRankingFromServer(DataManager.Instance.userData.id, result =>
        {
            //UIManager.Instance.CloseLoading();
            Debug.Log($"GetRankingFromServer : {result.topRanks?.Count}");
            if (result == null) return;
            if(result.topRanks != null)
            {
                rankingList.UpdateList(result.topRanks.ToArray());
                myRankItem.SetData(result.myRank);
            }
        }, date, 50, (PuzzleManager.Level)(currentLevelIdx + 1));
    }
}
