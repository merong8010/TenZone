using System.Globalization;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UniRx;
using Newtonsoft.Json;
using GameData;

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

    public Dictionary<PuzzleLevel, Record> recordAll = new Dictionary<PuzzleLevel, Record>();
    public Dictionary<PuzzleLevel, Record> recordToday = new Dictionary<PuzzleLevel, Record>();
    public Dictionary<string, MailList.Data> mailDatas = new Dictionary<string, MailList.Data>();
    public List<PurchaseData> purchaseDatas = new List<PurchaseData>();

    public void Copy(UserData from)
    {
        authType = from.authType;
        id = from.id;
        nickname = from.nickname;
        heart = from.heart;
        lastHeartTime = from.lastHeartTime;
        countryCode = from.countryCode;
        foreach(KeyValuePair<GoodsType, int> pair in from.goods)
        {
            if(goods.ContainsKey(pair.Key))
                goods[pair.Key] = pair.Value;
            else
                goods.Add(pair.Key, pair.Value);
        }
        exp = from.exp;
        level = from.level;
        nicknameChangeCount = from.nicknameChangeCount;
        banMessage = from.banMessage;
        lastPlayDate = from.lastPlayDate;
        attendanceCount = from.attendanceCount;
        IsRewardAttendanceAd = from.IsRewardAttendanceAd;
        attendanceRewardDate = from.attendanceRewardDate;
        recordAll = from.recordAll;
        recordToday = from.recordToday;
        mailDatas = from.mailDatas;
        purchaseDatas = from.purchaseDatas;
    }

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

    public bool IsNewRecord(PuzzleLevel level, int point, bool today)
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
        countryCode = Util.GetCountryCode();

        heart = DataManager.Instance.MaxHeart;
        lastHeartTime = GameManager.Instance.dateTime.Value.ToTick();
        goods.Add(GameData.GoodsType.Gold, DataManager.Instance.GetConfig("defaultGold"));
        goods.Add(GameData.GoodsType.Shuffle, DataManager.Instance.GetConfig("defaultShuffle"));
        goods.Add(GameData.GoodsType.Search, DataManager.Instance.GetConfig("defaultSearch"));
        goods.Add(GameData.GoodsType.Time_10s, DataManager.Instance.GetConfig("defaultTime_10s"));
        goods.Add(GameData.GoodsType.Explode, DataManager.Instance.GetConfig("defaultExplode"));
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
        else
        {
            UIManager.Instance.Message.Show(Message.Type.Simple, string.Format(TextManager.Get("NotEnougnGoods"), TextManager.Get(GoodsType.Heart.ToString())));
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

            UIManager.Instance.Open<PopupReward>().SetData(rewards, "LevelupReward");
        }

        //FirebaseManager.Instance.SubmitScoreLevel(DataManager.Instance.Get<GameData.UserLevel>().Where(x => x.level < level).Sum(x => x.exp) + exp);
        int totalExp = DataManager.Instance.Get<GameData.UserLevel>().Where(x => x.level < level).Sum(x => x.exp) + exp;
        FirebaseManager.Instance.SubmitScore(PuzzleLevel.None, "ALL", id, nickname, level, totalExp, countryCode);
        FirebaseManager.Instance.SaveUserData(this);
    }

    public void Charge(GoodsList.Data[] datas)
    {
        for(int i = 0; i < datas.Length; i++)
        {
            Charge(datas[i]);
        }
    }

    public void Charge(GoodsList.Data data)
    {
        Charge(data.type, data.amount);
        //if (!goods.TryAdd(data.type, data.amount))
        //{
        //    goods[data.type] += data.amount;
        //}
        //FirebaseManager.Instance.SaveUserData(this);
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

    public bool Use(GameData.GoodsType type, int amount, bool showMessage = false)
    {
        if (!goods.ContainsKey(type) || goods[type] < amount)
        {
            if (showMessage) UIManager.Instance.Message.Show(Message.Type.Simple, string.Format(TextManager.Get("NotEnoughGoods"), TextManager.Get(type.ToString())));
            return false;
        }
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
        
        return data.buyMaxCount <= 0 || GetPurchaseCount(data) + count <= data.buyMaxCount;
    }

    public void AddMailData(string title, string desc, GoodsList.Data[] rewards)
    {
        if (mailDatas == null) mailDatas = new Dictionary<string, MailList.Data>();
        MailList.Data mailData = new MailList.Data();
        mailData.id = UniqueId(mailDatas.Keys.ToList());
        mailData.title = title;
        mailData.desc = desc;
        mailData.rewards = rewards;
        mailDatas.Add(mailData.id, mailData);
        FirebaseManager.Instance.SaveUserData(this);
        UIManager.Instance.Message.Show(Message.Type.Confirm, TextManager.Get("SendMailShopReward"));
        //FirebaseManager.Instance.SendMail(title, desc, rewards);
    }
    private const string PURCHASEKEY = "purchase_";
    private string UniqueId(List<string> keys)
    {
        if(keys == null || keys.Count == 0 || !keys.Exists(x => x.Contains(PURCHASEKEY)))
        {
            return PURCHASEKEY + "0";
        }
        return PURCHASEKEY+(keys.Where(x => x.Contains(PURCHASEKEY)).Select(x => int.Parse(x.Remove(0, PURCHASEKEY.Length))).Max() + 1).ToString();
    }
    public void AddPurchase(GameData.Shop data, int count = 1)
    {
        Debug.Log($"AddPurchase  {CanPurchase(data, count)} | {data.isRewardMail}");
        if (!CanPurchase(data, count)) return;

        if(data.isRewardMail)
        {
            //UIManager.Instance.Message.Show(Message.Type.Confirm, TextManager.Get("SendMailShopReward"));
            AddMailData(data.name, data.desc, data.rewards);
            //FirebaseManager.Instance.SendMail(data.name, data.desc, data.rewards);
        }
        else
        {
            Charge(data.rewards);
            UIManager.Instance.Open<PopupReward>().SetData(data.rewards);
        }
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
            purchaseDatas.Add(myData);
        }

        if(data.id == VIP_ID)
        {
            ADManager.Instance.HideBanner(true);
            UIManager.Instance.Refresh();
            //SafeArea
        }
        FirebaseManager.Instance.SaveUserData(this);
        //UIManager.Instance.Message.Show(Message.Type.Confirm, string.Format(TextManager.Get("PurchaseSuccess"), TextManager.Get(data.name)));
    }
}
