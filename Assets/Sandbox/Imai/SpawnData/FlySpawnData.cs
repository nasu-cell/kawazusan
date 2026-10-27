using UnityEngine;

[CreateAssetMenu(
    fileName = "FlySpawnData",
    menuName = "Game/Fly Spawn Data"
)]
public class FlySpawnData : ScriptableObject
{
    public GameObject flyPrefab;

    public FlyType flyType;

    public MovePattern movePattern;

    public SpawnArea spawnArea;
}