using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PopAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float animationTime = 0.5f;
    [SerializeField] private float moveDistance = 10f;

    [Header("Color Settings")]
    [SerializeField] private Color startColor = Color.clear;
    [SerializeField] private Color endColor = Color.white;

    private Vector3 initialPosition;
    private Sequence popSequence;

    private Image targetImage;

    private void Awake()
    {
        // Hierarchy上に配置されている位置を基準位置として保存する
        initialPosition = transform.position;

        // 子オブジェクトにあるImageを取得する
        targetImage = GetComponentInChildren<Image>();
    }

    private void OnEnable()
    {
        PlayPopAnimation();
    }

    private void OnDisable()
    {
        popSequence?.Kill();
    }

    private void PlayPopAnimation()
    {
        popSequence?.Kill();

        // 基準位置から下に移動した位置を開始位置にする
        transform.position = initialPosition + Vector3.down * moveDistance;

        SetStartColor();

        popSequence = DOTween.Sequence();

        // 開始位置からHierarchy上の基準位置まで移動する
        popSequence.Append(
            transform.DOMove(
                initialPosition,
                animationTime
            )
            .SetEase(Ease.OutCubic)
        );

        // 子オブジェクトのImageを同時に指定した色へ変化させる
        if (targetImage != null)
        {
            popSequence.Join(
                targetImage
                    .DOColor(endColor, animationTime)
                    .SetEase(Ease.OutCubic)
            );
        }
    }

    private void SetStartColor()
    {
        if (targetImage != null)
        {
            targetImage.color = startColor;
        }
    }
}