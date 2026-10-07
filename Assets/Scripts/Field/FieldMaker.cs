using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class FieldMaker : MonoBehaviour
{

    [Header("Reference")]
    [SerializeField] private GameObject field;
    [SerializeField] private GameObject barrierBlock;
    [SerializeField] private FieldManager fieldManager;

    [Header("Field Setting")]
    [SerializeField] private Vector2Int fieldSize = new(10, 10);
    [SerializeField] private float fieldSpacing = 2.0f;
    [SerializeField, HideInInspector] private bool[] enabledCells = new bool[0];

    [Header("Generated Object")]
    [SerializeField] private Transform generatedRoot;

    private bool isGenerating = false;
    private bool rebuildQueued = false;

    private void OnValidate()
    {
        //if (Application.isPlaying)
        //{
        //    return;
        //}
        //
        //QueueGenerateField();
    }

    private void Start()
    {
        if(!Application.isPlaying)
        {
            return;
        }

        RegisterFieldManager();
        var cameraobject = Camera.main?.gameObject;
        float height;
        if (fieldSize.x > fieldSize.y)
        {
            height = fieldSize.x * 1.25f;
        }
        else
        {
            height = fieldSize.y * 1.5f;
        }
        Vector3 center = new Vector3(fieldSize.x - 1, 0.0f, fieldSize.y - 1);
        cameraobject.GetComponent<FieldCamera>()?.SetChaseBox(center, new Vector3(fieldSize.x * 1.5f, 4.0f, fieldSize.y * 1.5f), new Vector3(fieldSize.x * fieldSpacing, 4.0f, fieldSize.y * fieldSpacing));
    }

    public void  RegisterFieldManager()
    {
        if(fieldManager == null)
        {
            return;
        }

        if(fieldManager == null || generatedRoot == null)
        {
            return;
        }

        fieldManager.BeginFieldSetup(fieldSize);

        FieldTile[] fieldTiles = generatedRoot.GetComponentsInChildren<FieldTile>();

        foreach (FieldTile fieldTile in fieldTiles)
        {
            Vector2Int coordinate = fieldTile.GetCoordinate();
            fieldManager.RegisterFieldTile(coordinate, fieldTile);
        }

    }

    [ContextMenu("Generate Field")]
    public void GenerateField()
    {
        if(fieldManager == null)
        {
            fieldManager = this.GetComponent<FieldManager>();
            if(fieldManager == null)
            {
                fieldManager = this.gameObject.AddComponent<FieldManager>();

                if(fieldManager == null)
                {
                    Debug.LogError("FieldManagerコンポーネントを追加できませんでした。");
                    return;
                }
            }
        }

        if(field == null || isGenerating)
        {
            return;
        }

        if (barrierBlock == null)
        {
            barrierBlock = Resources.Load<GameObject>("Prefubs/BarrierBlock");
        }

        isGenerating = true;

        try
        {
            CreateGenerateRoot();
            ClearField();

            // FieldManagerにフィールドサイズを通知
            fieldManager.BeginFieldSetup(fieldSize);

            for (int x = 0; x < fieldSize.x; x++)
            {
                for (int y = 0; y < fieldSize.y; y++)
                {
                    GameObject newField = Instantiate(field, generatedRoot);

                    newField.name = $"Field_{x}_{y}";
                    newField.transform.localPosition = new Vector3(x * 1.0f * fieldSpacing, 0.0f, y * 1.0f * fieldSpacing);
                    newField.transform.localRotation = Quaternion.identity;

                    // FieldManagerに登録
                    FieldTile FieldTile = newField.GetComponent<FieldTile>();
                    if (FieldTile != null)
                    {
                        FieldTile.SetCoodinate(x, y);
                        fieldManager.RegisterFieldTile(new Vector2Int(x, y), FieldTile);
                    }
                    else
                    {
                        Debug.LogError($"FieldTileコンポーネントが見つかりません: {newField.name}");
                    }
                }
            }

            // バリアブロックの生成
            if (barrierBlock != null)
            {
                //4面分
                {//左面
                    GameObject barrierInstans = Instantiate(barrierBlock, generatedRoot);

                    barrierInstans.name = $"Barrier_Left";
                    barrierInstans.transform.localPosition = new Vector3(-1.5f, 5.0f, (fieldSize.y - 1) * fieldSpacing * 0.5f );
                    barrierInstans.transform.localScale = new Vector3(1.0f, 10.0f, fieldSize.y * fieldSpacing);
                }

                {//奥面
                    GameObject barrierInstans = Instantiate(barrierBlock, generatedRoot);

                    barrierInstans.name = $"Barrier_Far";
                    barrierInstans.transform.localPosition = new Vector3((fieldSize.x - 1) * fieldSpacing * 0.5f,5.0f,fieldSize.y * fieldSpacing - 0.5f);
                    barrierInstans.transform.localScale = new Vector3(fieldSize.x * fieldSpacing, 10.0f, 1.0f);
                }

                {//右面
                    GameObject barrierInstans = Instantiate(barrierBlock, generatedRoot);

                    barrierInstans.name = $"Barrier_Right";
                    barrierInstans.transform.localPosition = new Vector3(fieldSize.x * fieldSpacing - 0.5f, 5.0f, (fieldSize.y - 1) * fieldSpacing * 0.5f);
                    barrierInstans.transform.localScale = new Vector3(1.0f, 10.0f, fieldSize.y * fieldSpacing);
                }

                {//奥面
                    GameObject barrierInstans = Instantiate(barrierBlock, generatedRoot);

                    barrierInstans.name = $"Barrier_Near";
                    barrierInstans.transform.localPosition = new Vector3((fieldSize.x - 1) * fieldSpacing * 0.5f, 5.0f, -1.5f);
                    barrierInstans.transform.localScale = new Vector3(fieldSize.x * fieldSpacing, 10.0f, 1.0f);
                }
            }

            // カメラの位置を調整
            var cameraobject = Camera.main?.gameObject;

            if (cameraobject != null)
            {
                float height;
                if(fieldSize.x > fieldSize.y)
                {
                    height = fieldSize.x * 1.25f;
                }
                else
                {
                    height = fieldSize.y * 1.5f;
                }
                cameraobject.transform.position = new Vector3((fieldSize.x - 1) * fieldSpacing / 2.0f, height, -height * 0.25f);

                Vector3 center = new Vector3(fieldSize.x - 1, 0.0f, fieldSize.y - 1);
                cameraobject.GetComponent<FieldCamera>()?.SetChaseBox(center, new Vector3(fieldSize.x * 1.5f, 4.0f, fieldSize.y * 1.5f), new Vector3(fieldSize.x * fieldSpacing, 4.0f, fieldSize.y * fieldSpacing));
            }
        }
        finally
        {
            isGenerating = false;
        }
    }

    private void CreateGenerateRoot()
    {
        if(generatedRoot != null)
        {
            return;
        }

        GameObject rootObject = new GameObject("Generated Fields");
        rootObject.transform.SetParent(transform);
        rootObject.transform.localPosition = Vector3.zero;
        rootObject.transform.localRotation = Quaternion.identity;

        generatedRoot = rootObject.transform;
    }

    private void ClearField()
    {
        for(int i = generatedRoot.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(generatedRoot.GetChild(i).gameObject);
        }
    }

    private void QueueGenerateField()
    {
#if UNITY_EDITOR
        if (rebuildQueued)
        {
            return;
        }

        rebuildQueued = true;
        EditorApplication.delayCall += GenerateFieldAfterValidate;
#endif
    }

#if UNITY_EDITOR
    private void GenerateFieldAfterValidate()
    {
        rebuildQueued = false;

        // スクリプト削除・再生開始などで無効になった場合は何もしない
        if (this == null || Application.isPlaying)
        {
            return;
        }

        GenerateField();
    }
#endif
}
