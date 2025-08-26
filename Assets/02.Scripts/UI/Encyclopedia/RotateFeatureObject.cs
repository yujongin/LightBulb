using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RotateFeatureObject : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    [SerializeField] private Transform targetObject; // 회전시킬 대상 오브젝트
    [SerializeField] private float rotationSpeed = 1f; // 회전 속도 조절
    [SerializeField] private GameObject rotateImage;
    [Header("Toggle Animation Action")]
    [SerializeField] private Toggle[] animationActionToggles;
    private Animator targetAnimator;
    private Vector2 lastMousePosition;

    void Start()
    {
        // 애니메이션 토글 이벤트 리스너 등록
        for (int i = 0; i < animationActionToggles.Length; i++)
        {
            int index = i;
            animationActionToggles[i].onValueChanged.AddListener((isOn) => OnAnimationToggleChanged(index, isOn));
        }
    }
    private void OnAnimationToggleChanged(int toggleIndex, bool isOn)
    {
        if (!isOn || targetAnimator == null) return;

        // 총 활성화된 토글 수 확인
        int activeToggleCount = 0;
        for (int i = 0; i < animationActionToggles.Length; i++)
        {
            if (animationActionToggles[i].gameObject.activeSelf)
            {
                activeToggleCount++;
            }
        }
        
        // 토글 인덱스에 따라 Blend 값을 동적으로 계산
        float blendValue = 0f;
        if (activeToggleCount > 1)
        {
            blendValue = (float)toggleIndex / (activeToggleCount - 1);
        }

        // Animator의 Blend 파라미터 설정
        targetAnimator.SetFloat("Blend", blendValue);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 시작 시 현재 마우스 위치를 저장
        rotateImage.SetActive(false);
        lastMousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetObject == null) return;

        // 마우스 이동 방향 계산
        Vector2 currentMousePosition = eventData.position;
        Vector2 mouseDelta = currentMousePosition - lastMousePosition;

        // Y축 회전 (좌우 드래그)
        targetObject.Rotate(Vector3.up, -mouseDelta.x * rotationSpeed, Space.World);

        // X축 회전 (상하 드래그)
        targetObject.Rotate(Vector3.right, mouseDelta.y * rotationSpeed, Space.World);

        lastMousePosition = currentMousePosition;
    }

    public void SetTargetObject(Transform target)
    {
        targetObject = target;
        targetAnimator = target.GetComponentInChildren<Animator>();
        
        // 애니메이터가 있고 AnimatorController가 설정되어 있는 경우
        if (targetAnimator != null && targetAnimator.runtimeAnimatorController != null)
        {
            // 애니메이션 클립 수 확인
            int clipCount = targetAnimator.runtimeAnimatorController.animationClips.Length;
            
            // 토글 활성화/비활성화 설정
            for (int i = 0; i < animationActionToggles.Length; i++)
            {
                if (i < clipCount)
                {
                    animationActionToggles[i].gameObject.SetActive(true);
                }
                else
                {
                    animationActionToggles[i].gameObject.SetActive(false);
                }
            }
            
            // 첫 번째 토글을 기본으로 선택
            if (clipCount > 0 && animationActionToggles.Length > 0)
            {
                animationActionToggles[0].isOn = true;
            }
        }
        else
        {
            // 애니메이터가 없거나 컨트롤러가 없는 경우 모든 토글 비활성화
            for (int i = 0; i < animationActionToggles.Length; i++)
            {
                animationActionToggles[i].gameObject.SetActive(false);
            }
        }
    }


    public void DeactiveTargetObject()
    {
        if (targetObject == null || targetAnimator == null) return;
        targetAnimator.SetFloat("Blend", 0f);
        animationActionToggles[0].isOn = true;
        targetObject.rotation = Quaternion.Euler(0, 0, 0);
        targetObject.gameObject.SetActive(false);
        rotateImage.SetActive(true);
    }
}
