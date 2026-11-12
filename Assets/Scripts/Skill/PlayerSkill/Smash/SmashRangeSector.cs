using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SC_SmashRangeSector : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Min(1)]
    private int segmentCount = 32;

    [SerializeField]
    private float heightOffset = 0.05f;

    private MeshFilter meshFilter;
    private Mesh sectorMesh;

    private float duration;
    private float timer;

    private bool initialized;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();

        sectorMesh = new Mesh();
        sectorMesh.name = "Smash Range Sector";

        meshFilter.sharedMesh = sectorMesh;
    }

    public void Init(
        Transform owner,
        float radius,
        float angleRange,
        float duration)
    {
        transform.localPosition = new Vector3(0f, heightOffset, 0f);

        transform.localRotation = Quaternion.identity;

        this.duration = Mathf.Max(0.01f, duration);
        timer = 0f;
        initialized = true;

        BuildSectorMesh(radius, angleRange);
    }

    private void Update()
    {
        if (!initialized) return;

        timer += Time.deltaTime;

        if (timer >= duration)
        {
            initialized = false;
            gameObject.SetActive(false);
        }
    }

    private void BuildSectorMesh(float radius, float angleRange)
    {
        sectorMesh.Clear();

        radius = Mathf.Max(0.01f, radius);
        angleRange = Mathf.Clamp(angleRange, 0.1f, 360f);

        // ê≥ñ ÇíÜêSÇ∆Ç∑ÇÈ
        float startAngle = -angleRange * 0.5f;

        int currentSegmentCount = Mathf.Max(1, segmentCount);

        int vertexCount = currentSegmentCount + 2;

        Vector3[] vertices = new Vector3[vertexCount];

        int[] triangles = new int[currentSegmentCount * 3];

        Vector2[] uvs = new Vector2[vertexCount];

        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);

        float angleStep = angleRange / currentSegmentCount;

        for (int i = 0; i <= currentSegmentCount; i++)
        {
            float angle = startAngle + angleStep * i;

            float rad = angle * Mathf.Deg2Rad;

            float sin = Mathf.Sin(rad);
            float cos = Mathf.Cos(rad);

            float x = sin * radius;
            float z = cos * radius;

            vertices[i + 1] = new Vector3(x, 0f, z);

            uvs[i + 1] = new Vector2((sin + 1f) * 0.5f, (cos + 1f) * 0.5f);
        }

        for (int i = 0; i < currentSegmentCount; i++)
        {
            int triIndex = i * 3;

            triangles[triIndex] = 0;
            triangles[triIndex + 1] = i + 1;
            triangles[triIndex + 2] = i + 2;
        }

        sectorMesh.vertices = vertices;
        sectorMesh.triangles = triangles;
        sectorMesh.uv = uvs;

        sectorMesh.RecalculateNormals();
        sectorMesh.RecalculateBounds();
    }

    private void OnDestroy()
    {
        if (sectorMesh != null)
        {
            Destroy(sectorMesh);
            sectorMesh = null;
        }
    }
}
