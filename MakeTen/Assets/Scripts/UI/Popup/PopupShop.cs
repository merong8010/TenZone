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
                    Refresh();
                    break;
                case GameData.ShopCostType.Ads:
                    ADManager.Instance.ShowReward(delegate(bool success)
                    {
                        if(success)
                        {
                            DataManager.Instance.userData.AddPurchase(data);
                            Refresh();
                        }
                        
                    });
                    break;
                case GameData.ShopCostType.Cash:
                    IAPManager.Instance.BuyProduct(data.id, successId =>
                    {
                        DataManager.Instance.userData.AddPurchase(data);
                        Refresh();
                    });
                    break;
                case GameData.ShopCostType.Goods:
                    UIManager.Instance.Message.Show(Message.Type.Ask, string.Format(TextManager.Get("Shop_Ask_Buy"), TextManager.Get(data.name)), callback: confirm =>
                    {
                        if(confirm)
                        {
                            if (DataManager.Instance.userData.Use(data.goodsType, data.costAmount))
                            {
                                DataManager.Instance.userData.AddPurchase(data);
                                Refresh();
                            }
                        }
                    });
                    
                    break;
            }
        });
    }

    public override void Open()
    {
        Init();
        base.Open();
    }

    public override void Refresh()
    {
        base.Refresh();

        GameData.ShopCategory category = (GameData.ShopCategory)currentTabIdx;
        shopList.UpdateList(DataManager.Instance.Get<GameData.Shop>().Where(x => x.category == category).ToArray());
    }

}
