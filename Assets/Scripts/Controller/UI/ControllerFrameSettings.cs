using UnityEngine;

// Controller‘I‘ðUI‚Ì˜g•\Ž¦Ý’è
[CreateAssetMenu(fileName = "ControllerFrameSettings", menuName = "Controller/Frame Settings")]
public sealed class ControllerFrameSettings : ScriptableObject
{
    [Header("Layout")]
    [SerializeField, Min(1.0f)]
    private float thickness = 8.0f;

    [SerializeField, Min(0.0f)]
    private float padding = 0.0f;

    [Header("Colors")]
    [SerializeField]
    private Color waitingColor  = Color.white;

    [SerializeField]
    private Color readyColor    = Color.green;

    public float Thickness => thickness;

    public float Padding => padding;

    public Color WaitingColor => waitingColor;

    public Color ReadyColor => readyColor;
}