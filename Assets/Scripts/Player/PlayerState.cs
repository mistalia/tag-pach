using UnityEngine;

public abstract class PlayerState : ScriptableObject
{
    protected Player owner = null;
    protected PlayerInputReader input = null;
    protected Animator animator = null;
    public virtual void EnterState(Player owner, PlayerInputReader input, Animator animator) { if (!this.owner) { this.owner = owner; } if (!this.input) { this.input = input; } if (!this.animator) { this.animator = animator; } }
    public virtual void ExitState() { }
    public virtual void UpdateState() { }
    public virtual void FixedUpdateState() { }
}
