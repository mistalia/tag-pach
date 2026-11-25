using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Mission : MonoBehaviour
{
    public static Mission Instance { get; private set; }

    [Header("Ref")]
    [SerializeField] private Image Border;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Image Check;

    [Header("Setting")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float checkWaveDuration = 0.5f;
    [SerializeField] private float checkWaveScale = 1.5f;

    private bool isMissionActive = false;
    private bool isCheckScaling = false;
    private bool isPlayed = false;

    private float missionFadeTimer = 0f;
    private float checkWaveTimer = 0f;

    private bool finishedMission = false;
    public bool IsFinishedMission() => finishedMission;

    void Start()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        Instance = this;

        if (Border == null || text == null || Check == null)
        {
            Debug.LogError("Mission: UI references are not assigned.");
        }

        Border.gameObject.SetActive(false);
        text.gameObject.SetActive(false);
        Check.gameObject.SetActive(false);
        finishedMission = false;
        isPlayed = false;
    }

    void Update()
    {
        if(isCheckScaling)
        {
            if (isCheckScaling)
            {
                checkWaveTimer += Time.deltaTime;

                float progress = checkWaveDuration <= 0f
                    ? 1f
                    : Mathf.Clamp01(checkWaveTimer / checkWaveDuration);

                float scale = Mathf.Lerp(checkWaveScale, 1f, progress);

                Check.transform.localScale = new Vector3(scale, scale, 1f);

                if (progress >= 1f)
                {
                    // ÅIƒTƒCƒY‚ðŠm’è‚µAk¬‚ð’âŽ~
                    Check.transform.localScale = Vector3.one;
                    isCheckScaling = false;

                    // Š®—¹‚µ‚½uŠÔ‚É1‰ñ‚¾‚¯ŒÄ‚Ô
                    HideMission();
                }
            }
        }

        if (!isMissionActive && isPlayed)
        {
            if(missionFadeTimer > 0f)
            {
                missionFadeTimer -= Time.deltaTime;
                float alpha = Mathf.Clamp01(missionFadeTimer / fadeOutDuration);
                SetUIAlpha(alpha);
            }
            else
            {
                Border.gameObject.SetActive(false);
                text.gameObject.SetActive(false);
                Check.gameObject.SetActive(false);
                finishedMission = true;
            }
        }
    }

    public void DisplayMission(string missionText)
    {
        Border.gameObject.SetActive(true);
        text.gameObject.SetActive(true);
        SetUIAlpha(1.0f);
        text.text = missionText;
        isMissionActive = true;
        finishedMission = false;
        isPlayed = true;
    }

    public void ClearMission()
    {
        if(!isMissionActive || isCheckScaling)
        {
            return;
        }

        Check.gameObject.SetActive(true);
        Check.transform.localScale = new Vector3(checkWaveScale, checkWaveScale, 1f);
        checkWaveTimer = 0.0f;
        isCheckScaling = true;
        finishedMission = false;
    }

    public void HideMission()
    {
        isMissionActive = false;
        missionFadeTimer = fadeOutDuration;
    }

    private void SetUIAlpha(float alpha)
    {
        Color borderColor = Border.color;
        borderColor.a = alpha;
        Border.color = borderColor;
        Color textColor = text.color;
        textColor.a = alpha;
        text.color = textColor;
        Color checkColor = Check.color;
        checkColor.a = alpha;
        Check.color = checkColor;
    }
}
