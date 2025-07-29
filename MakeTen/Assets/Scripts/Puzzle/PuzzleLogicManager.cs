using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class PuzzleLogicManager : MonoBehaviour
{
    public enum GameState { Continue, NeedShuffle, NoMoreMatch }
    public event Action<List<Block>> OnAbilityBlocksRemoved;

    private BlockGridManager blockGridManager;
    private PuzzleUIManager uiManager;
    private const int TargetSumNum = 10;

    private Dictionary<Vector2Int, Vector2Int> hints = new Dictionary<Vector2Int, Vector2Int>();
    private DateTime searchCooldownTime;
    private DateTime explodeCooldownTime;

    private Dictionary<Vector2Int, Block> blockMap;

    public bool IsInit = false;
    public void Initialize(BlockGridManager gridManager, PuzzleUIManager uiManager)
    {
        blockGridManager = gridManager;
        this.uiManager = uiManager;
        var now = GameManager.Instance.dateTime.Value;
        searchCooldownTime = now;
        explodeCooldownTime = now;

        var allBlocks = gridManager.GetAllBlocks();
        if (allBlocks != null)
        {
            // 각 블록의 좌표를 키로 사용하여 딕셔너리를 생성합니다. O(1) 시간 복잡도로 블록을 조회할 수 있습니다.
            blockMap = allBlocks.ToDictionary(b => new Vector2Int(b.column, b.row));
        }
        IsInit = true;
    }

    /// <summary>
    /// 현재 게임 보드에서 가능한 모든 힌트를 찾습니다.
    /// </summary>
    public void FindAllHints()
    {
        var blocks = blockGridManager.GetAllBlocks();
        if (blocks == null || !blocks.Any()) return;

        hints.Clear();
        var currentLevel = PuzzleManager.Instance.CurrentLevel;

        foreach (var block in blocks)
        {
            //if (block.num == 0) continue;

            // CheckCombinations를 한 번만 호출합니다.
            Vector2Int result = CheckCombinations(block, currentLevel);
            if (result != default)
            {
                hints[new Vector2Int(block.column, block.row)] = result;
            }
        }
    }

    /// <summary>
    /// 현재 게임 상태를 반환합니다.
    /// </summary>
    public GameState GetGameState()
    {
        if (!blockGridManager.IsInit) return GameState.Continue;
        if (hints.Count > 0)
        {
            ShowTutorial();
            return GameState.Continue;
        }

        var aliveBlocks = blockGridManager.GetAllBlocks().Where(x => x.num > 0).Select(x => x.num).ToArray();
        return HasAnyCombinationSum(aliveBlocks) ? GameState.NeedShuffle : GameState.NoMoreMatch;
    }

    // [TODO] 힌트 보여주기 기능 구현 (예: 일정 시간 후 랜덤 힌트 표시)
    public int tutorialCount = 3;
    private void ShowTutorial()
    {
        if (DataManager.Instance.userData.IsTutorial && tutorialCount > 0)
        {
            var list = hints.ToList();
            var show = list[UnityEngine.Random.Range(0, list.Count)];
            uiManager.ShowTutorial(blockGridManager.GetAllBlocks().SingleOrDefault(block => block.column == show.Key.x && block.row == show.Key.y), blockGridManager.GetAllBlocks().SingleOrDefault(block => block.column == show.Value.x && block.row == show.Value.y));
            tutorialCount--;
        }
    }

    #region Special Abilities
    public void UseSearchAbility()
    {
        if (hints.Count == 0) return;
        if (GameManager.Instance.dateTime.Value < searchCooldownTime) return;
        if (!DataManager.Instance.userData.Use(GameData.GoodsType.Search, 1)) return;

        var hintToShow = GetRandomHint();
        foreach (var block in hintToShow)
        {
            block.Focus(true);
        }

        float term = DataManager.Instance.SearchTerm;
        searchCooldownTime = GameManager.Instance.dateTime.Value.AddSeconds(term);
        // [TODO] 쿨타임 UI 업데이트 로직 호출
    }

    public void UseExplodeAbility()
    {
        if (hints.Count == 0) return;
        if (GameManager.Instance.dateTime.Value < explodeCooldownTime) return;
        if (!DataManager.Instance.userData.Use(GameData.GoodsType.Explode, 1)) return;

        var blocksToExplode = GetRandomHint();
        OnAbilityBlocksRemoved?.Invoke(blocksToExplode);

        float term = DataManager.Instance.ExplodeTerm;
        explodeCooldownTime = GameManager.Instance.dateTime.Value.AddSeconds(term);
        // [TODO] 쿨타임 UI 업데이트 로직 호출
    }

    private List<Block> GetRandomHint()
    {
        var list = hints.ToList();
        var show = list[UnityEngine.Random.Range(0, list.Count)];

        var blocks = blockGridManager.GetAllBlocks();
        return blocks.Where(b =>
            b.column >= show.Key.x && b.column <= show.Value.x &&
            b.row >= show.Key.y && b.row <= show.Value.y).ToList();
    }
    #endregion

    #region Combination Check
    /// <summary>
    /// startBlock을 좌상단으로 하는 사각형 조합 중 합이 10이 되는 조합을 찾습니다.
    /// </summary>
    /// <param name="startBlock">검사를 시작할 블록</param>
    /// <param name="level">현재 게임 레벨 정보</param>
    /// <returns>조합을 찾은 경우 사각형의 우하단 좌표, 못 찾은 경우 default</returns>
    /// /// <summary>
    /// 화면 방향에 따라 적절한 조합 탐색 메서드를 호출하는 분기 메서드입니다.
    /// </summary>
    private Vector2Int CheckCombinations(Block startBlock, GameData.GameLevel level)
    {
        if (Util.GetDeviceOrientation() == DeviceOrientation.Portrait)
        {
            return CheckCombinationsPortrait(startBlock, level);
        }
        else
        {
            return CheckCombinationsLandscape(startBlock, level);
        }
    }

    /// <summary>
    /// Portrait(세로) 모드 조합 탐색: startBlock을 좌상단으로 하여 우하단으로 확장하며 탐색합니다. (논리좌표: c++, r++)
    /// </summary>
    private Vector2Int CheckCombinationsPortrait(Block startBlock, GameData.GameLevel level)
    {
        int startC = startBlock.column;
        int startR = startBlock.row;

        for (int endR = startR; endR < level.row; endR++)
        {
            int rectangleSum = 0;
            for (int endC = startC; endC < level.column; endC++)
            {
                int newColumnSum = 0;
                for (int r = startR; r <= endR; r++)
                {
                    if (blockMap.TryGetValue(new Vector2Int(endC, r), out Block block) && block.num > 0)
                    {
                        newColumnSum += block.num;
                    }
                }
                rectangleSum += newColumnSum;

                if (endC == startC && endR == startR) continue;
                if (rectangleSum == TargetSumNum) return new Vector2Int(endC, endR);
                if (rectangleSum > TargetSumNum) break;
            }
        }
        return default;
    }

    /// <summary>
    /// Landscape(가로) 모드 조합 탐색: startBlock을 시각적 좌상단으로 하여 시각적 우하단으로 확장하며 탐색합니다. (논리좌표: c++, r--)
    /// </summary>
    private Vector2Int CheckCombinationsLandscape(Block startBlock, GameData.GameLevel level)
    {
        int startC = startBlock.column;
        int startR = startBlock.row;

        // 시각적 '아래'로 확장 (논리적 column 증가)
        for (int endC = startC; endC < level.column; endC++)
        {
            int rectangleSum = 0;
            // 시각적 '오른쪽'으로 확장 (논리적 row 감소)
            for (int endR = startR; endR >= 0; endR--)
            {
                int newRowSum = 0;
                for (int c = startC; c <= endC; c++)
                {
                    if (blockMap.TryGetValue(new Vector2Int(c, endR), out Block block) && block.num > 0)
                    {
                        newRowSum += block.num;
                    }
                }
                rectangleSum += newRowSum;

                if (endC == startC && endR == startR) continue;
                if (rectangleSum == TargetSumNum) return new Vector2Int(endC, endR);
                if (rectangleSum > TargetSumNum) break;
            }
        }
        return default;
    }
    //private Vector2Int CheckCombinations(Block startBlock, GameData.GameLevel level)
    //{
    //    const int TargetSum = 10;
    //    int startC = startBlock.column;
    //    int startR = startBlock.row;

    //    // (startC, startR)을 좌상단으로, (endR, endC)를 우하단으로 하는 모든 사각형을 검사합니다.
    //    for (int endR = startR; endR < level.row; endR++)
    //    {
    //        // 현재 높이(endR)에서, 사각형의 너비가 변할 때의 합계를 저장할 변수
    //        int rectangleSum = 0;

    //        for (int endC = startC; endC < level.column; endC++)
    //        {
    //            // --- 수정된 부분 시작 ---
    //            // 새로 추가된 '열'의 합계만 계산합니다.
    //            int newColumnSum = 0;
    //            for (int r = startR; r <= endR; r++)
    //            {
    //                if (blockMap.TryGetValue(new Vector2Int(endC, r), out Block block) && block.num > 0)
    //                {
    //                    newColumnSum += block.num;
    //                }
    //            }
    //            // 이전 사각형의 합계에 새 열의 합계만 더해줍니다.
    //            rectangleSum += newColumnSum;
    //            // --- 수정된 부분 끝 ---

    //            // 1x1 크기의 사각형(자기 자신)은 조합으로 치지 않음
    //            if (endC == startC && endR == startR) continue;

    //            // 합이 10이면 성공
    //            if (rectangleSum == TargetSum)
    //            {
    //                return new Vector2Int(endC, endR);
    //            }

    //            // 합이 10을 초과하면 더 이상 오른쪽으로 확장하는 것은 무의미하므로 다음 행으로 넘어갑니다. (최적화)
    //            if (rectangleSum > TargetSum)
    //            {
    //                break;
    //            }
    //        }
    //    }

    //    // 모든 사각형을 검사했지만 조합을 찾지 못했습니다.
    //    return default;
    //}


    private bool HasAnyCombinationSum(int[] nums)
    {
        return CheckSumRecursive(nums, 0, 0, 0);
    }

    private bool CheckSumRecursive(int[] nums, int index, int currentSum, int count)
    {
        if (count >= 2 && currentSum == TargetSumNum)
            return true;
        if (index >= nums.Length || currentSum > TargetSumNum)
            return false;

        // 현재 숫자를 포함하는 경우와 포함하지 않는 경우 모두 탐색
        if (CheckSumRecursive(nums, index + 1, currentSum + nums[index], count + 1)) return true;
        if (CheckSumRecursive(nums, index + 1, currentSum, count)) return true;

        return false;
    }
    #endregion
}