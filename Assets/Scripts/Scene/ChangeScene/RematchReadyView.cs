using TMPro;
using UnityEngine;

// 再戦準備画面全体の時間・Phase表示を管理する
public sealed class RematchReadyView : MonoBehaviour
{
    [Header("Final Confirmation")]
    [SerializeField]
    private GameObject finalConfirmationView;

    [Header("Timer")]
    [SerializeField]
    private TMP_Text remainingTimeText;

    private RematchReadyManager manager;

    private void Start()
    {
        manager = RematchReadyManager.Instance;

        if (manager == null)
        {
            Debug.LogError("[RematchReadyView] RematchReadyManagerが存在しません。", this);
            enabled = false;
            return;
        }

        manager.RemainingTimeChanged += HandleRemainingTimeChanged;
        manager.PhaseChanged += HandlePhaseChanged;

        HandleRemainingTimeChanged(manager.RemainingTime);
        HandlePhaseChanged(manager.CurrentPhase);
    }

    private void OnDestroy()
    {
        if (manager == null)
            return;

        manager.RemainingTimeChanged -= HandleRemainingTimeChanged;
        manager.PhaseChanged -= HandlePhaseChanged;
    }

    private void HandleRemainingTimeChanged(float remainingTime)
    {
        if (remainingTimeText == null)
            return;

        remainingTimeText.text = Mathf.CeilToInt(remainingTime).ToString();
    }

    // 現在のPhaseに応じて準備画面と最終確認画面を切り替える
    private void HandlePhaseChanged(RematchReadyPhase phase)
    {
        SetActive(finalConfirmationView, phase == RematchReadyPhase.FinalConfirmation);
    }

    private static void SetActive(GameObject target, bool isActive)
    {
        if (target != null)
            target.SetActive(isActive);
    }
}