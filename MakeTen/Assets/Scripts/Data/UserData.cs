using System.Globalization;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UniRx;
using Newtonsoft.Json;

public class UserData
{
    public FirebaseManager.AuthenticatedType authType;
    public string id;
    public string nickname;
    private int heart;
    public long lastHeartTime;
    public string countryCode;
    public ReactiveDictionary<GameData.GoodsType, int> goods = new ReactiveDictionary<GameData.GoodsType, int>();

    public int exp;
    public int level;

    public class Record : IComparable<Record>
    {
        public int point;
        public int remainMilliSeconds;
        public long timeStamp;

        public Record(int point, int remain)
        {
            this.point = point;
            this.remainMilliSeconds = remain;
            timeStamp = GameManager.Instance.dateTime.Value.Ticks;
        }

        public int CompareTo(Record other)
        {
            if (point != other.point) return point - other.point;
            if (remainMilliSeconds != other.remainMilliSeconds) return other.remainMilliSeconds - remainMilliSeconds;
            return (int)(other.timeStamp - timeStamp);
        }
    }
    public Dictionary<PuzzleManager.Level, Record> recordAll = new Dictionary<PuzzleManager.Level, Record>();
    public Dictionary<PuzzleManager.Level, Record> recordToday = new Dictionary<PuzzleManager.Level, Record>();

    public bool IsNewRecord(PuzzleManager.Level level, int point, int remain, bool today)
    {
        Record newRecord = new Record(point, remain);
        if (today)
        {
            if(recordToday.ContainsKey(level))
            {
                if(recordToday[level].CompareTo(newRecord) < 0)
                {
                    recordToday[level] = newRecord;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                recordToday.Add(level, newRecord);
                return true;
            }
        }
        else
        {
            if (recordAll.ContainsKey(level))
            {
                if (recordAll[level].CompareTo(newRecord) < 0)
                {
                    recordAll[level] = newRecord;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                recordAll.Add(level, newRecord);
                return true;
            }
        }
    }

    public MailList.Data[] mailDatas;

    //private void UpdateHeartByTime()
    //{
    //    TimeSpan timePassed = DateTime.Now - lastHeartTime;
    //    int recoverCount = Mathf.FloorToInt((float)timePassed.TotalMinutes / heartRecoveryTimeMinutes);
    //    if (recoverCount > 0)
    //    {
    //        currentHeart = Mathf.Min(currentHeart + recoverCount, maxHeart);
    //        lastHeartTime = lastHeartTime.AddMinutes(recoverCount * heartRecoveryTimeMinutes);
    //        SaveHeartData();
    //    }
    //}
    public int Heart
    {
        get
        {
            if(heart < DataManager.Instance.MaxHeart)
            {
                if(GameManager.Instance.dateTime !=null)
                {
                    int passedSec = (int)(GameManager.Instance.dateTime.Value.ToTick() - lastHeartTime);
                    int recoverCount = Mathf.FloorToInt((float)passedSec / DataManager.Instance.HeartChargeTime);
                    if(recoverCount > 0)
                    {
                        heart = Mathf.Min(heart + recoverCount, DataManager.Instance.MaxHeart);
                        lastHeartTime = lastHeartTime + (recoverCount * DataManager.Instance.HeartChargeTime);
                    }
                    //long currentTime = GameManager.Instance.dateTime.Value.ToTick();
                    //if (lastHeartTime + DataManager.Instance.HeartChargeTime < currentTime)
                    //{
                    //    //heart += 1;
                    //    heart += (int)((currentTime - lastHeartTime) / DataManager.Instance.HeartChargeTime);
                    //    if(heart >= DataManager.Instance.MaxHeart)
                    //    {
                    //        heart = DataManager.Instance.MaxHeart;
                    //        lastHeartTime = currentTime;
                    //    }
                    //    else
                    //    {
                    //        lastHeartTime = currentTime - ((currentTime - lastHeartTime) % DataManager.Instance.HeartChargeTime);
                    //        //lastHeartChargeTime = currentTime + ((currentTime - lastHeartChargeTime) % DataManager.Instance.HeartChargeTime);
                    //    }
                    //}
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
        heart = DataManager.Instance.MaxHeart;
        lastHeartTime = GameManager.Instance.dateTime.Value.ToTick();
        goods.Add(GameData.GoodsType.Gold, DataManager.Instance.Get<GameData.Config>().SingleOrDefault(x => x.key == "defaultGold").val);
        goods.Add(GameData.GoodsType.Gem, DataManager.Instance.Get<GameData.Config>().SingleOrDefault(x => x.key == "defaultGem").val);
        goods.Add(GameData.GoodsType.Shuffle, DataManager.Instance.Get<GameData.Config>().SingleOrDefault(x => x.key == "defaultShuffle").val);
        level = 1;
        FirebaseManager.Instance.SaveUserData(this);
    }


    public bool UseHeart()
    {
        if (GameManager.Instance.dateTime == null) return false;
        if(Heart > 0)
        {
            heart -= 1;
            if(heart == DataManager.Instance.MaxHeart-1)
            {
                lastHeartTime = GameManager.Instance.dateTime.Value.ToTick();
            }
            FirebaseManager.Instance.SaveUserData(this);
            return true;
        }
        return false;
    }

    public void ChargeHeart()
    {
        heart += 1;
        //lastHeartChargeTime = GameManager.Instance.dateTime.Value.ToTick();
        //if(heart >= DataManager.Instance.MaxHeart)
        //{
        //    lastHeartChargeTime = GameManager.Instance.dateTime.Value.ToTick();
        //}
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
        if(this.exp > DataManager.Instance.Get<GameData.UserLevel>().SingleOrDefault(x => x.level == level).exp)
        {
            List<GoodsList.Data> rewards = new List<GoodsList.Data>();
            while (this.exp > DataManager.Instance.Get<GameData.UserLevel>().SingleOrDefault(x => x.level == level).exp)
            {
                this.level += 1;
                this.exp -= DataManager.Instance.Get<GameData.UserLevel>().SingleOrDefault(x => x.level == level).exp;
                rewards.AddRange(DataManager.Instance.Get<GameData.UserLevel>().SingleOrDefault(x => x.level == level).rewards);
            }

            for(int i = 0; i < rewards.Count; i++)
            {
                Charge(rewards[i].type, rewards[i].amount);
            }

            UIManager.Instance.Open<PopupReward>().SetData(rewards);
        }

        FirebaseManager.Instance.SaveUserData(this);
    }

    public void Charge(GoodsList.Data data)
    {
        if (!goods.TryAdd(data.type, data.amount))
        {
            goods[data.type] += data.amount;
        }
        FirebaseManager.Instance.SaveUserData(this);
    }

    public void Charge(GameData.GoodsType type, int amount)
    {
        if(!goods.TryAdd(type, amount))
        {
            goods[type] += amount;
        }
        FirebaseManager.Instance.SaveUserData(this);
    }

    public bool Use(GameData.GoodsType type, int amount)
    {
        if (!goods.ContainsKey(type)) return false;
        if (goods[type] < amount) return false;
        goods[type] -= amount;
        FirebaseManager.Instance.SaveUserData(this);
        return true;
    }
}
