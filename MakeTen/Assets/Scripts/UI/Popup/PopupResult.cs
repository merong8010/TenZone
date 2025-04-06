using UnityEngine;
using UnityEngine.UI;

public class PopupResult : Popup
{
    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private Text timeText;

    public void SetData(int point, float time)
    {
        scoreText.text = point.ToString();
        timeText.text = time.ToString("n0");

        FirebaseManager.Instance.SubmitScore(point);
    }
}
