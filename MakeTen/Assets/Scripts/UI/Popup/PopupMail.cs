using UnityEngine;

public class PopupMail : Popup
{
    [SerializeField]
    private MailList mailList;
    [SerializeField]
    private GameObject noMailObj;

    public override void Open()
    {
        base.Open();
    }

    public override void Refresh()
    {
        base.Refresh();

        if (DataManager.Instance.userData.mailDatas.Length > 0)
        {
            mailList.gameObject.SetActive(true);
            noMailObj.SetActive(false);
            mailList.UpdateList(DataManager.Instance.userData.mailDatas);
        }
        else
        {
            mailList.gameObject.SetActive(false);
            noMailObj.SetActive(true);
        }
    }
}
