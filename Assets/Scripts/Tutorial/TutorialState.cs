using UnityEngine;

public class TutorialState : ScriptableObject
{
    protected enum TutorialStep
    {
        Step1,
        Step2,
        Step3,
        Step4,
        Step5,
        Step6,
        Step7,
        Step8,
        Step9,
        Step10
    }
    protected TutorialManager tutorialManager;
    protected TutorialStep currentStep = TutorialStep.Step1;
    public string Key { get; private set; }
    public string NextStateKey { get;  private set; }

    protected bool missionDisplayed = false;
    protected bool missionCleared = false;
    public void SetClear(bool cleared)
    {
        missionCleared = cleared;
    }


    public void SetKey(string key, string nextStateKey)
    {
        this.Key = key;
        this.NextStateKey = nextStateKey;
    }


    public virtual void EnterState(TutorialManager tutorialManager)
    {
        this.tutorialManager = tutorialManager;
        missionCleared = false;

        tutorialManager.GetPlayerMag().SetCanPlaceStone(false);
        tutorialManager.GetPlayerMag().SetSmashEnable(false);
        tutorialManager.GetPlayerMag().SetForcePlaceEnable(false);

        tutorialManager.GetPlayerOsh().SetCanPlaceStone(false);
        tutorialManager.GetPlayerOsh().SetSmashEnable(false);
        tutorialManager.GetPlayerOsh().SetForcePlaceEnable(false);

    }

    public virtual void UpdateState(){}

    public virtual void FixedUpdateState(){}

    public virtual void LateUpdateState(){}

    public virtual void ExitState(){}

    protected void ChangeStep(TutorialStep nextStep)
    {
        currentStep = nextStep;
    }
}
