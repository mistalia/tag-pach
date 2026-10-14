using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerStateSmashed",
    menuName = "PlayerState/Smashed")]
public class PlayerStateSmashed : PlayerState
{
    private const float MinDirectionSqrMagnitude = 0.0001f;

    [Header("壁の判定")]
    [SerializeField]
    [Tooltip("吹き飛ばしを止める壁のレイヤー")]
    private LayerMask wallLayers;

    [SerializeField, Min(0f)]
    [Tooltip("壁へのめり込みを避けるために空ける距離")]
    private float wallPadding = 0.01f;

    private Rigidbody playerRigidbody;

    private Vector3 knockbackDirection;
    private float knockbackSpeed;
    private float remainingDistance;
    private float stunEndTime;

    // 吹き飛ばしの終了とは別に、追加被弾を無視する期間を表す。
    public bool IsStunned => Time.time < stunEndTime;

    public override void EnterState(
        Player owner,
        PlayerInputReader input)
    {
        base.EnterState(owner, input);

        playerRigidbody = owner.GetComponent<Rigidbody>();

        knockbackDirection = Vector3.zero;
        knockbackSpeed = 0f;
        remainingDistance = 0f;
        stunEndTime = 0f;

        if (playerRigidbody == null)
        {
            Debug.LogError(
                "PlayerStateSmashedに必要なRigidbodyがありません。",
                owner);

            owner.ChangeState("idle");
            return;
        }

        playerRigidbody.linearVelocity = Vector3.zero;
    }

    // Playerがこのステートへ切り替えた直後に呼ぶ。
    public void Setup(
        Vector3 direction,
        float distance,
        float speed,
        float stunDuration)
    {
        direction.y = 0f;

        if (!IsFinite(direction.x) ||
            !IsFinite(direction.z) ||
            !IsFinite(distance) ||
            !IsFinite(speed) ||
            !IsFinite(stunDuration) ||
            distance < 0f ||
            speed <= 0f ||
            stunDuration < 0f ||
            (distance > 0f &&
             direction.sqrMagnitude < MinDirectionSqrMagnitude))
        {
            Debug.LogError(
                "ノックバック情報が不正です。",
                owner);

            return;
        }

        knockbackDirection = direction.normalized;
        knockbackSpeed = speed;
        remainingDistance = distance;
        stunEndTime = Time.time + stunDuration;

        
    }

    public override void FixedUpdateState()
    {
        if (playerRigidbody == null)
        {
            return;
        }

        if (remainingDistance <= 0f)
        {
            if (!IsStunned)
            {
                owner.ChangeState("idle");
            }

            return;
        }

        float moveDistance = Mathf.Min(
            knockbackSpeed * Time.fixedDeltaTime,
            remainingDistance);

        bool hitWall = TryGetWallDistance(
            moveDistance,
            out float allowedDistance);

        if (allowedDistance > 0f)
        {
            Vector3 movement =
                knockbackDirection * allowedDistance;

            playerRigidbody.MovePosition(
                playerRigidbody.position + movement);
        }

        if (hitWall)
        {
            // 壁で止まった場合は、残りの距離を待ち続けない。
            remainingDistance = 0f;
        }
        else
        {
            remainingDistance = Mathf.Max(
                0f,
                remainingDistance - allowedDistance);
        }

        // 最後のMovePositionが反映されてから、
        // 次のFixedUpdateStateで復帰を判断する。
    }

    private bool TryGetWallDistance(
        float moveDistance,
        out float allowedDistance)
    {
        allowedDistance = moveDistance;
        bool hitWall = false;

        RaycastHit[] hits = playerRigidbody.SweepTestAll(
            knockbackDirection,
            moveDistance + wallPadding,
            QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            // 床タイルの縁を壁と判定して、
            // 吹き飛ばしが途中で止まるのを防ぐ。
            if (hit.collider.GetComponentInParent<FieldTile>() != null)
            {
                continue;
            }

            int hitLayer = hit.collider.gameObject.layer;

            if ((wallLayers.value & (1 << hitLayer)) == 0)
            {
                continue;
            }

            // 進行方向を遮る面だけを壁として扱う。
            if (Vector3.Dot(hit.normal, knockbackDirection) >= 0f)
            {
                continue;
            }

            float distanceToWall = Mathf.Max(
                0f,
                hit.distance - wallPadding);

            allowedDistance = Mathf.Min(
                allowedDistance,
                distanceToWall);

           
            hitWall = true;
        }

        return hitWall;
    }

    public override void ExitState()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
        }

        knockbackDirection = Vector3.zero;
        knockbackSpeed = 0f;
        remainingDistance = 0f;
        stunEndTime = 0f;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}