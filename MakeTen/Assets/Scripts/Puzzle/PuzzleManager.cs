using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using System.Text;
using System.Collections;
using System.Collections.Generic;
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
        //Initialize();
    }

    private Coroutine finishCoroutine;

    private bool isInit = false;

    [SerializeField]
    private SafeArea blockSafeArea;
    public void Initialize()
    {
        if (isInit) return;
        isInit = true;
        if (blocks == null) blocks = new Block[] { };

        currentPoint = new ReactiveProperty<int>();
        
        HUD.Instance.Initialize(currentPoint);
        blockSafeArea.refreshAction = RefreshPosition;
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
                blockObj.SetData(new Block.Data(column, row, currentLevel));
                blocks = blocks.Append(blockObj).ToArray();
            }
        }

        currentPoint.Value = 0;
        remainMilliSeconds = 0;
        //System.Random rand = new System.Random();
        //for (int i = 0; i < blocks.Length; i++)
        //{
        //    blocks[i].SetData(new Block.Data(currentLevel));
        //}

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

        CheckHint();
    }

    public void RefreshPosition()
    {
        if(currentLevel == null) return;
        blockSize = new Vector2((blocksRT.rect.width - (blockGap.x * currentLevel.column)) / currentLevel.column, (blocksRT.rect.height - (blockGap.y * currentLevel.row)) / currentLevel.row);
        blockStartPos = new Vector2(-(blockSize.x + blockGap.x) * (currentLevel.column - 1) * 0.5f, -(blockSize.y + blockGap.y) * (currentLevel.row - 1) * 0.5f);

        for (int row = 0; row < currentLevel.row; row++)
        {
            for (int column = 0; column < currentLevel.column; column++)
            {
                //Block blockObj = pooler.GetObject<Block>("block", blockParent, blockStartPos + new Vector2((blockSize.x + blockGap.x) * column, (blockSize.y + blockGap.y) * row), Vector3.one);
                
                Block blockObj = blocks.SingleOrDefault(x => x.name == $"block_{column}_{row}");
                blockObj.transform.localPosition = blockStartPos + new Vector2((blockSize.x + blockGap.x) * column, (blockSize.y + blockGap.y) * row);
                //blockObj.name = $"block_{column}_{row}";
                blockObj.SetSize(blockSize);
                //blocks = blocks.Append(blockObj).ToArray();
            }
        }
    }

    private DeviceOrientation lastOrientation = DeviceOrientation.Unknown;
    

    public void Shuffle()
    {
        Block.Data[] datas  = blocks.Select(x => x.GetData()).ToArray().Shuffle();
        
        for(int i = 0; i < blocks.Length; i++)
        {
            datas[i].column = blocks[i].column;
            datas[i].row = blocks[i].row;
            blocks[i].SetData(datas[i]);
        }
        CheckHint();
    }

    private IEnumerator CheckFinish()
    {
        while (blocks.ToList().Exists(x => x.num > 0) && GameManager.Instance.dateTime.Value.Ticks <= finishTime.Ticks)
        {
            yield return new WaitForEndOfFrame();
        }

        GameResult();

        finishCoroutine = null;
    }

    private void GameResult()
    {
        if (DataManager.Instance.userData.IsNewRecord(currentLevel.level, currentPoint.Value, remainMilliSeconds, true))
        {
            FirebaseManager.Instance.SubmitScore(currentLevel.level, GameManager.Instance.dateTime.Value.ToDateText(), currentPoint.Value, remainMilliSeconds);
        }
        if (DataManager.Instance.userData.IsNewRecord(currentLevel.level, currentPoint.Value, remainMilliSeconds, false))
        {
            FirebaseManager.Instance.SubmitScore(currentLevel.level, FirebaseManager.KEY.RANKING_ALL, currentPoint.Value, remainMilliSeconds);
        }
        int exp = Mathf.FloorToInt(currentLevel.exp * ((float)currentPoint.Value / (currentLevel.row * currentLevel.column)));
        UIManager.Instance.Open<PopupResult>().SetData(currentPoint.Value, remainMilliSeconds, exp);

        DataManager.Instance.userData.ChargeExp(exp);
    }


    [SerializeField]
    private Camera cam;
    [SerializeField]
    private RectTransform canvasRect;
    public void OnClick()
    {
        isDrag = true;
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;

            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].Focus(false);
            }
        }
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

                CheckHint();

#if !UNITY_EDITOR
                if (OptionManager.Instance.Get(OptionManager.Type.HAPTIC))
                    Haptic.Execute();
#endif
            }
            focus = null;
        }

        isDrag = false;
        dragTransform.gameObject.SetActive(false);

        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].Focus(false);
        }
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

        if (lastOrientation != Util.GetDeviceOrientation())
        {
            RefreshPosition();
            lastOrientation = Util.GetDeviceOrientation();
        }
    }

    private void CheckHint()
    {
        hint.Clear();
        for(int i = 0; i < blocks.Length-1; i++)
        {
            if (blocks[i].num == 0) continue;
            Vector2Int resultColumn = CheckBlockColumn(blocks[i].column, blocks[i].row, blocks[i].num);
            if(resultColumn != default(Vector2Int))
            {
                hint.Add(new Vector2Int(blocks[i].column, blocks[i].row), resultColumn);
                continue;
            }
            else
            {
                Vector2Int resultRow = CheckBlockRow(blocks[i].column, blocks[i].row, blocks[i].num);
                if (resultRow != default(Vector2Int))
                {
                    hint.Add(new Vector2Int(blocks[i].column, blocks[i].row), resultRow);
                }
            }
        }
        if(hint.Count == 0)
        {
            UIManager.Instance.Message.Show(Message.Type.Ask, "no block need use Shuffle", callback: confirm =>
            {
                if (confirm)
                {
                    Shuffle();
                }
                else
                {
                    //GameManager.Instance.GoScene(GameManager.Scene.Main);
                    GameResult();
                }
            });
        }
        else
        {
            if(hintCoroutine != null)  StopCoroutine(hintCoroutine);
            hintCoroutine = StartCoroutine(ShowHint());
        }
    }

    private const float hintWaitTime = 1f;
    private const float hintShowTime = 2f;
    private Coroutine hintCoroutine;
    private IEnumerator ShowHint()
    {
        Debug.Log($"ShowHint {hint.Count}");
        yield return Yielders.Get(hintWaitTime);
        var list = hint.ToList();
        var show = list[UnityEngine.Random.Range(0, list.Count)];
        Debug.Log(show.Key + " | " + show.Value);
        Block[] focusBlocks = blocks.Where(x => x.column >= show.Key.x && x.column <= show.Value.x && x.row >= show.Key.y && x.row <= show.Value.y).ToArray();
        Debug.Log($"FocusBlocks {focusBlocks.Length}");
        for(int i = 0; i < focusBlocks.Length; i++)
        {
            focusBlocks[i].Focus(true);
        }
        yield return Yielders.Get(hintShowTime);
        for (int i = 0; i < focusBlocks.Length; i++)
        {
            focusBlocks[i].Focus(false);
        }
    }
    [SerializeField]
    Dictionary<Vector2Int, Vector2Int> hint = new Dictionary<Vector2Int, Vector2Int>();

    private Vector2Int CheckBlockColumn(int column, int row, int num)
    {
        if (row == currentLevel.row - 1 && column == currentLevel.column - 1) return default(Vector2Int);
        bool endColumn = column + 1 == currentLevel.column;
        int searchColumn = endColumn ? 0 : 1;
        int searchRow = endColumn ? 1 : 0;
        int sum = num;
        
        while (true)
        {
            if (endColumn)
            {
                sum += blocks.Where(x => x.column >= column && x.column <= column + searchColumn && x.row == row + searchRow).Sum(x=>x.num);
            }
            else
            {
                sum += blocks.SingleOrDefault(x => x.column == column + searchColumn && x.row == row + searchRow).num;
            }
            
            if (sum == TargetSumNum) return new Vector2Int(column+searchColumn, row+searchRow);
            if(sum < TargetSumNum)
            {
                if(column+searchColumn < currentLevel.column -1)
                {
                    searchColumn += 1;
                }
                else
                {
                    if(row + searchRow == currentLevel.row -1)
                    {
                        break;
                    }
                    searchRow += 1;
                    endColumn = true;
                }
            }
            else
            {
                if(!endColumn)
                {
                    if (row + searchRow == currentLevel.row - 1)
                    {
                        break;
                    }
                    searchColumn -= 1;
                    searchRow += 1;
                    endColumn = true;
                }
                else
                {
                    break;
                }
            }
        }
        return default(Vector2Int);

    }

    private Vector2Int CheckBlockRow(int column, int row, int num)
    {
        if(row == currentLevel.row-1 && column == currentLevel.column-1) return default(Vector2Int);
        bool endRow = row + 1 == currentLevel.row;
        int searchColumn = endRow ? 1 : 0;
        int searchRow = endRow ? 0 : 1;
        int sum = num;
        
        while (true)
        {
            if (endRow)
            {
                sum += blocks.Where(x => x.row >= row && x.row <= row + searchRow && x.column == column + searchColumn).Sum(x => x.num);
            }
            else
            {
                sum += blocks.SingleOrDefault(x => x.column == column + searchColumn && x.row == row + searchRow).num;
            }

            if (sum == TargetSumNum) return new Vector2Int(column+searchColumn, row+searchRow);
            if (sum < TargetSumNum)
            {
                if (row + searchRow < currentLevel.row - 1)
                {
                    searchRow += 1;
                }
                else
                {
                    if (column + searchColumn == currentLevel.column - 1)
                    {
                        break;
                    }
                    searchColumn += 1;
                    endRow = true;
                }
            }
            else
            {
                if (!endRow)
                {
                    if (column + searchColumn == currentLevel.column - 1)
                    {
                        break;
                    }
                    searchRow -= 1;
                    searchColumn += 1;
                    endRow = true;
                }
                else
                {
                    break;
                }
            }
        }
        return default(Vector2Int);
    }

    public void AddSeconds(float sec)
    {
        finishTime = finishTime.AddSeconds(sec);
        //HUD.Instance.ShowAddSeconds(sec);
    }

    private RectTransform tutorialTransform;

    public void Tutorial()
    {

    }
}
