using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PopupShop : Popup
{
    [SerializeField]
    private TabGroup tab;
    private bool isInit = false;

    private int currentTabIdx;
    [SerializeField]
    private ShopList shopList;

    private void Init()
    {
        if (isInit) return;
        isInit = true;

        tab.Init(0, idx =>
        {
            currentTabIdx = idx;
            Refresh();
        });

        shopList.SetEvent(data =>
        {
            switch(data.costType)
            {
                case GameData.ShopCostType.Free:
                    DataManager.Instance.userData.AddPurchase(data);
                    break;
                case GameData.ShopCostType.Ads:
                    ADManager.Instance.ShowReward(delegate
                    {
                        DataManager.Instance.userData.AddPurchase(data);
                    });
                    break;
                case GameData.ShopCostType.Cash:
                    IAPManager.Instance.BuyProduct(data.id, successId =>
                    {
                        DataManager.Instance.userData.AddPurchase(data);
                    });
                    break;
                case GameData.ShopCostType.Goods:
                    if(DataManager.Instance.userData.Use(data.goodsType, data.costAmount))
                    {
                        DataManager.Instance.userData.AddPurchase(data);
                    }
                    break;
            }
        });
    }

    public override void Open()
    {
        base.Open();

        Init();
        Refresh();
    }

    public override void Refresh()
    {
        base.Refresh();

        GameData.ShopCategory category = (GameData.ShopCategory)currentTabIdx;
        shopList.UpdateList(DataManager.Instance.Get<GameData.Shop>().Where(x => x.category == category).ToArray());
    }

}
