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
        public static string USER = "Users";
        public static string NICKNAME = "UserNicknames";
        public static string RANKING = "Leaderboard";
        public static string RANKING_ALL = "ALL";
    }

    public bool IsReady => db != null && user != null;
    public AuthenticatedType authType
    {
        get
        {
            if(user != null)
            {
                string providerId = user.ProviderData.FirstOrDefault()?.ProviderId;
                Debug.Log("providerId : " + providerId+" | "+user.Email);
                if (providerId == "google.com")
                {
                    return AuthenticatedType.Google;
                }
                else if (providerId == "apple.com")
                {
                    return AuthenticatedType.Apple;
                }
                else if(!string.IsNullOrEmpty(user.Email))
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
    /// </summary>
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
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
                if (user == null) SignInAnonymously();
#if UNITY_ANDROID
                //InitializePlayGamesLogin();
#endif
#if UNITY_IOS
                if (AppleAuthManager.IsCurrentPlatformSupported)
                {
                    _appleAuthManager = new AppleAuthManager(new PayloadDeserializer());
                }
#endif
            }
            else
            {
                Debug.LogError("Firebase 초기화 실패: " + task.Result);
            }
        });
    }

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

    public void LoadAllGameDatas(Action<DataSnapshot> callback)
    {
        db.Child("GameData").GetValueAsync().ContinueWithOnMainThread(task =>
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

    private DatabaseReference myDB;
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
                Debug.LogError("Failed to save user data: " + task.Exception);
            }
        });
    }

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
                Debug.LogError("Failed to get user data: " + task.Exception);
            }
        });
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

    public void SignInWithEmail(string email, string password)
    {
        LinkAnonymousToAuth(EmailAuthProvider.GetCredential(email, password));
    }

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

    private void LinkAnonymousToAuth(Credential credential)
    {
        string anonymousUid = user.UserId;

        user.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            Debug.Log("LinkWithCredentialAsync : " + task.IsCompletedSuccessfully+" | "+task.Exception?.ToString());
            if (task.IsCompletedSuccessfully)
            {
                user = task.Result.User;
                DataManager.Instance.RefreshUserData();
                UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AuthenticationSuccess"));
                UIManager.Instance.Get<PopupSettings>().Refresh();
            }
            else
            {
                auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(signInTask =>
                {
                    if (signInTask.IsCompletedSuccessfully)
                    {
                        FirebaseUser signedInUser = signInTask.Result;
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

    private void MigrateUserData(string anonymousUid, string authUid)
    {
        Debug.Log($"MigrateUserData | anonymousUid : {anonymousUid} | authUid : {authUid}");
        myDB = null;
        var data = new Dictionary<string, object>
        {
            { "anonymousUid", anonymousUid },
            { "authUid", authUid }
        };
        functions.GetHttpsCallable("migrateUserData").CallAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DataManager.Instance.RefreshUserData();
                UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AuthenticationSuccess"));
                UIManager.Instance.Get<PopupSettings>().Refresh();
            }
            else
            {
                UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AuthenticationFail"));
            }
        });
    }

    private void DeleteAnonymousUserData(string anonymousUid, string newUid)
    {
        Debug.Log($"DeleteAnonymousUserData | anonymousUid : {anonymousUid} | authUid : {newUid}");
        var data = new Dictionary<string, object>
        {
            { "anonymousUid", anonymousUid }
        };
        functions.GetHttpsCallable("deleteUserData").CallAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                myDB = null;
                user = FirebaseAuth.DefaultInstance.CurrentUser;
                DataManager.Instance.RefreshUserData();
                UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("FederatedSuccess"));
                UIManager.Instance.Get<PopupSettings>().Refresh();
            }
            else
            {
                UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AuthenticationFail"));
                LogOut();
            }
        });
    }



    //private void AuthCompleteCallback(FirebaseUser authUser, string authToken = "")
    //{
    //    db.Child(KEY.USER).Child(authUser.UserId).GetValueAsync().ContinueWithOnMainThread(userDataTask =>
    //    {
    //        DataSnapshot userData = userDataTask.Result;
    //        if (userData.Exists)
    //        {
    //            UIManager.Instance.Message.Show(Message.Type.Ask, TextManager.Get("ExistUserData"), callback: change =>
    //            {
    //                if (change)
    //                {
    //                    //DataManager.Instance.RefreshUserData();
    //                    var data = new Dictionary<string, object>
    //                    {
    //                        {"anonymousUid", user.UserId }
    //                    };
    //                    functions.GetHttpsCallable("deleteUserData").CallAsync(data).ContinueWithOnMainThread(task =>
    //                    {
    //                        if (task.IsCompletedSuccessfully)
    //                        {
    //                            user = authUser;
    //                            myDB = null;
    //                            DataManager.Instance.RefreshUserData();
    //                            UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("FederatedSuccess"));
    //                            UIManager.Instance.Get<PopupSettings>().Refresh();
    //                        }
    //                        else
    //                        {
    //                            UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AuthenticationFail"));
    //                        }
    //                    });
    //                }
    //            });
    //        }
    //        else
    //        {
    //            Credential credential = null;
    //            switch(currentAuthType)
    //            {
    //                case AuthenticatedType.Google:
    //                    credential = GoogleAuthProvider.GetCredential(authToken, null);
    //                    break;
    //                case AuthenticatedType.Apple:
    //                    //credential = OAuthProvider.GetCredential(authToken,)
    //                    break;
    //                case AuthenticatedType.Email:
    //                    credential = EmailAuthProvider.GetCredential(authUser.Email, authToken);
    //                    break;
    //            }
    //            Debug.Log(authUser.Email + " | " + authToken+" | "+user.Email);
    //            user.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(linkTask =>
    //            {
    //                Debug.Log(linkTask.Exception?.ToString());
    //                if(linkTask.IsCompletedSuccessfully)
    //                {
    //                    myDB = null;
    //                    var data = new Dictionary<string, object>
    //                    {
    //                        {"anonymousUid", user.UserId },
    //                        {"authUid", authUser.UserId }
    //                    };
    //                    functions.GetHttpsCallable("migrateUserData").CallAsync(data).ContinueWithOnMainThread(task =>
    //                    {
    //                        if (task.IsCompletedSuccessfully)
    //                        {
    //                            DataManager.Instance.RefreshUserData();
    //                            UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AuthenticationSuccess"));
    //                            UIManager.Instance.Get<PopupSettings>().Refresh();
    //                        }
    //                        else
    //                        {
    //                            UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AuthenticationFail"));
    //                        }
    //                    });
    //                }
    //                else
    //                {
    //                    UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AuthenticationFail"));
    //                }
    //            });
    //        }
    //    });
    //}

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

                    // 👉 서버에 identityToken 전송해서 검증 가능
                    // 👉 또는 Firebase Auth 연동
                }
            },
            error =>
            {
                Debug.LogError($"Apple SignIn 실패: {error}");
            }
        );
#endif
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
        //var updates = new Dictionary<string, object>
        //{
        //    [$"{KEY.NICKNAME}/{nickname}"] = DataManager.Instance.userData.id,
        //    [$"{KEY.USER}/{DataManager.Instance.userData.id}/nickname"] = nickname
        //};
        //updates[$"{KEY.NICKNAME}/{DataManager.Instance.userData.nickname}"] = null;

        //db.UpdateChildrenAsync(updates).ContinueWithOnMainThread(updateTask =>
        //{
        //    ResultCheckNickname result = default;
        //    if (updateTask.IsFaulted)
        //    {
        //        Debug.LogError("닉네임 변경 실패: " + updateTask.Exception);
        //        result.success = false;
        //        result.message = updateTask.Exception.Message;
        //        callback?.Invoke(result);
        //    }
        //    else
        //    {
        //        Debug.Log("닉네임 변경 성공!");
        //        result.success = true;
        //        result.message = TextManager.Get("nickname_ok");
        //        DataManager.Instance.userData.nickname = nickname;
        //        callback?.Invoke(result);
        //    }
        //});

        functions.GetHttpsCallable("changeNickname").CallAsync(new Dictionary<string, object> { { "nickname", nickname } }).ContinueWithOnMainThread(task =>
        {
            ResultCheckNickname result = default;
            if (task.IsFaulted)
            {
                Debug.LogError("닉네임 변경 실패: " + task.Exception);
                result.success = false;
                result.message = task.Exception.Message;
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


//    public void SubmitScoreLevel(int exp, Action<int> callback = null)
//    {
//        RankingList.LevelData entry = new RankingList.LevelData(DataManager.Instance.userData.id, DataManager.Instance.userData.level, DataManager.Instance.userData.nickname, exp, DataManager.Instance.userData.countryCode);

//        db.Child(KEY.RANKING).Child("Level").Child(DataManager.Instance.userData.id).SetRawJsonValueAsync(JsonConvert.SerializeObject(entry));
//        callback?.Invoke(0);
////#if UNITY_EDITOR

////#else
////        var data = new Dictionary<string, object>
////        {
////            { "gameLevel", gameLevel.ToString() },
////            { "date", date },
////            { "id", entry.id },
////            { "level", entry.level },
////            { "name", entry.name },
////            { "point", entry.point },
////            { "remainMilliSeconds", entry.remainMilliSeconds },
////            { "timeStamp", entry.timeStamp },
////            { "countryCode", entry.countryCode }
////        };

////        functions.GetHttpsCallable("SubmitScore").CallAsync(data).ContinueWith(task =>
////        {
////            if (task.IsFaulted)
////            {
////                Debug.LogError("랭킹 등록 실패: " + task.Exception);
////                return;
////            }

////            var result = task.Result.Data as Dictionary<string, object>;
////            int myRank = Convert.ToInt32(result["myRank"]);
////            callback?.Invoke(myRank);
////        });
////#endif


//    }

    public void SubmitScore(PuzzleManager.Level gameLevel, string date, int score, Action<int> callback = null)
    {
        Debug.Log($"SubmitScore  {gameLevel} | {date} | {score}");
        Debug.Log($"userData : {DataManager.Instance.userData}");
        RankingList.Data entry = new RankingList.Data(DataManager.Instance.userData.id, DataManager.Instance.userData.level, DataManager.Instance.userData.nickname, score, DataManager.Instance.userData.countryCode);
        Debug.Log($"entry : {entry}");
        db.Child(KEY.RANKING).Child(gameLevel.ToString()).Child(date).Child(DataManager.Instance.userData.id).SetRawJsonValueAsync(JsonConvert.SerializeObject(entry));
        callback?.Invoke(0);
    }

    public void SubmitScore(PuzzleManager.Level gameLevel, string date, string userId, string nickname, int point, string countryCode, Action<int> callback = null)
    {
        var data = new Dictionary<string, object>
        {
            { "gameLevel", gameLevel.ToString() },
            { "date", date },
            { "userId", userId },
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
        //#if UNITY_EDITOR
        //db.Child("Leaderboard").Child(gameLevel.ToString()).Child(date).GetValueAsync().ContinueWithOnMainThread(task =>
        //{
        //    if (task.IsCompletedSuccessfully)
        //    {
        //        DataSnapshot dataSnapshot = task.Result;
        //        if (dataSnapshot.Exists)
        //        {
        //            PopupRanking.RankingListWithMyRank resultData = new PopupRanking.RankingListWithMyRank();
        //            resultData.topRanks = new List<RankingList.PointData>();
        //            foreach (var user in dataSnapshot.Children)
        //            {
        //                string id = user.Key;
        //                var json = user.Value as Dictionary<string, object>;
        //                RankingList.PointData entry = new RankingList.PointData(id,
        //                    json.ContainsKey("rank") ? Convert.ToInt32(json["rank"].ToString()) : 0,
        //                    json.ContainsKey("level") ? Convert.ToInt32(json["level"].ToString()) : 0,
        //                    json.ContainsKey("name") ? json["name"].ToString() : "Unknown",
        //                    json.ContainsKey("point") ? Convert.ToInt32(json["point"]) : 0,
        //                    json.ContainsKey("countryCode") ? json["countryCode"].ToString() : "??",
        //                    json.ContainsKey("timeStamp") ? Convert.ToInt32(json["timeStamp"].ToString()) : 0);

        //                resultData.topRanks.Add(entry);
        //            }
        //            resultData.myRank = resultData.topRanks.SingleOrDefault(x => x.id == userId);
        //            resultData.topRanks = resultData.topRanks.OrderBy(x => x.rank == 0 ? int.MaxValue : x.rank).ToList();
        //            // 랭킹 포인트 순으로 정렬
        //            //rankingList.Sort((a, b) => b.point.CompareTo(a.point));

        //            callback?.Invoke(resultData);
        //        }
        //        else
        //        {
        //            callback?.Invoke(null);
        //        }
        //    }
        //    else
        //    {
        //        callback?.Invoke(null);
        //    }
        //});

        //return;
        //#endif
        var data = new Dictionary<string, object>
        {
            { "gameLevel", gameLevel.ToString() },
            { "date", date },
            { "userId", userId },
            { "limit", limit }
        };
        Debug.Log($"GetRankingFromServer {gameLevel} | {date} | {userId} | {limit}");
        functions.GetHttpsCallable("GetRanking").CallAsync(data).ContinueWithOnMainThread(task =>
        {
            Debug.Log($"GetRanking callback | {task.IsCompletedSuccessfully}");
            if (task.IsCompletedSuccessfully)
            {
                try
                {
                    var result = task.Result.Data as Dictionary<string, object>;

                    // Top 랭킹 파싱
                    var topRankings = result["topRankings"] as List<object>;
                    Debug.Log("=== 전체 랭킹 ===");
                    PopupRanking.RankingListWithMyRank resultData = new PopupRanking.RankingListWithMyRank();
                    resultData.topRanks = new List<RankingList.Data>();

                    for (int i = 0; i < topRankings.Count; i++)
                    {
                        var entry = topRankings[i] as Dictionary<string, object>;
                        RankingList.Data data = JsonConvert.DeserializeObject<RankingList.Data>(JsonConvert.SerializeObject(entry));
                        resultData.topRanks.Add(data);
                    }

                    resultData.topRanks = resultData.topRanks.OrderBy(x => x.rank == 0 ? int.MaxValue : x.rank).ToList();
                    // 내 랭킹 파싱
                    int myRank = Convert.ToInt32(result["myRank"]);
                    if (myRank > 0)
                    {
                        var myEntry = result["myEntry"] as Dictionary<string, object>;
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
                //foreach (var e in task.Exception.Flatten().InnerExceptions)
                //{
                //    Debug.LogError($"Function call error: {e.Message}");
                //}
                Debug.LogError("랭킹 가져오기 실패: " + task.Exception);
                callback?.Invoke(null);
            }
            
        });
    }
    
    //[Serializable]
    //private class GoogleReceipt
    //{
    //    public string Store;
    //    public string TransactionID;
    //    public string Payload;
    //}

    //[Serializable]
    //private class PayloadJson
    //{
    //    public string json;
    //    public string signature;
    //    public string skuDetails;
    //}

    //public class PayloadData
    //{
    //    public string orderId;
    //    public string packageName;
    //    public string productId;
    //    public long purchaseTime;
    //    public int purchaseState;
    //    public string purchaseToken;
    //}

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
        //public long priceAmountMicros;
        //public string priceCurrencyCode;
        public System.DateTime orderDate;
        public string receipt;
        public string signature;
        public string json;
        public override string ToString()
        {
            //return base.ToString();
            return "orderId : " + orderId + "\n"
                + "packageName : " + packageName + "\n"
                + "productId : " + productId + "\n"
                + "purchaseToken : " + purchaseToken;


        }
    }

    private class PurchaseData
    {
        public string productId;
        public string purchaseToken;
    }

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

    private void Update()
    {
#if UNITY_IOS
        _appleAuthManager?.Update();
#endif
    }
}
