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
    public class Config : Data
    {
        public int HeartChargeTime;
        public int MaxHeart;
        public int PuzzleTime;
        public int defaultGold;
        public int defaultGem;
        public int defaultShuffle;
    }

    public class LanguageInfo
    {
        public string key;
        public string KR;
        public string US;
        public string JP;
        public string TW;
    }

    public class Language : Data
    {
        public LanguageInfo[] Vals;
    }

    public class GameLevelInfo
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

    public class GameLevel : Data
    {
        public GameLevelInfo[] Vals;
    }

    public class UserLevelInfo
    {
        public int level;
        public int exp;
        public GoodsList.Data[] rewards;
    }

    public class UserLevel : Data
    {
        public UserLevelInfo[] Vals;
    }

    public class ForbiddenWordInfo
    {
        public string word;
    }

    public class ForbiddenWord : Data
    {
        public ForbiddenWordInfo[] Vals;
    }
}
