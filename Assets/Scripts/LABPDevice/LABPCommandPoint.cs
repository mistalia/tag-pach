using UnityEngine;

public class LABP_CommandPoint : MonoBehaviour
{
    [Header("Effect Objects")]
    [SerializeField] private GameObject markerEffect;
    [SerializeField] private Transform pinFront;

    private ParticleSystem[] markerParticles;
    private ParticleSystem[] pinParticles;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    [Header("Pin Animation")]
    [SerializeField] private float floatHeight = 0.2f;
    [SerializeField] private float floatSpeed = 3.0f;
    [SerializeField] private float displayDuration = 3.0f;

    [Header("Pin Transform")]
    [SerializeField]
    private Vector3 pinOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Field")]
    private Transform fieldRoot;
    private Bounds fieldBounds;
    private bool hasFieldBounds;

    private Vector3 startPosition;
    private Vector3 pinInitialPosition;

    private bool isSelecting;
    private bool isConfirmed;
    private float confirmedTimer;

    public bool IsSelecting => isSelecting;
    public bool IsConfirmed => isConfirmed;

    private void Awake()
    {
        // マーカーの取得
        if (markerEffect == null)
        {
            Transform marker =
                transform.Find("eff_gasha_pin_marker");

            if (marker != null)
            {
                markerEffect = marker.gameObject;
            }
        }

        // ピンと子エフェクトを取得
        if (pinFront != null)
        {
            pinInitialPosition = pinOffset;
            pinFront.localPosition = pinInitialPosition;

            pinParticles =
                pinFront.GetComponentsInChildren<ParticleSystem>(true);
        }
        else
        {
            Debug.LogError(
                "gasha_pin is not found", this);
        }

        SetupRenderOrder();

        Hide();
    }

    private void Update()
    {
        if (!isConfirmed)
        {
            return;
        }

        UpdatePinVisual();

        confirmedTimer -= Time.deltaTime;

        if (confirmedTimer <= 0.0f)
        {
            Hide();
        }
    }

    private void FindFieldRoot()
    {
        if (fieldRoot != null)
        {
            return;
        }

        FieldManager manager = FieldManager.Instance;

        if (manager == null)
        {
            manager = FindAnyObjectByType<FieldManager>();
        }

        if (manager == null)
        {
            Debug.LogError(
                "FieldManagerが見つかりません。", this);

            return;
        }

        fieldRoot = manager.transform.Find("Generated Fields");

        if (fieldRoot == null)
        {
            Debug.LogError(
                "Generated Fieldsが見つかりません。", this);
        }
    }

    private void UpdateFieldBounds()
    {
        hasFieldBounds = false;

        FindFieldRoot();

        if (fieldRoot == null)
        {
            return;
        }

        FieldTile[] tiles =
            fieldRoot.GetComponentsInChildren<FieldTile>();

        foreach (FieldTile tile in tiles)
        {
            if (tile == null)
            {
                continue;
            }

            Collider col = tile.GetComponent<Collider>();

            if (col == null)
            {
                continue;
            }

            if (!hasFieldBounds)
            {
                fieldBounds = col.bounds;
                hasFieldBounds = true;
            }
            else
            {
                fieldBounds.Encapsulate(col.bounds);
            }
        }
    }

    public void BeginSelection(Vector3 devicePosition)
    {
        UpdateFieldBounds();

        startPosition = devicePosition + spawnOffset;

        if (hasFieldBounds)
        {
            startPosition.x = Mathf.Clamp(
                startPosition.x,
                fieldBounds.min.x,
                fieldBounds.max.x);

            startPosition.z = Mathf.Clamp(
                startPosition.z,
                fieldBounds.min.z,
                fieldBounds.max.z);

            startPosition.y = fieldBounds.max.y;
        }

        transform.position = startPosition;
        transform.rotation = Quaternion.identity;

        isSelecting = true;
        isConfirmed = false;
        confirmedTimer = 0.0f;

        gameObject.SetActive(true);

        // マーカー表示・再生
        if (markerEffect != null)
        {
            markerEffect.SetActive(true);

            PlayParticles(markerParticles);
        }

        // 確定前はピンを非表示
        if (pinFront != null)
        {
            StopParticles(pinParticles);

            pinFront.localPosition = pinInitialPosition;
            pinFront.gameObject.SetActive(false);
        }
    }

    public void MoveMarker(Vector2 input)
    {
        if (!isSelecting)
        {
            return;
        }

        Vector3 direction =
            new Vector3(input.x, 0.0f, input.y);

        direction =
            Vector3.ClampMagnitude(direction, 1.0f);

        Vector3 nextPosition =
            transform.position +
            direction * moveSpeed * Time.deltaTime;

        if (hasFieldBounds)
        {
            nextPosition.x = Mathf.Clamp(
                nextPosition.x,
                fieldBounds.min.x,
                fieldBounds.max.x);

            nextPosition.z = Mathf.Clamp(
                nextPosition.z,
                fieldBounds.min.z,
                fieldBounds.max.z);
        }

        transform.position = nextPosition;
    }

    public void Confirm()
    {
        if (!isSelecting)
        {
            return;
        }

        isSelecting = false;
        isConfirmed = true;
        confirmedTimer = displayDuration;

        // 確定後はピンと子エフェクトを表示
        if (pinFront != null)
        {
            pinFront.gameObject.SetActive(true);

            UpdatePinVisual();

            PlayParticles(pinParticles);
        }
    }

    public void Hide()
    {
        isSelecting = false;
        isConfirmed = false;
        confirmedTimer = 0.0f;

        // マーカー停止
        StopParticles(markerParticles);

        if (markerEffect != null)
        {
            markerEffect.SetActive(false);
        }

        // ピン停止
        StopParticles(pinParticles);

        if (pinFront != null)
        {
            pinFront.localPosition = pinInitialPosition;
            pinFront.gameObject.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    public Vector3 GetTargetPosition()
    {
        return transform.position;
    }

    // ========================================
    // ParticleSystem制御
    // ========================================

    private void PlayParticles(ParticleSystem[] particles)
    {
        if (particles == null)
        {
            return;
        }

        foreach (ParticleSystem particle in particles)
        {
            if (particle == null)
            {
                continue;
            }

            particle.Stop(
                false,
                ParticleSystemStopBehavior.StopEmittingAndClear);

            particle.Play(false);
        }
    }

    private void StopParticles(ParticleSystem[] particles)
    {
        if (particles == null)
        {
            return;
        }

        foreach (ParticleSystem particle in particles)
        {
            if (particle == null)
            {
                continue;
            }

            particle.Stop(
                false,
                ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // ========================================
    // ピンの表示位置・アニメーション
    // ========================================

    private void UpdatePinVisual()
    {
        if (pinFront == null)
        {
            return;
        }

        // 上下アニメーション
        float offset =
            Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        pinFront.localPosition =
            pinInitialPosition + Vector3.up * offset;
    }

    private void SetupRenderOrder()
    {
        // 地面マーカー
        if (markerEffect != null)
        {
            Renderer[] renderers =
                markerEffect.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                renderer.sortingOrder = 0;
            }
        }

        // ピンと背面エフェクト
        if (pinFront != null)
        {
            Renderer[] renderers =
                pinFront.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                if (renderer is ParticleSystemRenderer)
                {
                    // ピンの背面エフェクト
                    renderer.sortingOrder = 10;
                }
                else
                {
                    // ピン本体
                    renderer.sortingOrder = 20;
                }
            }
        }
    }
}
