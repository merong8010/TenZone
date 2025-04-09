using UnityEngine;
using System.Collections.Generic;
namespace GameData
{
    public enum GoodsType
    {
        None,
        Gold,
        Gem,
        Shuffle,
    }

    public class Data
    {

    }
    //public class Config : Data
    //{
    //    public int HeartChargeTime;
    //    public int MaxHeart;
    //    public int PuzzleTime;
    //    public int defaultGold;
    //    public int defaultGem;
    //    public int defaultShuffle;
    //}

    public class Config : Data
    {
        public int id;
        public string key;
        public int val;
    }

    public class Language : Data
    {
        public string key;
        public string KR;
        public string US;
        public string JP;
        public string TW;
    }

    //public class Language : Data
    //{
    //    public LanguageInfo[] Vals;
    //}

    public class GameLevel : Data
    {
        public PuzzleManager.Level level;
        public int row;
        public int column;
        public float mean;
        public float stdDev;
        public int time;
        public int exp;
        public int unlockLevel;
    }

    //public class GameLevel : Data
    //{
    //    public GameLevelInfo[] Vals;
    //}

    public class UserLevel : Data
    {
        public int level;
        public int exp;
        public string rewardTypes;
        public string rewardAmounts;
        private GoodsList.Data[] _rewards;
        public GoodsList.Data[] rewards
        {
            get
            {
                if(_rewards == null)
                {
                    string[] types = rewardTypes.Split(',');
                    string[] amounts = rewardAmounts.Split(',');
                    if(types.Length == amounts.Length)
                    {
                        _rewards = new GoodsList.Data[types.Length];
                        for (int i = 0; i < types.Length; i++)
                        {
                            _rewards[i] = new GoodsList.Data();
                            _rewards[i].type = (GoodsType)System.Enum.Parse(typeof(GoodsType), types[i]);
                            _rewards[i].amount = int.Parse(amounts[i]);
                        }
                    }
                }
                return _rewards;
            }
        }
        //private Dictionary<GoodsType, int> _rewards;
        //public Dictionary<GoodsType, int> rewards
        //{
        //    get
        //    {
        //        if(_rewards == null)
        //        {
        //            _rewards = new Dictionary<GoodsType, int>();
        //            string[] types = rewardTypes.Split(',');
        //            string[] amounts = rewardAmounts.Split(',');
        //            for(int i = 0; i < types.Length; i++)
        //            {
        //                _rewards.Add((GoodsType)System.Enum.Parse(typeof(GoodsType), types[i]), int.Parse(amounts[i]));
        //            }
        //        }
        //        return _rewards;
        //    }
        //}
    }

    //public class UserLevel : Data
    //{
    //    public UserLevelInfo[] Vals;
    //}

    public class ForbiddenWord : Data
    {
        public string word;
    }

    //public class ForbiddenWord : Data
    //{
    //    public ForbiddenWordInfo[] Vals;
    //}
}
