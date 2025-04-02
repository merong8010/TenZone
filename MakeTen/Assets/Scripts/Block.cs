using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    private int _num;
    public int num
    {
        private set
        {
            _num = value;
            numText.text = _num.ToString();
        }
        get
        {
            if (!numObj.activeSelf) return 0;
            return _num;
        }
    }

    [SerializeField]
    private GameObject numObj;
    [SerializeField]
    private Text numText;
    [SerializeField]
    private Color defaultColor;
    [SerializeField]
    private Color focusColor;
    [SerializeField]
    private RectTransform rectTransform;

    public void SetSize(Vector2 size)
    {
        rectTransform.sizeDelta = size;
    }
    public void Reset()
    {
        this.num = 0;
    }

    public void Init(int num)
    {
        numObj.SetActive(true);
        Focus(false);

        this.num = num;
    }

    public void Focus(bool isFocus)
    {
        if (!numObj.activeSelf) return;
        numText.color = isFocus ? focusColor : defaultColor;
    }

    public void Break()
    {
        numObj.SetActive(false);
    }

}
