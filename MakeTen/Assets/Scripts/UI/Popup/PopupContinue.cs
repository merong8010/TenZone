using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupContinue : Popup
{
    [SerializeField]
    private TextMeshProUGUI countText;
    [SerializeField]
    private Text descText;
    [SerializeField]
    private GoodsDisplay cost;

    private Coroutine countCoroutine;
    public override void Open()
    {
        base.Open();
    }

    public void SetData(GameData.Continue data)
    {
        descText.text = string.Format(TextManager.Get("AskContinue"), data.addSeconds);
        cost.SetStaticValue(data.goodsType, data.goodsAmount);
        countCoroutine = StartCoroutine(CountTime());
    }

    private IEnumerator CountTime()
    {
        int count = 10;
        countText.text = count.ToString();
        while (count >= 0)
        {
            yield return Yielders.Get(1f);
            count--;
            countText.text = count.ToString();
        }
        ClickNo();
    }

    public void ClickContinue()
    {
        Close();
        PuzzleManager.Instance.ContinueGame();
    }

    public void ClickNo()
    {
        Close();
        PuzzleManager.Instance.GameResult();
    }
}