using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UniRx;
using Newtonsoft.Json;
using GameData;
using UserData;

public class UserDataManager
{
    public UserData.Info Info;
    public UserData.Goods Goods;
    public UserData.Attendance Attendance;
    public UserData.Record Record;
    public UserData.Mail Mail;
    public UserData.Purchase Purchase;
    public UserData.Quest Quest;

    public UserDataManager()
    {

    }
    public UserDataManager(string json)
    {
        
        if(string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("new UserData()");
            Info = new Info();
            Info.id = FirebaseManager.Instance.UserId;
            Info.countryCode = Util.GetCountryCode();
            Info.level = 1;
            Info.MarkAsDirty();

            Goods = new Goods();
            Goods.heart = DataManager.Instance.MaxHeart;
            Goods.lastHeartTime = GameManager.Instance.dateTime.Value.ToTick();
            Goods.datas.Add(GameData.GoodsType.Gold, DataManager.Instance.GetConfig("defaultGold"));
            Goods.datas.Add(GameData.GoodsType.Shuffle, DataManager.Instance.GetConfig("defaultShuffle"));
            Goods.datas.Add(GameData.GoodsType.Search, DataManager.Instance.GetConfig("defaultSearch"));
            Goods.datas.Add(GameData.GoodsType.Time_10s, DataManager.Instance.GetConfig("defaultTime_10s"));
            Goods.datas.Add(GameData.GoodsType.Explode, DataManager.Instance.GetConfig("defaultExplode"));
            Goods.MarkAsDirty();

            Attendance = new UserData.Attendance();
            Attendance.count = 0;
            Attendance.rewardDate = GameManager.Instance.dateTime.Value.AddDays(-1).ToDateText();
            Attendance.MarkAsDirty();

            Record = new UserData.Record();
            Record.MarkAsDirty();

            Mail = new UserData.Mail();
            Mail.MarkAsDirty();

            Purchase = new UserData.Purchase();
            Purchase.MarkAsDirty();

            Quest = new UserData.Quest();
            Quest.MarkAsDirty();
            SaveData();
        }
        else
        {
            UserDataManager myData = JsonConvert.DeserializeObject<UserDataManager>(json);
            this.Info = myData.Info != null ? myData.Info : new UserData.Info();
            this.Goods = myData.Goods != null ? myData.Goods : new UserData.Goods();
            this.Attendance = myData.Attendance != null ? myData.Attendance : new UserData.Attendance();
            this.Record = myData.Record != null ? myData.Record : new UserData.Record();
            this.Mail = myData.Mail != null ? myData.Mail : new UserData.Mail();
            this.Purchase = myData.Purchase != null ? myData.Purchase : new UserData.Purchase();
            this.Quest = myData.Quest != null ? myData.Quest : new UserData.Quest();
            //CheckDate();
        }
    }

    public void SaveData()
    {
        List<UserData.DataObject> saveDatas = new List<UserData.DataObject>();
        if (Info.IsDirty) saveDatas.Add(Info);
        if (Goods.IsDirty) saveDatas.Add(Goods);
        if (Attendance.IsDirty) saveDatas.Add(Attendance);
        if (Record.IsDirty) saveDatas.Add(Record);
        if (Mail.IsDirty) saveDatas.Add(Mail);
        if (Purchase.IsDirty) saveDatas.Add(Purchase);
        if (Quest.IsDirty) saveDatas.Add(Quest);
        if(saveDatas.Count > 0) FirebaseManager.Instance.SaveUserData(saveDatas);
    }

    public void CheckDate()
    {
        var questDatas = DataManager.Instance.Get<GameData.Quest>().Where(x => x.category == QuestCategory.daily || x.category == QuestCategory.weekly);
        foreach(GameData.Quest questData in questDatas)
        {
            UserData.Quest.Data userData = GetQuest(questData.id);
            if((questData.category == QuestCategory.daily && !userData.lastUpdateTime.LongToDateTime().IsSameDate(GameManager.Instance.dateTime.Value)) || (questData.category == QuestCategory.weekly && !userData.lastUpdateTime.LongToDateTime().IsSameWeek(GameManager.Instance.dateTime.Value)))
            {
                userData.count = 0;
                userData.isRewardClaimed = false;
                userData.isCompleted = false;
            }
        }
        Quest.MarkAsDirty();
    }

    public const string VIP_ID = "tenzone.vip";
    private const string PURCHASEKEY = "purchase_";
    public bool isVIP
    {
        get
        {
            return GetPurchaseCount(VIP_ID) > 0;
        }
    }

    public bool IsTutorial
    {
        get
        {
            return Record.all.Count == 0;
        }

    }

    public void CountNicknameChange()
    {
        Info.nicknameChangeCount++;
        Info.MarkAsDirty();
    }

    public bool IsNewRecord(PuzzleLevel level, int point, bool today)
    {
        UserData.Record.Data newRecord = new UserData.Record.Data(point);
        if (today)
        {
            if (Record.lastPlayDate != GameManager.Instance.dateTime.Value.ToDateText())
            {
                Record.today.Clear();
            }
            Record.lastPlayDate = GameManager.Instance.dateTime.Value.ToDateText();

            if (Record.today.ContainsKey(level))
            {
                if (Record.today[level].CompareTo(newRecord) < 0)
                {
                    Record.today[level] = newRecord;
                    Record.MarkAsDirty();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Record.today.Add(level, newRecord);
                Record.MarkAsDirty();
                return true;
            }
        }
        else
        {
            if (Record.all.ContainsKey(level))
            {
                if (Record.all[level].CompareTo(newRecord) < 0)
                {
                    Record.all[level] = newRecord;
                    Record.MarkAsDirty();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Record.all.Add(level, newRecord);
                Record.MarkAsDirty();
                return true;
            }
        }
    }

    public int Heart
    {
        get
        {
            if (Goods.heart < DataManager.Instance.MaxHeart)
            {
                if (GameManager.Instance.dateTime != null)
                {
                    int passedSec = (int)(GameManager.Instance.dateTime.Value.ToTick() - Goods.lastHeartTime);
                    int recoverCount = Mathf.FloorToInt((float)passedSec / DataManager.Instance.HeartChargeTime);
                    //Debug.Log($"passedSec : {heart} | {passedSec} | {recoverCount}");
                    if (recoverCount > 0)
                    {
                        Goods.heart = Mathf.Min(Goods.heart + recoverCount, DataManager.Instance.MaxHeart);
                        Goods.lastHeartTime = Goods.lastHeartTime + (recoverCount * DataManager.Instance.HeartChargeTime);
                    }
                }
            }

            return Goods.heart;
        }
        set
        {
            Goods.heart = value;
        }
    }

    public bool IsAttendanceRewardable
    {
        get
        {
            if (Attendance.rewardDate != GameManager.Instance.dateTime.Value.ToDateText())
            {
                Attendance.isRewardAd = false;
                return true;
            }
            return false;
        }
    }

    public void RewardAttendacne()
    {
        Attendance.count += 1;
        Attendance.rewardDate = GameManager.Instance.dateTime.Value.ToDateText();
        Attendance.MarkAsDirty();

        DoQuest(QuestType.attendance);
    }

    public void RewardAttendanceAd()
    {
        Attendance.isRewardAd = true;
    }

    public bool UseHeart()
    {
        if (GameManager.Instance.dateTime == null) return false;

        if (Heart > 0)
        {
            Goods.heart -= 1;
            if (Goods.heart == DataManager.Instance.MaxHeart - 1)
            {
                Goods.lastHeartTime = GameManager.Instance.dateTime.Value.ToTick();
            }
            Goods.MarkAsDirty();
            DoQuest(QuestType.useHeart);
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
        Goods.heart += count;
        if (Goods.heart >= DataManager.Instance.MaxHeart)
        {
            Goods.lastHeartTime = GameManager.Instance.dateTime.Value.ToTick() - DataManager.Instance.HeartChargeTime;
        }
        Goods.MarkAsDirty();
    }

    public void UpdateData(string userId, FirebaseManager.AuthenticatedType authType)
    {
        Info.id = userId;
        Info.authType = authType;

        Info.MarkAsDirty();
    }

    public void ChargeExp(int exp)
    {
        Info.exp += exp;
        if (Info.exp > DataManager.Instance.Get<GameData.UserLevel>().SingleOrDefault(x => x.level == Info.level).exp)
        {
            List<GoodsList.Data> rewards = new List<GoodsList.Data>();
            while (Info.exp >= DataManager.Instance.Get<GameData.UserLevel>().SingleOrDefault(x => x.level == Info.level).exp)
            {
                Info.exp -= DataManager.Instance.Get<GameData.UserLevel>().SingleOrDefault(x => x.level == Info.level).exp;
                Info.level += 1;
                rewards.AddRange(DataManager.Instance.Get<GameData.UserLevel>().SingleOrDefault(x => x.level == Info.level).rewards);
            }

            for (int i = 0; i < rewards.Count; i++)
            {
                Charge(rewards[i].type, rewards[i].amount);
            }

            UIManager.Instance.Open<PopupReward>().SetData(rewards, "LevelupReward");
        }

        int totalExp = DataManager.Instance.Get<GameData.UserLevel>().Where(x => x.level < Info.level).Sum(x => x.exp) + exp;
        FirebaseManager.Instance.SubmitScore(PuzzleLevel.None, "ALL", Info.id, Info.nickname, Info.level, totalExp, Info.countryCode);
        Info.MarkAsDirty();
    }

    public void Charge(GoodsList.Data[] datas)
    {
        for (int i = 0; i < datas.Length; i++)
        {
            Charge(datas[i]);
        }
    }

    public void Charge(GoodsList.Data data)
    {
        Charge(data.type, data.amount);
    }

    public void Charge(GameData.GoodsType type, int amount)
    {
        if (type == GameData.GoodsType.Heart)
        {
            ChargeHeart(amount);
            return;
        }
        if (type == GameData.GoodsType.EXP)
        {
            ChargeExp(amount);
            return;
        }

        if (!Goods.datas.TryAdd(type, amount))
        {
            Goods.datas[type] += amount;
        }
        Goods.MarkAsDirty();
    }

    public bool Use(GameData.GoodsType type, int amount, bool showMessage = false)
    {
        if (!Goods.datas.ContainsKey(type) || Goods.datas[type] < amount)
        {
            if (showMessage) UIManager.Instance.Message.Show(Message.Type.Simple, string.Format(TextManager.Get("NotEnoughGoods"), TextManager.Get(type.ToString())));
            return false;
        }
        Goods.datas[type] -= amount;
        Goods.MarkAsDirty();

        DoQuest(QuestType.useGoods, amount, (int)type);
        return true;
    }

    public bool Has(GameData.GoodsType type, int amount)
    {
        if (!Goods.datas.ContainsKey(type)) return false;
        if (Goods.datas[type] < amount) return false;
        return true;
    }

    public int GetPurchaseCount(string shopId)
    {
        return GetPurchaseCount(DataManager.Instance.Get<GameData.Shop>().SingleOrDefault(x => x.id == shopId));
    }

    public int GetPurchaseCount(GameData.Shop data)
    {
        if (Purchase.datas == null) return 0;
        UserData.Purchase.Data myData = Purchase.datas.SingleOrDefault(x => x.id == data.id);
        if (myData == null) return 0;
        switch (data.buyPeriod)
        {
            case GameData.TimePeriod.Daily:
                if (myData.lastPurchaseTick.LongToDateTime().IsSameDate(GameManager.Instance.dateTime.Value))
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
        if (Mail.datas == null) Mail.datas = new Dictionary<string, MailList.Data>();
        MailList.Data mailData = new MailList.Data();
        mailData.id = UniqueId(Mail.datas.Keys.ToList());
        mailData.title = title;
        mailData.desc = desc;
        mailData.rewards = rewards;
        Mail.datas.Add(mailData.id, mailData);
        UIManager.Instance.Message.Show(Message.Type.Confirm, TextManager.Get("SendMailShopReward"));

        Mail.MarkAsDirty();
    }

    
    private string UniqueId(List<string> keys)
    {
        if (keys == null || keys.Count == 0 || !keys.Exists(x => x.Contains(PURCHASEKEY)))
        {
            return PURCHASEKEY + "0";
        }
        return PURCHASEKEY + (keys.Where(x => x.Contains(PURCHASEKEY)).Select(x => int.Parse(x.Remove(0, PURCHASEKEY.Length))).Max() + 1).ToString();
    }
    public void AddPurchase(GameData.Shop data, int count = 1)
    {
        if (!CanPurchase(data, count)) return;

        if (data.isRewardMail)
        {
            AddMailData(data.name, data.desc, data.rewards);
        }
        else
        {
            Charge(data.rewards);
            UIManager.Instance.Open<PopupReward>().SetData(data.rewards);
        }
        UserData.Purchase.Data myData = Purchase.datas.SingleOrDefault(x => x.id == data.id);
        if (myData != null)
        {
            myData.count += count;
            myData.lastPurchaseTick = GameManager.Instance.dateTime.Value.ToTick();
        }
        else
        {
            myData = new UserData.Purchase.Data();
            myData.id = data.id;
            myData.count = count;
            myData.lastPurchaseTick = GameManager.Instance.dateTime.Value.ToTick();
            Purchase.datas.Add(myData);
        }

        if (data.id == VIP_ID)
        {
            ADManager.Instance.HideBanner(true);
            UIManager.Instance.Refresh();
        }
        Purchase.MarkAsDirty();
    }

    public UserData.Quest.Data GetQuest(int questId)
    {
        if (!Quest.progress.ContainsKey(questId))
        {
            Quest.progress[questId] = new UserData.Quest.Data { id = questId, lastUpdateTime = GameManager.Instance.dateTime.Value.ToTick() };
        }

        return Quest.progress[questId];
    }

    public void DoQuest(QuestType type, int count = 1, int questId = 0)
    {
        var questDatas = DataManager.Instance.Get<GameData.Quest>().Where(x => x.type == type && (x.questId == questId || x.questId == 0));
        foreach (GameData.Quest data in questDatas)
        {
            UserData.Quest.Data progress = GetQuest(data.id);
            
            if (progress.isCompleted) continue;

            // 카운트 증가 및 완료 처리
            progress.count += count;
            if (data.category != QuestCategory.repeat && progress.count >= data.questCount)
            {
                progress.isCompleted = true;
                progress.count = data.questCount;

                DoQuest(QuestType.clearQuest, 1, (int)data.category);
                // TODO: 유저에게 퀘스트 완료 알림
            }
        }
        Quest.MarkAsDirty();
    }

    public bool ReceiveQuestReward(int questId)
    {
        if (!Quest.progress.ContainsKey(questId))
        {
            Quest.progress[questId] = new UserData.Quest.Data { id = questId };
        }

        GameData.Quest questData = DataManager.Instance.Get<GameData.Quest>().SingleOrDefault(x => x.id == questId);
        UserData.Quest.Data progress = Quest.progress[questId];
        if (questData.category != QuestCategory.repeat && progress.isCompleted && !progress.isRewardClaimed)
        {
            Charge(questData.rewards);
            UIManager.Instance.Open<PopupReward>().SetData(questData.rewards);
            progress.isRewardClaimed = true;
            Quest.MarkAsDirty();
            return true;
        }
        if(questData.category == QuestCategory.repeat && progress.count >= questData.questCount)
        {
            int rewardCount = progress.count / questData.questCount;
            var rewardDatas = questData.rewards.Select(x => new GoodsList.Data() { type = x.type, amount = x.amount * rewardCount }).ToArray();
            Charge(rewardDatas);
            UIManager.Instance.Open<PopupReward>().SetData(rewardDatas);
            progress.count -= rewardCount * questData.questCount;
            Quest.MarkAsDirty();
            return true;
        }
        return false;
    }
}