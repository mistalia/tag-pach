using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ResultSceneController : MonoBehaviour
{
    private static (Player Winner, Player Loser, int Wins, int Losses)? pendingResult;

    [SerializeField] private CanvasGroup presentation;
    [SerializeField] private Button nextButton;
    [SerializeField] private Image winnerPortrait;
    [SerializeField] private Image winnerPartner;
    [SerializeField] private Image opponentPortrait;
    [SerializeField] private Text caption;
    [SerializeField] private Text score;
    [SerializeField] private Sprite oshiro;
    [SerializeField] private Sprite magnifica;
    [SerializeField] private GameObject oshiroModel;
    [SerializeField] private GameObject magnificaModel;
    [SerializeField, Min(0)] private float entranceDuration = 0.4f;
    [SerializeField, Min(0)] private float exitDuration = 0.2f;

    private bool canContinue;
    private float? savedTimeScale;

    private void OnDestroy()
    {
        if (savedTimeScale.HasValue) Time.timeScale = savedTimeScale.Value;
    }

    public static void Record(Player winner, Player loser, int wins, int losses)
    {
        pendingResult = (winner, loser, wins, losses);
    }

    private void OnEnable() => nextButton.onClick.AddListener(ContinueToTitle);
    private void OnDisable() => nextButton.onClick.RemoveListener(ContinueToTitle);

    private IEnumerator Start()
    {
        var result = pendingResult;
        pendingResult = null;
        if (result.HasValue)
        {
            Sprite portrait = result.Value.Winner.ResultPortrait;
            bool isOshiro = portrait == oshiro;
            bool isMagnifica = portrait == magnifica;
            caption.text = isOshiro ? "[TagPach] マグニフィカと勝負！ 絶対勝つ！！"
                : isMagnifica ? "[TagPach] オシロくんたちと勝負！ 絶対勝つ！！"
                : "[TagPach] 対戦終了";
            score.text = $"{result.Value.Wins}  -  {result.Value.Losses}";

            Scene field = result.Value.Winner.gameObject.scene;
            Camera fieldCamera = null;
            foreach (var root in field.GetRootGameObjects())
            {
                foreach (var canvas in root.GetComponentsInChildren<Canvas>()) canvas.enabled = false;
                foreach (var events in root.GetComponentsInChildren<EventSystem>()) events.gameObject.SetActive(false);
                foreach (var listener in root.GetComponentsInChildren<AudioListener>()) listener.enabled = false;
                foreach (var camera in root.GetComponentsInChildren<Camera>())
                {
                    if (camera.CompareTag("MainCamera")) fieldCamera = camera;
                    camera.enabled = false;
                }
            }
            foreach (var root in gameObject.scene.GetRootGameObjects())
                foreach (var light in root.GetComponentsInChildren<Light>()) light.enabled = false;

            savedTimeScale = Time.timeScale;
            Time.timeScale = 0;
            result.Value.Winner.enabled = false;
            result.Value.Loser.enabled = false;
            winnerPartner.enabled = false;
            Canvas.ForceUpdateCanvases();
            ResultFieldView.Show(winnerPortrait, result.Value.Winner, true, isOshiro ? oshiroModel : magnificaModel, fieldCamera);
            ResultFieldView.Show(opponentPortrait, result.Value.Loser, false, isOshiro ? magnificaModel : oshiroModel, fieldCamera);
        }

        presentation.interactable = false;
        yield return Fade(0, 1, entranceDuration);
        presentation.interactable = true;
        canContinue = true;
        EventSystem.current?.SetSelectedGameObject(nextButton.gameObject);
    }

    private void ContinueToTitle()
    {
        if (!canContinue)
            return;

        canContinue = false;
        presentation.interactable = false;
        StartCoroutine(LoadTitle());
    }

    private IEnumerator LoadTitle()
    {
        yield return Fade(1, 0, exitDuration);
        yield return SceneManager.LoadSceneAsync("TitleScene");
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        presentation.alpha = from;
        for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            presentation.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        presentation.alpha = to;
    }
}
