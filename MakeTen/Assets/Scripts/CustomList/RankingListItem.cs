using UnityEngine;
using static FirebaseManager;
using UnityEngine.UI;
using UnityEngine.U2D;

public class RankingListItem : ListItem<RankingList.Data>
{
    [SerializeField]
    private Text rankText;
    [SerializeField]
    private Text idText;
    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private Image countryImage;
    public override void SetData(RankingList.Data data)
    {
        base.SetData(data);
        rankText.text = data.rank.ToString();
        idText.text = data.name;
        scoreText.text = data.score.ToString();
        countryImage.sprite = Resources.Load<SpriteAtlas>("Graphics/Flags").GetSprite(data.countryCode.ToLower());
        //DataManager.Instance.GetFlags(data.countryCode, flagSprite => countryImage.sprite = flagSprite);
    }

    public void UpdateFlag(Sprite flag)
    {
        countryImage.sprite = flag;
    }
}
