using UnityEngine;

[CreateAssetMenu(
    fileName = "ZigZagUpMovePattern",
    menuName = "Game/Move Pattern/ZigZag Up"
)]
public class ZigZagUpMovePattern : MovePattern
{
    [SerializeField]
    private float direction = 1f;

    [SerializeField]
    private float amplitude = 1f;

    [SerializeField]
    private float frequency = 2f;

    public override Vector3 GetMovement(
        Transform fly,
        float time)
    {
        float x =
            Mathf.Sin(time * frequency) * amplitude;

        return new Vector3(
            x,
            direction * speed,
            0f
        );
    }
}