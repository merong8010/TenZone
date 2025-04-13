using DG.Tweening;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationRect : MonoBehaviour
{
    public float timeAnimScale = 0.3f;
    public float timeDelayScale = 0.05f;

    public float timeAnimRight = 0.3f;
    public float timeDelayRight = 0.05f;
    public float timeDelayRightNext = 0f;

    public float timeAnimLeft = 0.3f;
    public float timeDelayLeft = 0.05f;
    public float timeDelayLeftNext = 0f;

    public float timeAnimTop = 0.3f;
    public float timeDelayTop = 0.05f;
    public float timeDelayTopNext = 0f;

    public float timeAnimBot = 0.3f;
    public float timeDelayBot = 0.05f;
    public float timeDelayBotNext = 0f;

    public RectTransform[] rectAnimScale;
    public RectTransform[] rectAnimRight;
    public RectTransform[] rectAnimLeft;
    public RectTransform[] rectAnimTop;
    public RectTransform[] rectAnimBot;

    Vector3 scaleStart = new Vector3(0.0f, 0.0f, 0.0f);

    private Dictionary<RectTransform, Vector2> origin = new Dictionary<RectTransform, Vector2>();
    private void OnEnable()
    {
#if DOTWEEN
        //AnimScaleIn();
        //AnimRightIn();
        //AnimLeftIn();
        //AnimTopIn();
        //AnimBotIn();
#else
                        enabled = false;
#endif
    }

    public void Show(bool show)
    {
        if (show)
        {
            AnimScaleIn();
            AnimRightIn();
            AnimLeftIn();
            AnimTopIn();
            AnimBotIn();
        }
        else
        {
            AnimScaleOut();
            AnimRightOut();
            AnimLeftOut();
            AnimTopOut();
            AnimBotOut();
        }
    }

#if DOTWEEN
    void AnimScaleIn()
    {
        for (int i = 0; i < rectAnimScale.Length; i++)
        {
            if (rectAnimScale[i] == null) continue;
            rectAnimScale[i].localScale = scaleStart;
            rectAnimScale[i].DOScale(Vector3.one, timeAnimScale).SetEase(Ease.OutBack).SetDelay(timeDelayScale + timeDelayScale * i);
        }
    }

    void AnimScaleOut()
    {
        for (int i = 0; i < rectAnimScale.Length; i++)
        {
            if (rectAnimScale[i] == null) continue;
            rectAnimScale[i].localScale = Vector3.one;
            rectAnimScale[i].DOScale(scaleStart, timeAnimScale).SetEase(Ease.OutBack).SetDelay(timeDelayScale + timeDelayScale * i);
        }
    }

    void AnimRightIn()
    {
        for (int i = 0; i < rectAnimRight.Length; i++)
        {
            if (rectAnimRight[i] == null) continue;
            origin.TryAdd(rectAnimRight[i], rectAnimRight[i].anchoredPosition);
            Vector2 vector2 = origin[rectAnimRight[i]];
            rectAnimRight[i].anchoredPosition = new Vector2(vector2.x + 1000, vector2.y);
            rectAnimRight[i].DOAnchorPosX(vector2.x, timeAnimRight).SetEase(Ease.OutCubic).SetDelay(timeDelayRight + timeDelayRightNext * i);
        }
    }

    void AnimRightOut()
    {
        for (int i = 0; i < rectAnimRight.Length; i++)
        {
            if (rectAnimRight[i] == null) continue;
            origin.TryAdd(rectAnimRight[i], rectAnimRight[i].anchoredPosition);
            Vector2 vector2 = origin[rectAnimRight[i]];
            rectAnimRight[i].anchoredPosition = new Vector2(vector2.x, vector2.y);
            rectAnimRight[i].DOAnchorPosX(vector2.x+1000, timeAnimRight).SetEase(Ease.OutCubic).SetDelay(timeDelayRight + timeDelayRightNext * i);
        }
    }

    void AnimLeftIn()
    {
        for (int i = 0; i < rectAnimLeft.Length; i++)
        {
            if (rectAnimLeft[i] == null) continue;
            origin.TryAdd(rectAnimLeft[i], rectAnimLeft[i].anchoredPosition);
            Vector2 vector2 = origin[rectAnimLeft[i]];
            rectAnimLeft[i].anchoredPosition = new Vector2(vector2.x - 1000, vector2.y);
            rectAnimLeft[i].DOAnchorPosX(vector2.x, timeAnimLeft).SetEase(Ease.OutCubic).SetDelay(timeDelayLeft + timeDelayLeftNext * i);
        }
    }

    void AnimLeftOut()
    {
        for (int i = 0; i < rectAnimLeft.Length; i++)
        {
            if (rectAnimLeft[i] == null) continue;
            origin.TryAdd(rectAnimLeft[i], rectAnimLeft[i].anchoredPosition);
            Vector2 vector2 = origin[rectAnimLeft[i]];
            rectAnimLeft[i].anchoredPosition = new Vector2(vector2.x, vector2.y);
            rectAnimLeft[i].DOAnchorPosX(vector2.x-1000, timeAnimLeft).SetEase(Ease.OutCubic).SetDelay(timeDelayLeft + timeDelayLeftNext * i);
        }
    }

    void AnimTopIn()
    {
        for (int i = 0; i < rectAnimTop.Length; i++)
        {
            if (rectAnimTop[i] == null) continue;
            origin.TryAdd(rectAnimTop[i], rectAnimTop[i].anchoredPosition);
            Vector2 vector2 = origin[rectAnimTop[i]];
            rectAnimTop[i].anchoredPosition = new Vector2(vector2.x, vector2.y + 1000);
            rectAnimTop[i].DOAnchorPosY(vector2.y, timeAnimTop).SetEase(Ease.OutCubic).SetDelay(timeDelayTop + timeDelayTopNext * i);
        }
    }

    void AnimTopOut()
    {
        for (int i = 0; i < rectAnimTop.Length; i++)
        {
            if (rectAnimTop[i] == null) continue;
            origin.TryAdd(rectAnimTop[i], rectAnimTop[i].anchoredPosition);
            Vector2 vector2 = origin[rectAnimTop[i]];
            rectAnimTop[i].anchoredPosition = new Vector2(vector2.x, vector2.y);
            rectAnimTop[i].DOAnchorPosY(vector2.y+1000, timeAnimTop).SetEase(Ease.OutCubic).SetDelay(timeDelayTop + timeDelayTopNext * i);
        }
    }

    void AnimBotIn()
    {
        for (int i = 0; i < rectAnimBot.Length; i++)
        {
            if (rectAnimBot[i] == null) continue;
            origin.TryAdd(rectAnimBot[i], rectAnimBot[i].anchoredPosition);
            Vector2 vector2 = origin[rectAnimBot[i]];
            rectAnimBot[i].anchoredPosition = new Vector2(vector2.x, vector2.y - 1000);
            rectAnimBot[i].DOAnchorPosY(vector2.y, timeAnimBot).SetEase(Ease.OutCubic).SetDelay(timeDelayBot + timeDelayBotNext * i);
        }
    }

    void AnimBotOut()
    {
        for (int i = 0; i < rectAnimBot.Length; i++)
        {
            if (rectAnimBot[i] == null) continue;
            origin.TryAdd(rectAnimBot[i], rectAnimBot[i].anchoredPosition);
            Vector2 vector2 = origin[rectAnimBot[i]];
            rectAnimBot[i].anchoredPosition = new Vector2(vector2.x, vector2.y);
            rectAnimBot[i].DOAnchorPosY(vector2.y-1000, timeAnimBot).SetEase(Ease.OutCubic).SetDelay(timeDelayBot + timeDelayBotNext * i);
        }
    }

    
#endif
}