using DG.Tweening;
using UnityEngine;

public class TitleFrogTransitionController : MonoBehaviour
{
    [SerializeField] private FrogAppearance frogAppearance;
    [SerializeField] private Transform tongueLine;
    [SerializeField] private Transform tongueTip;
    [SerializeField] private SpriteRenderer tongueLineRenderer;
    [SerializeField] private SpriteRenderer tongueTipRenderer;
    [SerializeField] private RectTransform titleLogo;
    [SerializeField] private RectTransform creditIcon;
    [SerializeField] private RectTransform tapToStart;
    [SerializeField] private SceneTransition sceneTransition;

    [Header("Tongue")]
    [SerializeField, Min(0.001f)] private float tongueLengthScale = 0.25f;
    [SerializeField, Min(0.01f)] private float tongueExtendDuration = 0.35f;
    [SerializeField, Min(0.01f)] private float logoPullDuration = 0.5f;

    [Header("UI Fade")]
    [SerializeField, Min(0f)] private float uiMoveDistance = 10f;
    [SerializeField, Min(0.01f)] private float uiFadeDuration = 0.5f;

    [Header("Title Logo Entrance")]
    [SerializeField, Min(0f)] private float titleFadeDelay = 1f;
    [SerializeField] private string titleAppearAnimationState = "appear";
    [SerializeField, Min(0.1f)] private float titleAppearDuration = 3.2f;

    private bool isTransitioning;
    private bool canStart;
    private Sequence transitionSequence;
    private Sequence logoEntranceSequence;
    private CanvasGroup titleCanvasGroup;
    private CanvasGroup creditCanvasGroup;
    private CanvasGroup tapCanvasGroup;
    private Animator titleAnimator;
    private Animator tapAnimator;
    private Vector2 titleBasePosition;
    private Vector2 creditBasePosition;
    private Vector2 tapBasePosition;

    private void Awake()
    {
        tongueLineRenderer.enabled = false;
        tongueTipRenderer.enabled = false;

        titleCanvasGroup = GetOrAddCanvasGroup(titleLogo);
        creditCanvasGroup = GetOrAddCanvasGroup(creditIcon);
        tapCanvasGroup = GetOrAddCanvasGroup(tapToStart);

        titleAnimator = titleLogo.GetComponent<Animator>();
        if (titleAnimator != null)
        {
            titleAnimator.enabled = false;
        }

        tapAnimator = tapToStart.GetComponent<Animator>();
        titleBasePosition = titleLogo.anchoredPosition;
        creditBasePosition = creditIcon.anchoredPosition;
        tapBasePosition = tapToStart.anchoredPosition;

        titleCanvasGroup.alpha = 0f;

        tapCanvasGroup.interactable = false;
        tapCanvasGroup.blocksRaycasts = false;

        PlayTitleLogoEntrance();
    }

    public bool PlayStartTransition()
    {
        if (isTransitioning || !canStart)
        {
            return false;
        }

        isTransitioning = true;
        canStart = false;
        tapCanvasGroup.interactable = false;
        tapCanvasGroup.blocksRaycasts = false;
        StopTitleLogoEntrance();
        if (tapAnimator != null)
        {
            tapAnimator.enabled = false;
        }

        frogAppearance.OpenMouthForTongue();
        tongueLineRenderer.enabled = true;
        tongueTipRenderer.enabled = true;

        Vector3 logoWorldPosition = titleLogo.position;
        logoWorldPosition.z = tongueLine.position.z;

        transitionSequence = DOTween.Sequence().SetUpdate(true)
            .Append(CreateTongueMove(tongueLine.position, logoWorldPosition, tongueExtendDuration))
            .Append(CreateLogoPullSequence())
            .OnComplete(() =>
            {
                transitionSequence = null;
                titleLogo.gameObject.SetActive(false);
                tongueLineRenderer.enabled = false;
                tongueTipRenderer.enabled = false;
                frogAppearance.CloseMouthAfterTongue();
                sceneTransition.SceneTrans(SceneTransition.SceneType.InGame);
            });

        return true;
    }

    private Tween CreateLogoPullSequence()
    {
        Vector3 logoStartLocalPosition = titleLogo.localPosition;
        Vector3 logoStartScale = titleLogo.localScale;
        float progress = 0f;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(
            () => progress,
            value =>
            {
                progress = value;
                float eased = DOVirtual.EasedValue(0f, 1f, value, Ease.InCubic);

                // Convert the moving mouth from world space to the logo parent's
                // local space. The same progress retracts the tongue, moves the
                // logo and shrinks it, so all three motions stay synchronized.
                Vector3 mouthLocalPosition = titleLogo.parent.InverseTransformPoint(tongueLine.position);
                titleLogo.localPosition = Vector3.Lerp(
                    logoStartLocalPosition,
                    mouthLocalPosition,
                    eased
                );
                titleLogo.localScale = Vector3.Lerp(logoStartScale, Vector3.zero, eased);
                UpdateTongue(titleLogo.position);
            },
            1f,
            logoPullDuration
        ).SetEase(Ease.Linear));

        sequence.Join(CreateUiFadeOut(
            creditIcon,
            creditCanvasGroup,
            creditBasePosition
        ));
        sequence.Join(CreateUiFadeOut(
            tapToStart,
            tapCanvasGroup,
            tapBasePosition
        ));
        return sequence;
    }

    private void PlayTitleLogoEntrance()
    {
        titleCanvasGroup.alpha = 0f;
        if (titleAnimator != null)
        {
            titleAnimator.enabled = false;
        }

        logoEntranceSequence = DOTween.Sequence().SetUpdate(true)
            .AppendInterval(titleFadeDelay)
            .AppendCallback(() =>
            {
                if (titleAnimator != null && !isTransitioning)
                {
                    titleAnimator.enabled = true;
                    titleAnimator.Rebind();
                    titleAnimator.Update(0f);
                    titleAnimator.Play(titleAppearAnimationState, 0, 0f);
                    titleAnimator.Update(0f);
                }
                titleCanvasGroup.alpha = 1f;
            })
            .AppendInterval(titleAppearDuration)
            .OnComplete(() =>
            {
                logoEntranceSequence = null;
                canStart = true;
                tapCanvasGroup.interactable = true;
                tapCanvasGroup.blocksRaycasts = true;
            });
    }

    private void StopTitleLogoEntrance()
    {
        logoEntranceSequence?.Kill();
        logoEntranceSequence = null;

        if (titleAnimator != null)
        {
            titleAnimator.enabled = false;
        }

        titleCanvasGroup.alpha = 1f;
        titleLogo.anchoredPosition = titleBasePosition;
    }

    private Tween CreateTongueMove(Vector3 from, Vector3 to, float duration)
    {
        float progress = 0f;

        return DOTween.To(
            () => progress,
            value =>
            {
                progress = value;
                float eased = DOVirtual.EasedValue(0f, 1f, value, Ease.OutCubic);
                UpdateTongue(Vector3.Lerp(from, to, eased));
            },
            1f,
            duration
        ).SetEase(Ease.Linear);
    }

    private Tween CreateUiFadeOut(
        RectTransform target,
        CanvasGroup canvasGroup,
        Vector2 basePosition)
    {
        return DOTween.Sequence()
            .Join(target.DOAnchorPos(
                basePosition + Vector2.down * uiMoveDistance,
                uiFadeDuration
            ).SetEase(Ease.InCubic))
            .Join(canvasGroup.DOFade(0f, uiFadeDuration).SetEase(Ease.InQuad));
    }

    private void UpdateTongue(Vector3 tipPosition)
    {
        tongueTip.position = tipPosition;
        Vector3 direction = tipPosition - tongueLine.position;
        float distance = direction.magnitude;

        if (distance <= 0.001f)
        {
            return;
        }

        tongueLine.up = direction;
        Vector3 scale = Vector3.one;
        scale.y = distance * tongueLengthScale;
        tongueLine.localScale = scale;
    }

    private void OnDestroy()
    {
        logoEntranceSequence?.Kill();
        transitionSequence?.Kill();
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