using UnityEngine;

public class HelpIcon : MonoBehaviour
{
    public static HelpIcon Instance { get; private set; }

    // インスタンス一覧
    [Header("Ref")]
    [SerializeField] private Help_Move move; public Help_Move Move => move;
    [SerializeField] private GameObject place; public GameObject Place => place;
    [SerializeField] private GameObject attack; public GameObject Attack => attack;
    [SerializeField] private GameObject forcePlace; public GameObject ForcePlace => forcePlace;
    [SerializeField] private GameObject scan; public GameObject Scan => scan;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        // 全非表示
        move.gameObject.SetActive(false);
        place.gameObject.SetActive(false);
        attack.gameObject.SetActive(false);
        forcePlace.gameObject.SetActive(false);
        scan.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        move = null;
        place = null;
    }
}