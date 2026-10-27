using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FrogAppearance : MonoBehaviour
{
    private readonly struct SpriteMaterialState
    {
        public readonly SpriteRenderer spriteRenderer;
        public readonly Material material;

        public SpriteMaterialState(
            SpriteRenderer spriteRenderer,
            Material material)
        {
            this.spriteRenderer = spriteRenderer;
            this.material = material;
        }
    }

    [SerializeField] private SpriteRenderer[] eyesSpriteRenderers;
    [SerializeField] private Transform[] eyesTransforms;
    [SerializeField] private Sprite[] eyesSprites;
    [SerializeField] private SpriteRenderer[] pupilSpriteRenderers;
    [SerializeField] private Transform[] pupilPivots;
    [SerializeField] private Sprite pupilSprite;
    [SerializeField] private SpriteRenderer[] headSpriteRenderers;
    [SerializeField] private Sprite[] headSprites;
    [SerializeField] private SpriteRenderer[] mouthSpriteRenderers;
    [SerializeField] private Transform mouthTransform;
    [SerializeField] private Sprite[] mouthSprites;
    [SerializeField] private SpriteRenderer[] bodySpriteRenderers;
    [SerializeField] private Sprite[] bodySprites;
    [SerializeField] private SpriteRenderer[] armSpriteRenderers;
    [SerializeField] private Sprite[] armSprites;
    [SerializeField] private Transform frogRoot;
    [SerializeField] private CameraShaker cameraShaker;
    [SerializeField] private SatietyController satietyController;
    [SerializeField, Min(0.01f)] private float animationFrameDuration = 0.15f;
    [SerializeField] private float faceMoveDistance = 1f;
    [SerializeField, Min(0f)] private float mouthTransitionDuration = 0.05f;
    [SerializeField, Min(0f)] private float interruptShakeDistance = 5f;
    [SerializeField, Min(0.01f)] private float interruptShakeStepDuration = 0.06f;
    [SerializeField, Min(0f)] private float xWaveAmplitude = 1f;
    [SerializeField, Min(0.01f)] private float xWavePeriod = 2f;
    [SerializeField, Min(0f)] private float yWaveAmplitude = 1f;
    [SerializeField, Min(0.01f)] private float yWavePeriod = 1.5f;
    [SerializeField] private Material interruptFlashMaterial;
    [SerializeField, Min(0f)] private float interruptFlashDuration = 0.1f;
    [SerializeField, Min(0f)] private float satietyEyeDuration = 0.4f;
    [SerializeField, Min(1f)] private float satietyScaleMultiplier = 1.12f;
    [SerializeField, Min(0.01f)] private float satietyScaleDuration = 0.1f;

    private Vector3[] eyesInitialLocalPositions;
    private Vector3 mouthInitialLocalPosition;
    private Coroutine appearanceAnimationCoroutine;
    private Tween mouthTransitionTween;
    private Sequence interruptShakeSequence;
    private Vector3 interruptBaseLocalPosition;
    private Vector3 waveBaseLocalPosition;
    private Vector3 lotusBaseLocalPosition;
    private float waveElapsedTime;
    [SerializeField] private Transform lotusRootTransform;
    private SpriteMaterialState[] originalSpriteMaterialStates;
    private Tween interruptFlashTween;
    private bool useInterruptMouthClose;
    private Tween eyeExpressionTween;
    private bool isSatietyFilled;
    private Vector3 frogBaseLocalScale;
    private Sequence satietyScaleSequence;

    private void Awake()
    {
        eyesInitialLocalPositions = new Vector3[eyesTransforms.Length];

        for (int i = 0; i < eyesTransforms.Length; i++)
        {
            eyesInitialLocalPositions[i] = eyesTransforms[i].localPosition;
        }

        mouthInitialLocalPosition = mouthTransform.localPosition;
        frogBaseLocalScale = frogRoot.localScale;
    }

    private void OnEnable()
    {
        if (satietyController != null)
        {
            satietyController.Added += OnSatietyAdded;
            satietyController.Filled += OnSatietyFilled;
        }

        waveBaseLocalPosition = transform.localPosition;
        if (lotusRootTransform != null)
        {
            lotusBaseLocalPosition = lotusRootTransform.localPosition;
        }
        waveElapsedTime = 0f;
        appearanceAnimationCoroutine = StartCoroutine(PlayAppearanceAnimation());
    }

    private void Update()
    {
        Transform appearanceTransform = transform;
        if (appearanceTransform == null)
        {
            return;
        }

        // realtimeSinceStartup does not reset on scene changes, so the wave phase
        // remains continuous between the title and in-game frogs.
        waveElapsedTime = (float)Time.realtimeSinceStartupAsDouble;

        float xOffset = Mathf.Sin(
            waveElapsedTime * Mathf.PI * 2f / Mathf.Max(xWavePeriod, 0.01f)
        ) * xWaveAmplitude;

        float yOffset = Mathf.Sin(
            waveElapsedTime * Mathf.PI * 2f / Mathf.Max(yWavePeriod, 0.01f)
        ) * yWaveAmplitude;

        appearanceTransform.localPosition = waveBaseLocalPosition
            + new Vector3(xOffset, yOffset, 0f);
        if (lotusRootTransform != null)
        {
            lotusRootTransform.localPosition = lotusBaseLocalPosition
                + new Vector3(xOffset, yOffset, 0f);
        }

    }

    private void OnDisable()
    {
        if (satietyController != null)
        {
            satietyController.Added -= OnSatietyAdded;
            satietyController.Filled -= OnSatietyFilled;
        }

        eyeExpressionTween?.Kill();
        eyeExpressionTween = null;
        StopSatietyScaleAnimation();

        if (appearanceAnimationCoroutine != null)
        {
            StopCoroutine(appearanceAnimationCoroutine);
            appearanceAnimationCoroutine = null;
        }

        mouthTransitionTween?.Kill();
        mouthTransitionTween = null;
        StopInterruptFlash();

        if (interruptShakeSequence != null)
        {
            interruptShakeSequence.Kill();
            interruptShakeSequence = null;
            frogRoot.localPosition = interruptBaseLocalPosition;
        }

        transform.localPosition = waveBaseLocalPosition;
    }

    private IEnumerator PlayAppearanceAnimation()
    {
        int frameIndex = 0;

        while (true)
        {
            ApplyAnimationFrame(frameIndex);
            yield return new WaitForSeconds(animationFrameDuration);

            frameIndex = (frameIndex + 1) % 4;
        }
    }

    private void ApplyAnimationFrame(int frameIndex)
    {
        int headSpriteIndex = frameIndex == 0 || frameIndex == 3 ? 0 : 1;
        int armSpriteIndex = frameIndex < 2 ? 0 : 1;
        int bodySpriteIndex = frameIndex == 3 ? 1 : 0;

        SetSprites(headSpriteRenderers, headSprites[headSpriteIndex]);
        SetSprites(armSpriteRenderers, armSprites[armSpriteIndex]);
        SetSprites(bodySpriteRenderers, bodySprites[bodySpriteIndex]);

        bool moveFaceDown = frameIndex == 1 || frameIndex == 2;
        SetFacePosition(moveFaceDown);
    }

    private void SetSprites(SpriteRenderer[] spriteRenderers, Sprite sprite)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    private void SetFacePosition(bool moveDown)
    {
        Vector3 offset = moveDown
            ? Vector3.down * faceMoveDistance
            : Vector3.zero;

        for (int i = 0; i < eyesTransforms.Length; i++)
        {
            eyesTransforms[i].localPosition = eyesInitialLocalPositions[i] + offset;
        }

        mouthTransform.localPosition = mouthInitialLocalPosition + offset;
    }

    public void OpenMouthForTongue()
    {
        mouthTransitionTween?.Kill();
        PlaySE(SoundData.SoundType.FlogMouthOpen);
        SetSprites(mouthSpriteRenderers, mouthSprites[1]);

        mouthTransitionTween = DOVirtual.DelayedCall(
            mouthTransitionDuration,
            () => SetSprites(mouthSpriteRenderers, mouthSprites[2])
        );
    }

    public void CloseMouthAfterTongue()
    {
        mouthTransitionTween?.Kill();

        int transitionSpriteIndex = useInterruptMouthClose ? 3 : 1;
        useInterruptMouthClose = false;
        SetSprites(mouthSpriteRenderers, mouthSprites[transitionSpriteIndex]);

        mouthTransitionTween = DOVirtual.DelayedCall(
            mouthTransitionDuration,
            () => SetSprites(mouthSpriteRenderers, mouthSprites[0])
        );
    }

    public void Interrupt(Vector2 causeWorldPosition)
    {
        mouthTransitionTween?.Kill();
        mouthTransitionTween = null;
        useInterruptMouthClose = true;
        SetSprites(mouthSpriteRenderers, mouthSprites[4]);

        PlayDirectionalInterruptShake(causeWorldPosition);
        PlayInterruptFlash();

        if (cameraShaker != null)
        {
            cameraShaker.ShakeHorizontal();
        }

        ShowTemporaryEyeExpression(1, 0.5f);
    }

    private void OnSatietyAdded(int amount)
    {
        ShowTemporaryEyeExpression(2, satietyEyeDuration);
        PlaySatietyScaleAnimation();
    }

    private void PlaySatietyScaleAnimation()
    {
        StopSatietyScaleAnimation();

        satietyScaleSequence = DOTween.Sequence()
            .Append(frogRoot.DOScale(
                frogBaseLocalScale * satietyScaleMultiplier,
                satietyScaleDuration
            ).SetEase(Ease.OutQuad))
            .Append(frogRoot.DOScale(
                frogBaseLocalScale,
                satietyScaleDuration
            ).SetEase(Ease.InQuad))
            .OnComplete(() => satietyScaleSequence = null);
    }

    private void StopSatietyScaleAnimation()
    {
        if (satietyScaleSequence == null)
        {
            return;
        }

        satietyScaleSequence.Kill();
        satietyScaleSequence = null;
        frogRoot.localScale = frogBaseLocalScale;
    }

    private void OnSatietyFilled()
    {
        isSatietyFilled = true;
        eyeExpressionTween?.Kill();
        eyeExpressionTween = null;
        SetEyeExpression(3, false);
    }

    private void ShowTemporaryEyeExpression(
        int spriteIndex,
        float duration)
    {
        if (isSatietyFilled)
        {
            return;
        }

        eyeExpressionTween?.Kill();
        SetEyeExpression(spriteIndex, false);

        eyeExpressionTween = DOVirtual.DelayedCall(
            duration,
            () =>
            {
                eyeExpressionTween = null;

                if (!isSatietyFilled)
                {
                    SetEyeExpression(0, true);
                }
            }
        );
    }

    private void SetEyeExpression(int spriteIndex, bool showPupils)
    {
        SetSprites(eyesSpriteRenderers, eyesSprites[spriteIndex]);

        foreach (SpriteRenderer pupilSpriteRenderer in pupilSpriteRenderers)
        {
            pupilSpriteRenderer.sprite = showPupils ? pupilSprite : null;
        }
    }

    private void PlayInterruptFlash()
    {
        StopInterruptFlash();

        if (interruptFlashMaterial == null)
        {
            return;
        }

        SpriteRenderer[] spriteRenderers =
            GetComponentsInChildren<SpriteRenderer>(true);

        originalSpriteMaterialStates =
            new SpriteMaterialState[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            SpriteRenderer spriteRenderer = spriteRenderers[i];

            originalSpriteMaterialStates[i] = new SpriteMaterialState(
                spriteRenderer,
                spriteRenderer.sharedMaterial
            );

            spriteRenderer.sharedMaterial = interruptFlashMaterial;
        }

        interruptFlashTween = DOVirtual.DelayedCall(
            interruptFlashDuration,
            () =>
            {
                interruptFlashTween = null;
                RestoreOriginalSpriteMaterials();
            }
        );
    }

    private void StopInterruptFlash()
    {
        interruptFlashTween?.Kill();
        interruptFlashTween = null;
        RestoreOriginalSpriteMaterials();
    }

    private void RestoreOriginalSpriteMaterials()
    {
        if (originalSpriteMaterialStates == null)
        {
            return;
        }

        foreach (SpriteMaterialState state in originalSpriteMaterialStates)
        {
            if (state.spriteRenderer != null)
            {
                state.spriteRenderer.sharedMaterial = state.material;
            }
        }

        originalSpriteMaterialStates = null;
    }

    private void PlayDirectionalInterruptShake(Vector2 causeWorldPosition)
    {
        if (interruptShakeSequence != null)
        {
            interruptShakeSequence.Kill();
            frogRoot.localPosition = interruptBaseLocalPosition;
        }

        interruptBaseLocalPosition = frogRoot.localPosition;

        Vector2 awayDirection =
            (Vector2)frogRoot.position - causeWorldPosition;

        if (awayDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            awayDirection = Vector2.right;
        }
        else
        {
            awayDirection.Normalize();
        }

        Vector3 largeAwayOffset =
            (Vector3)(awayDirection * interruptShakeDistance);

        if (frogRoot.parent != null)
        {
            largeAwayOffset = frogRoot.parent.InverseTransformVector(
                largeAwayOffset
            );
        }

        Vector3 largeAwayPosition = interruptBaseLocalPosition
            + largeAwayOffset;
        Vector3 smallTowardPosition = interruptBaseLocalPosition
            - largeAwayOffset * 0.45f;
        Vector3 smallerAwayPosition = interruptBaseLocalPosition
            + largeAwayOffset * 0.2f;

        interruptShakeSequence = DOTween.Sequence()
            .Append(frogRoot.DOLocalMove(
                largeAwayPosition,
                interruptShakeStepDuration
            ).SetEase(Ease.OutQuad))
            .Append(frogRoot.DOLocalMove(
                smallTowardPosition,
                interruptShakeStepDuration
            ).SetEase(Ease.InOutQuad))
            .Append(frogRoot.DOLocalMove(
                smallerAwayPosition,
                interruptShakeStepDuration
            ).SetEase(Ease.InOutQuad))
            .Append(frogRoot.DOLocalMove(
                interruptBaseLocalPosition,
                interruptShakeStepDuration
            ).SetEase(Ease.InQuad))
            .OnComplete(() => interruptShakeSequence = null);
    }

    public void RotatePupilsTowards(Vector2 targetWorldPosition)
    {
        foreach (Transform pupilPivot in pupilPivots)
        {
            Vector2 direction =
                targetWorldPosition - (Vector2)pupilPivot.position;

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                continue;
            }

            pupilPivot.up = direction.normalized;
        }
    }

    private static void PlaySE(SoundData.SoundType soundType)
    {
        if (SEManager.Instance != null)
        {
            SEManager.Instance.PlaySE(soundType);
        }
    }
}
