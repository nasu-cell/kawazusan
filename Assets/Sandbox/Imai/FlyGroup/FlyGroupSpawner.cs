using UnityEngine;

public class FlyFormationSpawner : MonoBehaviour
{
    [Header("大群スポーンデータ")]
    [SerializeField]
    private FlyFormationSpawnData[] spawnDatas;


    [Header("シード値")]
    [SerializeField]
    private int seed = 12345;


    [Header("生成間隔")]
    [SerializeField]
    private float spawnInterval = 10f;


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


    private System.Random random;


    private void Start()
    {
        // シード値から乱数生成器を作成
        random = new System.Random(seed);

        StartCoroutine(SpawnLoop());
    }


    private System.Collections.IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            SpawnFormation();

        }
    }


    /// <summary>
    /// 大群を1つ生成する
    /// </summary>
    private void SpawnFormation()
    {
        if (spawnDatas == null || spawnDatas.Length == 0)
        {
            Debug.LogWarning(
                "FlyFormationSpawnDataが設定されていません。"
            );

            return;
        }


        // SpawnDataをランダムに1つ選択
        FlyFormationSpawnData spawnData =
            GetRandomSpawnData();


        if (spawnData == null)
        {
            Debug.LogWarning(
                "有効なFlyFormationSpawnDataがありません。"
            );

            return;
        }


        // SpawnDataからスポーンエリアを取得
        SpawnArea spawnArea =
            spawnData.spawnArea;


        // 大群の基準位置を決定
        Vector3 basePosition =
            GetRandomSpawnPosition(spawnArea);


        // Formationを取得
        FlyFormation formation =
            spawnData.formation;


        if (formation == null ||
            formation.positions == null ||
            formation.positions.Length == 0)
        {
            Debug.LogWarning(
                $"{spawnData.name} にFormationが設定されていません。"
            );

            return;
        }


        // Formationの人数分だけハエを生成
        foreach (Vector2 offset in formation.positions)
        {
            Vector3 spawnPosition =
                basePosition +
                new Vector3(
                    offset.x,
                    offset.y,
                    0f
                );


            GameObject flyObject =
                Instantiate(
                    spawnData.flyPrefab,
                    spawnPosition,
                    Quaternion.identity
                );


            // FlyMoveを取得
            FlyMove flyMove =
                flyObject.GetComponent<FlyMove>();


            if (flyMove != null)
            {
                // 全員に同じ移動パターンを設定
                flyMove.SetMovePattern(
                    spawnData.movePattern
                );
            }
            else
            {
                Debug.LogWarning(
                    $"{spawnData.flyPrefab.name} に " +
                    "FlyMoveがありません。"
                );
            }
        }


        Debug.Log(
            $"{spawnData.name} の大群を生成しました。" +
            $" 数: {formation.positions.Length}"
        );
    }


    /// <summary>
    /// SpawnDataからランダムに1つ選択する
    /// </summary>
    private FlyFormationSpawnData GetRandomSpawnData()
    {
        int index =
            random.Next(0, spawnDatas.Length);

        return spawnDatas[index];
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