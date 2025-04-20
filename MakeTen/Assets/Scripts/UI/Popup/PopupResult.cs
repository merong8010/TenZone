using UnityEngine;
using UnityEngine.UI;

public class PopupResult : Popup
{
    [SerializeField]
    private TMPro.TextMeshProUGUI scoreText;
    
    [SerializeField]
    private TMPro.TextMeshProUGUI expText;

    public void SetData(int point, int exp)
    {
        scoreText.text = point.ToString();
        expText.text = exp.ToString();
    }

    public override void Close()
    {
        base.Close();
        GameManager.Instance.GoScene(GameManager.Scene.Main);
    }
}
