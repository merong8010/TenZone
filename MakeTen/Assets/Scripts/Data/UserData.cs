using System.Globalization;
using UnityEngine;

public class UserData
{
    public const int MaxHeart = 5;
    public const int HeartChargeTime = 600;

    public string id;
    public string nickname;
    private int heart;
    private long lastHeartChargeTime;
    public string countryCode;

    public int Heart
    {
        get
        {
            //if(lastHeartChargeTime)

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
        lastHeartChargeTime = GameManager.Instance.dateTime.Value.Ticks;
    }
}
