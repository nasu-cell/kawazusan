using UnityEngine;

public class FlyMove : MonoBehaviour
{
    private MovePattern movePattern;

    private float startTime;

    public void SetMovePattern(MovePattern pattern)
    {
        movePattern = pattern;
        startTime = Time.time;
    }

    private void Update()
    {
        if (movePattern == null)
        {
            return;
        }

        Vector3 movement =
            movePattern.GetMovement(
                transform,
                Time.time - startTime
            );

        transform.position += movement * Time.deltaTime;
    }
}