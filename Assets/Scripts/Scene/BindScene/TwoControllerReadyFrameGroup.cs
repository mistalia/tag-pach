using UnityEngine;

// TwoControllers‚Å1‘ä‚ÌController‚ÌReadyó‘Ô‚ğ2‚Â‚Ì˜g‚Ö”½‰f
public sealed class TwoControllerReadyFrameGroup : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField, Range(1, 2)]
    private int controllerNumber = 1;

    [Header("Frames")]
    [SerializeField]
    private ControllerReadyFrame firstFrame;

    [SerializeField]
    private ControllerReadyFrame secondFrame;

    private ControllerSessionManager session;

    private void Start()
    {
        session = ControllerSessionManager.Instance;

        if (session == null)
        {
            Debug.LogError("[TwoControllerReadyFrameGroup] ControllerSessionManager‚ª‘¶İ‚µ‚Ü‚¹‚ñB", this);
            return;
        }

        if (session.BindingMode != ControllerBindingMode.TwoControllers)
            return;

        session.ControllerReadyChanged += HandleControllerReadyChanged;

        Refresh();
    }

    private void OnDestroy()
    {
        if (session != null)
            session.ControllerReadyChanged -= HandleControllerReadyChanged;
    }

    // ‘ÎÛController‚ÌReady•ÏX‚ğ2‚Â‚Ì˜g‚Ö”½‰f‚·‚é
    private void HandleControllerReadyChanged(int teamIndex, int changedControllerNumber, bool isReady)
    {
        if (changedControllerNumber != controllerNumber)
            return;

        SetReady(isReady);
    }

    // Œ»İ•Û‚³‚ê‚Ä‚¢‚éReadyó‘Ô‚ğ˜g‚Ö“¯Šú‚·‚é
    private void Refresh()
    {
        ControllerRoleSelection selection = session.GetControllerSelection(controllerNumber);

        SetReady(selection != null && selection.IsReady);
    }

    private void SetReady(bool isReady)
    {
        firstFrame?.SetReady(isReady);
        secondFrame?.SetReady(isReady);
    }
}