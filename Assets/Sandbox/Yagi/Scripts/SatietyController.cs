using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using TMPro;

public class SatietyController : MonoBehaviour
{
    private int currentSatiety;
    public int CurrentSatiety
    {
        get => currentSatiety;
    }

    private int maxSatiety;
    public int MaxSatiety
    {
        get => maxSatiety;
    }

    public event Action Filled;
    public event Action<int> Added;
    public event Action<int> Decreased;

    [Header("References")]
    [SerializeField] private TongueController tongueController;

    [Header("UI References")]
    [SerializeField] private Slider slider;
    [SerializeField] private RectTransform unmaskedText;
    [SerializeField] private RectTransform maskedText;
    [SerializeField] private RectMask2D filledTextMask;
    [SerializeField] private TMP_Text satietyStateText;
    [SerializeField] private TMP_Text maskedSatietyStateText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite[] bgImages;
    [SerializeField] private Sprite interruptBGImage;
    [SerializeField] private Transform animationTarget;

    [Header("Fill Colors")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Color normalFillColor = new Color32(0xD8, 0xF5, 0xB4, 0xFF);
    [SerializeField] private Color interruptFillColor = new Color32(0xEA, 0x6C, 0x6C, 0xFF);

    [Header("Text Colors")]
    [SerializeField] private Color normalTextColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    [SerializeField] private Color interruptTextColor = new Color32(0xDA, 0x16, 0x16, 0xFF);

    [Header("Add Satiety Animation")]
    [SerializeField, Min(0f)] private float scaleIncreasePerAmount = 0.05f;
    [SerializeField, Min(1f)] private float maxScaleMultiplier = 1.35f;
    [SerializeField, Min(0f)] private float rotationDegreesPerAmount = 2f;
    [SerializeField, Min(0f)] private float maxRotationDegrees = 12f;
    [SerializeField, Min(0.01f)] private float animationStepDuration = 0.06f;

    [Header("Interrupt Animation")]
    [SerializeField, Min(0)] private int interruptDecreaseAmount = 2;
    [SerializeField, Min(1f)] private float interruptScaleMultiplier = 1.15f;
    [SerializeField, Min(0f)] private float interruptShakeRotationDegrees = 8f;
    [SerializeField, Min(0f)] private float interruptShakePositionDistance = 4f;
    [SerializeField, Min(0.01f)] private float interruptStepDuration = 0.05f;
    [SerializeField, Min(0f)] private float interruptBGDuration = 0.5f;

    private Vector3 baseLocalScale;
    private Quaternion baseLocalRotation;
    private Vector3 currentAnimBaseLocalPosition;
    private bool isPositionShaking;
    private Sequence addSatietySequence;
    private Sequence interruptSequence;
    private Tween interruptBGTween;
    private bool isInterrupting;

    private void Awake()
    {
        if (animationTarget == null)
        {
            animationTarget = transform;
        }

        if (tongueController == null)
        {
            tongueController = FindAnyObjectByType<TongueController>();
        }

        if (fillImage == null && slider != null && slider.fillRect != null)
        {
            fillImage = slider.fillRect.GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        baseLocalScale = animationTarget.localScale;
        baseLocalRotation = animationTarget.localRotation;
        slider.onValueChanged.AddListener(OnSliderValueChanged);

        if (fillImage == null && slider != null && slider.fillRect != null)
        {
            fillImage = slider.fillRect.GetComponent<Image>();
        }

        SetFillColor(normalFillColor);
        SetTextColor(normalTextColor);

        if (tongueController != null)
        {
            tongueController.Interrupted += OnTongueInterrupted;
        }

        RefreshFilledTextMask();
    }

    private void LateUpdate()
    {
        RefreshFilledTextMask();
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnSliderValueChanged);

        if (tongueController != null)
        {
            tongueController.Interrupted -= OnTongueInterrupted;
        }

        StopAddSatietyAnimation();
        StopInterruptAnimation();
    }

    public void InitSatiety(int maxSatiety)
    {
        this.maxSatiety = maxSatiety;
        this.currentSatiety = 0;
        slider.maxValue = maxSatiety;
        slider.value = currentSatiety;
    }

    public void AddSatiety(int amount)
    {
        StopInterruptAnimation();

        currentSatiety += amount;
        bool reachedMaxSatiety = currentSatiety >= maxSatiety;

        if (reachedMaxSatiety)
        {
            currentSatiety = maxSatiety;
        }

        slider.value = currentSatiety;

        if (amount > 0)
        {
            PlayAddSatietyAnimation(amount);
            Added?.Invoke(amount);
        }

        if (reachedMaxSatiety)
        {
            Filled?.Invoke();
        }
    }

    private void OnTongueInterrupted(Vector2 _)
    {
        Interrupt(interruptDecreaseAmount);
    }

    public void Interrupt(int decreaseAmount = 2)
    {
        if (decreaseAmount > 0)
        {
            currentSatiety = Mathf.Max(0, currentSatiety - decreaseAmount);
            slider.value = currentSatiety;
            Decreased?.Invoke(decreaseAmount);
        }

        PlayInterruptAnimation();
    }

    private void PlayAddSatietyAnimation(int amount)
    {
        StopAddSatietyAnimation();

        float scaleMultiplier = Mathf.Min(
            1f + amount * scaleIncreasePerAmount,
            Mathf.Max(1f, maxScaleMultiplier)
        );

        float rotationDegrees = Mathf.Min(
            amount * rotationDegreesPerAmount,
            maxRotationDegrees
        );

        Vector3 enlargedScale = baseLocalScale * scaleMultiplier;
        Quaternion clockwiseRotation = baseLocalRotation
            * Quaternion.Euler(0f, 0f, -rotationDegrees);
        Quaternion smallerCounterClockwiseRotation = baseLocalRotation
            * Quaternion.Euler(0f, 0f, rotationDegrees * 0.45f);
        Quaternion smallestClockwiseRotation = baseLocalRotation
            * Quaternion.Euler(0f, 0f, -rotationDegrees * 0.2f);

        addSatietySequence = DOTween.Sequence()
            .Append(animationTarget.DOLocalRotateQuaternion(
                clockwiseRotation,
                animationStepDuration
            ).SetEase(Ease.OutQuad))
            .Join(animationTarget.DOScale(
                enlargedScale,
                animationStepDuration
            ).SetEase(Ease.OutQuad))
            .Append(animationTarget.DOLocalRotateQuaternion(
                smallerCounterClockwiseRotation,
                animationStepDuration
            ).SetEase(Ease.InOutQuad))
            .Append(animationTarget.DOLocalRotateQuaternion(
                smallestClockwiseRotation,
                animationStepDuration
            ).SetEase(Ease.InOutQuad))
            .Append(animationTarget.DOLocalRotateQuaternion(
                baseLocalRotation,
                animationStepDuration
            ).SetEase(Ease.InQuad))
            .Insert(
                animationStepDuration,
                animationTarget.DOScale(
                    baseLocalScale,
                    animationStepDuration * 3f
                ).SetEase(Ease.OutQuad)
            )
            .OnComplete(() => addSatietySequence = null);
    }

    private void StopAddSatietyAnimation()
    {
        if (addSatietySequence == null)
        {
            return;
        }

        addSatietySequence.Kill();
        addSatietySequence = null;
        animationTarget.localScale = baseLocalScale;
        animationTarget.localRotation = baseLocalRotation;
    }

    private void PlayInterruptAnimation()
    {
        StopAddSatietyAnimation();
        StopInterruptAnimation();

        isInterrupting = true;

        Sprite frame5Sprite = interruptBGImage;
        if (frame5Sprite == null && bgImages != null && bgImages.Length > 5)
        {
            frame5Sprite = bgImages[5];
        }

        if (backgroundImage != null && frame5Sprite != null)
        {
            backgroundImage.sprite = frame5Sprite;
        }

        SetFillColor(interruptFillColor);
        SetTextColor(interruptTextColor);

        interruptBGTween = DOVirtual.DelayedCall(
            interruptBGDuration,
            () =>
            {
                interruptBGTween = null;
                isInterrupting = false;
                SetFillColor(normalFillColor);
                SetTextColor(normalTextColor);
                UpdateSatietyPresentation();
            }
        );

        currentAnimBaseLocalPosition = animationTarget.localPosition;
        isPositionShaking = true;

        Vector3 enlargedScale = baseLocalScale * interruptScaleMultiplier;

        Vector3 leftPos = currentAnimBaseLocalPosition + Vector3.left * interruptShakePositionDistance;
        Vector3 rightPos = currentAnimBaseLocalPosition + Vector3.right * (interruptShakePositionDistance * 0.7f);
        Vector3 smallerLeftPos = currentAnimBaseLocalPosition + Vector3.left * (interruptShakePositionDistance * 0.4f);

        Quaternion leftRot = baseLocalRotation * Quaternion.Euler(0f, 0f, interruptShakeRotationDegrees);
        Quaternion rightRot = baseLocalRotation * Quaternion.Euler(0f, 0f, -interruptShakeRotationDegrees * 0.7f);
        Quaternion smallerLeftRot = baseLocalRotation * Quaternion.Euler(0f, 0f, interruptShakeRotationDegrees * 0.4f);

        interruptSequence = DOTween.Sequence()
            .Append(animationTarget.DOScale(enlargedScale, interruptStepDuration).SetEase(Ease.OutQuad))
            .Join(animationTarget.DOLocalMove(leftPos, interruptStepDuration).SetEase(Ease.OutQuad))
            .Join(animationTarget.DOLocalRotateQuaternion(leftRot, interruptStepDuration).SetEase(Ease.OutQuad))
            .Append(animationTarget.DOLocalMove(rightPos, interruptStepDuration).SetEase(Ease.InOutQuad))
            .Join(animationTarget.DOLocalRotateQuaternion(rightRot, interruptStepDuration).SetEase(Ease.InOutQuad))
            .Append(animationTarget.DOLocalMove(smallerLeftPos, interruptStepDuration).SetEase(Ease.InOutQuad))
            .Join(animationTarget.DOLocalRotateQuaternion(smallerLeftRot, interruptStepDuration).SetEase(Ease.InOutQuad))
            .Append(animationTarget.DOLocalMove(currentAnimBaseLocalPosition, interruptStepDuration).SetEase(Ease.InQuad))
            .Join(animationTarget.DOLocalRotateQuaternion(baseLocalRotation, interruptStepDuration).SetEase(Ease.InQuad))
            .Join(animationTarget.DOScale(baseLocalScale, interruptStepDuration * 2f).SetEase(Ease.OutQuad))
            .OnComplete(() =>
            {
                isPositionShaking = false;
                interruptSequence = null;
            });
    }

    private void StopInterruptAnimation()
    {
        interruptBGTween?.Kill();
        interruptBGTween = null;

        if (interruptSequence != null)
        {
            interruptSequence.Kill();
            interruptSequence = null;
        }

        if (animationTarget != null)
        {
            animationTarget.localScale = baseLocalScale;
            animationTarget.localRotation = baseLocalRotation;

            if (isPositionShaking)
            {
                animationTarget.localPosition = currentAnimBaseLocalPosition;
                isPositionShaking = false;
            }
        }

        if (isInterrupting)
        {
            isInterrupting = false;
            SetFillColor(normalFillColor);
            SetTextColor(normalTextColor);
            UpdateSatietyPresentation();
        }
    }

    private void SetFillColor(Color color)
    {
        if (fillImage != null)
        {
            fillImage.color = color;
        }
    }

    private void SetTextColor(Color color)
    {
        if (satietyStateText != null)
        {
            satietyStateText.color = color;
        }

        if (maskedSatietyStateText != null)
        {
            // maskedSatietyStateText.color = color;
        }
    }

    private void SyncMaskedTextTransform()
    {
        if (unmaskedText == null || maskedText == null)
        {
            return;
        }

        maskedText.position = unmaskedText.position;
        maskedText.rotation = unmaskedText.rotation;
        maskedText.sizeDelta = unmaskedText.sizeDelta;
    }

    private void UpdateFilledTextMaskPadding()
    {
        if (filledTextMask == null)
        {
            return;
        }

        Rect maskRect = filledTextMask.rectTransform.rect;
        float hiddenWidth = maskRect.width * (1f - slider.normalizedValue);
        float hiddenHeight = maskRect.height * (1f - slider.normalizedValue);
        Vector4 padding = Vector4.zero;

        switch (slider.direction)
        {
            case Slider.Direction.LeftToRight:
                padding.z = hiddenWidth;
                break;
            case Slider.Direction.RightToLeft:
                padding.x = hiddenWidth;
                break;
            case Slider.Direction.BottomToTop:
                padding.w = hiddenHeight;
                break;
            case Slider.Direction.TopToBottom:
                padding.y = hiddenHeight;
                break;
        }

        filledTextMask.padding = padding;
    }

    private void RefreshFilledTextMask()
    {
        SyncMaskedTextTransform();
        UpdateFilledTextMaskPadding();
        UpdateSatietyPresentation();
    }

    private void UpdateSatietyPresentation()
    {
        if (isInterrupting)
        {
            return;
        }

        int stateIndex = Mathf.Clamp(
            Mathf.FloorToInt(slider.normalizedValue * 4f),
            0,
            4
        );

        string stateText = stateIndex switch
        {
            0 => "はらぺこ",
            1 => "まだまだ",
            2 => "まあまあ",
            3 => "いいかんじ",
            _ => "まんぷく！"
        };

        if (satietyStateText != null)
        {
            satietyStateText.text = stateText;
        }

        if (maskedSatietyStateText != null)
        {
            maskedSatietyStateText.text = stateText;
        }

        if (backgroundImage != null
            && bgImages != null
            && stateIndex < bgImages.Length
            && bgImages[stateIndex] != null)
        {
            backgroundImage.sprite = bgImages[stateIndex];
        }
    }

    private void OnSliderValueChanged(float _)
    {
        RefreshFilledTextMask();
    }

    public void UpdateSatiety(int currentSatiety)
    {
        this.currentSatiety = currentSatiety;
        slider.value = currentSatiety;
    }
}
