using UnityEngine;

public class Step2_SetLightBulb : StepBase
{
    private GameObject m_lightBulb;
    private GameObject m_lightBulbHologram;
    private GameObject m_lightBulbHolder;
    private GameObject m_lightBulbHolderHologram;

    private Step2State m_step2State;
    private enum Step2State
    {
        Ready,
        SetLightBulbHolder,
        SetLightBulb,
    }

    protected override void Start()
    {
        base.Start();
        Managers.Command.RegisterCommand(21, this);
        Managers.Command.RegisterCommand(22, this);
    }

    public override void Execute()
    {
        switch (m_step2State)
        {
            case Step2State.Ready:
                Managers.UI.ShowPopUpText("SetLightBulbHolder");
                m_step2State = Step2State.SetLightBulbHolder;
                m_lightBulbHolder = Managers.Object.GetObject("LightBulbHolder");
                m_lightBulbHolderHologram = Managers.Object.GetObject("LightBulbHolder_Hologram");
                m_lightBulb = Managers.Object.GetObject("LightBulb");
                m_lightBulbHologram = Managers.Object.GetObject("LightBulb_Hologram");
                m_lightBulbHolderHologram.SetActive(true);
                Managers.UI.SetButtonInteractable(21, true);

                break;
            case Step2State.SetLightBulbHolder:
                Managers.UI.ShowPopUpText("SetLightBulb");
                m_lightBulbHologram.SetActive(true);
                ScaleMove(m_lightBulbHolder, Vector3.one, true);
                Managers.UI.SetButtonInteractable(22, true);
                m_step2State = Step2State.SetLightBulb;
                break;
            case Step2State.SetLightBulb:
                Managers.UI.ShowPopUpText("PutLightBulb");
                ScaleMove(m_lightBulb, Vector3.one, true);
                break;
        }
    }

    public override void Redo()
    {
        Managers.Object.GetObject("Battery").GetComponent<CtrlBattery>().Redo();
        Managers.UI.ShowPopUpText("SetLightBulbHolder");
        m_lightBulbHolder = Managers.Object.GetObject("LightBulbHolder");
        m_lightBulb = Managers.Object.GetObject("LightBulb");
        m_lightBulb.SetActive(true);
        m_lightBulbHolder.SetActive(true);
        Managers.UI.GetButtonByStepIndex(21).gameObject.SetActive(false);
        Managers.UI.GetButtonByStepIndex(22).gameObject.SetActive(false);
        Managers.UI.ShowPopUpText("PutLightBulb");
        m_step2State = Step2State.SetLightBulb;
    }
}
