using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class DataManager : Singleton<DataManager>
{
    public bool IsLoadComplete
    {
        get
        {
            return language != null && config != null && userData != null;
        }
    }

    
    public GameData.Config[] config;
    public GameData.GameLevel[] gameLevel;
    public GameData.UserLevel[] userLevel;
    public GameData.ForbiddenWord[] forbiddenWord;
    public GameData.Language[] language;

    public int MaxHeart;
    public int HeartChargeTime;

    public UserData userData;

    public void LoadGameDatas()
    {
        language = null;
        config = null;
        gameLevel = null;
        userLevel = null;
        userData = null;
        StartCoroutine(GetGameDatas());
    }

    private IEnumerator GetGameDatas()
    {
        Debug.Log("GetGameDatas");
        yield return GetGameData<GameData.Config>(result =>
        {
            config = result;
        });
        yield return GetGameData<GameData.Language>(result =>
        {
            language = result;
        });
        yield return GetGameData<GameData.GameLevel>(result =>
        {
            gameLevel = result;
        });
        
        yield return GetGameData<GameData.UserLevel>(result =>
        {
            userLevel = result;
        });
        
        FirebaseManager.Instance.GetUserData((UserData userData) =>
        {
            this.userData = userData;
        });

        yield return new WaitUntil(() => userData != null);
        TextManager.LoadDatas(userData.countryCode, language);
    }

    private IEnumerator GetGameData<T>(Action<T> callback) where T : GameData.Data
    {
        GameData.Data wait = null;
        FirebaseManager.Instance.GetGameData<T>(typeof(T).Name, result =>
        {
            wait = result;
            callback.Invoke(result);
        });

        yield return new WaitUntil(() => wait != null);
    }

    private IEnumerator GetGameData<T>(Action<T[]> callback) where T : GameData.Data
    {
        GameData.Data[] wait = null;
        FirebaseManager.Instance.GetGameData<T>(typeof(T).Name, result =>
        {
            wait = result;
            callback.Invoke(result);
        });

        yield return new WaitUntil(() => wait != null);
    }

    public void RefreshUserData()
    {
        UIManager.Instance.Loading("Load User Data");
        FirebaseManager.Instance.GetUserData((UserData userData) =>
        {
            this.userData = userData;
            UIManager.Instance.CloseLoading();
        });
    }
}