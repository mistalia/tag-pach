using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameEndUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject messageRoot;

    [Header("Message")]
    [SerializeField] private string winFormat = "{0} WIN!";

    [SerializeField] private string scoreFormat = "{0} - {1}";

    [Header("Duration")]
    [SerializeField, Min(0f)]
    private float initialDelay = 0.5f;

    [SerializeField, Min(0f)]
    private float winDuration = 3f;

    private Coroutine displayCoroutine;
    private Action onComplete;

    private void Awake()
    {
        if (messageRoot != null)
        {
            messageRoot.SetActive(false);
        }
    }

    // 結果UI表示開始
    public void Show(
    Player winner,
    Player player1,
    Player player2,
    int player1Wins,
    int player2Wins,
    Action completeCallback = null)
    {
        Hide();

        onComplete = completeCallback;

        if (messageText == null || messageRoot == null ||
            winner == null || player1 == null || player2 == null)
        {
            Debug.LogError("GameEndUIの参照が未設定です。", this);

            Action callback = onComplete;
            onComplete = null;
            callback?.Invoke();
            return;
        }

        displayCoroutine = StartCoroutine(
            ShowSequence(
                winner,
                player1,
                player2,
                player1Wins,
                player2Wins));
    }

    private IEnumerator ShowSequence(
    Player winner,
    Player player1,
    Player player2,
    int player1Wins,
    int player2Wins)
    {
        yield return new WaitForSecondsRealtime(initialDelay);

        // PlayerのTeamColorを取得
        string p1Color =
            ColorUtility.ToHtmlStringRGB(player1.TeamColor);

        string p2Color =
            ColorUtility.ToHtmlStringRGB(player2.TeamColor);

        // 勝者の情報
        bool isPlayer1Winner = winner == player1;

        string winnerName =
            isPlayer1Winner ? "Player1" : "Player2";

        string winnerColor =
            isPlayer1Winner ? p1Color : p2Color;

        // 勝者表示
        messageText.text =
            $"<color=#{winnerColor}>{winnerName} WIN!</color>";

        messageRoot.SetActive(true);

        yield return new WaitForSecondsRealtime(winDuration);

        // 勝利数表示
        messageText.text =
            $"<color=#{p1Color}>{player1Wins}</color>" +
            " - " +
            $"<color=#{p2Color}>{player2Wins}</color>";

        displayCoroutine = null;

        // 表示完了通知
        Action callback = onComplete;
        onComplete = null;
        callback?.Invoke();
    }

    // UI表示終了
    public void Hide()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }

        onComplete = null;

        if (messageRoot != null)
        {
            messageRoot.SetActive(false);
        }
    }

    private void OnDisable()
    {
        Hide();
    }
}