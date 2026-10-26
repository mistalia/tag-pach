using TMPro;
using UnityEngine;

public class WarningEvent : MonoBehaviour
{
    public static WarningEvent Instance { get; private set; }

    [SerializeField] private GameObject warningEventObjTop;
    [SerializeField] private GameObject warningEventObjBottom;

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void SetWarningEventActive(bool isActive)
    {
        warningEventObjTop.SetActive(isActive);
        warningEventObjBottom.SetActive(isActive);
    }

}
