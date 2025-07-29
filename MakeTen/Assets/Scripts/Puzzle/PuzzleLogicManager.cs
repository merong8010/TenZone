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
            //if (block.num == 0) continue;

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
        if (!blockGridManager.IsInit) return GameState.Continue;
        if (hints.Count > 0)
        {
            ShowTutorial();
            return GameState.Continue;
        }

        var aliveBlocks = blockGridManager.GetAllBlocks().Where(x => x.num > 0).Select(x => x.num).ToArray();
        return HasAnyCombinationSum(aliveBlocks) ? GameState.NeedShuffle : GameState.NoMoreMatch;
    }

    // [TODO] ��Ʈ �����ֱ� ��� ���� (��: ���� �ð� �� ���� ��Ʈ ǥ��)
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
    /// /// <summary>
    /// ȭ�� ���⿡ ���� ������ ���� Ž�� �޼��带 ȣ���ϴ� �б� �޼����Դϴ�.
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
    /// Portrait(����) ��� ���� Ž��: startBlock�� �»������ �Ͽ� ���ϴ����� Ȯ���ϸ� Ž���մϴ�. (����ǥ: c++, r++)
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
    /// Landscape(����) ��� ���� Ž��: startBlock�� �ð��� �»������ �Ͽ� �ð��� ���ϴ����� Ȯ���ϸ� Ž���մϴ�. (����ǥ: c++, r--)
    /// </summary>
    private Vector2Int CheckCombinationsLandscape(Block startBlock, GameData.GameLevel level)
    {
        int startC = startBlock.column;
        int startR = startBlock.row;

        // �ð��� '�Ʒ�'�� Ȯ�� (���� column ����)
        for (int endC = startC; endC < level.column; endC++)
        {
            int rectangleSum = 0;
            // �ð��� '������'���� Ȯ�� (���� row ����)
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

    //    // (startC, startR)�� �»������, (endR, endC)�� ���ϴ����� �ϴ� ��� �簢���� �˻��մϴ�.
    //    for (int endR = startR; endR < level.row; endR++)
    //    {
    //        // ���� ����(endR)����, �簢���� �ʺ� ���� ���� �հ踦 ������ ����
    //        int rectangleSum = 0;

    //        for (int endC = startC; endC < level.column; endC++)
    //        {
    //            // --- ������ �κ� ���� ---
    //            // ���� �߰��� '��'�� �հ踸 ����մϴ�.
    //            int newColumnSum = 0;
    //            for (int r = startR; r <= endR; r++)
    //            {
    //                if (blockMap.TryGetValue(new Vector2Int(endC, r), out Block block) && block.num > 0)
    //                {
    //                    newColumnSum += block.num;
    //                }
    //            }
    //            // ���� �簢���� �հ迡 �� ���� �հ踸 �����ݴϴ�.
    //            rectangleSum += newColumnSum;
    //            // --- ������ �κ� �� ---

    //            // 1x1 ũ���� �簢��(�ڱ� �ڽ�)�� �������� ġ�� ����
    //            if (endC == startC && endR == startR) continue;

    //            // ���� 10�̸� ����
    //            if (rectangleSum == TargetSum)
    //            {
    //                return new Vector2Int(endC, endR);
    //            }

    //            // ���� 10�� �ʰ��ϸ� �� �̻� ���������� Ȯ���ϴ� ���� ���ǹ��ϹǷ� ���� ������ �Ѿ�ϴ�. (����ȭ)
    //            if (rectangleSum > TargetSum)
    //            {
    //                break;
    //            }
    //        }
    //    }

    //    // ��� �簢���� �˻������� ������ ã�� ���߽��ϴ�.
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

        // ���� ���ڸ� �����ϴ� ���� �������� �ʴ� ��� ��� Ž��
        if (CheckSumRecursive(nums, index + 1, currentSum + nums[index], count + 1)) return true;
        if (CheckSumRecursive(nums, index + 1, currentSum, count)) return true;

        return false;
    }
    #endregion
}