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
            Debug.Log(userData.Heart);
        });
    }
    
}