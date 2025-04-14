//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Purchasing;
//using AppsFlyerSDK;
//using UnityEngine.Purchasing.Security;

//using Unity.Services.Core;
//using Unity.Services.Core.Environments;
//using System.Linq;
//using Chart;
//using UnityEngine.Events;
//using UnityEngine.Purchasing.Extension;
//using Product = UnityEngine.Purchasing.Product;

//public class IAPManager : Singleton<IAPManager>, IDetailedStoreListener, IAppsFlyerValidateReceipt
//{
//    IStoreController m_StoreController;
//#if UNITY_ANDROID
//    IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;
//#elif UNITY_IOS
//    IAppleExtensions m_AppleExtensions;
//#endif
//    public string environment = "production";
//    private bool isInitUGS = false;
//    private bool isWaitInitPurchase = false;
//    public static bool IsInitialized;
    
//    async void Start()
//    {
//        try
//        {
//            var options = new InitializationOptions()
//                .SetEnvironmentName(environment);

//            Debug.Log($"UGS Init!!");
//            await UnityServices.InitializeAsync(options);

//            isInitUGS = true;
//            if (isWaitInitPurchase)
//            {
//                InitializePurchasing();
//            }
//        }
//        catch (System.Exception exception)
//        {
//            Debug.LogError("UGS Fail : " + exception.Message);
//        }
//    }

    

//    public void InitializePurchasing()
//    {
//        if (!isInitUGS)
//        {
//            isWaitInitPurchase = true;
//            return;
//        }

//        if (GameManager.Instance.CurrentServer.testPurchase) return;
        
//        var shopChart = DataManager.Instance.GetChart<Chart.BM_Shop>();
//        if (shopChart != null)
//        {
//            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
//            for (int i = 0; i < shopChart.rows.Length; i++)
//            {
//                if (shopChart.rows[i].cost_type == ShopCostType.cash)
//                {
//                    builder.AddProduct(shopChart.rows[i].product_id, ProductType.Consumable);
//                }
//            }
//            UnityPurchasing.Initialize(this, builder);
//        }
        
//        Debug.Log($"IAPManager.Init!!");
//    }

//    public string GetPrice(string storeCode)
//    {
//#if UNITY_EDITOR
//        return
//            $"E{DataManager.Instance.GetChart<Chart.BM_Shop>().rows.FirstOrDefault(x => x.product_id == storeCode).cost_count}";
//        // Chart.Shop shopChart = DataManager.Instance.GetChart<Chart.Shop>();
//        // return "$" + shopChart.rows.Where(x => x.store_purchase_code == storeCode).First().cost_count;
//#else
//        if (GameManager.Instance.CurrentServer.testPurchase)
//        {
//            return $"T{DataManager.Instance.GetChart<Chart.BM_Shop>().rows.FirstOrDefault(x => x.product_id == storeCode).cost_count}";
//            // var shopChart = DataManager.Instance.GetChart<Chart.BM_Shop>().rows
//            //     .FirstOrDefault(x => x.product_id == storeCode);
//            // return $"T{shopChart.cost_count}";
//        }
        
//        for (int i = 0; i < m_StoreController.products.all.Length; i++)
//        {
//            if (m_StoreController.products.all[i].definition.id == storeCode)
//                return m_StoreController.products.all[i].metadata.localizedPriceString;
//        }
//        return "-";
//#endif

//    }

//    public void Buy(Chart.BM_ShopInfo chart, Action completed = null)
//    {
//        //UISystem.Instance.Open<Toast>().Init(TextManager.Get("toast_not_available"));
//        //return;

//        var blocker = UISystem.Instance.Open<BlockerPage>(closed: ()=>
//        {
//            completed?.Invoke();
//        });
        
//#if UNITY_EDITOR
//        UISystem.Instance.Open<PopupDialogBox>().Init("Test", "Purchase Item? id : " + chart.product_id, "yes", "no",
//            result =>
//            {
//                if (result)
//                {
//                    AddPurchaseItem(chart);
//                    return;
//                }

//                blocker.ForceClose();
//            });
//#else
//        if (GameManager.Instance.CurrentServer.testPurchase)
//        {
//            UISystem.Instance.Open<PopupDialogBox>().Init("Test", "Purchase Item? id : " + chart.product_id, "yes", "no",
//                result =>
//                {
//                    if (result)
//                    {
//                        AddPurchaseItem(chart);
//                        return;
//                    }

//                    blocker.ForceClose();
//                });
//        }
//        else
//        {
//            Debug.Log($"Try purchase {chart.product_id}");
//            m_StoreController?.InitiatePurchase(chart.product_id);
//        }
//#endif
//    }

//    public void AddPurchaseItem(Chart.BM_ShopInfo chart)
//    {
//        Debug.Log($"Purchase Complete {chart.product_id}");

//        var productChart = DataManager.Instance.GetChart<Chart.Product>().Get(chart.product_key);
//        if (productChart.Equals(default(Chart.ProductInfo)))
//        {
//            if (DataManager.Instance.GetChart<Chart.PassSalesSchedule>().rows.ToList().Exists(x => x.product_key == chart.product_key))
//            {
//                //DataManager.Instance.GetTable<User.PlayerData>().AddPurchaseCount(chart.cost_count);
//                DataManager.Instance.GetTable<User.GameShopData>().OnPurchase(chart, 1, true);
//                BackendManager.Instance.TransactionTable(false);
//                UISystem.Instance.Find<BlockerPage>()?.ForceClose();

//                if(chart.cost_type != ShopCostType.cash)
//                {
//                    FirebaseManager.Instance.Log("log_shop_noniap", new FirebaseManager.LogParam("shop_id", chart.product_key.ToString()),
//                        new FirebaseManager.LogParam("shop_price", chart.cost_count.ToString()));
//                }             
//            }
            
//            return;
//        }

//        DataManager.Instance.GetTable<User.GameShopData>().OnPurchase(chart, 1, true); 
        
//        if (productChart.item_list.Length > 0 && productChart.item_list[0] > 0)
//        {
//            if (chart.category == IAPShopCategory.subscription)
//            {
//                // 즉시 획득
//                for (var i = 0; i < productChart.item_list.Length; i++)
//                {
//                    DataManager.AddItem(productChart.item_list[i], productChart.item_count[i],
//                        productChart.item_enhance[i]);
//                }

//                UISystem.Instance.Open<PopupReward>(closed: () =>
//                {
//                    // 월정액 보상 팝업 오픈
//                    UISystem.Instance.Open<PopupSubscriptionReward>();
//                }).Init(productChart.item_list, productChart.item_count, productChart.item_enhance);
//                DataManager.Instance.GetTable<User.GameShopData>().UpdateSubscriptionAlert();
//                if (chart.product_key == PopupSubscription.PremiumSubscriptionKey)
//                {
//                    BattleManager.Instance.ActivateBuffSubscription(DataManager.Instance.GetTable<User.GameShopData>().subscriptionDict[PopupSubscription.PremiumSubscriptionKey].GetRemainSeconds());
//                }
//            }
//            else
//            {
//                // 메일 전송
//                var filtered = productChart.item_list
//                    .Select((key, index) => new
//                    {
//                        Key = key, Count = productChart.item_count[index], enhance = productChart.item_enhance[index]
//                    }).Where(x => x.Key is not (>= 3900 and < 4000));
                
//                DataManager.Instance.GetTable<User.MailData>().AddData($"product_{chart.product_key}",
//                    chart.category == IAPShopCategory.pass ? $"product_{chart.product_key}" : "purchase_mail_desc",
//                    filtered.Select(x=>x.Key).ToArray(),
//                    filtered.Select(x=>x.Count).ToArray(),
//                    filtered.Select(x=>x.enhance).ToArray(),
//                    0);
//            }
//        }

//        if (chart.cost_type != ShopCostType.cash)
//        {
//            FirebaseManager.Instance.Log("log_shop_noniap", new FirebaseManager.LogParam("shop_id", chart.product_key.ToString()),
//                new FirebaseManager.LogParam("shop_price", chart.cost_count.ToString()));
//        }

//        BackendManager.Instance.TransactionTable(false);
//        UISystem.Instance.Find<BlockerPage>()?.ForceClose();
//    }
    
//    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
//    {
//        Debug.Log("OnInitialized IAP");
//        IsInitialized = true;
//        m_StoreController = controller;
//#if UNITY_ANDROID
//        m_GooglePlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
//#elif UNITY_IOS
//        m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();
//#endif
//        Restore();
//    }

//    public Product GetProductInfo(string storeCode)
//    {
//        return m_StoreController?.products?.WithStoreSpecificID(storeCode);
//    }
//    private const string PlayStorePublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAl1UhHt0UJB6SGyZepHhXCZAPk3CE7W/B8wE+1pn/Fs4NiO/V8tET7ldI0eZ3SqnROaeupuqiqK3o/yUuw1uGDzFlKxTJGNlFfbUusnZn1UfV27aPgQj51G9AL0EAfz1x/6dApkUXPt6YAbSCYxdwuyKROtmkRlLPUHZAbuMe4e6bd1bzn1fBGESSGrVkznzpWLeEHoe9qFMBMxqCOMqg8MNloHEDdipak6Z+AawUgHsOh/ZIrO/1V+HKZmApa4YFa65GN0I55mMXYh76JUb3YhPDy/zajmL4rkATrEHudZAa8BUK6vasRMCBASJxEgvNNwcOcVc0Atps5i1O05QopQIDAQAB";
//    //private const string PlayStorePublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAzSOVRFB9fOwmLQx/V8JJAjL+ZT/4/u0dKh3nCBqYR9ZhYfqFuQFHxwUE+kJ8oiTDalnnj/TdXxvXDZFtz4ZebJqq4CQzip7u1vqAsH77TktoMTHlikjqHwhZhBBTyP7B3Kp+X45eMEKQw1FB4SR1sCfXyQfoRtBw41NSeQMVUUc60mcYQaGW9xM+6Sa3Yg2uZwDAziF24oiI8Ks/c7NLAdJYfA0QBxdO/3lbv6OaeqNp/we+irV8og3YfPGJyHh0qUSwhGJJ8EETTsEbLCRQm8jGr5BES/hrl+FphrW08+BzVLPIu1vLdhvnXyDrESamSCijTlNGLMGSvmuO5f89WQIDAQAB";

//    public ReceiptData GetReceiptData(PurchaseEventArgs e)
//    {
//        ReceiptData data = new ReceiptData();

//        if (e != null)
//        {
//            //Main receipt root
//            string receiptString = e.purchasedProduct.receipt;
//            Debug.Log("receiptString " + receiptString);
//            var receiptDict = (Dictionary<string, object>)MiniJson.JsonDecode(receiptString);
//            Debug.Log("receiptDict COUNT" + receiptDict.Count);

//#if UNITY_ANDROID
//            //Next level Paylod dict
//            string payloadString = (string)receiptDict["Payload"];
//            Debug.Log("payloadString " + payloadString);
//            var payloadDict = (Dictionary<string, object>)MiniJson.JsonDecode(payloadString);

//            //Stuff from json object
//            string jsonString = (string)payloadDict["json"];
//            Debug.Log("jsonString " + jsonString);
//            var jsonDict = (Dictionary<string, object>)MiniJson.JsonDecode(jsonString);
//            string orderIdString = (string)jsonDict["orderId"];
//            Debug.Log("orderIdString " + orderIdString);
//            string packageNameString = (string)jsonDict["packageName"];
//            Debug.Log("packageNameString " + packageNameString);
//            string productIdString = (string)jsonDict["productId"];
//            Debug.Log("productIdString " + productIdString);

//            double orderDateDouble = System.Convert.ToDouble(jsonDict["purchaseTime"]);
//            Debug.Log("orderDateDouble " + orderDateDouble);

//            string purchaseTokenString = (string)jsonDict["purchaseToken"];
//            Debug.Log("purchaseTokenString " + purchaseTokenString);

//            string signatureString = (string)payloadDict["signature"];
//            Debug.Log("signatureString " + signatureString);


//            //Creating UTC from Epox
//            System.DateTime orderDateTemp = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
//            orderDateTemp = orderDateTemp.AddMilliseconds(orderDateDouble);

//            data.orderId = orderIdString;
//            data.packageName = packageNameString;
//            data.productId = productIdString;
//            data.purchaseToken = purchaseTokenString;
//            //data.priceAmountMicros = priceAmountMicrosLong;
//            //data.priceCurrencyCode = priceCurrencyCodeString;
//            data.orderDate = orderDateTemp;
//            data.receipt = receiptString;
//            data.signature = signatureString;
//            data.json = jsonString;
//#endif
//            Debug.Log("GetReceiptData succesfull");
//        }
//        else
//        {
//            Debug.Log("PurchaseEventArgs is NULL");
//        }

//        return data;
//    }

//    public class ReceiptData
//    {
//        public string orderId;
//        public string packageName;
//        public string productId;
//        public string purchaseToken;
//        //public long priceAmountMicros;
//        //public string priceCurrencyCode;
//        public System.DateTime orderDate;
//        public string receipt;
//        public string signature;
//        public string json;
//        public override string ToString()
//        {
//            //return base.ToString();
//            return "orderId : " + orderId + "\n"
//                + "packageName : " + packageName + "\n"
//                + "productId : " + productId + "\n"
//                + "purchaseToken : " + purchaseToken;


//        }
//    }

//    public void didFinishValidateReceipt(string result)
//    {
//        AppsFlyer.AFLog("didFinishValidateReceipt", result);
//    }

//    public void didFinishValidateReceiptWithError(string error)
//    {
//        AppsFlyer.AFLog("didFinishValidateReceiptWithError", error);
//    }
    
//    private void AppsFlyerEvent(PurchaseEventArgs args, ReceiptData receiptData)
//    {
//        //Debug.Log("AppsFlyerEvent  receipt : " + args.purchasedProduct.receipt);
//        //AppsFlyerAndroid appsFlyer = new AppsFlyerAndroid(); 
//#if UNITY_ANDROID
//        //ReceiptData receiptData = GetReceiptData(args);
//        AppsFlyer.validateAndSendInAppPurchase(PlayStorePublicKey, receiptData.signature, receiptData.json,
//            args.purchasedProduct.metadata.localizedPrice.ToString(), args.purchasedProduct.metadata.isoCurrencyCode,
//            new Dictionary<string, string>()
//                { { "af_projected_revenue", $"{(args.purchasedProduct.metadata.localizedPrice * 2).ToString()}" } },
//            this);
//#endif

//#if UNITY_IOS
//        if(GameManager.Instance.serverType != GameManager.ServerType.Live)
//            AppsFlyer.setUseReceiptValidationSandbox(true);

//        AppsFlyer.validateAndSendInAppPurchase(args.purchasedProduct.definition.storeSpecificId,
//            args.purchasedProduct.metadata.localizedPrice.ToString(), args.purchasedProduct.metadata.isoCurrencyCode,
//            args.purchasedProduct.transactionID, new Dictionary<string, string>()
//            {
//                { "af_projected_revenue", $"{(args.purchasedProduct.metadata.localizedPrice * 2).ToString()}" }
//            }, this);
//#endif
//    }

//    private void FirebaseLog(PurchaseEventArgs args, string orderId)
//    {
//        BM_ShopInfo chart = DataManager.Instance.GetChart<Chart.BM_Shop>().rows.SingleOrDefault(x => x.product_id == args.purchasedProduct.definition.id);

//        FirebaseManager.Instance.Log("log_shop_iap", new FirebaseManager.LogParam("shop_id", chart.product_key.ToString()),
//                new FirebaseManager.LogParam("shop_price", chart.cost_count.ToString()), new FirebaseManager.LogParam("idx", orderId));


//        //Firebase.Analytics.FirebaseAnalytics.LogEvent("test_purchase", Firebase.Analytics.FirebaseAnalytics.ParameterValue, (int)(args.purchasedProduct.metadata.localizedPrice * 2));

//        Firebase.Analytics.FirebaseAnalytics.LogEvent("in_app_purchase2",
//            new Firebase.Analytics.Parameter("shop_id", args.purchasedProduct.definition.storeSpecificId),
//            new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterCurrency, args.purchasedProduct.metadata.isoCurrencyCode),
//            new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, (double)(args.purchasedProduct.metadata.localizedPrice * 2)));
//    }

//    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
//    {
//        bool isSuccess = BackendManager.Instance.ValidatePurchase(args);
//        if (isSuccess)
//        {
//            ReceiptData receiptData = GetReceiptData(args);
//            AppsFlyerEvent(args, receiptData);
//            FirebaseLog(args, receiptData.orderId);
            
//            AddPurchaseItem(DataManager.Instance.GetChart<Chart.BM_Shop>().rows
//                .SingleOrDefault(x => x.product_id == args.purchasedProduct.definition.id));
//        }
//        else
//        {
//            UISystem.Instance.Find<BlockerPage>()?.ForceClose();
            
//            UISystem.Instance.Open<Toast>().Init(TextManager.Get("toast_purchase_fail"));
//        }

//        return PurchaseProcessingResult.Complete;
//    }

//    public void Restore()
//    {
//#if UNITY_ANDROID
//        m_GooglePlayStoreExtensions.RestoreTransactions(OnRestore);
//#elif UNITY_IOS
//        m_AppleExtensions.RestoreTransactions(OnRestore);
//#endif
//    }

//    void OnRestore(bool success, string message)
//    {
//        //var restoreMessage = "";
//        if (success)
//        {
//            // This does not mean anything was restored,
//            // merely that the restoration process succeeded.
//            //restoreMessage = "Restore Successful";
//            //Chart.ShopTable shopTable = DataManager.Instance.GetChart<Chart.ShopTable>(Chart.TableName.Shop);
//            //Chart.ShopInfo removeAdInfo = shopTable.rows.SingleOrDefault(x => x.ShopTable_ID == RemoveAdvertiseShopTableId);
//            //if (m_StoreController.products.WithID(removeAdInfo.StorePurchaseCode_iOS).hasReceipt)
//            //{
//            //    DataManager.Instance.PurchaseRemoveAdvertise();
//            //}

//            //Chart.ShopInfo autoSurpriseInfo = shopTable.rows.SingleOrDefault(x => x.ShopTable_ID == AutoSurpriseBox);
//            //if (m_StoreController.products.WithID(autoSurpriseInfo.StorePurchaseCode_iOS).hasReceipt)
//            //{
//            //    DataManager.Instance.PurchaseAutoSurpriseBox();
//            //}

//            //Chart.ShopInfo autoStageMoveInfo = shopTable.rows.SingleOrDefault(x => x.ShopTable_ID == AutoStageMove);
//            //if (m_StoreController.products.WithID(autoStageMoveInfo.StorePurchaseCode_iOS).hasReceipt)
//            //{
//            //    DataManager.Instance.PurchaseAutoStageMove();
//            //}

//            //Chart.ShopInfo autoStageMoveInfoSale = shopTable.rows.SingleOrDefault(x => x.ShopTable_ID == AutoStageMoveSale);
//            //if (m_StoreController.products.WithID(autoStageMoveInfoSale.StorePurchaseCode_iOS).hasReceipt)
//            //{
//            //    DataManager.Instance.PurchaseAutoStageMove();
//            //}
//        }
//        else
//        {
//            // Restoration failed.
//            //restoreMessage = "Restore Failed";
//        }

//        Debug.Log(message);
//    }

//    public bool HasPurchased(string id)
//    {
//        var purchasedItem = m_StoreController.products.WithID(id);
//        return purchasedItem != null && purchasedItem.hasReceipt;
//    }

//    //void OnDeferredPurchase(Product product)
//    //{
//    //    Debug.Log("OnDeferredPurchase : " + product.definition.id);

//    //    AddPurchaseItem(DataManager.Instance.GetChart<Chart.Shop>().rows.SingleOrDefault(x => x.store_purchase_code == product.definition.id));
//    //    PopupManager.Instance.SimpleMessage(TextManager.Get("Popup/Notice/RestorePurchase/Complete"));
//    //}

//    public void RestorePurchaseIOS()
//    {
//#if UNITY_IOS
//        //User.ShopData userShopData = DataManager.Instance.GetTable<User.ShopData>(User.TableName.Shop);
//        //Chart.ShopTable shopTable = DataManager.Instance.GetChart<Chart.ShopTable>(Chart.TableName.Shop);

//        //Chart.ShopInfo removeAdvertiseInfo = shopTable.rows.SingleOrDefault(x => x.ShopTable_ID == RemoveAdvertiseShopTableId);

//        //Product removeAdProduct = GetProductInfo(removeAdvertiseInfo.StorePurchaseCode_iOS);
//        //var removeAdReceipt = m_AppleExtensions.GetTransactionReceiptForProduct(removeAdProduct);

//        //if (!string.IsNullOrEmpty(removeAdReceipt))
//        //{
//        //    if (removeAdvertiseInfo.ShopTable_ID == RemoveAdvertiseShopTableId)
//        //    {
//        //        if (userShopData.GetPurchaseBuyCount(removeAdvertiseInfo) > 0)
//        //        {
//        //            DataManager.Instance.PurchaseRemoveAdvertise();
//        //        }
//        //        else
//        //        {
//        //            userShopData.AddPurchase(removeAdvertiseInfo);
//        //        }
//        //    }
//        //}

//        //Chart.ShopInfo autoSurpriseInfo = shopTable.rows.SingleOrDefault(x => x.ShopTable_ID == AutoSurpriseBox);
//        //Product autoSurpriseProduct = GetProductInfo(autoSurpriseInfo.StorePurchaseCode_iOS);
//        //var autoSurpriseReceipt = m_AppleExtensions.GetTransactionReceiptForProduct(autoSurpriseProduct);
//        //if (!string.IsNullOrEmpty(autoSurpriseReceipt))
//        //{
//        //    if (autoSurpriseInfo.ShopTable_ID == AutoSurpriseBox)
//        //    {
//        //        if (userShopData.GetPurchaseBuyCount(autoSurpriseInfo) > 0)
//        //        {
//        //            DataManager.Instance.PurchaseRemoveAdvertise();
//        //        }
//        //        else
//        //        {
//        //            userShopData.AddPurchase(autoSurpriseInfo);
//        //        }
//        //    }
//        //}

//        //Chart.ShopInfo autoStageMoveInfo = shopTable.rows.SingleOrDefault(x => x.ShopTable_ID == AutoStageMove);

//        //Product autoStageMoveProduct = GetProductInfo(autoStageMoveInfo.StorePurchaseCode_iOS);
//        //var autoStageMoveReceipt = m_AppleExtensions.GetTransactionReceiptForProduct(autoStageMoveProduct);


//        //if (!string.IsNullOrEmpty(autoStageMoveReceipt))
//        //{
//        //    if (autoStageMoveInfo.ShopTable_ID == AutoStageMove)
//        //    {
//        //        if (userShopData.GetPurchaseBuyCount(autoStageMoveInfo) > 0)
//        //        {
//        //            DataManager.Instance.PurchaseRemoveAdvertise();
//        //        }
//        //        else
//        //        {
//        //            userShopData.AddPurchase(autoStageMoveInfo);
//        //        }
//        //    }
//        //}

//        //Chart.ShopInfo autoStageMoveInfoSale = shopTable.rows.SingleOrDefault(x => x.ShopTable_ID == AutoStageMoveSale);

//        //Product autoStageMoveProductSale = GetProductInfo(autoStageMoveInfoSale.StorePurchaseCode_iOS);
//        //var autoStageMoveReceiptSale = m_AppleExtensions.GetTransactionReceiptForProduct(autoStageMoveProductSale);

//        //if (!string.IsNullOrEmpty(autoStageMoveReceiptSale))
//        //{
//        //    if (autoStageMoveInfoSale.ShopTable_ID == AutoStageMoveSale)
//        //    {
//        //        if (userShopData.GetPurchaseBuyCount(autoStageMoveInfoSale) > 0)
//        //        {
//        //            DataManager.Instance.PurchaseRemoveAdvertise();
//        //        }
//        //        else
//        //        {
//        //            userShopData.AddPurchase(autoStageMoveInfoSale);
//        //        }
//        //    }
//        //}
//        //PopupManager.Instance.SimpleMessage(TextManager.Get("Popup/Notice/RestorePurchase/Complete"));

//        UISystem.Instance.Open<Toast>().Init(TextManager.Get("RestorePurchaseComplete"));
//#endif
//    }

//    public void OnInitializeFailed(InitializationFailureReason error)
//    {
//        UISystem.Instance.Open<PopupDialogBox>().Init(TextManager.Get("notice"),
//            $"{TextManager.Get("Popup/Notice/Shop/Product/Initialize/Fail")}\n{error}", TextManager.Get("check"),
//            _ =>
//            {
//                InitializePurchasing();
//            });
//    }

//    public void OnInitializeFailed(InitializationFailureReason error, string message)
//    {
//        UISystem.Instance.Open<PopupDialogBox>().Init(TextManager.Get("notice"),
//            $"{TextManager.Get("Popup/Notice/Shop/Product/Initialize/Fail")}\n{error}\n{message}",
//            TextManager.Get("check"),
//            _ =>
//            {
//                InitializePurchasing();
//            });
//    }

//    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
//    {
//        UISystem.Instance.Open<PopupDialogBox>().Init(TextManager.Get("notice"),
//            $"{TextManager.Get("Popup/Notice/Shop/Product/Purchase/Fail")}\n{failureDescription.message}",
//            TextManager.Get("check"));
        
//        UISystem.Instance.Find<BlockerPage>()?.ForceClose();
//    }
//    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
//    {
//        UISystem.Instance.Open<PopupDialogBox>().Init(TextManager.Get("notice"),
//            $"{TextManager.Get("Popup/Notice/Shop/Product/Purchase/Fail")}\n{failureReason}",
//            TextManager.Get("check"));
        
//        UISystem.Instance.Find<BlockerPage>()?.ForceClose();
//    }
//}