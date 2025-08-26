using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CtrlAlligatorlead : CtrlInteractable
{
    #region Serialized Fields

    [SerializeField] private Ease m_animationEase = Ease.OutQuad;
    #endregion

    #region Private Fields
    private const int C_BLEND_SHAPE_INDEX = 0;
    private const float C_PRESSED_VALUE = 100f;
    private const float C_RELEASED_VALUE = 0f;
    private SkinnedMeshRenderer m_skinnedMeshRenderer;
    private Tween m_currentTween;

    private bool isPointerDown = false;
    private LeadPoint[] allLeadPoints;
    #endregion

    #region Unity Lifecycle
    protected override void Start()
    {
        base.Start();

        // 자식 오브젝트에서 SkinnedMeshRenderer 찾기
        m_skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        if (m_skinnedMeshRenderer == null)
        {
            
        }
    }
    protected override void Update()
    {
        base.Update();
        if (isPointerDown)
        {
            LookAtClosestLeadPoint();
        }
    }
    protected override void OnDestroy()
    {
        // 메모리 누수 방지를 위한 Tween 정리
        m_currentTween?.Kill();
        base.OnDestroy();
    }
    #endregion

    #region Event Handlers
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (m_skinnedMeshRenderer != null)
        {
            AnimateBlendShape(C_PRESSED_VALUE);
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (m_skinnedMeshRenderer != null)
        {
            AnimateBlendShape(C_RELEASED_VALUE);
        }
    }
    #endregion

    #region Private Methods
    private void AnimateBlendShape(float targetValue)
    {
        // 기존 애니메이션이 실행 중이면 중지
        m_currentTween?.Kill();

        float currentValue = m_skinnedMeshRenderer.GetBlendShapeWeight(C_BLEND_SHAPE_INDEX);

        m_currentTween = DOTween.To(
            () => currentValue,
            value => m_skinnedMeshRenderer.SetBlendShapeWeight(C_BLEND_SHAPE_INDEX, value),
            targetValue,
            0.1f
        ).SetEase(m_animationEase);
    }



    /// <summary>
    /// 가장 가까운 LeadPoint를 찾아서 x벡터(right)가 바라보도록 회전
    /// </summary>
    private void LookAtClosestLeadPoint()
    {
        // 씬에서 모든 LeadPoint 찾기
        if (allLeadPoints == null)
        {
            allLeadPoints = FindObjectsByType<LeadPoint>(FindObjectsSortMode.None);
        }

        if (allLeadPoints.Length == 0)
        {
            
            return;
        }

        // 가장 가까운 LeadPoint 찾기
        LeadPoint closestLeadPoint = null;
        float closestDistance = float.MaxValue;

        foreach (LeadPoint leadPoint in allLeadPoints)
        {
            if (leadPoint == null) continue;

            float distance = Vector3.Distance(transform.position, leadPoint.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestLeadPoint = leadPoint;
            }
        }

        // 가장 가까운 LeadPoint를 x벡터(right)가 바라보도록 회전
        if (closestLeadPoint != null)
        {
            Vector3 direction = (closestLeadPoint.transform.position - transform.position).normalized;
            direction.y = 0; // Y축 회전만 (수평 회전)

            if (direction.magnitude > 0.01f)
            {
                // Right가 LeadPoint를 바라보도록 하기 위해 direction을 -90도 회전
                Vector3 rightDirection = Quaternion.AngleAxis(-90f, Vector3.up) * direction;
                Quaternion targetRotation = Quaternion.LookRotation(rightDirection);

                // 즉시 회전 (애니메이션 없이)
                transform.rotation = targetRotation;

                
            }
        }
    }
    #endregion
}
