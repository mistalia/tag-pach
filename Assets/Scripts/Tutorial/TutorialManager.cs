using UnityEngine;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Ref")]
    [SerializeField] private Player player_mag;
    public Player GetPlayerMag() => player_mag;
    [SerializeField]private PlayerInputReader pInput_mag;
    public PlayerInputReader GetPlayerInputMag() => pInput_mag;
    [SerializeField] private Player player_osh;
    public Player GetPlayerOsh() => player_osh;
    [SerializeField]private PlayerInputReader pInput_osh;
    public PlayerInputReader GetPlayerInputOsh() => pInput_osh;

    [Header("Tutorial Setting")]
    [SerializeField] private TutorialStateList stateList;
    [SerializeField] private string initialStateKey;

    private TutorialState currentState;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }

        Instance = this;

        if(player_mag == null || player_osh == null)
        {
            Debug.LogError("TutorialManager: Player references are not assigned.");
        }

        if(!pInput_mag)
        {
            pInput_mag = player_mag.GetComponent<PlayerInputReader>();
        }
        if(!pInput_osh)
        {
            pInput_osh = player_osh.GetComponent<PlayerInputReader>();
        }

        if(!pInput_mag || !pInput_osh)
        {
            Debug.LogError("TutorialManager: PlayerInputReader references are not assigned.");
        }

        stateList.CreateRunTimeCopies();

    }

    private void OnDestroy()
    {
        stateList.DestroyRunTimeCopies();
    }

    void Start()
    {
        ChangeState(initialStateKey);
    }

    void Update()
    {
        currentState?.UpdateState();
    }
    
    void FixedUpdate()
    {
        currentState?.FixedUpdateState();
    }

    void LateUpdate()
    {
        currentState?.LateUpdateState();
    }

    public void ChangeState(string nextStateKey)
    {
        if(string.IsNullOrWhiteSpace(nextStateKey))
        {
            Debug.LogError("TutorialManager: nextStateKey is null or empty.");
            return;
        }
        if(!stateList.TryGetRuntimeState(nextStateKey, out var nextState))
        {
            Debug.LogError($"TutorialManager: nextStateKey '{nextStateKey}' is not found.");
            return;
        }
        
        if (currentState == nextState)
        {
            return;
        }

        currentState?.ExitState();
        currentState = nextState;
        currentState.EnterState(this);
        
    }

    public void EnablePlayer()
    {
        player_mag.enabled = true;
        player_osh.enabled = true;
    }

    public void DisablePlayer()
    {
        player_mag.enabled = false;
        player_osh.enabled = false;
    }

    public void MissionClear()
    {
        currentState?.SetClear(true);
    }
}
