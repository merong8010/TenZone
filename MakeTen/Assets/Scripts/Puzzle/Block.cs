using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;

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

    public int row;
    public int column;

    public int bonus { private set; get; }

    [SerializeField]
    private GameObject numObj;
    [SerializeField]
    private RectTransform rectTransform;

    [SerializeField]
    private GameObject defaultObj;
    [SerializeField]
    private TextMeshProUGUI defaultNumText;
    [SerializeField]
    private TextMeshProUGUI defaultBonusText;

    [SerializeField]
    private GameObject focusObj;
    [SerializeField]
    private TextMeshProUGUI focusNumText;
    [SerializeField]
    private TextMeshProUGUI focusBonusText;

    [System.Serializable]
    public class Data
    {
        public int column;
        public int row;

        public int num;
        public int bonus;
        
        public Data(int column, int row, GameData.GameLevel level)
        {
            this.column = column;
            this.row = row;
            num = Util.GenerateGaussianRandom(level.mean, level.stdDev);
            //bonus = ((float)level.bonusCount/(level.row * level.column)).IsSuccess() ? Random.Range(level.bonusTimeMin, level.bonusTimeMax) : 0;
            bonus = ((float)level.bonusCount / (level.row * level.column)).IsSuccess() ? Util.GenerateGaussianRandom(level.bonusTimeMin, level.bonusTimeMax) : 0;
        }
    }
    [SerializeField]
    private Data data;
    public void SetData(Data data)
    {
        this.data = data;
        column = data.column;
        row = data.row;

        defaultNumText.text = focusNumText.text = data.num.ToString();
        defaultBonusText.text = focusBonusText.text = data.bonus > 0 ? $"+{data.bonus}s" : string.Empty;
        numObj.SetActive(data.num > 0);
        Focus(false);
    }

    public Data GetData()
    {
        return data;
    }
    public void SetNum(int num)
    {
        data.num = num;
        defaultNumText.text = focusNumText.text = data.num.ToString();
    }
    public void SetSize(Vector2 size)
    {
        rectTransform.sizeDelta = size;
    }
    
    public void Focus(bool isFocus)
    {
        if (!numObj.activeSelf) return;
        defaultObj.SetActive(!isFocus);
        focusObj.SetActive(isFocus);
    }

    [SerializeField]
    private string effectTag;
    [SerializeField]
    private float effectDuration = 3f;
    [SerializeField]
    private Vector3 effectScale;
    public void Break()
    {
        numObj.SetActive(false);
        data.num = 0;
        ObjectPooler.Instance.GetObject<Effect>(effectTag, PuzzleManager.Instance.transform, scale : effectScale, position : transform.localPosition, autoReturnTime: effectDuration);
        if(data.bonus > 0)
        {
            PuzzleManager.Instance.AddSeconds(data.bonus);
            //ObjectPooler.Instance.GetObject<Effect>("block_bonus", PuzzleManager.Instance.transform, transform.localPosition, autoReturnTime: 1f);
        }
    }
    public void InitRandom()
    {
        if (aniCoroutine != null) StopCoroutine(aniCoroutine);
        aniCoroutine = StartCoroutine(RandomAnimation());
    }

    private Coroutine aniCoroutine;
    private void OnEnable()
    {
        if(randomAniObjs != null && randomAniObjs.Length > 0)
            InitRandom();
    }

    [SerializeField]
    private float randomChangeMin = 3f;
    [SerializeField]
    private float randomChangeMax = 10f;

    [SerializeField]
    private GameObject[] randomAniObjs;
    [SerializeField]
    private TextMeshProUGUI[] randomAniTexts;
    private IEnumerator RandomAnimation()
    {
        int ran = Random.Range(1, 9);

        for(int i = 0; i < randomAniTexts.Length; i++)
        {
            randomAniTexts[i].text = ran.ToString();
        }

        while (gameObject.activeInHierarchy)
        {
            int ranShow = Random.Range(0, 4);
            for (int i = 0; i < randomAniObjs.Length; i++)
            {
                randomAniObjs[i].SetActive(i == ranShow);
            }

            yield return Yielders.Get(Random.Range(randomChangeMin, randomChangeMax));
        }
    }

}
