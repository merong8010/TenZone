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
        [JsonIgnore] // �������� ���� �ӽ� ������
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
            public int id; // ���� ����Ʈ ID (��ųʸ��� Key�� ����)
            public int count; // ���� ���� Ƚ��
            public bool isCompleted; // �Ϸ� ����
            public bool isRewardClaimed; // ���� ���� ����
            public long lastUpdateTime; // (���û���) ����/�ְ� ����Ʈ �ʱ�ȭ�� ���� ������ ������Ʈ �ð� (timestamp)
        }

        public Dictionary<int, Data> progress = new Dictionary<int, Data>();
    }
}


