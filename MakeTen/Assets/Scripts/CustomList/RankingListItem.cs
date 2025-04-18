using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using TMPro;

public class RankingListItem : ListItem<RankingList.Data>
{
    [SerializeField]
    private TextMeshProUGUI rankText;
    [SerializeField]
    private TextMeshProUGUI levelText;
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private TextMeshProUGUI pointText;
    [SerializeField]
    private TextMeshProUGUI remainTimeText;
    [SerializeField]
    private TextMeshProUGUI timeStampText;
    [SerializeField]
    private Image countryImage;
    public override void SetData(RankingList.Data data)
    {
        base.SetData(data);

        if(data.GetType() == typeof(RankingList.PointData))
        {
            RankingList.PointData pointData = (RankingList.PointData)data;
            pointText.text = pointData.point.ToString();
            remainTimeText.text = pointData.remainMilliSeconds.MilliSecondsToTimeText();
        }
        else
        {
            RankingList.LevelData levelData = (RankingList.LevelData)data;
            pointText.text = levelData.exp.ToString();
        }
        
        rankText.text = data.rank.ToString();
        levelText.text = data.level.ToLevelText();
        nameText.text = data.name;
        SpriteAtlas sa = Resources.Load<SpriteAtlas>("Graphics/Flags");
        Sprite flag = sa.GetSprite(data.countryCode.ToLower());
        Debug.Log(sa + " |" + flag + " | " + data.countryCode.ToLower()+" | "+ sa.spriteCount);
        
        //countryImage.sprite = Resources.Load<SpriteAtlas>("Graphics/Flags").GetSprite(data.countryCode.ToLower());
        countryImage.sprite = flag;
        timeStampText.text = data.timeStamp.ToTimeText();
        //DataManager.Instance.GetFlags(data.countryCode, flagSprite => countryImage.sprite = flagSprite);
    }

    public void UpdateFlag(Sprite flag)
    {
        countryImage.sprite = flag;
    }
}
