using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Functions;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Globalization;
using System;
using Firebase.Auth;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;


public class FirebaseManager : Singleton<FirebaseManager>
{
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
                functions = FirebaseFunctions.DefaultInstance;
                user = auth.CurrentUser;
            }
            else
            {
                Debug.LogError("Firebase 초기화 실패: " + task.Result);
            }
        });
    }

    public void SubmitScore(PuzzleManager.Level gameLevel, string date, int score, int milliseconds)
    {
        RankingList.Data entry = new RankingList.Data(DataManager.Instance.userData.id, DataManager.Instance.userData.nickname, score, milliseconds, DataManager.Instance.userData.countryCode);
        db.Child(KEY.RANKING).Child(gameLevel.ToString()).Child(date).Child(DataManager.Instance.userData.id).SetRawJsonValueAsync(JsonConvert.SerializeObject(entry));
    }

    public void TestSubmitScore(PuzzleManager.Level gameLevel, string date, string userId, string nickname, int score, int milliSeconds, string countryCode)
    {
        RankingList.Data entry = new RankingList.Data(userId, nickname, score, milliSeconds, countryCode);
        db.Child(KEY.RANKING).Child(gameLevel.ToString()).Child(date).Child(userId).SetRawJsonValueAsync(JsonConvert.SerializeObject(entry));
    }

    //public void GetTopScores(int limit = 10, Action<List<RankingList.Data>> callback = null)
    //{
    //    db.Child("leaderboard")
    //        .OrderByChild("point")
    //        .LimitToLast(limit) // score가 높은 순
    //        .GetValueAsync().ContinueWithOnMainThread(task => {
    //            if (task.IsCompleted)
    //            {
    //                DataSnapshot snapshot = task.Result;
    //                List<RankingList.Data> topEntries = new List<RankingList.Data>();
    //                foreach (DataSnapshot child in snapshot.Children)
    //                {
    //                    string id = child.Child("id").Value.ToString();
    //                    string name = child.Child("name").Value.ToString();
    //                    int point = int.Parse(child.Child("point").Value.ToString());
    //                    int remain = int.Parse(child.Child("remainMilliSeconds").Value.ToString());
    //                    string countryCode = child.Child("countryCode").Value.ToString();

    //                    topEntries.Add(new RankingList.Data(id, name, point, remain, countryCode));
    //                }

    //            // 낮은 점수부터 오므로 뒤집기
    //                topEntries.Reverse();
    //                for(int i = 0; i < topEntries.Count; i++)
    //                {
    //                    topEntries[i].rank = i + 1;
    //                }
    //                //foreach (var entry in topEntries)
    //                //{
    //                //    Debug.Log($"🏆 {entry.name} - {entry.score}");
    //                //}
    //                if (callback != null) callback.Invoke(topEntries);
    //            }


    //        });
    //}

    public void GetGameData<T>(string nodeName, Action<T> callback) where T : GameData.Data
    {
        db.Child("sheetData").Child(nodeName).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Debug.Log($"game Info {nodeName} : {snapshot.GetRawJsonValue()}");
                    callback.Invoke(JsonConvert.DeserializeObject<T>(snapshot.GetRawJsonValue()));
                    //callback.Invoke(JsonConvert.DeserializeObject<T>(snapshot.GetRawJsonValue()));
                }
                else
                {
                    //callback.Invoke(new UserData(userId));
                    Debug.Log($"No gameInfo found. {typeof(T).Name}");
                }
            }
            else
            {
                Debug.LogError($"Failed to get {nodeName} : {task.Exception}");
            }
        });
    }

    public void GetGameData<T>(string nodeName, Action<T[]> callback) where T : GameData.Data
    {
        db.Child("sheetData").Child(nodeName).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Debug.Log($"game Info {nodeName} : {snapshot.GetRawJsonValue()}");
                    callback.Invoke(JsonConvert.DeserializeObject<T[]>(snapshot.GetRawJsonValue()));
                    //callback.Invoke(JsonConvert.DeserializeObject<T>(snapshot.GetRawJsonValue()));
                }
                else
                {
                    //callback.Invoke(new UserData(userId));
                    Debug.Log($"No gameInfo found. {typeof(T).Name}");
                }
            }
            else
            {
                Debug.LogError($"Failed to get {nodeName} : {task.Exception}");
            }
        });
        //db.Child(nodeName).GetValueAsync().ContinueWithOnMainThread(task => {
        //    if (task.IsCompleted)
        //    {
        //        DataSnapshot snapshot = task.Result;
        //        if (snapshot.Exists)
        //        {
        //            Debug.Log($"game Info {nodeName} : {snapshot.GetRawJsonValue()}");
        //            callback.Invoke(JsonConvert.DeserializeObject<T>(snapshot.GetRawJsonValue()));
        //        }
        //        else
        //        {
        //            //callback.Invoke(new UserData(userId));
        //            Debug.Log("No gameInfo found.");
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError($"Failed to get {nodeName} : {task.Exception}");
        //    }
        //});
    }

    public void SaveUserData(UserData data)
    {
        string json = JsonConvert.SerializeObject(data);
        db.Child(KEY.USER).Child(data.id).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
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
            GetUserData(SystemInfo.deviceUniqueIdentifier, callback);
        }
        
    }

    public void GetUserData(string userId, Action<UserData> callback)
    {
        db.Child(KEY.USER).Child(userId).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    //string json = snapshot.GetRawJsonValue();
                    //User user = JsonUtility.FromJson<User>(json);
                    Debug.Log("User Info: " + snapshot.GetRawJsonValue());
                    callback.Invoke(JsonConvert.DeserializeObject<UserData>(snapshot.GetRawJsonValue()));
                }
                else
                {
                    callback.Invoke(new UserData(userId));
                    Debug.Log("No user found.");
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

    public void ChangeUserId(string oldKey, string newKey)
    {
        db.Child(KEY.USER).Child(oldKey).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;
                object data = snapshot.Value;

                // 새 키에 데이터 저장
                db.Child(KEY.USER).Child(newKey).SetValueAsync(data).ContinueWithOnMainThread(setTask =>
                {
                    if (setTask.IsCompleted)
                    {
                        // 기존 키 삭제
                        db.Child(KEY.USER).Child(oldKey).RemoveValueAsync().ContinueWithOnMainThread(removeTask =>
                        {
                            if (removeTask.IsCompleted)
                            {
                                Debug.Log("키값 변경 완료");
                            }
                            else
                            {
                                Debug.LogError("기존 키 삭제 실패: " + removeTask.Exception);
                            }
                        });
                    }
                    else
                    {
                        Debug.LogError("새 키 저장 실패: " + setTask.Exception);
                    }
                });
            }
            else
            {
                Debug.LogWarning("기존 키에 데이터가 없습니다.");
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
        TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin(true, GoogleLoginCallback);
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
                //Debug.Log("Google Login Success: " + auth.CurrentUser.Email);
                IsUserData(user.UserId, isUser =>
                {
                    if(isUser)
                    {
                        UIManager.Instance.Message.Show(Message.Type.Ask, TextManager.Get("ExistUserData"), callback: result =>
                        {
                            if(result)
                            {
                                user = authTask.Result;
                                DataManager.Instance.RefreshUserData();

                                UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("FederatedSuccess"));
                            }
                            else
                            {
                                auth.SignOut();
                            }
                            UIManager.Instance.Main.Refresh();
                        });
                    }
                    else
                    {
                        user = authTask.Result;
                        //ChangeUserId(SystemInfo.deviceUniqueIdentifier, user.UserId);
                        UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AuthenticationSuccess"));

                        DataManager.Instance.userData.UpdateData(user.UserId, AuthenticatedType.Google);
                        RemoveUserId(SystemInfo.deviceUniqueIdentifier);

                        UIManager.Instance.Main.Refresh();
                    }
                });
            }
        });
    }

    public void StartAppleLogin()
    {
        TheBackend.ToolKit.AppleLogin.Android.AppleLogin("com.thebackend.testapp.applelogin", out var error, true, token => {
            Debug.Log("토큰 : " + token);
            Debug.Log("토큰 발급이 완료되었습니다. 로그인이 가능합니다.");
            auth.SignInWithCredentialAsync(OAuthProvider.GetCredential("apple.com", token, null, null)).ContinueWith(authTask =>
            {
                if (authTask.IsCompleted && !authTask.IsFaulted)
                {
                    user = authTask.Result;
                }
            });


            // 경고! : 다음과 같이 동기 함수를 호출하지 마세요
            // var bro = Backend.BMember.AuthorizeFederation(token, FederationType.Apple);

            // 아래와 같이 비동기 함수를 호출해주세요,
            //Backend.BMember.AuthorizeFederation(token, FederationType.Apple, callback => {
            //    Debug.Log("애플 로그인 결과 : " + callback);
            //});
        });

        if (string.IsNullOrEmpty(error) == false)
        {
            Debug.Log("에러 : " + error);
        }
    }

    public void LogOut()
    {
        auth.SignOut();
        Application.Quit();
    }
    public enum AuthenticatedType
    {
        None,
        Google,
        Apple,
    }
    public AuthenticatedType AuthType
    {
        get
        {
            //if (user!=null)
            //{
            //    user.Metadata.
            //}
            return AuthenticatedType.None;
        }
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
        GameData.ForbiddenWord[] forbiddenWordTable = DataManager.Instance.forbiddenWord;
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

        db.UpdateChildrenAsync(updates).ContinueWith(updateTask =>
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


    public void GetRankingFromServer(string userId, Action<PopupRanking.RankingListWithMyRank> callback = null, string date = "ALL", int limit = 10, PuzzleManager.Level gameLevel = PuzzleManager.Level.Normal)
    {
        var data = new Dictionary<string, object>
        {
            { "gameLevel", gameLevel.ToString() },
            { "date", date },
            { "userId", userId },
            { "limit", limit }
        };

        functions.GetHttpsCallable($"GetRanking").CallAsync(data).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("랭킹 가져오기 실패: " + task.Exception);
                return;
            }

            var result = task.Result.Data as Dictionary<string, object>;

            // Top 랭킹 파싱
            var topRankings = result["topRankings"] as List<object>;
            Debug.Log("=== 전체 랭킹 ===");
            PopupRanking.RankingListWithMyRank resultData = new PopupRanking.RankingListWithMyRank();
            resultData.topRanks = new List<RankingList.Data>();

            for (int i = 0; i < topRankings.Count; i++)
            {
                var entry = topRankings[i] as Dictionary<string, object>;
                Debug.Log($"{i} | {entry.ToString()}");
                RankingList.Data data = JsonConvert.DeserializeObject<RankingList.Data>(entry.ToString());
                data.rank = i + 1;
                resultData.topRanks.Add(data);
                //Debug.Log($"{i + 1}등 - {entry["nickname"]} / 점수: {entry["score"]} / 클리어 시간: {entry["clearTime"]}");
            }

            // 내 랭킹 파싱
            int myRank = Convert.ToInt32(result["myRank"]);
            if (myRank > 0)
            {
                var myEntry = result["myEntry"] as Dictionary<string, object>;
                RankingList.Data data = JsonConvert.DeserializeObject<RankingList.Data>(myEntry.ToString());
                data.rank = myRank;
                resultData.myRank = data;
                Debug.Log($"=== 내 랭킹 ===\n내 순위: {myRank}등 / 닉네임: {myEntry["nickname"]} / 점수: {myEntry["score"]} / 클리어 시간: {myEntry["clearTime"]}");
            }
            else
            {
                Debug.Log("내 랭킹 정보가 없습니다.");
            }

            callback?.Invoke(resultData);
        });
    }


    
}
