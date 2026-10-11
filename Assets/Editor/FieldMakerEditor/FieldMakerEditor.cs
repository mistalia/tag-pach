using UnityEngine;
using UnityEditor;
using Codice.CM.Common;

public class FieldMakerEditor : EditorWindow
{
    [SerializeField] private FieldMaker fieldMaker;

    public SerializedObject fieldData;
    public SerializedProperty fieldSizeProp;
    public SerializedProperty enableCellProp;

    private Vector2 scrollPos;

    private const float cellSize = 32f;

    [MenuItem("Tools/Field Maker")]
    private static void Open()
    {
        FieldMakerEditor window = GetWindow<FieldMakerEditor>("Field Maker");

        window.minSize = new Vector2(350f, 400f);
        window.Show();
    }

    private void OnEnable()
    {
        BindTarget();

        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;

        fieldData?.Dispose();
        fieldData = null;
    }

    private void OnUndoRedo()
    {
        Repaint();
    }

    private void BindTarget()
    {
        fieldData?.Dispose();

        fieldData = null;
        fieldSizeProp = null;
        enableCellProp = null;

        if(fieldMaker == null)
        {
            return;
        }

        fieldData = new SerializedObject(fieldMaker);
        fieldSizeProp = fieldData.FindProperty("fieldSize");
        enableCellProp = fieldData.FindProperty("enableCell");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("フィールド編集", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        FieldMaker selectedFieldMaker = (FieldMaker)EditorGUILayout.ObjectField("編集対象", fieldMaker, typeof(FieldMaker), true);

        if(EditorGUI.EndChangeCheck())
        {
            fieldMaker = selectedFieldMaker;
            BindTarget();
        }

        if(fieldMaker == null)
        {
            EditorGUILayout.HelpBox("HieralchyからFieldMakerが付いたオブジェクトを「編集対象」に設定してください。", MessageType.Info);
            return;
        }

        if(fieldData == null)
        {
            BindTarget();
        }

        if(fieldSizeProp == null || enableCellProp == null)
        {
            EditorGUILayout.HelpBox("FieldMakerのフィールド情報を取得できませんでした。", MessageType.Error);
            return;
        }

        if(EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("プレイモード中は編集できません。", MessageType.Warning);
            return;
        }

        fieldData.Update();
        Vector2Int oldSize = fieldSizeProp.vector2IntValue;
        Vector2Int newSize = EditorGUILayout.Vector2IntField("フィールドサイズ( X / Y )", oldSize);

        // Clamp the new size to be within the range of 1 to 64
        newSize.x= Mathf.Clamp(newSize.x, 1, 64);
        newSize.y= Mathf.Clamp(newSize.y, 1, 64);

        if(newSize != oldSize || enableCellProp.arraySize != newSize.x * newSize.y)
        {
            ResizeCell(oldSize, newSize);
            fieldSizeProp.vector2IntValue = newSize;
        }

        EditorGUILayout.HelpBox("クリックで床の有無を切り替えます\n" + "緑 = 床あり / 灰色 = 床無し", MessageType.Info);


        using (new EditorGUILayout.HorizontalScope())
        {
            if(GUILayout.Button("フィールド生成"))
            {
                fieldMaker.GenerateField();
            }
        }
            using (new EditorGUILayout.HorizontalScope())
        {
            if(GUILayout.Button("全て有効"))
            {
                SetAllCell(true);
            }

            if(GUILayout.Button("全て解除"))
            {
                SetAllCell(false);
            }
        }

        DrawGrid(newSize);

        fieldData.ApplyModifiedProperties();
    }

    private void DrawGrid(Vector2Int size)
    {
        Rect vp = GUILayoutUtility.GetRect(100f,100f,GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        if(Event.current.type == EventType.Layout)
        {
            return;
        }

        const float margin = cellSize;

        float boardWidth = size.x * cellSize;
        float boardHeight = size.y * cellSize;

        float contentWidth = Mathf.Max(vp.width - 20.0f, boardWidth + margin * 2f);
        float contentHeight = Mathf.Max(vp.height - 20.0f, boardHeight + margin * 2f);

        Rect contentRect = new Rect(0f, 0f, contentWidth, contentHeight);

        scrollPos = GUI.BeginScrollView(vp, scrollPos, contentRect);

        Color backgroundColor = new Color(0.16f, 0.16f, 0.16f);
        Color boardColor = new Color(0.23f, 0.23f, 0.23f);
        Color floorColor = new Color(0.25f, 0.65f, 0.38f);
        Color lineColor = new Color(0.35f, 0.35f, 0.35f);

        Rect boardRect = new Rect(margin, margin, boardWidth, boardHeight);

        EditorGUI.DrawRect(contentRect, backgroundColor);
        EditorGUI.DrawRect(boardRect, boardColor);

        GUIStyle coordinateStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };
        coordinateStyle.normal.textColor = Color.white;

        for(int z = 0; z < size.y; z++)
        {
            for (int x = 0; x < size.x; x++)
            {
                int index = x + z * size.x;
                SerializedProperty cell = enableCellProp.GetArrayElementAtIndex(index);

                Rect cellRect = new Rect(margin + x * cellSize, margin + (size.y - 1 - z) * cellSize, cellSize, cellSize);

                if (cell.boolValue)
                {
                    EditorGUI.DrawRect(cellRect, floorColor);
                }

                if(GUI.Button(cellRect,GUIContent.none,GUIStyle.none))
                {
                    cell.boolValue = !cell.boolValue;
                    Repaint();
                }

                GUI.Label(cellRect, $"({x},{z})", coordinateStyle);
            }
        }

        for(float x = 0f; x  < contentWidth; x += cellSize)
        {
            EditorGUI.DrawRect(new Rect(x,0f,1f,contentHeight), lineColor);
        }

        for(float y = 0f; y < contentHeight; y += cellSize)
        {
            EditorGUI.DrawRect(new Rect(0f, y, contentWidth, 1f), lineColor);
        }

        // 編集可能範囲を枠で囲む
        Color borderColor = new Color(0.65f, 0.8f, 1f);

        EditorGUI.DrawRect(
            new Rect(boardRect.x, boardRect.y, boardWidth, 2f),
            borderColor);

        EditorGUI.DrawRect(
            new Rect(boardRect.x, boardRect.yMax - 2f, boardWidth, 2f),
            borderColor);

        EditorGUI.DrawRect(
            new Rect(boardRect.x, boardRect.y, 2f, boardHeight),
            borderColor);

        EditorGUI.DrawRect(
            new Rect(boardRect.xMax - 2f, boardRect.y, 2f, boardHeight),
            borderColor);

        GUI.EndScrollView();

    }

    private void SetAllCell(bool val)
    {
        for(int i = 0;i < enableCellProp.arraySize;i++)
        {
            enableCellProp.GetArrayElementAtIndex(i).boolValue = val;
        }
    }

    private void ResizeCell(Vector2Int oldSize, Vector2Int newSize)
    {
        bool[] oldCell = new bool[enableCellProp.arraySize];

        for(int i = 0; i < oldCell.Length; i++)
        {
            oldCell[i] = enableCellProp.GetArrayElementAtIndex(i).boolValue;
        }

        enableCellProp.arraySize = newSize.x * newSize.y;

        for(int z = 0; z < newSize.y; z++)
        {
            for(int x = 0; x < newSize.x;x++)
            {
                bool value = false;

                if(x < oldSize.x && z < oldSize.y)
                {
                    int oldIndex = z * oldSize.x + x;

                    if(oldIndex < oldCell.Length)
                    {
                        value = oldCell[oldIndex];
                    }
                }

                int newIndex = z * newSize.x + x;

                enableCellProp.GetArrayElementAtIndex(newIndex).boolValue = value;
            }
        }
    }
}