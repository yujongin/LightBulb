using UnityEngine;

/// <summary>
/// 게임오브젝트가 카메라를 바라보도록 하는 스크립트
/// 회전 축 선택, 반대 방향 바라보기, 성능 최적화 기능 포함
/// </summary>
public class LookCameraLabel : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("바라볼 카메라 (null이면 메인 카메라 자동 사용)")]
    [SerializeField] private Camera m_targetCamera;
    [Tooltip("카메라 대신 반대 방향을 바라봅니다")]
    [SerializeField] private bool m_lookAway = false;
    
    [Header("Follow Settings")]
    [Tooltip("따라다닐 타겟 오브젝트")]
    [SerializeField] private Transform m_followTarget;
    [Tooltip("오브젝트를 따라다닙니다")]
    [SerializeField] private bool m_enableFollow = false;
    [Tooltip("타겟 오브젝트로부터의 위치 오프셋")]
    [SerializeField] private Vector3 m_followOffset = Vector3.zero;
    
    [Header("Follow Constraints")]
    [Tooltip("X축 위치 따라가기")]
    [SerializeField] private bool m_followX = true;
    [Tooltip("Y축 위치 따라가기")]
    [SerializeField] private bool m_followY = true;
    [Tooltip("Z축 위치 따라가기")]
    [SerializeField] private bool m_followZ = true;
    
    [Header("Rotation Settings")]
    [Tooltip("X축 회전 제한")]
    [SerializeField] private bool m_freezeX = false;
    [Tooltip("Y축 회전 제한")]
    [SerializeField] private bool m_freezeY = false;
    [Tooltip("Z축 회전 제한")]
    [SerializeField] private bool m_freezeZ = false;
    
    [Header("Performance Settings")]
    [Tooltip("매 프레임 업데이트 (false면 카메라 위치 변화시만 업데이트)")]
    [SerializeField] private bool m_updateEveryFrame = true;
    [Tooltip("업데이트 간격 (초, updateEveryFrame이 false일 때만 사용)")]
    [SerializeField] private float m_updateInterval = 0.1f;
    
    // 성능 최적화를 위한 변수들
    private Vector3 m_lastCameraPosition;
    private Quaternion m_lastCameraRotation;
    private Vector3 m_lastTargetPosition;  // 따라다니는 타겟의 마지막 위치
    private Vector3 m_originalRotation;
    private Vector3 m_originalPosition;    // 원래 위치 저장
    private float m_lastUpdateTime;
    
    /// <summary>
    /// 초기화
    /// </summary>
    private void Start()
    {
        // 카메라가 설정되지 않았으면 메인 카메라 사용
        if (m_targetCamera == null)
        {
            m_targetCamera = Camera.main;
            
            if (m_targetCamera == null)
            {
                
                enabled = false;
                return;
            }
        }
        
        // 초기 상태 저장
        m_originalRotation = transform.eulerAngles;
        m_originalPosition = transform.position;
        m_lastCameraPosition = m_targetCamera.transform.position;
        m_lastCameraRotation = m_targetCamera.transform.rotation;
        
        // 따라다니기 타겟 초기화
        if (m_followTarget != null)
        {
            m_lastTargetPosition = m_followTarget.position;
        }
        
        m_lastUpdateTime = Time.time;
        
        // 초기 위치 및 방향 설정
        if (m_enableFollow) FollowTarget();
        LookAtCamera();
    }
    
    /// <summary>
    /// 매 프레임 업데이트
    /// </summary>
    private void Update()
    {
        if (m_updateEveryFrame)
        {
            // 매 프레임 업데이트
            if (m_enableFollow) FollowTarget();
            if (m_targetCamera != null) LookAtCamera();
        }
        else
        {
            // 타겟이나 카메라 위치/회전이 변경되었거나 일정 시간이 지났을 때만 업데이트
            bool targetChanged = HasTargetChanged();
            bool cameraChanged = m_targetCamera != null && HasCameraChanged();
            bool timeElapsed = Time.time - m_lastUpdateTime >= m_updateInterval;
            
            if (targetChanged || cameraChanged || timeElapsed)
            {
                if (m_enableFollow && targetChanged) FollowTarget();
                if (m_targetCamera != null && cameraChanged) LookAtCamera();
                
                // 상태 업데이트
                if (m_followTarget != null)
                    m_lastTargetPosition = m_followTarget.position;
                if (m_targetCamera != null)
                {
                    m_lastCameraPosition = m_targetCamera.transform.position;
                    m_lastCameraRotation = m_targetCamera.transform.rotation;
                }
                m_lastUpdateTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// 따라다니는 타겟이 이동했는지 확인
    /// </summary>
    /// <returns>타겟 위치가 변경되었으면 true</returns>
    private bool HasTargetChanged()
    {
        if (m_followTarget == null) return false;
        return Vector3.Distance(m_followTarget.position, m_lastTargetPosition) > 0.01f;
    }
    
    /// <summary>
    /// 카메라가 이동했는지 확인
    /// </summary>
    /// <returns>카메라 위치나 회전이 변경되었으면 true</returns>
    private bool HasCameraChanged()
    {
        return Vector3.Distance(m_targetCamera.transform.position, m_lastCameraPosition) > 0.01f ||
               Quaternion.Angle(m_targetCamera.transform.rotation, m_lastCameraRotation) > 0.1f;
    }
    
    /// <summary>
    /// 카메라를 바라보도록 회전
    /// </summary>
    private void LookAtCamera()
    {
        Vector3 targetPosition = m_targetCamera.transform.position;
        Vector3 currentPosition = transform.position;
        
        // 반대 방향을 바라보는 경우
        if (m_lookAway)
        {
            Vector3 direction = currentPosition - targetPosition;
            targetPosition = currentPosition + direction;
        }
        
        // 기본 LookAt 회전 계산
        Vector3 lookDirection = targetPosition - currentPosition;
        
        if (lookDirection.magnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            Vector3 targetEuler = targetRotation.eulerAngles;
            Vector3 currentEuler = transform.eulerAngles;
            
            // 축 제한 적용
            Vector3 finalEuler = new Vector3(
                m_freezeX ? m_originalRotation.x : targetEuler.x,
                m_freezeY ? m_originalRotation.y : targetEuler.y,
                m_freezeZ ? m_originalRotation.z : targetEuler.z
            );
            
            // 최종 회전 적용
            transform.rotation = Quaternion.Euler(finalEuler);
        }
    }
    
    /// <summary>
    /// 타겟 카메라 설정
    /// </summary>
    /// <param name="camera">새로운 타겟 카메라</param>
    public void SetTargetCamera(Camera camera)
    {
        m_targetCamera = camera;
        if (m_targetCamera != null)
        {
            m_lastCameraPosition = m_targetCamera.transform.position;
            m_lastCameraRotation = m_targetCamera.transform.rotation;
            LookAtCamera();
        }
    }
    
    /// <summary>
    /// 바라보는 방향 토글 (정방향/반대방향)
    /// </summary>
    public void ToggleLookDirection()
    {
        m_lookAway = !m_lookAway;
        LookAtCamera();
    }
    
    /// <summary>
    /// 바라보는 방향 설정
    /// </summary>
    /// <param name="lookAway">true면 반대 방향, false면 정방향</param>
    public void SetLookDirection(bool lookAway)
    {
        m_lookAway = lookAway;
        LookAtCamera();
    }
    
    /// <summary>
    /// 회전 축 제한 설정
    /// </summary>
    /// <param name="freezeX">X축 제한</param>
    /// <param name="freezeY">Y축 제한</param>
    /// <param name="freezeZ">Z축 제한</param>
    public void SetRotationConstraints(bool freezeX, bool freezeY, bool freezeZ)
    {
        m_freezeX = freezeX;
        m_freezeY = freezeY;
        m_freezeZ = freezeZ;
        LookAtCamera();
    }
    
    /// <summary>
    /// 업데이트 모드 설정
    /// </summary>
    /// <param name="updateEveryFrame">매 프레임 업데이트 여부</param>
    public void SetUpdateMode(bool updateEveryFrame)
    {
        m_updateEveryFrame = updateEveryFrame;
    }
    
    /// <summary>
    /// 따라다니기와 바라보기 모두 강제 업데이트
    /// </summary>
    public void ForceUpdate()
    {
        if (m_enableFollow && m_followTarget != null) FollowTarget();
        if (m_targetCamera != null) LookAtCamera();
    }
    
    /// <summary>
    /// 원래 위치로 복원
    /// </summary>
    public void ResetToOriginalPosition()
    {
        transform.position = m_originalPosition;
    }
    
    /// <summary>
    /// 원래 회전값으로 초기화
    /// </summary>
    public void ResetToOriginalRotation()
    {
        transform.rotation = Quaternion.Euler(m_originalRotation);
    }
    
    /// <summary>
    /// 따라다닐 타겟 설정
    /// </summary>
    /// <param name="target">새로운 따라다닐 타겟</param>
    public void SetFollowTarget(Transform target)
    {
        m_followTarget = target;
        if (m_followTarget != null)
        {
            m_lastTargetPosition = m_followTarget.position;
            if (m_enableFollow) FollowTarget();
        }
    }
    
    /// <summary>
    /// 따라다니기 활성화/비활성화
    /// </summary>
    /// <param name="enable">따라다니기 활성화 여부</param>
    public void SetFollowEnabled(bool enable)
    {
        m_enableFollow = enable;
        if (m_enableFollow && m_followTarget != null)
        {
            FollowTarget();
        }
    }
    
    /// <summary>
    /// 따라다니기 오프셋 설정
    /// </summary>
    /// <param name="offset">타겟으로부터의 오프셋</param>
    public void SetFollowOffset(Vector3 offset)
    {
        m_followOffset = offset;
        if (m_enableFollow && m_followTarget != null)
        {
            FollowTarget();
        }
    }
    
    /// <summary>
    /// 따라다니기 축 제한 설정
    /// </summary>
    /// <param name="followX">X축 따라가기</param>
    /// <param name="followY">Y축 따라가기</param>
    /// <param name="followZ">Z축 따라가기</param>
    public void SetFollowConstraints(bool followX, bool followY, bool followZ)
    {
        m_followX = followX;
        m_followY = followY;
        m_followZ = followZ;
        if (m_enableFollow && m_followTarget != null)
        {
            FollowTarget();
        }
    }
    
    /// <summary>
    /// 원래 위치와 회전으로 완전 초기화
    /// </summary>
    public void ResetToOriginal()
    {
        transform.position = m_originalPosition;
        transform.rotation = Quaternion.Euler(m_originalRotation);
    }
    
    /// <summary>
    /// 타겟을 따라다니기
    /// </summary>
    private void FollowTarget()
    {
        if (m_followTarget == null) return;
        
        Vector3 targetPosition = m_followTarget.position + m_followOffset;
        Vector3 currentPosition = transform.position;
        
        // 축별 제한 적용
        Vector3 finalPosition = new Vector3(
            m_followX ? targetPosition.x : currentPosition.x,
            m_followY ? targetPosition.y : currentPosition.y,
            m_followZ ? targetPosition.z : currentPosition.z
        );
        
        // 최종 위치 적용
        transform.position = finalPosition;
    }
}
