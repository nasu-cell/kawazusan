using DG.Tweening;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class UITransitionAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform[] targets;
    [SerializeField, Min(0f)] private float moveDistance = 20f;
    [SerializeField, Min(0f)] private float fadeInDelay = 1f;
    [SerializeField, Min(0f)] private float fadeInInterval = 0.5f;
    [SerializeField, Min(0.01f)] private float duration = 0.5f;
    [SerializeField] private bool playOnStart = true;

    public float Duration => duration;

    private CanvasGroup[] canvasGroups;
    private Vector2[] baseAnchoredPositions;
    private Sequence transitionSequence;

    private void Awake()
    {
        canvasGroups = new CanvasGroup[targets.Length];
        baseAnchoredPositions = new Vector2[targets.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            RectTransform target = targets[i];
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroups[i] = canvasGroup;
            baseAnchoredPositions[i] = target.anchoredPosition;

            if (playOnStart)
            {
                target.anchoredPosition = GetHiddenPosition(i);
                canvasGroup.alpha = 0f;
            }
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayFadeIn();
        }
    }

    private void OnDisable()
    {
        transitionSequence?.Kill();
        transitionSequence = null;
    }

    public void PlayFadeIn()
    {
        transitionSequence?.Kill();
        transitionSequence = DOTween.Sequence().SetUpdate(true);

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].anchoredPosition = GetHiddenPosition(i);
            canvasGroups[i].alpha = 0f;

            float startTime = fadeInDelay + fadeInInterval * i;

            Tween moveTween = targets[i]
                .DOAnchorPos(baseAnchoredPositions[i], duration)
                .SetEase(Ease.OutCubic);

            transitionSequence.Insert(startTime, moveTween);
            transitionSequence.Insert(
                startTime,
                canvasGroups[i].DOFade(1f, duration)
                    .SetEase(Ease.OutQuad)
            );
        }

        transitionSequence.OnComplete(() => transitionSequence = null);
    }

    public void PlayFadeOut()
    {
        transitionSequence?.Kill();
        transitionSequence = DOTween.Sequence().SetUpdate(true);

        for (int i = 0; i < targets.Length; i++)
        {
            transitionSequence.Join(
                targets[i].DOAnchorPos(GetHiddenPosition(i), duration)
                    .SetEase(Ease.InCubic)
            );
            transitionSequence.Join(
                canvasGroups[i].DOFade(0f, duration)
                    .SetEase(Ease.InQuad)
            );
        }

        transitionSequence.OnComplete(() => transitionSequence = null);
    }

    private Vector2 GetHiddenPosition(int index)
    {
        return baseAnchoredPositions[index] + Vector2.down * moveDistance;
    }
}
