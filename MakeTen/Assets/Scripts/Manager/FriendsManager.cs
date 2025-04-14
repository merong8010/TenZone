using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FriendManager : MonoBehaviour
{
    private DatabaseReference dbRef;
    private FirebaseAuth auth;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            dbRef = FirebaseDatabase.DefaultInstance.RootReference;
            auth = FirebaseAuth.DefaultInstance;
        });
    }

    /// <summary>
    /// 친구 요청 보내기
    /// </summary>
    public void SendFriendRequest(string targetUserId)
    {
        string myUid = auth.CurrentUser.UserId;
        dbRef.Child("users").Child(targetUserId).Child("friendRequests").Child(myUid).SetValueAsync("pending");
        Debug.Log($"친구 요청 전송됨 → {targetUserId}");
    }

    /// <summary>
    /// 친구 요청 수락
    /// </summary>
    public async Task AcceptFriendRequest(string requesterUid)
    {
        string myUid = auth.CurrentUser.UserId;

        // 1. 친구로 등록
        await dbRef.Child("users").Child(myUid).Child("friends").Child(requesterUid).SetValueAsync(true);
        await dbRef.Child("users").Child(requesterUid).Child("friends").Child(myUid).SetValueAsync(true);

        // 2. 요청 제거
        await dbRef.Child("users").Child(myUid).Child("friendRequests").Child(requesterUid).RemoveValueAsync();

        Debug.Log($"친구 요청 수락 완료 → {requesterUid}");
    }

    /// <summary>
    /// 친구 목록 가져오기
    /// </summary>
    public async Task<List<string>> GetFriendList()
    {
        string myUid = auth.CurrentUser.UserId;
        List<string> friendIds = new List<string>();

        var snapshot = await dbRef.Child("users").Child(myUid).Child("friends").GetValueAsync();
        if (snapshot.Exists)
        {
            foreach (var child in snapshot.Children)
            {
                friendIds.Add(child.Key);
            }
        }

        return friendIds;
    }

    /// <summary>
    /// 받은 친구 요청 리스트 보기
    /// </summary>
    public async Task<List<string>> GetIncomingFriendRequests()
    {
        string myUid = auth.CurrentUser.UserId;
        List<string> requests = new List<string>();

        var snapshot = await dbRef.Child("users").Child(myUid).Child("friendRequests").GetValueAsync();
        if (snapshot.Exists)
        {
            foreach (var child in snapshot.Children)
            {
                if (child.Value.ToString() == "pending")
                {
                    requests.Add(child.Key);
                }
            }
        }

        return requests;
    }
}
