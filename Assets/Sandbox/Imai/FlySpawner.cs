using UnityEngine;

public class FlySpawner : MonoBehaviour
{
    [Header("スポーンデータ")]
    [SerializeField]
    private FlySpawnData[] spawnDatas;


    [Header("シード値")]
    [SerializeField]
    private int seed = 12345;


    [Header("ハエの出現確率")]
    [SerializeField]
    private float normalWeight = 80f;

    [SerializeField]
    private float bossWeight = 10f;

    [SerializeField]
    private float beeWeight = 10f;


    [Header("左エリア")]
    [SerializeField]
    private float leftMinX = -12f;

    [SerializeField]
    private float leftMaxX = -10f;

    [SerializeField]
    private float leftMinY = -4f;

    [SerializeField]
    private float leftMaxY = 8f;


    [Header("右エリア")]
    [SerializeField]
    private float rightMinX = 10f;

    [SerializeField]
    private float rightMaxX = 12f;

    [SerializeField]
    private float rightMinY = -4f;

    [SerializeField]
    private float rightMaxY = 8f;


    [Header("下エリア")]
    [SerializeField]
    private float bottomMinX = -10f;

    [SerializeField]
    private float bottomMaxX = 10f;

    [SerializeField]
    private float bottomMinY = -8f;

    [SerializeField]
    private float bottomMaxY = -6f;


    [Header("生成間隔")]
    [SerializeField]
    private float minSpawnInterval = 1f;

    [SerializeField]
    private float maxSpawnInterval = 3f;


    private System.Random random;
    private Coroutine spawnLoopCoroutine;
    private bool canSpawn = true;


    private void Awake()
    {
        random = new System.Random(seed);
    }


    private void Start()
    {
        // シード値から乱数生成器を作成
        if (canSpawn)
        {
            StartSpawning();
        }
    }


    public void StartSpawning()
    {
        canSpawn = true;

        if (spawnLoopCoroutine == null && isActiveAndEnabled)
        {
            spawnLoopCoroutine = StartCoroutine(SpawnLoop());
        }
    }


    public void StopSpawning()
    {
        canSpawn = false;

        if (spawnLoopCoroutine != null)
        {
            StopCoroutine(spawnLoopCoroutine);
            spawnLoopCoroutine = null;
        }
    }


    private void OnDisable()
    {
        if (spawnLoopCoroutine != null)
        {
            StopCoroutine(spawnLoopCoroutine);
            spawnLoopCoroutine = null;
        }
    }


    private System.Collections.IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnFly();

            float interval = GetRandomSpawnInterval();

            yield return new WaitForSeconds(interval);
        }
    }


    private void SpawnFly()
    {
        if (spawnDatas == null || spawnDatas.Length == 0)
        {
            Debug.LogWarning(
                "SpawnDataが設定されていません。"
            );

            return;
        }


        // ハエの種類を確率で決定
        FlyType flyType = GetRandomFlyType();


        // 選ばれた種類のSpawnDataからランダムに選択
        FlySpawnData spawnData =
            GetRandomSpawnData(flyType);


        if (spawnData == null)
        {
            Debug.LogWarning(
                $"{flyType} に対応するSpawnDataがありません。"
            );

            return;
        }


        // SpawnDataに設定されたスポーンエリアを取得
        SpawnArea spawnArea =
            spawnData.spawnArea;


        // スポーン位置をランダムに決定
        Vector3 spawnPosition =
            GetRandomSpawnPosition(spawnArea);


        // Prefabを生成
        GameObject flyObject = Instantiate(
            spawnData.flyPrefab,
            spawnPosition,
            Quaternion.identity
        );


        // FlyMoveを取得
        FlyMove flyMove =
            flyObject.GetComponent<FlyMove>();


        if (flyMove != null)
        {
            // 移動パターンを渡す
            flyMove.SetMovePattern(
                spawnData.movePattern
            );
        }
        else
        {
            Debug.LogWarning(
                $"{spawnData.flyPrefab.name} に " +
                $"FlyMove がありません。"
            );
        }
    }


    /// <summary>
    /// ハエの種類を確率で決定する
    /// </summary>
    private FlyType GetRandomFlyType()
    {
        float totalWeight =
            normalWeight +
            bossWeight +
            beeWeight;


        // 重みの合計が0以下の場合
        if (totalWeight <= 0f)
        {
            Debug.LogWarning(
                "ハエの出現確率が正しく設定されていません。"
            );

            return FlyType.Normal;
        }


        float value =
            (float)random.NextDouble()
            * totalWeight;


        // Normal
        if (value < normalWeight)
        {
            return FlyType.Normal;
        }


        value -= normalWeight;


        // Big
        if (value < bossWeight)
        {
            return FlyType.Big;
        }


        // Bee
        return FlyType.Bee;
    }


    /// <summary>
    /// 指定された種類のSpawnDataからランダムに1つ選択する
    /// </summary>
    private FlySpawnData GetRandomSpawnData(
        FlyType flyType)
    {
        System.Collections.Generic.List<FlySpawnData>
            candidates =
            new System.Collections.Generic.List<FlySpawnData>();


        // 指定されたFlyTypeのSpawnDataだけを候補にする
        foreach (FlySpawnData data in spawnDatas)
        {
            if (data != null &&
                data.flyType == flyType)
            {
                candidates.Add(data);
            }
        }


        // 候補がない
        if (candidates.Count == 0)
        {
            return null;
        }


        // 候補の中からランダムに選択
        int index =
            random.Next(0, candidates.Count);


        return candidates[index];
    }


    /// <summary>
    /// SpawnAreaに応じたランダムなスポーン位置を取得する
    /// </summary>
    private Vector3 GetRandomSpawnPosition(
        SpawnArea spawnArea)
    {
        float x;
        float y;


        switch (spawnArea)
        {
            case SpawnArea.Left:

                x = GetRandomFloat(
                    leftMinX,
                    leftMaxX
                );

                y = GetRandomFloat(
                    leftMinY,
                    leftMaxY
                );

                break;


            case SpawnArea.Right:

                x = GetRandomFloat(
                    rightMinX,
                    rightMaxX
                );

                y = GetRandomFloat(
                    rightMinY,
                    rightMaxY
                );

                break;


            case SpawnArea.Bottom:

                x = GetRandomFloat(
                    bottomMinX,
                    bottomMaxX
                );

                y = GetRandomFloat(
                    bottomMinY,
                    bottomMaxY
                );

                break;


            default:

                x = 0f;
                y = 0f;

                break;
        }


        return new Vector3(
            x,
            y,
            0f
        );
    }


    /// <summary>
    /// スポーン間隔をランダムに取得する
    /// </summary>
    private float GetRandomSpawnInterval()
    {
        return GetRandomFloat(
            minSpawnInterval,
            maxSpawnInterval
        );
    }


    /// <summary>
    /// 指定範囲のランダムなfloatを取得する
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
