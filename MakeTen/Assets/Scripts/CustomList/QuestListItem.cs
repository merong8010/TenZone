using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestListItem : ListItem<QuestList.Data>
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private TextMeshProUGUI countText;
    [SerializeField]
    private Image progressImage;
    [SerializeField]
    private GoodsList rewards;
    [SerializeField]
    private GameObject buttonDisable;
    [SerializeField]
    private GameObject buttonEnable;
    [SerializeField]
    private GameObject completeObj;

    public override void SetData(QuestList.Data data)
    {
        base.SetData(data);
        nameText.text = data.GetName();
        countText.text = data.userData.count.ToProgressText(data.gameData.questCount);
        if(data.gameData.questCount <= data.userData.count)
        {
            buttonEnable.SetActive(true);
            buttonDisable.SetActive(false);
        }
        else
        {
            buttonEnable.SetActive(false);
            buttonDisable.SetActive(true);
        }
        completeObj.SetActive(data.userData.isRewardClaimed);
        progressImage.fillAmount = (float)data.userData.count / data.gameData.questCount;
        rewards.UpdateList(data.gameData.rewards);
    }
}
