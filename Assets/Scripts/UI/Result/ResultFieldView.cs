using UnityEngine;
using UnityEngine.UI;

// Renders the players in the field scene retained underneath the result UI.
internal sealed class ResultFieldView : MonoBehaviour
{
    private RenderTexture frame;
    private Camera fieldCamera;

    public static void Show(Image placeholder, Player player, bool winner, GameObject fallbackModel, Camera fieldCamera)
    {
        RectTransform panel = (RectTransform)placeholder.transform.parent;
        var surface = new GameObject("Field Camera View", typeof(RectTransform), typeof(RawImage));
        var rect = (RectTransform)surface.transform;
        rect.SetParent(panel, false);
        rect.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var image = surface.GetComponent<RawImage>();
        image.raycastTarget = false;

        int height = winner ? 720 : 384;
        float aspect = panel.rect.width / panel.rect.height;
        var view = surface.AddComponent<ResultFieldView>();
        view.frame = new RenderTexture(Mathf.RoundToInt(height * aspect), height, 24);
        image.texture = view.frame;
        var cameraObject = new GameObject("Result Camera");
        var camera = cameraObject.AddComponent<Camera>();
        view.fieldCamera = camera;
        camera.targetTexture = view.frame;
        camera.aspect = aspect;
        camera.fieldOfView = 36;
        camera.nearClipPlane = 0.04f;
        camera.clearFlags = winner && fieldCamera ? fieldCamera.clearFlags : CameraClearFlags.SolidColor;
        camera.backgroundColor = winner && fieldCamera ? fieldCamera.backgroundColor : Color.white;

        var capsule = player.GetComponent<CapsuleCollider>();
        if (fallbackModel && capsule && player.GetComponentInChildren<SkinnedMeshRenderer>() == null)
        {
            var visual = Instantiate(fallbackModel, player.transform, false);
            Bounds original = ModelBounds(visual.transform);
            if (original.size.y > 0.001f)
            {
                visual.transform.localScale *= capsule.bounds.size.y / original.size.y;
                visual.transform.position += capsule.bounds.center - ModelBounds(visual.transform).center;
            }
        }
        Bounds body = ModelBounds(player.transform);
        Vector3 focus = body.center + Vector3.up * body.size.y * 0.18f;
        float halfHeight = body.size.y * (winner ? 0.47f : 0.34f);
        Vector3 front = player.transform.forward;
        if (winner && player.GetLABPDevice() != null)
        {
            Bounds partner = ModelBounds(player.GetLABPDevice().transform);
            Vector3 separation = partner.center - focus;
            focus += separation * 0.4f;
            // Turn toward the pair from the side so their silhouettes stay separate.
            Vector3 side = Vector3.Cross(Vector3.up, separation).normalized;
            if (Vector3.Dot(side, front) < 0) side = -side;
            front = (front + side).normalized;
            halfHeight = Mathf.Max(halfHeight, (separation.magnitude * 0.6f + partner.extents.magnitude * 0.65f) / aspect);
        }
        if (!winner)
        {
            int layer = LayerMask.NameToLayer("ResultOpponent");
            foreach (Renderer renderer in player.GetComponentsInChildren<Renderer>())
                renderer.gameObject.layer = layer;
            camera.cullingMask = 1 << layer;
        }
        float distance = Mathf.Max(halfHeight, 0.35f) / Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
        camera.transform.position = focus + (front + Vector3.up * 0.18f).normalized * distance;
        camera.transform.LookAt(focus);
        placeholder.enabled = false;
    }

    private static Bounds ModelBounds(Transform subject)
    {
        Bounds bounds = default;
        bool hasBounds = false;
        foreach (var skin in subject.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (!skin.enabled || !skin.sharedMesh) continue;
            var baked = new Mesh();
            skin.BakeMesh(baked);
            foreach (var vertex in baked.vertices)
                Include(skin.transform.TransformPoint(vertex));
            Destroy(baked);
        }
        foreach (var mesh in subject.GetComponentsInChildren<MeshFilter>())
        {
            var renderer = mesh.GetComponent<MeshRenderer>();
            if (!renderer || !renderer.enabled || !mesh.sharedMesh || renderer.bounds.size.sqrMagnitude < 0.0001f) continue;
            Include(renderer.bounds.min);
            Include(renderer.bounds.max);
        }
        if (hasBounds) return bounds;

        void Include(Vector3 point)
        {
            if (hasBounds) bounds.Encapsulate(point);
            else bounds = new Bounds(point, Vector3.zero);
            hasBounds = true;
        }
        var collider = subject.GetComponentInChildren<Collider>();
        return collider != null ? collider.bounds : new Bounds(subject.position, Vector3.one);
    }

    private void OnDestroy()
    {
        if (fieldCamera != null)
        {
            fieldCamera.targetTexture = null;
            Destroy(fieldCamera.gameObject);
        }
        if (frame != null) Destroy(frame);
    }
}
