using UnityEngine;

namespace GameData
{
    public class Data
    {

    }
    public class Config : Data
    {
        public int HeartChargeTime;
        public int MaxHeart;
        public int PuzzleTime;
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
}
