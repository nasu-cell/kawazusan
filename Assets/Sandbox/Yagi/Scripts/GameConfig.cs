using UnityEngine;

[CreateAssetMenu(
    fileName = "GameConfig",
    menuName = "Game/GameConfig"
)]
public class GameConfig : ScriptableObject
{
    [Header("Player")]
    [SerializeField] private int satietyMax = 100;

    [Header("Tongue")]
    [SerializeField] private float tongueExtendSpeed = 5f;
    [SerializeField] private float tongueRetractSpeed = 5f;
    [SerializeField] private float tongueMaxLength = 10f;

    [Header("Spawn")]
    [SerializeField] private int spawnSeed = 0;

    public int SatietyMax => satietyMax;
    public float TongueExtendSpeed => tongueExtendSpeed;
    public float TongueRetractSpeed => tongueRetractSpeed;
    public float TongueMaxLength => tongueMaxLength;
    public int SpawnSeed => spawnSeed;
}