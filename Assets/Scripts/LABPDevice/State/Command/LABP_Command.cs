
using UnityEngine;

[CreateAssetMenu(fileName = "LABP_Command", menuName = "LABPState/Command")]
public class LABP_Command : LABPState
{
    [Header("ŽwŽ¦Ý’è")]
    [SerializeField, Min(0f)]
    private float commandDuration = 2.0f;

    private float commandTimer;

    public override void EnterState(LABPDevice owner,Animator animator,Player player)
    {
        base.EnterState(owner, animator, player);

        commandTimer = 0f;

        // Šm’è‚µ‚½ŽwŽ¦•ûŒü‚ðŽæ“¾
        Vector3 direction = owner.GetCommandDirection();
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.0001f)
        {
            owner.transform.rotation = Quaternion.LookRotation(direction.normalized);
        }

        owner.ShowCommandMarker();
    }

    public override void UpdateState()
    {
        base.UpdateState();

        commandTimer += Time.deltaTime;

        if (commandTimer >= commandDuration)
        {
            owner.ChangeState("chase");
        }
    }

    public override void FixedUpdateState()
    {

    }

    public override void ExitState()
    {
        owner.HideCommandMarker();

        commandTimer = 0f;

        base.ExitState();
    }
}

