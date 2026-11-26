using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Spawn Settings")]
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    [Header("Fall Respawn Settings")]
    [SerializeField] private float fallHeight = -10.0f;
    [SerializeField] private float respawnDelay = 2.0f;

    private float fallTimer;
    private bool isFalling;

    [Header("Reference")]
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerStateList stateList;
    [SerializeField] private string initializeStatekey = "idle";
    [SerializeField] private LayerMask fieldLayer; // フィールドのレイヤーマスク
    [SerializeField] private LABPDevice labpDevice;
    [SerializeField] private GameObject rootDisableObj;
    [SerializeField] private ParticleSystem runEffect;

    public readonly float moveDeadzone = 0.3f; // 移動入力のデッドゾーン

    public void SetLABPDevice(LABPDevice device)
    {
        labpDevice = device;
    }
    public LABPDevice GetLABPDevice()
    {
        return labpDevice;
    }

    [Header("Player TeamColor")]
    [SerializeField] private Color teamColor = Color.red;
    public Color TeamColor => teamColor;

    private bool canPlaceStone = true;
    public void SetCanPlaceStone(bool value)
    {
        canPlaceStone = value;
    }

    private bool smashEnable = true;
    public void SetSmashEnable(bool value)
    {
        smashEnable = value;
    }

    private bool forcePlaceEnable = true;
    public void SetForcePlaceEnable(bool value)
    {
        forcePlaceEnable = value;
    }

    private PlayerState currentState;
    public FieldTile OnStandingPiece { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (!input)
        {
            input = GetComponent<PlayerInputReader>();
            if (!input)
            {
                Debug.LogError("PlayerInputReader is not found");
                return;
            }
        }

        if(animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator is not found");
                return;
            }
        }

        // ステータスのコピーインスタンスを生成
        stateList.CreateRunTimeCopies();

        SetCanPlaceStone(true);

        // 初期位置・初期回転を保存
        InitializeSpawnPoint();
    }

    void Start()
    {
        //---------------------------------------------------------------
        TurnManager.Instance?.RegisterPlayer(this);
        //---------------------------------------------------------------

        ChangeState(initializeStatekey);
        StopRunEffect();
    }

    // Update is called once per frame
    void Update()
    {
        // 落下・復帰処理
        if (UpdateFallRespawn())
        {
            return;
        }

        currentState?.UpdateState();

        // Result中の入力処理
        if (UpdateResultInput())
        {
            return;
        }

        AnyStateTransition();
    }

    void FixedUpdate()
    {
        currentState?.FixedUpdateState();
    }

    private void LateUpdate()
    {
        if(TryGetFieldTileBelow(-0.5f, 1.0f, fieldLayer))
        {
            OnStandingPiece.LightUpTile();
        }
    }

    void OnDestroy()
    {
        stateList.DestroyRunTimeCopies();
    }

    // Read-only access to this player's runtime state for skill UI.
    public bool TryGetRuntimeState(string key, out PlayerState state)
    {
        state = null;
        return stateList != null && stateList.TryGetRuntimeState(key, out state);
    }

    public void ChangeState(string newStateKey)
    {
        if(!stateList.TryGetRuntimeState(newStateKey, out var newState))
        {
            Debug.LogError($"State not found: {newStateKey}");
            return;
        }

        // CanUseが必要なStateだけチェック
        if (newState is PlayerStateForcePlace forcePlace &&
            !forcePlace.CanUse(this))
        {
            Debug.LogError("ForcePlace state CanUse is false ");
            return;
        }

        if (newState is PlayerStateSmash smash &&
            !smash.CanUse(this))
        {
            Debug.LogError("Smash state CanUse is false ");
            return;
        }

        if (currentState != null)
        {
            currentState.ExitState();
        }

        currentState = newState;
        currentState.EnterState(this, input, animator);
    }

    public void AddKnockback(
        Vector3 direction,
        float distance,
        float speed,
        float stunDuration)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        // スタン中の追撃では、最初の命中情報を上書きしない。
        if (currentState is PlayerStateSmashed currentSmashed &&
            currentSmashed.IsStunned)
        {
            return;
        }

        direction.y = 0f;
        float directionSqrMagnitude = direction.sqrMagnitude;

        if (!IsValidKnockbackValue(directionSqrMagnitude) ||
            !IsValidKnockbackValue(distance) ||
            !IsValidKnockbackValue(speed) ||
            !IsValidKnockbackValue(stunDuration) ||
            speed <= 0f ||
            (distance > 0f && directionSqrMagnitude < 0.0001f))
        {
            Debug.LogWarning(
                "ノックバック情報が不正なため、被弾を受け付けません。",
                this);
            return;
        }

        if (stateList == null ||
            !stateList.TryGetRuntimeState("knockback", out var state) ||
            state is not PlayerStateSmashed smashedState)
        {
            Debug.LogError(
                "knockbackにPlayerStateSmashedが登録されていません。",
                this);
            return;
        }

        if (!TryGetComponent<Rigidbody>(out _))
        {
            Debug.LogError(
                "ノックバックに必要なRigidbodyがありません。",
                this);
            return;
        }

        ChangeState("knockback");

        // EnterStateで初期化した後に、今回の被弾情報を渡す。
        if (currentState == smashedState)
        {
            smashedState.Setup(direction, distance, speed, stunDuration);
        }
    }

    private static bool IsValidKnockbackValue(float value)
    {
        return !float.IsNaN(value) &&
               !float.IsInfinity(value) &&
               value >= 0f;
    }

    private void AnyStateTransition()
    {
        // 再配置中・被弾中・強制配置スキル中は入力で割り込まない。
        if (IsResult() || IsBattleStart() ||
            currentState is PlayerStateSmashed ||
            currentState is PlayerStateForcePlace)
        {
            return;
        }

        if (input == null)
        {
            return;
        }

        if (input.ForcePlace != null &&
            input.ForcePlace.Pressed && 
            forcePlaceEnable)
        {
            ChangeState("forcePlace");
            return;
        }

        if (input.Attack != null &&
            input.Attack.Pressed &&
            smashEnable)
        {
            ChangeState("smash");
            return;
        }

        if (input.Place != null 
            && input.Place.Pressed 
            && canPlaceStone)
        {
            ChangeState("place");
        }

  
        UpdatePointCommandInput();

    }

    public bool TryGetFieldTileBelow(float rayStartHeight, float rayDistance, LayerMask fieldLayer)
    {
        Vector3 origin = transform.position + Vector3.up * rayStartHeight;

        FieldTile FieldTile= null;

        if (Physics.Raycast(origin,Vector3.down,out RaycastHit hit, rayDistance, fieldLayer,QueryTriggerInteraction.Ignore))
        {
            FieldTile = hit.collider.GetComponent<FieldTile>();
            if(FieldTile != null)
            {
                OnStandingPiece?.ResetTileColor();
                OnStandingPiece = FieldTile;
                //OnStandingPiece.LightUpTile();

                Debug.DrawLine(origin, hit.point, Color.red, 1f);
                return true;
            }
        }


        if (OnStandingPiece != null)
        {
            OnStandingPiece.ResetTileColor();
            OnStandingPiece = null;
        }
        Debug.DrawLine(origin, origin + Vector3.down * rayDistance, Color.green, 1f);
        return false;
    }

    public bool TryForcePlaceStone()
    {
        TurnManager turn = TurnManager.Instance;

        if (!isActiveAndEnabled ||
            !canPlaceStone ||
            currentState is PlayerStateSmashed ||
            turn == null ||
            !turn.IsBattleStarted() ||
            turn.IsBattlePaused() ||
            turn.GetCurrentStateType() != TurnStateType.Playing ||
            !turn.IsAttackPlayer(this) ||
            Time.timeScale <= 0f)
        {
            return false;
        }

        if (stateList == null ||
            !stateList.TryGetRuntimeState("place", out var state) ||
            state is not PlayerStatePlace placeState ||
            !placeState.CanPlaceAtFoot(this))
        {
            return false;
        }

        // 通常の配置と同じステートを使う。
        ChangeState("place");

        // 駒の生成はアニメーション後なので、開始できたか確認する。
        return labpDevice != null && labpDevice.IsPlacing;
    }


    //---------------------------------------------------------------
    public void StartResult()
    {
        // Result開始時は通常状態へ戻す
        ChangeState("idle");

        // 石の配置は禁止
        SetCanPlaceStone(false);
    }

    public bool IsResult()
    {
        return TurnManager.Instance != null &&
               TurnManager.Instance.GetCurrentStateType()
                   == TurnStateType.Result;
    }

    public bool IsBattleStart()
    {
        return TurnManager.Instance != null &&
               TurnManager.Instance.GetCurrentStateType()
                   == TurnStateType.BattleStart;
    }

    //プレイヤーリセット
    public void ResetBattle()
    {
        // �΂�u�����Ԃɖ߂�
        SetCanPlaceStone(true);

        Respawn();
    }

    // Result中の確認入力
    private bool UpdateResultInput()
    {
        if (!IsResult())
        {
            return false;
        }

        if (input != null &&
            input.Place != null &&
            input.Place.Pressed)
        {
            TurnManager.Instance.RequestEndResult(this);
        }

        return true;
    }

    // 初期位置・初期回転を保存
    private void InitializeSpawnPoint()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private bool UpdateFallRespawn()
    {
        // 落下中ではない場合
        if (!isFalling)
        {
            if (transform.position.y < fallHeight)
            {
                isFalling = true;
                fallTimer = respawnDelay;
            }
            else
            {
                return false;
            }
        }

        // 落下中は復帰時間をカウント
        fallTimer -= Time.deltaTime;

        if (fallTimer <= 0.0f)
        {
            Respawn();
        }

        // このフレームは通常操作を行わない
        return true;
    }

    public void Respawn()
    {
        // 落下状態を解除
        isFalling = false;
        fallTimer = 0.0f;

        // 現在のステートを終了して通常状態へ
        ChangeState("idle");

        // Rigidbodyの速度をリセット
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 初期位置・回転に戻す
        transform.SetPositionAndRotation(initialPosition, initialRotation);

        // 足元のTileをリセット
        if (OnStandingPiece != null)
        {
            OnStandingPiece.ResetTileColor();
            OnStandingPiece = null;
        }
    }

    // Deviceへの指示入力
    private void UpdatePointCommandInput()
    {
        if (labpDevice == null || input == null)
        {
            return;
        }

        // コマンドボタンで開閉
        if (input.Command.Pressed)
        {
            if (labpDevice.IsPointCommandOpen())
            {
                labpDevice.ClosePointCommand();
            }
            else
            {
                labpDevice.OpenPointCommand();
            }

            return;
        }

        if (!labpDevice.IsPointCommandOpen())
        {
            return;
        }

        // マーカーを移動
        labpDevice.SelectPointPosition(input.CommandMoveValue);

        // 確定
        if (input.Select.Pressed)
        {
            labpDevice.ConfirmPointCommand();
        }
    }

    //---------------------------------------------------------------

    public void StartRunEffect()
    {
        if (runEffect != null && !runEffect.isPlaying)
        {
            runEffect.Play();
        }
    }

    public void StopRunEffect()
    {
        if (runEffect != null && runEffect.isPlaying)
        {
            runEffect.Stop();
        }
    }
}
