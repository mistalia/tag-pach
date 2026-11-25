using UnityEngine;

public class Dummy_Player : MonoBehaviour
{
    private Player attackedOwner = null;
    [SerializeField] private Animator animator;

    void Start()
    {
        attackedOwner = null;
        if(animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }


    public void HitAttack(Player owner)
    {
        attackedOwner = owner;
        animator.SetTrigger("tHit");
    }

    public Player GetAttackOwner()
    {
        return attackedOwner;
    }

}
