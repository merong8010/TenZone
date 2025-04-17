using UnityEngine;
using UnityEngine.UI;

public class PopupNickname : Popup
{
    [SerializeField]
    private InputField nicknameInput;
    [SerializeField]
    private Text resultText;

    [SerializeField]
    private GoodsDisplay cost;
    [SerializeField]
    private GameObject free;

    public override void Open()
    {
        base.Open();
        nicknameInput.text = DataManager.Instance.userData.nickname;
        resultText.text = string.Empty;

        Refresh();
    }

    public override void Refresh()
    {
        base.Refresh();

        if (DataManager.Instance.userData.nicknameChangeCount == 0)
        {
            cost.gameObject.SetActive(false);
            free.SetActive(true);
        }
        else
        {
            cost.gameObject.SetActive(true);
            cost.Set(DataManager.Instance.GetConfigGoodsType("nicknameChangeCostType"), DataManager.Instance.GetConfig("nicknameChangeCostAmount"));
            free.SetActive(false);
        }
    }

    public void ClickChange()
    {
        if (DataManager.Instance.userData.nicknameChangeCount > 0 && !DataManager.Instance.userData.Has(DataManager.Instance.GetConfigGoodsType("nicknameChangeCostType"), DataManager.Instance.GetConfig("nicknameChangeCostAmount")))
        {
            return;
        }
        FirebaseManager.Instance.CheckNickname(nicknameInput.text, result =>
        {
            if (result.success)
            {
                FirebaseManager.Instance.UpdateNickName(nicknameInput.text, changeCallback =>
                {
                    if (changeCallback.success)
                    {
                        DataManager.Instance.userData.nicknameChangeCount += 1;

                        if (DataManager.Instance.userData.nicknameChangeCount > 1)
                        {
                            DataManager.Instance.userData.Use(DataManager.Instance.GetConfigGoodsType("nicknameChangeCostType"), DataManager.Instance.GetConfig("nicknameChangeCostAmount"));
                        }
                    }
                    resultText.text = result.message;
                    resultText.color = result.success ? Color.green : Color.red;

                    Refresh();
                });
                
            }
            else
            {
                resultText.text = result.message;
                resultText.color = Color.red;

                Refresh();
            }
        });


    }
}