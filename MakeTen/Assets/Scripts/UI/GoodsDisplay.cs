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

    [Header("데이터 소스")]
    [SerializeField]
    private GoodsDataAtlas goodsAtlas; // 리팩토링: 스크립터블 오브젝트 참조

    [Header("설정")]
    [SerializeField]
    private GameData.GoodsType type;
    [SerializeField]
    private bool isSubscribe;

    [Header("UI 참조")]
    [SerializeField]
    private Image icon;
    [SerializeField]
    private TextMeshProUGUI text;
    [SerializeField]
    private GameObject adsObj;
    [SerializeField]
    private TextMeshProUGUI priceText; // 리팩토링: 일관성을 위해 TextMeshProUGUI로 변경

    private IDisposable disposable;

    // 리팩토링: 상점 아이템을 위한 Set 메서드가 더 깔끔해졌습니다.
    public void SetCost(GameData.ShopCostType costType, GameData.GoodsType type, int price, string shopId = "")
    {
        // 먼저 모든 선택적 UI 요소를 비활성화합니다.
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
                    priceText.text = TextManager.Get("Free"); // TextManager가 있다고 가정
                }
                break;

            case GameData.ShopCostType.Cash:
                if (priceText != null)
                {
                    priceText.gameObject.SetActive(true);
                    priceText.text = IAPManager.Instance.GetPrice(shopId); // IAPManager가 있다고 가정
                }
                break;

            case GameData.ShopCostType.Ads:
                if (adsObj != null) adsObj.SetActive(true);
                break;

            case GameData.ShopCostType.Goods:
                // 비용이 재화인 경우, 정적인 재화 아이템처럼 표시합니다.
                SetStaticValue(type, price);
                break;
        }
    }

    // 리팩토링: 정적 값을 설정하는 더 명확한 이름의 메서드
    public void SetStaticValue(GameData.GoodsType goodsType, int amount)
    {
        //Debug.Log($"SetStaticValue {goodsType} | {amount}");
        disposable?.Dispose();
        isSubscribe = false;

        this.type = goodsType;
        this.amount = amount;

        UpdateVisuals(amount.ToString("n0"));
    }

    // 리팩토링: 구독형 디스플레이를 초기화하는 더 명확한 이름의 메서드
    public void InitializeSubscribed(GameData.GoodsType goodsType)
    {
        isSubscribe = true;
        this.type = goodsType;

        // 실제 구독은 OnEnable에서 처리하여 재활성화 시에도 대응합니다.
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

        // 변경(Replace) 이벤트를 { Key, Value = NewValue } 형태로 변환
        var replaceStream = DataManager.Instance.userData.Goods.datas.ObserveReplace().Select(e => new { e.Key, Value = e.NewValue });

        // 두 스트림을 Merge로 합쳐서 구독
        disposable = addStream.Merge(replaceStream)
            .Where(x => x.Key == type) // 스트림 자체를 필터링
            .Subscribe(x => // 이제 'type'과 관련된 변경일 때만 실행됨
            {
                // if 조건문이 더 이상 필요 없음
                UpdateVisuals(x.Value.ToString("n0"));
            });
    }

    // 리팩토링: UI 시각 요소를 업데이트하는 중앙화된 헬퍼 메서드
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