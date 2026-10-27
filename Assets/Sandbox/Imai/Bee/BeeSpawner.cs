using System.Collections.Generic;
using UnityEngine;

public class BeeSpawner : MonoBehaviour
{
    [Header("ハチPrefab")]
    [SerializeField]
    private GameObject beePrefab;


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


    [Header("初期生成数")]
    [SerializeField]
    private int initialBeeCount = 1;


    [Header("満腹度による追加生成")]
    [SerializeField]
    private BeeSpawnThreshold[] spawnThresholds;


    private System.Random random;
    [SerializeField]
    private SatietyController satietyController;

    // 現在生成されているハチの数
    private int currentBeeCount = 0;

    // 次に判定するThresholdの番号
    private int nextThresholdIndex = 0;

    private bool canSpawn = true;
    private bool hasSpawnedInitialBees = false;


    private void Awake()
    {
        random = new System.Random(seed);
    }


    private void Start()
    {
        if (canSpawn)
        {
            StartSpawning();
        }
    }


    public void StartSpawning()
    {
        canSpawn = true;

        if (random == null)
        {
            random = new System.Random(seed);
        }

        if (!hasSpawnedInitialBees)
        {
            hasSpawnedInitialBees = true;

            // 最初のハチを生成
            for (int i = 0; i < initialBeeCount; i++)
            {
                SpawnBee();
            }
        }
    }


    public void StopSpawning()
    {
        canSpawn = false;
    }


    private void Update()
    {
        if (!canSpawn)
        {
            return;
        }

        if (satietyController == null)
        {
            return;
        }


        if (spawnThresholds == null || nextThresholdIndex >= spawnThresholds.Length)
        {
            return;
        }


        // 現在の満腹度の割合
        float satietyRatio =
            (float)satietyController.CurrentSatiety
            / satietyController.MaxSatiety;


        // 次の閾値を超えたか確認
        if (satietyRatio >=
            spawnThresholds[nextThresholdIndex].satietyRatio)
        {
            int spawnCount =
                spawnThresholds[nextThresholdIndex].beeCount;


            // 指定された数までハチを増やす
            while (currentBeeCount < spawnCount)
            {
                SpawnBee();
            }


            // 次の閾値へ
            nextThresholdIndex++;
        }
    }


    /// <summary>
    /// ハチを1匹生成する
    /// </summary>
    private void SpawnBee()
    {
        if (beePrefab == null)
        {
            Debug.LogError(
                "BeePrefabが設定されていません。"
            );

            return;
        }


        // スポーン位置をランダムに決定
        float x =
            GetRandomFloat(minX, maxX);

        float y =
            GetRandomFloat(minY, maxY);


        Vector3 spawnPosition =
            new Vector3(x, y, 0f);


        // ハチを生成
        GameObject beeObject =
            Instantiate(
                beePrefab,
                spawnPosition,
                Quaternion.identity
            );


        currentBeeCount++;


        // ハチごとのSeedを生成
        int beeSeed =
            random.Next();


        BeeMove beeMove = beeObject.GetComponent<BeeMove>();

        if (beeMove != null)
        {
            beeMove.SetSeed(beeSeed);
        }

        Debug.Log(
            $"ハチを生成しました。" +
            $" No.{currentBeeCount}" +
            $" Seed:{beeSeed}"
        );
    }


    /// <summary>
    /// 指定された範囲からランダムなfloatを取得する
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


/// <summary>
/// 満腹度に応じたハチの生成設定
/// </summary>
[System.Serializable]
public class BeeSpawnThreshold
{
    [Range(0f, 1f)]
    public float satietyRatio;

    public int beeCount;
}
