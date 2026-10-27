using UnityEngine;

public class DespawnArea : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        FlyController fly =
            other.GetComponent<FlyController>();

        if (fly == null)
        {
            return;
        }

        fly.Despawn();
    }
}