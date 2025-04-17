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

        pointText.text = data.point.ToString();
        remainTimeText.text = data.remainMilliSeconds.MilliSecondsToTimeText();

        rankText.text = data.rank.ToString();
        levelText.text = data.level.ToLevelText();
        nameText.text = data.name;
        countryImage.sprite = Resources.Load<SpriteAtlas>("Graphics/Flags").GetSprite(data.countryCode.ToLower());
        timeStampText.text = data.timeStamp.ToTimeText();
        //DataManager.Instance.GetFlags(data.countryCode, flagSprite => countryImage.sprite = flagSprite);
    }

    public void UpdateFlag(Sprite flag)
    {
        countryImage.sprite = flag;
    }
}
