using UnityEngine;

[CreateAssetMenu(
    fileName = "FlyFormationSpawnData",
    menuName = "Game/Fly Formation Spawn Data"
)]
public class FlyFormationSpawnData : ScriptableObject
{
    public GameObject flyPrefab;
    public FlyType flyType;
    public MovePattern movePattern;
    public SpawnArea spawnArea;
    public FlyFormation formation;
}