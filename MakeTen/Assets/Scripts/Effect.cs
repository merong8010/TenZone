using UnityEngine;

public class Effect : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI text;

    public void SetText(string text)
    {
        this.text.text = text;
    }
}
