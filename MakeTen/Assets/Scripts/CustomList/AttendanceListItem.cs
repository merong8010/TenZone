using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameData;
using System.Linq;

public class AttendanceListItem : ListItem<GameData.Attendance>
{
    [SerializeField]
    private TextMeshProUGUI dateText;
    [SerializeField]
    private GoodsList rewardList;
    [SerializeField]
    private GameObject rewardedObj;

    public override void SetData(Attendance data)
    {
        base.SetData(data);
        int dateMax = DataManager.Instance.Get<Attendance>().Max(x => x.date);
        int currentIdx = DataManager.Instance.userData.attendanceCount % dateMax;
        if(data.date-1 == currentIdx)
        {
            //today
            if (DataManager.Instance.userData.attendanceRewardDate == GameManager.Instance.dateTime.Value.ToDateText())
            {
                rewardedObj.SetActive(true);
            }
            else
            {
                rewardedObj.SetActive(false);
            }
        }
        else if(data.date - 1 < currentIdx)
        {
            //fast
            rewardedObj.SetActive(true);
        }
        else
        {
            //future
            rewardedObj.SetActive(false);
        }

        dateText.text = data.date.ToString();
        rewardList.UpdateList(data.rewards);
    }
}
