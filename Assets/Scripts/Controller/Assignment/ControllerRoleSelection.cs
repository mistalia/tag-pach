// 1台のControllerのRole選択とOK状態を管理する
public sealed class ControllerRoleSelection
{
    public RegisteredController Controller { get; }

    public ControllerRole SelectedRole { get; private set; }

    public bool IsReady { get; private set; }

    public ControllerRoleSelection(RegisteredController controller, ControllerRole initialRole)
    {
        Controller      = controller;
        SelectedRole    = initialRole;
        IsReady         = false;
    }

    // Roleを現在とは反対へ切り替える
    public bool ToggleRole()
    {
        if (IsReady)
            return false;

        SelectedRole = SelectedRole == ControllerRole.Operation ? ControllerRole.Placement : ControllerRole.Operation;

        return true;
    }

    // 再戦準備画面などで確定したRoleを反映する
    public void ApplyConfirmedRole(ControllerRole role)
    {
        SelectedRole = role;
    }


    // 現在のRoleで準備完了にする
    public bool SetReady()
    {
        if (IsReady)
            return false;

        IsReady = true;

        return true;
    }

    // OK状態を解除して、再びRoleを変更可能にする
    public bool CancelReady()
    {
        if (!IsReady)
            return false;

        IsReady = false;

        return true;
    }

    public void ResetReady()
    {
        IsReady = false;
    }
}
