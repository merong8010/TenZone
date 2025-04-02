using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using System.Text;
using System;
using System.Collections;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private Image dragTransform;

    //public RectTransform canvasRect;  // 캔버스
    //public Image rectImagePrefab;     // 생성할 이미지 프리팹

    //private Image currentImage;
    private Vector2 startPos;
    private bool isDrag;

    [SerializeField]
    private Transform blockParent;
    [SerializeField]
    private GameObject blockObj;
    //[SerializeField]
    private Block[] blocks;

    private const int TargetSumNum = 10;

    private ReactiveProperty<int> currentPoint;
    private ReactiveProperty<float> currentTime;

    [SerializeField]
    private Text pointText;
    [SerializeField]
    private Text timeText;

    [SerializeField]
    private int rowCount;
    [SerializeField]
    private int columnCount;

    [SerializeField]
    private Vector2 blockStartPos;
    [SerializeField]
    private Vector2 blockSize;
    [SerializeField]
    private Vector2 blockGap;

    protected override void Awake()
    {
        base.Awake();
        //InitBlocks();
        Initialize();
        InitBlocks();
    }

    private Coroutine timeCoroutine;

    private void Initialize()
    {
        if (blocks == null) blocks = new Block[] { };

        blockStartPos = new Vector2(-(blockSize.x + blockGap.x) * columnCount * 0.5f, -(blockSize.y + blockGap.y) * rowCount * 0.5f);
        for (int row = 0; row < rowCount; row++)
        {
            for (int column = 0; column < columnCount; column++)
            {
                Block block = Instantiate(blockObj, blockParent).GetComponent<Block>();
                block.transform.localPosition = blockStartPos + new Vector2((blockSize.x + blockGap.x) * column, (blockSize.y + blockGap.y) * row);
                block.SetSize(blockSize);
                blocks = blocks.Append(block).ToArray();
            }
        }

        currentPoint = new ReactiveProperty<int>();
        currentPoint.Subscribe(x => { pointText.text = new StringBuilder().Append("point : ").Append(x).ToString(); });
        
        currentTime = new ReactiveProperty<float>();
        currentTime.Subscribe(x => { timeText.text = new StringBuilder().Append("time : ").Append(Mathf.RoundToInt(x)).ToString(); });
    }

    public void InitBlocks()
    {
        currentPoint.Value = 0;
        
        //System.Random rand = new System.Random();
        for(int i = 0; i < blocks.Length; i++)
        {
            blocks[i].Reset();
            if( i == blocks.Length -1)
            {
                int sum = blocks.Sum(x => x.num);
                int lastNum = TargetSumNum - (sum % TargetSumNum);
                blocks[i].Init(lastNum);
            }
            else
            {
                blocks[i].Init(Util.GenerateGaussianRandom());
            }
        }

        currentTime.Value = 0f;
        if (timeCoroutine != null) StopCoroutine(timeCoroutine);
        timeCoroutine = StartCoroutine(CheckTime());
    }

    private IEnumerator CheckTime()
    {
        while(blocks.ToList().Exists(x => x.num > 0))
        {
            yield return new WaitForEndOfFrame();
            currentTime.Value += UnityEngine.Time.deltaTime;
        }
    }


    [SerializeField]
    private Camera cam;
    [SerializeField]
    private RectTransform canvasRect;
    public void OnClick()
    {
        isDrag = true;

        //startPos = Input.mousePosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, cam, out startPos);
        dragTransform.gameObject.SetActive(true);
        dragTransform.rectTransform.anchoredPosition = startPos;
        dragTransform.rectTransform.sizeDelta = Vector2.zero;
    }

    public void OnRelease()
    {
        if (focus.Length > 0)
        {
            if (focus.Sum(x => x.num) == TargetSumNum)
            {
                for (int i = 0; i < focus.Length; i++)
                {
                    focus[i].Break();
                }
                currentPoint.Value += focus.Length;
            }
        }

        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].Focus(false);
        }

        isDrag = false;
        dragTransform.gameObject.SetActive(false);
    }
    private Block[] focus = new Block[] { };
    void Update()
    {
        if(isDrag)
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

        //if (Input.touchCount > 0)
        //{
        //    Touch touch = Input.GetTouch(0);
        //    Block[] focus = new Block[] { };
        //    switch (touch.phase)
        //    {
        //        case TouchPhase.Began:
        //            // 터치 시작 시, 이미지 생성
        //            startPos = touch.position;
        //            dragTransform.gameObject.SetActive(true);
        //            //currentImage = Instantiate(rectImagePrefab, canvasRect);
        //            dragTransform.rectTransform.anchoredPosition = startPos;
        //            dragTransform.rectTransform.sizeDelta = Vector2.zero;

        //            isDrag = true;
        //            break;

        //        case TouchPhase.Moved:
        //            if (isDrag)
        //            {
        //                // 현재 터치 위치까지 크기 조정
        //                Vector2 currentPos = touch.position;
        //                Vector2 size = currentPos - startPos;
        //                dragTransform.rectTransform.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));

        //                // 위치 조정 (좌상단 기준)
        //                dragTransform.rectTransform.anchoredPosition = startPos + size / 2;

        //                focus = blocks.Where(x => dragTransform.rectTransform.rect.Contains(x.transform.position)).ToArray();
        //                for (int i = 0; i < blocks.Length; i++)
        //                {
        //                    blocks[i].Focus(focus.Contains(blocks[i]));
        //                }
        //            }
        //            break;

        //        case TouchPhase.Ended:
        //            if(isDrag)
        //            {
        //                if(focus.Length > 0)
        //                {
        //                    if(focus.Sum(x => x.num) == TargetSumNum)
        //                    {
        //                        for(int i = 0; i < focus.Length; i++)
        //                        {
        //                            focus[i].Break();
        //                        }
        //                        currentPoint += focus.Length;
        //                    }
        //                }

        //                isDrag = false;
        //                dragTransform.gameObject.SetActive(false);
        //            }
                    
                    
        //            break;
        //    }
        //}
    }
}
