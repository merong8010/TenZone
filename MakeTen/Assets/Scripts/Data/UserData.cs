using System.Globalization;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UniRx;
using Newtonsoft.Json;

public class UserData
{
    public const string VIP_ID = "tenzone.vip";
    public bool isVIP
    {
        get
        {
            return GetPurchaseCount(VIP_ID) > 0;
        }
    }
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
    public int attendanceCount;
    public bool IsRewardAttendanceAd;
    public string attendanceRewardDate;

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
    [JsonIgnore]
    public bool IsTutorial
    {
        get
        {
            return recordAll.Count == 0;
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
        goods.Add(GameData.GoodsType.Shuffle, DataManager.Instance.GetConfig("defaultShuffle"));
        goods.Add(GameData.GoodsType.Search, DataManager.Instance.GetConfig("defaultSearch"));
        goods.Add(GameData.GoodsType.Time_10s, DataManager.Instance.GetConfig("defaultTime_10s"));
        level = 1;
        attendanceCount = 0;
        attendanceRewardDate = GameManager.Instance.dateTime.Value.AddDays(-1).ToDateText();
        FirebaseManager.Instance.SaveUserData(this);
    }

    public void CheckDate()
    {
        //attendanceRewardDate != GameManager.Instance.dateTime.Value.ToDateText()
    }

    public bool IsAttendanceRewardable
    {
        get
        {
            if (attendanceRewardDate != GameManager.Instance.dateTime.Value.ToDateText())
            {
                IsRewardAttendanceAd = false;
                return true;
            }
            return false;
        }
    }

    public void RewardAttendacne()
    {
        attendanceCount += 1;
        attendanceRewardDate = GameManager.Instance.dateTime.Value.ToDateText();
    }

    public void RewardAttendanceAd()
    {
        IsRewardAttendanceAd = true;
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

    public void Charge(GoodsList.Data[] datas)
    {
        for(int i = 0; i < datas.Length; i++)
        {
            if (!goods.TryAdd(datas[i].type, datas[i].amount))
            {
                goods[datas[i].type] += datas[i].amount;
            }
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

    public class PurchaseData
    {
        public long lastPurchaseTick;
        public int count;
        public string id;
    }

    public PurchaseData[] purchaseDatas;

    public int GetPurchaseCount(string shopId)
    {
        return GetPurchaseCount(DataManager.Instance.Get<GameData.Shop>().SingleOrDefault(x => x.id == shopId));
    }

    public int GetPurchaseCount(GameData.Shop data)
    {
        if (purchaseDatas == null) return 0;
        PurchaseData myData = purchaseDatas.SingleOrDefault(x => x.id == data.id);
        if (myData == null) return 0;
        switch(data.buyPeriod)
        {
            case GameData.TimePeriod.Daily:
                if(myData.lastPurchaseTick.LongToDateTime().IsSameDate(GameManager.Instance.dateTime.Value))
                {
                    return myData.count;
                }
                break;
            case GameData.TimePeriod.Weekly:
                if (myData.lastPurchaseTick.LongToDateTime().IsSameWeek(GameManager.Instance.dateTime.Value))
                {
                    return myData.count;
                }
                break;
            case GameData.TimePeriod.Monthly:
                if (myData.lastPurchaseTick.LongToDateTime().IsSameMonth(GameManager.Instance.dateTime.Value))
                {
                    return myData.count;
                }
                break;
            default:
                return myData.count;

        }

        return 0;
    }

    public bool CanPurchase(GameData.Shop data, int count = 1)
    {
        return GetPurchaseCount(data) + count <= data.buyMaxCount;
    }

    public void AddMailData(string title, string desc, GoodsList.Data[] rewards)
    {
        FirebaseManager.Instance.SendMail(title, desc, rewards);
    }
    public void AddPurchase(GameData.Shop data, int count = 1)
    {
        if (!CanPurchase(data, count)) return;

        if(data.isRewardMail)
        {
            UIManager.Instance.Message.Show(Message.Type.Confirm, TextManager.Get("SendMailShopReward"));
            FirebaseManager.Instance.SendMail(data.name, data.desc, data.rewards);
        }
        else
        {
            Charge(data.rewards);
            UIManager.Instance.Open<PopupReward>().SetData(data.rewards);
        }
        if (purchaseDatas == null) purchaseDatas = new PurchaseData[] { };
        PurchaseData myData = purchaseDatas.SingleOrDefault(x => x.id == data.id);
        if (myData != null)
        {
            myData.count += count;
            myData.lastPurchaseTick = GameManager.Instance.dateTime.Value.ToTick();
        }
        else
        {
            myData = new PurchaseData();
            myData.id = data.id;
            myData.count = count;
            myData.lastPurchaseTick = GameManager.Instance.dateTime.Value.ToTick();
            purchaseDatas = purchaseDatas.Append(myData).ToArray();
        }

        if(data.id == VIP_ID)
        {
            ADManager.Instance.HideBanner(true);
            UIManager.Instance.Refresh();
            //SafeArea
        }
    }
}
