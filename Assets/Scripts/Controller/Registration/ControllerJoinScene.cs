using UnityEngine;
using UnityEngine.InputSystem;

// コントローラー登録画面の進行管理
public sealed class ControllerJoinScene : MonoBehaviour
{
    // 次に遷移するシーン名
    [SerializeField]
    private string nextSceneName = "DefaultScene";

    [SerializeField]
    private TransitionSettings transitionSettings;

    [SerializeField]
    private InputActionReference proceedAction;

    [SerializeField]
    private InputActionReference backAction;

    private void Start()
    {
        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null)
            return;

        session.ResetAllReadyStates();
    }

    private void OnEnable()
    {
        if (proceedAction != null)
        {
            proceedAction.action.performed += OnProceedPerformed;
            proceedAction.action.Enable();
        }

        if (backAction != null)
        {
            backAction.action.performed += OnBackPerformed;
            backAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (proceedAction != null)
        {
            proceedAction.action.performed -= OnProceedPerformed;
        }

        if (backAction != null)
        {
            backAction.action.performed -= OnBackPerformed;
        }
    }

    // 条件を満たしていれば次のシーンへ遷移する
    public void LoadNextScene()
    {
        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null)
            return;

        // 両チームの準備が完了してから次のシーンへ進む
        if (!session.AreAllTeamsComplete)
            return;

        if (!session.CommitCurrentRoleAssignments())
        {
            Debug.LogError("[ControllerJoinScene] 選択されたRoleを確定できませんでした。", this);

            return;
        }

        SceneTransitionManager.Instance.LoadScene(nextSceneName, transitionSettings);
    }

    private void OnProceedPerformed(InputAction.CallbackContext context)
    {
        LoadNextScene();
    }

    private void OnBackPerformed(InputAction.CallbackContext context)
    {
        SceneTransitionManager.Instance.LoadScene("TitleScene", transitionSettings);
    }
}