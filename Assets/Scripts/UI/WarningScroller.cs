using System.Drawing;
using TMPro;
using UnityEngine;

public class WarningScroller : MonoBehaviour
{
    [SerializeField] private TMP_Text warningText_1;
    [SerializeField] private TMP_Text warningText_2;

    [SerializeField] private string warningMessage = "WARNING: Hacking!!!";

    [SerializeField] private float ScrollSpeed = -128f;
    [SerializeField] private float Gap = 32f;

    private RectTransform bandRect;
    private float segmentWidth;
    private float offset;
    private float previousWidth;
    private bool initialized;


    private void OnEnable()
    {
        initialized = false;
        offset = 0f;
        previousWidth = -1f;
    }

    private void LateUpdate()
    {
        if(warningText_1 == null || warningText_2 == null)
        {
            return;
        }

        if(bandRect == null)
        {
            bandRect = this.GetComponent<RectTransform>();
        }

        float width = bandRect.rect.width;

        if(width <= 0f)
        {
            return;
        }

        if(!initialized || width != previousWidth)
        {
            BuildText(width);
        }

        offset = Mathf.Repeat(offset + (ScrollSpeed * Time.deltaTime), segmentWidth);
        
        warningText_1.rectTransform.anchoredPosition = new Vector2(offset - segmentWidth, 0f);
        warningText_2.rectTransform.anchoredPosition = new Vector2(offset, 0f);

    }

    private void BuildText(float bandWidth)
    {
        ConfigureText(warningText_1);
        ConfigureText(warningText_2);

        string unit = string.IsNullOrEmpty(warningMessage) ? "WARNING!!!" : warningMessage;

        string content = unit;
        float textWidth = warningText_1.GetPreferredValues(content).x;

        for(int i = 0; textWidth < bandWidth && i < 256;i++)
        {
            content += " " + unit;
            textWidth = warningText_1.GetPreferredValues(content).x;
        }

        segmentWidth = Mathf.Max(textWidth, bandWidth + Gap);

        warningText_1.text = content;
        warningText_2.text = content;

        warningText_1.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, segmentWidth);
        warningText_2.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, segmentWidth);

        offset = 0f;
        previousWidth = bandWidth;
        initialized = true;
    }

    private void ConfigureText(TMP_Text label)
    {
        label.enableAutoSizing = false;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.raycastTarget = false;

        RectTransform rect = label.rectTransform;

        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.sizeDelta = Vector2.zero;
    }

}
