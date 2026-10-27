using UnityEngine;

[CreateAssetMenu(
    fileName = "StraightMovePattern",
    menuName = "Game/Move Pattern/Straight"
)]
public class StraightMovePattern : MovePattern
{
    [SerializeField]
    private float direction = 1f;

    public override Vector3 GetMovement(
        Transform fly,
        float time)
    {
        return Vector3.right * speed * direction;
    }
}