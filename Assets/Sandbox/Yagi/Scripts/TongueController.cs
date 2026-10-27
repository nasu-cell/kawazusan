using UnityEngine;
using DG.Tweening;
using System;

public class TongueController : MonoBehaviour
{
    public event Action ExtensionStarted;
    public event Action Retracted;
    public event Action<Vector2> Interrupted;

    [SerializeField] private TongueState currentState = TongueState.Ready;

    [SerializeField] private Transform tongueLine;
    [SerializeField] private Transform tongueTip;
    [SerializeField] private SpriteRenderer tongueLineSpriteRenderer;
    [SerializeField] private SpriteRenderer tongueTipSpriteRenderer;
    [SerializeField] private TongueCollisionDetector collisionDetector;
    [SerializeField] private ParticleInterruptSpawner particleInterruptSpawner;

    [SerializeField] private float tongueLengthScale = 0.25f;
    [SerializeField] private float extendSpeed = 5f;
    [SerializeField] private Ease extendEase = Ease.OutQuad;

    [SerializeField] private float retractDelay = 0.5f;
    [SerializeField] private Ease retractEase = Ease.InQuad;

    private Tween tongueTween;
    private Tween retractDelayTween;
    private bool canExtend = true;
    [SerializeField] private FrogAppearance frogAppearance;

    private void Awake()
    {
        SetTongueVisible(false);
    }

    private void Start()
    {
        tongueLine.localScale = Vector3.one;

        // 初期状態をCollisionDetectorに通
        collisionDetector.UpdateTonguePosition(
            tongueLine.position,
            tongueLine.position
        );
    }

    public void ExtendTongue(Vector2 targetPosition)
    {
        if (!canExtend || currentState != TongueState.Ready)
            return;

        currentState = TongueState.Extending;
        SetTongueVisible(true);
        frogAppearance.OpenMouthForTongue();
        ExtensionStarted?.Invoke();

        ExtendTongueTween(targetPosition);
    }

    public void SetInputEnabled(bool isEnabled)
    {
        canExtend = isEnabled;
    }

    private void ExtendTongueTween(Vector2 targetPosition)
    {
        tongueTween?.Kill();
        retractDelayTween?.Kill();

        Vector2 startPosition = tongueLine.position;

        float distance = Vector2.Distance(
            startPosition,
            targetPosition
        );

        float extendDuration = Mathf.Max(
            distance / extendSpeed,
            0.01f
        );

        float t = 0f;

        tongueTween = DOTween.To(
            () => t,
            value =>
            {
                t = value;

                float easedT = DOVirtual.EasedValue(
                    0f,
                    1f,
                    t,
                    extendEase
                );

                Vector2 currentTipPosition = Vector2.Lerp(
                    tongueLine.position,
                    targetPosition,
                    easedT
                );

                UpdateTongue(currentTipPosition);
            },
            1f,
            extendDuration
        )
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            UpdateTongue(targetPosition);

            currentState = TongueState.Retracting;

            retractDelayTween = DOVirtual.DelayedCall(
                retractDelay,
                () =>
                {
                    RetractTongue(
                        targetPosition,
                        extendDuration
                    );
                }
            );
        });
    }

    private void RetractTongue(
        Vector2 targetPosition,
        float duration)
    {
        float t = 0f;

        tongueTween = DOTween.To(
            () => t,
            value =>
            {
                t = value;

                float easedT = DOVirtual.EasedValue(
                    1f,
                    0f,
                    t,
                    retractEase
                );

                Vector2 currentTipPosition = Vector2.Lerp(
                    tongueLine.position,
                    targetPosition,
                    easedT
                );

                UpdateTongue(currentTipPosition);
            },
            1f,
            duration
        )
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            UpdateTongue(tongueLine.position);

            // Targetを解放
            collisionDetector.ReleaseTarget();

            currentState = TongueState.Ready;
            SetTongueVisible(false);
            frogAppearance.CloseMouthAfterTongue();
            Retracted?.Invoke();
        });
    }

    private void SetTongueVisible(bool isVisible)
    {
        tongueLineSpriteRenderer.enabled = isVisible;
        tongueTipSpriteRenderer.enabled = isVisible;
    }

    private void UpdateTongue(Vector2 tipPosition)
    {
        // -------------------------
        // TongueTip
        // -------------------------

        tongueTip.position = tipPosition;

        // -------------------------
        // TongueLine
        // -------------------------

        Vector2 direction =
            tipPosition - (Vector2)tongueLine.position;

        float distance = direction.magnitude;

        if (distance > 0.001f)
        {
            // SpriteのY+方向を舌の方向に向ける
            tongueLine.up = direction;

            Vector3 scale = Vector3.one;
            scale.y = distance * tongueLengthScale;

            tongueLine.localScale = scale;
        }
        else
        {
            /*
            tongueLine.localScale = Vector3.zero;
            */
        }

        // -------------------------
        // 捕獲対象
        // -------------------------
        
        collisionDetector.UpdateTonguePosition(
            tongueLine.position,
            tipPosition
        );
    }

    public void InterruptTongue(Vector2 causeWorldPosition)
    {
        // 伸びるTween・縮む待機Tweenを停止
        tongueTween?.Kill();
        retractDelayTween?.Kill();

        // 現在の舌先位置を取得
        Vector2 currentTipPosition = tongueTip.position;

        // すぐ縮む
        currentState = TongueState.Retracting;

        float currentDistance = Vector2.Distance(
            tongueLine.position,
            currentTipPosition
        );

        float retractDuration = Mathf.Max(
            currentDistance / extendSpeed,
            0.01f
        );

        RetractTongue(
            currentTipPosition,
            retractDuration
        );

        // 見た目の演出で問題が起きても、舌の収縮処理は先に開始しておく
        frogAppearance.Interrupt(causeWorldPosition);

        if (particleInterruptSpawner != null)
        {
            particleInterruptSpawner.SpawnParticle(causeWorldPosition);
        }

        Interrupted?.Invoke(causeWorldPosition);
    }

    private void OnDestroy()
    {
        tongueTween?.Kill();
        retractDelayTween?.Kill();
    }
}
