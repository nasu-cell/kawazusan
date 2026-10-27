using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;

public class ClearPanel : MonoBehaviour
{
    private const string BestClearTimeKey = "BestClearTimeSeconds";
    private const string RecordLabel = "\u3055\u3044\u305d\u304f\u304d\u308d\u304f";

    [SerializeField] private TextMeshProUGUI clearText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI winRecordText;
    [SerializeField] private TextMeshProUGUI backText;
    [SerializeField] private GameHUDController gameHUDController;
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private EventTrigger returnTitleButton;
    [SerializeField] private EventTrigger restartGameButton;
    [SerializeField, Min(0.01f)] private float textFadeOutDuration = 0.5f;
    [SerializeField, Min(0f)] private float recordChangeDelay = 1f;
    [SerializeField, Min(0.01f)] private float recordAnimationDuration = 0.45f;
    [SerializeField, Min(1f)] private float recordPunchScale = 1.25f;
    [SerializeField, Min(0f)] private float recordShakeAngle = 8f;
    bool canReturnToTitle = false;
    bool isLeaving = false;

    void Start()
    {
        FindWinRecordTextIfNeeded();
        if (gameHUDController == null)
        {
            gameHUDController = FindFirstObjectByType<GameHUDController>();
        }

        timeText.text = FormatRecord(PlayerPrefs.HasKey(BestClearTimeKey)
            ? PlayerPrefs.GetFloat(BestClearTimeKey)
            : (float?)null);

        clearText.DOFade(0f, 0f);
        timeText.DOFade(0f, 0f);
        if (winRecordText != null)
        {
            winRecordText.DOFade(0f, 0f);
        }
        backText.DOFade(0f, 0f);
        AddClickHandler(returnTitleButton, ReturnToTitleWithAnimator);
        AddClickHandler(restartGameButton, RestartGame);
    }

    void Update()
    {
        if (isLeaving || !Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (IsPointerInsideButton(returnTitleButton))
        {
            ReturnToTitleWithAnimator();
            return;
        }

        if (IsPointerInsideButton(restartGameButton))
        {
            RestartGame();
            return;
        }

        if (canReturnToTitle)
        {
            ReturnToTitleSmooth();
        }
    }

    public void Show()
    {
        canReturnToTitle = true;
    }

    public async UniTask ShowAsync(float elapsedTime)
    {
        bool hasPreviousRecord = PlayerPrefs.HasKey(BestClearTimeKey);
        float previousRecord = hasPreviousRecord
            ? PlayerPrefs.GetFloat(BestClearTimeKey)
            : float.PositiveInfinity;
        bool isNewRecord = !hasPreviousRecord || elapsedTime < previousRecord;

        timeText.text = FormatRecord(hasPreviousRecord ? previousRecord : (float?)null);
        if (isNewRecord)
        {
            PlayerPrefs.SetFloat(BestClearTimeKey, elapsedTime);
            PlayerPrefs.Save();
        }

        canReturnToTitle = false;
        clearText.DOFade(1f, 0.5f);
        await UniTask.Delay(500);
        if (isLeaving) return;

        timeText.DOFade(1f, 0.5f);
        await UniTask.Delay(500);
        if (isLeaving) return;

        if (isNewRecord)
        {
            await UniTask.Delay(Mathf.CeilToInt(recordChangeDelay * 1000f));
            if (isLeaving) return;

            timeText.text = FormatRecord(elapsedTime);
            PlayRecordChangeAnimation();
            await UniTask.Delay(Mathf.CeilToInt(recordAnimationDuration * 1000f));
            if (isLeaving) return;

            if (winRecordText != null)
            {
                winRecordText.DOFade(1f, 0.5f);
                await UniTask.Delay(500);
                if (isLeaving) return;
            }
        }

        backText.DOFade(1f, 0.5f);
        await UniTask.Delay(500);
        if (isLeaving) return;

        canReturnToTitle = true;
    }

    public void ReturnToTitleWithAnimator()
    {
        if (isLeaving || !canReturnToTitle)
        {
            return;
        }

        isLeaving = true;
        ReturnToTitleAsync(true).Forget();
    }

    public void ReturnToTitleSmooth()
    {
        if (isLeaving || !canReturnToTitle)
        {
            return;
        }

        isLeaving = true;
        ReturnToTitleAsync(false).Forget();
    }

    public void RestartGame()
    {
        if (isLeaving || !canReturnToTitle)
        {
            return;
        }

        isLeaving = true;
        PlayTransitionSE();
        sceneTransition.SceneTransWithAnimator(SceneTransition.SceneType.InGame);
    }

    private async UniTask ReturnToTitleAsync(bool useAnimator)
    {
        if (canReturnToTitle)
        {
            gameHUDController?.FadeOutHud();
            clearText.DOKill();
            timeText.DOKill();
            timeText.transform.DOKill();
            timeText.transform.localScale = Vector3.one;
            timeText.transform.localRotation = Quaternion.identity;
            if (winRecordText != null)
            {
                winRecordText.DOKill();
                winRecordText.DOFade(0f, textFadeOutDuration).SetUpdate(true);
            }
            backText.DOKill();
            clearText.DOFade(0f, textFadeOutDuration).SetUpdate(true);
            timeText.DOFade(0f, textFadeOutDuration).SetUpdate(true);
            backText.DOFade(0f, textFadeOutDuration).SetUpdate(true);
            await UniTask.Delay(
                Mathf.CeilToInt(textFadeOutDuration * 1000f),
                ignoreTimeScale: true
            );
        }

        PlayTransitionSE();
        if (useAnimator)
        {
            sceneTransition.SceneTransWithAnimator(SceneTransition.SceneType.Title);
        }
        else
        {
            sceneTransition.SceneTrans(SceneTransition.SceneType.Title);
        }
    }

    private static void AddClickHandler(EventTrigger trigger, UnityEngine.Events.UnityAction callback)
    {
        if (trigger == null)
        {
            return;
        }

        Button button = trigger.GetComponent<Button>();
        if (button == null)
        {
            button = trigger.gameObject.AddComponent<Button>();
        }

        button.targetGraphic = trigger.GetComponent<Graphic>();
        button.onClick.AddListener(callback);

        // These EventTriggers contain no persistent callbacks. Disable them so
        // the standard Button is the single, reliable click receiver.
        trigger.enabled = false;
    }

    private static bool IsPointerInsideButton(EventTrigger trigger)
    {
        if (trigger == null || !trigger.gameObject.activeInHierarchy)
        {
            return false;
        }

        RectTransform rectTransform = trigger.transform as RectTransform;
        return rectTransform != null
            && RectTransformUtility.RectangleContainsScreenPoint(
                rectTransform,
                Input.mousePosition,
                Camera.main
            );
    }

    private static void PlayTransitionSE()
    {
        if (SEManager.Instance != null)
        {
            SEManager.Instance.PlaySE(SoundData.SoundType.TapToStart);
        }
    }

    private void PlayRecordChangeAnimation()
    {
        Transform recordTransform = timeText.transform;
        recordTransform.DOKill();
        recordTransform.localScale = Vector3.one;
        recordTransform.localRotation = Quaternion.identity;

        recordTransform
            .DOPunchScale(Vector3.one * (recordPunchScale - 1f), recordAnimationDuration, 1, 0.5f);
        recordTransform
            .DOShakeRotation(
                recordAnimationDuration,
                new Vector3(0f, 0f, recordShakeAngle),
                12,
                90f,
                false,
                ShakeRandomnessMode.Harmonic
            )
            .OnComplete(() => recordTransform.localRotation = Quaternion.identity);
    }

    private void FindWinRecordTextIfNeeded()
    {
        if (winRecordText != null)
        {
            return;
        }

        foreach (TextMeshProUGUI text in GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.name == "WinRecord" || text.name == "WnRecord")
            {
                winRecordText = text;
                return;
            }
        }
    }

    private static string FormatRecord(float? elapsedTime)
    {
        if (!elapsedTime.HasValue)
        {
            return $"{RecordLabel}\n--:--";
        }

        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedTime.Value));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{RecordLabel}\n{minutes:00}:{seconds:00}";
    }
}
