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
            // �� ����� ��ǥ�� Ű�� ����Ͽ� ��ųʸ��� �����մϴ�. O(1) �ð� ���⵵�� ����� ��ȸ�� �� �ֽ��ϴ�.
            blockMap = allBlocks.ToDictionary(b => new Vector2Int(b.column, b.row));
        }
        IsInit = true;
    }

    /// <summary>
    /// ���� ���� ���忡�� ������ ��� ��Ʈ�� ã���ϴ�.
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

            // CheckCombinations�� �� ���� ȣ���մϴ�.
            Vector2Int result = CheckCombinations(block, currentLevel);
            if (result != default)
            {
                hints[new Vector2Int(block.column, block.row)] = result;
            }
        }
    }

    /// <summary>
    /// ���� ���� ���¸� ��ȯ�մϴ�.
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

    // [TODO] ��Ʈ �����ֱ� ��� ���� (��: ���� �ð� �� ���� ��Ʈ ǥ��)

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
        // [TODO] ��Ÿ�� UI ������Ʈ ���� ȣ��
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
        // [TODO] ��Ÿ�� UI ������Ʈ ���� ȣ��
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
    /// startBlock�� �»������ �ϴ� �簢�� ���� �� ���� 10�� �Ǵ� ������ ã���ϴ�.
    /// </summary>
    /// <param name="startBlock">�˻縦 ������ ���</param>
    /// <param name="level">���� ���� ���� ����</param>
    /// <returns>������ ã�� ��� �簢���� ���ϴ� ��ǥ, �� ã�� ��� default</returns>
    private Vector2Int CheckCombinations(Block startBlock, GameData.GameLevel level)
    {
        const int TargetSum = 10;
        int startC = startBlock.column;
        int startR = startBlock.row;

        // (startC, startR)�� �»������, (endC, endR)�� ���ϴ����� �ϴ� ��� �簢���� �˻��մϴ�.
        for (int endR = startR; endR < level.row; endR++)
        {
            int rowSum = 0; // ����ȭ: ���� ������ ���� ����

            for (int endC = startC; endC < level.column; endC++)
            {
                // 1x1 ũ���� �簢��(�ڱ� �ڽ�)�� �������� ġ�� ����
                if (endC == startC && endR == startR) continue;

                int currentSum = 0;
                // �簢�� ������ ��� ��� ��ȣ�� ���մϴ�.
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

                // ���� 10�̸� ����
                if (currentSum == TargetSum)
                {
                    return new Vector2Int(endC, endR);
                }

                // ���� 10�� �ʰ��ϸ� �� �̻� ���������� Ȯ���ϴ� ���� ���ǹ��ϹǷ� ���� ������ �Ѿ�ϴ�. (����ȭ)
                if (currentSum > TargetSum)
                {
                    break;
                }
            }
        }

        // ��� �簢���� �˻������� ������ ã�� ���߽��ϴ�.
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

        // ���� ���ڸ� �����ϴ� ���� �������� �ʴ� ��� ��� Ž��
        if (CheckSumRecursive(nums, index + 1, currentSum + nums[index], count + 1)) return true;
        if (CheckSumRecursive(nums, index + 1, currentSum, count)) return true;

        return false;
    }
    #endregion
}