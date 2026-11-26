using UnityEngine;
using UnityEngine.Tilemaps;

public class Warunoko_BombThrower : MonoBehaviour
{
    private BombSpawner spawner;
    private Bomb throwObject;
    private Vector3 landingPoint;
    private FieldTile targetTile;
    private float currentCountTime = 0.0f;
    private bool throwWaiting = false;
    private bool animated = false;

    [Header("Serring")]
    [SerializeField]private float throwWaitTime = 0.3f;
    [SerializeField]private float afterWaitTime = 0.4f;

    [Header("Reference")]
    [SerializeField] Animator animator;

    private void Awake()
    {
        if (!animator)
        {
            Debug.Log("ワルノコ: アニメーターがアタッチされていません");
        }
    }

    public void Initialize(BombSpawner spawner, Bomb throwObject, Vector3 landingPoint, FieldTile targetTile)
    {
        this.spawner = spawner;
        this.throwObject = throwObject;
        this.landingPoint = landingPoint;
        this.targetTile = targetTile;
        currentCountTime = 0.0f;
        animated = false;
        throwWaiting = true;
        currentCountTime = throwWaitTime;
        EffectManager.Instance?.PlayEffect("WarunokoSpawn", transform.position);
    }

    private void FixedUpdate()
    {
        if(throwWaiting)
        {
            if (currentCountTime >= 0.0f)
            {
                currentCountTime -= Time.fixedDeltaTime;
            }
            else
            {
                StartThrow();
                throwWaiting = false;
            }
        }
        if (animated)
        {
            if (currentCountTime >= 0.0f)
            {
                currentCountTime -= Time.fixedDeltaTime;
            }
            else
            {
                //ここで削除イベント
                EffectManager.Instance?.PlayEffect("WarunokoSpawn", transform.position);
                Destroy(this.gameObject);
                animated = false;
            }
        }
    }

    private void StartThrow()
    {
        animator?.SetTrigger("tThrow");
        CreateBomb();
    }

    private void CreateBomb()
    {
        throwObject.transform.position = transform.position + (Vector3.up * 1.5f);
        spawner.RegisterBomb(throwObject, targetTile);
        spawner.StartBomb(throwObject, landingPoint);
        animated = true;
        currentCountTime = afterWaitTime;
    }
}
