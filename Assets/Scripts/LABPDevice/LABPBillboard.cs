using UnityEngine;

public class LABPBillboard : MonoBehaviour
{
    private LABPCommunication communication;

    private Camera targetCamera;

    private Vector3 initialLocalPosition;

    private void Awake()
    {
        communication = GetComponentInParent<LABPCommunication>();

        if (communication == null)
        {
            Debug.LogError("LABPCommunication is not found", this);
        }

        initialLocalPosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            return;
        }

        // Device‚ğŠî€‚É‚µ‚½ˆÊ’u
        Vector3 centerPosition = transform.parent.TransformPoint(initialLocalPosition);

        // ƒJƒƒ‰•ûŒü‚Ö­‚µˆÚ“®
        Vector3 cameraDirection = (targetCamera.transform.position - centerPosition).normalized;

        transform.position = centerPosition + cameraDirection * communication.GetCameraOffset();

        // ƒJƒƒ‰‚Æ“¯‚¶Œü‚«‚É‚·‚é
        transform.rotation = targetCamera.transform.rotation;
    }
}
