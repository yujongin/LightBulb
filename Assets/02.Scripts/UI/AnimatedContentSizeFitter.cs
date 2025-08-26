using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class AnimatedContentSizeFitter : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Ease animationEase = Ease.OutQuart;
    [SerializeField] private bool animateOnStart = true;
    
    [Header("Size Settings")]
    [SerializeField] private bool fitWidth = false;
    [SerializeField] private bool fitHeight = true;
    [SerializeField] private float padding = 0f;
    [SerializeField] private bool includeInactiveChildren = false;
    
    [Header("Split Line Settings")]
    [SerializeField] private GameObject splitLine;
    
    [Header("Layout Detection")]
    [SerializeField] private bool autoDetectLayoutGroup = true;
    [SerializeField] private float updateInterval = 0.1f;
    
    private ScrollViewAutoResizer scrollViewAutoResizer;
    private RectTransform rectTransform;
    private VerticalLayoutGroup verticalLayoutGroup;
    private HorizontalLayoutGroup horizontalLayoutGroup;
    private GridLayoutGroup gridLayoutGroup;
    private Tween sizeTween;
    private Vector2 lastCalculatedSize;
    private int lastChildCount;
    private bool isInitialized = false;

#if UNITY_EDITOR
    private Vector2 lastEditorSize;
    private int lastEditorChildCount;
#endif
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        scrollViewAutoResizer = FindFirstObjectByType<ScrollViewAutoResizer>();
        CacheLayoutGroups();
        lastChildCount = GetActiveChildCount();

#if UNITY_EDITOR
        lastEditorChildCount = lastChildCount;
        lastEditorSize = rectTransform.sizeDelta;
#endif
    }
    
    private void Start()
    {
        if (Application.isPlaying)
        {
            if (animateOnStart)
            {
                StartCoroutine(DelayedInitialization());
            }
            else
            {
                Initialize();
            }
        }
        else
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += () => UpdateSize(false);
#endif
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            EditorApplication.delayCall += () => {
                if (this != null && gameObject != null)
                {
                    // 필수 컴포넌트들이 null인지 확인
                    if (rectTransform == null)
                        rectTransform = GetComponent<RectTransform>();
                    
                    if (rectTransform != null)
                    {
                        CacheLayoutGroups();
                        UpdateSize(false);
                    }
                }
            };
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            CheckEditorChanges();
        }
    }

    private void CheckEditorChanges()
    {
        // 필수 컴포넌트 null 체크
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform == null || this == null || gameObject == null) return;

        int currentChildCount = GetActiveChildCount();
        Vector2 currentRequiredSize = CalculateRequiredSize();
        
        bool sizeChanged = !Mathf.Approximately(currentRequiredSize.x, lastEditorSize.x) || 
                          !Mathf.Approximately(currentRequiredSize.y, lastEditorSize.y);
        bool childCountChanged = currentChildCount != lastEditorChildCount;
        
        if (sizeChanged || childCountChanged)
        {
            UpdateSize(false);
            lastEditorChildCount = currentChildCount;
            lastEditorSize = currentRequiredSize;
        }
        
        // SplitLine 오브젝트 활성화/비활성화 처리 (에디터)
        UpdateSplitLineVisibility(currentChildCount);
    }
#endif
    
    private IEnumerator DelayedInitialization()
    {
        yield return new WaitForEndOfFrame();
        Initialize();
    }
    
    private void Initialize()
    {
        UpdateSize(false);
        isInitialized = true;
        
        // 초기 splitLine 상태 설정
        UpdateSplitLineVisibility(GetActiveChildCount());
        
        StartCoroutine(UpdateLoop());
    }
    
    private void CacheLayoutGroups()
    {
        if (autoDetectLayoutGroup)
        {
            verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
            horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
            gridLayoutGroup = GetComponent<GridLayoutGroup>();
        }
    }
    
    private IEnumerator UpdateLoop()
    {
        while (this != null && gameObject != null)
        {
            yield return new WaitForSeconds(updateInterval);
            CheckForChanges();
        }
    }
    
    private void CheckForChanges()
    {
        if (!isInitialized) return;
        
        int currentChildCount = GetActiveChildCount();
        Vector2 currentRequiredSize = CalculateRequiredSize();
        
        bool sizeChanged = !Mathf.Approximately(currentRequiredSize.x, lastCalculatedSize.x) || 
                          !Mathf.Approximately(currentRequiredSize.y, lastCalculatedSize.y);
        bool childCountChanged = currentChildCount != lastChildCount;
        
        if (sizeChanged || childCountChanged)
        {
            UpdateSize(true);
            lastChildCount = currentChildCount;
        }
        
        // SplitLine 오브젝트 활성화/비활성화 처리
        UpdateSplitLineVisibility(currentChildCount);
    }
    
    private void UpdateSplitLineVisibility(int childCount)
    {
        if (splitLine != null)
        {
            splitLine.SetActive(childCount > 0);
        }
    }
    
    private int GetActiveChildCount()
    {
        // transform이 null인지 확인
        if (transform == null)
            return 0;
        
        int count = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child != null && (includeInactiveChildren || child.gameObject.activeInHierarchy))
            {
                count++;
            }
        }
        return count;
    }
    
    private Vector2 CalculateRequiredSize()
    {
        // 컴포넌트들이 null인지 확인
        if (this == null || gameObject == null || transform == null)
            return Vector2.zero;
        
        Vector2 requiredSize = Vector2.zero;
        
        if (verticalLayoutGroup != null)
        {
            requiredSize = CalculateVerticalLayoutSize();
        }
        else if (horizontalLayoutGroup != null)
        {
            requiredSize = CalculateHorizontalLayoutSize();
        }
        else if (gridLayoutGroup != null)
        {
            requiredSize = CalculateGridLayoutSize();
        }
        else
        {
            requiredSize = CalculateManualSize();
        }
        
        requiredSize.x += padding * 2;
        requiredSize.y += padding * 2;
        
        return requiredSize;
    }
    
    private Vector2 CalculateVerticalLayoutSize()
    {
        // null 체크
        if (verticalLayoutGroup == null || transform == null)
            return Vector2.zero;
        
        float totalHeight = verticalLayoutGroup.padding.top + verticalLayoutGroup.padding.bottom;
        float maxWidth = 0f;
        int activeChildren = 0;
        
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null || (!includeInactiveChildren && !child.gameObject.activeInHierarchy)) continue;
            
            RectTransform childRect = child as RectTransform;
            if (childRect != null)
            {
                totalHeight += childRect.sizeDelta.y;
                maxWidth = Mathf.Max(maxWidth, childRect.sizeDelta.x);
                activeChildren++;
            }
        }
        
        if (activeChildren > 1)
        {
            totalHeight += verticalLayoutGroup.spacing * (activeChildren - 1);
        }
        
        return new Vector2(maxWidth + verticalLayoutGroup.padding.left + verticalLayoutGroup.padding.right, totalHeight);
    }
    
    private Vector2 CalculateHorizontalLayoutSize()
    {
        // null 체크
        if (horizontalLayoutGroup == null || transform == null)
            return Vector2.zero;
        
        float totalWidth = horizontalLayoutGroup.padding.left + horizontalLayoutGroup.padding.right;
        float maxHeight = 0f;
        int activeChildren = 0;
        
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null || (!includeInactiveChildren && !child.gameObject.activeInHierarchy)) continue;
            
            RectTransform childRect = child as RectTransform;
            if (childRect != null)
            {
                totalWidth += childRect.sizeDelta.x;
                maxHeight = Mathf.Max(maxHeight, childRect.sizeDelta.y);
                activeChildren++;
            }
        }
        
        if (activeChildren > 1)
        {
            totalWidth += horizontalLayoutGroup.spacing * (activeChildren - 1);
        }
        
        return new Vector2(totalWidth, maxHeight + horizontalLayoutGroup.padding.top + horizontalLayoutGroup.padding.bottom);
    }
    
    private Vector2 CalculateGridLayoutSize()
    {
        // null 체크
        if (gridLayoutGroup == null)
            return Vector2.zero;
        
        int activeChildren = GetActiveChildCount();
        if (activeChildren == 0) return Vector2.zero;
        
        Vector2 cellSize = gridLayoutGroup.cellSize;
        Vector2 spacing = gridLayoutGroup.spacing;
        RectOffset padding = gridLayoutGroup.padding;
        
        int constraintCount = gridLayoutGroup.constraintCount;
        
        int columns, rows;
        if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            columns = constraintCount;
            rows = Mathf.CeilToInt((float)activeChildren / columns);
        }
        else if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedRowCount)
        {
            rows = constraintCount;
            columns = Mathf.CeilToInt((float)activeChildren / rows);
        }
        else
        {
            columns = Mathf.CeilToInt(Mathf.Sqrt(activeChildren));
            rows = Mathf.CeilToInt((float)activeChildren / columns);
        }
        
        float totalWidth = columns * cellSize.x + (columns - 1) * spacing.x + padding.left + padding.right;
        float totalHeight = rows * cellSize.y + (rows - 1) * spacing.y + padding.top + padding.bottom;
        
        return new Vector2(totalWidth, totalHeight);
    }
    
    private Vector2 CalculateManualSize()
    {
        // null 체크
        if (transform == null)
            return Vector2.zero;
        
        Vector2 totalSize = Vector2.zero;
        
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null || (!includeInactiveChildren && !child.gameObject.activeInHierarchy)) continue;
            
            RectTransform childRect = child as RectTransform;
            if (childRect != null)
            {
                if (fitHeight)
                {
                    totalSize.y += childRect.sizeDelta.y;
                }
                if (fitWidth)
                {
                    totalSize.x = Mathf.Max(totalSize.x, childRect.sizeDelta.x);
                }
            }
        }
        
        return totalSize;
    }
    
    public void UpdateSize(bool animate = true)
    {
        // 필수 컴포넌트 null 체크
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform == null)
        {
            
            return;
        }
        
        Vector2 requiredSize = CalculateRequiredSize();
        lastCalculatedSize = requiredSize;
        
        Vector2 currentSize = rectTransform.sizeDelta;
        Vector2 targetSize = new Vector2(
            fitWidth ? requiredSize.x : currentSize.x,
            fitHeight ? requiredSize.y : currentSize.y
        );
        
        // 에디터에서는 항상 애니메이션 없이 즉시 업데이트
        bool shouldAnimate = animate && animationDuration > 0 && Application.isPlaying;
        
        if (shouldAnimate)
        {
            AnimateToSize(targetSize);
        }
        else
        {
            rectTransform.sizeDelta = targetSize;
        }
    }
    
    private void AnimateToSize(Vector2 targetSize)
    {
        if (sizeTween != null && sizeTween.IsActive())
        {
            sizeTween.Kill();
        }
        sizeTween = rectTransform.DOSizeDelta(targetSize, animationDuration)
            .SetEase(animationEase)
            .SetUpdate(true);
    }
    
    public void ForceUpdateSize()
    {
        UpdateSize(false);
    }
    
    
    private void OnDestroy()
    {
        if (sizeTween != null && sizeTween.IsActive())
        {
            sizeTween.Kill();
        }
    }
    
    private void OnTransformChildrenChanged()
    {
        // 컴포넌트가 null인지 확인
        if (this == null || gameObject == null)
            return;
        
        if (Application.isPlaying)
        {
            if (isInitialized)
            {
                StartCoroutine(DelayedUpdate());
            }
        }
        else
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += () => {
                if (this != null && gameObject != null)
                {
                    UpdateSize(false);
                }
            };
#endif
        }
    }
    
    private IEnumerator DelayedUpdate()
    {
        yield return new WaitForEndOfFrame();
        UpdateSize(true);
    }
} 