using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TitleSceneController : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] private Button twoPlayerButton;
    [SerializeField] private Button fourPlayerButton;
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField] private Text statusText;

    [Header("Controller Registration")]
    [SerializeField] private string registrationSceneName = "BindScene";
    [SerializeField] private ControllerSessionManager twoPlayerSessionPrefab;
    [SerializeField] private ControllerSessionManager fourPlayerSessionPrefab;

    [Header("Presentation")]
    [SerializeField] private TitleEntranceAnimation entranceAnimation;
    [SerializeField, Min(0.0f)] private float entranceDuration = 0.65f;
    [SerializeField, Min(0.0f)] private float exitDuration = 0.2f;

    private bool isTransitioning;

    private void OnEnable()
    {
        if (twoPlayerButton != null)
            twoPlayerButton.onClick.AddListener(StartTwoPlayerGame);
        if (fourPlayerButton != null)
            fourPlayerButton.onClick.AddListener(StartFourPlayerGame);
    }

    private IEnumerator Start()
    {
        if (twoPlayerButton == null || fourPlayerButton == null || menuCanvasGroup == null)
        {
            Debug.LogError("[TitleSceneController] Menu references are not configured.", this);
            enabled = false;
            yield break;
        }

        menuCanvasGroup.interactable = false;
        if (entranceAnimation != null && entranceAnimation.isActiveAndEnabled)
            yield return entranceAnimation.Play();
        else
            yield return FadeMenu(0.0f, 1.0f, entranceDuration);
        menuCanvasGroup.interactable = true;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(twoPlayerButton.gameObject);
    }

    private void OnDisable()
    {
        if (twoPlayerButton != null)
            twoPlayerButton.onClick.RemoveListener(StartTwoPlayerGame);
        if (fourPlayerButton != null)
            fourPlayerButton.onClick.RemoveListener(StartFourPlayerGame);
    }

    public void StartTwoPlayerGame()
    {
        StartGame(twoPlayerSessionPrefab, ControllerBindingMode.TwoControllers);
    }

    public void StartFourPlayerGame()
    {
        StartGame(fourPlayerSessionPrefab, ControllerBindingMode.FourControllers);
    }

    private void StartGame(ControllerSessionManager sessionPrefab, ControllerBindingMode bindingMode)
    {
        if (isTransitioning || menuCanvasGroup == null || !menuCanvasGroup.interactable)
            return;

        if (sessionPrefab == null || sessionPrefab.BindingMode != bindingMode)
        {
            ShowError("コントローラー設定を確認してください。");
            return;
        }

        if (string.IsNullOrWhiteSpace(registrationSceneName) ||
            !Application.CanStreamedLevelBeLoaded(registrationSceneName))
        {
            ShowError("コントローラー登録シーンがビルド設定にありません。");
            return;
        }

        isTransitioning = true;
        menuCanvasGroup.interactable = false;
        StartCoroutine(LoadRegistrationScene(sessionPrefab));
    }

    private IEnumerator LoadRegistrationScene(ControllerSessionManager sessionPrefab)
    {
        ControllerSessionManager session = ControllerSessionManager.Instance;
        if (session == null)
            session = Instantiate(sessionPrefab);

        if (session.BindingMode != sessionPrefab.BindingMode)
            session.SetBindingMode(sessionPrefab.BindingMode);
        else
            session.AutoRegisterConnectedDevices();
        session.ResetAllReadyStates();
        if (TeamPlayerAssignmentManager.Instance != null)
            TeamPlayerAssignmentManager.Instance.ResetAssignments();

        yield return FadeMenu(1.0f, 0.0f, exitDuration);
        yield return SceneManager.LoadSceneAsync(registrationSceneName);
    }

    private IEnumerator FadeMenu(float fromAlpha, float toAlpha, float duration)
    {
        menuCanvasGroup.alpha = fromAlpha;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            menuCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        menuCanvasGroup.alpha = toAlpha;
    }

    private void ShowError(string message)
    {
        if (statusText != null)
            statusText.text = message;
        Debug.LogError($"[TitleSceneController] {message}", this);
    }
}
