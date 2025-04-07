using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Globalization;
using System;
using Firebase.Auth;
public class FirebaseManager : Singleton<FirebaseManager>
{
    /// <summary>
    //leaderboard: {
    //    user1: { name: "Alice", score: 123 },
    //    user2: { name: "Bob", score: 400 },
    //    user3: { name: "Carol", score: 250 }
    //}

    public bool IsReady => reference != null;
    private DatabaseReference reference;
    private FirebaseAuth auth;
    private FirebaseUser user;
    /// </summary>
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase Ready");
                FirebaseApp app = FirebaseApp.DefaultInstance;
                reference = FirebaseDatabase.DefaultInstance.RootReference;
                auth = FirebaseAuth.DefaultInstance;

                user = auth.CurrentUser;
            }
            else
            {
                Debug.LogError("Firebase 초기화 실패: " + task.Result);
            }
        });
    }

    public void SubmitScore(int score)
    {
        //DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        RankingList.Data entry = new RankingList.Data(DataManager.Instance.userData.id, score, DataManager.Instance.userData.countryCode);
        
        reference.Child("leaderboard").Child(DataManager.Instance.userData.id).SetRawJsonValueAsync(JsonConvert.SerializeObject(entry));
    }

    public void TestSubmitScore(string userId, int score, string countryCode)
    {
        RankingList.Data entry = new RankingList.Data(userId, score, countryCode);

        reference.Child("leaderboard").Child(userId).SetRawJsonValueAsync(JsonConvert.SerializeObject(entry));
    }

    public void GetTopScores(int limit = 10, Action<List<RankingList.Data>> callback = null)
    {
        FirebaseDatabase.DefaultInstance.GetReference("leaderboard")
            .OrderByChild("score")
            .LimitToLast(limit) // score가 높은 순
            .GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    List<RankingList.Data> topEntries = new List<RankingList.Data>();
                    foreach (DataSnapshot child in snapshot.Children)
                    {
                        string name = child.Child("name").Value.ToString();
                        int score = int.Parse(child.Child("score").Value.ToString());
                        string countryCode = child.Child("countryCode").Value.ToString();
                        
                        topEntries.Add(new RankingList.Data(name, score, countryCode));
                    }

                // 낮은 점수부터 오므로 뒤집기
                    topEntries.Reverse();
                    for(int i = 0; i < topEntries.Count; i++)
                    {
                        topEntries[i].rank = i + 1;
                    }
                    //foreach (var entry in topEntries)
                    //{
                    //    Debug.Log($"🏆 {entry.name} - {entry.score}");
                    //}
                    if (callback != null) callback.Invoke(topEntries);
                }
                

            });
    }

    public void GetGameData<T>(string nodeName, Action<T> callback) where T : GameData.Data
    {
        reference.Child(nodeName).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Debug.Log($"game Info {nodeName} : {snapshot.GetRawJsonValue()}");
                    callback.Invoke(JsonConvert.DeserializeObject<T>(snapshot.GetRawJsonValue()));
                }
                else
                {
                    //callback.Invoke(new UserData(userId));
                    Debug.Log("No gameInfo found.");
                }
            }
            else
            {
                Debug.LogError($"Failed to get {nodeName} : {task.Exception}");
            }
        });
    }

    public void SaveUserData(UserData data)
    {
        string json = JsonConvert.SerializeObject(data);
        reference.Child("users").Child(data.id).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
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
        reference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task => {
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
        reference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task => {
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
        reference.Child("users").Child(oldKey).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;
                object data = snapshot.Value;

                // 새 키에 데이터 저장
                reference.Child("users").Child(newKey).SetValueAsync(data).ContinueWithOnMainThread(setTask =>
                {
                    if (setTask.IsCompleted)
                    {
                        // 기존 키 삭제
                        reference.Child("users").Child(oldKey).RemoveValueAsync().ContinueWithOnMainThread(removeTask =>
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
        reference.Child("users").Child(userKey).RemoveValueAsync().ContinueWithOnMainThread(task =>
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
}
