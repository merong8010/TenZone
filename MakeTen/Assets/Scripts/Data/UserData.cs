using System.Globalization;
using UnityEngine;

public class UserData
{
    public const int MaxHeart = 5;
    public const int HeartChargeTime = 600;

    public string id;
    public string nickname;
    private int heart;
    public long nextHeartChargeTime;
    public string countryCode;

    public int Heart
    {
        get
        {
            if(heart < MaxHeart)
            {
                if(GameManager.Instance.dateTime !=null)
                {
                    long currentTime = GameManager.Instance.dateTime.Value.ToTick();
                    //638795125480740600 | 638795126420000000
                    Debug.Log(nextHeartChargeTime + " | " + currentTime);
                    if (nextHeartChargeTime < currentTime)
                    {

                        heart += 1;
                        heart += (int)((currentTime - nextHeartChargeTime) / HeartChargeTime);
                        if(heart > MaxHeart)
                        {
                            heart = MaxHeart;
                        }
                        nextHeartChargeTime = currentTime;
                    }
                }
            }
            
            return heart;
        }
    }

    public UserData()
    {

    }

    public UserData(string userId)
    {
        id = userId;

        //string country = RegionInfo.CurrentRegion.EnglishName; // 국가 이름
        countryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
        heart = MaxHeart;
        nextHeartChargeTime = GameManager.Instance.dateTime.Value.ToTick();
    }


    public bool UseHeart()
    {
        Debug.Log($"UseHeart | {GameManager.Instance.dateTime} | {Heart}");
        if (GameManager.Instance.dateTime == null) return false;
        if(Heart > 0)
        {
            heart -= 1;
            if (nextHeartChargeTime <= GameManager.Instance.dateTime.Value.ToTick())
            {
                nextHeartChargeTime = GameManager.Instance.dateTime.Value.ToTick() + HeartChargeTime;
            }
            //
            FirebaseManager.Instance.SaveUserData(this);
            HUD.Instance.UpdateHeart();
            return true;
        }
        return false;
    }


}
