using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Functions;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using Firebase.Auth;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Google;
using UnityEngine.SocialPlatforms;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.Purchasing;
using System.Threading.Tasks;
#if UNITY_IOS
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#endif

public class FirebaseManager : Singleton<FirebaseManager>
{
    public enum AuthenticatedType
    {
        None,
        Google,
        Apple,
        Email,
    }

    public static class KEY
    {
        public const string GameData = "GameData";
        public const string USER = "Users";
        public const string NICKNAME = "UserNicknames";
        public const string RANKING = "Leaderboard";
        public const string RANKING_ALL = "ALL";
    }

    /// <summary>
    /// 준비완료 상태인지 체크 (데이터베이스레퍼런스, 로그인이 완료 됐는지 체크)
    /// </summary>
    public bool IsReady => db != null && user != null;

    /// <summary>
    /// 로그인 인증 타입 체크
    /// </summary>
    public AuthenticatedType authType
    {
        get
        {
            if (user != null)
            {
                string providerId = user.ProviderData.FirstOrDefault()?.ProviderId;
                if (providerId == "google.com")
                {
                    return AuthenticatedType.Google;
                }
                else if (providerId == "apple.com")
                {
                    return AuthenticatedType.Apple;
                }
                else if (!string.IsNullOrEmpty(user.Email))
                {
                    return AuthenticatedType.Email;
                }
            }
            return AuthenticatedType.None;
        }
    }

    private DatabaseReference db;
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseFunctions functions;
    private DatabaseReference myDB;

    /// </summary>
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseDatabase.DefaultInstance.RootReference;
                auth = FirebaseAuth.DefaultInstance;
                functions = FirebaseFunctions.GetInstance("us-central1");
                user = auth.CurrentUser;

                GoogleSignIn.Configuration = new GoogleSignInConfiguration
                {
                    WebClientId = "8377165168-8tlhbou2cf2kq5it7hnedqfeqr8cp7ak.apps.googleusercontent.com",
                    UseGameSignIn = false,
                    RequestEmail = true,
                    RequestIdToken = true
                };
#if UNITY_IOS
                if (AppleAuthManager.IsCurrentPlatformSupported)
                {
                    _appleAuthManager = new AppleAuthManager(new PayloadDeserializer());
                }
#endif
                //인증된 로그인 상태가 아니라면 익명로그인 시도 
                if (authType == AuthenticatedType.None) SignInAnonymously();
            }
            else
            {
                Debug.LogError("Firebase 초기화 실패: " + task.Result);
            }
        });
    }
    
    /// <summary>
    /// 익명 로그인
    /// </summary>
    private void SignInAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                UIManager.Instance.Message.Show(Message.Type.Confirm, "Retry Connect", callback: confirm =>
                {
                    SignInAnonymously();
                });
                return;
            }

            user = task.Result.User;
        });
    }

    /// <summary>
    /// 모든 게임데이터 불러오기
    /// 데이터매니저에서 호출 
    /// </summary>
    /// <param name="callback"></param>
    public void LoadAllGameDatas(Action<DataSnapshot> callback)
    {
        db.Child(KEY.GameData).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if(task.IsCompletedSuccessfully)
            {
                callback.Invoke(task.Result);
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("❌ GameData Load Failed (Exception): " + task.Exception);
            }
            else if (task.IsCanceled)
            {
                Debug.LogWarning("⚠️ GameData Load was canceled.");
            }
            else
            {
                Debug.LogError("Fail Load GameData");
            }
        });
    }

    /// <summary>
    /// 유저데이터 저장
    /// </summary>
    /// <param name="data"></param>
    public void SaveUserData(UserData data)
    {
        if (myDB == null)
        {
            myDB = db.Child(KEY.USER).Child(data.id);
            myDB.ValueChanged += HandleMyDBChanged;
        }
        string json = JsonConvert.SerializeObject(data);
        myDB.SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("User data saved successfully.");
            }
            else
            {
                Debug.LogError($"Failed to save user data: {task.Exception}");
            }
        });
    }

    /// <summary>
    /// 자신의 유저데이터 가져오기
    /// 데이터매니저에서 호출
    /// </summary>
    /// <param name="callback"></param>
    public void GetUserData(Action<UserData> callback)
    {
        if (myDB == null)
        {
            myDB = db.Child(KEY.USER).Child(user.UserId);
            myDB.ValueChanged += HandleMyDBChanged;
        }

        myDB.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    UserData myData = JsonConvert.DeserializeObject<UserData>(snapshot.GetRawJsonValue());
                    if (!string.IsNullOrEmpty(myData.banMessage))
                    {
                        UIManager.Instance.Message.Show(Message.Type.Simple, myData.banMessage, callback: confirm =>
                        {
                            Application.Quit();
                        });
                        return;
                    }
                    callback.Invoke(myData);
                }
                else
                {
                    Debug.Log("No user found.");
                    try
                    {
                        callback.Invoke(new UserData(user.UserId));
                    }
                    catch(System.Exception exception)
                    {
                        Debug.LogError(exception.ToString());
                    }
                }
            }
            else
            {
                Debug.LogError($"Failed to get user data: {task.Exception}");
            }
        });
    }

    /// <summary>
    /// 자신의 유저데이터가 변경됐을때의 리스너
    /// 계정생성시 서버에서 닉네임 생성시 호출
    /// 또는 관리툴에서 밴할시에 호출
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void HandleMyDBChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"유저 데이터 변경 중 오류: {args.DatabaseError.Message}");
            return;
        }

        if (args.Snapshot.Exists)
        {
            string json = args.Snapshot.GetRawJsonValue();
            UserData myData = JsonConvert.DeserializeObject<UserData>(json);

            ///밴 처리
            if (!string.IsNullOrEmpty(myData.banMessage))
            {
                UIManager.Instance.Message.Show(Message.Type.Simple, myData.banMessage, callback: confirm =>
                {
                    Application.Quit();
                });
                return;
            }

            DataManager.Instance.UpdateUserData(myData);
        }
    }

    /// <summary>
    /// 이메일 로그인
    /// 에디터에서만 기능제공
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    public void SignInWithEmail(string email, string password)
    {
        LinkAnonymousToAuth(EmailAuthProvider.GetCredential(email, password));
    }


    /// <summary>
    /// 구글 로그인
    /// </summary>
    public void StartGoogleLogin()
    {
        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"{task.Status} | {task.Exception} | {task.Exception?.Message} | {task.Exception?.StackTrace}");
                return;
            }
            LinkAnonymousToAuth(GoogleAuthProvider.GetCredential(task.Result.IdToken, null));
        });
    }

    /// <summary>
    /// 익명 로그인에서 인증 로그인으로 변경
    /// </summary>
    /// <param name="credential"></param>
    private void LinkAnonymousToAuth(Credential credential)
    {
        string anonymousUid = user.UserId;

        ///익명에서 인증로그인으로 전환시도
        user.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            ///전환 성공시
            if (task.IsCompletedSuccessfully)
            {
                user = task.Result.User;
                DataManager.Instance.RefreshUserData();
                UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AuthenticationSuccess"));
                UIManager.Instance.Get<PopupSettings>().Refresh();
            }
            else
            {
                ///전환 실패시 로그인시도했던 유저로 로그인
                auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(signInTask =>
                {
                    if (signInTask.IsCompletedSuccessfully)
                    {
                        FirebaseUser signedInUser = signInTask.Result;
                        /// 기존 익명로그인 계정 유저데이터 삭제
                        DeleteAnonymousUserData(anonymousUid, signedInUser.UserId);
                    }
                    else
                    {
                        UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AuthenticationFail"));
                    }
                });
            }
        });
    }

    /// <summary>
    /// 익명 계정 삭제및 로그인 계정으로 새로고침
    /// </summary>
    /// <param name="anonymousUid"></param>
    /// <param name="newUid"></param>
    private void DeleteAnonymousUserData(string anonymousUid, string newUid)
    {
        Debug.Log($"DeleteAnonymousUserData | anonymousUid : {anonymousUid} | authUid : {newUid}");
        var data = new Dictionary<string, object>
        {
            { "anonymousUid", anonymousUid }
        };
        functions.GetHttpsCallable("deleteUserData").CallAsync(data).ContinueWithOnMainThread(task =>
        {
            /// 익명 계정 삭제 성공시
            if (task.IsCompletedSuccessfully)
            {
                myDB = null;
                user = FirebaseAuth.DefaultInstance.CurrentUser;
                DataManager.Instance.RefreshUserData();
                UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("FederatedSuccess"));
                UIManager.Instance.Get<PopupSettings>().Refresh();
            }
            else /// 실패시 로그아웃 후 게임종료 
            {
                UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AuthenticationFail"));
                LogOut();
            }
        });
    }

    /// <summary>
    /// 애플 서비스 시작시에 작업필요
    /// </summary>
#if UNITY_IOS
    private IAppleAuthManager _appleAuthManager;
#endif
    public void StartAppleLogin()
    {
#if UNITY_IOS
        if (_appleAuthManager == null)
        {
            Debug.LogError("AppleAuthManager not initialized");
            return;
        }

        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

        _appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                if (credential is IAppleIDCredential appleIDCredential)
                {
                    var userId = appleIDCredential.User;
                    var identityToken = System.Text.Encoding.UTF8.GetString(appleIDCredential.IdentityToken);
                    var authorizationCode = System.Text.Encoding.UTF8.GetString(appleIDCredential.AuthorizationCode);

                    Debug.Log($"Apple SignIn 성공!\nUserId: {userId}\nIdentityToken: {identityToken}");

                    // 서버에 identityToken 전송해서 검증 가능
                    // 또는 Firebase Auth 연동
                }
            },
            error =>
            {
                Debug.LogError($"Apple SignIn 실패: {error}");
            }
        );
#endif
    }

    public void LogOut()
    {
        auth.SignOut();
        Application.Quit();
    }

    /// <summary>
    /// 공용 데이터 삽입
    /// </summary>
    /// <param name="refName"></param>
    /// <param name="rawJson"></param>
    public void InsertData(string refName, string rawJson)
    {
        db.Child(refName).SetRawJsonValueAsync(rawJson).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log($"InsertData {refName} | {rawJson}");
            }
            else
            {
                Debug.LogError($"Fiel InserData {task.Exception} ");
            }
        });
    }

    /// <summary>
    /// 닉네임
    /// </summary>
#region NicknameCheck
    public struct ResultCheckNickname
    {
        public bool success;
        public string message;
    }

    /// <summary>
    /// 닉네임이 변경가능한지 체크
    /// </summary>
    /// <param name="nickname">체크할 닉네임</param>
    /// <param name="callback">결과 반환</param>
    public void CheckNickname(string nickname, Action<ResultCheckNickname> callback)
    {
        ResultCheckNickname result = default;

        if (nickname == null)
        {
            result.success = false;
            result.message = TextManager.Get("nicknameNull");
            callback.Invoke(result);
        }

        if (nickname.Length < 2 || nickname.Length > 10)
        {
            result.success = false;
            result.message = TextManager.Get("nicknameLengthError");
            callback.Invoke(result);
        }

        string resultNick = Regex.Replace(nickname, @"[^a-zA-Z0-9가-힇ぁ-ゔァ-ヴー々〆〤一-龥]", "", RegexOptions.Singleline);
        if (resultNick != nickname)
        {
            result.success = false;
            result.message = TextManager.Get("nicknameException");
            callback.Invoke(result);
        }
        GameData.ForbiddenWord[] forbiddenWordTable = DataManager.Instance.Get<GameData.ForbiddenWord>();
        foreach (var info in forbiddenWordTable)
        {
            if (nickname.Contains(info.word))
            {
                result.success = false;
                result.message = TextManager.Get("nicknameForbiddenWord");
                callback.Invoke(result);
            }
        }

        HasServerNickname(nickname, has =>
        {
            if(has)
            {
                result.success = false;
                result.message = TextManager.Get("nicknameDuplicate");
                callback.Invoke(result);
            }
            else
            {
                result.success = true;
                result.message = TextManager.Get("nickname_ok");
                callback.Invoke(result);
            }
        });
    }
    /// <summary>
    /// 서버에 중복된 닉네임이 있는지 체크
    /// </summary>
    /// <param name="nickName"></param>
    /// <param name="result"></param>
    public void HasServerNickname(string nickName, Action<bool> result)
    {
        db.Child(KEY.NICKNAME).Child(nickName).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if(task.IsFaulted)
            {
                result.Invoke(true);
                return;
            }
            DataSnapshot snapshot = task.Result;
            result.Invoke(snapshot.Exists);
        });
    }

    /// <summary>
    /// 닉네임 변경
    /// </summary>
    /// <param name="nickname"></param>
    /// <param name="callback"></param>
    public void UpdateNickName(string nickname, Action<ResultCheckNickname> callback)
    {
        functions.GetHttpsCallable("changeNickname").CallAsync(new Dictionary<string, object> { { "nickname", nickname } }).ContinueWithOnMainThread(task =>
        {
            ResultCheckNickname result = default;
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("닉네임 변경 성공!");
                result.success = true;
                result.message = TextManager.Get("nickname_ok");
                DataManager.Instance.userData.nickname = nickname;
                callback?.Invoke(result);
            }
            else
            {
                Debug.LogError($"닉네임 변경 실패: {task.Exception}");
                result.success = false;
                result.message = task.Exception.Message;
                callback?.Invoke(result);
            }
        });
    }
#endregion

    /// <summary>
    /// 랭킹에 점수 등록
    /// </summary>
    /// <param name="gameLevel">퍼즐 난이도</param>
    /// <param name="date">오늘날짜 또는 전체</param>
    /// <param name="point">점수</param>
    /// <param name="callback"></param>
    public void SubmitScore(PuzzleManager.Level gameLevel, string date, int point, Action<int> callback = null)
    {
        SubmitScore(gameLevel, date, DataManager.Instance.userData.id, DataManager.Instance.userData.nickname, DataManager.Instance.userData.level, point, DataManager.Instance.userData.countryCode, callback);
    }

    /// <summary>
    /// 랭킹에 점수 등록
    /// </summary>
    /// <param name="gameLevel"></param>
    /// <param name="date"></param>
    /// <param name="userId"></param>
    /// <param name="nickname"></param>
    /// <param name="level"></param>
    /// <param name="point"></param>
    /// <param name="countryCode"></param>
    /// <param name="callback"></param>
    public void SubmitScore(PuzzleManager.Level gameLevel, string date, string userId, string nickname, int level, int point, string countryCode, Action<int> callback = null)
    {
        var data = new Dictionary<string, object>
        {
            { "gameLevel", gameLevel.ToString() },
            { "date", date },
            { "userId", userId },
            { "level", level},
            { "nickname", nickname },
            { "point", point },
            { "countryCode", countryCode }
        };
        functions.GetHttpsCallable("SubmitScore").CallAsync(data).ContinueWithOnMainThread(task =>
        {
            if(task.IsCompletedSuccessfully)
            {
                var result = task.Result.Data as Dictionary<string, object>;
                callback?.Invoke((int)result["myRank"]);
            }
            else
            {
                callback?.Invoke(0);
            }
        });
    }

    public void GetRankingFromServer(string userId, Action<PopupRanking.RankingListWithMyRank> callback = null, string date = "ALL", int limit = 10, PuzzleManager.Level gameLevel = PuzzleManager.Level.Normal)
    {
        var data = new Dictionary<string, object>
        {
            { "gameLevel", gameLevel.ToString() },
            { "date", date },
            { "userId", userId },
            { "limit", limit }
        };

        functions.GetHttpsCallable("GetRanking").CallAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                try
                {
                    var result = task.Result.Data as Dictionary<object, object>;

                    //foreach(var item in result)
                    //{
                    //    Debug.Log(item.Key + " | " + item.Value);
                    //}
                    // Top 랭킹 파싱
                    var topRankings = result["topRankings"] as List<object>;
                    Debug.Log("=== 전체 랭킹 ===");
                    PopupRanking.RankingListWithMyRank resultData = new PopupRanking.RankingListWithMyRank();
                    resultData.topRanks = new List<RankingList.Data>();

                    for (int i = 0; i < topRankings.Count; i++)
                    {
                        var entry = topRankings[i] as Dictionary<object, object>;
                        RankingList.Data data = JsonConvert.DeserializeObject<RankingList.Data>(JsonConvert.SerializeObject(entry));
                        resultData.topRanks.Add(data);
                    }

                    resultData.topRanks.Sort();
                    for(int i = 0; i < resultData.topRanks.Count; i++)
                    {

                    }

                    resultData.topRanks = resultData.topRanks.OrderBy(x => x.rank == 0 ? int.MaxValue : x.rank).ToList();
                    // 내 랭킹 파싱
                    int myRank = Convert.ToInt32(result["myRank"]);
                    if (myRank > 0)
                    {
                        var myEntry = result["myEntry"] as Dictionary<object, object>;
                        RankingList.Data data = JsonConvert.DeserializeObject<RankingList.Data>(JsonConvert.SerializeObject(myEntry));
                        data.rank = myRank;
                        resultData.myRank = data;
                    }
                    else
                    {
                        Debug.Log("내 랭킹 정보가 없습니다.");
                    }
                    callback?.Invoke(resultData);
                }
                catch(Exception e)
                {
                    Debug.Log(e);
                    callback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError("랭킹 가져오기 실패: " + task.Exception);
                callback?.Invoke(null);
            }
            
        });
    }

    #region IAP
    /// <summary>
    /// 영수증 데이터 가져오기
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public ReceiptData GetReceiptData(PurchaseEventArgs e)
    {
        ReceiptData data = new ReceiptData();

        if (e != null)
        {
            //Main receipt root
            string receiptString = e.purchasedProduct.receipt;
            Debug.Log("receiptString " + receiptString);
            var receiptDict = (Dictionary<string, object>)MiniJson.JsonDecode(receiptString);
            Debug.Log("receiptDict COUNT" + receiptDict.Count);
#if UNITY_ANDROID
            //Next level Paylod dict
            string payloadString = (string)receiptDict["Payload"];
            Debug.Log("payloadString " + payloadString);
            var payloadDict = (Dictionary<string, object>)MiniJson.JsonDecode(payloadString);

            //Stuff from json object
            string jsonString = (string)payloadDict["json"];
            Debug.Log("jsonString " + jsonString);
            var jsonDict = (Dictionary<string, object>)MiniJson.JsonDecode(jsonString);
            string orderIdString = (string)jsonDict["orderId"];
            Debug.Log("orderIdString " + orderIdString);
            string packageNameString = (string)jsonDict["packageName"];
            Debug.Log("packageNameString " + packageNameString);
            string productIdString = (string)jsonDict["productId"];
            Debug.Log("productIdString " + productIdString);

            double orderDateDouble = System.Convert.ToDouble(jsonDict["purchaseTime"]);
            Debug.Log("orderDateDouble " + orderDateDouble);

            string purchaseTokenString = (string)jsonDict["purchaseToken"];
            Debug.Log("purchaseTokenString " + purchaseTokenString);

            string signatureString = (string)payloadDict["signature"];
            Debug.Log("signatureString " + signatureString);


            //Creating UTC from Epox
            System.DateTime orderDateTemp = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            orderDateTemp = orderDateTemp.AddMilliseconds(orderDateDouble);

            data.orderId = orderIdString;
            data.packageName = packageNameString;
            data.productId = productIdString;
            data.purchaseToken = purchaseTokenString;
            //data.priceAmountMicros = priceAmountMicrosLong;
            //data.priceCurrencyCode = priceCurrencyCodeString;
            data.orderDate = orderDateTemp;
            data.receipt = receiptString;
            data.signature = signatureString;
            data.json = jsonString;
#endif
            Debug.Log("GetReceiptData succesfull");
        }
        else
        {
            Debug.Log("PurchaseEventArgs is NULL");
        }

        return data;
    }

    public class ReceiptData
    {
        public string orderId;
        public string packageName;
        public string productId;
        public string purchaseToken;
        public DateTime orderDate;
        public string receipt;
        public string signature;
        public string json;
        public override string ToString()
        {
            return new StringBuilder().Append("orderId : ").Append(orderId).AppendLine()
                .Append("packageName : ").Append(packageName).AppendLine()
                .Append("productId : ").Append(productId).AppendLine()
                .Append("purchaseToken : ").Append(purchaseToken).ToString();

        }
    }

    private class PurchaseData
    {
        public string productId;
        public string purchaseToken;
    }

    /// <summary>
    /// 영수증 검증
    /// </summary>
    /// <param name="args"></param>
    /// <param name="onResult"></param>
    public void ValidatePurchase(PurchaseEventArgs args, Action<bool> onResult)
    {
#if UNITY_ANDROID
        string url = "https://us-central1-maketen-2631f.cloudfunctions.net/validatePurchase";
        ReceiptData receipt = GetReceiptData(args);
        var json = JsonConvert.SerializeObject(new PurchaseData()
        {
            productId = args.purchasedProduct.definition.id,
            purchaseToken = receipt.purchaseToken
        });
#elif UNITY_IOS
        string url = "https://us-central1-maketen-2631f.cloudfunctions.net/validatePurchaseiOS";
        var postData = new
        {
            receiptData = args.purchasedProduct.receipt,
        };

        string json = JsonConvert.SerializeObject(postData);
#endif
        Debug.Log($"ValidatePurchase | {json}");
        StartCoroutine(PostValidate(url, json, onResult));
    }
    /// <summary>
    /// 실제 영수증 검증 호출
    /// </summary>
    /// <param name="url"></param>
    /// <param name="json"></param>
    /// <param name="onResult"></param>
    /// <returns></returns>
    private IEnumerator PostValidate(string url, string json, Action<bool> onResult)
    {
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result == UnityWebRequest.Result.Success)
#else
            if (!www.isHttpError && !www.isNetworkError)
#endif
            {
                onResult?.Invoke(true);
            }
            else
            {
                Debug.LogError($"[FirebaseValidator] 요청 실패: {www.error}");
                onResult?.Invoke(false);
            }
        }
    }
    #endregion

    /// <summary>
    /// 유저에게 메일 보내기
    /// 상품 구매후에 보상 보내기
    /// </summary>
    /// <param name="title"></param>
    /// <param name="desc"></param>
    /// <param name="rewards"></param>
    public void SendMail(string title, string desc, GoodsList.Data[] rewards)
    {
        string mailId = myDB.Child("mailDatas").Push().Key;

        var mailData = new Dictionary<string, object>
        {
            { "id", mailId },
            { "title", title },
            { "desc", desc },
            { "rewards", rewards },
            { "receiveDate", GameManager.Instance.dateTime.Value.ToTimeText() }
        };

        myDB.Child("mailDatas")
            .SetValueAsync(mailData)
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                    Debug.Log($"메일 전송 완료: {mailId}");
                else
                    Debug.LogError($"메일 전송 실패: {task.Exception}");
            });
    }

    private void Update()
    {
#if UNITY_IOS
        _appleAuthManager?.Update();
#endif
    }
}
