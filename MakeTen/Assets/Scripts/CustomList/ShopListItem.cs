using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameData;
using UnityEngine.U2D;

public class ShopListItem : ListItem<GameData.Shop>
{
    [SerializeField]
    private Color[] categoryColors;
    [SerializeField]
    private Image frameImage;

    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text descText;
    [SerializeField]
    private TextMeshProUGUI purchasedCount;

    [SerializeField]
    private Image iconImage;
    [SerializeField]
    private GoodsList rewardList;

    [SerializeField]
    private GoodsDisplay cost;

    [SerializeField]
    private GameObject lockObj;
    [SerializeField]
    private Text unlockConditionText;

    public override void SetData(Shop data)
    {
        base.SetData(data);
        frameImage.color = categoryColors[(int)data.category];
        if(nameText != null)
        {
            nameText.text = TextManager.Get(data.name);
        }

        if(descText != null)
        {
            descText.text = TextManager.Get(data.desc);
        }

        if(purchasedCount != null)
        {
            purchasedCount.text = data.buyMaxCount > 0 ? DataManager.Instance.userData.GetPurchaseCount(data).ToProgressText(data.buyMaxCount) : "";
        }
        if(iconImage != null && !string.IsNullOrEmpty(data.resource))
        {
            iconImage.gameObject.SetActive(true);
            iconImage.sprite = Resources.Load<SpriteAtlas>("Graphics/Shop").GetSprite(data.resource);
        }
        else
        {
            iconImage.gameObject.SetActive(false);
        }
        if(rewardList != null)
        {
            rewardList.UpdateList(data.rewards);
        }
        cost.SetCost(data.costType, data.goodsType, data.costAmount, data.id);

        if(data.unlockLevel > 0 && DataManager.Instance.userData.Info.level < data.unlockLevel)
        {
            lockObj.SetActive(true);
            unlockConditionText.text = string.Format(TextManager.Get("UnlockConditionLevel"), data.unlockLevel);
        }
        else if(!string.IsNullOrEmpty(data.unlockShopId) && DataManager.Instance.userData.GetPurchaseCount(data.unlockShopId) == 0)
        {
            lockObj.SetActive(true);
            unlockConditionText.text = string.Format(TextManager.Get("UnlockConditionShop"), data.unlockShopId);
        }
        else if (!DataManager.Instance.userData.CanPurchase(data))
        {
            lockObj.SetActive(true);
            unlockConditionText.text = string.Format(TextManager.Get("purchaseComplete"), data.unlockShopId);
        }
        else
        {
            lockObj.SetActive(false);
        }
    }
}
