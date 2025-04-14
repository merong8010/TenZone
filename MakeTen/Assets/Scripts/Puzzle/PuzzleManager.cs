using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using System.Text;
using System.Collections;
using System;

public class PuzzleManager : Singleton<PuzzleManager>
{
    public enum Level
    {
        None,
        Easy,
        Normal,
        Hard,
        Expert,
    }
    [SerializeField]
    private Image dragTransform;
    /// <summary>
    /// 터치 입력 값
    /// </summary>
    private Vector2 startPos;
    private bool isDrag;

    [SerializeField]
    private Transform blockParent;
    [SerializeField]
    private GameObject blockObj;
    //[SerializeField]
    private Block[] blocks;

    private const int TargetSumNum = 10;
    //private const int GameTime = 100;

    private ReactiveProperty<int> currentPoint;
    public DateTime finishTime;

    private GameData.GameLevel currentLevel;
    public int CurrentGameTime => currentLevel != null ? currentLevel.time : 0;

    private Vector2 blockStartPos;
    [SerializeField]
    private Vector2 blockSize;
    [SerializeField]
    private Vector2 blockGap;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private Coroutine finishCoroutine;

    private bool isInit = false;
    private void Initialize()
    {
        if (isInit) return;
        isInit = true;
        if (blocks == null) blocks = new Block[] { };

        currentPoint = new ReactiveProperty<int>();
        
        HUD.Instance.Initialize(currentPoint);
    }

    [SerializeField]
    private ObjectPooler pooler;

    [SerializeField]
    private RectTransform blocksRT;

    public void GameStart(GameData.GameLevel level)
    {
        currentLevel = level;
        InitBlocks();
    }

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

        blockSize = new Vector2((blocksRT.rect.width - (blockGap.x * currentLevel.column)) / currentLevel.column, (blocksRT.rect.height - (blockGap.y * currentLevel.row)) / currentLevel.row);
        blockStartPos = new Vector2(-(blockSize.x + blockGap.x) * (currentLevel.column - 1) * 0.5f, -(blockSize.y + blockGap.y) * (currentLevel.row - 1) * 0.5f);

        for (int row = 0; row < currentLevel.row; row++)
        {
            for (int column = 0; column < currentLevel.column; column++)
            {
                Block blockObj = pooler.GetObject<Block>("block", blockParent, blockStartPos + new Vector2((blockSize.x + blockGap.x) * column, (blockSize.y + blockGap.y) * row), Vector3.one);
                blockObj.name = $"block_{column}_{row}";
                blockObj.SetSize(blockSize);
                blocks = blocks.Append(blockObj).ToArray();
            }
        }

        currentPoint.Value = 0;
        remainMilliSeconds = 0;
        //System.Random rand = new System.Random();
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].SetData(new Block.Data(currentLevel));
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
                        //blocks[i].Init(blocks[i].num - 1);
                        blocks[i].SetNum(blocks[i].num - 1);
                        remain -= 1;
                    }

                    if (remain == 0) break;
                }
            }
        }

        finishTime = GameManager.Instance.dateTime.Value.AddSeconds(currentLevel.time);
        if (finishCoroutine != null) StopCoroutine(finishCoroutine);
        finishCoroutine = StartCoroutine(CheckFinish());
    }

    public void Shuffle()
    {
        Block.Data[] datas  = blocks.Select(x => x.GetData()).ToArray().Shuffle();
        
        for(int i = 0; i < blocks.Length; i++)
        {
            blocks[i].SetData(datas[i]);
        }
    }

    private IEnumerator CheckFinish()
    {
        while (blocks.ToList().Exists(x => x.num > 0) && GameManager.Instance.dateTime.Value.Ticks <= finishTime.Ticks)
        {
            yield return new WaitForEndOfFrame();
        }

        UIManager.Instance.Open<PopupResult>().SetData(currentPoint.Value, finishTime.Ticks - GameManager.Instance.dateTime.Value.Ticks);
        
        if(DataManager.Instance.userData.IsNewRecord(currentLevel.level, currentPoint.Value, remainMilliSeconds, true))
        {
            FirebaseManager.Instance.SubmitScore(currentLevel.level, GameManager.Instance.dateTime.Value.ToDateText(), currentPoint.Value, remainMilliSeconds);
        }
        if (DataManager.Instance.userData.IsNewRecord(currentLevel.level, currentPoint.Value, remainMilliSeconds, false))
        {
            FirebaseManager.Instance.SubmitScore(currentLevel.level, FirebaseManager.KEY.RANKING_ALL, currentPoint.Value, remainMilliSeconds);
        }

        DataManager.Instance.userData.ChargeExp(Mathf.FloorToInt(currentLevel.exp * ((float)currentPoint.Value / (currentLevel.row * currentLevel.column))));
        finishCoroutine = null;
    }


    [SerializeField]
    private Camera cam;
    [SerializeField]
    private RectTransform canvasRect;
    public void OnClick()
    {
        isDrag = true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, cam, out startPos);
        dragTransform.gameObject.SetActive(true);
        dragTransform.rectTransform.anchoredPosition = startPos;
        dragTransform.rectTransform.sizeDelta = Vector2.zero;
    }

    private int remainMilliSeconds;

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
                remainMilliSeconds = (int)(finishTime.Ticks - GameManager.Instance.dateTime.Value.Ticks);
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
