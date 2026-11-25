using UnityEngine;

[CreateAssetMenu(fileName = "PlaPlace_Tutorial", menuName = "Tutorial/State/PlaPlace_Tutorial")]
public class PlaPlace_Tutorials : TutorialState
{
    [SerializeField] private DialogueData startDialogueData;
    [SerializeField] private DialogueData DialogueData_2;

    public override void EnterState(TutorialManager tutorialManager)
    {
        base.EnterState(tutorialManager);
        currentStep = TutorialStep.Step1;
        Dialogue.Instance.DisplayDialogue(startDialogueData);
    }

    public override void UpdateState()
    {
        base.UpdateState();

        switch(currentStep)
        {
            case TutorialStep.Step1:
                // ダイアログ1の終了を検知
                if (startDialogueData.IsFinished)
                {
                    tutorialManager.GetPlayerMag().SetCanPlaceStone(true);
                    tutorialManager.GetPlayerOsh().SetCanPlaceStone(true);
                    Mission.Instance.DisplayMission("両方のプレイヤーが石を置く");
                    ChangeStep(TutorialStep.Step2);
                }
                break;

            case TutorialStep.Step2:
                var FiMa = FieldManager.Instance;
                
                HelpIcon.Instance.Place.gameObject.SetActive(true);

                bool oshPutStone = false;
                bool magPutStone = false;

                if (FiMa.GetStoneCount() >= 2)
                {
                    foreach (var tile in FiMa.GetAllStoneField())
                    {
                        if (tile.PutedStone != null)
                        {
                            if (tile.PutedStone.Owner == tutorialManager.GetPlayerOsh())
                            {// オシロの石が存在している
                                oshPutStone = true;
                            }
                            else if (tile.PutedStone.Owner == tutorialManager.GetPlayerMag())
                            {// マグニの石が存在している
                                magPutStone = true;
                            }
                        }

                        if (oshPutStone && magPutStone)
                        {
                            tutorialManager.GetPlayerMag().SetCanPlaceStone(false);
                            tutorialManager.GetPlayerOsh().SetCanPlaceStone(false);
                            Mission.Instance.ClearMission();
                            HelpIcon.Instance.Place.gameObject.SetActive(false);
                            ChangeStep(TutorialStep.Step3);
                            break;
                        }
                    }
                }
                break;

            case TutorialStep.Step3:
                if(Mission.Instance.IsFinishedMission())
                {
                    Dialogue.Instance.DisplayDialogue(DialogueData_2);
                    ChangeStep(TutorialStep.Step4);
                }
                break;

            case TutorialStep.Step4:
                if(DialogueData_2)
                {
                    FieldManager.Instance.ResetField();
                    tutorialManager.ChangeState(this.NextStateKey);
                }
                // Implement logic for Step4
                break;
            case TutorialStep.Step5:
                // Implement logic for Step5
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
