using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using System.Text;
using System.Collections;

public class PuzzleManager : Singleton<PuzzleManager>
{
    [SerializeField]
    private Image dragTransform;

    //public RectTransform canvasRect;  // 캔버스
    //public Image rectImagePrefab;     // 생성할 이미지 프리팹

    //private Image currentImage;
    private Vector2 startPos;
    private bool isDrag;

    [SerializeField]
    private Transform blockParent;
    [SerializeField]
    private GameObject blockObj;
    //[SerializeField]
    private Block[] blocks;

    private const int TargetSumNum = 10;

    private ReactiveProperty<int> currentPoint;
    private ReactiveProperty<float> currentTime;

    [SerializeField]
    private Text pointText;
    [SerializeField]
    private Text timeText;

    [SerializeField]
    private int rowCount;
    [SerializeField]
    private int columnCount;

    [SerializeField]
    private Vector2 blockStartPos;
    [SerializeField]
    private Vector2 blockSize;
    [SerializeField]
    private Vector2 blockGap;

    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(Initialize());
    }

    private Coroutine timeCoroutine;

    private IEnumerator Initialize()
    {
        if (blocks == null) blocks = new Block[] { };

        currentPoint = new ReactiveProperty<int>();
        currentPoint.Subscribe(x => { pointText.text = new StringBuilder().Append("point : ").Append(x).ToString(); });

        currentTime = new ReactiveProperty<float>();
        currentTime.Subscribe(x => { timeText.text = new StringBuilder().Append("time : ").Append(Mathf.RoundToInt(x)).ToString(); });

        yield return new WaitForSeconds(1f);

        InitBlocks();
    }

    [SerializeField]
    private InputField meanInput;
    private float mean = 3f;
    private const float meanDefault = 3f;

    [SerializeField]
    private InputField stdDevInput;
    private float stdDev = 3.5f;
    private const float stdDevDefault = 3.5f;

    public void OnValueChangeMean(string str)
    {
        if(!float.TryParse(str, out mean))
        {
            meanInput.text = meanDefault.ToString();
            mean = meanDefault;
        }
    }

    public void OnValueChangeStdDev(string str)
    {
        if (!float.TryParse(str, out stdDev))
        {
            stdDevInput.text = stdDevDefault.ToString();
            stdDev = stdDevDefault;
        }
    }

    [SerializeField]
    private InputField columnInput;
    private const int columnDefault = 17;
    [SerializeField]
    private InputField rowInput;
    private const int rowDefault = 10;

    public void OnValueChangeColumn(string str)
    {
        if (!int.TryParse(str, out columnCount))
        {
            columnInput.text = columnDefault.ToString();
            columnCount = columnDefault;
        }
    }

    public void OnValueChangeRow(string str)
    {
        if (!int.TryParse(str, out rowCount))
        {
            rowInput.text = rowDefault.ToString();
            rowCount = rowDefault;
        }
    }

    [SerializeField]
    private ObjectPooler pooler;

    public void InitBlocks()
    {
        if(blocks.Length > 0)
        {
            for(int i = 0; i < blocks.Length; i++)
            {
                pooler.ReturnObject("block", blocks[i].gameObject);
            }
        }

        blocks = new Block[] { };

        blockStartPos = new Vector2(-(blockSize.x + blockGap.x) * columnCount * 0.5f, -(blockSize.y + blockGap.y) * rowCount * 0.5f);
        for (int row = 0; row < rowCount; row++)
        {
            for (int column = 0; column < columnCount; column++)
            {
                GameObject blockObj = pooler.GetObject("block");
                blockObj.name = $"block_{row}_{column}";
                blockObj.transform.SetParent(blockParent);
                blockObj.transform.localScale = Vector3.one;
                blockObj.transform.localPosition = blockStartPos + new Vector2((blockSize.x + blockGap.x) * column, (blockSize.y + blockGap.y) * row);
                Block block = blockObj.GetComponent<Block>();
                block.SetSize(blockSize);
                blocks = blocks.Append(block).ToArray();
            }
        }

        currentPoint.Value = 0;

        //System.Random rand = new System.Random();
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].Init(Util.GenerateGaussianRandom(mean, stdDev));
        }

        int remain = blocks.Sum(x => x.num) % TargetSumNum;
        if (remain > 0)
        {
            while(remain > 0)
            {
                for (int i = 0; i < blocks.Length; i++)
                {
                    if (blocks[i].num > 1)
                    {
                        blocks[i].Init(blocks[i].num - 1);
                        remain -= 1;
                    }

                    if (remain == 0) break;
                }
            }
            
        }

        int num9 = blocks.Count(x => x.num == 9);
        int num1 = blocks.Count(x => x.num == 1);
        currentTime.Value = 0f;
        if (timeCoroutine != null) StopCoroutine(timeCoroutine);
        timeCoroutine = StartCoroutine(CheckTime());
    }

    public void Shuffle()
    {
        int[] nums = blocks.Select(x => x.num).ToArray().Shuffle();
        
        for(int i = 0; i < blocks.Length; i++)
        {
            blocks[i].Init(nums[i]);
        }
    }

    private IEnumerator CheckTime()
    {
        while (blocks.ToList().Exists(x => x.num > 0))
        {
            yield return new WaitForEndOfFrame();
            currentTime.Value += UnityEngine.Time.deltaTime;
        }

        InitBlocks();
    }


    [SerializeField]
    private Camera cam;
    [SerializeField]
    private RectTransform canvasRect;
    public void OnClick()
    {
        isDrag = true;

        //startPos = Input.mousePosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, cam, out startPos);
        dragTransform.gameObject.SetActive(true);
        dragTransform.rectTransform.anchoredPosition = startPos;
        dragTransform.rectTransform.sizeDelta = Vector2.zero;
    }

    public void OnRelease()
    {
        if (focus != null && focus.Length > 0)
        {
            if (focus.Sum(x => x.num) == TargetSumNum)
            {
                for (int i = 0; i < focus.Length; i++)
                {
                    focus[i].Break();
                }
                currentPoint.Value += focus.Length;
            }
            focus = null;
        }

        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].Focus(false);
        }

        isDrag = false;
        dragTransform.gameObject.SetActive(false);
    }

    private Block[] focus = new Block[] { };
    void Update()
    {
        if (isDrag)
        {
            // 현재 터치 위치까지 크기 조정
            //Vector2 currentPos = Input.mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, cam, out Vector2 currentPos);
            Vector2 size = currentPos - startPos;
            dragTransform.rectTransform.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
            // 위치 조정 (좌상단 기준)
            dragTransform.rectTransform.anchoredPosition = startPos + size / 2;

            focus = blocks.Where(x => dragTransform.rectTransform.IsInside(x.transform)).ToArray();
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].Focus(focus.Contains(blocks[i]));
            }
        }
    }
}
