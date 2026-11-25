using UnityEngine;
using UnityEngine.UI;

public class Help_Move : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private Image fillOsh;
    [SerializeField] private Image fillMag;

    [Header("Settings")]
    [SerializeField] private float moveScale = 50f;

    private Vector2 inputOsh;
    private Vector2 inputMag;

    public void SetInput(Vector2 osh, Vector2 mag)
    {
        inputOsh = osh;
        inputMag = mag;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {

        if(fillOsh == null || fillMag == null)
        {
            Debug.LogError("Fill images are not assigned in the inspector.");
            return;
        }

        inputMag = Vector2.zero;
        inputOsh = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (fillMag == null || fillOsh == null)
        {
            return;
        }

        Vector2 mag = Vector2.ClampMagnitude(inputMag, 1f);
        Vector2 osh = Vector2.ClampMagnitude(inputOsh, 1f);

        fillMag.rectTransform.anchoredPosition = mag * moveScale;
        fillOsh.rectTransform.anchoredPosition = osh * moveScale;
    }
}
