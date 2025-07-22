using UnityEngine;
using UnityEngine.UI;

public class PopupResult : Popup
{
    [SerializeField]
    private TMPro.TextMeshProUGUI scoreText;

    //[SerializeField]
    //private TMPro.TextMeshProUGUI expText;
    //[SerializeField]
    //private TMPro.TextMeshProUGUI goldText;

    [SerializeField]
    private GoodsDisplay exp;
    [SerializeField]
    private GoodsDisplay gold;

    [SerializeField]
    private GameObject vipObj;

    [SerializeField]
    private GameObject adObj;
    [SerializeField]
    private GoodsDisplay expAd;
    [SerializeField]
    private GoodsDisplay goldAd;

    public void SetData(int point, int exp, int gold)
    {
        scoreText.text = point.ToString();
        this.exp.SetStaticValue(GameData.GoodsType.EXP, exp);
        this.gold.SetStaticValue(GameData.GoodsType.Gold, gold);

        if(DataManager.Instance.userData.isVIP)
        {
            vipObj.SetActive(true);
            adObj.SetActive(false);
        }
        else
        {
            vipObj.SetActive(false);
            adObj.SetActive(true);
            expAd.SetStaticValue(GameData.GoodsType.EXP, exp);
            goldAd.SetStaticValue(GameData.GoodsType.Gold, gold);
        }
        //expText.text = exp.ToString();
        //goldText.text = gold.ToString();
    }

    public override void Close()
    {
        base.Close();
        GameManager.Instance.GoScene(GameManager.Scene.Main);
    }

    public void ClckAdReward()
    {
        base.Close();
        ADManager.Instance.ShowReward(delegate(bool success)
        {
            if (success)
            {
                GoodsList.Data[] rewards = new GoodsList.Data[2];
                rewards[0] = new GoodsList.Data();
                rewards[0].type = GameData.GoodsType.EXP;
                rewards[0].amount = expAd.amount;
                rewards[1] = new GoodsList.Data();
                rewards[1].type = GameData.GoodsType.Gold;
                rewards[1].amount = goldAd.amount;

                UIManager.Instance.Open<PopupReward>(delegate () { GameManager.Instance.GoScene(GameManager.Scene.Main); }).SetData(rewards);
                DataManager.Instance.userData.Charge(rewards);
            }
            else
            {
                GameManager.Instance.GoScene(GameManager.Scene.Main);
            }
                
        });
    }
}
