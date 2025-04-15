using UnityEngine;
using UnityEngine.UI;

public class PopupResult : Popup
{
    [SerializeField]
    private TMPro.TextMeshProUGUI scoreText;
    [SerializeField]
    private TMPro.TextMeshProUGUI timeText;

    [SerializeField]
    private TMPro.TextMeshProUGUI expText;

    public void SetData(int point, int time, int exp)
    {
        scoreText.text = point.ToString();
        timeText.text = time.MilliSecondsToTimeText();
        expText.text = exp.ToString();
    }

    public override void Close()
    {
        base.Close();
        GameManager.Instance.GoScene(GameManager.Scene.Main);
    }
}
