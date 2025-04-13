using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class Title : MonoBehaviour
{
    [SerializeField]
    private Transform blockParent;

    [SerializeField]
    private GameObject loginObj;
    [SerializeField]
    private GameObject tabPlayObj;

    [SerializeField]
    private Text statusText;
   

    public void Awake()
    {
        //blockSize = new Vector2((blocksRT.rect.width - (blockGap.x * currentLevel.column)) / currentLevel.column, (blocksRT.rect.height - (blockGap.y * currentLevel.row)) / currentLevel.row);
        //Debug.Log($"blockSize : {blockSize.x}:{blockSize.y} | {blocksRT.sizeDelta.x} : {blocksRT.sizeDelta.y} | {blockGap.x} : {blockGap.y} | {blocksRT.rect.width} : {blocksRT.rect.height}");
        //blockStartPos = new Vector2(-(blockSize.x + blockGap.x) * (currentLevel.column - 1) * 0.5f, -(blockSize.y + blockGap.y) * (currentLevel.row - 1) * 0.5f);
        //for (int row = 0; row < currentLevel.row; row++)
        //{
        //    for (int column = 0; column < currentLevel.column; column++)
        //    {
        //        Block blockObj = pooler.GetObject<Block>("block", blockParent, blockStartPos + new Vector2((blockSize.x + blockGap.x) * column, (blockSize.y + blockGap.y) * row), Vector3.one);
        //        blockObj.name = $"block_{row}_{column}";
        //        //blockObj.transform.SetParent(blockParent);
        //        //blockObj.transform.localScale = Vector3.one;
        //        //blockObj.transform.localPosition = blockStartPos + new Vector2((blockSize.x + blockGap.x) * column, (blockSize.y + blockGap.y) * row);
        //        //Block block = blockObj.GetComponent<Block>();
        //        //block.SetSize(blockSize);
        //        blocks = blocks.Append(blockObj).ToArray();
        //    }
        //}

        
    }

    

    public void SetStatus(string text, bool showLogins = false, bool showTap = false)
    {
        statusText.text = text;
        loginObj.SetActive(showLogins);
        tabPlayObj.SetActive(showTap);
    }

    public void ClickGuest()
    {

    }

    public void ClickGoogle()
    {

    }

    public void ClickApple()
    { 

    }

    public void GameStart()
    {
        GameManager.Instance.GoScene(GameManager.Scene.Main);
    }
}
