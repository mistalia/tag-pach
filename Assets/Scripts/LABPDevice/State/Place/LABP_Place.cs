using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "LABP_Place", menuName = "LABP/State/Place")]
public class LABP_Place : LABPState
{
    private enum PlaceState
    {
        None,
        StartUp,
        Dropping,
        EndDrop
    }

    private PlaceState currentPlaceState = PlaceState.None;

    [Header("Settings")]
    [SerializeField] private float MoveDuration = 0.5f;
    [SerializeField] private GameObject stonePrefab; // 石のプレハブ

    private Vector3 startPosition = Vector3.zero;
    private Vector3 targetPosition = Vector3.zero;

    private FieldTile targetField = null;

    public override void EnterState(LABPDevice owner, Animator animator, Player player)
    {
        base.EnterState(owner, animator, player);

        currentPlaceState = PlaceState.None;

        if(player.OnStandingPiece != null)
        {
            targetField = player.OnStandingPiece;
            targetPosition = targetField.transform.position + Vector3.up * (owner.transform.localScale.y * 0.5f); 
            startPosition = targetPosition;
            startPosition.y += owner.transform.position.y;
        }
        else
        {
            Debug.LogWarning("Player is not standing on a valid field tile.");
            owner.ChangeState("chase");
        }

        if(stonePrefab == null)
            stonePrefab = Resources.Load<GameObject>("Prefabs/Stone_Komanoko");
        {
            if (stonePrefab == null)
            {
                Debug.LogError("Stone prefab is not assigned in LABP_Place.");
                owner.ChangeState("chase");
            }
        }
    }


    public override void UpdateState()
    {
        base.UpdateState();

    }

    public override void FixedUpdateState()
    {

        var inf = animator.GetCurrentAnimatorStateInfo(0);
        switch (currentPlaceState)
        {
            case PlaceState.None:

                if (Vector3.Distance(owner.transform.position, startPosition) > 0.1f)
                {
                    var movePosition = Vector3.Lerp(owner.transform.position, startPosition, Time.deltaTime / MoveDuration);
                    owner.transform.position = movePosition;
                    // Y180方向へラープさせる
                    owner.transform.rotation = Quaternion.Lerp(owner.transform.rotation, Quaternion.Euler(0f, 180f, 0f), Time.deltaTime / MoveDuration);

                }
                else
                {
                    owner.transform.position = startPosition;
                    currentPlaceState = PlaceState.StartUp;
                    animator.SetBool("bPlace", true);
                }
                break;

            case PlaceState.StartUp:

                if (inf.IsName("Place_Up"))
                {

                    float clamp = Mathf.Clamp01(inf.normalizedTime);
                    if (inf.normalizedTime >= 1.0f)
                    {
                        currentPlaceState = PlaceState.Dropping;
                    }
                }
                break;

            case PlaceState.Dropping:

                if (inf.IsName("Place_Down"))
                {
                    float clamp = Mathf.Clamp01(inf.normalizedTime);
                    var dropPosition = Vector3.Lerp(owner.transform.position, targetPosition, clamp);

                    Debug.Log($"DroppPos : {dropPosition}");
                    owner.transform.position = dropPosition;

                    if (clamp >= 1.0f)
                    {
                        owner.transform.position = targetPosition;
                        currentPlaceState = PlaceState.EndDrop;
                    }
                }
                break;
            
            case PlaceState.EndDrop:

                Stone stone = Instantiate(stonePrefab).GetComponent<Stone>();
                stone.SetOwner(player);
                targetField.PutStone(stone);
                //---------------------------------------------------------------
                if (TurnManager.Instance != null)
                    TurnManager.Instance.ConsumeTurn(player);
                //---------------------------------------------------------------


                owner.ChangeState("chase");
                break;
        }

    }

    public override void ExitState()
    {
        base.ExitState();
        animator.SetBool("bPlace", false);
    }

}