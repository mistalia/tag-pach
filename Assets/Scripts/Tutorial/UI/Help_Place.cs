using UnityEngine;
using UnityEngine.UI;

public class Help_Place : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image helpIcon_left;
    [SerializeField] private Image helpIcon_right;

    [Header("Settings")]
    [SerializeField] private Sprite releaseImage;
    [SerializeField] private Sprite pressedImage;

    void Start()
    {
        helpIcon_left.sprite = releaseImage;
        helpIcon_right.sprite = releaseImage;
    }
}
