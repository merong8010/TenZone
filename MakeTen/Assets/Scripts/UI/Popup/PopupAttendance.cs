using UnityEngine;
using System.Linq;

public class PopupAttendance : Popup
{
    [SerializeField]
    private AttendanceList list;

    private bool isInit;
    private void Init()
    {
        if (isInit) return;
        isInit = true;
        list.SetEvent(ClickAttendanceListItem);
    }

    public void ClickAttendanceListItem(GameData.Attendance data)
    {
        int dateMax = DataManager.Instance.Get<GameData.Attendance>().Max(x => x.date);
        int currentIdx = DataManager.Instance.userData.attendanceCount % dateMax;
        if (DataManager.Instance.userData.attendanceRewardDate == GameManager.Instance.dateTime.Value.ToDateText())
        {
            currentIdx -= 1;
        }
        if (data.date-1 == currentIdx)
        {
            if(DataManager.Instance.userData.IsAttendanceRewardable)
            {
                DataManager.Instance.userData.RewardAttendacne();
                for (int i = 0; i < data.rewards.Length; i++)
                {
                    DataManager.Instance.userData.Charge(data.rewards[i].type, data.rewards[i].amount);
                }

                UIManager.Instance.Open<PopupReward>().SetData(data.rewards);
                Refresh();
            }
            else if(!DataManager.Instance.userData.IsRewardAttendanceAd)
            {
                ADManager.Instance.ShowReward(result =>
                {
                    DataManager.Instance.userData.RewardAttendanceAd();
                    for (int i = 0; i < data.rewards.Length; i++)
                    {
                        DataManager.Instance.userData.Charge(data.rewards[i].type, data.rewards[i].amount);
                    }

                    UIManager.Instance.Open<PopupReward>().SetData(data.rewards);
                    Refresh();
                });
            }
            else
            {
                UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AlreadyRewarded"));
            }
        }
        else if(data.date-1 < currentIdx)
        {
            UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("AlreadyRewarded"));
        }
        else
        {
            UIManager.Instance.Message.Show(Message.Type.Simple, TextManager.Get("NeedMoreAttendace"));
        }
    }

    public override void Open()
    {
        Init();
        base.Open();
    }

    public override void Refresh()
    {
        base.Refresh();
        list.UpdateList(DataManager.Instance.Get<GameData.Attendance>());
    }
}
