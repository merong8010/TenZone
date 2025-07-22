using UnityEngine;
using System.Linq;
using System.Collections;

public class BlockGridManager : MonoBehaviour
{
    [Header("Grid Configuration")]
    [SerializeField] private Transform blockParent;
    [SerializeField] private RectTransform blocksRT;
    [SerializeField] private Vector2 blockGap;
    [SerializeField] private SafeArea safeArea;

    private Block[] blocks;
    private GameData.GameLevel currentLevel;
    private Block.Data[] blockDatas;

    public Block[] GetAllBlocks() => blocks;
    public bool HasActiveBlocks() => blocks != null && blocks.Any(b => b != null && b.num > 0);
    public bool IsInit = false;
    /// <summary>
    /// ������ ������ ���� ��� �׸��带 �ʱ�ȭ�ϰ� �����մϴ�.
    /// </summary>
    public void InitializeGrid(GameData.GameLevel level)
    {
        currentLevel = level;
        ClearGrid();

        // 1. ��� ������ ����
        CreateBlockData();

        // 2. ������ 10�� ����� �ǵ��� ����
        AdjustDataSum();

        // 3. ��� ������Ʈ ���� �� ��ġ
        StartCoroutine(CreateBlockObjects());
        safeArea.refreshAction = () =>
        {
            StartCoroutine(RepositionBlocks());
        };
    }

    /// <summary>
    /// ��� ��ġ�� ȭ�� ���⿡ �°� �������մϴ�.
    /// </summary>
    public IEnumerator RepositionBlocks()
    {
        yield return Yielders.EndOfFrame;
        if (currentLevel == null || !gameObject.activeInHierarchy || blocks == null) yield break;

        var (blockSize, blockStartPos) = CalculateBlockLayout();
        var orientation = Util.GetDeviceOrientation();

        foreach (var block in blocks)
        {
            block.SetSize(blockSize);
            if (orientation == DeviceOrientation.Portrait)
            {
                block.transform.localPosition = blockStartPos + new Vector2((blockSize.x + blockGap.x) * block.column, (blockSize.y + blockGap.y) * block.row);
            }
            else
            {
                block.transform.localPosition = blockStartPos + new Vector2((blockSize.x + blockGap.x) * (currentLevel.row - block.row - 1), (blockSize.y + blockGap.y) * block.column);
            }
        }

        PuzzleManager.Instance.CheckGameState();
    }

    /// <summary>
    /// ��ϵ��� ���� �����͸� �����ϴ�.
    /// </summary>
    public void ShuffleBlocks()
    {
        Block.Data[] datas = blocks.Select(x => x.GetData()).ToArray().Shuffle();

        for (int i = 0; i < blocks.Length; i++)
        {
            // ��ġ ������ �״�� �ΰ� ���ڿ� ���� �����͸� ��ü
            datas[i].column = blocks[i].column;
            datas[i].row = blocks[i].row;
            blocks[i].SetData(datas[i]);
        }

        PuzzleManager.Instance.CheckGameState();
    }

    private void CreateBlockData()
    {
        blockDatas = new Block.Data[currentLevel.row * currentLevel.column];
        int idx = 0;
        for (int row = 0; row < currentLevel.row; row++)
        {
            for (int column = 0; column < currentLevel.column; column++)
            {
                blockDatas[idx] = new Block.Data(column, row, currentLevel);
                idx++;
            }
        }
    }

    private void AdjustDataSum()
    {
        int remain = blockDatas.Sum(x => x.num) % 10; // TargetSumNum = 10
        if (remain == 0) return;

        while (remain > 0)
        {
            for (int i = 0; i < blockDatas.Length; i++)
            {
                if (blockDatas[i].num > 1)
                {
                    blockDatas[i].num -= 1;
                    remain -= 1;
                    if (remain == 0) break;
                }
            }
        }
    }

    private IEnumerator CreateBlockObjects()
    {
        yield return Yielders.EndOfFrame;

        var (blockSize, blockStartPos) = CalculateBlockLayout();
        var orientation = Util.GetDeviceOrientation();
        blocks = new Block[currentLevel.row * currentLevel.column];
        int idx = 0;

        for (int row = 0; row < currentLevel.row; row++)
        {
            for (int column = 0; column < currentLevel.column; column++)
            {
                Block blockObj = ObjectPooler.Instance.Get<Block>("block", blockParent);
                blockObj.name = $"block_{column}_{row}";
                blockObj.SetSize(blockSize);

                if (orientation == DeviceOrientation.Portrait)
                {
                    blockObj.transform.localPosition = blockStartPos + new Vector2((blockSize.x + blockGap.x) * column, (blockSize.y + blockGap.y) * row);
                }
                else
                {
                    // ���� ����� �� ��ġ ���
                    blockObj.transform.localPosition = blockStartPos + new Vector2((blockSize.x + blockGap.x) * (currentLevel.row - row - 1), (blockSize.y + blockGap.y) * column);
                }

                Block.Data data = blockDatas.Single(x => x.column == column && x.row == row);
                blockObj.SetData(data);
                blocks[idx] = blockObj;
                idx++;
            }
        }
        IsInit = true;
    }

    

    private (Vector2, Vector2) CalculateBlockLayout()
    {
        float width = blocksRT.rect.width;
        float height = blocksRT.rect.height;
        DeviceOrientation orientation = Util.GetDeviceOrientation();
        Vector2 size, startPos;

        if (orientation == DeviceOrientation.Portrait)
        {
            size = new Vector2((width - (blockGap.x * currentLevel.column)) / currentLevel.column, (height - (blockGap.y * currentLevel.row)) / currentLevel.row);
            startPos = new Vector2(-(size.x + blockGap.x) * (currentLevel.column - 1) * 0.5f, -(size.y + blockGap.y) * (currentLevel.row - 1) * 0.5f);
        }
        else
        {
            size = new Vector2((width - (blockGap.y * currentLevel.row)) / currentLevel.row, (height - (blockGap.x * currentLevel.column)) / currentLevel.column);
            startPos = new Vector2(-(size.x + blockGap.y) * (currentLevel.row - 1) * 0.5f, -(size.y + blockGap.x) * (currentLevel.column - 1) * 0.5f);
        }
        return (size, startPos);
    }

    private void ClearGrid()
    {
        if (blocks != null && blocks.Length > 0)
        {
            foreach (var block in blocks)
            {
                if (block != null)
                    ObjectPooler.Instance.ReturnObject("block", block.gameObject);
            }
        }
        blocks = null;
        blockDatas = null;
    }

    public void ReturnAllBlocks()
    {
        ClearGrid();
    }
}