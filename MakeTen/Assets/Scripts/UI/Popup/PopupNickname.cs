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
    public override void Open()
    {
        base.Open();
        resultText.text = string.Empty;
    }

    public override void Refresh()
    {
        base.Refresh();
        Debug.Log($"PopupNickname.Refresh | {DataManager.Instance.userData.Info.nickname} | {DataManager.Instance.userData.Info.nicknameChangeCount}");
        nicknameInput.text = DataManager.Instance.userData.Info.nickname;
        
        if (DataManager.Instance.userData.Info.nicknameChangeCount == 0)
        {
            cost.SetCost(GameData.ShopCostType.Free, GameData.GoodsType.None, 0);
        }
        else
        {
            cost.SetStaticValue(DataManager.Instance.GetConfigGoodsType("nicknameChangeCostType"), DataManager.Instance.GetConfig("nicknameChangeCostAmount"));
        }
    }

    public void ClickChange()
    {
        if (DataManager.Instance.userData.Info.nicknameChangeCount > 0 && !DataManager.Instance.userData.Has(DataManager.Instance.GetConfigGoodsType("nicknameChangeCostType"), DataManager.Instance.GetConfig("nicknameChangeCostAmount")))
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
                        DataManager.Instance.userData.CountNicknameChange();
                        
                        if (DataManager.Instance.userData.Info.nicknameChangeCount > 1)
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