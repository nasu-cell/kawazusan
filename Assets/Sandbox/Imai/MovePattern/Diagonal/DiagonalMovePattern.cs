using UnityEngine;

[CreateAssetMenu(
    fileName = "DiagonalMovePattern",
    menuName = "Game/Move Pattern/Diagonal"
)]
public class DiagonalMovePattern : MovePattern
{
    [SerializeField]
    private float horizontalDirection = 1f;

    [SerializeField]
    private float verticalDirection = 1f;

    public override Vector3 GetMovement(
        Transform fly,
        float time)
    {
        return new Vector3(
            horizontalDirection * speed,
            verticalDirection * speed,
            0f
        );
    }
}