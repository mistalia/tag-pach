using UnityEngine.InputSystem;

// 操作担当と配置担当の2台を、1つのコントローラーチームとして管理
public sealed class ControllerTeam
{
    public ControllerAssignment OperationAssignment { get; private set; }

    public ControllerAssignment PlacementAssignment { get; private set; }

    public bool IsComplete => OperationAssignment != null && PlacementAssignment != null;

    public bool TryAssign(InputDevice device, ControllerRole role)
    {
        if (device == null)
            return false;

        switch (role)
        {
            case ControllerRole.Operation:

                if (OperationAssignment != null)
                    return false;

                OperationAssignment = new ControllerAssignment(device, ControllerRole.Operation);

                return true;

            case ControllerRole.Placement:

                if (PlacementAssignment != null)
                    return false;

                PlacementAssignment = new ControllerAssignment(device, ControllerRole.Placement);

                return true;

            default:
                return false;
        }
    }

    public InputDevice GetDevice(ControllerRole role)
    {
        switch (role)
        {
            case ControllerRole.Operation:
                return OperationAssignment?.Device;

            case ControllerRole.Placement:
                return PlacementAssignment?.Device;

            default:
                return null;
        }
    }
}