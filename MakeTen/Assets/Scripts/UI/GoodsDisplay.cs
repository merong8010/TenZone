// GoodsDisplay.cs
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using TMPro;
using UnityEditor;

public class GoodsDisplay : MonoBehaviour
{
    public GameData.GoodsType goodsType => type;
    public int amount { get; private set; }

    [Header("������ �ҽ�")]
    [SerializeField]
    private GoodsDataAtlas goodsAtlas; // �����丵: ��ũ���ͺ� ������Ʈ ����

    [Header("����")]
    [SerializeField]
    private GameData.GoodsType type;
    [SerializeField]
    private bool isSubscribe;

    [Header("UI ����")]
    [SerializeField]
    private Image icon;
    [SerializeField]
    private TextMeshProUGUI text;
    [SerializeField]
    private GameObject adsObj;
    [SerializeField]
    private TextMeshProUGUI priceText; // �����丵: �ϰ����� ���� TextMeshProUGUI�� ����

    private IDisposable disposable;

    // �����丵: ���� �������� ���� Set �޼��尡 �� ����������ϴ�.
    public void SetCost(GameData.ShopCostType costType, GameData.GoodsType type, int price, string shopId = "")
    {
        // ���� ��� ������ UI ��Ҹ� ��Ȱ��ȭ�մϴ�.
        if (adsObj != null) adsObj.SetActive(false);
        if (priceText != null) priceText.gameObject.SetActive(false);
        icon.gameObject.SetActive(false);
        text.gameObject.SetActive(false);

        switch (costType)
        {
            case GameData.ShopCostType.Free:
                if (priceText != null)
                {
                    priceText.gameObject.SetActive(true);
                    priceText.text = TextManager.Get("Free"); // TextManager�� �ִٰ� ����
                }
                break;

            case GameData.ShopCostType.Cash:
                if (priceText != null)
                {
                    priceText.gameObject.SetActive(true);
                    priceText.text = IAPManager.Instance.GetPrice(shopId); // IAPManager�� �ִٰ� ����
                }
                break;

            case GameData.ShopCostType.Ads:
                if (adsObj != null) adsObj.SetActive(true);
                break;

            case GameData.ShopCostType.Goods:
                // ����� ��ȭ�� ���, ������ ��ȭ ������ó�� ǥ���մϴ�.
                SetStaticValue(type, price);
                break;
        }
    }

    // �����丵: ���� ���� �����ϴ� �� ��Ȯ�� �̸��� �޼���
    public void SetStaticValue(GameData.GoodsType goodsType, int amount)
    {
        //Debug.Log($"SetStaticValue {goodsType} | {amount}");
        disposable?.Dispose();
        isSubscribe = false;

        this.type = goodsType;
        this.amount = amount;

        UpdateVisuals(amount.ToString("n0"));
    }

    // �����丵: ������ ���÷��̸� �ʱ�ȭ�ϴ� �� ��Ȯ�� �̸��� �޼���
    public void InitializeSubscribed(GameData.GoodsType goodsType)
    {
        isSubscribe = true;
        this.type = goodsType;

        // ���� ������ OnEnable���� ó���Ͽ� ��Ȱ��ȭ �ÿ��� �����մϴ�.
        if (gameObject.activeInHierarchy)
        {
            Subscribe();
        }
    }

    private void OnEnable()
    {
        if (isSubscribe)
        {
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
        if (!isSubscribe || type == GameData.GoodsType.None || DataManager.Instance?.userData == null)
        {
            UpdateVisuals("0");
            return;
        }

        long initialValue = 0;
        if (DataManager.Instance.userData.Goods.datas.ContainsKey(type))
        {
            initialValue = DataManager.Instance.userData.Goods.datas[type];
        }

        UpdateVisuals(initialValue.ToString("n0"));

        var addStream = DataManager.Instance.userData.Goods.datas.ObserveAdd().Select(e => new { e.Key, Value = e.Value });

        // ����(Replace) �̺�Ʈ�� { Key, Value = NewValue } ���·� ��ȯ
        var replaceStream = DataManager.Instance.userData.Goods.datas.ObserveReplace().Select(e => new { e.Key, Value = e.NewValue });

        // �� ��Ʈ���� Merge�� ���ļ� ����
        disposable = addStream.Merge(replaceStream)
            .Where(x => x.Key == type) // ��Ʈ�� ��ü�� ���͸�
            .Subscribe(x => // ���� 'type'�� ���õ� ������ ���� �����
            {
                // if ���ǹ��� �� �̻� �ʿ� ����
                UpdateVisuals(x.Value.ToString("n0"));
            });
    }

    // �����丵: UI �ð� ��Ҹ� ������Ʈ�ϴ� �߾�ȭ�� ���� �޼���
    private void UpdateVisuals(string valueText)
    {
        if (adsObj != null) adsObj.SetActive(false);
        if (priceText != null) priceText.gameObject.SetActive(false);

        icon.gameObject.SetActive(true);
        text.gameObject.SetActive(true);

        if (goodsAtlas != null)
        {
            icon.sprite = goodsAtlas.GetSprite(type);
            icon.SetNativeSize();
        }

        text.text = valueText;
    }
}