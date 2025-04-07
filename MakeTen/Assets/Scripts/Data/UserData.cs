using System.Globalization;
using UnityEngine;

public class UserData
{
    //public const int MaxHeart = 5;
    //public const int HeartChargeTime = 600;

    public FirebaseManager.AuthenticatedType authType;
    public string id;
    public string nickname;
    private int heart;
    public long nextHeartChargeTime;
    public string countryCode;

    public int Heart
    {
        get
        {
            if(heart < DataManager.Instance.config.MaxHeart)
            {
                if(GameManager.Instance.dateTime !=null)
                {
                    long currentTime = GameManager.Instance.dateTime.Value.ToTick();
                    if (nextHeartChargeTime < currentTime)
                    {

                        heart += 1;
                        heart += (int)((currentTime - nextHeartChargeTime) / DataManager.Instance.config.HeartChargeTime);
                        if(heart >= DataManager.Instance.config.MaxHeart)
                        {
                            heart = DataManager.Instance.config.MaxHeart;
                            nextHeartChargeTime = currentTime;
                        }
                        else
                        {
                            nextHeartChargeTime = currentTime + ((currentTime - nextHeartChargeTime) % DataManager.Instance.config.HeartChargeTime);
                        }
                    }
                }
            }
            
            return heart;
        }
        set
        {
            heart = value;
        }
    }

    public UserData()
    {

    }

    public UserData(string userId)
    {
        id = userId;

        //string country = RegionInfo.CurrentRegion.EnglishName; // ???? ????
        countryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
        heart = DataManager.Instance.config.MaxHeart;
        nextHeartChargeTime = GameManager.Instance.dateTime.Value.ToTick();
    }


    public bool UseHeart()
    {
        Debug.Log($"UseHeart | {GameManager.Instance.dateTime} | {Heart}");
        if (GameManager.Instance.dateTime == null) return false;
        if(Heart > 0)
        {
            heart -= 1;
            if (heart < DataManager.Instance.config.MaxHeart && nextHeartChargeTime <= GameManager.Instance.dateTime.Value.ToTick())
            {
                nextHeartChargeTime = GameManager.Instance.dateTime.Value.ToTick() + DataManager.Instance.config.HeartChargeTime;
            }
            //
            FirebaseManager.Instance.SaveUserData(this);
            HUD.Instance.UpdateHeart();
            return true;
        }
        return false;
    }

    public void ChargeHeart()
    {
        heart += 1;
        if(heart >= DataManager.Instance.config.MaxHeart)
        {
            nextHeartChargeTime = GameManager.Instance.dateTime.Value.ToTick();
        }
        HUD.Instance.UpdateHeart();
        FirebaseManager.Instance.SaveUserData(this);
    }

    public void UpdateData(string userId, FirebaseManager.AuthenticatedType authType)
    {
        id = userId;
        this.authType = authType;

        FirebaseManager.Instance.SaveUserData(this);
    }
}
