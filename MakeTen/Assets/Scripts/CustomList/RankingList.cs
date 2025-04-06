using System.Linq;
using UnityEngine;

public class RankingList : CustomList<RankingList.Data>
{
    public class Data
    {
        public int rank;
        public string name;
        public int score;
        public string countryCode;

        public Data(string name, int score, string countryCode)
        {
            this.name = name;
            this.score = score;
            this.countryCode = countryCode;
        }
        public Data(int rank, string name, int score, string countryCode)
        {
            this.rank = rank;
            this.name = name;
            this.score = score;
            this.countryCode = countryCode;
        }
    }

    public void UpdateFlags(string countryCode, Sprite flag)
    {
        listItems.Where(x => x.GetData().countryCode == countryCode).ToList().ForEach(x => ((RankingListItem)x).UpdateFlag(flag));
        //for (int i = 0; i < .Count; i++)
        //{
            
        //}
    }
}
