using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 백과사전 아이템 클릭 처리 및 UI 관리 클래스
/// </summary>
public class EncyclopediaItem : MonoBehaviour, IPointerClickHandler
{
    [Header("Encyclopedia Settings")]
    [SerializeField] private string itemKey;
    [SerializeField] private Sprite[] itemSprites = new Sprite[0];
    [SerializeField] private GameObject featureObject;

    [Header("UI Components")]
    [SerializeField] private Image clickIcon;
    [SerializeField] private Sprite iconSelectedSprite;

    // 캐시된 컴포넌트 참조
    private RotateFeatureObject rotateFeatureObject;
    private EncyclopediaManager encyclopediaManager;
    private EncyclopediaData encyclopediaData;
    private EncyclopediaDataParser dataParser;
    private Tween clickIconTween;

    private void Awake()
    {
        CacheComponents();
        InitializeItem();
        SetupClickIconAnimation();
    }
    private void CacheComponents()
    {
        rotateFeatureObject = FindFirstObjectByType<RotateFeatureObject>(FindObjectsInactive.Include);
        encyclopediaManager = FindFirstObjectByType<EncyclopediaManager>(FindObjectsInactive.Include);
        dataParser = FindFirstObjectByType<EncyclopediaDataParser>(FindObjectsInactive.Include);
    }
    private void InitializeItem()
    {
        if (encyclopediaData != null)
        {
            Managers.System.RegisterCondition("encyclopedia_" + encyclopediaData.itemKey);
        }
        if (itemSprites != null && itemSprites.Length > 0)
        {
            dataParser.SetCachedImages(itemKey, itemSprites);
        }
    }

    private void SetupClickIconAnimation()
    {
        if (clickIcon == null) return;

        float yOffset = 0.1f;
        float animationDuration = 0.5f;

        clickIconTween = clickIcon.rectTransform
            .DOAnchorPosY(clickIcon.rectTransform.anchoredPosition.y + yOffset, animationDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void ShowEncyclopedia()
    {
        if (encyclopediaManager == null) return;
        if (encyclopediaData == null)
        {
            encyclopediaData = dataParser.FindByKey(itemKey);
        }
        encyclopediaManager.gameObject.SetActive(true);
        encyclopediaManager.InitEncyclopediaInfo(encyclopediaData);
        HandleFeatureObject();
        UpdateClickIcon();
    }

    private void HandleFeatureObject()
    {
        if (featureObject != null)
        {
            SetupFeatureObjectObservation();
        }
        else
        {
            DisableObservationPanel();
        }
    }

    private void SetupFeatureObjectObservation()
    {
        if (rotateFeatureObject == null) return;

        rotateFeatureObject.DeactiveTargetObject();
        featureObject.SetActive(true);
        encyclopediaManager.ActiveObservationPanel(true);
        rotateFeatureObject.SetTargetObject(featureObject.transform);
    }

    private void DisableObservationPanel()
    {
        encyclopediaManager.ActiveObservationPanel(false);
    }

    private void UpdateClickIcon()
    {
        if (clickIcon == null) return;

        clickIcon.sprite = iconSelectedSprite;
        StopClickIconAnimation();
    }
    private void StopClickIconAnimation()
    {
        if (clickIconTween != null)
        {
            clickIconTween.Kill();
            clickIconTween = null;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (dataParser.GetCachedImages(itemKey).Count > 0)
        {
            ShowEncyclopedia();
        }
    }

    private void OnDestroy()
    {
        StopClickIconAnimation();
    }
}
