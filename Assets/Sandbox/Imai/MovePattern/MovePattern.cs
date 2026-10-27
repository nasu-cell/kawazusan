using UnityEngine;

public abstract class MovePattern : ScriptableObject
{
    public float speed = 2f;

    public abstract Vector3 GetMovement(Transform fly, float time);
}