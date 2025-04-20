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

    private void Start()
    {
        
    }

    public void Set(GameData.GoodsType type)
    {
        this.type = type;
        isSubscribe = true;
        icon.sprite = Resources.Load<SpriteAtlas>("Graphics/Goods").GetSprite(type.ToString());
        Subscribe();
    }

    public void Set(GameData.GoodsType type, int amount)
    {
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
