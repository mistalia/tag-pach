using UnityEngine;

public class LABPDevice : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private LABPStateList stateList;
    [SerializeField] private string initializeStateKey = "chase";
    [SerializeField] private Animator animator;
    [SerializeField] private Player player;
    [SerializeField] private ParticleSystem MoveEffect;
    [SerializeField] private string spawnEffectKey = "Spawn_";
    public string GetSpawnEffectKey() => spawnEffectKey;
    [SerializeField] private string placeEffectKey = "Place_";

    [SerializeField] private string deviceKey = "Gasha";
    public string GetPlaceEffectKey() => placeEffectKey;

    private LABPCommunication communication;

    private LABPCommandPointController pointController;

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

        //------------------------------------------------------------------- 
        communication = GetComponent<LABPCommunication>();
        
        if (communication == null)
        {
            Debug.LogError("LABPDevice: LABPCommunication is not found", this);
        }

        pointController = GetComponent<LABPCommandPointController>();

        if (pointController == null)
        {
            Debug.LogError("LABPDevice: LABPCommandPointController is not found", this);
        }
        //-------------------------------------------------------------------

        StopMoveEffect();
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

    private void OnDestroy()
    {
        stateList.DestroyRunTimeCopies();
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

    //-------------------------------------------------------------------
    // 指示UIを開く
    public void OpenCommand()
    {
        if (IsCommanding() || IsPlacing)
        {
            return;
        }

        communication?.OpenCommand();
    }

    // 方向を選択
    public void SelectCommandDirection(Vector2 direction)
    {
        communication?.SelectDirection(direction);
    }

    // 指示を確定
    public void ConfirmCommand()
    {
        communication?.ConfirmCommand();
    }

    // 指示UIを閉じる
    public void CloseCommand()
    {
        communication?.CloseCommand();
    }

    // 指示UIが開いているか
    public bool IsCommandOpen()
    {
        return communication != null && communication.IsCommandOpen;
    }

    public void ShowCommandMarker()
    {
        if (communication)
        {
            communication.ShowCommandMarker();
        }
    }

    public void HideCommandMarker()
    {
        if (communication)
        {
            communication.HideCommandMarker();
        }
    }

    public Vector3 GetCommandDirection()
    {
        if (communication == null)
        {
            return Vector3.zero;
        }

        return communication.GetConfirmedWorldDirection();
    }

    public bool IsCommanding()
    {
        return currentState is LABP_Command;
    }


    // =====================================
    // 新しい位置指定コマンド
    // =====================================

    public void OpenPointCommand()
    {
        if (IsCommanding() || IsPlacing ||
            IsCommandOpen())
        {
            return;
        }

        pointController?.OpenCommand();
    }

    public void SelectPointPosition(Vector2 input)
    {
        pointController?.SelectPosition(input);
    }

    public void ConfirmPointCommand()
    {
        pointController?.ConfirmCommand();
    }

    public void ClosePointCommand()
    {
        pointController?.CloseCommand();
    }

    public bool IsPointCommandOpen()
    {
        return pointController != null &&
               pointController.IsCommandOpen;
    }

    public bool HasConfirmedPoint()
    {
        return pointController != null &&
               pointController.HasConfirmedPosition();
    }

    public Vector3 GetCommandPointPosition()
    {
        return pointController != null
            ? pointController.GetTargetPosition()
            : Vector3.zero;
    }

    public Player GetPlayer()
    {
        return player;
    }
    //-------------------------------------------------------------------

    public void PlayMoveEffect()
    {
        if (MoveEffect && !MoveEffect.isPlaying)
        {
            MoveEffect.Play();
        }
    }

    public void StopMoveEffect()
    {
        if (MoveEffect && MoveEffect.isPlaying)
        {
            MoveEffect.Stop();
        }
    }

    public void PlaySearch()
    {
        animator?.SetTrigger("tSearch");
        EffectManager.Instance?.PlayEffect("Search_" + deviceKey, player.transform.position);
    }
}