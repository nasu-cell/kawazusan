using UnityEngine;
using DG.Tweening;

public class FlyController : MonoBehaviour
{
    [Header("ハエの種類")]
    [SerializeField]
    private FlyType flyType;


    [Header("ハエのステータス")]
    [SerializeField]
    private int maxHP = 10;

    [SerializeField]
    private int hungerValue = 10;

    [Header("ダメージ演出")]
    [SerializeField]
    private Transform damageShakeTarget;

    [SerializeField, Min(0f)]
    private float damageShakeDistance = 1f;

    [SerializeField, Min(0.01f)]
    private float damageShakeStepDuration = 0.04f;


    private int currentHP;

    private FlyState currentState = FlyState.Moving;

    private FlyMove flyMove;

    private Sequence damageShakeSequence;

    private Vector3 damageShakeBaseLocalPosition;


    private void Awake()
    {
        currentHP = maxHP;

        flyMove = GetComponent<FlyMove>();
    }


    public TongueHitResult OnTongueHit(int damage)
    {
        // ハチの場合
        if (flyType == FlyType.Bee)
        {
            return TongueHitResult.Interrupt;
        }


        // Moving状態以外では攻撃を受けない
        if (currentState != FlyState.Moving)
        {
            return TongueHitResult.Damage;
        }


        // ダメージを受ける
        currentHP -= damage;

        Debug.Log(
            $"{gameObject.name} が {damage} ダメージを受けた。" +
            $" HP: {currentHP}/{maxHP}"
        );


        // HPが0になった
        if (currentHP <= 0)
        {
            currentHP = 0;

            Capture();

            return TongueHitResult.Capture;
        }


        PlayDamageShake();


        // まだHPが残っている
        return TongueHitResult.Damage;
    }


    public void Capture()
    {
        if (currentState != FlyState.Moving)
        {
            return;
        }


        currentState = FlyState.Captured;


        // 捕まったので移動を停止
        if (flyMove != null)
        {
            flyMove.enabled = false;
        }


        Debug.Log(
            $"{gameObject.name} がCapturedになりました。"
        );
    }


    public void Consume()
    {
        if (currentState != FlyState.Captured)
        {
            return;
        }


        currentState = FlyState.Consumed;


        Debug.Log(
            $"{gameObject.name} を食べた。" +
            $" 満腹度 +{hungerValue}"
        );


        // 現時点では仮
        Destroy(gameObject);
    }


    public void Despawn()
    {
        if (currentState == FlyState.Consumed)
        {
            return;
        }


        currentState = FlyState.Despawned;


        Debug.Log(
            $"{gameObject.name} が画面外に出ました。"
        );


        Destroy(gameObject);
    }


    public void Release()
    {
        if (currentState != FlyState.Captured)
        {
            return;
        }


        // Releaseされたハエは次の一撃で再捕獲できるようにする
        currentHP = 1;


        currentState = FlyState.Moving;


        // 移動を再開
        if (flyMove != null)
        {
            flyMove.enabled = true;
        }


        Debug.Log(
            $"{gameObject.name} がReleaseされました。" +
            $" HP: {currentHP}"
        );
    }


    public FlyState GetState()
    {
        return currentState;
    }


    public FlyType GetFlyType()
    {
        return flyType;
    }


    public int GetCurrentHP()
    {
        return currentHP;
    }


    public int GetHungerValue()
    {
        return hungerValue;
    }


    private void PlayDamageShake()
    {
        if (damageShakeTarget == null)
        {
            return;
        }


        StopDamageShakeAndResetPosition();

        damageShakeBaseLocalPosition = damageShakeTarget.localPosition;

        float initialDirection = Random.value < 0.5f ? -1f : 1f;
        float largeOffset = initialDirection * damageShakeDistance;
        float smallOppositeOffset = -initialDirection * damageShakeDistance * 0.5f;
        float smallerInitialOffset = initialDirection * damageShakeDistance * 0.25f;

        damageShakeSequence = DOTween.Sequence()
            .Append(damageShakeTarget.DOLocalMoveX(
                damageShakeBaseLocalPosition.x + largeOffset,
                damageShakeStepDuration
            ).SetEase(Ease.OutQuad))
            .Append(damageShakeTarget.DOLocalMoveX(
                damageShakeBaseLocalPosition.x + smallOppositeOffset,
                damageShakeStepDuration
            ).SetEase(Ease.InOutQuad))
            .Append(damageShakeTarget.DOLocalMoveX(
                damageShakeBaseLocalPosition.x + smallerInitialOffset,
                damageShakeStepDuration
            ).SetEase(Ease.InOutQuad))
            .Append(damageShakeTarget.DOLocalMoveX(
                damageShakeBaseLocalPosition.x,
                damageShakeStepDuration
            ).SetEase(Ease.InQuad))
            .OnComplete(() => damageShakeSequence = null);
    }


    private void OnDisable()
    {
        StopDamageShakeAndResetPosition();
    }


    private void StopDamageShakeAndResetPosition()
    {
        if (damageShakeSequence == null)
        {
            return;
        }


        damageShakeSequence.Kill();
        damageShakeSequence = null;
        damageShakeTarget.localPosition = damageShakeBaseLocalPosition;
    }
}
