using UnityEditor;
using UnityEngine;

public class SpectatorPreviewWindow : EditorWindow
{
    [SerializeField] private RenderTexture spectatorTexture;

    [MenuItem("Tools/Spectator Preview")]
    private static void Open()
    {
        SpectatorPreviewWindow window =
            GetWindow<SpectatorPreviewWindow>("Spectator Preview");

        window.minSize = new Vector2(320f, 220f);
        window.Show();
    }

    private void OnEnable()
    {
        EditorApplication.update += UpdatePreview;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdatePreview;
    }

    private void UpdatePreview()
    {
        // 再生中は継続的に映像を更新
        if (EditorApplication.isPlaying)
        {
            Repaint();
        }
    }

    private void OnGUI()
    {
        spectatorTexture =
            (RenderTexture)EditorGUILayout.ObjectField(
                "観戦用RT",
                spectatorTexture,
                typeof(RenderTexture),
                false);

        if (spectatorTexture == null)
        {
            EditorGUILayout.HelpBox(
                "観戦カメラの出力先と同じRenderTextureを指定してください。",
                MessageType.Info);

            return;
        }

        Rect previewRect = GUILayoutUtility.GetRect(
            1f,
            1f,
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true));

        if (Event.current.type == EventType.Repaint)
        {
            // 余白を黒くする
            EditorGUI.DrawRect(previewRect, Color.black);

            // 縦横比を維持してRTを表示する
            GUI.DrawTexture(
                previewRect,
                spectatorTexture,
                ScaleMode.ScaleToFit,
                false);
        }
    }
}
