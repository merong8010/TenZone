using UnityEngine;
using System.Collections.Generic;

public class PopupReward : Popup
{
    [SerializeField]
    private GoodsList rewardList;

    public void SetData(List<GoodsList.Data> datas)
    {
        rewardList.UpdateList(datas);
    }

    public void SetData(GoodsList.Data[] datas)
    {
        rewardList.UpdateList(datas);
    }
}
