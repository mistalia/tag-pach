using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Ope_Smash", menuName = "Tutorial/State/Ope_Smash")]
public class Ope_Smash : TutorialState
{
    [Header("Setting")]
    [SerializeField] private DialogueData startDialogueData;
    [SerializeField] private DialogueData DialogueData_2;
    [SerializeField] private GameObject dummyPrefub;

    private const float left = 6.0f, right = 16.0f, y = 0.5f, z = 10.0f;

    private Dummy_Player dummyRight;
    private Dummy_Player dummyLeft;

    public override void EnterState(TutorialManager tutorialManager)
    {
        base.EnterState(tutorialManager);
        Dialogue.Instance.DisplayDialogue(startDialogueData);

        currentStep = TutorialStep.Step1;

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
            case TutorialStep.Step1: // ダイアログ1が終了 -> ミッションの表示

                if (startDialogueData.IsFinished)
                {// ダイアログが消えて且つまだミッションを表示していないならミッションを表示する
                    Mission.Instance.DisplayMission("ダミーに攻撃する");

                    dummyLeft = Instantiate(dummyPrefub).GetComponent<Dummy_Player>();
                    dummyLeft.transform.position = new Vector3(left, y, z);

                    dummyRight = Instantiate(dummyPrefub).GetComponent<Dummy_Player>();
                    dummyRight.transform.position = new Vector3(right, y, z);

                    tutorialManager.GetPlayerMag().SetSmashEnable(true);
                    tutorialManager.GetPlayerOsh().SetSmashEnable(true);

                    HelpIcon.Instance.Attack.SetActive(true);

                    ChangeStep(TutorialStep.Step2);
                }
                break;

            case TutorialStep.Step2: // ミッションのクリア判定をチェック

                if ((dummyLeft.GetAttackOwner() == tutorialManager.GetPlayerMag() && dummyRight.GetAttackOwner() == tutorialManager.GetPlayerOsh()) ||
                    (dummyLeft.GetAttackOwner() == tutorialManager.GetPlayerOsh() && dummyRight.GetAttackOwner() == tutorialManager.GetPlayerMag()))
                {
                    Mission.Instance.ClearMission();

                    HelpIcon.Instance.Attack.SetActive(false);

                    ChangeStep(TutorialStep.Step3);
                }

                break;

            case TutorialStep.Step3: // チュートリアル終了

                if (Mission.Instance.IsFinishedMission())
                {
                    Dialogue.Instance.DisplayDialogue(DialogueData_2);
                    ChangeStep(TutorialStep.Step4);
                }

                break;

            case TutorialStep.Step4:

                if(DialogueData_2.IsFinished)
                {
                    Destroy(dummyLeft.gameObject);
                    Destroy(dummyRight.gameObject);

                    tutorialManager.GetPlayerMag().SetSmashEnable(false);
                    tutorialManager.GetPlayerOsh().SetSmashEnable(false);

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