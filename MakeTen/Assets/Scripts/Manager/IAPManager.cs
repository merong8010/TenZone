using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Core;
using Unity.Services.Core.Environments;

public class IAPManager : Singleton<IAPManager>, IDetailedStoreListener
{
    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;
    // === 이벤트 콜백 ===
    //public static event Action<string> OnPurchaseSuccess;
    //public static event Action<string, PurchaseFailureReason> OnPurchaseFailed;
    //public static event Action<string> OnRestoreSuccess;
    //public static event Action<string> OnRestoreFailed;

    // === 상품 ID ===
    //public const string PRODUCT_COINS = "coins_100";
    //public const string PRODUCT_REMOVE_ADS = "remove_ads";

    protected override void Awake()
    {
        base.Awake();
    }

    async void Start()
    {
        try
        {
            var options = new InitializationOptions()
                .SetEnvironmentName("development");

            await UnityServices.InitializeAsync(options);
        }
        catch (Exception exception)
        {
            Debug.LogError("UnityService.InitializactionAsync : " + exception);
            // An error occurred during initialization.
        }
    }

    public void InitializePurchasing()
    {
        if (IsInitialized()) return;
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        string[] shopIds = DataManager.Instance.Get<GameData.Shop>().Where(x => x.costType == GameData.ShopCostType.Cash).Select(x => x.id).ToArray();
        for (int i = 0; i < shopIds.Length; i++)
        {
            builder.AddProduct(shopIds[i], ProductType.Consumable);
        }
        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    public string GetPrice(string shopId)
    {
#if UNITY_EDITOR
        return $"$ {DataManager.Instance.Get<GameData.Shop>().SingleOrDefault(x => x.id == shopId).costAmount.ToString("n0")}";
#else
        return storeController.products.WithID(shopId)?.metadata.localizedPriceString;
#endif
    }

    private Action<string> successCallback = null;
    public void BuyProduct(string productId, Action<string> successCallback)
    {
        //UIManager.Instance.Loading("Purchasing", 0.5f, 0f, 0f);
#if UNITY_EDITOR
        UIManager.Instance.Message.Show(Message.Type.Ask, $"Buy productId : {productId}", callback: confirm =>
        {
            if(confirm) PurchaseSuccess(productId);
        });
        //PurchaseSuccess(productId);
        return;
#endif
        if (IsInitialized())
        {
            Product product = storeController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                this.successCallback = successCallback;
                storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.LogWarning($"[IAP] 구매 불가: {productId}");
                PurchaseFail(productId, PurchaseFailureReason.ProductUnavailable);
                //OnPurchaseFailed?.Invoke(productId, PurchaseFailureReason.ProductUnavailable);
            }
        }
        else
        {
            Debug.LogWarning("[IAP] 초기화되지 않음");
            PurchaseFail(productId, PurchaseFailureReason.PurchasingUnavailable);
        }
    }

    public void RestorePurchases()
    {
#if UNITY_IOS
        if (!IsInitialized())
        {
            Debug.LogWarning("복원 실패: 초기화되지 않음");
            OnRestoreFailed?.Invoke("not_initialized");
            return;
        }

        var apple = storeExtensionProvider.GetExtension<IAppleExtensions>();
        apple.RestoreTransactions((result) =>
        {
            if (result)
            {
                Debug.Log("복원 성공");
                OnRestoreSuccess?.Invoke("restore_complete");
            }
            else
            {
                Debug.LogWarning("복원 실패");
                OnRestoreFailed?.Invoke("restore_failed");
            }
        });
#else
        Debug.Log("복원은 iOS에서만 지원됩니다.");
#endif
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string productId = args.purchasedProduct.definition.id;

        // 서버 검증 비동기 시작
        FirebaseManager.Instance.ValidatePurchase(args, (isValid) =>
        {
            if (isValid)
            {
                Debug.Log($"서버 검증 성공: {productId}");
                PurchaseSuccess(productId);
            }
            else
            {
                Debug.LogWarning($"서버 검증 실패: {productId}");
                PurchaseFail(productId, PurchaseFailureReason.Unknown);
            }
        });

        return PurchaseProcessingResult.Pending; // 서버 응답 대기
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
        Debug.Log("IAP 초기화 성공");
    }

    public void PurchaseSuccess(string productId)
    {
        if(successCallback != null)
        {
            successCallback.Invoke(productId);
            successCallback = null;
        }
        
        UIManager.Instance.CloseLoading();
        UIManager.Instance.Message.Show(Message.Type.Confirm, string.Format(TextManager.Get("PurchaseSuccess"), TextManager.Get(DataManager.Instance.Get<GameData.Shop>().SingleOrDefault(x => x.id == productId).name)));
    }

    public void PurchaseFail(string productId, PurchaseFailureReason reason)
    {
        successCallback = null;
        UIManager.Instance.CloseLoading();
        UIManager.Instance.Message.Show(Message.Type.Confirm, string.Format(TextManager.Get("PurchaseFail"), reason.ToString()));
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        ((IStoreListener)Instance).OnInitializeFailed(error, message);
    }

    void IDetailedStoreListener.OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        ((IDetailedStoreListener)Instance).OnPurchaseFailed(product, failureDescription);
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        //((IStoreListener)Instance).OnInitializeFailed(error);
    }

    void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        //((IStoreListener)Instance).OnPurchaseFailed(product, failureReason);
    }
}
