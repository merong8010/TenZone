using UnityEngine;

public class UserData
{
    public const int MaxHeart = 5;
    public const int HeartChargeTime = 600;

    public string id;
    public string nickname;
    private int heart;
    private long lastHeartChargeTime;

    public int Heart
    {
        get
        {
            //if(lastHeartChargeTime)

            return heart;
        }
    }
}
