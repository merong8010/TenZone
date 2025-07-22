using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using System.Collections;

public class PuzzleUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMPro.TextMeshProUGUI pointText;
    [SerializeField] private TMPro.TextMeshProUGUI timeText;
    [SerializeField] private Image timeBar;
    [SerializeField] private Image searchCoolImage;
    [SerializeField] private Image explodeCoolImage;
    [SerializeField] private RectTransform tutorialRT;
    [SerializeField] private Camera cam;

    private PuzzleManager puzzleManager;
    private IDisposable pointDispose;
    private IDisposable timeDispose;

    public void Initialize(PuzzleManager manager, ReactiveProperty<int> currentPoint)
    {
        puzzleManager = manager;
        pointDispose = currentPoint.Subscribe(x => pointText.text = x.ToString("n0"));
    }

    public void StartTimers()
    {
        timeDispose?.Dispose();
        timeDispose = GameManager.Instance.reactiveTime.Subscribe(x =>
        {
            if (x.Ticks <= puzzleManager.FinishTime.Ticks)
            {
                TimeSpan timeSpan = (puzzleManager.FinishTime - x);
                timeText.text = string.Format("{0}:{1:00}.{2}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 100);

                int totalTime = puzzleManager.CurrentLevel != null ? puzzleManager.CurrentLevel.time : 1;
                timeBar.fillAmount = (float)timeSpan.TotalSeconds / totalTime;
            }
        });

        StartSearchCool();
        StartExplodeCool();
    }

    public void ClickPause()
    {
        //PuzzleManager.Instance.IsPaused = true;
        UIManager.Instance.Message.Show(Message.Type.Ask, TextManager.Get("PuzzleQuit"), callback: (yes) =>
        {
            if (yes)
            {
                PuzzleManager.Instance.Finish();
                GameManager.Instance.GoScene(GameManager.Scene.Main);
            }
            //else PuzzleManager.Instance.IsPaused = false;
        });
    }

    public void StartSearchCool()
    {
        StartCoroutine(CheckSearchCool(GameManager.Instance.dateTime.Value.AddSeconds(DataManager.Instance.SearchTerm), DataManager.Instance.SearchTerm));
    }

    private IEnumerator CheckSearchCool(DateTime coolFinish, float max)
    {
        while ((GameManager.Instance.dateTime != null && coolFinish.Ticks > GameManager.Instance.dateTime.Value.Ticks) || GameManager.Instance.dateTime == null)
        {
            if (GameManager.Instance.dateTime != null)
                searchCoolImage.fillAmount = (coolFinish.Ticks - GameManager.Instance.dateTime.Value.Ticks) / (max * TimeSpan.TicksPerSecond);
            yield return Yielders.EndOfFrame;
            Debug.Log($"{GameManager.Instance.dateTime.Value.Ticks} | {searchCoolImage.fillAmount}");
        }
    }

    public void StartExplodeCool()
    {
        StartCoroutine(CheckExplodeCool(GameManager.Instance.dateTime.Value.AddSeconds(DataManager.Instance.ExplodeTerm), DataManager.Instance.ExplodeTerm));
    }

    private IEnumerator CheckExplodeCool(DateTime coolFinish, float max)
    {
        while ((GameManager.Instance.dateTime != null && coolFinish.Ticks > GameManager.Instance.dateTime.Value.Ticks) || GameManager.Instance.dateTime == null)
        {
            if (GameManager.Instance.dateTime != null)
                explodeCoolImage.fillAmount = (coolFinish.Ticks - GameManager.Instance.dateTime.Value.Ticks) / (max * TimeSpan.TicksPerSecond);
            yield return Yielders.EndOfFrame;
        }
    }

    public void UpdateCooldownImage(Image image, float fillAmount)
    {
        if (image != null)
        {
            image.fillAmount = fillAmount;
        }
    }

    public void ShowResultPopup(int score, int exp, int gold)
    {
        UIManager.Instance.Open<PopupResult>().SetData(score, exp, gold);
    }

    public void ShowMessage(Message.Type type, string textKey, Action<bool> callback)
    {
        UIManager.Instance.Message.Show(type, TextManager.Get(textKey));
    }

    public void ShowPausePopup(Action onConfirm, Action onCancel)
    {
        ShowMessage(Message.Type.Ask, "PuzzleQuit", (yes) => {
            if (yes) onConfirm?.Invoke();
            else onCancel?.Invoke();
        });
    }

    public void ShowBonusText(string text)
    {
        Vector2 pointTextPos = RectTransformUtility.WorldToScreenPoint(cam, pointText.transform.position) - ((Vector2)Util.GetScreenSize() * 0.5f);
        ObjectPooler.Instance.Get<Effect>("textPointBonus", transform, pointTextPos, autoReturnTime: 2f).SetText(text);
    }

    public void ShowTimeBonusText(string text)
    {
        Vector2 timeTextPos = RectTransformUtility.WorldToScreenPoint(cam, timeText.transform.position) - ((Vector2)Util.GetScreenSize() * 0.5f);
        ObjectPooler.Instance.Get<Effect>("textPointBonus", transform, timeTextPos, autoReturnTime: 2f).SetText(text);
    }

    public void ShowTutorial()
    {
        // 튜토리얼 UI 표시 로직 (필요시 PuzzleLogicManager와 연동)
        if (DataManager.Instance.userData.IsTutorial)
            tutorialRT.gameObject.SetActive(true);
    }

    public void HideTutorial()
    {
        if (tutorialRT.gameObject.activeSelf)
        {
            tutorialRT.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        pointDispose?.Dispose();
        timeDispose?.Dispose();
    }
}