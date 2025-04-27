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

public class FirebaseManager : Singleton<FirebaseManager>
{
    public enum AuthenticatedType
    {
        None,
        Google,
        Apple,
    }

    public static class KEY
    {
        public static string USER = "Users";
        public static string NICKNAME = "UserNicknames";
        public static string RANKING = "Leaderboard";
        public static string RANKING_ALL = "ALL";
    }

    public bool IsReady => db != null;
    private DatabaseReference db;
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseFunctions functions;
    /// </summary>
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase Ready");
                FirebaseApp app = FirebaseApp.DefaultInstance;
                db = FirebaseDatabase.DefaultInstance.RootReference;
                auth = FirebaseAuth.DefaultInstance;
                functions = FirebaseFunctions.GetInstance("asia-southeast1");
                user = auth.CurrentUser;

                GoogleSignIn.Configuration = new GoogleSignInConfiguration
                {
                    //8377165168-vo1mlgvteg95clbfad5gm25hk50vo8ke.apps.googleusercontent.com
                    //8377165168-0uskm7n18l5fbeueqpla74soog96k0g3.apps.googleusercontent.com
                    WebClientId = "8377165168-8tlhbou2cf2kq5it7hnedqfeqr8cp7ak.apps.googleusercontent.com",
                    //WebClientId = "8377165168-0uskm7n18l5fbeueqpla74soog96k0g3.apps.googleusercontent.com",
                    UseGameSignIn = false,
                    RequestEmail = true,
                    RequestIdToken = true
                };

#if UNITY_ANDROID
                //InitializePlayGamesLogin();
#endif
            }
            else
            {
                Debug.LogError("Firebase 초기화 실패: " + task.Result);
            }
        });
    }

    public void LoadAllGameDatas(Action<DataSnapshot> callback)
    {
        db.Child("GameData").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if(task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                callback.Invoke(snapshot);
            }
            else
            {
                Debug.LogError("Fail Load GameData");
            }
        });
        //db.Child("GameData").
    }

    private DatabaseReference myDB;
    public void SaveUserData(UserData data)
    {
        if (myDB == null)
        {
            myDB = db.Child(KEY.USER).Child(data.id);
            myDB.ValueChanged += HandleMyDBChanged;
        }
        string json = JsonConvert.SerializeObject(data);
        Debug.Log("SaveUserData " + json);
        myDB.SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("User data saved successfully.");
            }
            else
            {
                Debug.LogError("Failed to save user data: " + task.Exception);
            }
        });
    }

    public void GetUserData(Action<UserData> callback)
    {
        if(user != null)
        {
            GetUserData(user.UserId, callback);
        }
        else
        {
            DeviceIDManager.deviceIDHandler += OnDeviceID;
            DeviceIDManager.GetDeviceID();

            void OnDeviceID(string deviceID)
            {
                Debug.Log($"::: CustomLogin {deviceID}");
                if (!string.IsNullOrEmpty(deviceID))
                {
                    GetUserData(deviceID, callback);
                }

                DeviceIDManager.deviceIDHandler -= OnDeviceID;
            }
            
        }
    }

    private void HandleMyDBChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("유저 데이터 변경 중 오류: " + args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot.Exists)
        {
            string json = args.Snapshot.GetRawJsonValue();
            UserData myData = JsonConvert.DeserializeObject<UserData>(json);
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

    public void GetUserData(string userId, Action<UserData> callback)
    {
        if (myDB == null)
        {
            myDB = db.Child(KEY.USER).Child(userId);
            myDB.ValueChanged += HandleMyDBChanged;
        }

        myDB.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Debug.Log("User Info: " + snapshot.GetRawJsonValue());
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
                    callback.Invoke(new UserData(userId));
                }
            }
            else
            {
                Debug.LogError("Failed to get user data: " + task.Exception);
            }
        });
    }

    public void IsUserData(string userId, Action<bool> callback)
    {
        db.Child(KEY.USER).Child(userId).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    callback.Invoke(true);
                }
                else
                {
                    callback.Invoke(false);
                }
            }
            else
            {
                Debug.LogError("Failed to get user data: " + task.Exception);
            }
        });
    }

    public void RemoveUserId(string userKey)
    {
        db.Child(KEY.USER).Child(userKey).RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.IsCompleted)
            {
                
            }
            else
            {
                Debug.LogWarning("기존 키에 데이터가 없습니다.");
            }
        });
    }

    public void StartGoogleLogin()
    {
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
        OnAuthenticationFinished);
    
        //TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin(true, GoogleLoginCallback);
        //GoogleSignIn
        //PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
        //PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptOnce, (result) =>
        //{
        //    if (result == SignInStatus.Success)
        //    {
        //        Debug.Log("GPGS 로그인 성공!");
        //        string idToken = PlayGamesPlatform.Instance.GetIdToken();
        //        Debug.Log("ID Token: " + idToken);
        //        GoogleLoginCallback(idToken);
        //        // 이 토큰을 Unity Authentication에 전달할 수 있음
        //    }
        //    else
        //    {
        //        Debug.LogError("GPGS 로그인 실패: " + result);
        //    }
        //});
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        Debug.Log("Authentication finished, processing on main thread");
        MainThreadDispatcher.Instance.Enqueue(() => ProcessAuthResult(task));
    }

    private void ProcessAuthResult(Task<GoogleSignInUser> task)
    {
        Debug.Log($"ProcessAuthResult task : {task.IsCompletedSuccessfully}");
        if (task.IsFaulted)
        {
            Debug.LogError($"{task.Status} | {task.Exception} | {task.Exception?.Message} | {task.Exception?.StackTrace}");
            return;
        }
        
        auth.SignInWithCredentialAsync(GoogleAuthProvider.GetCredential(task.Result.IdToken, null)).ContinueWith(authTask =>
        {
            if (authTask.IsCompleted)
            {
                user = authTask.Result;
                IsUserData(user.UserId, isUser =>
                {
                    Debug.Log($"IsUserData | {isUser}");
                    if (isUser)
                    {
                        UIManager.Instance.Message.Show(Message.Type.Ask, TextManager.Get("ExistUserData"), callback: result =>
                        {
                            if (result)
                            {
                                //DataManager.Instance.userData.UpdateData(user.UserId, AuthenticatedType.Google);
                                myDB = null;
                                GetUserData(user.UserId, userResult =>
                                {
                                    DataManager.Instance.userData = userResult;
                                    RemoveUserId(SystemInfo.deviceUniqueIdentifier);
                                    UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("FederatedSuccess"));
                                    HUD.Instance.UpdateUserData(DataManager.Instance.userData);
                                });
                                
                            }
                            else
                            {
                                auth.SignOut();
                            }
                            UIManager.Instance.Get<PopupSettings>().Refresh();
                            //UIManager.Instance.Main.Refresh();
                        });
                    }
                    else
                    {
                        myDB = null;
                        UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AuthenticationSuccess"));
                        DataManager.Instance.userData.UpdateData(user.UserId, AuthenticatedType.Google);
                        RemoveUserId(SystemInfo.deviceUniqueIdentifier);
                        UIManager.Instance.Get<PopupSettings>().Refresh();
                        HUD.Instance.UpdateUserData(DataManager.Instance.userData);
                    }
                });
            }
            //else
            //{
            //    user = authTask.Result;
            //    UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AuthenticationSuccess"));
            //    DataManager.Instance.userData.UpdateData(user.UserId, AuthenticatedType.Google);
            //    RemoveUserId(SystemInfo.deviceUniqueIdentifier);
            //    UIManager.Instance.Get<PopupSettings>().Refresh();
            //}
        });
    }

    private void GoogleLoginCallback(bool isSuccess, string errorMessage, string token)
    {
        if (isSuccess == false)
        {
            Debug.LogError(errorMessage);
            return;
        }
        auth.SignInWithCredentialAsync(GoogleAuthProvider.GetCredential(token, null)).ContinueWith(authTask =>
        {
            if (authTask.IsCompleted)
            {
                IsUserData(user.UserId, isUser =>
                {
                    if(isUser)
                    {
                        UIManager.Instance.Message.Show(Message.Type.Ask, TextManager.Get("ExistUserData"), callback: result =>
                        {
                            if(result)
                            {
                                user = authTask.Result;
                                UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("FederatedSuccess"));
                            }
                            else
                            {
                                auth.SignOut();
                            }
                            UIManager.Instance.Get<PopupSettings>().Refresh();
                            //UIManager.Instance.Main.Refresh();
                        });
                    }
                    else
                    {
                        user = authTask.Result;
                        UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AuthenticationSuccess"));
                        DataManager.Instance.userData.UpdateData(user.UserId, AuthenticatedType.Google);
                        RemoveUserId(SystemInfo.deviceUniqueIdentifier);
                        UIManager.Instance.Get<PopupSettings>().Refresh();
                        //UIManager.Instance.Main.Refresh();
                    }
                });
            }
        });
    }

    public void StartAppleLogin()
    {
        //TheBackend.ToolKit.AppleLogin.Android.AppleLogin("com.thebackend.testapp.applelogin", out var error, true, token => {
        //    Debug.Log("토큰 : " + token);
        //    Debug.Log("토큰 발급이 완료되었습니다. 로그인이 가능합니다.");
        //    auth.SignInWithCredentialAsync(OAuthProvider.GetCredential("apple.com", token, null, null)).ContinueWith(authTask =>
        //    {
        //        if (authTask.IsCompleted && !authTask.IsFaulted)
        //        {
        //            user = authTask.Result;
        //        }
        //    });


        //    // 경고! : 다음과 같이 동기 함수를 호출하지 마세요
        //    // var bro = Backend.BMember.AuthorizeFederation(token, FederationType.Apple);

        //    // 아래와 같이 비동기 함수를 호출해주세요,
        //    //Backend.BMember.AuthorizeFederation(token, FederationType.Apple, callback => {
        //    //    Debug.Log("애플 로그인 결과 : " + callback);
        //    //});
        //});

        //if (string.IsNullOrEmpty(error) == false)
        //{
        //    Debug.Log("에러 : " + error);
        //}
    }

    public void LogOut()
    {
        auth.SignOut();
        Application.Quit();
    }

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

    #region NicknameCheck
    public string FreeNick
    {
        get
        {
            string[] strs = DataManager.Instance.userData.id.Split('-');
            return new StringBuilder().Append("Player").Append(strs.First().Substring(0, 6)).ToString();
        }
    }

    public bool IsFreeNick
    {
        get
        {
            if (FreeNick.Contains(DataManager.Instance.userData.nickname)) return true;
            return false;
        }
    }

    public void CreateAvailableNickname(Action<string> callback)
    {
        if (!string.IsNullOrEmpty(DataManager.Instance.userData.nickname))
            return;

        string freeNick = FreeNick;
        HasServerNickname(freeNick, has =>
        {
            if(has)
            {
                NextFreeNick(freeNick, 0, callback);
            }
            else
            {
                callback.Invoke(freeNick);
            }
        });
    }

    private void NextFreeNick(string nick, int i, Action<string> callback)
    {
        string currentNick = string.Format("{0}{1:000}", nick, i);
        HasServerNickname(currentNick, has =>
        {
            if (has)
            {
                NextFreeNick(nick, i + 1, callback);
            }
            else
            {
                callback.Invoke(currentNick);
            }
        });
    }

    public struct ResultCheckNickname
    {
        public bool success;
        public string message;
    }

    public void CheckNickname(string nickname, Action<ResultCheckNickname> callback)
    {
        ResultCheckNickname result = default;

        if (nickname == null)
        {
            result.success = false;
            result.message = TextManager.Get("nicknameNull");
            callback.Invoke(result);
        }
        if (nickname.Length < 2)
        {
            result.success = false;
            result.message = TextManager.Get("nicknameNull");
            callback.Invoke(result);
        }
        if (nickname.Length > 10)
        {
            result.success = false;
            result.message = TextManager.Get("nicknameNull");
            callback.Invoke(result);
        }
        string resultNick = Regex.Replace(nickname, @"[^a-zA-Z0-9가-힇ぁ-ゔァ-ヴー々〆〤一-龥]", "", RegexOptions.Singleline);
        if (resultNick != nickname)
        {
            result.success = false;
            result.message = TextManager.Get("nicknameNull");
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
                result.message = TextManager.Get("nicknameAlready_Exists");
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

    public void UpdateNickName(string nickname, Action<ResultCheckNickname> callback)
    {
        var updates = new Dictionary<string, object>
        {
            [$"{KEY.NICKNAME}/{nickname}"] = DataManager.Instance.userData.id,
            [$"{KEY.USER}/{DataManager.Instance.userData.id}/nickname"] = nickname
        };
        updates[$"{KEY.NICKNAME}/{DataManager.Instance.userData.nickname}"] = null;

        db.UpdateChildrenAsync(updates).ContinueWithOnMainThread(updateTask =>
        {
            ResultCheckNickname result = default;
            if (updateTask.IsFaulted)
            {
                Debug.LogError("닉네임 변경 실패: " + updateTask.Exception);
                result.success = false;
                result.message = updateTask.Exception.Message;
                callback?.Invoke(result);
            }
            else
            {
                Debug.Log("닉네임 변경 성공!");
                result.success = true;
                result.message = TextManager.Get("nickname_ok");
                DataManager.Instance.userData.nickname = nickname;
                callback?.Invoke(result);
            }
        });
    }
    #endregion


    public void SubmitScoreLevel(int exp, Action<int> callback = null)
    {
        RankingList.LevelData entry = new RankingList.LevelData(DataManager.Instance.userData.id, DataManager.Instance.userData.level, DataManager.Instance.userData.nickname, exp, DataManager.Instance.userData.countryCode);

        db.Child(KEY.RANKING).Child("Level").Child(DataManager.Instance.userData.id).SetRawJsonValueAsync(JsonConvert.SerializeObject(entry));
        callback?.Invoke(0);
//#if UNITY_EDITOR

//#else
//        var data = new Dictionary<string, object>
//        {
//            { "gameLevel", gameLevel.ToString() },
//            { "date", date },
//            { "id", entry.id },
//            { "level", entry.level },
//            { "name", entry.name },
//            { "point", entry.point },
//            { "remainMilliSeconds", entry.remainMilliSeconds },
//            { "timeStamp", entry.timeStamp },
//            { "countryCode", entry.countryCode }
//        };

//        functions.GetHttpsCallable("SubmitScore").CallAsync(data).ContinueWith(task =>
//        {
//            if (task.IsFaulted)
//            {
//                Debug.LogError("랭킹 등록 실패: " + task.Exception);
//                return;
//            }

//            var result = task.Result.Data as Dictionary<string, object>;
//            int myRank = Convert.ToInt32(result["myRank"]);
//            callback?.Invoke(myRank);
//        });
//#endif


    }

    public void SubmitScore(PuzzleManager.Level gameLevel, string date, int score, Action<int> callback = null)
    {
        Debug.Log($"SubmitScore  {gameLevel} | {date} | {score}");
        Debug.Log($"userData : {DataManager.Instance.userData}");
        RankingList.PointData entry = new RankingList.PointData(DataManager.Instance.userData.id, DataManager.Instance.userData.level, DataManager.Instance.userData.nickname, score, DataManager.Instance.userData.countryCode);
        Debug.Log($"entry : {entry}");
        db.Child(KEY.RANKING).Child(gameLevel.ToString()).Child(date).Child(DataManager.Instance.userData.id).SetRawJsonValueAsync(JsonConvert.SerializeObject(entry));
        callback?.Invoke(0);
//#if UNITY_EDITOR
//        //db.Child(KEY.RANKING).Child(gameLevel.ToString()).Child(date).GetValueAsync().ContinueWithOnMainThread(task =>
//        //{
//        //    if(task.IsCompletedSuccessfully)
//        //    {
//        //        DataSnapshot snapshot = task.Result;
//        //        List<RankingList.PointData> ranks = new List<RankingList.PointData>();
//        //        if(snapshot.Exists)
//        //        {
//        //            foreach(var child in snapshot.Children)
//        //            {
//        //                var data = child.Value as Dictionary<string, object>;
//        //                RankingList.PointData rankData = new RankingList.PointData(
//        //                    child.Key,
//        //                    data.ContainsKey("rank") ? int.Parse(data["rank"].ToString()) : 0,
//        //                    data.ContainsKey("level") ? int.Parse(data["level"].ToString()) : 1,
//        //                    data.ContainsKey("name") ? data["name"].ToString() : "NoName",
//        //                    data.ContainsKey("point") ? int.Parse(data["point"].ToString()) : 0,
//        //                    data.ContainsKey("remainMilliSeconds") ? int.Parse(data["remainMilliSeconds"].ToString()) : 0,
//        //                    data.ContainsKey("countryCode") ? data["countryCode"].ToString() : "US",
//        //                    data.ContainsKey("timeStamp") ? long.Parse(data["timeStamp"].ToString()) : 0);
//        //                ranks.Add()
//        //            }
//        //        }
//        //    }
//        //});
//        //for (int i = 0; i < topRankings.Count; i++)
//        //{
//        //    var entry = topRankings[i] as Dictionary<string, object>;
//        //    RankingList.Data data = JsonConvert.DeserializeObject<RankingList.Data>(JsonConvert.SerializeObject(entry));
//        //    data.rank = i + 1;
//        //    resultData.topRanks.Add(data);
//        //}

//        //// 내 랭킹 파싱
//        //int myRank = Convert.ToInt32(result["myRank"]);
//        //if (myRank > 0)
//        //{
//        //    var myEntry = result["myEntry"] as Dictionary<string, object>;
//        //    RankingList.Data data = JsonConvert.DeserializeObject<RankingList.Data>(JsonConvert.SerializeObject(myEntry));
//        //    data.rank = myRank;
//        //    resultData.myRank = data;
//        //}
//        //else
//        //{
//        //    Debug.Log("내 랭킹 정보가 없습니다.");
//        //}


//#else
//        var data = new Dictionary<string, object>
//        {
//            { "gameLevel", gameLevel.ToString() },
//            { "date", date },
//            { "id", entry.id },
//            { "level", entry.level },
//            { "name", entry.name },
//            { "point", entry.point },
//            { "remainMilliSeconds", entry.remainMilliSeconds },
//            { "timeStamp", entry.timeStamp },
//            { "countryCode", entry.countryCode }
//        };

//        functions.GetHttpsCallable("SubmitScore").CallAsync(data).ContinueWith(task =>
//        {
//            if (task.IsFaulted)
//            {
//                Debug.LogError("랭킹 등록 실패: " + task.Exception);
//                return;
//            }

//            var result = task.Result.Data as Dictionary<string, object>;
//            int myRank = Convert.ToInt32(result["myRank"]);
//            callback?.Invoke(myRank);
//        });
//#endif
    }

    public void TestSubmitScore(PuzzleManager.Level gameLevel, string date, string userId, string nickname, int score, string countryCode)
    {
        RankingList.PointData entry = new RankingList.PointData(userId, UnityEngine.Random.Range(10, 40), nickname, score, countryCode);
        db.Child(KEY.RANKING).Child(gameLevel.ToString()).Child(date).Child(userId).SetRawJsonValueAsync(JsonConvert.SerializeObject(entry));
    }

    public void GetRankingFromServer(string userId, Action<PopupRanking.RankingListWithMyRank> callback = null, string date = "ALL", int limit = 10, PuzzleManager.Level gameLevel = PuzzleManager.Level.Normal)
    {
//#if UNITY_EDITOR
        db.Child("Leaderboard").Child(gameLevel.ToString()).Child(date).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DataSnapshot dataSnapshot = task.Result;
                if (dataSnapshot.Exists)
                {
                    PopupRanking.RankingListWithMyRank resultData = new PopupRanking.RankingListWithMyRank();
                    resultData.topRanks = new List<RankingList.PointData>();
                    foreach (var user in dataSnapshot.Children)
                    {
                        string id = user.Key;
                        var json = user.Value as Dictionary<string, object>;
                        RankingList.PointData entry = new RankingList.PointData(id,
                            json.ContainsKey("rank") ? Convert.ToInt32(json["rank"].ToString()) : 0,
                            json.ContainsKey("level") ? Convert.ToInt32(json["level"].ToString()) : 0,
                            json.ContainsKey("name") ? json["name"].ToString() : "Unknown",
                            json.ContainsKey("point") ? Convert.ToInt32(json["point"]) : 0,
                            json.ContainsKey("countryCode") ? json["countryCode"].ToString() : "??",
                            json.ContainsKey("timeStamp") ? Convert.ToInt32(json["timeStamp"].ToString()) : 0);

                        resultData.topRanks.Add(entry);
                    }
                    resultData.myRank = resultData.topRanks.SingleOrDefault(x => x.id == userId);
                    resultData.topRanks = resultData.topRanks.OrderBy(x => x.rank == 0 ? int.MaxValue : x.rank).ToList();
                    // 랭킹 포인트 순으로 정렬
                    //rankingList.Sort((a, b) => b.point.CompareTo(a.point));

                    callback?.Invoke(resultData);
                }
                else
                {
                    callback?.Invoke(null);
                }
            }
            else
            {
                callback?.Invoke(null);
            }
        });

        //return;
//#endif
//        var data = new Dictionary<string, object>
//        {
//            { "gameLevel", gameLevel.ToString() },
//            { "date", date },
//            { "userId", userId },
//            { "limit", limit }
//        };

//        functions.GetHttpsCallable("GetRanking").CallAsync(data).ContinueWithOnMainThread(task =>
//        {
//            if (task.IsFaulted)
//            {
//                //foreach (var e in task.Exception.Flatten().InnerExceptions)
//                //{
//                //    Debug.LogError($"Function call error: {e.Message}");
//                //}
//                Debug.LogError("랭킹 가져오기 실패: " + task.Exception);
//                callback?.Invoke(null);
//                return;
//            }

//            var result = task.Result.Data as Dictionary<string, object>;

//            // Top 랭킹 파싱
//            var topRankings = result["topRankings"] as List<object>;
//            Debug.Log("=== 전체 랭킹 ===");
//            PopupRanking.RankingListWithMyRank resultData = new PopupRanking.RankingListWithMyRank();
//            resultData.topRanks = new List<RankingList.PointData>();

//            for (int i = 0; i < topRankings.Count; i++)
//            {
//                var entry = topRankings[i] as Dictionary<string, object>;
//                RankingList.PointData data = JsonConvert.DeserializeObject<RankingList.PointData>(JsonConvert.SerializeObject(entry));
//                resultData.topRanks.Add(data);
//            }

//            resultData.topRanks = resultData.topRanks.OrderBy(x => x.rank == 0 ? int.MaxValue : x.rank).ToList();
//            // 내 랭킹 파싱
//            int myRank = Convert.ToInt32(result["myRank"]);
//            if (myRank > 0)
//            {
//                var myEntry = result["myEntry"] as Dictionary<string, object>;
//                RankingList.PointData data = JsonConvert.DeserializeObject<RankingList.PointData>(JsonConvert.SerializeObject(myEntry));
//                data.rank = myRank;
//                resultData.myRank = data;
//            }
//            else
//            {
//                Debug.Log("내 랭킹 정보가 없습니다.");
//            }
//            callback?.Invoke(resultData);
//        });
    }
    private class PurchaseData
    {
        public string productId;
        public string receipt;
    }
    public void ValidatePurchase(PurchaseEventArgs args, Action<bool> onResult)
    {
        string url = "https://us-central1-maketen-2631f.cloudfunctions.net/validatePurchase";
        var json = JsonConvert.SerializeObject(new PurchaseData()
        {
            productId = args.purchasedProduct.definition.id,
            receipt = args.purchasedProduct.receipt
        });
        StartCoroutine(PostValidate(url, json, onResult));
    }
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
}
