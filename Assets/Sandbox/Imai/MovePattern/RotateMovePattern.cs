using UnityEngine;

[CreateAssetMenu(
    fileName = "RotateMovePattern",
    menuName = "Game/Move Pattern/Rotate"
)]
public class RotateMovePattern : MovePattern
{
    [SerializeField]
    private float direction = 1f;

    [SerializeField]
    private float radius = 1f;

    [SerializeField]
    private float rotationSpeed = 2f;

    public override Vector3 GetMovement(
        Transform fly,
        float time)
    {
        // 円運動による移動
        float circleX =
            Mathf.Cos(time * rotationSpeed) * radius;

        float circleY =
            Mathf.Sin(time * rotationSpeed) * radius;

        // 左右への移動
        float moveX = direction * speed;

        return new Vector3(
            moveX + circleX,
            circleY,
            0f
        );
    }
}