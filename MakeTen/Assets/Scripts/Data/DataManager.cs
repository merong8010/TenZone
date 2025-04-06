using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class DataManager : Singleton<DataManager>
{
    public UserData userData;

    public void LoadUserData()
    {
        FirebaseManager.Instance.GetUserData(SystemInfo.deviceUniqueIdentifier, (UserData userData) =>
        {
            this.userData = userData;
        });
    }
    [SerializeField]
    private FlagFetcher flagFetcher;
    private Dictionary<string, Sprite> flagsDic = new Dictionary<string, Sprite>();
    
    public void GetFlags(string countryCode, Action<Sprite> callback)
    {
        if(flagsDic.ContainsKey(countryCode))
        {
            if (flagsDic[countryCode] != null)
                callback.Invoke(flagsDic[countryCode]);
        }
        else
        {
            flagsDic.Add(countryCode, null);
            flagFetcher.GetFlag(countryCode, flagSprite =>
            {
                UIManager.Instance.Get<PopupRanking>().UpdateFlags(countryCode, flagSprite);
                callback.Invoke(flagSprite);
            });
        }
    }
}