using UnityEngine;

[CreateAssetMenu(fileName = "Tutorial_rule", menuName = "Tutorial/State/Tutorial_rule")]
public class Tutorial_rule : TutorialState
{
    [Header("Ref")]
    [SerializeField] private GameObject stonePrefub;

    [Header("Setting")]
    [SerializeField] private DialogueData startDialogueData;
    [SerializeField] private DialogueData DialogueData_2;
    [SerializeField] private DialogueData failedDialogue;
    [SerializeField] private DialogueData cancelDialogue;

    [SerializeField] private int magSampleLineIndex = 7;
    [SerializeField] private int oshSampleLineIndex = 4;

    private Vector2Int[] magSampleLine = new Vector2Int[4];
    private Vector2Int[] oshSampleLine = new Vector2Int[4];

    private bool mag_matched = false;
    private bool osh_matched = false;

    private FieldTile[] tgtTile = new FieldTile[4];

    public override void EnterState(TutorialManager tutorialManager)
    {
        base.EnterState(tutorialManager);
        currentStep = TutorialStep.Step1;
        Dialogue.Instance.DisplayDialogue(startDialogueData);
        if (stonePrefub == null)
        {
            stonePrefub = Resources.Load<GameObject>("Prefabs/Stone_Komanoko");
        }

        magSampleLine[0] = new Vector2Int(magSampleLineIndex, 1);
        magSampleLine[1] = new Vector2Int(magSampleLineIndex, 2);
        magSampleLine[2] = new Vector2Int(magSampleLineIndex, 3);
        magSampleLine[3] = new Vector2Int(magSampleLineIndex, 4);

        oshSampleLine[0] = new Vector2Int(oshSampleLineIndex, 1);
        oshSampleLine[1] = new Vector2Int(oshSampleLineIndex, 2);
        oshSampleLine[2] = new Vector2Int(oshSampleLineIndex, 3);
        oshSampleLine[3] = new Vector2Int(oshSampleLineIndex, 4);

        mag_matched = false;
        osh_matched = false;
    }

    public override void UpdateState()
    {
        base.UpdateState();
        switch (currentStep)
        {
            case TutorialStep.Step1:
                if (startDialogueData.IsFinished)
                {
                    ChangeStep(TutorialStep.Step2);
                }
                break;

            case TutorialStep.Step2:
                // サンプル列に駒を配置
                FieldManager.Instance.ResetField();
                foreach (var tileCord in magSampleLine)
                {
                    var tile = FieldManager.Instance.GetFieldTile(tileCord);

                    Stone data = Instantiate(stonePrefub).GetComponent<Stone>();
                    data.SetOwner(tutorialManager.GetPlayerMag());
                    tile.PutStone(data, "Red");
                }

                foreach (var tileCord in oshSampleLine)
                {
                    var tile = FieldManager.Instance.GetFieldTile(tileCord);

                    Stone data = Instantiate(stonePrefub).GetComponent<Stone>();
                    data.SetOwner(tutorialManager.GetPlayerOsh());
                    tile.PutStone(data, "Blue");
                }

                MatchStorage.Instance.ClearMatchData();
                Mission.Instance.DisplayMission("自分の色の駒を5個並べる");
                ChangeStep(TutorialStep.Step3);

                mag_matched = false;
                osh_matched = false;

                tutorialManager.GetPlayerMag().SetCanPlaceStone(true);
                tutorialManager.GetPlayerOsh().SetCanPlaceStone(true);

                break;

            case TutorialStep.Step3:

                //それぞれの目的マスの状況確認
                //オシロ
                tgtTile[0] = FieldManager.Instance.GetFieldTile(new Vector2Int(oshSampleLineIndex, 0));
                tgtTile[1] = FieldManager.Instance.GetFieldTile(new Vector2Int(oshSampleLineIndex, 5));

                //マグニ
                tgtTile[2] = FieldManager.Instance.GetFieldTile(new Vector2Int(magSampleLineIndex, 0));
                tgtTile[3] = FieldManager.Instance.GetFieldTile(new Vector2Int(magSampleLineIndex, 5));

                foreach (var tgt in FieldManager.Instance.GetAllStoneField())
                {
                    if (tgt.GetCoordinate() != new Vector2Int(magSampleLineIndex, 0) && tgt.GetCoordinate() != new Vector2Int(magSampleLineIndex, 1) && tgt.GetCoordinate() != new Vector2Int(magSampleLineIndex, 2) && tgt.GetCoordinate() != new Vector2Int(magSampleLineIndex, 3) && tgt.GetCoordinate() != new Vector2Int(magSampleLineIndex, 4) && tgt.GetCoordinate() != new Vector2Int(magSampleLineIndex, 5) &&
                        tgt.GetCoordinate() != new Vector2Int(oshSampleLineIndex, 0) && tgt.GetCoordinate() != new Vector2Int(oshSampleLineIndex, 1) && tgt.GetCoordinate() != new Vector2Int(oshSampleLineIndex, 2) && tgt.GetCoordinate() != new Vector2Int(oshSampleLineIndex, 3) && tgt.GetCoordinate() != new Vector2Int(oshSampleLineIndex, 4) && tgt.GetCoordinate() != new Vector2Int(oshSampleLineIndex, 5))
                    {//サンプル列以外に駒が置かれている場合はダイアログを出してリセット
                        Dialogue.Instance.DisplayDialogue(cancelDialogue);
                        ChangeStep(TutorialStep.Step6);
                    }

                    tgt.ShowColor();
                }

                // オシロマッチ確認
                if ((tgtTile[0].PutedStone != null) && (tgtTile[0].PutedStone.Owner == tutorialManager.GetPlayerOsh()) ||
                    (tgtTile[1].PutedStone != null) && (tgtTile[1].PutedStone.Owner == tutorialManager.GetPlayerOsh()))
                {
                    osh_matched = true;
                }
                else if ((tgtTile[0].PutedStone != null) && (tgtTile[0].PutedStone.Owner == tutorialManager.GetPlayerMag()) &&
                         (tgtTile[1].PutedStone != null) && (tgtTile[1].PutedStone.Owner == tutorialManager.GetPlayerMag()))
                {
                    Dialogue.Instance.DisplayDialogue(failedDialogue);
                    ChangeStep(TutorialStep.Step5);
                }

                // マグニマッチ確認
                if ((tgtTile[2].PutedStone != null) && (tgtTile[2].PutedStone.Owner == tutorialManager.GetPlayerMag()) ||
                (tgtTile[3].PutedStone != null) && (tgtTile[3].PutedStone.Owner == tutorialManager.GetPlayerMag()))
                {
                    mag_matched = true;
                }
                else if ((tgtTile[2].PutedStone != null) && (tgtTile[2].PutedStone.Owner == tutorialManager.GetPlayerOsh()) &&
                (tgtTile[3].PutedStone != null) && (tgtTile[3].PutedStone.Owner == tutorialManager.GetPlayerOsh()))
                {
                    Dialogue.Instance.DisplayDialogue(failedDialogue);
                    ChangeStep(TutorialStep.Step5);
                }

                if (osh_matched && mag_matched)
                {
                    Mission.Instance.ClearMission();
                    ChangeStep(TutorialStep.Step4);
                }
                else if (
                    tgtTile[0].PutedStone != null &&
                    tgtTile[1].PutedStone != null &&
                    tgtTile[2].PutedStone != null &&
                    tgtTile[3].PutedStone != null)
                {
                    Dialogue.Instance.DisplayDialogue(failedDialogue);
                    ChangeStep(TutorialStep.Step5);
                }

                break;

            case TutorialStep.Step4:
                // 両方が目標達成
                if (Mission.Instance.IsFinishedMission())
                {
                    tutorialManager.GetPlayerMag().SetCanPlaceStone(false);
                    tutorialManager.GetPlayerOsh().SetCanPlaceStone(false);
                    Dialogue.Instance.DisplayDialogue(DialogueData_2);
                    ChangeStep(TutorialStep.Step7);
                }
                break;

            case TutorialStep.Step5:
                // 詰み防止分岐
                if (failedDialogue.IsFinished)
                {
                    ChangeStep(TutorialStep.Step2);
                }
                break;

            case TutorialStep.Step6:
                // ターゲット以外の場所に置かれている場合はリセット分岐
                if (cancelDialogue.IsFinished)
                {
                    ChangeStep(TutorialStep.Step2);
                }
                break;

            case TutorialStep.Step7:

                if (DialogueData_2.IsFinished)
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
