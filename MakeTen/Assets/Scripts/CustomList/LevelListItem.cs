using GameData;
using UnityEngine;
using UnityEngine.UI;

public class LevelListItem : ListItem<GameLevel>
{
    [SerializeField]
    private Color[] levelColors;
    [SerializeField]
    private Text levelText;
    [SerializeField]
    private Text sizeText;
    [SerializeField]
    private Text timeText;
    [SerializeField]
    private GameObject lockObj;
    [SerializeField]
    private Text unlockCondition;

    public override void SetData(GameLevel data)
    {
        base.SetData(data);

        levelText.text = TextManager.Get(data.level.ToString());
        //switch(data.level)
        //{
        //    case PuzzleManager.Level.Easy:
        //        //levelText.color = Color.green;

        //        break;
        //    case PuzzleManager.Level.Normal:
        //        levelText.color = Color.yellow;
        //        break;
        //    case PuzzleManager.Level.Hard:
        //        levelText.color = Color.red;
        //        break;
        //    case PuzzleManager.Level.Expert:
        //        levelText.color = Color.cyan;
        //        break;
        //}
        levelText.color = levelColors[(int)data.level];

        sizeText.text = $"{data.column} x {data.row}";
        timeText.text = data.time.ToTimeText();

        if(DataManager.Instance.userData.Info.level < data.unlockLevel)
        {
            lockObj.SetActive(true);
            unlockCondition.text = string.Format(TextManager.Get("UnlockConditionLevel"), data.unlockLevel);
        }
        else
        {
            lockObj.SetActive(false);
        }
    }
}
