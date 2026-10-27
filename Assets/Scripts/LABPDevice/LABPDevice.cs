using UnityEngine;

public class LABPDevice : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private LABPStateList stateList;
    [SerializeField] private string initializeStateKey = "chase";
    [SerializeField] private Animator animator;
    [SerializeField] private Player player;

    private LABPState currentState;

    public bool IsPlacing => currentState is LABP_Place;

    private void Awake()
    {
        if(!animator)
        {
            animator = GetComponent<Animator>();
            if (!animator)
            {
                Debug.LogError("LABPDevice: Animator is not found");
                return;
            }
        }
        if(!player)
        {
            Debug.LogError("LABPDevice: Player is not found");
            return;
        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(player.GetLABPDevice() == null || player.GetLABPDevice() != this)
        {
            player.SetLABPDevice(this);
        }

        stateList.CreateRunTimeCopies();
        ChangeState(initializeStateKey);
    }

    // Update is called once per frame
    void Update()
    {
        currentState?.UpdateState();
    }

    private void FixedUpdate()
    {
        currentState?.FixedUpdateState();
    }

    public void ChangeState(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            Debug.LogError("LABPDevice: State key is null or empty");
            return;
        }
        if (!stateList.TryGetRuntimeState(key, out LABPState nextState))
        {
            Debug.LogError($"LABPDevice: State not found ({key})");
            return;
        }
        if (currentState == nextState)
        {
            return;
        }
        currentState?.ExitState();
        currentState = nextState;
        currentState.EnterState(this, animator, player);
    }

}