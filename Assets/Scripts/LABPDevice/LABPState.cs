using UnityEngine;

public class LABPState : ScriptableObject
{
    protected LABPDevice owner = null;
    protected Animator animator = null;
    protected Player player = null;

    public virtual void EnterState(LABPDevice owner, Animator animator, Player player) { if (!this.owner) 
        { this.owner = owner; } if (!this.animator) { this.animator = animator; } if (!this.player) { this.player = player; } }
    public virtual void ExitState() { }
    public virtual void UpdateState() { }
    public virtual void FixedUpdateState() { }

}