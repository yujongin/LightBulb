using UnityEngine;
using DG.Tweening;

public class CtrlObjects : CtrlInteractable 
{
    [Header("회전 설정")]
    [SerializeField] private bool m_enableLookAtCenter = true; // 중심점을 바라보는 회전 활성화
    [SerializeField] private Vector3 m_lookAtTarget = Vector3.zero; // 바라볼 중심점 (월드 좌표)
    [SerializeField] private float m_yRotationOffset = 0f; // Y축 회전 오프셋 (도 단위, 90도면 left가 forward 방향을 바라봄)
    [SerializeField] private bool m_smoothRotation = true; // 부드러운 회전 활성화
    [SerializeField] private float m_rotationSpeed = 5f; // 회전 속도
    [SerializeField] private bool m_lockYRotation = false; // Y축 회전 고정 (수평면에서만 회전)
    
    private Vector3 m_rotationCenter; // 회전 중심점
    private Quaternion m_targetRotation; // 목표 회전값
    
    #region Unity 생명주기
    protected override void Start()
    {
        base.Start();
        UpdateRotationCenter();
    }
    
    protected override void Update()
    {
        base.Update();
        
        // 드래그 중이고 회전이 활성화된 경우 회전 처리
        if (m_isDragging && m_enableLookAtCenter)
        {
            UpdateRotation();
        }
    }
    #endregion
    
    #region 드래그 오버라이드
    protected override void OnDragStart()
    {
        base.OnDragStart();
        UpdateRotationCenter();
    }
    
    protected override void OnDragUpdate()
    {
        base.OnDragUpdate();
        
        if (m_enableLookAtCenter)
        {
            UpdateRotation();
        }
    }
    
    protected override void OnDragEnd()
    {
        base.OnDragEnd();
        
        // 드래그 종료 시 부드러운 회전으로 최종 위치로 이동
        if (m_enableLookAtCenter && m_smoothRotation)
        {
            ApplyTargetRotation();
        }
    }
    #endregion
    
    #region 회전 처리
    private void UpdateRotationCenter()
    {
        // 회전 중심점 계산 (Y축 오프셋은 더 이상 사용하지 않음)
        m_rotationCenter = m_lookAtTarget;
    }
    
    private void UpdateRotation()
    {
        // 현재 위치에서 회전 중심점까지의 방향 계산
        Vector3 direction = m_rotationCenter - transform.position;
        
        // Y축 회전 고정이 활성화된 경우 Y축 성분 제거
        if (m_lockYRotation)
        {
            direction.y = 0f;
        }
        
        // 방향이 유효한 경우에만 회전 계산
        if (direction.magnitude > 0.001f)
        {
            // 목표 회전값 계산 (Z축이 중심점을 향하도록)
            m_targetRotation = Quaternion.LookRotation(direction);
            
            // Y축 회전 오프셋 적용 (도 단위를 라디안으로 변환)
            if (Mathf.Abs(m_yRotationOffset) > 0.001f)
            {
                Quaternion offsetRotation = Quaternion.Euler(0, m_yRotationOffset, 0);
                m_targetRotation = m_targetRotation * offsetRotation;
            }
            
            // 부드러운 회전 적용
            if (m_smoothRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, m_targetRotation, m_rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = m_targetRotation;
            }
        }
    }
    
    private void ApplyTargetRotation()
    {
        // DOTween을 사용한 부드러운 회전
        transform.DORotate(m_targetRotation.eulerAngles, 0.3f).SetEase(Ease.OutQuad);
    }
    #endregion
    
    #region 공개 메서드
    /// <summary>
    /// 중심점을 바라보는 회전 활성화/비활성화
    /// </summary>
    public void SetLookAtCenterEnabled(bool enabled)
    {
        m_enableLookAtCenter = enabled;
    }
    
    /// <summary>
    /// 바라볼 중심점 설정
    /// </summary>
    public void SetLookAtTarget(Vector3 target)
    {
        m_lookAtTarget = target;
        UpdateRotationCenter();
    }
    
    /// <summary>
    /// Y축 회전 오프셋 설정 (도 단위)
    /// </summary>
    public void SetYRotationOffset(float offset)
    {
        m_yRotationOffset = offset;
    }
    
    /// <summary>
    /// 부드러운 회전 활성화/비활성화
    /// </summary>
    public void SetSmoothRotationEnabled(bool enabled)
    {
        m_smoothRotation = enabled;
    }
    
    /// <summary>
    /// 회전 속도 설정
    /// </summary>
    public void SetRotationSpeed(float speed)
    {
        m_rotationSpeed = speed;
    }
    
    /// <summary>
    /// Y축 회전 고정 활성화/비활성화
    /// </summary>
    public void SetLockYRotationEnabled(bool enabled)
    {
        m_lockYRotation = enabled;
    }
    
    /// <summary>
    /// 즉시 목표 회전값으로 회전
    /// </summary>
    public void RotateToTargetImmediate()
    {
        if (m_enableLookAtCenter)
        {
            UpdateRotationCenter();
            UpdateRotation();
            transform.rotation = m_targetRotation;
        }
    }
    
    /// <summary>
    /// 애니메이션으로 목표 회전값으로 회전
    /// </summary>
    public void RotateToTargetAnimated(float duration = 0.3f)
    {
        if (m_enableLookAtCenter)
        {
            UpdateRotationCenter();
            UpdateRotation();
            transform.DORotate(m_targetRotation.eulerAngles, duration).SetEase(Ease.OutQuad);
        }
    }
    #endregion
    
    #region 속성 접근자
    /// <summary>
    /// 중심점을 바라보는 회전 활성화 상태
    /// </summary>
    public bool IsLookAtCenterEnabled => m_enableLookAtCenter;
    
    /// <summary>
    /// 현재 바라보는 중심점
    /// </summary>
    public Vector3 LookAtTarget => m_lookAtTarget;
    
    /// <summary>
    /// 현재 Y축 회전 오프셋 (도 단위)
    /// </summary>
    public float YRotationOffset => m_yRotationOffset;
    
    /// <summary>
    /// 부드러운 회전 활성화 상태
    /// </summary>
    public bool IsSmoothRotationEnabled => m_smoothRotation;
    
    /// <summary>
    /// 현재 회전 속도
    /// </summary>
    public float RotationSpeed => m_rotationSpeed;
    
    /// <summary>
    /// Y축 회전 고정 활성화 상태
    /// </summary>
    public bool IsLockYRotationEnabled => m_lockYRotation;
    
    /// <summary>
    /// 현재 회전 중심점
    /// </summary>
    public Vector3 RotationCenter => m_rotationCenter;
    
    /// <summary>
    /// 현재 목표 회전값
    /// </summary>
    public Quaternion TargetRotation => m_targetRotation;
    #endregion
}
