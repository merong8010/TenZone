using UnityEngine;
using UnityEngine.UI;

public class MailListItem : ListItem<MailList.Data>
{
    [SerializeField]
    private Text titleText;
    [SerializeField]
    private Text descText;

    [SerializeField]
    private GoodsList rewardList;

    private Text timeText;

    public override void SetData(MailList.Data data)
    {
        base.SetData(data);

        titleText.text = TextManager.Get(data.title);
        descText.text = TextManager.Get(data.desc);

        rewardList.UpdateList(data.rewards);
        timeText.text = data.receiveDate.ToTimeText();
    }
}
