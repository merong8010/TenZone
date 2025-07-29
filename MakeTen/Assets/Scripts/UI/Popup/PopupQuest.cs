using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PopupQuest : Popup
{
    [SerializeField]
    private TabGroup currentTab;
    [SerializeField]
    private QuestList list;
    [SerializeField]
    private Text remainTimeText;

    private int currentIdx;
    public override void Open()
    {
        base.Open();
        currentTab.Init(0, idx =>
        {
            currentIdx = idx;
            Refresh();
        });
        list.SetEvent(data =>
        {
            if (DataManager.Instance.userData.ReceiveQuestReward(data.gameData.id))
                Refresh();
        });
    }

    public override void Refresh()
    {
        base.Refresh();
        GameData.QuestCategory category = (GameData.QuestCategory)(currentIdx+1);
        var questDatas = DataManager.Instance.Get<GameData.Quest>().Where(x => x.category == category);
        QuestList.Data[] datas = new QuestList.Data[questDatas.Count()];
        int idx = 0;
        foreach(var questData in questDatas)
        {
            datas[idx] = new QuestList.Data();
            datas[idx].gameData = questData;
            datas[idx].userData = DataManager.Instance.userData.GetQuest(questData.id);
            idx++;
        }
        
        list.UpdateList(datas.OrderBy(x => x.userData.isRewardClaimed).ThenByDescending(x => (float)x.userData.count / x.gameData.questCount).ToArray());
        disposable?.Dispose();
        switch(category)
        {
            case GameData.QuestCategory.daily:
                disposable = GameManager.Instance.reactiveTime.Subscribe(x =>
                {
                    remainTimeText.text = string.Format(TextManager.Get("ResetRemainTime"), x.RemainTimeNextDay().ToHourTimeText());
                });
                break;
            case GameData.QuestCategory.weekly:
                disposable = GameManager.Instance.reactiveTime.Subscribe(x =>
                {
                    remainTimeText.text = string.Format(TextManager.Get("ResetRemainTime"), x.RemainTimeNextWeek().ToDateTimeText());
                });
                break;
            default:
                remainTimeText.text = "";
                break;
        }
    }

    private IDisposable disposable;
}
