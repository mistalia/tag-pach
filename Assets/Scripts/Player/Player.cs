using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private PlayerStateList stateList;
    [SerializeField] private string initializeStatekey = "idle";
    [SerializeField] private LayerMask fieldLayer; // フィールドのレイヤーマスク

    [Header("Player TeamColor")]
    [SerializeField] private Color teamColor = Color.red;
    public Color TeamColor => teamColor;

    //---------------------------------------------------------------
    [Header("Reposition Visual")]
    private GameObject normalMesh;
    private GameObject repositionMesh;

    private Renderer normalMeshRenderer;
    private Renderer repositionMeshRenderer;

    //---------------------------------------------------------------

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

        //---------------------------------------------------------------
        // NormalMeshを取得
        Transform normalMeshTransform = transform.Find("NormalMesh");

        if (normalMeshTransform != null)
        {
            normalMesh = normalMeshTransform.gameObject;
        }
        else
        {
            Debug.LogError("NormalMesh is not found");
            return;
        }

        // RepositionPointerを取得
        Transform repositionMeshTransform = transform.Find("RepositionMesh");

        if (repositionMeshTransform != null)
        {
            repositionMesh = repositionMeshTransform.gameObject;
        }
        else
        {
            Debug.LogError("RepositionPointer is not found");
            return;
        }

        normalMeshRenderer = normalMesh.GetComponent<Renderer>();

        repositionMeshRenderer = repositionMesh.GetComponent<Renderer>();

        if (!normalMeshRenderer || !repositionMeshRenderer)
        {
            Debug.LogError("NormalMesh または RepositionMesh にRendererがありません。");

            return;
        }
        //---------------------------------------------------------------

        // ステータスのコピーインスタンスを生成
        stateList.CreateRunTimeCopies();

        canPlaceStone = true;
    }

    void Start()
    {
        //---------------------------------------------------------------
        TurnManager.Instance.RegisterPlayer(this);
        repositionMesh.SetActive(false);
        //---------------------------------------------------------------

        ChangeState(initializeStatekey);
    }

    // Update is called once per frame
    void Update()
    {
        currentState?.UpdateState();

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

    public bool IsRepositioning =>
    currentState is PlayerStateReposition ||
    (TurnManager.Instance != null &&
     TurnManager.Instance.GetCurrentStateType() == TurnStateType.Reposition);

    public void AddKnockback(
        Vector3 direction,
        float distance,
        float speed,
        float stunDuration)
    {
        if (!isActiveAndEnabled || IsRepositioning)
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
        // 命中情報が届いていたら、操作より優先して吹き飛ばされる状態へ入る。
        if (IsRepositioning || currentState is PlayerStateSmashed)
        {
            return;
        }

        if (input.Attack.Pressed &&
            stateList.TryGetRuntimeState("smash", out var attackState) &&
            attackState is PlayerStateSmash smashState &&
            smashState.CanUse(this))
        {
            ChangeState("smash");
            return;
        }

        // どの状態からでも特定のイベントで遷移するトランジションはここに記述する
        if (input.Place.Pressed && TurnManager.Instance.IsAttackPlayer(this) && canPlaceStone) 
        {
            ChangeState("place");
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


    //---------------------------------------------------------------
    public void StartReposition()
    {
        ChangeState("reposition");
    }

    public void EndReposition()
    {
        ChangeState("idle");
    }

    public void EnterRepositionVisual()
    {
        normalMesh.SetActive(false);
        repositionMesh.SetActive(true);
    }

    public void ExitRepositionVisual()
    {
        repositionMesh.SetActive(false);
        normalMesh.SetActive(true);
    }

    public void SetRepositionColor(Color color)
    {
        normalMeshRenderer.material.color = color;
        repositionMeshRenderer.material.color = color;
    }

    public void StartResult()
    {
        ChangeState("result");
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

        // Reposition用Visualになっている可能性を考慮
        repositionMesh.SetActive(false);
        normalMesh.SetActive(true);
    }

    //---------------------------------------------------------------
}
