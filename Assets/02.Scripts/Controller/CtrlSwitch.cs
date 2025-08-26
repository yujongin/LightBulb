using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CtrlSwitch : CtrlInteractable
{
    #region Serialized Fields
    [SerializeField] private SkinnedMeshRenderer m_switchRenderer;
    [SerializeField] private CircuitManager m_circuitManager;
    [SerializeField] private Ease m_animationEase = Ease.OutQuad;
    #endregion

    #region Private Fields
    private const int C_BLEND_SHAPE_INDEX = 0;
    private const float C_PRESSED_VALUE = 100f;
    private const float C_RELEASED_VALUE = 0f;
    private Tween m_currentTween;
    #endregion

    #region Unity Lifecycle


    protected override void Start()
    {
        // SkinnedMeshRenderer가 할당되지 않은 경우 자동으로 찾기
        if (m_switchRenderer == null)
        {
            m_switchRenderer = GetComponent<SkinnedMeshRenderer>();
        }
        m_circuitManager = FindFirstObjectByType<CircuitManager>();
    }

    protected override void OnDestroy()
    {
        // 메모리 누수 방지를 위한 Tween 정리
        m_currentTween?.Kill();
    }
    #endregion

    #region Event Handlers
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (m_switchRenderer != null)
        {
            AnimateBlendShape(C_PRESSED_VALUE);
        }
        m_circuitManager.SetSwitchState(true);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (m_switchRenderer != null)
        {
            AnimateBlendShape(C_RELEASED_VALUE);
        }
        m_circuitManager.SetSwitchState(false);
    }
    #endregion

    #region Private Methods
    private void AnimateBlendShape(float targetValue)
    {
        // 기존 애니메이션이 실행 중이면 중지
        m_currentTween?.Kill();

        float currentValue = m_switchRenderer.GetBlendShapeWeight(C_BLEND_SHAPE_INDEX);

        m_currentTween = DOTween.To(
            () => currentValue,
            value => m_switchRenderer.SetBlendShapeWeight(C_BLEND_SHAPE_INDEX, value),
            targetValue,
            0.1f
        ).SetEase(m_animationEase);
    }
    #endregion
}

