using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using GameData;
using System.Linq;
public class ShopCategoryItem : ListItem<List<GameData.Shop>>
{
    [SerializeField]
    private Text categoryText;
    [SerializeField]
    private ShopList list;

    private bool isInit = false;
    private void Init()
    {
        if (isInit) return;
        isInit = true;
        list.SetEvent(data =>
        {
            if (!DataManager.Instance.userData.CanPurchase(data))
            {
                UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("purchaseComplete"));
                return;
            }
            switch (data.costType)
            {
                case GameData.ShopCostType.Free:
                    DataManager.Instance.userData.AddPurchase(data);
                    UIManager.Instance.Get<PopupShop>().Refresh();
                    break;
                case GameData.ShopCostType.Ads:
                    ADManager.Instance.ShowReward(delegate (bool success)
                    {
                        if (success)
                        {
                            DataManager.Instance.userData.AddPurchase(data);
                            UIManager.Instance.Get<PopupShop>().Refresh();
                        }

                    });
                    break;
                case GameData.ShopCostType.Cash:
                    IAPManager.Instance.BuyProduct(data.id);
                    break;
                case GameData.ShopCostType.Goods:
                    UIManager.Instance.Message.Show(Message.Type.Ask, string.Format(TextManager.Get("Shop_Ask_Buy"), TextManager.Get(data.name)), callback: confirm =>
                    {
                        if (confirm)
                        {
                            if (DataManager.Instance.userData.Use(data.goodsType, data.costAmount))
                            {
                                DataManager.Instance.userData.AddPurchase(data);
                                UIManager.Instance.Get<PopupShop>().Refresh();
                            }
                        }
                    });

                    break;
            }
        });
    }
    public override void SetData(List<Shop> data)
    {
        Init();
        base.SetData(data);
        categoryText.text = TextManager.Get(data.First().category.ToString());
        list.UpdateList(data);
    }
}
