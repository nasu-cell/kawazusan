using UnityEngine;

public class BigDespawn : MonoBehaviour
{
    [Header("消滅までの時間")]
    [SerializeField]
    private float despawnTime = 10f;


    private float timer;

    private FlyController flyController;
    private BeeMove beeMove;

    // 捕獲中だったか
    private bool wasCaptured = false;


    private void Start()
    {
        timer = despawnTime;

        flyController =
            GetComponent<FlyController>();

        beeMove =
            GetComponent<BeeMove>();
    }


    private void Update()
    {
        if (flyController == null)
        {
            return;
        }


        FlyState state =
            flyController.GetState();


        // -------------------------
        // 捕獲中
        // -------------------------

        if (state == FlyState.Captured)
        {
            // 初めてCapturedになったときだけ移動停止
            if (!wasCaptured)
            {
                wasCaptured = true;

                if (beeMove != null)
                {
                    beeMove.enabled = false;
                }
            }

            // 捕獲中はタイマーを進めない
            return;
        }


        // -------------------------
        // 捕獲状態から戻った
        // -------------------------

        if (wasCaptured)
        {
            wasCaptured = false;

            if (beeMove != null)
            {
                beeMove.enabled = true;
            }
        }


        // -------------------------
        // すでに食べられた場合
        // -------------------------

        if (state == FlyState.Consumed)
        {
            return;
        }


        // -------------------------
        // 消滅済みの場合
        // -------------------------

        if (state == FlyState.Despawned)
        {
            return;
        }


        // -------------------------
        // 消滅タイマー
        // -------------------------

        timer -= Time.deltaTime;


        if (timer <= 0f)
        {
            Despawn();
        }
    }


    private void Despawn()
    {
        if (flyController != null)
        {
            flyController.Despawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}