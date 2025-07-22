using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class PuzzleLogicManager : MonoBehaviour
{
    public enum GameState { Continue, NeedShuffle, NoMoreMatch }
    public event Action<List<Block>> OnAbilityBlocksRemoved;

    private BlockGridManager blockGridManager;
    private const int TargetSumNum = 10;

    private Dictionary<Vector2Int, Vector2Int> hints = new Dictionary<Vector2Int, Vector2Int>();
    private DateTime searchCooldownTime;
    private DateTime explodeCooldownTime;

    private Dictionary<Vector2Int, Block> blockMap;

    public bool IsInit = false;
    public void Initialize(BlockGridManager gridManager)
    {
        blockGridManager = gridManager;
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
            if (block.num == 0) continue;

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
        if (hints.Count > 0 || !blockGridManager.IsInit)
        {
            return GameState.Continue;
        }

        var aliveBlocks = blockGridManager.GetAllBlocks().Where(x => x.num > 0).Select(x => x.num).ToArray();
        return HasAnyCombinationSum(aliveBlocks) ? GameState.NeedShuffle : GameState.NoMoreMatch;
    }

    // [TODO] 힌트 보여주기 기능 구현 (예: 일정 시간 후 랜덤 힌트 표시)

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
    private Vector2Int CheckCombinations(Block startBlock, GameData.GameLevel level)
    {
        const int TargetSum = 10;
        int startC = startBlock.column;
        int startR = startBlock.row;

        // (startC, startR)을 좌상단으로, (endC, endR)을 우하단으로 하는 모든 사각형을 검사합니다.
        for (int endR = startR; endR < level.row; endR++)
        {
            int rowSum = 0; // 최적화: 이전 열들의 합을 저장

            for (int endC = startC; endC < level.column; endC++)
            {
                // 1x1 크기의 사각형(자기 자신)은 조합으로 치지 않음
                if (endC == startC && endR == startR) continue;

                int currentSum = 0;
                // 사각형 내부의 모든 블록 번호를 더합니다.
                for (int r = startR; r <= endR; r++)
                {
                    for (int c = startC; c <= endC; c++)
                    {
                        if (blockMap.TryGetValue(new Vector2Int(c, r), out Block block) && block.num > 0)
                        {
                            currentSum += block.num;
                        }
                    }
                }

                // 합이 10이면 성공
                if (currentSum == TargetSum)
                {
                    return new Vector2Int(endC, endR);
                }

                // 합이 10을 초과하면 더 이상 오른쪽으로 확장하는 것은 무의미하므로 다음 행으로 넘어갑니다. (최적화)
                if (currentSum > TargetSum)
                {
                    break;
                }
            }
        }

        // 모든 사각형을 검사했지만 조합을 찾지 못했습니다.
        return default;
    }

    //private Vector2Int CheckBlockColumn(int column, int row, int num)

    //{

    //    if (row == currentLevel.row - 1 && column == currentLevel.column - 1) return default(Vector2Int);

    //    bool endColumn = column + 1 == currentLevel.column;

    //    int searchColumn = endColumn ? 0 : 1;

    //    int searchRow = endColumn ? 1 : 0;

    //    int sum = num;



    //    while (true)

    //    {

    //        if (endColumn)

    //        {

    //            sum += blocks.Where(x => x.column >= column && x.column <= column + searchColumn && x.row == row + searchRow).Sum(x => x.num);

    //        }

    //        else

    //        {

    //            sum += blocks.SingleOrDefault(x => x.column == column + searchColumn && x.row == row + searchRow).num;

    //        }



    //        if (sum == TargetSumNum) return new Vector2Int(column + searchColumn, row + searchRow);

    //        if (sum < TargetSumNum)

    //        {

    //            if (column + searchColumn < currentLevel.column - 1)

    //            {

    //                searchColumn += 1;

    //            }

    //            else

    //            {

    //                if (row + searchRow == currentLevel.row - 1)

    //                {

    //                    break;

    //                }

    //                searchRow += 1;

    //                endColumn = true;

    //            }

    //        }

    //        else

    //        {

    //            if (!endColumn)

    //            {

    //                if (row + searchRow == currentLevel.row - 1)

    //                {

    //                    break;

    //                }

    //                searchColumn -= 1;

    //                searchRow += 1;

    //                endColumn = true;

    //            }

    //            else

    //            {

    //                break;

    //            }

    //        }

    //    }

    //    return default(Vector2Int);



    //}



    //private Vector2Int CheckBlockRow(int column, int row, int num)

    //{

    //    if (row == currentLevel.row - 1 && column == currentLevel.column - 1) return default(Vector2Int);

    //    bool endRow = row + 1 == currentLevel.row;

    //    int searchColumn = endRow ? 1 : 0;

    //    int searchRow = endRow ? 0 : 1;

    //    int sum = num;



    //    while (true)

    //    {

    //        if (endRow)

    //        {

    //            sum += blocks.Where(x => x.row >= row && x.row <= row + searchRow && x.column == column + searchColumn).Sum(x => x.num);

    //        }

    //        else

    //        {

    //            sum += blocks.SingleOrDefault(x => x.column == column + searchColumn && x.row == row + searchRow).num;

    //        }



    //        if (sum == TargetSumNum) return new Vector2Int(column + searchColumn, row + searchRow);

    //        if (sum < TargetSumNum)

    //        {

    //            if (row + searchRow < currentLevel.row - 1)

    //            {

    //                searchRow += 1;

    //            }

    //            else

    //            {

    //                if (column + searchColumn == currentLevel.column - 1)

    //                {

    //                    break;

    //                }

    //                searchColumn += 1;

    //                endRow = true;

    //            }

    //        }

    //        else

    //        {

    //            if (!endRow)

    //            {

    //                if (column + searchColumn == currentLevel.column - 1)

    //                {

    //                    break;

    //                }

    //                searchRow -= 1;

    //                searchColumn += 1;

    //                endRow = true;

    //            }

    //            else

    //            {

    //                break;

    //            }

    //        }

    //    }

    //    return default(Vector2Int);

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