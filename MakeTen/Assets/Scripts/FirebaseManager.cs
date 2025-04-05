using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;

public class FirebaseManager : Singleton<FirebaseManager>
{
    /// <summary>
    //leaderboard: {
    //    user1: { name: "Alice", score: 123 },
    //    user2: { name: "Bob", score: 400 },
    //    user3: { name: "Carol", score: 250 }
    //}

    /// </summary>
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase Ready");
            }
            else
            {
                Debug.LogError("Firebase 초기화 실패: " + task.Result);
            }
        });
    }

    public void SubmitScore(string userId, string userName, int score)
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        LeaderboardEntry entry = new LeaderboardEntry(userName, score);
        string json = JsonUtility.ToJson(entry);

        reference.Child("leaderboard").Child(userId).SetRawJsonValueAsync(json);
    }

    [System.Serializable]
    public class LeaderboardEntry
    {
        public string name;
        public int score;

        public LeaderboardEntry(string name, int score)
        {
            this.name = name;
            this.score = score;
        }
    }

    public void GetTopScores(int limit = 10)
    {
        FirebaseDatabase.DefaultInstance.GetReference("leaderboard")
            .OrderByChild("score")
            .LimitToLast(limit) // score가 높은 순
            .GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    List<LeaderboardEntry> topEntries = new List<LeaderboardEntry>();

                    foreach (DataSnapshot child in snapshot.Children)
                    {
                        string name = child.Child("name").Value.ToString();
                        int score = int.Parse(child.Child("score").Value.ToString());
                        topEntries.Add(new LeaderboardEntry(name, score));
                    }

                // 낮은 점수부터 오므로 뒤집기
                topEntries.Reverse();

                    foreach (var entry in topEntries)
                    {
                        Debug.Log($"🏆 {entry.name} - {entry.score}");
                    }
                }
            });
    }
}
