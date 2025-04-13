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
            defaultText.text = defaultFocusText.text = bonusText.text = bonusFocusText.text = _num.ToString();
            _num.ToString();
            numObj.SetActive(_num > 0);
            Focus(false);
        }
        get
        {
            if (!numObj.activeSelf) return 0;
            return _num;
        }
    }

    public int bonus { private set; get; }

    [SerializeField]
    private GameObject numObj;
    //[SerializeField]
    //private Text numText;
    //[SerializeField]
    //private Color defaultColor;
    //[SerializeField]
    //private Color focusColor;
    [SerializeField]
    private RectTransform rectTransform;

    [SerializeField]
    private GameObject defaultObj;
    [SerializeField]
    private Text defaultText;
    [SerializeField]
    private GameObject defaultFocusObj;
    [SerializeField]
    private Text defaultFocusText;

    [SerializeField]
    private GameObject bonusObj;
    [SerializeField]
    private Text bonusText;
    [SerializeField]
    private GameObject bonusFocusObj;
    [SerializeField]
    private Text bonusFocusText;

    public void SetSize(Vector2 size)
    {
        rectTransform.sizeDelta = size;
    }
    public void Reset()
    {
        this.num = 0;
    }

    public void Init(int num, int bonus = 0)
    {
        this.num = num;
        this.bonus = bonus;
        if(bonus > 0)
        {
            bonusObj.SetActive(bonus > 0);
            defaultObj.SetActive(bonus == 0);
            bonusFocusObj.SetActive(false);
            defaultFocusObj.SetActive(false);
        }
    }


    public void Focus(bool isFocus)
    {
        if (!numObj.activeSelf) return;
        if(bonus > 0)
        {
            bonusObj.SetActive(!isFocus);
            bonusFocusObj.SetActive(isFocus);
        }
        else
        {
            defaultObj.SetActive(!isFocus);
            defaultFocusObj.SetActive(isFocus);
        }
    }

    public void Break()
    {
        numObj.SetActive(false);
        ObjectPooler.Instance.GetObject<Effect>("block_break", PuzzleManager.Instance.transform, position : transform.localPosition, autoReturnTime: 1f);
        if(bonus > 0)
        {
            ObjectPooler.Instance.GetObject<Effect>("block_bonus", PuzzleManager.Instance.transform, transform.localPosition, autoReturnTime: 1f);
        }
        
    }
    public void InitRandom()
    {
        StartCoroutine(RandomAnimation());
    }

    [SerializeField]
    private float randomChangeMin = 3f;
    [SerializeField]
    private float randomChangeMax = 10f;

    private IEnumerator RandomAnimation()
    {
        GameObject[] objs = new GameObject[4];
        objs[0] = defaultObj;
        objs[1] = defaultFocusObj;
        objs[2] = bonusObj;
        objs[3] = bonusFocusObj;
        int ran = Random.Range(1, 9);
        defaultText.text = defaultFocusText.text = bonusText.text = bonusFocusText.text = ran.ToString();

        while (gameObject.activeInHierarchy)
        {
            int ranShow = Random.Range(0, 4);
            for (int i = 0; i < objs.Length; i++)
            {
                objs[i].SetActive(i == ranShow);
            }

            yield return Yielders.Get(Random.Range(randomChangeMin, randomChangeMax));
        }
    }

}
