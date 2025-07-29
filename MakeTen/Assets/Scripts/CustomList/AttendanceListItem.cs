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
    [SerializeField]
    private GameObject rewardButtonObj;
    [SerializeField]
    private GameObject adButtonObj;
    [SerializeField]
    private GameObject needMoreAttendance;

    public override void SetData(Attendance data)
    {
        base.SetData(data);
        int dateMax = DataManager.Instance.Get<Attendance>().Max(x => x.date);
        int currentIdx = DataManager.Instance.userData.Attendance.count % dateMax;
        if(DataManager.Instance.userData.Attendance.rewardDate == GameManager.Instance.dateTime.Value.ToDateText())
        {
            currentIdx -= 1;
        }

        if(data.date-1 == currentIdx)
        {
            //today
            if (DataManager.Instance.userData.IsAttendanceRewardable)
            {
                rewardedObj.SetActive(false);
                rewardButtonObj.SetActive(true);
                adButtonObj.SetActive(false);
            }
            else if(!DataManager.Instance.userData.Attendance.isRewardAd)
            {
                rewardedObj.SetActive(false);
                rewardButtonObj.SetActive(false);
                adButtonObj.SetActive(true);
            }
            else
            {
                rewardedObj.SetActive(true);
                rewardButtonObj.SetActive(true);
                adButtonObj.SetActive(false);
            }
            needMoreAttendance.SetActive(false);
        }
        else if(data.date - 1 < currentIdx)
        {
            //fast
            rewardButtonObj.SetActive(true);
            adButtonObj.SetActive(false);
            rewardedObj.SetActive(true);
            needMoreAttendance.SetActive(false);
        }
        else
        {
            //future
            rewardButtonObj.SetActive(true);
            adButtonObj.SetActive(false);
            rewardedObj.SetActive(false);
            needMoreAttendance.SetActive(true);
        }

        dateText.text = data.date.ToString();
        rewardList.UpdateList(data.rewards);
    }
}
