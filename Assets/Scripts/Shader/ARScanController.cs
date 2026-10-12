using UnityEngine;

public class ARScanController : MonoBehaviour
{
    [SerializeField] private Material ScanWave;
    [SerializeField] private Transform scanCenter;

    [SerializeField] private float scanSpeed = 1f;
    [SerializeField] private float maxScanRadius = 15f;

    private float currentRadius = 0f;

    private void Start()
    {
        currentRadius = 0f;

        if (ScanWave != null)
            ScanWave.SetFloat("_ScanRadius", currentRadius);
    }

    private void Update()
{
    Debug.Log(
        $"Speed={scanSpeed}, " +
        $"DeltaTime={Time.deltaTime}, " +
        $"TimeScale={Time.timeScale}, " +
        $"MaxRadius={maxScanRadius}, " +
        $"CurrentRadius={currentRadius}"
    );

    if (ScanWave == null || scanCenter == null)
        return;

    currentRadius += scanSpeed * Time.deltaTime;
    currentRadius = Mathf.Min(currentRadius, maxScanRadius);

    ScanWave.SetVector("_ScanCenter", scanCenter.position);
    ScanWave.SetFloat("_ScanRadius", currentRadius);
}
}