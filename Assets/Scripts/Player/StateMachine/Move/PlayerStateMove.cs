using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStateMove", menuName = "PlayerState/Move")]
public class PlayerStateMove : PlayerState
{
    [SerializeField] private float moveSpeed = 5f;

    public override void EnterState(Player owner, PlayerInputReader input, Animator animator)
    {
        base.EnterState(owner, input, animator);

        animator?.SetBool("bRun", true);
        owner.StartRunEffect();
    }

    public override void UpdateState()
    {
        base.UpdateState();

        if (input.MoveValue == Vector2.zero)
        {
            owner.ChangeState("idle");
        }
    }

    public override void FixedUpdateState()
    {
        base.FixedUpdateState();

        var rb = owner.GetComponent<Rigidbody>();
        var moveDirection = new Vector3(input.MoveValue.x, 0.0f, input.MoveValue.y);

        moveDirection.Normalize();
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);;

        // Rotate the player to face the movement direction
        if (moveDirection.magnitude != owner.moveDeadzone)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
        }
    }

    public override void ExitState()
    {
        base.ExitState();
        var rb = owner.GetComponent<Rigidbody>();

        animator?.SetBool("bRun", false);

        owner.StopRunEffect();

        rb.linearVelocity = Vector3.zero;
    }
}
