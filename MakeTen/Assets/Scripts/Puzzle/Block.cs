using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    //private int _num;
    public int num
    {
        private set
        {
            //_num = value;
            //defaultText.text = defaultFocusText.text = bonusText.text = bonusFocusText.text = _num.ToString();
            //_num.ToString();
            //numObj.SetActive(_num > 0);
            //Focus(false);
        }
        get
        {
            if (!numObj.activeSelf) return 0;
            return data.num;
            //return _num;
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

    public class Data
    {
        public int num;
        public int bonus;
        public bool shuffle;

        public Data(GameData.GameLevel level)
        {
            num = Util.GenerateGaussianRandom(level.mean, level.stdDev);
            bonus = level.bonusRate.IsSuccess() ? Random.Range(level.bonusTimeMin, level.bonusTimeMax) : 0;
            shuffle = level.shuffleRate.IsSuccess();
        }
    }
    private Data data;
    public void SetData(Data data)
    {
        this.data = data;

        defaultText.text = defaultFocusText.text = bonusText.text = bonusFocusText.text = data.num.ToString();
        numObj.SetActive(data.num > 0);
        bonusObj.SetActive(data.bonus > 0);
        defaultObj.SetActive(data.bonus == 0);
        Focus(false);
    }

    public Data GetData()
    {
        return data;
    }
    public void SetNum(int num)
    {
        data.num = num;
    }
    public void SetSize(Vector2 size)
    {
        rectTransform.sizeDelta = size;
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
        data.num = 0;
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
