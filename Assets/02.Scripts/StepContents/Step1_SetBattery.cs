using UnityEngine;

public class Step1_SetBattery : StepBase
{
    private GameObject m_battery;
    private GameObject m_batteryHolder;
    private GameObject m_batteryHologram;
    private GameObject m_batteryHolderHologram;

    private Step1State m_step1State;
    private enum Step1State
    {
        SetBatteryHolder,
        SetBattery,
    }

    protected override void Start()
    {
        base.Start();
        Managers.Command.RegisterCommand(11, this);
        Managers.Command.RegisterCommand(12, this);
        m_step1State = Step1State.SetBatteryHolder;
        Managers.UI.ShowPopUpText("ExplorationGoal", () =>
        {
            SetBatteryHolder();
        });
    }
    public override void Execute()
    {
        switch (m_step1State)
        {
            case Step1State.SetBatteryHolder:
                m_batteryHolder = Managers.Object.GetObject("BatteryHolder");
                m_batteryHologram = Managers.Object.GetObject("Battery_Hologram");
                m_batteryHologram.SetActive(true);
                Managers.UI.GetButtonByStepIndex(12).interactable = true;
                Managers.UI.ShowPopUpText("SetBattery");
                m_step1State = Step1State.SetBattery;
                break;
            case Step1State.SetBattery:
                m_battery = Managers.Object.GetObject("Battery");
                ScaleMove(m_battery, new Vector3(1, 1, 1), true, 0.5f);
                Managers.UI.ShowPopUpText("FlipBattery");
                break;
        }
    }

    public override void Redo()
    {
        m_batteryHolderHologram = Managers.Object.GetObject("BatteryHolder_Hologram");
        m_batteryHolderHologram.SetActive(false);
        m_batteryHolder = Managers.Object.GetObject("BatteryHolder");
        m_batteryHolder.SetActive(true);
        m_battery = Managers.Object.GetObject("Battery");
        m_battery.SetActive(true);
        Managers.UI.ShowPopUpText("FlipBattery");
        Managers.UI.GetButtonByStepIndex(11).gameObject.SetActive(false);
        Managers.UI.GetButtonByStepIndex(12).gameObject.SetActive(false);
        m_step1State = Step1State.SetBattery;
    }

    public void SetBatteryHolder()
    {
        m_batteryHolderHologram = Managers.Object.GetObject("BatteryHolder_Hologram");
        m_batteryHolderHologram.SetActive(true);
        Managers.UI.ShowPopUpText("SetBatteryHolder");
    }
}
