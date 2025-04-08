using GameData;
using UnityEngine;
using UnityEngine.UI;

public class LevelListItem : ListItem<GameLevelInfo>
{
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

    public override void SetData(GameLevelInfo data)
    {
        base.SetData(data);

        levelText.text = data.level.ToString();
        switch(data.level)
        {
            case PuzzleManager.Level.Easy:
                levelText.color = Color.green;
                break;
            case PuzzleManager.Level.Normal:
                levelText.color = Color.yellow;
                break;
            case PuzzleManager.Level.Hard:
                levelText.color = Color.red;
                break;
            case PuzzleManager.Level.Expert:
                levelText.color = Color.cyan;
                break;
        }
        sizeText.text = $"{data.column} x {data.row}";
        timeText.text = data.time.ToTimeText();

        if(DataManager.Instance.userData.level < data.unlockLevel)
        {
            lockObj.SetActive(true);
            unlockCondition.text = string.Format(TextManager.Get("unlockConditionLevel"), data.unlockLevel);
        }
        else
        {
            lockObj.SetActive(false);
        }
    }
}
