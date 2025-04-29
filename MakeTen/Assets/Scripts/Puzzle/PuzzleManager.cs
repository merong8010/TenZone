using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;

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
        //bonusMaxPoint = DataManager.Instance.Get<GameData.Config>().SingleOrDefault(x => x.key == "bonusMaxPoint").val
        bonusMaxPoint = DataManager.Instance.GetConfig("bonusMaxPoint");
    }

    private int bonusMaxPoint;

    //[SerializeField]
    //private ObjectPooler pooler;

    [SerializeField]
    private RectTransform blocksRT;

    public void GameStart(GameData.GameLevel level, bool use10Seconds)
    {
        currentLevel = level;

        InitBlocks(use10Seconds);
    }

    public void ClearBlocks()
    {
        if (blocks.Length > 0)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                ObjectPooler.Instance.ReturnObject("block", blocks[i].gameObject);
            }
        }
        blocks = new Block[] { };
    }

    public void InitBlocks(bool bonus10Seconds)
    {
        blockSize = new Vector2((blocksRT.rect.width - (blockGap.x * currentLevel.column)) / currentLevel.column, (blocksRT.rect.height - (blockGap.y * currentLevel.row)) / currentLevel.row);
        blockStartPos = new Vector2(-(blockSize.x + blockGap.x) * (currentLevel.column - 1) * 0.5f, -(blockSize.y + blockGap.y) * (currentLevel.row - 1) * 0.5f);

        for (int row = 0; row < currentLevel.row; row++)
        {
            for (int column = 0; column < currentLevel.column; column++)
            {
                Block blockObj = ObjectPooler.Instance.Get<Block>("block", blockParent, blockStartPos + new Vector2((blockSize.x + blockGap.x) * column, (blockSize.y + blockGap.y) * row), Vector3.one);
                blockObj.name = $"block_{column}_{row}";
                blockObj.SetSize(blockSize);
                blockObj.SetData(new Block.Data(column, row, currentLevel));
                blocks = blocks.Append(blockObj).ToArray();
            }
        }

        currentPoint.Value = 0;
        lastPointTicks = GameManager.Instance.dateTime.Value.Ticks;
        
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
        if(bonus10Seconds)
        {
            if(DataManager.Instance.userData.Use(GameData.GoodsType.Time_10s, 1))
            {
                finishTime = finishTime.AddSeconds(10);
            }
        }
        if (finishCoroutine != null) StopCoroutine(finishCoroutine);
        finishCoroutine = StartCoroutine(CheckFinish());

        searchTime = GameManager.Instance.dateTime.Value.ToTick().LongToDateTime();
        explodeTime = GameManager.Instance.dateTime.Value.ToTick().LongToDateTime();
        HUD.Instance.StartSearchCool(searchTime.AddSeconds(DataManager.Instance.SearchTerm), DataManager.Instance.SearchTerm);
        HUD.Instance.StartExplodeCool(searchTime.AddSeconds(DataManager.Instance.ExplodeTerm), DataManager.Instance.ExplodeTerm);
        CheckHint();
        IsPause = false;
    }

    public void RefreshPosition()
    {
        if (currentLevel == null) return;
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(RefreshDelay(0f));
    }

    private IEnumerator RefreshDelay(float delay)
    {
        yield return Yielders.EndOfFrame;
        yield return Yielders.Get(delay);
        float width = blocksRT.rect.xMax - blocksRT.rect.xMin;
        float height = blocksRT.rect.yMax - blocksRT.rect.yMin;

        Debug.Log($"PuzzleManager.RefreshPosition {blocksRT.rect.width},{blocksRT.rect.height} | {width} , {height} | {((RectTransform)blocksRT.parent).rect.width},{((RectTransform)blocksRT.parent).rect.height}");
        blockSize = new Vector2((width - (blockGap.x * currentLevel.column)) / currentLevel.column, (height - (blockGap.y * currentLevel.row)) / currentLevel.row);
        blockStartPos = new Vector2(-(blockSize.x + blockGap.x) * (currentLevel.column - 1) * 0.5f, -(blockSize.y + blockGap.y) * (currentLevel.row - 1) * 0.5f);

        for (int row = 0; row < currentLevel.row; row++)
        {
            for (int column = 0; column < currentLevel.column; column++)
            {
                Block blockObj = blocks.SingleOrDefault(x => x.name == $"block_{column}_{row}");
                blockObj.transform.localPosition = blockStartPos + new Vector2((blockSize.x + blockGap.x) * column, (blockSize.y + blockGap.y) * row);
                blockObj.SetSize(blockSize);
            }
        }
    }

    //private DeviceOrientation lastOrientation = DeviceOrientation.Unknown;
    
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
        while (State() != GameState.Continue)
        {
            Shuffle();
            CheckHint();
        }
    }

    public bool IsPause;

    private IEnumerator CheckFinish()
    {
        while (blocks != null && blocks.ToList().Exists(x => x != null && x.num > 0) && GameManager.Instance.dateTime.Value.Ticks <= finishTime.Ticks)
        {
            yield return Yielders.EndOfFrame;
        }
        if(blocks != null && !blocks.ToList().Exists(x => x != null && x.num > 0))
        {
            IsPause = true;
        }
        GameResult();

        finishCoroutine = null;
    }

    //private bool isFinish

    private void GameResult()
    {
        if (DataManager.Instance.userData.IsNewRecord(currentLevel.level, currentPoint.Value, true))
        {
            FirebaseManager.Instance.SubmitScore(currentLevel.level, GameManager.Instance.dateTime.Value.ToDateText(), currentPoint.Value);
        }
        if (DataManager.Instance.userData.IsNewRecord(currentLevel.level, currentPoint.Value, false))
        {
            FirebaseManager.Instance.SubmitScore(currentLevel.level, FirebaseManager.KEY.RANKING_ALL, currentPoint.Value);
        }
        int total = currentLevel.row * currentLevel.column;
        int maxPoint = total + (total / 2) * bonusMaxPoint;
        //float pointRate = (float)currentPoint.Value / (currentLevel.row * currentLevel.column);
        float pointRate = (float)currentPoint.Value / maxPoint;

        int exp = Mathf.FloorToInt(currentLevel.exp * pointRate);
        int gold = Mathf.FloorToInt(currentLevel.gold * pointRate);
        if(DataManager.Instance.userData.isVIP)
        {
            exp *= 2;
            gold *= 2;
        }
        UIManager.Instance.Open<PopupResult>().SetData(currentPoint.Value, exp, gold);

        DataManager.Instance.userData.ChargeExp(exp);
        DataManager.Instance.userData.Charge(GameData.GoodsType.Gold, gold);
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
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Util.GetMousePosition(), cam, out startPos);
        dragTransform.gameObject.SetActive(true);
        dragTransform.rectTransform.anchoredPosition = startPos;
        dragTransform.rectTransform.sizeDelta = Vector2.zero;
    }

    private long lastPointTicks;

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
                
                //lastPointTicks = (int)(finishTime.Ticks - GameManager.Instance.dateTime.Value.Ticks);

                int bonus = bonusMaxPoint - Mathf.FloorToInt((GameManager.Instance.dateTime.Value.Ticks - lastPointTicks)/10000000f);
                if(bonus > 0)
                {
                    //ObjectPooler.Instance.GetObject<Effect>("textPointBonus").SetText($"+{bonus}");
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Util.GetMousePosition(), cam, out startPos);
                    ObjectPooler.Instance.Get<Effect>("textPointBonus", transform, position: startPos, autoReturnTime: 1f).SetText($"+{bonus}");
                }

                currentPoint.Value += focus.Length+bonus;

                lastPointTicks = GameManager.Instance.dateTime.Value.Ticks;

                if (blocks.ToList().Exists(x => x.num > 0))
                {
                    CheckHint();
                    CheckGameState();
                }
//#if !UNITY_EDITOR
//                if (OptionManager.Instance.Get(OptionManager.Type.HAPTIC))
//                    Haptic.Execute();
//#endif
                if(tutorialRT.gameObject.activeSelf)
                {
                    tutorialRT.gameObject.SetActive(false);
                }
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
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Util.GetMousePosition(), cam, out Vector2 currentPos);
            Vector2 size = currentPos - startPos;
            dragTransform.rectTransform.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
            // 위치 조정 (좌상단 기준)
            dragTransform.rectTransform.anchoredPosition = startPos + size / 2;

            focus = blocks.Where(x => dragTransform.rectTransform.IsInside(x.transform) && x.num > 0).ToArray();
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].Focus(focus.Contains(blocks[i]));
            }
        }

        if(IsPause)
        {
            finishTime = finishTime.AddSeconds(Time.deltaTime);
        }
    }

    private void CheckHint()
    {
        if (blocks != null && !blocks.ToList().Exists(x => x != null && x.num > 0))
        {
            return;
        }
        hint.Clear();
        for(int i = 0; i < blocks.Length-1; i++)
        {
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

        if(DataManager.Instance.userData.IsTutorial)
        {
            if(hint.Count > 0)
                StartCoroutine(ShowHint());
        }
    }

    private void CheckGameState()
    {
        GameState state = State();
        if (state > GameState.Continue)
        {
            if (state == GameState.NeedShuffle && !DataManager.Instance.userData.Has(GameData.GoodsType.Shuffle, 1) || state == GameState.NoMoreMatch)
            {
                UIManager.Instance.Message.Show(Message.Type.Confirm, TextManager.Get("NoMoreMatch"), callback: confirm =>
                {
                    GameResult();
                });
            }
            else
            {
                UIManager.Instance.Message.Show(Message.Type.Ask, TextManager.Get("NeedShuffle"), callback: confirm =>
                {
                    if (confirm)
                    {
                        if (DataManager.Instance.userData.IsTutorial || DataManager.Instance.userData.Use(GameData.GoodsType.Shuffle, 1))
                        {
                            Shuffle();
                        }
                        else
                        {
                            GameResult();
                        }
                    }
                    else
                    {
                        GameResult();
                    }
                });
            }
        }
    }

    private enum GameState
    {
        Continue,
        NeedShuffle,
        NoMoreMatch,
    }
    /// <summary>
    /// 게임이 진행 가능한지 체크 
    /// </summary>
    /// <returns>
    /// 0 : 진행가능 ,
    /// 1 : 블록섞기 필요
    /// 2 : 진행 불가능,
    /// </returns>
    private GameState State()
    {
        if(hint.Count == 0)
        {
            Block[] aliveBlocks = blocks.Where(x => x.num > 0).ToArray();
            if (HasCombinationSum(aliveBlocks.Select(x => x.num).ToArray()))
                return GameState.NeedShuffle;
            return GameState.NoMoreMatch;
        }
        return GameState.Continue;
    }

    private bool isTraining = false;

    private const float hintWaitTime = 1f;
    private const float hintShowTime = 2f;
    private Coroutine hintCoroutine;

    private DateTime searchTime;
    private DateTime explodeTime;

    public void Explode()
    {
        Debug.Log($"Explode  {hint.Count()} | {GameManager.Instance.dateTime.Value.Ticks} | {explodeTime.AddSeconds(DataManager.Instance.ExplodeTerm).Ticks}  | {GameManager.Instance.dateTime.Value.Ticks < explodeTime.AddSeconds(DataManager.Instance.ExplodeTerm).Ticks}");
        if (hint.Count() == 0) return;
        if (GameManager.Instance.dateTime.Value.Ticks < explodeTime.AddSeconds(DataManager.Instance.ExplodeTerm).Ticks) return;
        if (DataManager.Instance.userData.Use(GameData.GoodsType.Explode, 1))
        {
            var list = hint.ToList();
            var show = list[UnityEngine.Random.Range(0, list.Count)];
            Block[] focusBlocks = blocks.Where(x => x.column >= show.Key.x && x.column <= show.Value.x && x.row >= show.Key.y && x.row <= show.Value.y).ToArray();
            for (int i = 0; i < focusBlocks.Length; i++)
            {
                focusBlocks[i].Focus(true);
            }

            int bonus = bonusMaxPoint - Mathf.FloorToInt((GameManager.Instance.dateTime.Value.Ticks - lastPointTicks) / 10000000f);
            if (bonus > 0)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Util.GetMousePosition(), cam, out startPos);
                ObjectPooler.Instance.Get<Effect>("textPointBonus", transform, position: startPos, autoReturnTime: 1f).SetText($"+{bonus}");
            }
            currentPoint.Value += focusBlocks.Length + bonus;
            lastPointTicks = GameManager.Instance.dateTime.Value.Ticks;
            for (int i = 0; i < focusBlocks.Length; i++)
            {
                focusBlocks[i].Break();
            }
            if (blocks != null && blocks.ToList().Exists(x => x != null && x.num > 0))
            {
                CheckHint();
                CheckGameState();
            }

            explodeTime = GameManager.Instance.dateTime.Value.ToTick().LongToDateTime();
            HUD.Instance.StartExplodeCool(explodeTime.AddSeconds(DataManager.Instance.ExplodeTerm), DataManager.Instance.ExplodeTerm);
        }
    }

    public void Search()
    {
        Debug.Log($"Search  {hint.Count()} | {GameManager.Instance.dateTime.Value.Ticks} | {searchTime.AddSeconds(DataManager.Instance.SearchTerm).Ticks}  | {GameManager.Instance.dateTime.Value.Ticks < searchTime.AddSeconds(DataManager.Instance.SearchTerm).Ticks}" );
        if (hint.Count() == 0) return;
        if (GameManager.Instance.dateTime.Value.Ticks < searchTime.AddSeconds(DataManager.Instance.SearchTerm).Ticks) return;
        if (DataManager.Instance.userData.Use(GameData.GoodsType.Search, 1))
        {
            var list = hint.ToList();
            var show = list[UnityEngine.Random.Range(0, list.Count)];
            Block[] focusBlocks = blocks.Where(x => x.column >= show.Key.x && x.column <= show.Value.x && x.row >= show.Key.y && x.row <= show.Value.y).ToArray();
            for (int i = 0; i < focusBlocks.Length; i++)
            {
                focusBlocks[i].Focus(true);
            }

            searchTime = GameManager.Instance.dateTime.Value.ToTick().LongToDateTime();
            HUD.Instance.StartSearchCool(searchTime.AddSeconds(DataManager.Instance.SearchTerm), DataManager.Instance.SearchTerm);
        }
    }
    [SerializeField]
    private RectTransform tutorialRT;
    private IEnumerator ShowHint()
    {
        yield return Yielders.Get(hintWaitTime);
        var list = hint.ToList();
        var show = list[UnityEngine.Random.Range(0, list.Count)];
        Block[] focusBlocks = blocks.Where(x => x.column >= show.Key.x && x.column <= show.Value.x && x.row >= show.Key.y && x.row <= show.Value.y).ToArray();
        for(int i = 0; i < focusBlocks.Length; i++)
        {
            focusBlocks[i].Focus(true);
        }
        if(DataManager.Instance.userData.IsTutorial && currentPoint.Value == 0)
        {
            Block startBlock = blocks.SingleOrDefault(x => x.column == show.Key.x && x.row == show.Key.y);
            Block finishBlock = blocks.SingleOrDefault(x => x.column == show.Value.x && x.row == show.Value.y);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(cam, startBlock.StartPos()), cam, out Vector2 tutoStartPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(cam, finishBlock.FinishPos()), cam, out Vector2 tutoFinishPos);

            tutorialRT.gameObject.SetActive(true);
            Vector2 size = tutoStartPos - tutoFinishPos;
            tutorialRT.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
            // 위치 조정 (좌상단 기준)
            //tutorialRT.anchoredPosition = tutoStartPos + size / 2;
            //tutorialRT.anchoredPosition = tutoStartPos + size;
            tutorialRT.anchoredPosition = tutoStartPos - size/2;
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

    private bool HasCombinationSum(int[] nums)
    {
        return CheckSumRecursive(nums, 0, 0, 0);
    }

    // 재귀 함수로 모든 조합 탐색
    private bool CheckSumRecursive(int[] nums, int index, int currentSum, int count)
    {
        if (count >= 2 && currentSum == TargetSumNum)
            return true;
        if (index >= nums.Length || currentSum > TargetSumNum)
            return false;

        // 현재 숫자를 포함하는 경우
        if (CheckSumRecursive(nums, index + 1, currentSum + nums[index], count + 1))
            return true;

        // 현재 숫자를 포함하지 않는 경우
        if (CheckSumRecursive(nums, index + 1, currentSum, count))
            return true;

        return false;
    }

    private RectTransform tutorialTransform;

    public void Tutorial()
    {

    }
}
