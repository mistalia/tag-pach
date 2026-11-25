using UnityEngine;

[CreateAssetMenu(fileName = "Ope_Force", menuName = "Tutorial/State/Ope_Force")]
public class Ope_Force : TutorialState
{
    [Header("Setting")]
    [SerializeField] private DialogueData startDialogue;
    [SerializeField] private DialogueData Dialogue2;
    [SerializeField] private GameObject dummyPrefub;


    private const float left = 6.0f, right = 16.0f, y = 0.5f, z = 10.0f;

    private Dummy_Player dummyRight;
    private Dummy_Player dummyLeft;


    public override void EnterState(TutorialManager tutorialManager)
    {
        base.EnterState(tutorialManager);
        Dialogue.Instance.DisplayDialogue(startDialogue);
        ChangeStep(TutorialStep.Step1);

        if (dummyPrefub == null)
        {
            dummyPrefub = Resources.Load<GameObject>("Prefubs/Tutorial/Dummy");
        }
    }

    public override void UpdateState()
    {
        base.UpdateState();

        switch (currentStep)
        {
            case TutorialStep.Step1:
                if (startDialogue.IsFinished)
                {
                    ChangeStep(TutorialStep.Step2);

                    dummyLeft = Instantiate(dummyPrefub).GetComponent<Dummy_Player>();
                    dummyLeft.transform.position = new Vector3(left, y, z);

                    dummyRight = Instantiate(dummyPrefub).GetComponent<Dummy_Player>();
                    dummyRight.transform.position = new Vector3(right, y, z);

                    tutorialManager.GetPlayerMag().SetForcePlaceEnable(true);
                    tutorialManager.GetPlayerOsh().SetForcePlaceEnable(true);

                    HelpIcon.Instance.ForcePlace.SetActive(true);

                    Mission.Instance.DisplayMission("É_É~Å[Ç…çUåÇÇ∑ÇÈ");
                }
                break;

            case TutorialStep.Step2:

                if ((dummyLeft.GetAttackOwner() == tutorialManager.GetPlayerMag() && dummyRight.GetAttackOwner() == tutorialManager.GetPlayerOsh()) ||
                    (dummyLeft.GetAttackOwner() == tutorialManager.GetPlayerOsh() && dummyRight.GetAttackOwner() == tutorialManager.GetPlayerMag()))
                {
                    Mission.Instance.ClearMission();

                    tutorialManager.GetPlayerMag().SetForcePlaceEnable(false);
                    tutorialManager.GetPlayerOsh().SetForcePlaceEnable(false);

                    HelpIcon.Instance.ForcePlace.SetActive(false);

                    ChangeStep(TutorialStep.Step3);
                }
                break;

            case TutorialStep.Step3:
                if (Mission.Instance.IsFinishedMission())
                {
                    Dialogue.Instance.DisplayDialogue(Dialogue2);
                    ChangeStep(TutorialStep.Step4);
                }
                break;

            case TutorialStep.Step4:
                if(Dialogue2.IsFinished)
                {
                    Destroy(dummyLeft.gameObject);
                    Destroy(dummyRight.gameObject);

                    tutorialManager.ChangeState(this.NextStateKey);
                }
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
