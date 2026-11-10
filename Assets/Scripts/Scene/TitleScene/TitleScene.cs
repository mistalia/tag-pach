using UnityEngine;
using UnityEngine.InputSystem;

public sealed class TitleScene : MonoBehaviour
{
    [SerializeField]
    private string bindSceneName = "BindScene";

    [SerializeField]
    private TransitionSettings transitionSettings;

    // TODO:後で正式な入力に変更する
    [Header("Input")]
    [SerializeField]
    private Key twoControllersKey   = Key.Digit2;

    [SerializeField]
    private Key fourControllersKey  = Key.Digit4;

    private bool isTransitioning;

    private void Update()
    {
        if (isTransitioning || Keyboard.current == null)
            return;

        if (Keyboard.current[twoControllersKey].wasPressedThisFrame)
        {
            // ２台モード設定
            StartWithMode(ControllerBindingMode.TwoControllersTest);
            return;
        }

        if (Keyboard.current[fourControllersKey].wasPressedThisFrame)
        {
            // 4台モード設定
            StartWithMode(ControllerBindingMode.FourControllers);
        }      
    }

    private void StartWithMode(ControllerBindingMode mode)
    {
        isTransitioning = true;

        ControllerSessionManager.Instance.SetBindingMode(mode);

        SceneTransitionManager.Instance.LoadScene(bindSceneName, transitionSettings);
    }
}