using UnityEngine;

public class QuestList : CustomList<QuestList.Data>
{
    public class Data
    {
        public GameData.Quest gameData;
        public UserData.Quest.Data userData;

        public string GetName()
        {
            if(gameData.type == GameData.QuestType.useGoods)
            {
                return string.Format(TextManager.Get($"QuestText_{gameData.type.ToString()}"), TextManager.Get(((GameData.GoodsType)gameData.questId).ToString()));
            }
            if(gameData.type == GameData.QuestType.clearQuest)
            {
                return string.Format(TextManager.Get($"QuestText_{gameData.type.ToString()}"), TextManager.Get(gameData.category.ToString()).ToString());
            }
            return TextManager.Get($"QuestText_{gameData.type.ToString()}");
        }
    }
}
