using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(
    fileName = "End_Tutorial",
    menuName = "Tutorial/State/End_Tutorial")]
public class End_Tutorial : TutorialState
{
    [Header("Setting")]
    [SerializeField] private DialogueData StartDialogue;
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private string sceneName;

    public override void EnterState(TutorialManager tutorialManager)
    {
        base.EnterState(tutorialManager);

        currentStep = TutorialStep.Step1;
        Dialogue.Instance.DisplayDialogue(StartDialogue);
    }

    public override void UpdateState()
    {
        base.UpdateState();

        switch(currentStep)
        {
            case TutorialStep.Step1:
                if (StartDialogue.IsFinished)
                {
                    SceneTransitionManager.Instance.LoadScene(sceneName, transitionSettings);
                }
                break;
            case TutorialStep.Step2:

                break;
        }
    }

    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
    }

    public override void LateUpdateState()
    {
        base.LateUpdateState();
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}
