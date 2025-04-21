using System.Globalization;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UniRx;
using Newtonsoft.Json;

public class UserData
{
    public bool isVIP = false;
    public FirebaseManager.AuthenticatedType authType;
    public string id;
    public string nickname;
    private int heart;
    public long lastHeartTime;
    public string countryCode;
    public ReactiveDictionary<GameData.GoodsType, int> goods = new ReactiveDictionary<GameData.GoodsType, int>();

    public int exp;
    public int level;

    public int nicknameChangeCount = 0;

    //cheater is not empty
    public string banMessage;

    public string lastPlayDate;

    public class Record : IComparable<Record>
    {
        public int point;
        public long timeStamp;

        public Record(int point)
        {
            this.point = point;
            timeStamp = GameManager.Instance.dateTime.Value.Ticks;
        }

        public int CompareTo(Record other)
        {
            if (point != other.point) return point - other.point;
            return (int)(other.timeStamp - timeStamp);
        }
    }
    public Dictionary<PuzzleManager.Level, Record> recordAll = new Dictionary<PuzzleManager.Level, Record>();
    public Dictionary<PuzzleManager.Level, Record> recordToday = new Dictionary<PuzzleManager.Level, Record>();

    public bool IsNewRecord(PuzzleManager.Level level, int point, bool today)
    {
        Record newRecord = new Record(point);
        if (today)
        {
            if(lastPlayDate != GameManager.Instance.dateTime.Value.ToDateText())
            {
                recordToday.Clear();
            }
            lastPlayDate = GameManager.Instance.dateTime.Value.ToDateText();

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

    //public MailList.Data[] mailDatas;
    public Dictionary<string, MailList.Data> mailDatas = new Dictionary<string, MailList.Data>();

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
                    //Debug.Log($"passedSec : {heart} | {passedSec} | {recoverCount}");
                    if(recoverCount > 0)
                    {
                        heart = Mathf.Min(heart + recoverCount, DataManager.Instance.MaxHeart);
                        lastHeartTime = lastHeartTime + (recoverCount * DataManager.Instance.HeartChargeTime);
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
        CultureInfo ci = CultureInfo.InstalledUICulture; // ???? new CultureInfo(Application.systemLanguage.ToString())
        RegionInfo region = new RegionInfo(ci.Name);
        //string country = RegionInfo.CurrentRegion.EnglishName; // ???? ????
        countryCode = region.TwoLetterISORegionName;
        heart = DataManager.Instance.MaxHeart;
        lastHeartTime = GameManager.Instance.dateTime.Value.ToTick();
        goods.Add(GameData.GoodsType.Gold, DataManager.Instance.GetConfig("defaultGold"));
        goods.Add(GameData.GoodsType.Gem, DataManager.Instance.GetConfig("defaultGem"));
        goods.Add(GameData.GoodsType.Shuffle, DataManager.Instance.GetConfig("defaultShuffle"));
        goods.Add(GameData.GoodsType.Search, DataManager.Instance.GetConfig("defaultSearch"));
        goods.Add(GameData.GoodsType.Time_10s, DataManager.Instance.GetConfig("defaultTime_10s"));
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

    public void ChargeHeart(int count = 1)
    {
        heart += count;
        if (heart >= DataManager.Instance.MaxHeart)
        {
            lastHeartTime = GameManager.Instance.dateTime.Value.ToTick() - DataManager.Instance.HeartChargeTime;
        }
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
            while (this.exp >= DataManager.Instance.Get<GameData.UserLevel>().SingleOrDefault(x => x.level == level).exp)
            {
                this.exp -= DataManager.Instance.Get<GameData.UserLevel>().SingleOrDefault(x => x.level == level).exp;
                this.level += 1;
                rewards.AddRange(DataManager.Instance.Get<GameData.UserLevel>().SingleOrDefault(x => x.level == level).rewards);
            }

            for(int i = 0; i < rewards.Count; i++)
            {
                Charge(rewards[i].type, rewards[i].amount);
            }

            UIManager.Instance.Open<PopupReward>().SetData(rewards);
        }

        FirebaseManager.Instance.SubmitScoreLevel(DataManager.Instance.Get<GameData.UserLevel>().Where(x => x.level < level).Sum(x => x.exp) + exp);
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
        if(type == GameData.GoodsType.Heart)
        {
            ChargeHeart(amount);
            return;
        }
        if( type == GameData.GoodsType.EXP)
        {
            ChargeExp(amount);
            return;
        }

        if(!goods.TryAdd(type, amount))
        {
            goods[type] += amount;
        }
        FirebaseManager.Instance.SaveUserData(this);
    }

    public bool Use(GameData.GoodsType type, int amount)
    {
        Debug.Log($"Use {type} | {amount}");
        if (!goods.ContainsKey(type)) return false;
        if (goods[type] < amount) return false;
        goods[type] -= amount;
        FirebaseManager.Instance.SaveUserData(this);
        return true;
    }

    public bool Has(GameData.GoodsType type, int amount)
    {
        if (!goods.ContainsKey(type)) return false;
        if (goods[type] < amount) return false;
        return true;
    }
}
