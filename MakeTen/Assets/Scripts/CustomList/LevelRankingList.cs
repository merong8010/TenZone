public class LevelRankingList : InfiniteScroll<LevelRankingList.Data>
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
        public int exp;
        public Data(string id, int level, string name, int exp, string countryCode, long timeStamp = 0)
        {
            this.id = id;
            this.level = level;
            this.name = name;
            this.exp = exp;
            this.countryCode = countryCode;
            this.timeStamp = timeStamp == 0 ? GameManager.Instance.dateTime.Value.ToTick() : timeStamp;
        }
        public Data(string id, int rank, int level, string name, int exp, string countryCode, long timeStamp = 0)
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