using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PopupRanking : Popup
{
    [SerializeField]
    private RankingList rankingList;
    public override void Open()
    {
        base.Open();
        FirebaseManager.Instance.GetTopScores(100, datas =>
        {
            rankingList.UpdateList(datas);

            string[] countryCodes = datas.Select(x => x.countryCode).Distinct().ToArray();
            foreach (string countryCode in countryCodes)
            {
                DataManager.Instance.GetFlags(countryCode, flag =>
                {
                    //List<RankingListItem> items = (List<RankingListItem>)rankingList.GetList().Where(x => x.GetData().countryCode == countryCode);
                    //for (int i = 0; i < items.Count; i++)
                    //{
                    //    items[i].UpdateFlag(flag);
                    //}
                    rankingList.UpdateFlags(countryCode, flag);
                });
            }
        });
        
    }

    public void UpdateFlags(string countryCode, Sprite sprite)
    {
        //foreach (RankingListItem item in rankingList.GetList().Where(x => x.GetData().countryCode == countryCode))
        //{
        //    item.UpdateFlag(sprite);
        //}
    }
}
