using System.Linq;
using UnityEngine;

public class RankingList : InfiniteScroll<RankingList.Data>
{
    public class Data
    {
        public int rank;
        public string id;
        public string name;
        public int point;
        public int remainMilliSeconds;
        public string countryCode;
        public long timeStamp;
        public Data()
        {

        }
        public Data(string id, string name, int score, int remainMilliSeconds, string countryCode, long timeStamp = 0)
        {
            this.id = id;
            this.name = name;
            this.point = score;
            this.countryCode = countryCode;
            this.remainMilliSeconds = remainMilliSeconds;
            this.timeStamp = timeStamp == 0 ? GameManager.Instance.dateTime.Value.ToTick() : timeStamp;
        }
        public Data(string id, int rank, string name, int score, int remainMilliSeconds, string countryCode, long timeStamp = 0)
        {
            this.id = id;
            this.rank = rank;
            this.name = name;
            this.point = score;
            this.countryCode = countryCode;
            this.remainMilliSeconds = remainMilliSeconds;
            this.timeStamp = timeStamp == 0 ? GameManager.Instance.dateTime.Value.ToTick() : timeStamp;
        }
    }
}
