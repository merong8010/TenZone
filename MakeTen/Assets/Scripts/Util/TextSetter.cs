using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.UI;
[RequireComponent(typeof(Text))]
public class TextSetter : MonoBehaviour
{
    [SerializeField]
    private Text text;
    [SerializeField]
    private string key;

    private bool isInit = false;
    private void Init()
    {
        if (isInit) return;
        isInit = true;
        if (text == null) text = GetComponent<Text>();
    }

    private void OnEnable()
    {
        Init();
        Refresh();
    }

    public void Refresh()
    {
        text.text = TextManager.Get(key);
    }
}
