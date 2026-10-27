using System.Collections.Generic;
using UnityEngine;

public class TongueCollisionDetector : MonoBehaviour
{
    [SerializeField] private SatietyController satietyController;
    [SerializeField] private GameHUDController gameHUDController;
    [SerializeField] private TongueController tongueController;

    private class TargetData
    {
        public FlyController flyController;
        public float ratio;
        public int hungerValue;
    }

    private readonly List<TargetData> targets = new List<TargetData>();

    private Vector2 tongueOrigin;
    private Vector2 tongueTip;

    public bool HasTarget => targets.Count > 0;
    private int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out FlyController flyController))
            return;

        TongueHitResult result = flyController.OnTongueHit(damage);

        switch (result)
        {
            case TongueHitResult.Capture:
                AddTarget(flyController);
                break;

            case TongueHitResult.Interrupt:
                ReleaseCapturedTargets();
                PlaySE(SoundData.SoundType.Damaged);
                Debug.Log("Tongue hit a target that interrupts the tongue.");
                tongueController.InterruptTongue(other.transform.position);
                break;
        }
    }

    private void AddTarget(FlyController flyController)
    {
        // すでに登録されているTargetなら追加しない
        foreach (TargetData targetData in targets)
        {
            if (targetData.flyController == flyController)
                return;
        }

        // 新しいTargetを追加
        TargetData newTarget = new TargetData();
        newTarget.flyController = flyController;
        newTarget.hungerValue = flyController.GetHungerValue();

        // Targetが舌の何%地点にいるかを保存
        float targetDistance =
            Vector2.Distance(
                tongueOrigin,
                flyController.transform.position
            );

        float tongueLength = Vector2.Distance(tongueOrigin, tongueTip);
        newTarget.ratio = targetDistance / tongueLength;

        targets.Add(newTarget);
        PlaySE(SoundData.SoundType.FlyTrapping);

        // World Rotationを0にする
        flyController.transform.rotation = Quaternion.identity;
    }

    public void UpdateTonguePosition(
        Vector2 origin,
        Vector2 tip)
    {
        tongueOrigin = origin;
        tongueTip = tip;

        if (targets.Count == 0)
            return;

        Vector2 tongueVector = tip - origin;
        float tongueLength = tongueVector.magnitude;

        if (tongueLength <= 0.001f)
            return;

        Vector2 direction = tongueVector.normalized;

        // 登録されている全Targetを更新
        foreach (TargetData targetData in targets)
        {
            if (targetData.flyController == null)
                continue;

            // 舌の現在の長さ × Targetの割合
            float targetDistance =
                tongueLength * targetData.ratio;

            Transform targetTransform =
                targetData.flyController.transform;

            // Positionだけ変更
            targetTransform.position =
                origin + direction * targetDistance;

            // World Rotationは常に0
            targetTransform.rotation = Quaternion.identity;
        }
    }

    public void ReleaseTarget()
    {
        Debug.Log("ReleaseTarget called. Number of targets: " + targets.Count);

        int targetCount = targets.Count;
        bool swallowedTarget = targetCount > 0;
        int totalHungerValue = 0;

        foreach (TargetData targetData in targets)
        {
            totalHungerValue += targetData.hungerValue;
            targetData.flyController.Consume();
        }

        targets.Clear();

        if (swallowedTarget)
        {
            PlaySE(SoundData.SoundType.FlogSwallowing);
        }

        if (targetCount >= 3)
        {
            if (gameHUDController == null)
            {
                gameHUDController = FindAnyObjectByType<GameHUDController>();
            }

            if (gameHUDController != null)
            {
                gameHUDController.ShowBonusText(targetCount);
            }
        }

        satietyController.AddSatiety(totalHungerValue);
    }

    private void ReleaseCapturedTargets()
    {
        foreach (TargetData targetData in targets)
        {
            if (targetData.flyController != null)
            {
                targetData.flyController.Release();
            }
        }

        targets.Clear();
    }

    private static void PlaySE(SoundData.SoundType soundType)
    {
        if (SEManager.Instance != null)
        {
            SEManager.Instance.PlaySE(soundType);
        }
    }
}
