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
        Search,
        Time_10s,
    }

    public enum ShopCostType
    {
        None,
        Free,
        Ads,
        Cash,
        Goods,
    }

    public enum TimePeriod
    {
        None,
        Daily,
        Weekly,
        Monthly,
        Yeary,
        TotalSeconds,
    }

    public class Data
    {

    }

    public class RewardData : Data
    {
        public string rewardTypes;
        public string rewardAmounts;
        private GoodsList.Data[] _rewards;
        public GoodsList.Data[] rewards
        {
            get
            {
                if (_rewards == null)
                {
                    string[] types = rewardTypes.Split(',');
                    string[] amounts = rewardAmounts.Split(',');
                    if (types.Length == amounts.Length)
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
    }

    public class Config : Data
    {
        public int id;
        public string key;
        public string val;

        public int Get()
        {
            if(int.TryParse(val, out int result))
            {
                return result;
            }
            return 0;
        }
        public GoodsType GetGoodsType()
        {
            if(System.Enum.TryParse<GoodsType>(val, out GoodsType type))
            {
                return type;
            }
            return GoodsType.None;
        }
    }

    public class Language : Data
    {
        public string key;
        public string KR;
        public string US;
        public string JP;
        public string TW;
    }

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
        public int bonusTimeMin;
        public int bonusTimeMax;
        public int bonusCount;
    }

    public class UserLevel : RewardData
    {
        public int level;
        public int exp;
    }

    public class ForbiddenWord : Data
    {
        public string word;
    }
    public class ShopData : RewardData
    {
        public string id;
        public ShopCostType costType;
        public GoodsType goodsType;
        public int costAmount;
        public TimePeriod buyPeriod;
        public int buyMaxCount;
        public int unlockLevel;
        public int unlockShopId;
    }
}
