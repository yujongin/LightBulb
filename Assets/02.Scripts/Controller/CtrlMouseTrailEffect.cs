using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 마우스 드래그할 때 trail 효과를 제공하는 스크립트
/// </summary>
public class CtrlMouseTrailEffect : MonoBehaviour
{
    [Header("Trail 효과 설정")]
    [SerializeField] private bool enableTrail = true; // trail 효과 활성화 여부
    [SerializeField] private Color trailColor = Color.white; // trail 색상
    [SerializeField] private float trailWidth = 0.02f; // trail 두께
    [SerializeField] private float trailTime = 0.2f; // trail 지속 시간
    [SerializeField] private Material trailMaterial; // trail 머티리얼 (선택사항)
    [SerializeField] private float trailZPosition = 1f; // trail의 Z 위치
    
    [Header("드래그 설정")]
    [SerializeField] private bool onlyOnDrag = true; // 드래그 중일 때만 trail 표시
    [SerializeField] private float minDragDistance = 0.001f; // 최소 드래그 거리 (픽셀)
    
    [Header("이미지 드래그 감지")]
    [SerializeField] private bool enableImageDragDetection = false; // 이미지 드래그 감지 활성화
    [SerializeField] private Image targetImage; // 감지할 이미지
    [SerializeField] private bool showDebugInfo = true; // 디버그 정보 표시
    [SerializeField] private int stepIndex;
    
    // Trail 관련 변수
    private GameObject trailObject;
    private TrailRenderer trailRenderer;
    private bool isDragging = false;
    private Vector3 dragStartPosition;
    private Camera mainCamera;
    
    // 이미지 드래그 감지 관련 변수
    private bool isOnTargetImage = false;
    private bool dragStartedOnImage = false;
    private RectTransform imageRectTransform;

    // // 이벤트
    // public System.Action<bool> OnImageDragDetected; // 이미지 위에서 드래그 감지 시 호출되는 이벤트
    // public System.Action OnDragOnImage; // 이미지 위에서 드래그 완료 시 호출되는 이벤트

    private void Start()
    {
        mainCamera = Camera.main;
        
        // Trail 효과 초기화
        if (enableTrail)
        {
            CreateTrail();
        }
        
        // 이미지 드래그 감지 초기화
        if (enableImageDragDetection && targetImage != null)
        {
            imageRectTransform = targetImage.GetComponent<RectTransform>();
        }
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        // 마우스 왼쪽 버튼을 누를 때
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }
        
        // 마우스 드래그 중일 때
        if (Input.GetMouseButton(0))
        {
            UpdateDrag();
        }
        
        // 마우스 버튼을 놓을 때
        if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }

    private void StartDrag()
    {
        isDragging = true;
        dragStartPosition = Input.mousePosition;
        
        // 이미지 드래그 감지
        if (enableImageDragDetection)
        {
            isOnTargetImage = IsPositionOnImage(Input.mousePosition);
            dragStartedOnImage = isOnTargetImage;
            
            if (showDebugInfo)
            {
                
            }
        }
        
        // Trail 효과 활성화
        if (enableTrail && onlyOnDrag)
        {
            if (trailObject == null)
            {
                CreateTrail();
            }
            else
            {
                UpdateTrailPosition();
                trailObject.SetActive(true);
            }
        }
    }

    private void UpdateDrag()
    {
        if (!isDragging) return;

        // 이미지 드래그 감지 업데이트
        if (enableImageDragDetection)
        {
            isOnTargetImage = IsPositionOnImage(Input.mousePosition);
        }

        // 최소 드래그 거리 확인
        float dragDistance = Vector3.Distance(Input.mousePosition, dragStartPosition);
        if (dragDistance < minDragDistance) return;

        // Trail 위치 업데이트
        if (enableTrail && trailObject != null)
        {
            UpdateTrailPosition();
        }
    }

    private void EndDrag()
    {
        if (!isDragging) return;

        // 이미지 위에서 드래그 완료된 경우 이벤트 호출
        if (enableImageDragDetection && dragStartedOnImage)
        {
            float dragDistance = Vector3.Distance(Input.mousePosition, dragStartPosition);
            if (dragDistance >= minDragDistance)
            {
                // 드래그 시작과 끝 모두 이미지 위에서 이루어졌는지 확인
                bool isEndingOnImage = IsPositionOnImage(Input.mousePosition);
                if (isEndingOnImage)
                {
                    // OnDragOnImage?.Invoke();
                    Managers.Command.ExecuteCommand(stepIndex);
                    if (showDebugInfo)
                    {
                        
                    }
                }
                else if (showDebugInfo)
                {
                    
                }
            }
        }

        isDragging = false;
        
        // Trail 효과 비활성화 (onlyOnDrag가 true인 경우)
        if (enableTrail && onlyOnDrag && trailObject != null)
        {
            trailObject.SetActive(false);
        }
    }

    private bool IsPositionOnImage(Vector3 screenPosition)
    {
        if (targetImage == null || imageRectTransform == null) return false;
        
        // 이미지가 활성화되어 있는지 확인
        if (!targetImage.gameObject.activeInHierarchy) return false;
        
        // 스크린 좌표를 이미지의 로컬 좌표로 변환
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            imageRectTransform, 
            screenPosition, 
            null, // Canvas의 Camera (Screen Space - Overlay인 경우 null)
            out localPoint
        );
        
        // 이미지의 Rect 범위 내에 있는지 확인
        Rect imageRect = imageRectTransform.rect;
        return imageRect.Contains(localPoint);
    }

    private void CreateTrail()
    {
        // Trail 오브젝트 생성
        trailObject = new GameObject("MouseTrail");
        trailObject.transform.SetParent(transform);
        
        // TrailRenderer 컴포넌트 추가
        trailRenderer = trailObject.AddComponent<TrailRenderer>();
        
        // Trail 설정
        trailRenderer.time = trailTime;
        trailRenderer.startWidth = trailWidth;
        trailRenderer.endWidth = 0f;
        trailRenderer.material = trailMaterial != null ? trailMaterial : new Material(Shader.Find("Sprites/Default"));
        trailRenderer.startColor = trailColor;
        trailRenderer.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
        trailRenderer.minVertexDistance = minDragDistance;
        
        // Trail 위치 초기화
        UpdateTrailPosition();
        
        // onlyOnDrag가 true인 경우 초기에는 비활성화
        if (onlyOnDrag)
        {
            trailObject.SetActive(false);
        }
    }

    private void UpdateTrailPosition()
    {
        if (trailObject != null && mainCamera != null)
        {
            // 마우스 위치를 월드 좌표로 변환
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, trailZPosition));
            trailObject.transform.position = mouseWorldPos;
        }
    }

    // 타겟 이미지 설정
    public void SetTargetImage(Image newImage)
    {
        targetImage = newImage;
        if (targetImage != null)
        {
            imageRectTransform = targetImage.GetComponent<RectTransform>();
        }
    }

    // 현재 타겟 이미지 반환
    public Image GetTargetImage()
    {
        return targetImage;
    }

    // 이미지 드래그 감지 활성화/비활성화
    public void EnableImageDragDetection(bool enable)
    {
        enableImageDragDetection = enable;
    }

    // 디버그 정보 표시 설정
    public void SetShowDebugInfo(bool show)
    {
        showDebugInfo = show;
    }

    // Trail 설정을 동적으로 변경하는 메서드들
    public void SetTrailColor(Color color)
    {
        trailColor = color;
        if (trailRenderer != null)
        {
            trailRenderer.startColor = color;
            trailRenderer.endColor = new Color(color.r, color.g, color.b, 0f);
        }
    }

    public void SetTrailWidth(float width)
    {
        trailWidth = width;
        if (trailRenderer != null)
        {
            trailRenderer.startWidth = width;
        }
    }

    public void SetTrailTime(float time)
    {
        trailTime = time;
        if (trailRenderer != null)
        {
            trailRenderer.time = time;
        }
    }

    public void EnableTrail(bool enable)
    {
        enableTrail = enable;
        if (trailObject != null)
        {
            trailObject.SetActive(enable);
        }
    }

    public void SetOnlyOnDrag(bool onlyDrag)
    {
        onlyOnDrag = onlyDrag;
        if (trailObject != null && !onlyDrag)
        {
            trailObject.SetActive(true);
        }
    }

    // Trail을 완전히 제거하는 메서드
    public void DestroyTrail()
    {
        if (trailObject != null)
        {
            DestroyImmediate(trailObject);
            trailObject = null;
            trailRenderer = null;
        }
    }
} 