using UnityEngine;
using TMPro;
using DG.Tweening;

public class GameHUDController : MonoBehaviour
{
    [Header("Colors & Texts")]
    [SerializeField] private Color satietyFillColor;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI bonusText;
    [SerializeField] private SatietyController satietyController;
    [SerializeField, Min(0.01f)] private float clearFadeOutDuration = 0.5f;

    [Header("Bonus Text Animation")]
    [SerializeField, Min(1f)] private float bonusScaleMultiplier = 1.3f;
    [SerializeField, Min(0f)] private float bonusShakeRotationDegrees = 10f;
    [SerializeField, Min(0.01f)] private float bonusAnimationStepDuration = 0.06f;
    [SerializeField, Min(0f)] private float bonusDisplayDuration = 0.5f;
    [SerializeField, Min(0.01f)] private float bonusFadeDuration = 0.2f;

    private CanvasGroup satietyCanvasGroup;
    private CanvasGroup timerCanvasGroup;
    private Sequence bonusTextSequence;
    private Vector3 bonusTextBaseScale = Vector3.one;
    private Quaternion bonusTextBaseRotation = Quaternion.identity;

    private void Awake()
    {
        if (bonusText == null)
        {
            bonusText = FindBonusText();
        }

        if (bonusText != null)
        {
            bonusTextBaseScale = bonusText.transform.localScale;
            bonusTextBaseRotation = bonusText.transform.localRotation;
            Color color = bonusText.color;
            color.a = 0f;
            bonusText.color = color;
            bonusText.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        bonusTextSequence?.Kill();
        bonusTextSequence = null;
    }

    private TextMeshProUGUI FindBonusText()
    {
        Transform bonusTransform = transform.Find("BonusText");
        if (bonusTransform != null && bonusTransform.TryGetComponent(out TextMeshProUGUI tmp))
        {
            return tmp;
        }

        foreach (TextMeshProUGUI tmpText in GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (tmpText.gameObject.name == "BonusText")
            {
                return tmpText;
            }
        }

        return null;
    }

    public void InitHUD(int maxSatiety)
    {
        SetupHudCanvasGroups();
        UpdateTimer(0f);
        satietyController.InitSatiety(maxSatiety);
    }

    public void UpdateTimer(float elapsedTime)
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateSatiety(int currentSatiety, int maxSatiety)
    {
        satietyController.UpdateSatiety(currentSatiety);
    }

    public void FilledSatiety()
    {
        timerText.color = satietyFillColor;
    }

    public void ShowBonusText(int count)
    {
        if (bonusText == null)
        {
            bonusText = FindBonusText();
        }

        if (bonusText == null || count < 3)
        {
            return;
        }

        bonusTextSequence?.Kill();

        bonusText.gameObject.SetActive(true);
        bonusText.text = $"{count}ひきまとめぐい！";

        // 登場時は透明度をいきなり1にする
        Color color = bonusText.color;
        color.a = 1f;
        bonusText.color = color;

        // スケールと回転をリセット
        bonusText.transform.localScale = bonusTextBaseScale;
        bonusText.transform.localRotation = bonusTextBaseRotation;

        Vector3 enlargedScale = bonusTextBaseScale * bonusScaleMultiplier;
        Quaternion leftRot = bonusTextBaseRotation * Quaternion.Euler(0f, 0f, bonusShakeRotationDegrees);
        Quaternion rightRot = bonusTextBaseRotation * Quaternion.Euler(0f, 0f, -bonusShakeRotationDegrees * 0.7f);
        Quaternion smallerLeftRot = bonusTextBaseRotation * Quaternion.Euler(0f, 0f, bonusShakeRotationDegrees * 0.4f);

        bonusTextSequence = DOTween.Sequence().SetUpdate(true);

        // 一瞬大きくなって、一瞬回転揺れする
        Sequence popAndShakeSequence = DOTween.Sequence()
            .Append(bonusText.transform.DOScale(enlargedScale, bonusAnimationStepDuration).SetEase(Ease.OutQuad))
            .Join(bonusText.transform.DOLocalRotateQuaternion(leftRot, bonusAnimationStepDuration).SetEase(Ease.OutQuad))
            .Append(bonusText.transform.DOLocalRotateQuaternion(rightRot, bonusAnimationStepDuration).SetEase(Ease.InOutQuad))
            .Append(bonusText.transform.DOLocalRotateQuaternion(smallerLeftRot, bonusAnimationStepDuration).SetEase(Ease.InOutQuad))
            .Append(bonusText.transform.DOLocalRotateQuaternion(bonusTextBaseRotation, bonusAnimationStepDuration).SetEase(Ease.InQuad))
            .Join(bonusText.transform.DOScale(bonusTextBaseScale, bonusAnimationStepDuration * 2f).SetEase(Ease.OutBack));

        bonusTextSequence.Append(popAndShakeSequence);

        // 0.5秒ほどしたら
        bonusTextSequence.AppendInterval(bonusDisplayDuration);

        // 0.5秒ほどかけて0に透明度をイージング
        bonusTextSequence.Append(
            bonusText.DOFade(0f, bonusFadeDuration).SetEase(Ease.OutQuad)
        );

        bonusTextSequence.OnComplete(() =>
        {
            bonusText.gameObject.SetActive(false);
            bonusTextSequence = null;
        });
    }

    public void FadeOutHud()
    {
        SetupHudCanvasGroups();

        satietyCanvasGroup.DOKill();
        timerCanvasGroup.DOKill();
        satietyCanvasGroup.DOFade(0f, clearFadeOutDuration).SetUpdate(true);
        timerCanvasGroup.DOFade(0f, clearFadeOutDuration).SetUpdate(true);
    }

    private void SetupHudCanvasGroups()
    {
        if (satietyCanvasGroup == null)
        {
            satietyCanvasGroup = GetOrAddCanvasGroup(satietyController.gameObject);
        }

        if (timerCanvasGroup == null)
        {
            Transform timerRoot = FindParentNamed(timerText.transform, "Timer");
            timerCanvasGroup = GetOrAddCanvasGroup(timerRoot.gameObject);
        }
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        return canvasGroup != null ? canvasGroup : target.AddComponent<CanvasGroup>();
    }

    private static Transform FindParentNamed(Transform start, string objectName)
    {
        Transform current = start;
        while (current != null)
        {
            if (current.name == objectName)
            {
                return current;
            }

            current = current.parent;
        }

        return start;
    }
}
