using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PopupMail : Popup
{
    [SerializeField]
    private MailList mailList;
    [SerializeField]
    private GameObject noMailObj;

    private bool isInit = false;
    private void Init()
    {
        if (isInit) return;
        isInit = true;
        mailList.SetEvent(ClickMailItem);
    }
    public override void Open()
    {
        Init();
        base.Open();
    }

    public override void Refresh()
    {
        base.Refresh();

        if (DataManager.Instance.userData.Mail.datas != null && DataManager.Instance.userData.Mail.datas.Count > 0)
        {
            mailList.gameObject.SetActive(true);
            noMailObj.SetActive(false);
            MailList.Data[] datas = new MailList.Data[DataManager.Instance.userData.Mail.datas.Count];
            int idx = 0;
            foreach(KeyValuePair<string,MailList.Data> pair in DataManager.Instance.userData.Mail.datas)
            {
                datas[idx] = pair.Value;
                datas[idx].id = pair.Key;
                idx++;
            }
            mailList.UpdateList(datas);
        }
        else
        {
            mailList.gameObject.SetActive(false);
            noMailObj.SetActive(true);
        }
    }

    public void ClickMailItem(MailList.Data data)
    {
        if(DataManager.Instance.userData.Mail.datas.ContainsKey(data.id))
        {
            DataManager.Instance.userData.Mail.datas.Remove(data.id);
            DataManager.Instance.userData.Mail.MarkAsDirty();
            if(data.rewards != null)
            {
                for (int i = 0; i < data.rewards.Length; i++)
                {
                    DataManager.Instance.userData.Charge(data.rewards[i].type, data.rewards[i].amount);
                }
                UIManager.Instance.Open<PopupReward>().SetData(data.rewards.ToList());
            }

            Refresh();
        }
            
    }
}
