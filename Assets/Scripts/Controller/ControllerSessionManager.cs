using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// バインド画面で登録された入力デバイスの管理

public sealed class ControllerSessionManager : MonoBehaviour
{
    public static ControllerSessionManager Instance { get; private set; }

    // 登録可能な最大プレイヤー数
    public const int MaxPlayerCount = 4;

    // 入力デバイスをプレイヤー登録順に保持する。
    private readonly List<InputDevice> registeredDevices = new();

    // デバイス登録完了時に通知される。
    public event Action<int, InputDevice> DeviceRegistered;

    // 登録情報がすべて削除されたときに通知される。
    public event Action RegistrationCleared;

    // 現在の登録人数
    public int PlayerCount => registeredDevices.Count;

    // 登録上限に達しているか
    public bool IsFull => PlayerCount >= MaxPlayerCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ゲームシーンへ登録情報を引き継ぐ
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // 入力デバイスを次の空きプレイヤーへ登録する。
    public bool TryRegisterDevice(InputDevice device)
    {
        if (!CanRegisterDevice(device))
            return false;

        // Addされた位置がそのままPlayer番号になる
        registeredDevices.Add(device);

        int playerIndex = registeredDevices.Count - 1;

        Debug.Log(
            $"[ControllerSessionManager] " +
            $"Player {playerIndex + 1}に登録しました。" +
            $"\n名前: {device.displayName}" +
            $"\n型: {device.GetType().Name}" +
            $"\nID: {device.deviceId}"
        );

        // UIなどに登録完了を通知する
        DeviceRegistered?.Invoke(playerIndex, device);

        return true;
    }

    // 指定されたデバイスが登録済みか確認する
    public bool IsRegistered(InputDevice device)
    {
        return device != null && registeredDevices.Contains(device);
    }

    // プレイヤー番号から入力デバイスを取得する
    public InputDevice GetDevice(int playerIndex)
    {
        if (!IsValidRegisteredPlayerIndex(playerIndex))
            return null;

        return registeredDevices[playerIndex];
    }

    // 登録済みデバイスをすべて削除する
    public void ClearRegistration()
    {
        if (registeredDevices.Count == 0)
            return;

        registeredDevices.Clear();

        Debug.Log(
            "[ControllerSessionManager] " +
            "すべてのコントローラー登録を解除しました。"
        );

        // UIへ登録解除を通知する
        RegistrationCleared?.Invoke();
    }

    // 指定デバイスが登録可能か確認する
    private bool CanRegisterDevice(InputDevice device)
    {
        if (device == null)
        {
            Debug.LogWarning(
                "[ControllerSessionManager] " +
                "nullの入力デバイスは登録できません。"
            );

            return false;
        }

        // 同じコントローラーの複数登録を防ぐ
        if (IsRegistered(device))
            return false;

        if (IsFull)
        {
            Debug.LogWarning(
                $"[ControllerSessionManager] " +
                $"登録可能人数は、{MaxPlayerCount}人までです。"
            );

            return false;
        }

        return true;
    }

    // 指定されたプレイヤー番号が登録範囲内か確認する
    private bool IsValidRegisteredPlayerIndex(int playerIndex)
    {
        return playerIndex >= 0 && playerIndex < registeredDevices.Count;
    }
}