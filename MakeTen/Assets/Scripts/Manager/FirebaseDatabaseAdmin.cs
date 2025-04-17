using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Functions;
using System.Collections.Generic;
using UniRx;
using System;
using System.Linq;
using System.Text;
using TMPro;

public class FirebaseDatabaseAdmin : MonoBehaviour
{
    private DatabaseReference dbRef;
    private FirebaseAuth auth;
    private FirebaseFunctions functions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if(task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                auth = FirebaseAuth.DefaultInstance;
                functions = FirebaseFunctions.DefaultInstance;
            }
        });
        //rewardDatas.ObserveAdd().Subscribe(x => UpdateRewards());
        //rewardDatas.ObserveReset().Subscribe(x => UpdateRewards());
        //rewardDatas.ObserveReplace().Subscribe(x => UpdateRewards());

        var enumNames = Enum.GetNames(typeof(GameData.GoodsType)).ToList();
        goodsDropDown.ClearOptions();
        goodsDropDown.AddOptions(enumNames);

        // 선택 시 이벤트 등록
        goodsDropDown.onValueChanged.AddListener(OnGoodsTypeChanged);
    }

    private void UpdateRewards()
    {
        StringBuilder sb = new StringBuilder();
        foreach(KeyValuePair<GameData.GoodsType, int> pair in rewardDatas)
        {
            if (sb.Length >= 0) sb.AppendLine();
            sb.Append(pair.Key.ToString()).Append(" : ").Append(pair.Value.ToString("n0"));
        }

        currentRewardsText.text = sb.ToString();
    }

    [SerializeField]
    private TMP_InputField idField;
    [SerializeField]
    private TMP_InputField titleField;
    [SerializeField]
    private TMP_InputField descField;
    private Dictionary<GameData.GoodsType, int> rewardDatas = new Dictionary<GameData.GoodsType, int>();
    [SerializeField]
    private TMP_Dropdown goodsDropDown;
    private GameData.GoodsType currentGoodsType;
    [SerializeField]
    private TMP_InputField goodsAmount;
    [SerializeField]
    private TextMeshProUGUI currentRewardsText;

    private void OnGoodsTypeChanged(int idx)
    {
        currentGoodsType = (GameData.GoodsType)idx;
    }

    public void ResetRewards()
    {
        rewardDatas.Clear();
        UpdateRewards();
    }

    public void AddRrewards()
    {
        if (currentGoodsType == GameData.GoodsType.None) return;
        if (!int.TryParse(goodsAmount.text, out int amount)) return;

        rewardDatas.Add(currentGoodsType, amount);
        UpdateRewards();
    }

    public void SendMail()
    {
        Debug.Log($"idField.text : {idField.text}");
        Dictionary<string, object>[] rewards = new Dictionary<string, object>[rewardDatas.Count];
        int idx = 0;
        foreach(KeyValuePair<GameData.GoodsType, int> pair in rewardDatas)
        {
            rewards[idx] = new Dictionary<string, object>(){
                    { "type", pair.Key.ToString() },
                    { "amount", pair.Value }
                };
            idx++;
        }
        var data = new Dictionary<string, object>
        {
            { "id", idField.text }, // 또는 닉네임으로 { "nickname", "testUser" }
            { "title", titleField.text },
            { "desc", descField.text },
            { "rewards", rewards }
        };

        functions
            .GetHttpsCallable("SendMail")
            .CallAsync(data)
            .ContinueWith(task => {
                if (task.IsFaulted)
                {
                    Debug.LogError("Function 호출 실패: " + task.Exception);
                }
                else
                {
                    Debug.Log("우편 전송 완료: " + task.Result.Data);
                }
            });
    }
    
}
