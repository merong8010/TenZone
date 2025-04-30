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
        public string nickname;
        public string countryCode;
        public long timeStamp;

        public int point; //level에서는 누적 경험치


        public Data()
        {

        }

        public Data(string id, int level, string nickname, int point, string countryCode, long timeStamp = 0)
        {
            this.id = id;
            this.level = level;
            this.nickname = nickname;
            this.point = point;
            this.countryCode = countryCode;
            this.timeStamp = timeStamp == 0 ? GameManager.Instance.dateTime.Value.ToTick() : timeStamp;
        }
        public Data(string id, int rank, int level, string nickname, int point, string countryCode, long timeStamp = 0)
        {
            this.id = id;
            this.rank = rank;
            this.level = level;
            this.nickname = nickname;
            this.point = point;
            this.countryCode = countryCode;
            this.timeStamp = timeStamp == 0 ? GameManager.Instance.dateTime.Value.ToTick() : timeStamp;
        }
    }
}
