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

    public int MaxHeart;
    public int HeartChargeTime;
    public int SearchTerm;
    public int ExplodeTerm;

    public UserData userData;

    public void LoadGameDatas()
    {
        gameDatas.Clear();
        StartCoroutine(LoadAllGameDatas());
    }

    public int GetConfig(string key)
    {
        return Get<GameData.Config>().SingleOrDefault(x => x.key == key).Get();
    }

    public GameData.GoodsType GetConfigGoodsType(string key)
    {
        return Get<GameData.Config>().SingleOrDefault(x => x.key == key).GetGoodsType();
    }

    private IEnumerator LoadAllGameDatas()
    {
        int dataTotalCount = 100;
        FirebaseManager.Instance.LoadAllGameDatas(result =>
        {
            dataTotalCount = (int)result.ChildrenCount;
            
            foreach (var data in result.Children)
            {
                Type type = Type.GetType($"GameData.{data.Key}").MakeArrayType();
                gameDatas.Add(data.Key, (GameData.Data[])JsonConvert.DeserializeObject(data.GetRawJsonValue(),type));
            }
        });

        yield return new WaitUntil(() => gameDatas.Count == dataTotalCount);
        string countryCode = PlayerPrefs.GetString("Locale", RegionInfo.CurrentRegion.TwoLetterISORegionName);
        TextManager.LoadDatas(countryCode, Get<GameData.Language>());

        MaxHeart = GetConfig("maxHeart");
        HeartChargeTime = GetConfig("heartChargeTime");
        SearchTerm = GetConfig("searchTerm");
        ExplodeTerm = GetConfig("explodeTerm");

        FirebaseManager.Instance.GetUserData((UserData userData) =>
        {
            this.userData = userData;
            HUD.Instance.UpdateUserData(userData);
        });

        yield return new WaitUntil(() => userData != null);
    }

    public void UpdateUserData(UserData data)
    {
        this.userData = data;
        HUD.Instance.UpdateUserData(userData);
    }
}