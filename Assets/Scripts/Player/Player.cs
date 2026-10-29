using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private PlayerStateList stateList;
    [SerializeField] private string initializeStatekey = "idle";
    [SerializeField] private LayerMask fieldLayer; // フィールドのレイヤーマスク
    [SerializeField] private LABPDevice labpDevice;
    [SerializeField] private GameObject rootDisableObj;

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

        // ステータスのコピーインスタンスを生成
        stateList.CreateRunTimeCopies();

        canPlaceStone = true;
    }

    void Start()
    {
        //---------------------------------------------------------------
        TurnManager.Instance.RegisterPlayer(this);
        //---------------------------------------------------------------

        ChangeState(initializeStatekey);
    }

    // Update is called once per frame
    void Update()
    {
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

    public void ChangeState(string newStateKey)
    {
        if(!stateList.TryGetRuntimeState(newStateKey, out var newState))
        {
            Debug.LogError($"State not found: {newStateKey}");
            return;
        }

        if (currentState != null)
        {
            currentState.ExitState();
        }
        currentState = newState;
        currentState.EnterState(this, input);
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
        if (IsResult() ||
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
            stateList.TryGetRuntimeState("forcePlace", out var skillState) &&
            skillState is PlayerStateForcePlace forcePlaceState &&
            forcePlaceState.CanUse(this))
        {
            ChangeState("forcePlace");
            return;
        }

        if (input.Attack != null &&
            input.Attack.Pressed &&
            stateList.TryGetRuntimeState("smash", out var attackState) &&
            attackState is PlayerStateSmash smashState &&
            smashState.CanUse(this))
        {
            ChangeState("smash");
            return;
        }

        if (input.Place != null &&
            input.Place.Pressed &&
            TurnManager.Instance != null &&
            TurnManager.Instance.IsAttackPlayer(this) &&
            canPlaceStone)
        {
            ChangeState("place");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryContactForcePlace(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryContactForcePlace(collision);
    }

    private void TryContactForcePlace(Collision collision)
    {
        if (!isActiveAndEnabled ||
            currentState is not PlayerStateForcePlace forcePlaceState)
        {
            return;
        }

        Player opponent =
            collision.collider.GetComponentInParent<Player>();

        if (!forcePlaceState.TryHit(opponent))
        {
            return;
        }

        // 命中後も自分が同じステートなら、通常状態へ戻す。
        if (currentState == forcePlaceState)
        {
            ChangeState("idle");
        }
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
                OnStandingPiece.LightUpTile();

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

    //プレイヤーリセット
    public void ResetBattle()
    {
        // 石を置ける状態に戻す
        canPlaceStone = true;

        // 現在立っているTileのハイライト解除
        if (OnStandingPiece != null)
        {
            OnStandingPiece.ResetTileColor();
            OnStandingPiece = null;
        }

        // Rigidbody停止
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 通常状態へ戻す
        ChangeState("idle");
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
    //---------------------------------------------------------------
}
