using UnityEngine;
using UnityEngine.InputSystem;

// コントローラー登録画面の進行管理
public sealed class ControllerJoinScene : MonoBehaviour
{
    // 次に遷移するシーン名
    [SerializeField]
    private string nextSceneName = "DevelopScene";

    [SerializeField]
    private TransitionSettings transitionSettings;

    [SerializeField]
    private InputActionReference proceedAction;

    [SerializeField]
    private InputActionReference clearRegistrationAction;

    private void OnEnable()
    {
        ControllerSessionManager session = ControllerSessionManager.Instance;

        // BindSceneに入ったら以前のバインド状態を破棄し、改めて登録
        if (session != null)
            session.ClearRegistration();

        if (proceedAction != null)
        {
            proceedAction.action.performed += OnProceedPerformed;
            proceedAction.action.Enable();
        }

        if (clearRegistrationAction != null)
        {
            clearRegistrationAction.action.performed += OnClearRegistrationPerformed;
            clearRegistrationAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (proceedAction != null)
        {
            proceedAction.action.performed -= OnProceedPerformed;
        }

        if (clearRegistrationAction != null)
        {
            clearRegistrationAction.action.performed -= OnClearRegistrationPerformed;
        }
    }

    // 条件を満たしていれば次のシーンへ遷移する
    public void LoadNextScene()
    {
        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null)
        {
            Debug.LogError(
                "[ControllerJoinScene] " +
                "ControllerSessionManagerが存在しません。",
                this
            );

            return;
        }

        // 登録が終了してから次のシーンへ進む
        if (!session.AreAllTeamsComplete)
            return;

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogError(
                "[ControllerJoinScene] " +
                "遷移先シーン名が設定されていません。",
                this
            );

            return;
        }

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError(
                "[ControllerJoinScene] " +
                "SceneTransitionManagerが存在しません。",
                this
            );

            return;
        }

        SceneTransitionManager.Instance.LoadScene(nextSceneName, transitionSettings);
    }


    // 登録済みデバイスをすべて解除する
    public void ClearRegistration()
    {
        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null)
        {
            Debug.LogError("[ControllerJoinScene] " + "ControllerSessionManagerが存在しません。", this);
            return;
        }

        session.ClearRegistration();
    }

    private void OnProceedPerformed(InputAction.CallbackContext context)
    {
        LoadNextScene();
    }

    private void OnClearRegistrationPerformed(InputAction.CallbackContext context)
    {
        ClearRegistration();
    }
}