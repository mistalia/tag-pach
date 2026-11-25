using UnityEngine;

[CreateAssetMenu(fileName = "LABP_Chase", menuName = "LABPState/Chase")]
public class LABP_Chase : LABPState
{
    [Header("Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Vector3 chaseOffset = new Vector3(1.0f, 1.1f, -1.0f);

    public override void EnterState(LABPDevice device, Animator animator, Player player)
    {
        base.EnterState(device, animator, player);

        var target = player.transform; //追従する対象をプレイヤーに設定

        if (target == null)
        {
            //追従する対象がいないなら自爆　芸術は爆発
            Debug.LogError("Target is not assigned in LABP_Chase script.");
            Destroy(this);
            return;
        }

        //初期位置を設定
        owner.transform.position = target.position + target.forward * chaseOffset.z + target.right * chaseOffset.x + target.up * chaseOffset.y;
        EffectManager.Instance.PlayEffect(owner.GetSpawnEffectKey(), owner.transform.position, Quaternion.identity);
    }

    public override void FixedUpdateState()
    {
        base.FixedUpdateState();

        var target = player.transform; //追従する対象をプレイヤーに設定

        //プレイヤーの向いている方向を前として、追従する対象の位置を計算
        Vector3 desiredPosition = target.position + target.forward * chaseOffset.z + target.right * chaseOffset.x + target.up * chaseOffset.y;

        Vector3 previousPosition = owner.transform.position;
        //現在の位置から目標位置までイージング
        owner.transform.position = Vector3.Lerp(previousPosition, desiredPosition, speed * Time.fixedDeltaTime);

        //プレイヤーの向きに合わせて回転
        Quaternion desiredRotation = Quaternion.LookRotation((target.position + target.right * chaseOffset.x + target.up * chaseOffset.y) - owner.transform.position);
        owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, desiredRotation, rotationSpeed * Time.fixedDeltaTime);

        float actualSpeed = Vector3.Distance(previousPosition, owner.transform.position) / Time.fixedDeltaTime;

        if (actualSpeed > 0.5f)
        {
            owner.PlayMoveEffect();
        }
        else
        {
            owner.StopMoveEffect();
        }

    }

    public override void ExitState()
    {
        base.ExitState();
        owner.StopMoveEffect(); //移動エフェクトを停止
    }
}
