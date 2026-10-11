using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStateResult", menuName = "PlayerState/Result")]
public class PlayerStateResult : PlayerState
{
    [SerializeField] private float moveSpeed = 5f;

    public override void EnterState(Player owner, PlayerInputReader input)
    {
        base.EnterState(owner, input);

        owner.SetCanPlaceStone(false);
    }

    public override void UpdateState()
    {
        base.UpdateState();

        if(input.Place.Pressed)
        {
            TurnManager.Instance.RequestEndResult(owner);
        }
    }

    public override void FixedUpdateState()
    {
        base.FixedUpdateState();

        var rb = owner.GetComponent<Rigidbody>();
        var moveDirection = new Vector3(input.MoveValue.x, 0.0f, input.MoveValue.y);

        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    public override void ExitState()
    {
        base.ExitState();
        var rb = owner.GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero;
    }
}
