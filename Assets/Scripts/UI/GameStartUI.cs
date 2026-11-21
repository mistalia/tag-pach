using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameStartUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject messageRoot;

    [Header("Message")]
    [SerializeField, TextArea]
    private string ruleMessage =
        "自分のチームの駒を5つ並べたら勝ち！";

    [SerializeField] private string startMessage = "スタート！";

    [Header("Duration")]
    [SerializeField, Min(0f)] private float initialDelay = 0.5f;
    [SerializeField, Min(0.1f)] private float ruleDuration = 3f;
    [SerializeField, Min(0.1f)] private float startDuration = 0.8f;

    private Coroutine displayCoroutine;
    private Action onComplete;

    private void Awake()
    {
        if (messageRoot != null)
        {
            messageRoot.SetActive(false);
        }
    }

    // UI表示開始
    public void Show(Action completeCallback = null)
    {
        if (messageText == null || messageRoot == null)
        {
            Debug.LogError(
                "Message TextまたはMessage Rootが未設定です。",
                this
            );

            completeCallback?.Invoke();
            return;
        }

        // 前回の表示を停止
        Hide();

        onComplete = completeCallback;
        displayCoroutine = StartCoroutine(ShowSequence());
    }

    private IEnumerator ShowSequence()
    {
        yield return new WaitForSecondsRealtime(initialDelay);

        messageText.text = ruleMessage;
        messageRoot.SetActive(true);

        yield return new WaitForSecondsRealtime(ruleDuration);

        messageText.text = startMessage;

        yield return new WaitForSecondsRealtime(startDuration);

        messageRoot.SetActive(false);
        displayCoroutine = null;

        // 完了通知
        Action callback = onComplete;
        onComplete = null;
        callback?.Invoke();
    }

    // UI表示停止
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

    //private void OnEnable()
    //{
    //    if (messageText == null || messageRoot == null)
    //    {
    //        Debug.LogError("Message TextまたはMessage Rootが未設定です。", this);
    //        return;
    //    }

    //    if (messageRoot != null)
    //    {
    //        messageRoot.SetActive(false);
    //    }

    //    displayCoroutine = StartCoroutine(ShowSequence());
    //}

    //private void OnDisable()
    //{
    //    if (displayCoroutine != null)
    //    {
    //        StopCoroutine(displayCoroutine);
    //        displayCoroutine = null;
    //    }

    //    if (messageRoot != null)
    //    {
    //        messageRoot.SetActive(false);
    //    }
    //}
}