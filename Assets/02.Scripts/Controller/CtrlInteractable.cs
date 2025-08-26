using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public abstract class CtrlInteractable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("상호작용 설정")]
    [SerializeField] private InteractionMode m_interactionMode = InteractionMode.Both;
    [SerializeField] private float m_clickThreshold = 0.3f; // 클릭으로 판단할 최대 시간 (초)
    [SerializeField] private float m_dragThreshold = 10f; // 드래그로 판단할 최소 거리 (픽셀)

    [Header("드래그 제약 설정")]
    [SerializeField] private bool m_enableDragFollow = true; // 마우스를 따라오는 드래그 활성화
    [SerializeField] private bool m_constraintX = false; // X축 이동 제한
    [SerializeField] private bool m_constraintY = false; // Y축 이동 제한
    [SerializeField] private bool m_constraintZ = false; // Z축 이동 제한
    [SerializeField] private float m_dragSensitivity = 1f; // 드래그 감도

    [Header("드래그 범위 제한")]
    [SerializeField] private bool m_enableDragBounds = false; // 드래그 범위 제한 활성화
    [SerializeField] private DragBoundsType m_boundsType = DragBoundsType.Box; // 범위 제한 타입
    [SerializeField] private Vector3 m_boundsCenter = Vector3.zero; // 범위 중심점 (로컬 좌표)
    [SerializeField] private Vector3 m_boundsSize = Vector3.one * 5f; // 박스 크기
    [SerializeField] private float m_boundsRadius = 2.5f; // 원형 반지름
    [SerializeField] private bool m_useWorldSpace = false; // 월드 좌표 사용 여부
    [SerializeField] private bool m_returnToBoundsOnRelease = true; // 드래그 종료 시 범위 내로 복귀

    [Header("원래 자리 복귀 설정")]
    [SerializeField] private bool m_returnToOriginalPosition = false; // 드래그 종료 시 원래 자리로 복귀
    [SerializeField] private float m_returnDuration = 0.5f; // 복귀 애니메이션 지속 시간
    [SerializeField] private Ease m_returnEase = Ease.OutBack; // 복귀 애니메이션 이징

    [Header("방향 드래그 설정")]
    [SerializeField] private bool m_enableDirectionDetection = false; // 방향 감지 활성화
    [SerializeField] private float m_directionThreshold = 50f; // 방향 판단 최소 거리 (픽셀)

    [Header("시각적 피드백 설정")]
    [SerializeField] private bool m_enableVisualFeedback = true;
    [SerializeField] private float m_animationDuration = 0.3f; // 애니메이션 지속 시간

    // 내부 상태 변수
    protected bool m_isDragging = false;
    private bool m_isClicking = false;

    private Vector2 m_startMousePosition;
    private Vector3 m_startObjectPosition;
    private Vector2 m_lastMousePosition;
    private float m_clickStartTime;
    private Camera m_camera;
    private DragDirection m_detectedDirection = DragDirection.None;
    private Tween m_returnTween; // 복귀 애니메이션 트윈

    // 시각적 피드백 컴포넌트
    private Renderer m_renderer;
    private Vector3 m_originalScale;

    public enum InteractionMode
    {
        None,           // 인터렉션 비활성화
        ClickOnly,      // 클릭만 가능
        DragOnly,       // 드래그만 가능
        Both            // 둘 다 가능
    }

    public enum DragDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public enum DragBoundsType
    {
        Box,        // 박스 영역
        Circle,     // 원형 영역
        Custom      // 커스텀 영역 (오버라이드 가능)
    }

    #region Unity 생명주기
    protected virtual void Start()
    {
        m_camera = Camera.main;
        if (m_camera == null)
        {
            m_camera = FindFirstObjectByType<Camera>();
        }
        m_startObjectPosition = transform.position;

        // 시각적 피드백 컴포넌트 초기화
        InitializeVisualComponents();
    }

    private void InitializeVisualComponents()
    {
        m_renderer = GetComponent<Renderer>();
        m_originalScale = transform.localScale;
    }

    protected virtual void Update()
    {
        // None 모드에서는 아무런 업데이트 불가
        if (m_interactionMode == InteractionMode.None) return;

        // 드래그 중일 때 마우스 따라오기 처리
        if (m_isDragging && m_enableDragFollow)
        {
            HandleDragFollow();
        }
    }

    protected virtual void OnDestroy()
    {
        // 진행 중인 복귀 애니메이션 정리
        if (m_returnTween != null && m_returnTween.IsActive())
        {
            m_returnTween.Kill();
        }
    }
    #endregion

    #region 이벤트 핸들러
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        // None 모드에서는 아무런 인터렉션 불가
        if (m_interactionMode == InteractionMode.None) return;

        // 클릭 처리를 위해 시작 마우스 위치는 항상 저장
        m_startMousePosition = eventData.position;
        m_lastMousePosition = eventData.position;

        if (m_interactionMode == InteractionMode.ClickOnly || m_interactionMode == InteractionMode.Both)
        {
            m_isClicking = true;
            m_clickStartTime = Time.time;
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        // None 모드에서는 아무런 인터렉션 불가
        if (m_interactionMode == InteractionMode.None) return;

        // 클릭 처리
        if (m_isClicking && (m_interactionMode == InteractionMode.ClickOnly || m_interactionMode == InteractionMode.Both))
        {
            float clickDuration = Time.time - m_clickStartTime;
            Vector2 mouseMovement = eventData.position - m_startMousePosition;

            if (clickDuration <= m_clickThreshold && mouseMovement.magnitude <= m_dragThreshold)
            {
                OnClick();
            }
        }

        // 드래그 종료 처리
        if (m_isDragging)
        {
            OnDragEnd();
        }

        // 상태 초기화
        m_isClicking = false;
        m_isDragging = false;
        m_detectedDirection = DragDirection.None;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        // None 모드나 ClickOnly 모드에서는 드래그 불가
        if (m_interactionMode == InteractionMode.None || m_interactionMode == InteractionMode.ClickOnly) return;

        Vector2 mouseMovement = eventData.position - m_startMousePosition;

        // 드래그 시작 판단
        if (!m_isDragging && mouseMovement.magnitude > m_dragThreshold)
        {
            m_isDragging = true;
            m_isClicking = false; // 드래그가 시작되면 클릭 취소
            OnDragStart();
        }

        // 드래그 중 처리
        if (m_isDragging)
        {
            // 방향 감지
            if (m_enableDirectionDetection && m_detectedDirection == DragDirection.None)
            {
                DetectDragDirection(mouseMovement);
            }

            OnDragUpdate();
        }

        m_lastMousePosition = eventData.position;
    }
    #endregion

    #region 드래그 처리
    private void HandleDragFollow()
    {
        if (m_camera == null) return;

        Vector3 mouseWorldPosition = m_camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, m_camera.WorldToScreenPoint(transform.position).z));
        Vector3 offset = mouseWorldPosition - m_camera.ScreenToWorldPoint(new Vector3(m_startMousePosition.x, m_startMousePosition.y, m_camera.WorldToScreenPoint(m_startObjectPosition).z));

        Vector3 newPosition = m_startObjectPosition + offset * m_dragSensitivity;

        // 축 제약 적용
        if (m_constraintX) newPosition.x = transform.position.x;
        if (m_constraintY) newPosition.y = transform.position.y;
        if (m_constraintZ) newPosition.z = transform.position.z;

        // 범위 제한 적용
        if (m_enableDragBounds)
        {
            newPosition = ClampToBounds(newPosition);
        }

        transform.position = newPosition;
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        Vector3 boundsCenter = m_useWorldSpace ? m_boundsCenter : transform.parent ? transform.parent.TransformPoint(m_boundsCenter) : m_boundsCenter;

        switch (m_boundsType)
        {
            case DragBoundsType.Box:
                return ClampToBox(position, boundsCenter);
            case DragBoundsType.Circle:
                return ClampToCircle(position, boundsCenter);
            case DragBoundsType.Custom:
                return ClampToCustomBounds(position, boundsCenter);
            default:
                return position;
        }
    }

    private Vector3 ClampToBox(Vector3 position, Vector3 center)
    {
        Vector3 halfSize = m_boundsSize * 0.5f;
        Vector3 clampedPosition = position;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, center.x - halfSize.x, center.x + halfSize.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, center.y - halfSize.y, center.y + halfSize.y);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, center.z - halfSize.z, center.z + halfSize.z);

        return clampedPosition;
    }

    private Vector3 ClampToCircle(Vector3 position, Vector3 center)
    {
        Vector3 direction = position - center;
        float distance = direction.magnitude;

        if (distance > m_boundsRadius)
        {
            return center + direction.normalized * m_boundsRadius;
        }

        return position;
    }

    /// <summary>
    /// 커스텀 범위 제한 (하위 클래스에서 오버라이드 가능)
    /// </summary>
    protected virtual Vector3 ClampToCustomBounds(Vector3 position, Vector3 center)
    {
        // 기본적으로는 원형 범위와 동일하게 작동
        return ClampToCircle(position, center);
    }

    private void DetectDragDirection(Vector2 mouseMovement)
    {
        if (mouseMovement.magnitude < m_directionThreshold) return;

        float absX = Mathf.Abs(mouseMovement.x);
        float absY = Mathf.Abs(mouseMovement.y);

        // 2D 방향 감지 (화면 기준)
        if (absX > absY)
        {
            // 수평 이동이 더 클 때
            m_detectedDirection = mouseMovement.x > 0 ? DragDirection.Right : DragDirection.Left;
        }
        else
        {
            // 수직 이동이 더 클 때
            m_detectedDirection = mouseMovement.y > 0 ? DragDirection.Up : DragDirection.Down;
        }

        
        OnDragDirection(m_detectedDirection);
    }
    #endregion

    #region 가상 메서드 (오버라이드 가능)
    /// <summary>
    /// 클릭했을 때 실행되는 메서드
    /// </summary>
    protected virtual void OnClick()
    {
        // 하위 클래스에서 구현
        if (m_enableVisualFeedback)
        {
            AnimateScale(OriginalScale * 1.1f);
            DOVirtual.DelayedCall(m_animationDuration, () =>
            {
                AnimateScale(OriginalScale);
            });
        }
    }

    /// <summary>
    /// 드래그 시작 시 실행되는 메서드
    /// </summary>
    protected virtual void OnDragStart()
    {
        // 진행 중인 복귀 애니메이션이 있으면 중단
        if (m_returnTween != null && m_returnTween.IsActive())
        {
            m_returnTween.Kill();
        }

        // returnToOriginalPosition이 false일 때는 현재 위치를 드래그 시작점으로 설정
        if (!m_returnToOriginalPosition)
        {
            m_startObjectPosition = transform.position;
        }
        // 시각적 피드백
        if (m_enableVisualFeedback)
        {
            AnimateScale(OriginalScale * 1.1f);
        }
        // 하위 클래스에서 구현
    }

    /// <summary>
    /// 드래그 중 지속적으로 실행되는 메서드
    /// </summary>
    protected virtual void OnDragUpdate()
    {
        // 하위 클래스에서 구현
    }

    /// <summary>
    /// 드래그 종료 시 실행되는 메서드
    /// </summary>
    protected virtual void OnDragEnd()
    {
        // 진행 중인 복귀 애니메이션이 있으면 정리
        if (m_returnTween != null && m_returnTween.IsActive())
        {
            m_returnTween.Kill();
        }

        // 드래그 종료 시 원래 자리로 복귀 (우선순위 높음)
        if (m_returnToOriginalPosition)
        {
            m_returnTween = transform.DOMove(m_startObjectPosition, m_returnDuration)
                .SetEase(m_returnEase)
                .OnComplete(() => OnReturnToOriginalComplete());
        }
        // 원래 자리로 복귀하지 않을 때만 범위 내로 복귀
        else if (m_enableDragBounds && m_returnToBoundsOnRelease)
        {
            Vector3 clampedPosition = ClampToBounds(transform.position);
            if (Vector3.Distance(transform.position, clampedPosition) > 0.01f)
            {
                m_returnTween = transform.DOMove(clampedPosition, m_returnDuration)
                    .SetEase(m_returnEase)
                    .OnComplete(() => OnReturnToBoundsComplete());
            }
        }

        // 시각적 피드백 복구
        if (m_enableVisualFeedback)
        {
            AnimateScale(OriginalScale);
        }
    }

    /// <summary>
    /// 원래 자리로 복귀 완료 시 실행되는 메서드
    /// </summary>
    protected virtual void OnReturnToOriginalComplete()
    {
        // 하위 클래스에서 구현
    }

    /// <summary>
    /// 범위 내로 복귀 완료 시 실행되는 메서드
    /// </summary>
    protected virtual void OnReturnToBoundsComplete()
    {
        // 하위 클래스에서 구현
    }

    /// <summary>
    /// 특정 방향으로 드래그했을 때 실행되는 메서드
    /// </summary>
    /// <param name="direction">감지된 드래그 방향</param>
    protected virtual void OnDragDirection(DragDirection direction)
    {
        // 하위 클래스에서 구현
    }
    #endregion

    #region 공개 메서드
    /// <summary>
    /// 상호작용 모드 설정
    /// </summary>
    public void SetInteractionMode(InteractionMode mode)
    {
        m_interactionMode = mode;
    }

    /// <summary>
    /// 드래그 따라오기 활성화/비활성화
    /// </summary>
    public void SetDragFollowEnabled(bool enabled)
    {
        m_enableDragFollow = enabled;
    }

    /// <summary>
    /// 축 제약 설정
    /// </summary>
    public void SetConstraints(bool constraintX, bool constraintY, bool constraintZ)
    {
        m_constraintX = constraintX;
        m_constraintY = constraintY;
        m_constraintZ = constraintZ;
    }

    /// <summary>
    /// 방향 감지 활성화/비활성화
    /// </summary>
    public void SetDirectionDetectionEnabled(bool enabled)
    {
        m_enableDirectionDetection = enabled;
    }

    /// <summary>
    /// 드래그 범위 제한 활성화/비활성화
    /// </summary>
    public void SetDragBoundsEnabled(bool enabled)
    {
        m_enableDragBounds = enabled;
    }

    /// <summary>
    /// 드래그 범위 타입 설정
    /// </summary>
    public void SetDragBoundsType(DragBoundsType boundsType)
    {
        m_boundsType = boundsType;
    }

    /// <summary>
    /// 박스 범위 설정
    /// </summary>
    public void SetBoxBounds(Vector3 center, Vector3 size, bool useWorldSpace = false)
    {
        m_boundsCenter = center;
        m_boundsSize = size;
        m_useWorldSpace = useWorldSpace;
        m_boundsType = DragBoundsType.Box;
    }

    /// <summary>
    /// 원형 범위 설정
    /// </summary>
    public void SetCircleBounds(Vector3 center, float radius, bool useWorldSpace = false)
    {
        m_boundsCenter = center;
        m_boundsRadius = radius;
        m_useWorldSpace = useWorldSpace;
        m_boundsType = DragBoundsType.Circle;
    }

    /// <summary>
    /// 범위 내로 즉시 이동
    /// </summary>
    public void ClampToBoundsImmediate()
    {
        if (m_enableDragBounds)
        {
            transform.position = ClampToBounds(transform.position);
        }
    }

    /// <summary>
    /// 현재 위치가 범위 내에 있는지 확인
    /// </summary>
    public bool IsWithinBounds()
    {
        if (!m_enableDragBounds) return true;

        Vector3 clampedPosition = ClampToBounds(transform.position);
        return Vector3.Distance(transform.position, clampedPosition) < 0.01f;
    }

    /// <summary>
    /// 현재 드래그 상태 반환
    /// </summary>
    public bool IsDragging => m_isDragging;

    /// <summary>
    /// 현재 감지된 드래그 방향 반환
    /// </summary>
    public DragDirection DetectedDirection => m_detectedDirection;

    /// <summary>
    /// 현재 상호작용 모드 반환
    /// </summary>
    public InteractionMode CurrentInteractionMode => m_interactionMode;

    /// <summary>
    /// 드래그 따라오기 활성화 상태 반환
    /// </summary>
    public bool IsDragFollowEnabled => m_enableDragFollow;

    /// <summary>
    /// 축 제약 상태 반환
    /// </summary>
    public (bool X, bool Y, bool Z) GetConstraints => (m_constraintX, m_constraintY, m_constraintZ);

    /// <summary>
    /// 방향 감지 활성화 상태 반환
    /// </summary>
    public bool IsDirectionDetectionEnabled => m_enableDirectionDetection;

    /// <summary>
    /// 드래그 범위 제한 활성화 상태 반환
    /// </summary>
    public bool IsDragBoundsEnabled => m_enableDragBounds;

    /// <summary>
    /// 현재 드래그 범위 타입 반환
    /// </summary>
    public DragBoundsType CurrentBoundsType => m_boundsType;

    /// <summary>
    /// 원래 자리로 복귀 기능 활성화/비활성화
    /// </summary>
    public void SetReturnToOriginalEnabled(bool enabled)
    {
        m_returnToOriginalPosition = enabled;
    }

    /// <summary>
    /// 복귀 애니메이션 설정
    /// </summary>
    public void SetReturnAnimation(float duration, Ease easing)
    {
        m_returnDuration = duration;
        m_returnEase = easing;
    }

    /// <summary>
    /// 즉시 원래 자리로 복귀
    /// </summary>
    public void ReturnToOriginalImmediate()
    {
        if (m_returnTween != null && m_returnTween.IsActive())
        {
            m_returnTween.Kill();
        }
        transform.position = m_startObjectPosition;
    }

    /// <summary>
    /// 애니메이션으로 원래 자리로 복귀
    /// </summary>
    public void ReturnToOriginalAnimated()
    {
        if (m_returnTween != null && m_returnTween.IsActive())
        {
            m_returnTween.Kill();
        }

        m_returnTween = transform.DOMove(m_startObjectPosition, m_returnDuration)
            .SetEase(m_returnEase)
            .OnComplete(() => OnReturnToOriginalComplete());
    }

    /// <summary>
    /// 현재 복귀 중인지 확인
    /// </summary>
    public bool IsReturning => m_returnTween != null && m_returnTween.IsActive();

    /// <summary>
    /// 원래 자리로 복귀 기능 활성화 상태 반환
    /// </summary>
    public bool IsReturnToOriginalEnabled => m_returnToOriginalPosition;

    /// <summary>
    /// 복귀 애니메이션 설정 반환
    /// </summary>
    public (float duration, Ease easing) GetReturnAnimationSettings => (m_returnDuration, m_returnEase);

    /// <summary>
    /// 드래그 시작 위치 반환
    /// </summary>
    public Vector3 StartPosition => m_startObjectPosition;
    #endregion

    #region 시각적 피드백 메서드

    /// <summary>
    /// 색상 변경 애니메이션
    /// </summary>
    /// <param name="color">변경할 색상</param>
    protected void ChangeColor(Color color)
    {
        if (m_renderer != null)
        {
            m_renderer.material.DOColor(color, m_animationDuration);
        }
    }

    /// <summary>
    /// 크기 변경 애니메이션
    /// </summary>
    /// <param name="targetScale">변경할 크기</param>
    protected void AnimateScale(Vector3 targetScale)
    {
        transform.DOScale(targetScale, m_animationDuration).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// 원래 크기로 복귀
    /// </summary>
    protected void ResetScale()
    {
        AnimateScale(m_originalScale);
    }

    /// <summary>
    /// 원래 크기 반환
    /// </summary>
    protected Vector3 OriginalScale => m_originalScale;

    /// <summary>
    /// 애니메이션 지속 시간 반환
    /// </summary>
    protected float AnimationDuration => m_animationDuration;

    /// <summary>
    /// 렌더러 컴포넌트 반환
    /// </summary>
    protected Renderer RendererComponent => m_renderer;

    #endregion
}