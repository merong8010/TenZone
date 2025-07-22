using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using System;

public class PuzzleInputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Input Dependencies")]
    [SerializeField] private Camera cam;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Image dragTransform;
    [SerializeField] private BlockGridManager blockGridManager;

    public event Action<List<Block>> OnValidBlocksSelected;

    private const int TargetSumNum = 10;
    private List<Block> focusedBlocks = new List<Block>();
    private Vector2 dragStartPos;
    private bool isDragging = false;
    private bool isInputEnabled = false;

    public void EnableInput(bool enable)
    {
        isInputEnabled = enable;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInputEnabled) return;

        isDragging = true;
        focusedBlocks.Clear();
        UnfocusAllBlocks();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, cam, out dragStartPos);
        dragTransform.gameObject.SetActive(true);
        dragTransform.rectTransform.anchoredPosition = dragStartPos;
        dragTransform.rectTransform.sizeDelta = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, cam, out Vector2 currentPos);
        Vector2 size = currentPos - dragStartPos;

        dragTransform.rectTransform.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
        dragTransform.rectTransform.anchoredPosition = dragStartPos + size / 2;

        UpdateFocusedBlocks();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;
        dragTransform.gameObject.SetActive(false);

        if (focusedBlocks.Count > 0 && focusedBlocks.Sum(x => x.num) == TargetSumNum)
        {
            OnValidBlocksSelected?.Invoke(new List<Block>(focusedBlocks));
        }

        UnfocusAllBlocks();
        focusedBlocks.Clear();
    }

    private void UpdateFocusedBlocks()
    {
        var allBlocks = blockGridManager.GetAllBlocks();
        if (allBlocks == null) return;

        foreach (var block in allBlocks)
        {
            if (block.num == 0) continue;

            bool isInside = dragTransform.rectTransform.IsInside(block.rectTransform);
            bool isFocused = focusedBlocks.Contains(block);

            if (isInside && !isFocused)
            {
                focusedBlocks.Add(block);
                block.Focus(true);
            }
            else if (!isInside && isFocused)
            {
                focusedBlocks.Remove(block);
                block.Focus(false);
            }
        }
    }

    private void UnfocusAllBlocks()
    {
        var allBlocks = blockGridManager.GetAllBlocks();
        if (allBlocks == null) return;

        foreach (var block in allBlocks)
        {
            if (block != null) block.Focus(false);
        }
    }
}