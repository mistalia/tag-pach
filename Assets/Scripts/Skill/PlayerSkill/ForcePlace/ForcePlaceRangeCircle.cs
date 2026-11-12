using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SC_ForcePlaceRangeCircle : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Min(3)]
    private int segmentCount = 32;

    [SerializeField]
    private float heightOffset = 0.05f;

    private Mesh circleMesh;
    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();

        circleMesh = new Mesh();
        circleMesh.name = "ForcePlace Range Circle";

        meshFilter.sharedMesh = circleMesh;
    }

    public void Init(float radius, float forwardOffset)
    {
        transform.localPosition = new Vector3(0f, heightOffset, forwardOffset);

        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        BuildCircleMesh(radius);
    }

    private void BuildCircleMesh(float radius)
    {
        circleMesh.Clear();

        radius = Mathf.Max(0.01f, radius);

        int count = Mathf.Max(3, segmentCount);

        Vector3[] vertices = new Vector3[count + 2];
        Vector2[] uvs = new Vector2[count + 2];
        int[] triangles = new int[count * 3];

        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);

        float angleStep = 360f / count;

        for (int i = 0; i <= count; i++)
        {
            float angle = angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            float x = Mathf.Sin(rad);
            float z = Mathf.Cos(rad);

            vertices[i + 1] = new Vector3(x * radius, 0f, z * radius);

            uvs[i + 1] = new Vector2((x + 1f) * 0.5f, (z + 1f) * 0.5f);
        }

        for (int i = 0; i < count; i++)
        {
            int index = i * 3;

            triangles[index] = 0;
            triangles[index + 1] = i + 1;
            triangles[index + 2] = i + 2;
        }

        circleMesh.vertices = vertices;
        circleMesh.triangles = triangles;
        circleMesh.uv = uvs;

        circleMesh.RecalculateNormals();
        circleMesh.RecalculateBounds();
    }

    private void OnDestroy()
    {
        if (circleMesh != null)
        {
            Destroy(circleMesh);
        }
    }
}
