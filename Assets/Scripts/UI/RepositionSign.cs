using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RepositionSign : MonoBehaviour
{
    public static RepositionSign Instance { get; private set; }

    [Header("Ref")]
    [SerializeField] private GameObject sign;
    [SerializeField] private GameObject inputSign_1;
    [SerializeField] private GameObject inputSign_2;
    [SerializeField] private GameObject guidObj;
    [SerializeField] private Slider turnTimerSlider;

    [Header("Sign Setting")]
    [SerializeField] private List<string> signTextList = new List<string>();
    [SerializeField] private float signTextChangeInterval = 1.0f;
    [SerializeField] private float signFlashInterval = 0.5f;

    private Image signImg;
    private TMP_Text signTex;

    private Image inputSign_1Img;
    private TMP_Text inputSign_1Tex;

    private Image inputSign_2Img;
    private TMP_Text inputSign_2Tex;

    private bool isSignActive = false;
    public bool SignActive
    {
        get { return isSignActive; }
        set
        {
            isSignActive = value;
            sign.SetActive(isSignActive);
            inputSign_1.SetActive(isSignActive);
            inputSign_2.SetActive(isSignActive);
            guidObj.SetActive(isSignActive);
            turnTimerSlider.gameObject.SetActive(isSignActive);

            if (!isSignActive)
            {
                SetInputSignColor(1, Color.gray, "Waiting...");
                SetInputSignColor(2, Color.gray, "Waiting...");
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        if (sign == null || inputSign_1 == null || inputSign_2 == null || guidObj == null)
        {
            Debug.LogError("One or more GameObject references are not assigned in RepiositionSign.");
            return;
        }

        if(turnTimerSlider == null)
        {
            Debug.LogError("Turn Timer Slider reference is not assigned in RepiositionSign.");
            return;
        }

        if (signTextList.Count ==0)
        {
            signTextList.Add("Meeting Time.");
            signTextList.Add("Meeting Time..");
            signTextList.Add("Meeting Time...");
        }

        signImg = sign.GetComponent<Image>();
        signTex = sign.GetComponentInChildren<TMP_Text>();

        inputSign_1Img = inputSign_1.GetComponent<Image>();
        inputSign_1Tex = inputSign_1.GetComponentInChildren<TMP_Text>();

        inputSign_2Img = inputSign_2.GetComponent<Image>();
        inputSign_2Tex = inputSign_2.GetComponentInChildren<TMP_Text>();

        this.SignActive = false; 
    }

    // Update is called once per frame
    void Update()
    {
        if(isSignActive)
        {
            signTex.text = signTextList[(int)(Time.time / signTextChangeInterval) % signTextList.Count];
            var alpha = (Mathf.PingPong(Time.time / signFlashInterval, 1.0f) * 0.8f) + 0.2f;
            signImg.color = new Color(signImg.color.r, signImg.color.g, signImg.color.b, alpha);
            signTex.color = new Color(signTex.color.r, signTex.color.g, signTex.color.b, alpha);
        }
    }

    public void SetInputSignColor(int playerIndex, Color color,string text)
    {
        Debug.Log($"SetInputSignColor called for player {playerIndex} with color {color} and text '{text}'");
        if (playerIndex == 1)
        {
            inputSign_1Img.color = color;
            inputSign_1Tex.text = text;
        }
        else if(playerIndex == 2)
        {
            inputSign_2Img.color = color;
            inputSign_2Tex.text = text;
        }
    }

    public void SetTurnTimer(float value,float maxValue)
    {
        turnTimerSlider.value = value;
        turnTimerSlider.maxValue = maxValue;
    }
}
