using UnityEngine;

public class GoodsList : CustomList<GoodsList.Data>
{
    public class Data
    {
        public GameData.GoodsType type;
        public int amount;
    }
}
