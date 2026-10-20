using UnityEngine;
using UnityEngine.UI;
public class SpectatorCamera : MonoBehaviour
{
    [Header("観戦カメラ")]
    [SerializeField] private Camera spectatorCamera;

    [Header("ディスプレイ接続がない時の出力先")]
    [SerializeField] private RenderTexture outputTex;

    [Header("ビルド用のRT表示")]
    [SerializeField] private RawImage fallbackView;

    [Header("デバッグ")]
    [SerializeField] private bool forceRenderTex;

    public RenderTexture SpectorTexture => outputTex;

    void Start()
    {
        if(!spectatorCamera)
        {
            spectatorCamera = GetComponent<Camera>();
        }
        if (!outputTex)
        {
            Debug.Log("SpectatorCamera: RenderTextureが設定されていません。");
        }

        bool useExDisplay = !forceRenderTex && forceRenderTex && Display.displays.Length > 1;

        if(useExDisplay)
        {
            ConnectDisplay();
        }
        else
        {
            ConnectRV();
        }
    }

    private void ConnectDisplay()
    {
        spectatorCamera.targetTexture = null;
        spectatorCamera.targetDisplay = 1;

        Display.displays[1].Activate();

        if(fallbackView != null)
        {
            fallbackView.gameObject.SetActive(true);
        }

        Debug.Log("SpectorCamera: 外部ディスプレイに接続しました。");

    }

    private void ConnectRV()
    {
        spectatorCamera.targetDisplay = 0;
        spectatorCamera.targetTexture = outputTex;

        if(fallbackView != null)
        {
            fallbackView.texture = outputTex;

            fallbackView.gameObject.SetActive(!Application.isEditor);
        }

        if(!Application.isEditor && fallbackView == null)
        {
            Debug.Log("SpectorCamera: RenderTextureに接続しました。表示用RawImageは未設定です。");
        }

        Debug.Log("SpectorCamera: RenderTextureに接続しました。");
    }

}
