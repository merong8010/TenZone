using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;

public class RankingList : InfiniteScroll<RankingList.Data>
{
    public class Data
    {
        public int rank;
        public string id;
        public int level;
        public string name;
        public string countryCode;
        public long timeStamp;
        public Data()
        {

        }
    }

    public class PointData : Data
    {
        public int point;
        public int remainMilliSeconds;

        public PointData(string id, int level, string name, int point, int remainMilliSeconds, string countryCode, long timeStamp = 0)
        {
            this.id = id;
            this.level = level;
            this.name = name;
            this.point = point;
            this.countryCode = countryCode;
            this.remainMilliSeconds = remainMilliSeconds;
            this.timeStamp = timeStamp == 0 ? GameManager.Instance.dateTime.Value.ToTick() : timeStamp;
        }
        public PointData(string id, int rank, int level, string name, int point, int remainMilliSeconds, string countryCode, long timeStamp = 0)
        {
            this.id = id;
            this.rank = rank;
            this.level = level;
            this.name = name;
            this.point = point;
            this.countryCode = countryCode;
            this.remainMilliSeconds = remainMilliSeconds;
            this.timeStamp = timeStamp == 0 ? GameManager.Instance.dateTime.Value.ToTick() : timeStamp;
        }
    }
    public class LevelData : Data
    {
        public int exp;
        public LevelData(string id, int level, string name, int exp, string countryCode, long timeStamp = 0)
        {
            this.id = id;
            this.level = level;
            this.name = name;
            this.exp = exp;
            this.countryCode = countryCode;
            this.timeStamp = timeStamp == 0 ? GameManager.Instance.dateTime.Value.ToTick() : timeStamp;
        }
        public LevelData(string id, int rank, int level, string name, int exp, string countryCode, long timeStamp = 0)
        {
            this.id = id;
            this.rank = rank;
            this.level = level;
            this.name = name;
            this.exp = exp;
            this.countryCode = countryCode;
            this.timeStamp = timeStamp == 0 ? GameManager.Instance.dateTime.Value.ToTick() : timeStamp;
        }
    }

}
