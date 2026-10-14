using UnityEngine;
using UnityEngine.UI;

public class TurnUI : MonoBehaviour
{
    public static TurnUI Instance { get; private set; }

    [Header("Ref")]
    [SerializeField] private Image image;
    [SerializeField] private Color defColor = Color.white;
    [SerializeField] private Text timeTex;

    void Start()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        image = GetComponent<Image>();
        if(image == null)
        {
            Debug.LogError("Image component not found on TurnUI GameObject.");
            return;
        }
        image.color = defColor;

        if(timeTex == null)
        {
            Debug.LogError("Text component for timeTex is not assigned in TurnUI.");
            return;
        }
    }

    public void SetColor(Color color)
    {
        if(image != null)
        {
            image.color = color;
        }
    }

    public void SetTimeText(string text)
    {
        if(timeTex != null)
        {
            timeTex.text = text;
        }
    }

}
