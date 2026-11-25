using System.Collections;
using UnityEngine;

// キャラクターの見た目だけを揺らすCharacter Shakeの実行対象
public sealed class CharacterShakeTarget : MonoBehaviour, ICharacterShakeTarget
{
    [SerializeField]
    private Transform visualRoot;

    private Coroutine shakeCoroutine;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    private void Awake()
    {
        if (visualRoot == null)
        {
            Debug.LogError("[CharacterShakeTarget] Visual Rootが設定されていません。", this);
            enabled = false;
            return;
        }

        initialLocalPosition = visualRoot.localPosition;
        initialLocalRotation = visualRoot.localRotation;
    }

    public void RequestShake(CharacterShakeRequest request)
    {
        if (!enabled || request.Duration <= 0.0f)
            return;

        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        ResetVisual();
        shakeCoroutine = StartCoroutine(ShakeCoroutine(request));
    }

    private IEnumerator ShakeCoroutine(CharacterShakeRequest request)
    {
        float elapsedTime = 0.0f;
        float randomTimer = 0.0f;
        Vector3 randomPositionOffset = Vector3.zero;
        Vector3 randomRotationOffset = Vector3.zero;

        while (elapsedTime < request.Duration)
        {
            float deltaTime = Time.unscaledDeltaTime;
            elapsedTime += deltaTime;

            if (request.Mode == CharacterShakeMode.Random)
            {
                randomTimer -= deltaTime;

                if (randomTimer <= 0.0f)
                {
                    CreateRandomOffsets(request, out randomPositionOffset, out randomRotationOffset);
                    randomTimer = request.Frequency > 0.0f ? 1.0f / request.Frequency : request.Duration;
                }

                ApplyRandomShake(randomPositionOffset, randomRotationOffset);
            }
            else
            {
                ApplyConstantShake(request, elapsedTime);
            }

            yield return null;
        }

        ResetVisual();
        shakeCoroutine = null;
    }

    // 一定周期の往復運動で毎回同じ揺れを再生する
    private void ApplyConstantShake(CharacterShakeRequest request, float elapsedTime)
    {
        float phase = elapsedTime * request.Frequency * Mathf.PI * 2.0f;
        float wave = Mathf.Sin(phase);
        Vector3 positionOffset = Vector3.right * wave * request.PositionStrength;
        Quaternion rotationOffset = Quaternion.Euler(0.0f, 0.0f, wave * request.RotationStrength);

        visualRoot.localPosition = initialLocalPosition + positionOffset;
        visualRoot.localRotation = initialLocalRotation * rotationOffset;
    }

    // Frequencyの周期ごとに新しいランダムOffsetを生成する
    private static void CreateRandomOffsets(CharacterShakeRequest request, out Vector3 positionOffset, out Vector3 rotationOffset)
    {
        positionOffset = Random.insideUnitSphere * request.PositionStrength;
        rotationOffset = Random.insideUnitSphere * request.RotationStrength;
    }

    private void ApplyRandomShake(Vector3 positionOffset, Vector3 rotationOffset)
    {
        visualRoot.localPosition = initialLocalPosition + positionOffset;
        visualRoot.localRotation = initialLocalRotation * Quaternion.Euler(rotationOffset);
    }

    private void OnDisable()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = null;
        ResetVisual();
    }

    // Shake終了時は必ず開始前の位置・回転へ戻す
    private void ResetVisual()
    {
        if (visualRoot == null)
            return;

        visualRoot.localPosition = initialLocalPosition;
        visualRoot.localRotation = initialLocalRotation;
    }
}