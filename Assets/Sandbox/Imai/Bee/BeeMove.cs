using UnityEngine;

public class BeeMove : MonoBehaviour
{
    [Header("移動範囲")]
    [SerializeField]
    private float minX = -10f;

    [SerializeField]
    private float maxX = 10f;

    [SerializeField]
    private float minY = -4f;

    [SerializeField]
    private float maxY = 8f;


    [Header("移動速度")]
    [SerializeField]
    private float moveSpeed = 2f;


    [Header("方向転換")]
    [SerializeField]
    private float minChangeTime = 1f;

    [SerializeField]
    private float maxChangeTime = 3f;


    [Header("上下の揺れ")]
    [SerializeField]
    private float waveAmplitude = 0.3f;

    [SerializeField]
    private float waveFrequency = 2f;


    // BeeSpawnerから受け取るSeed
    private int seed;

    // 個体ごとの乱数
    private System.Random random;

    // 現在の目標地点
    private Vector2 targetPosition;

    // 次の方向転換までの時間
    private float changeTimer;

    // 上下の揺れ用
    private float waveTime;


    /// <summary>
    /// BeeSpawnerから個体Seedを受け取る
    /// </summary>
    public void SetSeed(int beeSeed)
    {
        seed = beeSeed;

        random = new System.Random(seed);

        // 最初の目標地点を決定
        SetNextTarget();

        // 方向転換までの時間を決定
        changeTimer =
            GetRandomFloat(
                minChangeTime,
                maxChangeTime
            );

        // 上下の揺れの開始位置を個体ごとに変える
        waveTime =
            GetRandomFloat(0f, 100f);
    }


    private void Update()
    {
        // Seedが設定されていない場合
        if (random == null)
        {
            return;
        }


        // -------------------------
        // 目標地点へ移動
        // -------------------------

        Vector2 currentPosition =
            transform.position;

        Vector2 direction =
            (targetPosition - currentPosition)
            .normalized;


        // 移動
        currentPosition +=
            direction
            * moveSpeed
            * Time.deltaTime;


        // -------------------------
        // 上下の揺れ
        // -------------------------

        waveTime +=
            Time.deltaTime
            * waveFrequency;

        float wave =
            Mathf.Sin(waveTime)
            * waveAmplitude;


        currentPosition.y +=
            wave * Time.deltaTime;


        // -------------------------
        // 移動範囲から出ないようにする
        // -------------------------

        currentPosition.x =
            Mathf.Clamp(
                currentPosition.x,
                minX,
                maxX
            );

        currentPosition.y =
            Mathf.Clamp(
                currentPosition.y,
                minY,
                maxY
            );


        transform.position =
            currentPosition;


        // -------------------------
        // 目標地点に到達したか
        // -------------------------

        if (Vector2.Distance(
                currentPosition,
                targetPosition
            ) < 0.1f)
        {
            SetNextTarget();
        }


        // -------------------------
        // 一定時間ごとに方向転換
        // -------------------------

        changeTimer -=
            Time.deltaTime;

        if (changeTimer <= 0f)
        {
            SetNextTarget();

            changeTimer =
                GetRandomFloat(
                    minChangeTime,
                    maxChangeTime
                );
        }
    }


    /// <summary>
    /// 次の目標地点を決める
    /// </summary>
    private void SetNextTarget()
    {
        float x =
            GetRandomFloat(
                minX,
                maxX
            );

        float y =
            GetRandomFloat(
                minY,
                maxY
            );


        targetPosition =
            new Vector2(x, y);
    }


    /// <summary>
    /// 指定範囲のランダムなfloatを取得
    /// </summary>
    private float GetRandomFloat(
        float min,
        float max)
    {
        return (float)random.NextDouble()
            * (max - min)
            + min;
    }
}
