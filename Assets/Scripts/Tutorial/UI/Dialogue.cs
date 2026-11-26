using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    public static Dialogue Instance { get; private set; }

    [Header("Ref")]
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private Image speakerIcon;
    [SerializeField] private TMP_Text messageBox;

    [Header("Settings")]
    [SerializeField] private float messageSpeed = 0.3f;

    private DialogueData dialogueData;
    private DialogueLine dialogueLine;
    private int currentLineIndex;

    private bool active = false;
    public bool Active => active;

    private bool isTyping = false;
    public bool IsTyping => isTyping;
    private Coroutine TypeCoroutine;

    public bool isDisplayNow { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(this);
        }
        Instance = this;

        active = false;
        dialogueRoot.SetActive(active);
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!active) return;

        if (!dialogueData) return;


        if (TutorialManager.Instance?.GetPlayerInputMag().Place.Pressed == true || TutorialManager.Instance?.GetPlayerInputOsh().Place.Pressed == true)
        {


            if (isTyping)
            {
                if (TypeCoroutine != null)
                {
                    StopCoroutine(TypeCoroutine);
                    TypeCoroutine = null;
                }

                messageBox.maxVisibleCharacters = int.MaxValue;
                isTyping = false;
            }
            else
            {
                if (dialogueData.Count > 0 && currentLineIndex < dialogueData.Count - 1)
                {
                    ++currentLineIndex;
                    ShowLine(currentLineIndex);
                }
                else
                {
                    active = false;
                    TutorialManager.Instance?.EnablePlayer();
                    dialogueData.IsFinished = true;
                    dialogueRoot.SetActive(active);
                }
            }
        }

    }

    private void FixedUpdate()
    {

    }

    public void DisplayDialogue(DialogueData data)
    {
        //データの取得
        dialogueData = data;
        if (dialogueData == null) return;

        //データの先頭を取得
        currentLineIndex = 0;
        if (!dialogueData.TryGetLine(currentLineIndex, out dialogueLine)) return;

        ShowLine(currentLineIndex);
    }

    private bool ShowLine(int lineIndex)
    {
        if (!dialogueData.TryGetLine(currentLineIndex, out dialogueLine)) return false;

        if (!active)
        {
            active = true;
            dialogueRoot?.SetActive(active);
        }

        if (TypeCoroutine != null)
        {
            StopCoroutine(TypeCoroutine);
            TypeCoroutine = null;

            messageBox.maxVisibleCharacters = int.MaxValue;
        }

        isTyping = false;
        dialogueData.IsFinished = false;
        speakerIcon.sprite = dialogueLine.Portrait;
        messageBox.maxVisibleCharacters = 0;
        messageBox.text = dialogueLine.Message;

        messageBox.ForceMeshUpdate();
        TypeCoroutine = StartCoroutine(TypeText());
        isTyping = true;

        TutorialManager.Instance?.DisablePlayer();

        return true;
    }

    private IEnumerator TypeText()
    {
        int charCount = messageBox.textInfo.characterCount;
        var wait = new WaitForSecondsRealtime(Mathf.Max(0.001f, messageSpeed));

        for (int i = 0; i <= charCount; i++)
        {
            messageBox.maxVisibleCharacters = i;
            yield return wait;
        }

        messageBox.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
        TypeCoroutine = null;
    }
}
