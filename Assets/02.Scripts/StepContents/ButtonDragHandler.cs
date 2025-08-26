using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// 버튼의 자식 Image를 드래그하여 특정 오브젝트에 올리면 짝이 되는 오브젝트를 생성하는 핸들러
/// </summary>
public class ButtonDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [System.Serializable]
    public class ObjectPair
    {
        public GameObject hologramObject; // 홀로그램 오브젝트 (복사본)
        public GameObject pairObject; // 올려두면 생길 오브젝트 (프리팹)
        public bool isCompleted = false; // 완성 여부
    }
    [SerializeField] private int stepIndex;

    // stepIndex에 대한 public 프로퍼티 추가
    public int StepIndex => stepIndex;

    [Header("Object Pairs")]
    [SerializeField] private List<ObjectPair> objectPairs = new List<ObjectPair>(); // 오브젝트 짝 목록

    [Header("Hologram Settings")]
    [SerializeField] private Material hologramMaterial; // 홀로그램 머티리얼

    private Image m_DragIcon; // 드래그할 이미지
    private Button m_Button; // 버튼 컴포넌트
    private Transform m_OriginalParent;
    private Vector3 m_OriginalPosition;
    private Canvas m_Canvas;
    private bool m_IsDragging = false;
    private bool m_IsOverTargetObject = false;

    [Header("Use Toggle")]
    [SerializeField] private bool isToggle = false;

    [Header("Events")]
    [SerializeField] private UnityEvent<GameObject> OnPairMatched; // 매치 성공 시 호출되는 이벤트 (매치된 pairObject 전달)

    private void Start()
    {
        m_Canvas = GetComponentInParent<Canvas>();
        if (m_DragIcon == null)
            m_DragIcon = transform.GetChild(0).GetComponent<Image>();
        if (m_Button == null)
            m_Button = GetComponent<Button>();

        CreateHologramObjects();
    }

    /// <summary>
    /// pairObject를 복사하여 홀로그램 오브젝트를 생성하고 홀로그램 머티리얼을 적용합니다
    /// </summary>
    private void CreateHologramObjects()
    {
        if (hologramMaterial == null)
        {
            
            return;
        }

        foreach (var pair in objectPairs)
        {
            if (pair.hologramObject == null)
            {
                // pairObject 복사
                pair.hologramObject = Instantiate(pair.pairObject);
                pair.hologramObject.name = pair.pairObject.name + "_Hologram";

                // 원본 pairObject와 같은 위치에 배치
                pair.hologramObject.transform.position = pair.pairObject.transform.position;
                pair.hologramObject.transform.rotation = pair.pairObject.transform.rotation;
                pair.hologramObject.transform.localScale = pair.pairObject.transform.localScale;

                // 불필요한 컴포넌트 제거 (MeshRenderer, MeshFilter, Collider만 남기고)
                RemoveUnnecessaryComponents(pair.hologramObject);

                // 홀로그램 오브젝트와 모든 자식의 Renderer에 홀로그램 머티리얼 적용
                ApplyHologramMaterial(pair.hologramObject);

                // 원본 pairObject는 비활성화
                pair.pairObject.SetActive(false);

                // 홀로그램 오브젝트 비활성화
                pair.hologramObject.SetActive(false);
            }
            string categoryName = RemoveNumberSuffix(pair.pairObject.name) + "_Hologram";
            Managers.Object.RegisterObject(pair.hologramObject.name, pair.hologramObject, categoryName);
        }
    }

    /// <summary>
    /// 문자열에서 마지막 밑줄과 숫자 부분을 제거합니다 (예: "TestObject_2" → "TestObject")
    /// </summary>
    private string RemoveNumberSuffix(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // 마지막 밑줄의 위치 찾기
        int lastUnderscoreIndex = input.LastIndexOf('_');

        // 밑줄이 없거나 마지막 문자라면 원본 반환
        if (lastUnderscoreIndex == -1 || lastUnderscoreIndex == input.Length - 1)
            return input;

        // 밑줄 뒤의 문자열 추출
        string suffix = input.Substring(lastUnderscoreIndex + 1);

        // 뒤의 문자들이 모두 숫자인지 확인
        bool isAllNumbers = true;
        foreach (char c in suffix)
        {
            if (!char.IsDigit(c))
            {
                isAllNumbers = false;
                break;
            }
        }

        // 모두 숫자라면 밑줄과 숫자 부분 제거
        if (isAllNumbers)
        {
            return input.Substring(0, lastUnderscoreIndex);
        }

        // 아니라면 원본 반환
        return input;
    }

    /// <summary>
    /// 홀로그램 오브젝트에서 MeshRenderer, MeshFilter, Collider를 제외한 불필요한 컴포넌트들을 제거합니다
    /// </summary>
    private void RemoveUnnecessaryComponents(GameObject obj)
    {
        // 자신과 모든 자식 오브젝트 처리
        RemoveComponentsFromObject(obj);
        
        // 모든 자식 오브젝트에 대해서도 재귀적으로 처리
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            RemoveUnnecessaryComponents(obj.transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 단일 오브젝트에서 필요한 컴포넌트만 남기고 나머지 제거
    /// </summary>
    private void RemoveComponentsFromObject(GameObject obj)
    {
        // 유지할 컴포넌트 타입들
        System.Type[] keepComponents = new System.Type[]
        {
            typeof(Transform),      // 기본 컴포넌트 (제거 불가)
            typeof(MeshRenderer),   // 렌더링용
            typeof(SkinnedMeshRenderer),   // 렌더링용
            typeof(MeshFilter),     // 메시 정보용 (MeshRenderer와 함께 필요)
            typeof(Collider),       // 콜라이더 (기본 클래스)
            typeof(BoxCollider),    // 박스 콜라이더
            typeof(SphereCollider), // 구체 콜라이더
            typeof(MeshCollider),   // 메시 콜라이더
            typeof(CapsuleCollider) // 캡슐 콜라이더
        };

        // 모든 컴포넌트 가져오기
        Component[] allComponents = obj.GetComponents<Component>();

        foreach (Component component in allComponents)
        {
            if (component == null) continue;

            // 유지할 컴포넌트인지 확인
            bool shouldKeep = false;
            foreach (System.Type keepType in keepComponents)
            {
                if (keepType.IsAssignableFrom(component.GetType()))
                {
                    shouldKeep = true;
                    break;
                }
            }

            // 유지하지 않을 컴포넌트라면 제거
            if (!shouldKeep)
            {
                if (Application.isPlaying)
                {
                    Destroy(component);
                }
                else
                {
                    DestroyImmediate(component);
                }
            }
        }
    }

    /// <summary>
    /// 오브젝트와 모든 자식의 Renderer에 홀로그램 머티리얼을 적용합니다
    /// </summary>
    private void ApplyHologramMaterial(GameObject obj)
    {
        // 자신의 Renderer에 홀로그램 머티리얼 적용
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material[] newMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = hologramMaterial;
            }
            renderer.materials = newMaterials;
        }

        // 모든 자식의 Renderer에도 홀로그램 머티리얼 적용
        Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer childRenderer in childRenderers)
        {
            if (childRenderer != renderer) // 자신은 이미 처리했으므로 제외
            {
                Material[] newMaterials = new Material[childRenderer.materials.Length];
                for (int i = 0; i < newMaterials.Length; i++)
                {
                    newMaterials[i] = hologramMaterial;
                }
                childRenderer.materials = newMaterials;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 버튼이 비활성화되어 있으면 드래그 시작하지 않음
        if (m_Button != null && !m_Button.interactable)
        {
            eventData.pointerDrag = null;
            return;
        }

        m_IsDragging = true;
        m_OriginalParent = m_DragIcon.transform.parent;
        m_OriginalPosition = m_DragIcon.transform.position;
        m_DragIcon.raycastTarget = false;
        m_DragIcon.transform.SetParent(m_Canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!m_IsDragging) return;

        m_DragIcon.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!m_IsDragging) return;

        m_IsDragging = false;
        m_IsOverTargetObject = false;

        // 3D 오브젝트에 닿았는지 확인
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {

            // 타겟 오브젝트인지 확인
            foreach (var pair in objectPairs)
            {
                if (hit.collider.gameObject == pair.hologramObject && !pair.isCompleted)
                {
                    hit.collider.gameObject.SetActive(false);
                    m_IsOverTargetObject = true;
                    break;
                }
            }
        }

        if (m_IsOverTargetObject)
        {
            // 짝이 되는 오브젝트 생성
            ActivatePairObject(hit.collider.gameObject);

            // 원래 위치로 복귀
            ReturnToOriginalPosition();
        }
        else
        {
            // 원래 위치로 복귀
            ReturnToOriginalPosition();
        }
    }

    private void ActivatePairObject(GameObject targetObject)
    {
        foreach (var pair in objectPairs)
        {
            if (pair.hologramObject == targetObject && !pair.isCompleted)
            {
                if (isToggle)
                    ResetAllPairs();

                // 짝이 되는 오브젝트 활성화
                if (pair.pairObject != null)
                {
                    // 홀로그램 오브젝트 비활성화
                    if (pair.hologramObject != null)
                    {
                        pair.hologramObject.SetActive(false);
                    }
                    // 원본 pairObject 활성화
                    pair.pairObject.SetActive(true);

                    pair.isCompleted = true;

                    // 매치 성공 이벤트 호출
                    OnPairMatched?.Invoke(pair.pairObject);

                    CheckAllPairsCompleted();
                }
                break;
            }
        }
    }

    private void ReturnToOriginalPosition()
    {
        m_DragIcon.transform.SetParent(m_OriginalParent);
        m_DragIcon.transform.SetAsFirstSibling();
        m_DragIcon.transform.position = m_OriginalPosition;
        m_DragIcon.raycastTarget = true;
    }

    private void CheckAllPairsCompleted()
    {
        bool allCompleted = true;
        foreach (var pair in objectPairs)
        {
            if (!pair.isCompleted)
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            Managers.Command.ExecuteCommand(stepIndex);

            // 모든 짝이 완성되면 버튼 비활성화
            if (m_Button != null)
            {
                m_Button.interactable = false;
                m_Button.gameObject.SetActive(false);
            }

            // 드래그 핸들러도 비활성화
            this.enabled = false;
        }
    }

    // 모든 짝을 리셋하는 메서드 (필요시 사용)
    public void ResetAllPairs()
    {
        foreach (var pair in objectPairs)
        {
            pair.isCompleted = false;

            // 원본 pairObject 비활성화
            pair.pairObject.SetActive(false);

            // 홀로그램 오브젝트 다시 활성화
            if (pair.hologramObject != null)
            {
                pair.hologramObject.SetActive(true);
            }
        }

        if (m_Button != null)
        {
            m_Button.interactable = true;
        }

        this.enabled = true;
    }

    /// <summary>
    /// 특정 pairObject에 대한 매치 성공 이벤트를 수동으로 호출합니다
    /// </summary>
    public void TriggerPairMatchedEvent(GameObject pairObject)
    {
        OnPairMatched?.Invoke(pairObject);
    }

    /// <summary>
    /// 매치 성공 이벤트에 리스너를 추가합니다
    /// </summary>
    public void AddPairMatchedListener(UnityAction<GameObject> listener)
    {
        OnPairMatched.AddListener(listener);
    }

    /// <summary>
    /// 매치 성공 이벤트에서 리스너를 제거합니다
    /// </summary>
    public void RemovePairMatchedListener(UnityAction<GameObject> listener)
    {
        OnPairMatched.RemoveListener(listener);
    }
    private void OnDestroy()
    {
        // 홀로그램 오브젝트들 정리
        foreach (var pair in objectPairs)
        {
            if (pair.hologramObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(pair.hologramObject);
                }
                else
                {
                    DestroyImmediate(pair.hologramObject);
                }
            }
        }
    }
}