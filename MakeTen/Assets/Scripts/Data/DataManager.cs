using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;
using System.Globalization;

public class DataManager : Singleton<DataManager>
{
    public bool IsLoadComplete
    {
        get
        {
            return userData != null;
        }
    }

    private Dictionary<string, GameData.Data[]> gameDatas = new Dictionary<string, GameData.Data[]>();

    public T[] Get<T>() where T: GameData.Data
    {
        if(gameDatas.ContainsKey(typeof(T).Name))
        {
            return (T[])gameDatas[typeof(T).Name];
        }
        return null;
    }

    public int MaxHeart { get; private set; }
    public int HeartChargeTime { get; private set; }
    public int SearchTerm;
    public int ExplodeTerm;

    public const string ConfigKey_MaxHeart = "maxHeart";
    public const string ConfigKey_HeartChargeTime = "heartChargeTime";
    public const string ConfigKey_SearchTerm = "searchTerm";
    public const string ConfigKey_ExplodeTerm = "explodeTerm";

    public UserData userData;

    public void LoadGameDatas()
    {
        gameDatas.Clear();
        StartCoroutine(LoadAllGameDatas());
    }

    public int GetConfig(string key)
    {
        GameData.Config val = Get<GameData.Config>()?.SingleOrDefault(x => x.key == key);
        if (val == null)
        {
            Debug.LogWarning($"[DataManager] Config key not found: '{key}'. Returning default value 0.");
            return 0;
        }

        return val.Get();
    }

    public GameData.GoodsType GetConfigGoodsType(string key)
    {
        return Get<GameData.Config>().SingleOrDefault(x => x.key == key).GetGoodsType();
    }

    private IEnumerator LoadAllGameDatas()
    {
        int dataTotalCount = 100;

        if(GameManager.Instance.isOffline)
        {
            var gameData = JsonConvert.DeserializeObject<Firebase.Database.DataSnapshot>(Resources.Load<TextAsset>("GameData.json").ToString());
            dataTotalCount = (int)gameData.ChildrenCount;
            foreach (var data in gameData.Children)
            {
                Type type = Type.GetType($"GameData.{data.Key}").MakeArrayType();
                gameDatas.Add(data.Key, (GameData.Data[])JsonConvert.DeserializeObject(data.GetRawJsonValue(), type));
            }
        }
        else
        {
            FirebaseManager.Instance.LoadAllGameDatas(result =>
            {
                dataTotalCount = (int)result.ChildrenCount;
                foreach (var data in result.Children)
                {
                    Type type = Type.GetType($"GameData.{data.Key}").MakeArrayType();
                    try
                    {
                        gameDatas.Add(data.Key, (GameData.Data[])JsonConvert.DeserializeObject(data.GetRawJsonValue(), type));
                    }
                    catch(System.Exception exception)
                    {
                        Debug.LogError($"load exception : {exception} \n {type} | {data.GetRawJsonValue()}");
                    }

                    TitleManager.Instance.SetStatus($"Loaded {data.Key}");
                }
            });
        }
        
        yield return new WaitUntil(() => gameDatas.Count == dataTotalCount);

        TitleManager.Instance.SetStatus($"Loaded All GameDatas");
        string countryCode = PlayerPrefs.GetString("Locale", Util.GetCountryCode());
        TextManager.LoadDatas(countryCode, Get<GameData.Language>());

        MaxHeart = GetConfig(ConfigKey_MaxHeart);
        HeartChargeTime = GetConfig(ConfigKey_HeartChargeTime);
        SearchTerm = GetConfig(ConfigKey_SearchTerm);
        ExplodeTerm = GetConfig(ConfigKey_ExplodeTerm);

        RefreshUserData();

        yield return new WaitUntil(() => userData != null);
    }

    public void UpdateUserData(UserData data)
    {
        if (userData == null) userData = data;
        else userData.Copy(data);

        if(MainManager.HasInstance)
        {
            MainManager.Instance.UpdateUserData(userData);
        }
        //HUD.Instance.UpdateUserData(userData);
    }

    public void RefreshUserData()
    {
        if(GameManager.Instance.isOffline)
        {
            string userDataJson = PlayerPrefs.GetString("OfflineUserData", string.Empty);

            if (string.IsNullOrEmpty(userDataJson))
            {
                // 저장된 데이터가 없으면 새로운 기본 유저 데이터 생성
                UpdateUserData(new UserData());
            }
            else
            {
                // 저장된 데이터가 있으면 불러오기
                UserData offlineData = JsonConvert.DeserializeObject<UserData>(userDataJson);
                UpdateUserData(offlineData);
            }
        }
        else
        {
            FirebaseManager.Instance.GetUserData(userData =>
            {
                UpdateUserData(userData);
            });
        }
    }
}