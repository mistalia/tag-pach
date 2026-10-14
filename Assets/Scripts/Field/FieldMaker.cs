using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class FieldPattern
{
    public bool HasData;
    public Vector2Int Size;
    public bool[] EnableCell = new bool[0];
}

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
    [SerializeField, HideInInspector] private bool[] enableCell = new bool[0];
    [SerializeField]
    private FieldPattern[] savedPattern = new FieldPattern[3]
    {
        new FieldPattern(),
        new FieldPattern(),
        new FieldPattern()
    };



    [Header("Generated Object")]
    [SerializeField] private Transform generatedRoot;

    const float oversize = 1f;
    const float barrierHeight = 10f;

    private bool isGenerating = false;
    private bool rebuildQueued = false;

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
                    if (!enableCell[x + y * fieldSize.x])
                    {
                        // 無効なセルは上方にバリアを作ってcontinue
                        GameObject barrierInstans = Instantiate(barrierBlock, generatedRoot);
                        barrierInstans.name = $"Barrier_{x}_{y}";
                        barrierInstans.transform.localPosition = new Vector3(x * 1.0f * fieldSpacing, 5.0f, y * 1.0f * fieldSpacing);
                        barrierInstans.transform.localScale = new Vector3(fieldSpacing, barrierHeight, fieldSpacing);
                        barrierInstans.tag = "BarrierBlock";
                        continue;
                    }

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
                    barrierInstans.transform.localScale = new Vector3(1.0f, barrierHeight, fieldSize.y * fieldSpacing + (fieldSpacing * oversize));
                    barrierInstans.tag = "BarrierBlock";
                }

                {//奥面
                    GameObject barrierInstans = Instantiate(barrierBlock, generatedRoot);

                    barrierInstans.name = $"Barrier_Far";
                    barrierInstans.transform.localPosition = new Vector3((fieldSize.x - 1) * fieldSpacing * 0.5f,5.0f,fieldSize.y * fieldSpacing - 0.5f);
                    barrierInstans.transform.localScale = new Vector3(fieldSize.x * fieldSpacing + (fieldSpacing * oversize), barrierHeight, 1.0f);
                    barrierInstans.tag = "BarrierBlock";
                }

                {//右面
                    GameObject barrierInstans = Instantiate(barrierBlock, generatedRoot);

                    barrierInstans.name = $"Barrier_Right";
                    barrierInstans.transform.localPosition = new Vector3(fieldSize.x * fieldSpacing - 0.5f, 5.0f, (fieldSize.y - 1) * fieldSpacing * 0.5f);
                    barrierInstans.transform.localScale = new Vector3(1.0f, barrierHeight, fieldSize.y * fieldSpacing + (fieldSpacing * oversize));
                    barrierInstans.tag = "BarrierBlock";
                }

                {//奥面
                    GameObject barrierInstans = Instantiate(barrierBlock, generatedRoot);

                    barrierInstans.name = $"Barrier_Near";
                    barrierInstans.transform.localPosition = new Vector3((fieldSize.x - 1) * fieldSpacing * 0.5f, 5.0f, -1.5f);
                    barrierInstans.transform.localScale = new Vector3(fieldSize.x * fieldSpacing + (fieldSpacing * oversize), barrierHeight, 1.0f);
                    barrierInstans.tag = "BarrierBlock";
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
        if(generatedRoot == null)
        {
            return;
        }


        for(int i = generatedRoot.childCount - 1; i >= 0; i--)
        {
            GameObject tgt = generatedRoot.GetChild(i).gameObject;

            if (Application.isPlaying)
            {
                var fieldTile = tgt.GetComponent<FieldTile>();
                if (fieldTile != null)
                {
                    fieldTile.RemoveStone();
                    fieldManager.UnregisterFieldTile(fieldTile.GetCoordinate());
                }

                tgt.SetActive(false);
                tgt.transform.SetParent(null, true);
                Destroy(tgt);
            }
            else
            {
                DestroyImmediate(tgt);
            }
        }
    }

    public void GenerateFieldFromPattern(int patternNumber)
    {
        // 外部からは1～3で指定する
        if (patternNumber < 1 || patternNumber > 3)
        {
            Debug.LogError("パターン番号は1～3で指定してください。", this);
            return;
        }

        int index = patternNumber - 1;

        if (savedPattern == null || index >= savedPattern.Length)
        {
            Debug.LogError("保存パターンの配列が未設定です。", this);
            return;
        }

        FieldPattern pattern = savedPattern[index];

        if (pattern == null || !pattern.HasData)
        {
            Debug.LogWarning(
                $"パターン{patternNumber}は未保存です。",
                this);
            return;
        }

        // 不正なデータで既存フィールドを消さないよう、先に確認する
        if (pattern.Size.x <= 0 || pattern.Size.y <= 0 ||
            pattern.EnableCell == null ||
            pattern.EnableCell.Length != (long)pattern.Size.x * pattern.Size.y)
        {
            Debug.LogError(
                $"パターン{patternNumber}のサイズと塗り情報が不正です。",
                this);
            return;
        }

        fieldSize = pattern.Size;

        // 保存済みデータと共有しないよう、配列をコピーする
        enableCell = (bool[])pattern.EnableCell.Clone();

        GenerateField();
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
