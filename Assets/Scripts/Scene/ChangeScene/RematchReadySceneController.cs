using UnityEngine;

// Äí€”õŠ®—¹Œã‚ÌScene‘JˆÚ‚ğ’S“–‚·‚é
public sealed class RematchReadySceneController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField]
    private string nextSceneName = "DefaultScene";

    [SerializeField]
    private TransitionSettings transitionSettings;

    private RematchReadyManager manager;
    private bool isTransitioning;

    private void Start()
    {
        manager = RematchReadyManager.Instance;

        if (manager == null)
        {
            Debug.LogError("[RematchReadySceneController] RematchReadyManager‚ª‘¶İ‚µ‚Ü‚¹‚ñB", this);

            enabled = false;
            return;
        }

        manager.TransitionRequested += HandleTransitionRequested;
    }

    private void OnDestroy()
    {
        if (manager != null)
            manager.TransitionRequested -= HandleTransitionRequested;
    }

    private void HandleTransitionRequested()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        SceneTransitionManager.Instance.LoadScene(nextSceneName, transitionSettings);
    }
}