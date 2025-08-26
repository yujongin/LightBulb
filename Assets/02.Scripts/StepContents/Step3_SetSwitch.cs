using System.Collections.Generic;
using UnityEngine;

public class Step3_SetSwitch : StepBase
{
    private GameObject m_switch;
    private GameObject m_switchHologram;

    private enum Step3State
    {
        Ready,
        SetSwitch,
    }

    private Step3State m_step3State;

    protected override void Start()
    {
        base.Start();
        Managers.Command.RegisterCommand(31, this);
    }

    public override void Execute()
    {
        switch (m_step3State)
        {
            case Step3State.Ready:
                Managers.UI.ShowPopUpText("SetSwitch");
                m_switch = Managers.Object.GetObject("Switch");
                m_switchHologram = Managers.Object.GetObject("Switch_Hologram");
                m_switchHologram.SetActive(true);
                Managers.UI.SetButtonInteractable(31, true);
                m_step3State = Step3State.SetSwitch;
                break;
            case Step3State.SetSwitch:
                ScaleMove(m_switch, Vector3.one, true);
                Managers.UI.ClosePopUpText();
                Managers.Camera.ChangeCamera(1);
                List<GameObject> lines = Managers.Object.GetGameObjectsByCategory("Alligator lead_Hologram");
                foreach (GameObject line in lines)
                {
                    line.SetActive(true);
                }
                Managers.UI.SetButtonInteractable(4, true);
                Managers.UI.ShowPopUpText("SetAlligatorLead");
                break;
        }
    }

    public override void Redo()
    {
        Managers.Object.GetObject("LightBulb").GetComponent<CtrlBulb>().Redo();
        m_switch = Managers.Object.GetObject("Switch");
        m_switch.SetActive(true);
        Managers.UI.GetButtonByStepIndex(31).gameObject.SetActive(false);
        Managers.UI.ClosePopUpText();
        Managers.Camera.ChangeCamera(1);
        m_step3State = Step3State.SetSwitch;
        List<GameObject> lines = Managers.Object.GetGameObjectsByCategory("Alligator lead_Hologram");
        foreach (GameObject line in lines)
        {
            line.SetActive(true);
        }

        Managers.UI.SetButtonInteractable(4, true);
        Managers.UI.ShowPopUpText("SetAlligatorLead");
    }
}
