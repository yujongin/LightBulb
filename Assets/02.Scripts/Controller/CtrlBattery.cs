using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CtrlBattery : CtrlInteractable
{
    [SerializeField] private GameObject m_battery;
    [SerializeField] private GameObject m_flipButton;

    bool m_isFlip = false;
    private GameObject m_batteryHolder;
    private Animator m_holderAnimator;
    protected override void Start()
    {
        base.Start();
        m_flipButton.SetActive(true);
        m_flipButton.GetComponent<Button>().onClick.AddListener(FlipBattery);
    }

    public void FlipBattery()
    {
        m_battery.transform.Rotate(0, 180, 0);
        m_isFlip = !m_isFlip;
    }

    protected override void OnDragStart()
    {
        base.OnDragStart();
        m_flipButton.SetActive(false);
    }
    protected override void OnDragEnd()
    {
        base.OnDragEnd();
        if (m_batteryHolder == null)
        {
            m_batteryHolder = Managers.Object.GetObject("BatteryHolder");
        }

        // 배터리와 홀더의 콜라이더 범위 확인
        if (m_batteryHolder != null)
        {
            Collider batteryCollider = GetComponent<Collider>();
            Collider holderCollider = m_batteryHolder.GetComponent<Collider>();

            if (batteryCollider != null && holderCollider != null)
            {
                // 콜라이더 범위가 겹치는지 확인
                if (batteryCollider.bounds.Intersects(holderCollider.bounds))
                {
                    // 홀더의 애니메이터 활성화
                    m_holderAnimator = m_batteryHolder.GetComponent<Animator>();
                    if (m_holderAnimator != null)
                    {
                        m_holderAnimator.enabled = true;
                        m_flipButton.SetActive(false);
                        m_holderAnimator.SetTrigger("Insert");
                        Managers.UI.ClosePopUpText();
                        // isFlip 상태에 따라 트리거 설정
                        if (m_isFlip)
                        {
                            SetInteractionMode(InteractionMode.None);
                            Sequence seq = DOTween.Sequence();
                            seq.AppendInterval(1f);
                            seq.Append(m_batteryHolder.transform.DOMove(new Vector3(-0.1f, 0.0233f, 0.15f), 2f).SetEase(Ease.InOutSine));
                            seq.Join(m_batteryHolder.transform.DORotate(new Vector3(0, 45, 0), 2f).SetEase(Ease.InOutSine));
                            seq.AppendCallback(() =>
                            {
                                batteryCollider.enabled = false;
                                Managers.Command.ExecuteCommand(2);
                            });
                        }
                        else
                        {
                            Managers.UI.ShowPopUpText("InCorrectBattery", () =>
                            {
                                Managers.UI.ClosePopUpText();
                                ResetBattery();
                            });
                        }
                    }
                }
                else
                {
                    m_flipButton.SetActive(true);
                }
            }
        }
    }
    public void Redo()
    {
        if (m_batteryHolder == null)
        {
            m_batteryHolder = Managers.Object.GetObject("BatteryHolder");
        }
        m_holderAnimator = m_batteryHolder.GetComponent<Animator>();
        if (m_holderAnimator != null)
        {
            SetInteractionMode(InteractionMode.None);
            m_holderAnimator.enabled = true;
            m_flipButton.SetActive(false);
            m_holderAnimator.Play("Insert", 0, 1);
            Managers.UI.ClosePopUpText();
            m_batteryHolder.transform.localPosition = new Vector3(-0.1f, 0.0233f, 0.15f);
            m_batteryHolder.transform.localRotation = Quaternion.Euler(0, 45, 0);
            FlipBattery();
            GetComponent<Collider>().enabled = false;
        }
    }
    public void ResetBattery()
    {
        m_holderAnimator.SetTrigger("Idle");
        m_holderAnimator.enabled = false;
        transform.localPosition = new Vector3(0, 0.12f, 0);
        m_flipButton.SetActive(true);
        Managers.UI.ShowPopUpText("FlipBattery");
    }
}
