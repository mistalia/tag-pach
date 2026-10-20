using UnityEngine;
using UnityEngine.InputSystem;

// 登録状態に応じてUIの更新
public sealed class ControllerJoinView : MonoBehaviour
{
    [SerializeField]
    private GameObject[] assignmentJoinedImages = new GameObject[ControllerSessionManager.MaxControllerCount];

    private ControllerSessionManager session;

    private bool isSubscribed;

    private void Start()
    {
        SubscribeSessionEvents();
    }

    private void OnEnable()
    {
        if (isSubscribed)
            return;

        SubscribeSessionEvents();
    }

    private void OnDisable()
    {
        UnsubscribeSessionEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeSessionEvents();
    }

    private void OnValidate()
    {
        if (assignmentJoinedImages == null)
            return;

        if (assignmentJoinedImages.Length != ControllerSessionManager.MaxControllerCount)
        {
            Debug.LogWarning(
                $"[ControllerJoinView] " +
                $"Assignment Joined Imagesには" +
                $"{ControllerSessionManager.MaxControllerCount}個の" +
                "要素を設定してください。",
                this
            );
        }
    }

    public void Refresh()
    {
        if (session == null)
        {
            SetAllImagesInactive();
            return;
        }

        for (int teamIndex = 0; teamIndex < ControllerSessionManager.TeamCount; teamIndex++)
        {
            SetAssignmentImageActive(teamIndex, ControllerRole.Operation, session.GetDevice(teamIndex, ControllerRole.Operation) != null);
            SetAssignmentImageActive(teamIndex, ControllerRole.Placement, session.GetDevice(teamIndex, ControllerRole.Placement) != null);
        }
    }

    private void SubscribeSessionEvents()
    {
        if (isSubscribed)
            return;

        session = ControllerSessionManager.Instance;

        if (session == null)
            return;

        session.AssignmentRegistered    += OnAssignmentRegistered;
        session.RegistrationCleared     += OnRegistrationCleared;

        isSubscribed = true;

        Refresh();
    }

    private void OnAssignmentRegistered(int teamIndex, ControllerRole role, InputDevice device)
    {
        SetAssignmentImageActive(teamIndex, role, true);
    }

    private void OnRegistrationCleared()
    {
        SetAllImagesInactive();
    }

    private void SetAllImagesInactive()
    {
        if (assignmentJoinedImages == null)
            return;

        for (int index = 0; index < assignmentJoinedImages.Length; index++)
        {
            GameObject targetImage = assignmentJoinedImages[index];

            if (targetImage != null)
                targetImage.SetActive(false);
        }
    }

    private void SetAssignmentImageActive(int teamIndex, ControllerRole role, bool isActive)
    {
        int imageIndex = GetAssignmentIndex(teamIndex, role);

        if (imageIndex < 0 || imageIndex >= assignmentJoinedImages.Length)
        {
            Debug.LogWarning(
                $"[ControllerJoinView] " +
                $"Team {teamIndex + 1} / {role}に対応する" +
                "UIが設定されていません。",
                this
            );

            return;
        }

        GameObject targetImage = assignmentJoinedImages[imageIndex];

        if (targetImage == null)
        {
            Debug.LogWarning(
                $"[ControllerJoinView] " +
                $"Element {imageIndex}の画像が設定されていません。",
                this
            );

            return;
        }

        targetImage.SetActive(isActive);
    }

    private static int GetAssignmentIndex(int teamIndex, ControllerRole role)
    {
        int roleOffset = role == ControllerRole.Operation ? 0 : 1;

        return teamIndex * ControllerSessionManager.ControllerCountPerTeam + roleOffset;
    }

    private void UnsubscribeSessionEvents()
    {
        if (session == null || !isSubscribed)
            return;

        session.AssignmentRegistered    -= OnAssignmentRegistered;
        session.RegistrationCleared     -= OnRegistrationCleared;

        session         = null;
        isSubscribed    = false;
    }
}