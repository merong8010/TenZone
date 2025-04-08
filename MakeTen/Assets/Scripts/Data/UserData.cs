using System.Globalization;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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
    public Dictionary<GameData.GoodsType, int> goods = new Dictionary<GameData.GoodsType, int>();

    public int exp;
    public int level;

    public MailList.Data[] mailDatas;

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
        goods.Add(GameData.GoodsType.Gold, DataManager.Instance.config.defaultGold);
        goods.Add(GameData.GoodsType.Gem, DataManager.Instance.config.defaultGem);
        goods.Add(GameData.GoodsType.Shuffle, DataManager.Instance.config.defaultShuffle);
        FirebaseManager.Instance.CreateAvailableNickname(nick =>
        {
            nickname = nick;
        });
    }


    public bool UseHeart()
    {
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

    public void ChargeExp(int exp)
    {
        this.exp += exp;
        if(this.exp > DataManager.Instance.userLevel.Vals.SingleOrDefault(x => x.level == level).exp)
        {
            List<GoodsList.Data> rewards = new List<GoodsList.Data>();
            while (this.exp > DataManager.Instance.userLevel.Vals.SingleOrDefault(x => x.level == level).exp)
            {
                this.level += 1;
                this.exp -= DataManager.Instance.userLevel.Vals.SingleOrDefault(x => x.level == level).exp;
                rewards.AddRange(DataManager.Instance.userLevel.Vals.SingleOrDefault(x => x.level == level).rewards);
            }

            for(int i = 0; i < rewards.Count; i++)
            {
                Charge(rewards[i].type, rewards[i].amount);
            }

            UIManager.Instance.Open<PopupReward>().SetData(rewards);
        }

    }

    public void Charge(GoodsList.Data data)
    {
        if (!goods.TryAdd(data.type, data.amount))
        {
            goods[data.type] += data.amount;
        }
    }

    public void Charge(GameData.GoodsType type, int amount)
    {
        if(!goods.TryAdd(type, amount))
        {
            goods[type] += amount;
        }
    }

    public bool Use(GameData.GoodsType type, int amount)
    {
        if (!goods.ContainsKey(type)) return false;
        if (goods[type] < amount) return false;
        goods[type] -= amount;
        return true;
    }
}
