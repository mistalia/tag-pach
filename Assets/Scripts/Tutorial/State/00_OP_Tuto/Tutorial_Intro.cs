using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialIntroduction", menuName = "Tutorial/State/TutorialIntroduction")]
public class TutorialIntroduction : TutorialState
{
    
    [SerializeField] private DialogueData startDialogue;
    [SerializeField] private DialogueData secondDialogue;

    [SerializeField] private float scanDuration = 1.0f;

    private float scanWait = 0f;

    private bool onece = false;

    public override void EnterState(TutorialManager tutorialManager)
    {
        base.EnterState(tutorialManager);
        Dialogue.Instance.DisplayDialogue(startDialogue);
        onece = false;
        scanWait = 0.0f;

    }

    public override void UpdateState()
    {
        base.UpdateState();

        if (startDialogue.IsFinished)
        {// ダイアログが消えたらAROpen
            
            if (!onece)
            {
                ARScanController.Instance.OnBattleStart();
                onece = true;
            }

            if (scanWait >= 0.0f)
            {
                scanWait += Time.deltaTime;
            }
        }        

        if(scanWait > scanDuration)
        {
            Dialogue.Instance.DisplayDialogue(secondDialogue);
            scanWait = -1f;
        }

        if (startDialogue.IsFinished && secondDialogue.IsFinished && scanWait <  0.0f)
        {// ２つめのダイアログが消えたら次のステートへ
            tutorialManager.ChangeState(this.NextStateKey);

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
