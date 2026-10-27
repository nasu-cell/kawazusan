using UnityEngine;
using DG.Tweening;

public class CameraShaker : MonoBehaviour
{
    [SerializeField, Min(0f)] private float shakeDistance = 5f;
    [SerializeField, Min(0.01f)] private float shakeStepDuration = 0.06f;

    private Sequence shakeSequence;
    private Vector3 baseLocalPosition;

    public void ShakeHorizontal()
    {
        StopShakeAndResetPosition();

        baseLocalPosition = transform.localPosition;

        float initialDirection = Random.value < 0.5f ? -1f : 1f;
        float largeOffset = initialDirection * shakeDistance;
        float smallOppositeOffset = -initialDirection * shakeDistance * 0.45f;
        float smallerInitialOffset = initialDirection * shakeDistance * 0.2f;

        shakeSequence = DOTween.Sequence()
            .Append(transform.DOLocalMoveX(
                baseLocalPosition.x + largeOffset,
                shakeStepDuration
            ).SetEase(Ease.OutQuad))
            .Append(transform.DOLocalMoveX(
                baseLocalPosition.x + smallOppositeOffset,
                shakeStepDuration
            ).SetEase(Ease.InOutQuad))
            .Append(transform.DOLocalMoveX(
                baseLocalPosition.x + smallerInitialOffset,
                shakeStepDuration
            ).SetEase(Ease.InOutQuad))
            .Append(transform.DOLocalMoveX(
                baseLocalPosition.x,
                shakeStepDuration
            ).SetEase(Ease.InQuad))
            .OnComplete(() => shakeSequence = null);
    }

    private void OnDisable()
    {
        StopShakeAndResetPosition();
    }

    private void StopShakeAndResetPosition()
    {
        if (shakeSequence == null)
        {
            return;
        }

        shakeSequence.Kill();
        shakeSequence = null;
        transform.localPosition = baseLocalPosition;
    }
}
