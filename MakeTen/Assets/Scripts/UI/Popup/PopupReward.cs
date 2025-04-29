using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PopupReward : Popup
{
    [SerializeField]
    private Text titleText;
    [SerializeField]
    private GoodsList rewardList;

    public void SetData(List<GoodsList.Data> datas, string titleKey = "Reward")
    {
        titleText.text = TextManager.Get(titleKey);
        rewardList.UpdateList(datas);
    }

    public void SetData(GoodsList.Data[] datas)
    {
        rewardList.UpdateList(datas);
    }
}
