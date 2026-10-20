using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

// 入力検出と登録依頼

public sealed class ControllerJoinDetector : MonoBehaviour
{
    private IDisposable buttonPressListener;

    private void OnEnable()
    {
        buttonPressListener = InputSystem.onAnyButtonPress.Call(OnAnyButtonPressed);
    }

    private void OnDisable()
    {
        StopListening();
    }

    private void OnDestroy()
    {
        StopListening();
    }

    private void OnAnyButtonPressed(InputControl control)
    {
        if (control == null)
            return;

        InputDevice device = control.device;

        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null)
        {
            Debug.LogError("[ControllerJoinDetector] " + "ControllerSessionManagerが存在しません。");

            return;
        }

        // 登録画面で参加対象外のデバイスは無視する
        if (!IsControllerDevice(device))
            return;

        if (!session.TryRegisterDevice(device))
            return;
    }

    private void StopListening()
    {
        buttonPressListener?.Dispose();
        buttonPressListener = null;
    }

    private static bool IsControllerDevice(InputDevice device)
    {
        if (device == null)
            return false;

        // 登録画面ではマウス入力をプレイヤーとして扱わない
        return device is not Mouse;
    }
}