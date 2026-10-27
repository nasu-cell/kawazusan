using UnityEngine;

[CreateAssetMenu(
    fileName = "FlyFormation",
    menuName = "Game/Fly Formation"
)]
public class FlyFormation : ScriptableObject
{
    public Vector2[] positions;
}