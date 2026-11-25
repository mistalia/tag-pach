using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "OpeMove_Tutorial", menuName = "Tutorial/State/OpeMove_Tutorial")]
public class OpeMove_Tutorial : TutorialState
{
    [SerializeField] private DialogueData startDialogueData;
    [SerializeField] private DialogueData DialogueData_2;

    [SerializeField] private Vector2Int[] lightingTileCordinate;
    [SerializeField] private float lightColorChangeInterval = 0.5f;

    private bool mag_OnTile = false;
    private bool osh_OnTile = false;

    private float[] lightTimer = new float[2];
    private bool[]  lightColor_red = new bool[2];

    private int currentTileIndex = 0;

    public override void EnterState(TutorialManager tutorialManager)
    {
        base.EnterState(tutorialManager);

        Dialogue.Instance.DisplayDialogue(startDialogueData);
        currentStep = TutorialStep.Step1;

        lightTimer = new float[lightingTileCordinate.Length];
        lightColor_red = new bool[lightingTileCordinate.Length];
    }

    public override void UpdateState()
    {
        base.UpdateState();

        switch (currentStep)
        {
            case TutorialStep.Step1: // ダイアログ1が終了 -> ミッションの表示
                if (startDialogueData.IsFinished)
                {// ダイアログが消えて且つまだミッションを表示していないならミッションを表示する
                    currentTileIndex = 0;
                    Mission.Instance.DisplayMission("光っているマスに乗る");
                    HelpIcon.Instance.Move.gameObject.SetActive(true);
                    ChangeStep(TutorialStep.Step2);
                }
                break;

            case TutorialStep.Step2: // ミッションのクリア判定をチェック

                HelpIcon.Instance.Move.SetInput(tutorialManager.GetPlayerInputOsh().MoveValue, tutorialManager.GetPlayerInputMag().MoveValue);

                mag_OnTile = false;
                osh_OnTile = false;

                foreach (Vector2Int coordinate in lightingTileCordinate)
                {
                    FieldTile tile = FieldManager.Instance.GetFieldTile(coordinate);

                    if (tile != null)
                    {
                        lightTimer[currentTileIndex] += Time.deltaTime;
                        if(lightTimer[currentTileIndex] >= lightColorChangeInterval)
                        {
                            if(lightColor_red[currentTileIndex])
                            {
                                EffectManager.Instance.PlayEffect("Field_Red", tile.transform.position + new Vector3(0, (tile.transform.localScale.y * 0.5f), 0), Quaternion.identity);
                            }
                            else
                            {
                                EffectManager.Instance.PlayEffect("Field_Blue", tile.transform.position + new Vector3(0, (tile.transform.localScale.y * 0.5f), 0), Quaternion.identity);
                            }

                            lightTimer[currentTileIndex] = 0f;
                            lightColor_red[currentTileIndex] = !lightColor_red[currentTileIndex];
                        }
                    }

                    var playerMag = tutorialManager.GetPlayerMag();
                    var playerOsh = tutorialManager.GetPlayerOsh();
                    if (tile != null && (playerMag.OnStandingPiece == tile))
                    {
                        mag_OnTile = true;
                    }

                    if (tile != null && (playerOsh.OnStandingPiece == tile))
                    {
                        osh_OnTile = true;
                    }

                    currentTileIndex++;
                    currentTileIndex %= lightingTileCordinate.Length;
                }

                if(mag_OnTile && osh_OnTile)
                {
                    HelpIcon.Instance.Move.gameObject.SetActive(false);
                    ChangeStep(TutorialStep.Step3);
                }

                break;

            case TutorialStep.Step3: // ミッションをクリアしたらミッションを消す
                Mission.Instance.ClearMission();
                ChangeStep(TutorialStep.Step4);
                break;

            case TutorialStep.Step4: // ミッションのアニメーション終了チェック
                if (Mission.Instance.IsFinishedMission())
                {// ミッションがクリアされて且つミッションを表示していたなら次のステートに遷移する
                    Dialogue.Instance.DisplayDialogue(DialogueData_2);
                    ChangeStep(TutorialStep.Step5);
                }
                break;

            case TutorialStep.Step5:
                if(DialogueData_2.IsFinished)
                {
                    tutorialManager.ChangeState(this.NextStateKey);
                    ChangeStep(TutorialStep.Step6);
                }
                break;

            case TutorialStep.Step6:
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
