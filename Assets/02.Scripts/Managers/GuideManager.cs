using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;

public class GuideManager : MonoBehaviour
{
    [Header("Guide Objects")]
    public GameObject ClickGuide;
    public GameObject DragGuide;

    [Header("Guide Data")]
    [SerializeField] private List<GuideData> guideDataList = new List<GuideData>();
    [SerializeField] private Sprite defaultDragGuideImage;
    private Tween dragGuideTween;
    private Sequence clickGuideSequence;

    public List<GuideData> GuideDataList => guideDataList;
    void Start()
    {
        ShowDragGuide(0);
    }
    #region 가이드 데이터 관리
    public bool CheckGuideData(int stepIndex)
    {
        return FindGuideData(stepIndex) != null;
    }

    public GuideData FindGuideData(int stepIndex)
    {
        return guideDataList.FirstOrDefault(data => data.stepIndex == stepIndex);
    }

    public void AddGuideData(GuideData guideData)
    {
        if (guideData == null || !guideData.IsValid())
        {
            
            return;
        }

        // 같은 stepIndex가 있으면 교체, 없으면 추가
        int existingIndex = guideDataList.FindIndex(data => data.stepIndex == guideData.stepIndex);
        if (existingIndex >= 0)
        {
            guideDataList[existingIndex] = guideData;
        }
        else
        {
            guideDataList.Add(guideData);
        }
    }

    public void RemoveGuide(int stepIndex)
    {
        GuideData dataToRemove = FindGuideData(stepIndex);
        if (dataToRemove != null)
        {
            guideDataList.Remove(dataToRemove);
        }
    }

    public void ClearAllGuides()
    {
        guideDataList.Clear();
        DeactivateAllGuides();
    }

    #endregion

    #region 드래그 가이드

    public void ShowDragGuide(int stepIndex, float delayTime = 0, float duration = 1.5f)
    {
        if (!IsValidGuideIndex(stepIndex, DragGuide)) return;

        GuideData currentGuide = FindGuideData(stepIndex);
        RectTransform dragGuideRect = DragGuide.GetComponent<RectTransform>();

        SetupGuidePosition(dragGuideRect, currentGuide.startPos);
        SetupDragGuideImage(currentGuide.guideImage);
        StartDragAnimation(dragGuideRect, currentGuide.endPos.anchoredPosition, delayTime, duration);
    }

    private void StartDragAnimation(RectTransform guideRect, Vector2 endPosition, float delayTime, float duration)
    {
        StopDragAnimation();

        dragGuideTween = guideRect.DOAnchorPos(endPosition, duration)
            .OnPlay(() => DragGuide.SetActive(true))
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Restart)
            .SetDelay(delayTime);
    }

    private void StopDragAnimation()
    {
        dragGuideTween?.Kill();
    }

    #endregion

    #region 클릭 가이드

    public void ShowClickGuide(int stepIndex, float delayTime = 0)
    {
        if (!IsValidGuideIndex(stepIndex, ClickGuide)) return;

        GuideData currentGuide = FindGuideData(stepIndex);
        RectTransform clickGuideRect = ClickGuide.GetComponent<RectTransform>();
        SetupGuidePosition(clickGuideRect, currentGuide.startPos);
        StartClickAnimation(delayTime);
    }

    private void StartClickAnimation(float delayTime)
    {
        StopClickAnimation();

        if (delayTime > 0)
        {
            clickGuideSequence = DOTween.Sequence()
                .AppendInterval(delayTime)
                .AppendCallback(() => ClickGuide.SetActive(true));
        }
        else
        {
            ClickGuide.SetActive(true);
        }
    }

    private void StopClickAnimation()
    {
        clickGuideSequence?.Kill();
    }

    #endregion

    #region 공통 기능

    private bool IsValidGuideIndex(int stepIndex, GameObject guideObject)
    {
        GuideData guideData = FindGuideData(stepIndex);
        if (guideData == null)
        {
            
            return false;
        }

        if (guideObject == null)
        {
            
            return false;
        }

        if (!guideData.IsValid())
        {
            
            return false;
        }

        return true;
    }

    private void SetupGuidePosition(RectTransform guideRect, RectTransform targetPos)
    {
        if (guideRect == null || targetPos == null) return;

        guideRect.anchorMin = targetPos.anchorMin;
        guideRect.anchorMax = targetPos.anchorMax;
        guideRect.anchoredPosition = targetPos.anchoredPosition;
    }

    private void SetupDragGuideImage(Sprite guideImage)
    {
        if (DragGuide == null) return;

        Image imageComponent = DragGuide.GetComponent<Image>();

        if (imageComponent != null)
        {
            // guideImage가 없으면 defaultDragGuideImage 사용
            Sprite spriteToUse = guideImage ?? defaultDragGuideImage;
            
            if (spriteToUse != null)
            {
                imageComponent.sprite = spriteToUse;
            }
        }
    }

    public void DeactivateAllGuides()
    {
        DeactivateDragGuide();
        DeactivateClickGuide();
    }

    public void DeactivateDragGuide()
    {
        if (DragGuide != null)
            DragGuide.SetActive(false);
        StopDragAnimation();
    }

    public void DeactivateClickGuide()
    {
        if (ClickGuide != null)
            ClickGuide.SetActive(false);
        StopClickAnimation();
    }

    #endregion

    #region 생명주기

    private void OnDestroy()
    {
        StopAllAnimations();
    }

    private void StopAllAnimations()
    {
        StopDragAnimation();
        StopClickAnimation();
    }

    #endregion
}

[System.Serializable]
public class GuideData
{
    public int stepIndex;
    public RectTransform startPos;
    public RectTransform endPos;
    public Sprite guideImage;
    public bool IsValid()
    {
        return startPos != null && endPos != null;
    }
}
