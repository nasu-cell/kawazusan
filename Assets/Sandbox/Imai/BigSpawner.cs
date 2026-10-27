using UnityEngine;

public class BigSpawner : MonoBehaviour
{
    [Header("ボスハエPrefab")]
    [SerializeField]
    private GameObject bigPrefab;


    [Header("シード値")]
    [SerializeField]
    private int seed = 12345;


    [Header("スポーン範囲")]
    [SerializeField]
    private float minX = -10f;

    [SerializeField]
    private float maxX = 10f;

    [SerializeField]
    private float minY = -4f;

    [SerializeField]
    private float maxY = 8f;


    [Header("生成間隔")]
    [SerializeField]
    private float spawnInterval = 10f;


    private System.Random random;


    private void Awake()
    {
        // シード値から乱数生成器を作成
        random = new System.Random(seed);
    }


    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }


    private System.Collections.IEnumerator SpawnLoop()
    {
        while (true)
        {
            // 最初は生成せず、一定時間待つ
            yield return new WaitForSeconds(spawnInterval);

            SpawnBig();
        }
    }


    /// <summary>
    /// ボスハエを1匹生成する
    /// </summary>
    private void SpawnBig()
    {
        if (bigPrefab == null)
        {
            Debug.LogError(
                "BigPrefabが設定されていません。"
            );

            return;
        }


        // スポーン位置をランダムに決定
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


        Vector3 spawnPosition =
            new Vector3(
                x,
                y,
                0f
            );


        // ボスハエを生成
        GameObject bigObject =
            Instantiate(
                bigPrefab,
                spawnPosition,
                Quaternion.identity
            );


        // 個体ごとのSeedを生成
        int bigSeed =
            random.Next();


        // BeeMoveを取得
        BeeMove beeMove =
            bigObject.GetComponent<BeeMove>();


        if (beeMove != null)
        {
            beeMove.SetSeed(bigSeed);
        }
        else
        {
            Debug.LogWarning(
                $"{bigPrefab.name} に " +
                "BeeMoveがありません。"
            );
        }


        Debug.Log(
            $"ボスハエを生成しました。" +
            $" Seed:{bigSeed}"
        );
    }


    /// <summary>
    /// 指定された範囲からランダムなfloatを取得
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