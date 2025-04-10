using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;

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

    public UserData userData;

    public void LoadGameDatas()
    {
        gameDatas.Clear();
        StartCoroutine(LoadAllGameDatas());
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

        MaxHeart = Get<GameData.Config>().SingleOrDefault(x => x.key == "maxHeart").val;
        HeartChargeTime = Get<GameData.Config>().SingleOrDefault(x => x.key == "heartChargeTime").val;

        FirebaseManager.Instance.GetUserData((UserData userData) =>
        {
            this.userData = userData;
            HUD.Instance.UpdateUserData(userData);
        });

        yield return new WaitUntil(() => userData != null);
        TextManager.LoadDatas(userData.countryCode, Get<GameData.Language>());
    }

    public void UpdateUserData(UserData data)
    {
        this.userData = data;
        HUD.Instance.UpdateUserData(userData);
    }

    //private IEnumerator GetGameDatas()
    //{
    //    Debug.Log("GetGameDatas");
    //    yield return GetGameData<GameData.Config>(result =>
    //    {
    //        config = result;
    //        MaxHeart = config.SingleOrDefault(x => x.key == "maxHeart").val;
    //        HeartChargeTime = config.SingleOrDefault(x => x.key == "heartChargeTime").val;
    //    });
    //    yield return GetGameData<GameData.Language>(result =>
    //    {
    //        language = result;
    //    });
    //    yield return GetGameData<GameData.GameLevel>(result =>
    //    {
    //        gameLevel = result;
    //    });
        
    //    yield return GetGameData<GameData.UserLevel>(result =>
    //    {
    //        userLevel = result;
    //    });

    //    yield return GetGameData<GameData.ForbiddenWord>(result =>
    //    {
    //        forbiddenWord = result;
    //    });

    //    FirebaseManager.Instance.GetUserData((UserData userData) =>
    //    {
    //        this.userData = userData;
    //    });

    //    yield return new WaitUntil(() => userData != null);
    //    TextManager.LoadDatas(userData.countryCode, language);
    //}

    //private IEnumerator GetGameData<T>(Action<T[]> callback) where T : GameData.Data
    //{
    //    GameData.Data[] wait = null;
    //    FirebaseManager.Instance.GetGameData<T>(typeof(T).Name, result =>
    //    {
    //        wait = result;
    //        callback.Invoke(result);
    //    });

    //    yield return new WaitUntil(() => wait != null);
    //}

    //public void RefreshUserData()
    //{
    //    UIManager.Instance.Loading("Load User Data");
    //    FirebaseManager.Instance.GetUserData((UserData userData) =>
    //    {
    //        this.userData = userData;
    //        UIManager.Instance.CloseLoading();
    //    });
    //}
}