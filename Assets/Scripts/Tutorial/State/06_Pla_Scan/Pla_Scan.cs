using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Pla_Scan",
    menuName = "Tutorial/State/Pla_Scan")]
public class Pla_Scan : TutorialState
{
    [Header("setting")]
    [SerializeField] private DialogueData startDialogue;
    [SerializeField] private DialogueData resetDialogue;
    [SerializeField] private DialogueData endDialogue;
    [SerializeField] private GameObject stonePrefub;

    static readonly int[][] math = new int[5][] {
        new int[5] {1, 1, 2, 1, 2},
        new int[5] {2, 1, 2, 1, 1},
        new int[5] {2, 1, 2, 1, 2},
        new int[5] {2, 2, 1, 1, 2},
        new int[5] {1, 2, 2, 2, 2}
    };

    private static readonly Vector2Int offset = new Vector2Int(3, 1);

    private Stone tempStone;

    private bool magPuted = false;
    private bool oshPuted = false;

    private List<FieldTile> defTIle;

    public override void EnterState(TutorialManager tutorialManager)
    {
        base.EnterState(tutorialManager);

        if (stonePrefub == null)
        {
            stonePrefub = Resources.Load<GameObject>("Prefabs/Stone_Komanoko");
            return;
        }

        Dialogue.Instance.DisplayDialogue(startDialogue);
        currentStep = TutorialStep.Step1;
        magPuted = false;
        oshPuted = false;
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
                }
                break;

            case TutorialStep.Step2:
                // 仮駒配置用ステート
                FieldManager.Instance.ResetField();

                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 5; y++)
                    {
                        tempStone = Instantiate(stonePrefub).GetComponent<Stone>();
                        if (math[y][x] == 1)
                        {
                            tempStone.SetOwner(tutorialManager.GetPlayerMag());
                            FieldManager.Instance.GetFieldTile(new Vector2Int(x + offset.x, (4 - y) + offset.y)).PutStone(tempStone, "Red");
                        }
                        if (math[y][x] == 2)
                        {
                            tempStone.SetOwner(tutorialManager.GetPlayerOsh());
                            FieldManager.Instance.GetFieldTile(new Vector2Int(x + offset.x, (4 - y) + offset.y)).PutStone(tempStone, "Blue");
                        }

                        FieldManager.Instance.GetFieldTile(new Vector2Int(x + offset.x, (4 - y) + offset.y)).ResetTileColor();
                        tempStone.ResetColor();
                    }
                }


                defTIle = FieldManager.Instance.GetAllStoneField();

                MatchStorage.Instance.ClearMatchData();
                Mission.Instance.DisplayMission("自分の色の駒を5個並べる");
                ChangeStep(TutorialStep.Step3);

                tutorialManager.GetPlayerMag().SetCanPlaceStone(true);
                tutorialManager.GetPlayerOsh().SetCanPlaceStone(true);

                HelpIcon.Instance.Scan.SetActive(true);

                magPuted = false;
                oshPuted = false;

                break;

            case TutorialStep.Step3:

                List<FieldTile> currentTile = FieldManager.Instance.GetAllStoneField();
                bool already = false;
                foreach (var tile in currentTile)
                {
                    already = false;
                    foreach (var def in defTIle)
                    {
                        if (tile == def)
                        {
                            already = true;
                            break;
                        }

                    }

                    if(already)
                    {
                        continue;
                    }

                    Debug.Log($"Put Stone at {tile.GetCoordinate()}");
                    if ((tile.GetCoordinate().x == offset.x + 3 && tile.GetCoordinate().y == offset.y + 5) && tile.PutedStone.Owner == tutorialManager.GetPlayerMag())
                    {
                        magPuted = true;
                    }
                    else if ((tile.GetCoordinate().x == offset.x + 5 && tile.GetCoordinate().y == offset.y + 0) && tile.PutedStone.Owner == tutorialManager.GetPlayerOsh())
                    {
                        oshPuted = true;
                    }
                    else
                    {
                        Dialogue.Instance.DisplayDialogue(resetDialogue);
                        ChangeStep(TutorialStep.Step5);
                    }

                }

                if(magPuted && oshPuted)
                {
                    Mission.Instance.ClearMission();
                    HelpIcon.Instance.Scan.SetActive(false);
                    ChangeStep(TutorialStep.Step4);
                }

                break;

            case TutorialStep.Step4:
                if (Mission.Instance.IsFinishedMission())
                {
                    Dialogue.Instance.DisplayDialogue(endDialogue);
                    ChangeStep(TutorialStep.Step10);
                }
                break;

            case TutorialStep.Step5:
                if (resetDialogue.IsFinished)
                {
                    ChangeStep(TutorialStep.Step2);
                }
                break;

            case TutorialStep.Step10:
                if (endDialogue.IsFinished)
                {
                    FieldManager.Instance.ResetField();
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
