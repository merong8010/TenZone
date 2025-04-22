using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using TMPro;
using UnityEngine.U2D;
public class GoodsDisplay : MonoBehaviour
{
    [SerializeField]
    private GameData.GoodsType type;
    [SerializeField]
    private bool isSubscribe;

    private IDisposable disposable;
    [SerializeField]
    private Image icon;
    [SerializeField]
    private TextMeshProUGUI text;
    [SerializeField]
    private GameObject adsObj;
    [SerializeField]
    private Text priceText;

    private void Start()
    {
        
    }

    public void Set(GameData.ShopCostType costType, GameData.GoodsType goodsType, int amount, string shopId = "")
    {
        switch(costType)
        {
            case GameData.ShopCostType.Free:
                icon.gameObject.SetActive(false);
                text.gameObject.SetActive(false);
                if(adsObj != null) adsObj.SetActive(false);
                if (priceText != null)
                {
                    priceText.gameObject.SetActive(true);
                    priceText.text = TextManager.Get("Free");
                }
                break;
            case GameData.ShopCostType.Cash:
                icon.gameObject.SetActive(false);
                text.gameObject.SetActive(false);
                if (adsObj != null) adsObj.SetActive(false);
                if (priceText != null)
                {
                    priceText.gameObject.SetActive(true);
                    priceText.text = IAPManager.Instance.GetPrice(shopId);
                }
                break;
            case GameData.ShopCostType.Ads:
                icon.gameObject.SetActive(false);
                text.gameObject.SetActive(false);
                if (adsObj != null) adsObj.SetActive(true);
                if (priceText != null)
                {
                    priceText.gameObject.SetActive(false);
                }
                break;
            case GameData.ShopCostType.Goods:
                Set(goodsType, amount);
                break;
        }
    }

    public void Set(GameData.GoodsType type)
    {
        if (adsObj != null) adsObj.SetActive(false);
        if (priceText != null) priceText.gameObject.SetActive(false);
        this.type = type;
        isSubscribe = true;
        icon.sprite = Resources.Load<SpriteAtlas>("Graphics/Goods").GetSprite(type.ToString());
        Subscribe();
    }

    public void Set(GameData.GoodsType type, int amount)
    {
        if (adsObj != null) adsObj.SetActive(false);
        if (priceText != null) priceText.gameObject.SetActive(false);
        isSubscribe = false;
        icon.sprite = Resources.Load<SpriteAtlas>("Graphics/Goods").GetSprite(type.ToString());
        text.text = amount.ToString("n0");
    }

    private void OnEnable()
    {
        if(isSubscribe)
        {
            icon.sprite = Resources.Load<SpriteAtlas>("Graphics/Goods").GetSprite(type.ToString());
            Subscribe();
        }
    }

    private void OnDisable()
    {
        disposable?.Dispose();
    }

    private void Subscribe()
    {
        disposable?.Dispose();
        if (!isSubscribe || type == GameData.GoodsType.None || DataManager.Instance.userData == null) return;
        text.text = DataManager.Instance.userData.goods[type].ToString("n0");
        disposable = DataManager.Instance.userData.goods.ObserveReplace().Where(x => x.Key == type).Subscribe(x => text.text = x.NewValue.ToString("n0"));
    }
}
