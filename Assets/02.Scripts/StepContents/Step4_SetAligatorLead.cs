using UnityEngine;

public class Step4_SetAligatorLead : StepBase
{
    [SerializeField] private GameObject[] m_alligatorLeads;
    private GameObject[] m_alligatorLeadHologram;
    private CircuitManager m_circuitManager;
    private enum Step4State
    {
        Ready,
        SetAlligatorLead,
    }

    private Step4State m_step4State = Step4State.Ready;
    protected override void Start()
    {
        base.Start();
        m_circuitManager = FindFirstObjectByType<CircuitManager>();
    }

    public override void Execute()
    {
        Managers.UI.SetChapterScrollActive(false);
        m_circuitManager.StartCheckCircuitCompletionCoroutine();
        Managers.UI.ShowPopUpText("MoveAlligatorLead");
        Managers.Object.GetObject("BatteryHolder").GetComponent<CtrlObjects>().enabled = true;
        Managers.Object.GetObject("LightBulbHolder").GetComponent<CtrlObjects>().enabled = true;
        Managers.Object.GetObject("Switch").GetComponent<CtrlObjects>().enabled = true;
    }

    public override void Redo()
    {
        Managers.UI.SetChapterScrollActive(false);
        foreach (GameObject alligatorLead in m_alligatorLeads)
        {
            alligatorLead.SetActive(true);
        }
        m_alligatorLeadHologram = Managers.Object.GetGameObjectsByCategory("Alligator lead_Hologram").ToArray();
        foreach (GameObject hologram in m_alligatorLeadHologram)
        {
            hologram.SetActive(false);
        }
        m_circuitManager.StartCheckCircuitCompletionCoroutine();
        Managers.UI.ShowPopUpText("MoveAlligatorLead");
        Managers.Object.GetObject("BatteryHolder").GetComponent<CtrlObjects>().enabled = true;
        Managers.Object.GetObject("LightBulbHolder").GetComponent<CtrlObjects>().enabled = true;
        Managers.Object.GetObject("Switch").GetComponent<CtrlObjects>().enabled = true;
    }
}
