using UnityEngine.InputSystem;

// Team内の2台のControllerと出撃可能状態を管理する
public sealed class ControllerTeam
{
    public ControllerRoleSelection FirstControllerSelection { get; private set; }

    public ControllerRoleSelection SecondControllerSelection { get; private set; }

    public ControllerAssignment OperationAssignment { get; private set; }

    public ControllerAssignment PlacementAssignment { get; private set; }

    public RegisteredController FirstController => FirstControllerSelection?.Controller;

    public RegisteredController SecondController => SecondControllerSelection?.Controller;

    public bool HasBothControllers => FirstController != null && SecondController != null;

    public bool AreAllReady => FirstControllerSelection != null &&
                               SecondControllerSelection != null &&
                               FirstControllerSelection.IsReady &&
                               SecondControllerSelection.IsReady;

    public bool AreRolesValid => FirstControllerSelection != null &&
                                 SecondControllerSelection != null &&
                                 FirstControllerSelection.SelectedRole != SecondControllerSelection.SelectedRole;

    public bool CanDeploy => AreAllReady && AreRolesValid;

    public bool TryAddController(RegisteredController controller)
    {
        if (controller == null || controller.Device == null)
            return false;

        if (FirstControllerSelection == null)
        {
            FirstControllerSelection = new ControllerRoleSelection(controller, ControllerRole.Operation);
            return true;
        }

        if (SecondControllerSelection == null)
        {
            SecondControllerSelection = new ControllerRoleSelection(controller, ControllerRole.Placement);
            return true;
        }

        return false;
    }

    public ControllerRoleSelection GetSelection(int controllerNumber)
    {
        if (FirstController?.ControllerNumber == controllerNumber)
            return FirstControllerSelection;

        if (SecondController?.ControllerNumber == controllerNumber)
            return SecondControllerSelection;

        return null;
    }

    public bool UpdateAssignments()
    {
        ClearAssignments();

        if (!CanDeploy)
            return false;

        CreateAssignments();

        return true;
    }

    public InputDevice GetDevice(ControllerRole role)
    {
        return role == ControllerRole.Operation ? OperationAssignment?.Device : PlacementAssignment?.Device;
    }

    // TwoControllersTestでは1台で両Roleを担当する
    public bool ConfigureSingleControllerMode()
    {
        if (FirstController == null || FirstController.Device == null)
            return false;

        OperationAssignment = new ControllerAssignment(FirstController.Device, ControllerRole.Operation);
        PlacementAssignment = new ControllerAssignment(FirstController.Device, ControllerRole.Placement);

        return true;
    }

    public void Reset()
    {
        FirstControllerSelection = null;
        SecondControllerSelection = null;

        ClearAssignments();
    }

    private void ClearAssignments()
    {
        OperationAssignment = null;
        PlacementAssignment = null;
    }

    // 再戦準備画面で確定したRole構成を反映する
    public bool ApplyConfirmedRoles(ControllerRole firstControllerRole, ControllerRole secondControllerRole)
    {
        if (!HasBothControllers)
            return false;

        if (firstControllerRole == secondControllerRole)
            return false;

        FirstControllerSelection.ApplyConfirmedRole(firstControllerRole);
        SecondControllerSelection.ApplyConfirmedRole(secondControllerRole);

        ClearAssignments();
        CreateAssignments();

        return true;
    }

    private void CreateAssignments()
    {
        ControllerRoleSelection operationSelection;
        ControllerRoleSelection placementSelection;

        if (FirstControllerSelection.SelectedRole == ControllerRole.Operation)
        {
            operationSelection = FirstControllerSelection;
            placementSelection = SecondControllerSelection;
        }
        else
        {
            operationSelection = SecondControllerSelection;
            placementSelection = FirstControllerSelection;
        }

        OperationAssignment = new ControllerAssignment(operationSelection.Controller.Device, ControllerRole.Operation);
        PlacementAssignment = new ControllerAssignment(placementSelection.Controller.Device, ControllerRole.Placement);
    }

    public void ResetReady()
    {
        FirstControllerSelection?.ResetReady();
        SecondControllerSelection?.ResetReady();
    }
}