using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UniRx;
using Newtonsoft.Json;
using GameData;
using System.Xml.Serialization;

namespace UserData
{
    public class DataObject
    {
        [JsonIgnore] // 저장하지 않을 임시 데이터
        public bool IsDirty { get; private set; }

        public void MarkAsDirty() { IsDirty = true; }
        public void Clean() { IsDirty = false; }
    }
    public class Info : DataObject
    {
        public FirebaseManager.AuthenticatedType authType;
        public string id;
        public string nickname;

        public string countryCode;
        public int exp;
        public int level;
        public int nicknameChangeCount = 0;
        //cheater is not empty
        public string banMessage;
    }

    public class Goods : DataObject
    {
        public int heart;
        public long lastHeartTime;
        public ReactiveDictionary<GameData.GoodsType, int> datas = new ReactiveDictionary<GameData.GoodsType, int>();
    }

    public class Attendance : DataObject
    {
        public int count;
        public bool isRewardAd;
        public string rewardDate;
    }

    public class Record : DataObject
    {
        public class Data : IComparable<Data>
        {
            public int point;
            public long timeStamp;

            public Data(int point)
            {
                this.point = point;
                timeStamp = GameManager.Instance.dateTime.Value.Ticks;
            }

            public int CompareTo(Data other)
            {
                if (point != other.point) return point - other.point;
                return (int)(other.timeStamp - timeStamp);
            }
        }
        public string lastPlayDate;

        public Dictionary<PuzzleLevel, Data> all = new Dictionary<PuzzleLevel, Data>();
        public Dictionary<PuzzleLevel, Data> today = new Dictionary<PuzzleLevel, Data>();
    }

    public class Mail : DataObject
    {
        public Dictionary<string, MailList.Data> datas = new Dictionary<string, MailList.Data>();
    }

    public class Purchase : DataObject
    {
        public class Data
        {
            public long lastPurchaseTick;
            public int count;
            public string id;
        }
        public List<Data> datas = new List<Data>();
    }

    public class Quest : DataObject
    {
        public class Data
        {
            public int id; // 원본 퀘스트 ID (딕셔너리의 Key와 동일)
            public int count; // 현재 진행 횟수
            public bool isCompleted; // 완료 여부
            public bool isRewardClaimed; // 보상 수령 여부
            public long lastUpdateTime; // (선택사항) 일일/주간 퀘스트 초기화를 위한 마지막 업데이트 시간 (timestamp)
        }

        public Dictionary<int, Data> progress = new Dictionary<int, Data>();
    }
}


