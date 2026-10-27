using System.Collections.Generic;
using UnityEngine;

public class ParticleInterruptSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TongueController tongueController;

    [Header("Particle Settings")]
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private int poolSize = 10;

    [Header("2D Settings")]
    [SerializeField] private float spawnZ = 0f;

    private readonly List<ParticleSystem> particlePool =
        new List<ParticleSystem>();

    private void Awake()
    {
        CreatePool();
    }

    private void OnEnable()
    {
        if (tongueController != null)
        {
            tongueController.Interrupted += OnTongueInterrupted;
        }
    }

    private void OnDisable()
    {
        if (tongueController != null)
        {
            tongueController.Interrupted -= OnTongueInterrupted;
        }
    }

    private void OnTongueInterrupted(Vector2 causePosition)
    {
        SpawnParticle(causePosition);
    }

    private void CreatePool()
    {
        if (particlePrefab == null)
        {
            Debug.LogError("Particle Prefabが設定されていません。");
            return;
        }

        if (poolSize <= 0)
        {
            Debug.LogError("Pool Sizeには1以上を設定してください。");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            CreateParticleInstance();
        }
    }

    private ParticleSystem CreateParticleInstance()
    {
        if (particlePrefab == null)
        {
            return null;
        }

        GameObject particleObject =
            Instantiate(particlePrefab, transform);

        ParticleSystem particle =
            particleObject.GetComponentInChildren<ParticleSystem>();

        if (particle == null)
        {
            Debug.LogError(
                "Particle Prefabまたはその子オブジェクトに" +
                "ParticleSystemがありません。"
            );

            Destroy(particleObject);
            return null;
        }

        particleObject.SetActive(false);
        particlePool.Add(particle);
        return particle;
    }

    public void SpawnParticle(Transform targetTransform)
    {
        if (targetTransform != null)
        {
            SpawnParticle(targetTransform.position);
        }
    }

    public void SpawnParticle(Vector2 worldPosition)
    {
        SpawnParticle(new Vector3(worldPosition.x, worldPosition.y, spawnZ));
    }

    public void SpawnParticle(Vector3 worldPosition)
    {
        ParticleSystem particle = GetAvailableParticle();

        if (particle == null)
        {
            return;
        }

        particle.transform.position =
            new Vector3(worldPosition.x, worldPosition.y, spawnZ);

        particle.gameObject.SetActive(true);
        particle.Play();
    }

    private ParticleSystem GetAvailableParticle()
    {
        foreach (ParticleSystem particle in particlePool)
        {
            if (particle != null && !particle.isPlaying)
            {
                return particle;
            }
        }

        return CreateParticleInstance();
    }
}