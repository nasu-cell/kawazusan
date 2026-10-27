using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ParticleTapSpawner : MonoBehaviour
{
    [Header("Particle Settings")]
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private int poolSize = 10;

    [Header("2D Settings")]
    [SerializeField] private float spawnZ = 0f;

    private readonly List<ParticleSystem> particlePool =
        new List<ParticleSystem>();

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        CreatePool();
    }

    private void Update()
    {
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 screenPosition =
                Touchscreen.current.primaryTouch.position.ReadValue();

            SpawnParticle(screenPosition);

            return;
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPosition =
                Mouse.current.position.ReadValue();

            SpawnParticle(screenPosition);
        }
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
                continue;
            }

            particleObject.SetActive(false);
            particlePool.Add(particle);
        }
    }

    private void SpawnParticle(Vector2 screenPosition)
    {
        ParticleSystem particle = GetAvailableParticle();

        if (particle == null)
        {
            return;
        }

        Vector3 worldPosition =
            GetWorldPosition(screenPosition);

        particle.transform.position = worldPosition;

        particle.gameObject.SetActive(true);
        particle.Play();
    }

    private ParticleSystem GetAvailableParticle()
    {
        foreach (ParticleSystem particle in particlePool)
        {
            if (!particle.isPlaying)
            {
                return particle;
            }
        }

        return null;
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Cameraが見つかりません。");
            return Vector3.zero;
        }

        float distanceFromCamera =
            Mathf.Abs(
                spawnZ - mainCamera.transform.position.z
            );

        Vector3 screenPositionWithDepth = new Vector3(
            screenPosition.x,
            screenPosition.y,
            distanceFromCamera
        );

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                screenPositionWithDepth
            );

        worldPosition.z = spawnZ;

        return worldPosition;
    }
}