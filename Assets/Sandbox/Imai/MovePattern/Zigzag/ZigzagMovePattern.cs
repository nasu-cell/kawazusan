using UnityEngine;

[CreateAssetMenu(
    fileName = "ZigZagMovePattern",
    menuName = "Game/Move Pattern/ZigZag"
)]
public class ZigZagMovePattern : MovePattern
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
        float y = Mathf.Sin(time * frequency) * amplitude;

        return new Vector3(
            direction * speed,
            y,
            0f
        );
    }
}