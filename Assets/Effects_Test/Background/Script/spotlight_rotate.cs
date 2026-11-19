using UnityEngine;

public class SpotlightSwing : MonoBehaviour
{
    [Header("首振りの最大角度（±度）")]
    [SerializeField] private float maxAngleX = 15f; // 上下の振れ幅（15度 ～ -15度）
    [SerializeField] private float maxAngleY = 25f; // 左右の振れ幅（25度 ～ -25度）

    [Header("首振りの速さ")]
    [SerializeField] private float speedX = 1.5f;
    [SerializeField] private float speedY = 1.0f;

    [Header("開始タイミングのズレ（複数ライト用）")]
    [SerializeField] private float timeOffset = 0f;

    private Quaternion initialRotation;

    void Start()
    {
        // ゲーム開始時のライトの向き（基準角度）を保存
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        float time = Time.time + timeOffset;

        // Mathf.Sin は -1 ～ 1 の間を滑らかに往復する値を返す
        float angleX = Mathf.Sin(time * speedX) * maxAngleX;
        float angleY = Mathf.Sin(time * speedY) * maxAngleY;

        // 基準の向きに対して、X軸とY軸の首振り角度を適用
        Quaternion swingRotation = Quaternion.Euler(angleX, angleY, 0f);
        transform.localRotation = initialRotation * swingRotation;
    }
}
