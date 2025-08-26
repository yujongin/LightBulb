using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

/// <summary>
/// ScrollView의 Content 크기에 맞춰서 ScrollView 자체의 높이를 자동으로 조정하는 스크립트
/// Content의 sizeDelta.y 값을 감지하여 ScrollView의 높이를 실시간으로 조정합니다.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(ScrollRect))]
public class ScrollViewAutoResizer : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;    // 크기 변경 애니메이션 지속 시간
    [SerializeField] private Ease animationEase = Ease.OutQuart; // 크기 변경 애니메이션 이징 타입
    [SerializeField] private bool animateOnStart = true;         // 시작 시 애니메이션 사용 여부

    [Header("Size Settings")]
    [SerializeField] private float heightOffset = 0f;    // Content 높이에 추가할 오프셋 값
    [SerializeField] private float maxHeight = 500f;     // ScrollView 최대 높이 제한값
    [SerializeField] private bool useMaxHeight = false;  // 최대 높이 제한 사용 여부

    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 0.1f; // 크기 변화 감지 주기 (초)
    [SerializeField] private bool autoUpdate = true;      // 자동 업데이트 사용 여부

    // 컴포넌트 참조
    private ScrollRect m_scrollRect;        // ScrollView의 ScrollRect 컴포넌트
    private RectTransform m_rectTransform;  // ScrollView의 RectTransform
    private RectTransform m_contentRect;    // ScrollView Content의 RectTransform

    // 애니메이션 관련
    private Tween m_sizeTween;             // 크기 변경 애니메이션 트윈

    // 상태 관리
    private float m_lastContentHeight;     // 이전 프레임의 Content 높이
    private bool m_isInitialized = false; // 초기화 완료 여부

#if UNITY_EDITOR
    private float m_lastEditorContentHeight; // 에디터용 이전 Content 높이
#endif

    /// <summary>
    /// 컴포넌트 초기화 및 참조 설정
    /// </summary>
    private void Awake()
    {
        m_scrollRect = GetComponent<ScrollRect>();
        m_rectTransform = GetComponent<RectTransform>();

        if (m_scrollRect.content != null)
        {
            m_contentRect = m_scrollRect.content;
            m_lastContentHeight = m_contentRect.sizeDelta.y;

#if UNITY_EDITOR
            m_lastEditorContentHeight = m_lastContentHeight;
#endif
        }
    }

    /// <summary>
    /// 스크립트 시작 시 초기화
    /// </summary>
    private void Start()
    {
        if (Application.isPlaying)
        {
            if (animateOnStart)
            {
                // 애니메이션과 함께 초기화
                StartCoroutine(DelayedInitialization());
            }
            else
            {
                // 즉시 초기화
                Initialize();
            }
        }
        else
        {
#if UNITY_EDITOR
            // 에디터에서는 즉시 크기 조정
            UnityEditor.EditorApplication.delayCall += () => UpdateScrollViewSize(false);
#endif
        }
    }

    /// <summary>
    /// 한 프레임 후 초기화를 위한 코루틴
    /// </summary>
    private IEnumerator DelayedInitialization()
    {
        yield return new WaitForEndOfFrame();
        Initialize();
    }

    /// <summary>
    /// 스크립트 초기화 실행
    /// </summary>
    private void Initialize()
    {
        if (m_contentRect == null)
        {
            
            return;
        }

        // 초기 크기 설정 (애니메이션 없이)
        UpdateScrollViewSize(false);
        m_isInitialized = true;

        // 자동 업데이트 활성화
        if (autoUpdate)
        {
            StartCoroutine(UpdateLoop());
        }
    }

    /// <summary>
    /// 주기적으로 Content 크기 변화를 감지하는 업데이트 루프
    /// </summary>
    private IEnumerator UpdateLoop()
    {
        while (this != null && gameObject != null)
        {
            yield return new WaitForSeconds(updateInterval);
            CheckForChanges();
        }
    }

    /// <summary>
    /// Content 크기 변화를 확인하고 필요시 ScrollView 크기 조정
    /// </summary>
    private void CheckForChanges()
    {
        if (!m_isInitialized || m_contentRect == null) return;
        m_contentRect = m_scrollRect.content;
        float currentContentHeight = m_contentRect.sizeDelta.y;

        // 높이 변화가 있는지 확인
        bool heightChanged = !Mathf.Approximately(currentContentHeight, m_lastContentHeight);

        if (heightChanged)
        {
            // 애니메이션과 함께 크기 조정
            UpdateScrollViewSize(true);
            m_lastContentHeight = currentContentHeight;
        }
    }

    /// <summary>
    /// ScrollView의 크기를 Content에 맞춰서 조정
    /// </summary>
    /// <param name="animate">애니메이션 사용 여부</param>
    public void UpdateScrollViewSize(bool animate = true)
    {
        if (m_contentRect == null || m_rectTransform == null) return;

        // Content의 크기를 직접 계산하고 오프셋 추가
        float contentHeight = m_contentRect.sizeDelta.y + heightOffset;

        // 최대 높이 제한 적용
        if (useMaxHeight && contentHeight > maxHeight)
        {
            contentHeight = maxHeight;
        }

        Vector2 currentSize = m_rectTransform.sizeDelta;
        Vector2 targetSize = new Vector2(currentSize.x, contentHeight);

        // 애니메이션 조건 확인
        bool shouldAnimate = animate && animationDuration > 0 && Application.isPlaying;

        if (shouldAnimate)
        {
            // 기존 애니메이션이 있으면 중지
            if (m_sizeTween != null && m_sizeTween.IsActive())
            {
                m_sizeTween.Kill();
            }

            // 새로운 애니메이션 시작
            m_sizeTween = m_rectTransform.DOSizeDelta(targetSize, animationDuration)
                .SetEase(animationEase)
                .SetUpdate(true);
        }
        else
        {
            // 즉시 크기 변경
            m_rectTransform.sizeDelta = targetSize;
        }
    }

    /// <summary>
    /// 스크립트 파괴 시 애니메이션 정리
    /// </summary>
    private void OnDestroy()
    {
        if (m_sizeTween != null && m_sizeTween.IsActive())
        {
            m_sizeTween.Kill();
        }
    }

    /// <summary>
    /// 에디터 모드에서 지속적인 업데이트
    /// </summary>
    private void Update()
    {
        if (!Application.isPlaying)
        {
#if UNITY_EDITOR
            CheckEditorChanges();
#endif
        }
    }


#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 Content 크기 변화 감지
    /// </summary>
    private void CheckEditorChanges()
    {
        if (m_contentRect == null) return;

        float currentContentHeight = m_contentRect.sizeDelta.y;

        // 높이 변화가 있는지 확인
        bool heightChanged = !Mathf.Approximately(currentContentHeight, m_lastEditorContentHeight);

        if (heightChanged)
        {
            UpdateScrollViewSize(false);
            m_lastEditorContentHeight = currentContentHeight;
        }
    }


    /// <summary>
    /// 에디터에서 Inspector 값 변경 시 호출
    /// </summary>
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    UpdateScrollViewSize(false);
                }
            };
        }
    }
#endif
}