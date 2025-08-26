using UnityEngine;
using DG.Tweening;

public class CtrlCube : CtrlInteractable
{
    [Header("테스트 설정")]
    [SerializeField] private Color m_originalColor = Color.white;
    [SerializeField] private Color m_clickColor = Color.green;
    [SerializeField] private Color m_dragColor = Color.red;
    [SerializeField] private bool m_enableSoundFeedback = true;

    [Header("테스트 키 설정")]
    [SerializeField] private KeyCode m_toggleModeKey = KeyCode.M;
    [SerializeField] private KeyCode m_toggleFollowKey = KeyCode.F;
    [SerializeField] private KeyCode m_toggleConstraintKey = KeyCode.C;
    [SerializeField] private KeyCode m_toggleBoundsKey = KeyCode.B;
    [SerializeField] private KeyCode m_toggleBoundsTypeKey = KeyCode.T;

    // 컴포넌트 참조
    private AudioSource m_audioSource;
    private Vector3 m_originalPosition;
    private int m_clickCount = 0;
    private int m_dragCount = 0;

    protected override void Start()
    {
        base.Start();
        InitializeComponents();
        LogTestInstructions();
    }

    private void InitializeComponents()
    {
        // 렌더러 초기화 (색상 저장을 위해)
        if (RendererComponent == null)
        {
            
        }
        else
        {
            m_originalColor = RendererComponent.material.color;
        }

        // 오디오 소스 초기화
        m_audioSource = GetComponent<AudioSource>();
        if (m_audioSource == null && m_enableSoundFeedback)
        {
            
        }

        // 원본 상태 저장
        m_originalPosition = transform.position;
    }

    private void LogTestInstructions()
    {
        
        
        
        
        
        
        
        
        
        
        
    }

    protected override void Update()
    {
        base.Update();
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        // 상호작용 모드 전환
        if (Input.GetKeyDown(m_toggleModeKey))
        {
            ToggleInteractionMode();
        }

        // 드래그 따라오기 토글
        if (Input.GetKeyDown(m_toggleFollowKey))
        {
            ToggleDragFollow();
        }

        // 축 제약 토글
        if (Input.GetKeyDown(m_toggleConstraintKey))
        {
            ToggleConstraints();
        }

        // 드래그 범위 토글
        if (Input.GetKeyDown(m_toggleBoundsKey))
        {
            ToggleBounds();
        }

        // 드래그 범위 유형 토글
        if (Input.GetKeyDown(m_toggleBoundsTypeKey))
        {
            ToggleBoundsType();
        }
    }

    #region CtrlInteractable 오버라이드 메서드

    protected override void OnClick()
    {
        base.OnClick();
        m_clickCount++;
        

        // 시각적 피드백

        ChangeColor(m_clickColor);

        // 사운드 피드백
        if (m_enableSoundFeedback && m_audioSource != null)
        {
            m_audioSource.pitch = 1.0f;
            m_audioSource.Play();
        }

        // 잠시 후 원래 상태로 복구
        DOVirtual.DelayedCall(AnimationDuration, () =>
        {
            ChangeColor(m_originalColor);
        });
    }

    protected override void OnDragStart()
    {
        base.OnDragStart();
        m_dragCount++;
        
        ChangeColor(m_dragColor);

        // 사운드 피드백
        if (m_enableSoundFeedback && m_audioSource != null)
        {
            m_audioSource.pitch = 1.5f;
            m_audioSource.Play();
        }
    }

    protected override void OnDragUpdate()
    {
        // 드래그 중 지속적으로 실행
        // 
    }

    protected override void OnDragEnd()
    {
        
        base.OnDragEnd();
        // 시각적 피드백 복구
 
            ChangeColor(m_originalColor);

        // 사운드 피드백
        if (m_enableSoundFeedback && m_audioSource != null)
        {
            m_audioSource.pitch = 0.8f;
            m_audioSource.Play();
        }
    }

    protected override void OnDragDirection(DragDirection direction)
    {
        string directionKorean = GetDirectionKorean(direction);
        

        // 방향에 따른 특별한 동작
        switch (direction)
        {
            case DragDirection.Up:
                
                break;
            case DragDirection.Down:
                
                break;
            case DragDirection.Left:
                
                break;
            case DragDirection.Right:
                
                break;
        }
    }

    #endregion

    #region 테스트 기능 메서드

    private void ToggleInteractionMode()
    {
        InteractionMode[] modes = { InteractionMode.Both, InteractionMode.ClickOnly, InteractionMode.DragOnly };
        int currentIndex = System.Array.IndexOf(modes, CurrentInteractionMode);
        int nextIndex = (currentIndex + 1) % modes.Length;

        SetInteractionMode(modes[nextIndex]);
        
    }

    private void ToggleDragFollow()
    {
        bool newState = !IsDragFollowEnabled;
        SetDragFollowEnabled(newState);
        
    }

    private void ToggleConstraints()
    {
        var constraints = GetConstraints;
        bool newState = !constraints.X;
        SetConstraints(newState, newState, newState);
        
    }

    private void ToggleBounds()
    {
        bool newState = !IsDragBoundsEnabled;
        SetDragBoundsEnabled(newState);
        

        // 처음 활성화할 때 기본 박스 범위 설정
        if (newState)
        {
            SetBoxBounds(m_originalPosition, Vector3.one * 3f, true);
            
        }
    }

    private void ToggleBoundsType()
    {
        if (!IsDragBoundsEnabled)
        {
            
            return;
        }

        DragBoundsType[] types = { DragBoundsType.Box, DragBoundsType.Circle, DragBoundsType.Custom };
        int currentIndex = System.Array.IndexOf(types, CurrentBoundsType);
        int nextIndex = (currentIndex + 1) % types.Length;

        switch (types[nextIndex])
        {
            case DragBoundsType.Box:
                SetBoxBounds(m_originalPosition, Vector3.one * 3f, true);
                
                break;
            case DragBoundsType.Circle:
                SetCircleBounds(m_originalPosition, 2f, true);
                
                break;
            case DragBoundsType.Custom:
                SetDragBoundsType(DragBoundsType.Custom);
                
                break;
        }
    }

    #endregion

    #region 애니메이션 메서드

    private void AnimateJump()
    {
        transform.DOJump(m_originalPosition, 1f, 1, AnimationDuration * 2f);
    }

    private void AnimateSquash()
    {
        Vector3 squashScale = new Vector3(OriginalScale.x * 1.2f, OriginalScale.y * 0.8f, OriginalScale.z * 1.2f);
        transform.DOScale(squashScale, AnimationDuration * 0.5f)
            .OnComplete(() => transform.DOScale(OriginalScale, AnimationDuration * 0.5f));
    }

    private void AnimateRotate(float angle)
    {
        transform.DORotate(new Vector3(0, angle, 0), AnimationDuration, RotateMode.LocalAxisAdd);
    }

    private void AnimateMove(Vector3 direction)
    {
        Vector3 targetPos = m_originalPosition + direction * 2f;
        transform.DOMove(targetPos, AnimationDuration)
            .OnComplete(() => transform.DOMove(m_originalPosition, AnimationDuration));
    }

    #endregion

    #region 유틸리티 메서드

    private string GetDirectionKorean(DragDirection direction)
    {
        switch (direction)
        {
            case DragDirection.Up: return "위";
            case DragDirection.Down: return "아래";
            case DragDirection.Left: return "왼쪽";
            case DragDirection.Right: return "오른쪽";
            default: return "없음";
        }
    }

    private string GetModeKorean(InteractionMode mode)
    {
        switch (mode)
        {
            case InteractionMode.Both: return "클릭+드래그";
            case InteractionMode.ClickOnly: return "클릭만";
            case InteractionMode.DragOnly: return "드래그만";
            default: return "알 수 없음";
        }
    }

    #endregion

    #region 디버그 메서드

    [ContextMenu("테스트 정보 출력")]
    private void PrintTestInfo()
    {
        
        
        
        
        
        
        
        
        if (IsDragBoundsEnabled)
        {
            
            
        }
        
        
    }

    [ContextMenu("상태 초기화")]
    private void ResetTestState()
    {
        m_clickCount = 0;
        m_dragCount = 0;
        transform.position = m_originalPosition;
        transform.localScale = OriginalScale;
        transform.rotation = Quaternion.identity;

        if (RendererComponent != null)
        {
            RendererComponent.material.color = m_originalColor;
        }

        
    }

    private string GetBoundsTypeKorean(DragBoundsType type)
    {
        switch (type)
        {
            case DragBoundsType.Box: return "박스";
            case DragBoundsType.Circle: return "원형";
            case DragBoundsType.Custom: return "커스텀";
            default: return "알 수 없음";
        }
    }

    #endregion
}
