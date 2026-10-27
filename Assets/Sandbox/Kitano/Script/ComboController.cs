using TMPro;
using UnityEngine;
using DG.Tweening;

public class ComboController : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI comboText;

    [Header("Display Settings")]
    [SerializeField] private float blinkTime = 0.5f;
    [SerializeField] private float displayTime = 1f;
    [SerializeField] private float fadeoutTime = 0.5f;
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 1f;

    [Header("Rainbow Settings")]
    [SerializeField] private float rainbowSpeed = 1f;

    private Sequence comboSequence;
    private Tween rainbowTween;

    public void ShowCombo(int combo)
    {
        comboSequence?.Kill();
        rainbowTween?.Kill();

        comboText.text = $"{combo} Combo";
        comboText.gameObject.SetActive(true);

        SetAlpha(0f);

        StartRainbowEffect();

        comboSequence = DOTween.Sequence();

        // 一瞬で最大α値まで表示する
        comboSequence.AppendCallback(() =>
        {
            SetAlpha(maxAlpha);
        });

        // 点滅させる
        comboSequence.Append(
            comboText
                .DOFade(0f, blinkTime)
                .SetEase(Ease.InOutSine)
        );

        comboSequence.Append(
            comboText
                .DOFade(maxAlpha, blinkTime)
                .SetEase(Ease.InOutSine)
        );

        comboSequence.SetLoops(2);

        // 点滅後、指定秒数表示する
        comboSequence.AppendInterval(displayTime);

        // 指定秒数をかけて消す
        comboSequence.Append(
            comboText
                .DOFade(0f, fadeoutTime)
                .SetEase(Ease.OutCubic)
        );

        comboSequence.OnComplete(() =>
        {
            comboText.gameObject.SetActive(false);
            rainbowTween?.Kill();
        });
    }

    private void StartRainbowEffect()
    {
        rainbowTween = DOTween.To(
            () => 0f,
            value =>
            {
                float hue = Mathf.Repeat(value, 1f);
                Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);

                rainbowColor.a = comboText.color.a;
                comboText.color = rainbowColor;
            },
            1f,
            1f / rainbowSpeed
        )
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Restart);
    }

    private void SetAlpha(float alpha)
    {
        Color color = comboText.color;
        color.a = alpha;
        comboText.color = color;
    }

    private void OnDisable()
    {
        comboSequence?.Kill();
        rainbowTween?.Kill();
    }
}