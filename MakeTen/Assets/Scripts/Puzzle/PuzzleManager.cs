using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UniRx;
using GameData; // UniRx를 사용하고 있으므로 using 구문 추가

public class PuzzleManager : Singleton<PuzzleManager>
{
    // --- 필드 및 프로퍼티 ---
    [Header("Component References")]
    [SerializeField] private BlockGridManager blockGridManager;
    [SerializeField] private PuzzleInputHandler inputHandler;
    [SerializeField] private PuzzleUIManager uiManager;
    [SerializeField] private PuzzleLogicManager logicManager;

    public GameData.GameLevel CurrentLevel { get; private set; }
    public DateTime FinishTime { get; private set; }
    //public bool IsPaused { get; set; } = false;

    private ReactiveProperty<int> currentPoint = new ReactiveProperty<int>();
    private long lastPointTicks;
    private int bonusMaxPoint;
    private bool isGameFinished = false;

    // --- Unity 생명주기 메서드 ---
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Start()
    {
        GameStart(GameManager.Instance.currentLevel, GameManager.Instance.isUse10Seconds, GameManager.Instance.isUseTimeFreeze);
        SoundManager.Instance.PlayBGM("puzzle");
    }

    private bool IsPaused = false;
    private void Update()
    {
        if (IsPaused)
        {
            FinishTime = FinishTime.AddSeconds(Time.deltaTime);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // 구독 해제는 UIManager에서 처리
    }

    // --- 초기화 ---
    public void Initialize()
    {
        // 다른 매니저 컴포넌트 가져오기
        //blockGridManager = GetComponent<BlockGridManager>();
        //inputHandler = GetComponent<PuzzleInputHandler>();
        //uiManager = GetComponent<PuzzleUIManager>();
        //logicManager = GetComponent<PuzzleLogicManager>();

        bonusMaxPoint = DataManager.Instance.GetConfig("bonusMaxPoint");

        // 이벤트 구독
        inputHandler.OnValidBlocksSelected += HandleValidSelection;
        logicManager.OnAbilityBlocksRemoved += HandleValidSelection;

        // UI 매니저 초기화
        uiManager.Initialize(this, currentPoint);
    }

    public bool isTimeFreeze;
    // --- 게임 흐름 제어 ---
    public void GameStart(GameData.GameLevel level, bool use10Seconds, bool useTimeFreeze)
    {
        isGameFinished = false;
        CurrentLevel = level;
        currentPoint.Value = 0;
        continueCount = 1;
        // 시간 설정
        int gameTime = CurrentLevel.time;
        if (use10Seconds && DataManager.Instance.userData.Use(GameData.GoodsType.Time_10s, 1))
        {
            gameTime += 10;
        }
        if (useTimeFreeze && DataManager.Instance.userData.Use(GameData.GoodsType.TimeFreeze, 1))
        {
            isTimeFreeze = true;
        }
        else isTimeFreeze = false;

            // [수정됨] .Value 제거
        FinishTime = GameManager.Instance.dateTime.Value.AddSeconds(gameTime);
        // [수정됨] .Value 제거
        lastPointTicks = GameManager.Instance.dateTime.Value.Ticks;

        // 매니저들에게 게임 시작 알림
        blockGridManager.InitializeGrid(CurrentLevel);
        StartCoroutine(WaitBlockInit());
    }
    private IEnumerator WaitBlockInit()
    {
        yield return new WaitUntil(() => blockGridManager.IsInit);
        logicManager.Initialize(blockGridManager, uiManager);
        inputHandler.EnableInput(true);
        uiManager.StartTimers(isTimeFreeze);
        uiManager.StartSearchCool();
        uiManager.StartExplodeCool();

        CheckGameState();
        StartCoroutine(CheckFinish());

        searchTime = GameManager.Instance.dateTime.Value;
        explodeTime = GameManager.Instance.dateTime.Value;
    }

    private int continueCount;
    public const int MaxContinueCount = 3;
    public void ContinueGame()
    {
        Continue cd = DataManager.Instance.Get<Continue>().SingleOrDefault(x => x.idx == continueCount);
        if (!DataManager.Instance.userData.Use(cd.goodsType, cd.goodsAmount))
        {
            GameResult();
            return;
        }
        isGameFinished = false;
        FinishTime = GameManager.Instance.dateTime.Value.AddSeconds(cd.addSeconds);
        inputHandler.EnableInput(true);
        uiManager.StartTimers();

        StartCoroutine(CheckFinish());

        continueCount++;
    }

    private IEnumerator CheckFinish()
    {
        yield return new WaitUntil(() => blockGridManager.IsInit);
        while (!isGameFinished)
        {
            // [수정됨] .Value 제거 및 Nullable 체크 방식 변경
            bool timeUp = !isTimeFreeze && GameManager.Instance.dateTime.Value.Ticks > FinishTime.Ticks;
            bool blocksLeft = blockGridManager.HasActiveBlocks();

            if (timeUp || !blocksLeft)
            {
                // isGameFinished = true; // GameResult에서 처리하도록 변경
                break;
            }
            yield return null;
        }
        inputHandler.EnableInput(false);
        if(!isTimeFreeze && !isGameFinished && DataManager.Instance.CanContinue(continueCount) && blockGridManager.HasActiveBlocks())
        {
            UIManager.Instance.Open<PopupContinue>().SetData(DataManager.Instance.Get<GameData.Continue>().SingleOrDefault(x => x.idx == continueCount));
        }
        else
        {
            GameResult();
        }
    }

    public void Finish()
    {
        isGameFinished = true;
    }

    public void GameResult()
    {
        if (isGameFinished) return;
        Finish();

        inputHandler.EnableInput(false);
        // 점수 기록
        if (DataManager.Instance.userData.IsNewRecord(CurrentLevel.level, currentPoint.Value, true))
        {
            FirebaseManager.Instance.SubmitScore(CurrentLevel.level, GameManager.Instance.dateTime.Value.ToDateText(), currentPoint.Value);
        }
        if (DataManager.Instance.userData.IsNewRecord(CurrentLevel.level, currentPoint.Value, false))
        {
            FirebaseManager.Instance.SubmitScore(CurrentLevel.level, FirebaseManager.KEY.RANKING_ALL, currentPoint.Value);
        }

        // 보상 계산
        int total = CurrentLevel.row * CurrentLevel.column;
        int maxPoint = total + (total / 2) * bonusMaxPoint;
        float pointRate = maxPoint > 0 ? (float)currentPoint.Value / maxPoint : 0;
        int exp = Mathf.FloorToInt(CurrentLevel.exp * pointRate);
        int gold = Mathf.FloorToInt(CurrentLevel.gold * pointRate);
        if (DataManager.Instance.userData.isVIP)
        {
            exp *= 2;
            gold *= 2;
        }

        // 결과 팝업 및 보상 지급
        uiManager.ShowResultPopup(currentPoint.Value, exp, gold);
        DataManager.Instance.userData.ChargeExp(exp);
        DataManager.Instance.userData.Charge(GameData.GoodsType.Gold, gold);

        DataManager.Instance.userData.DoQuest(QuestType.finish);
        if (!blockGridManager.HasActiveBlocks()) DataManager.Instance.userData.DoQuest(QuestType.allClear);
    }

    //public void ClickPause()
    //{
    //    IsPaused = true;
    //    uiManager.ShowPausePopup(
    //        onConfirm: () => {
    //            blockGridManager.ReturnAllBlocks();
    //            GameManager.Instance.GoScene(GameManager.Scene.Main);
    //        },
    //        onCancel: () => IsPaused = false
    //    );
    //}

    // --- 이벤트 핸들러 ---
    private void HandleValidSelection(List<Block> selectedBlocks)
    {
        // [수정됨] .Value 제거
        int bonus = Math.Max(0, bonusMaxPoint - Mathf.FloorToInt((float)(GameManager.Instance.dateTime.Value.Ticks - lastPointTicks) / TimeSpan.TicksPerSecond));
        if (bonus > 0)
        {
            uiManager.ShowBonusText($"+{bonus}");
        }

        currentPoint.Value += selectedBlocks.Count + bonus;
        // [수정됨] .Value 제거
        lastPointTicks = GameManager.Instance.dateTime.Value.Ticks;

        foreach (var block in selectedBlocks)
        {
            block.Break();
        }

        // 튜토리얼 비활성화
        uiManager.HideTutorial();

        if (blockGridManager.HasActiveBlocks())
        {
            CheckGameState();
        }
    }
    
    public void CheckGameState()
    {
        if (!logicManager.IsInit) return;
        logicManager.FindAllHints();
        var gameState = logicManager.GetGameState();
        
        if (gameState == PuzzleLogicManager.GameState.Continue) return;

        if (gameState == PuzzleLogicManager.GameState.NoMoreMatch ||
            (gameState == PuzzleLogicManager.GameState.NeedShuffle && !DataManager.Instance.userData.Has(GameData.GoodsType.Shuffle, 1)))
        {
            IsPaused = true;
            inputHandler.EnableInput(false);
            UIManager.Instance.Message.Show(Message.Type.Confirm, TextManager.Get("NoMoreMatch"), callback : (bool confirm) => { GameResult(); });
        }
        else // NeedShuffle
        {
            if (DataManager.Instance.userData.IsTutorial)
            {
                blockGridManager.ShuffleBlocks();
                CheckGameState();
                return;
            }

            UIManager.Instance.Message.Show(Message.Type.Ask, TextManager.Get("NeedShuffle"), callback : confirm => {
                if (confirm && DataManager.Instance.userData.Use(GameData.GoodsType.Shuffle, 1))
                {
                    blockGridManager.ShuffleBlocks();
                    CheckGameState();
                }
                else
                {
                    GameResult();
                }
            });
        }
    }

    public void AddSeconds(float sec)
    {
        FinishTime = FinishTime.AddSeconds(sec);
        uiManager.ShowTimeBonusText($"+{sec}sec");
    }

    public void ReturnBlockObj()
    {
        blockGridManager.ReturnAllBlocks();
    }

    private DateTime searchTime = DateTime.MinValue;
    private DateTime explodeTime = DateTime.MinValue;
    public void Shuffle()
    {
        if (DataManager.Instance.userData.Use(GameData.GoodsType.Shuffle, 1))
        {
            blockGridManager.ShuffleBlocks();
            CheckGameState();
        }
    }

    public void Search()
    {
        if (GameManager.Instance.dateTime.Value < searchTime.AddSeconds(DataManager.Instance.SearchTerm))
            return;
        searchTime = GameManager.Instance.dateTime.Value;
        logicManager.UseSearchAbility();
        uiManager.StartSearchCool();
    }

    public void Explode()
    {
        if (GameManager.Instance.dateTime.Value < explodeTime.AddSeconds(DataManager.Instance.ExplodeTerm))
            return;
        explodeTime = GameManager.Instance.dateTime.Value;

        logicManager.UseExplodeAbility();
        uiManager.StartExplodeCool();
    }
}