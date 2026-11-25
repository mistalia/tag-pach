using System.Collections;
using UnityEngine;

public class FieldCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float cameraMoveSpeed = 1f;
    [SerializeField] private float cameraZoomSpeed = 1f;
    [SerializeField] private float cameraFOVMin = 50f;
    [SerializeField] private float cameraFOVMax = 60f;
    [SerializeField] private float chaseBoxInsideRatio = 0.8f;

    public static FieldCamera Instance { get; private set; }

    private Camera cam;
    private Vector3 defaultPosition;
    private Vector3 chaseBoxSize = new Vector3(1f, 1f, 1f);
    private Vector3 chaseBoxOffset = new Vector3(0f, 0f, 0f);
    private Vector3 fieldWorldSize = new Vector3(1f, 1f, 1f);
    private Vector3 tgtPos;
    private Vector3 tgtSize;

    private Vector3 camToBoxCoordinate;
    private Vector3 defBoxOffset;
    private Vector3 defBoxSize;

    private Player player1 = null;
    private Player player2 = null;

    void Start()
    {
        if(Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        cam = this.GetComponent<Camera>();
        defaultPosition = transform.position;
        defBoxOffset = chaseBoxOffset;
        defBoxSize = chaseBoxSize;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (TurnManager.Instance)
        {
            player1 = TurnManager.Instance.GetPlayer1();
            player2 = TurnManager.Instance.GetPlayer2();
        }
        else if(TutorialManager.Instance)
        {
            player1 = TutorialManager.Instance.GetPlayerMag();
            player2 = TutorialManager.Instance.GetPlayerOsh();
        }

        if(player1 == null || player2 == null)
        {
            return;
        }

        Bounds defBox = new Bounds(defBoxOffset, defBoxSize);
        Bounds chaseBox = new Bounds(chaseBoxOffset, chaseBoxSize);

        bool player1InDefBox = player1 != null && defBox.Contains(player1.transform.position);
        bool player2InDefBox = player2 != null && defBox.Contains(player2.transform.position);

        bool player1InChaseBox = player1 != null && chaseBox.Contains(player1.transform.position);
        bool player2InChaseBox = player2 != null && chaseBox.Contains(player2.transform.position);

        Vector2 player1Pos = new Vector2(player1.transform.position.x, player1.transform.position.z);
        Vector2 player2Pos = new Vector2(player2.transform.position.x, player2.transform.position.z);
        Vector2 boxPos = new Vector2(chaseBoxOffset.x, chaseBoxOffset.z);

        if (player1InDefBox && player2InDefBox)
        {//どちらも内側にいる
            tgtSize = defBoxSize;
            tgtPos = defaultPosition;
        }
        else
        {//どちらも居ない
            // どちらも居ない場合は、両者の中間地点にカメラを移動させる
            tgtPos = new Vector3((player1Pos.x + player2Pos.x) / 2.0f, 0.0f, (player1Pos.y + player2Pos.y) / 2.0f) - camToBoxCoordinate;
        }

        // カメラのズームを調整する
        {
            float distanceX = Mathf.Abs(player1Pos.x - player2Pos.x);
            float distanceY = Mathf.Abs(player1Pos.y - player2Pos.y);

            if (distanceX > chaseBoxSize.x || distanceY > chaseBoxSize.z)
            {
                tgtSize = chaseBoxSize + new Vector3(cameraZoomSpeed, 0.0f, cameraZoomSpeed);
            }
            else if (distanceX < chaseBoxSize.x * chaseBoxInsideRatio && distanceY < chaseBoxSize.z * chaseBoxInsideRatio)
            {
                tgtSize = chaseBoxSize - new Vector3(cameraZoomSpeed, 0.0f, cameraZoomSpeed);
            }
            else
            {
                tgtSize = chaseBoxSize;
            }
        }

        tgtSize = new Vector3(Mathf.Max(tgtSize.x, defBoxSize.x), defBoxSize.y, Mathf.Max(tgtSize.z, defBoxSize.z));

        chaseBoxSize = Vector3.Lerp(chaseBoxSize, tgtSize, Time.deltaTime * cameraZoomSpeed);

        // カメラの位置を制限する
        tgtPos.x = Mathf.Clamp(tgtPos.x, fieldWorldSize.x * 0.25f, fieldWorldSize.x * 0.75f);
        tgtPos.z = Mathf.Clamp(tgtPos.z, -fieldWorldSize.z * 0.5f, fieldWorldSize.z * 0.75f);

        this.transform.position = Vector3.Lerp(this.transform.position, tgtPos, Time.deltaTime * cameraMoveSpeed);
        RepairsBoxOffset();

        SetFieldOfView();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(defBoxOffset, defBoxSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(chaseBoxOffset, chaseBoxSize);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(chaseBoxOffset, chaseBoxSize * chaseBoxInsideRatio);

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(defBoxOffset, fieldWorldSize);
    }

    public void SetChaseBox(Vector3 Offset,Vector3 Size, Vector3 FieldWorldSize)
    {
        chaseBoxOffset = Offset;
        defBoxOffset = Offset;
        chaseBoxSize = Size;
        chaseBoxSize.y = 4.0f;
        defBoxSize = Size;
        fieldWorldSize = FieldWorldSize;
        fieldWorldSize.y = 4.0f;
        camToBoxCoordinate = chaseBoxOffset - defaultPosition;
    }

    public void RepairsBoxOffset()
    {
        chaseBoxOffset = this.transform.position + camToBoxCoordinate;
    }

    //視野角の調整
    public void SetFieldOfView()
    {
        float ratioX = Mathf.InverseLerp(defBoxSize.x, fieldWorldSize.x, chaseBoxSize.x);
        float ratioZ = Mathf.InverseLerp(defBoxSize.z, fieldWorldSize.z, chaseBoxSize.z);

        float ratio = Mathf.Max(ratioX, ratioZ);

        cam.fieldOfView = Mathf.Lerp(cameraFOVMin, cameraFOVMax, ratio);
    }

    public void CameraShake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }
}