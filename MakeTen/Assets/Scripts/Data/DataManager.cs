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

    public GameData.Language language;
    public GameData.Config config;
    public UserData userData;

    public void LoadGameDatas()
    {
        language = null;
        config = null;
        userData = null;
        StartCoroutine(GetGameDatas());
    }

    private IEnumerator GetGameDatas()
    {
        bool isLoad = false;
        FirebaseManager.Instance.GetGameData<GameData.Config>("Config", configData =>
        {
            isLoad = true;
            config = configData;
        });

        yield return new WaitUntil(() => isLoad);

        isLoad = false;
        FirebaseManager.Instance.GetGameData<GameData.Language>("Language", language =>
        {
            isLoad = true;
            this.language = language;
        });

        yield return new WaitUntil(() => isLoad);

        isLoad = false;
        FirebaseManager.Instance.GetUserData((UserData userData) =>
        {
            isLoad = true;
            this.userData = userData;
        });

        yield return new WaitUntil(() => isLoad);
        TextManager.LoadDatas(userData.countryCode, language);
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