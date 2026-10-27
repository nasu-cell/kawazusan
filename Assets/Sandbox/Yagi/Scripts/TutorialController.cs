using DG.Tweening;
using TMPro;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] tutorialText;
    [SerializeField] private SpriteRenderer tapIcon;

    [Header("Tutorial Flow")]
    [SerializeField] private TongueController tongueController;
    [SerializeField] private SatietyController satietyController;
    [SerializeField] private InGameController inGameController;
    [SerializeField] private FlySpawner flySpawner;
    [SerializeField] private BeeSpawner beeSpawner;

    [Header("HUD Targets")]
    [SerializeField] private RectTransform satietySlider;
    [SerializeField] private RectTransform timer;

    [Header("Animation")]
    [SerializeField, Min(0f)] private float moveDistance = 10f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.5f;
    [SerializeField, Min(0f)] private float initialTutorialDelay = 1f;
    [SerializeField, Min(0f)] private float secondTutorialDelay = 0.5f;
    [SerializeField, Min(0f)] private float timerStartDelay = 0.2f;

    private Vector2[] tutorialBasePositions;
    private Color[] tutorialBaseColors;
    private Vector3 tapIconBasePosition;
    private Color tapIconBaseColor;
    private Vector2 satietyBasePosition;
    private Vector2 timerBasePosition;
    private CanvasGroup satietyCanvasGroup;
    private CanvasGroup timerCanvasGroup;
    private Sequence flowSequence;
    private Tween timerStartTween;
    private bool waitingForFirstExtension;
    private bool waitingForSatietyAdded;

    private void Awake()
    {
        CachePresentationState();
        HideInitialElements();
        inGameController.PauseTimer();

        if (flySpawner == null)
        {
            flySpawner = FindAnyObjectByType<FlySpawner>();
        }

        if (beeSpawner == null)
        {
            beeSpawner = FindAnyObjectByType<BeeSpawner>();
        }

        if (flySpawner != null)
        {
            flySpawner.StopSpawning();
        }

        if (beeSpawner != null)
        {
            beeSpawner.StopSpawning();
        }

        tongueController.SetInputEnabled(false);
    }

    private void OnEnable()
    {
        tongueController.ExtensionStarted += OnTongueExtensionStarted;
        satietyController.Added += OnSatietyAdded;
    }

    private void Start()
    {
        waitingForFirstExtension = true;
        PlayFirstTutorialFadeIn();
    }

    private void OnDisable()
    {
        tongueController.ExtensionStarted -= OnTongueExtensionStarted;
        satietyController.Added -= OnSatietyAdded;
        flowSequence?.Kill();
        timerStartTween?.Kill();
        flowSequence = null;
        timerStartTween = null;
    }

    private void CachePresentationState()
    {
        tutorialBasePositions = new Vector2[tutorialText.Length];
        tutorialBaseColors = new Color[tutorialText.Length];

        for (int i = 0; i < tutorialText.Length; i++)
        {
            tutorialBasePositions[i] = tutorialText[i].rectTransform.anchoredPosition;
            tutorialBaseColors[i] = tutorialText[i].color;
        }

        tapIconBasePosition = tapIcon.transform.localPosition;
        tapIconBaseColor = tapIcon.color;
        satietyBasePosition = satietySlider.anchoredPosition;
        timerBasePosition = timer.anchoredPosition;
        satietyCanvasGroup = GetOrAddCanvasGroup(satietySlider);
        timerCanvasGroup = GetOrAddCanvasGroup(timer);
    }

    private void HideInitialElements()
    {
        for (int i = 0; i < tutorialText.Length; i++)
        {
            SetTextHidden(i);
        }

        SetTapIconHidden();
        SetHudHidden(satietySlider, satietyCanvasGroup, satietyBasePosition);
        SetHudHidden(timer, timerCanvasGroup, timerBasePosition);
    }

    private void PlayFirstTutorialFadeIn()
    {
        flowSequence?.Kill();
        flowSequence = DOTween.Sequence().SetUpdate(true)
            .AppendInterval(initialTutorialDelay)
            .Append(CreateTextFadeIn(0))
            .Join(CreateTapIconFadeIn())
            .OnComplete(() =>
            {
                flowSequence = null;
                tongueController.SetInputEnabled(true);
            });
    }

    private void OnTongueExtensionStarted()
    {
        if (!waitingForFirstExtension)
        {
            return;
        }

        waitingForFirstExtension = false;
        flowSequence?.Kill();
        flowSequence = DOTween.Sequence().SetUpdate(true)
            .Join(CreateTextFadeOut(0))
            .Join(CreateTapIconFadeOut())
            .Append(CreateHudFadeIn(
                satietySlider,
                satietyCanvasGroup,
                satietyBasePosition
            ))
            .AppendInterval(secondTutorialDelay)
            .Append(CreateTextFadeIn(1))
            .OnComplete(OnSecondTutorialShown);
    }

    private void OnSecondTutorialShown()
    {
        flowSequence = null;
        waitingForSatietyAdded = true;
        tongueController.SetInputEnabled(true);

        if (flySpawner != null)
        {
            flySpawner.StartSpawning();
        }

        if (beeSpawner != null)
        {
            beeSpawner.StartSpawning();
        }

        flowSequence = DOTween.Sequence().SetUpdate(true)
            .Append(CreateHudFadeIn(timer, timerCanvasGroup, timerBasePosition))
            .OnComplete(() =>
            {
                flowSequence = null;
                timerStartTween = DOVirtual.DelayedCall(
                    timerStartDelay,
                    inGameController.StartTimer
                ).SetUpdate(true).OnComplete(() => timerStartTween = null);
            });
    }

    private void OnSatietyAdded(int amount)
    {
        if (!waitingForSatietyAdded || amount <= 0)
        {
            return;
        }

        waitingForSatietyAdded = false;
        CreateTextFadeOut(1).SetUpdate(true);
    }

    private Tween CreateTextFadeIn(int index)
    {
        TextMeshProUGUI text = tutorialText[index];
        RectTransform rectTransform = text.rectTransform;
        rectTransform.anchoredPosition = GetHiddenPosition(tutorialBasePositions[index]);
        text.color = WithAlpha(tutorialBaseColors[index], 0f);

        return DOTween.Sequence()
            .Join(rectTransform.DOAnchorPos(tutorialBasePositions[index], fadeDuration)
                .SetEase(Ease.OutCubic))
            .Join(text.DOFade(tutorialBaseColors[index].a, fadeDuration)
                .SetEase(Ease.OutQuad));
    }

    private Tween CreateTextFadeOut(int index)
    {
        TextMeshProUGUI text = tutorialText[index];

        return DOTween.Sequence()
            .Join(text.rectTransform
                .DOAnchorPos(GetHiddenPosition(tutorialBasePositions[index]), fadeDuration)
                .SetEase(Ease.InCubic))
            .Join(text.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));
    }

    private Tween CreateTapIconFadeIn()
    {
        tapIcon.transform.localPosition = GetHiddenPosition(tapIconBasePosition);
        tapIcon.color = WithAlpha(tapIconBaseColor, 0f);

        return DOTween.Sequence()
            .Join(tapIcon.transform.DOLocalMove(tapIconBasePosition, fadeDuration)
                .SetEase(Ease.OutCubic))
            .Join(tapIcon.DOFade(tapIconBaseColor.a, fadeDuration)
                .SetEase(Ease.OutQuad));
    }

    private Tween CreateTapIconFadeOut()
    {
        return DOTween.Sequence()
            .Join(tapIcon.transform
                .DOLocalMove(GetHiddenPosition(tapIconBasePosition), fadeDuration)
                .SetEase(Ease.InCubic))
            .Join(tapIcon.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));
    }

    private Tween CreateHudFadeIn(
        RectTransform target,
        CanvasGroup canvasGroup,
        Vector2 basePosition)
    {
        SetHudHidden(target, canvasGroup, basePosition);

        return DOTween.Sequence()
            .Join(target.DOAnchorPos(basePosition, fadeDuration).SetEase(Ease.OutCubic))
            .Join(canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad));
    }

    private void SetTextHidden(int index)
    {
        tutorialText[index].rectTransform.anchoredPosition =
            GetHiddenPosition(tutorialBasePositions[index]);
        tutorialText[index].color = WithAlpha(tutorialBaseColors[index], 0f);
    }

    private void SetTapIconHidden()
    {
        tapIcon.transform.localPosition = GetHiddenPosition(tapIconBasePosition);
        tapIcon.color = WithAlpha(tapIconBaseColor, 0f);
    }

    private void SetHudHidden(
        RectTransform target,
        CanvasGroup canvasGroup,
        Vector2 basePosition)
    {
        target.anchoredPosition = GetHiddenPosition(basePosition);
        canvasGroup.alpha = 0f;
    }

    private Vector2 GetHiddenPosition(Vector2 basePosition)
    {
        return basePosition + Vector2.down * moveDistance;
    }

    private Vector3 GetHiddenPosition(Vector3 basePosition)
    {
        return basePosition + Vector3.down * moveDistance;
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    private static CanvasGroup GetOrAddCanvasGroup(RectTransform target)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
        }

        return canvasGroup;
    }
}
