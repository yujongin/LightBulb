using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
public class UIManager : MonoBehaviour
{
    [Header("PopUp Panels")]
    [SerializeField] private GameObject successPanel;
    [SerializeField] private Button successButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private GameObject failPanel;
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private PopUpText[] popUpTexts;

    [Header("StepBtns")]
    [SerializeField] private ScrollRect chapterScrollRect;
    [SerializeField] private GameObject[] chapterPanels;
    [SerializeField] private int chapterIndex = 0;

    [Header("LabelButton")]
    [SerializeField] private Button showLabelButton;
    [SerializeField] private Sprite showLabelSprite;
    [SerializeField] private Sprite hideLabelSprite;
    private bool isShowLabel = true;
    // 캐시된 UI 컴포넌트들
    private TMP_Text successText;
    private TMP_Text failText;
    private Button failButton;
    private TMP_Text guideText;
    private ButtonDragHandler[] stepBtnHandlers;

    // 프로퍼티
    public ButtonDragHandler[] StepBtnHandlers => stepBtnHandlers;
    public int ChapterIndex => chapterIndex;

    #region 초기화

    public void Init()
    {
        showLabelButton.onClick.AddListener(() => ShowLabelButton(!isShowLabel));
        InitializeStepBtnHandlers();
        InitializePopupComponents();
        SubscribeToEvents();
        SetActiveChapter(chapterIndex);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetActiveChapter(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetActiveChapter(1);
        }

    }
    private void InitializeStepBtnHandlers()
    {
        if (chapterPanels == null || chapterPanels.Length == 0) return;

        List<ButtonDragHandler> allHandlers = new List<ButtonDragHandler>();

        foreach (GameObject panel in chapterPanels)
        {
            if (panel != null)
            {
                ButtonDragHandler[] handlers = panel.GetComponentsInChildren<ButtonDragHandler>(true);
                allHandlers.AddRange(handlers);
            }
        }

        stepBtnHandlers = allHandlers.OrderBy(handler => handler.StepIndex).ToArray();
    }

    private void InitializePopupComponents()
    {
        InitializeSuccessPanel();
        InitializeFailPanel();
        InitializeGuidePanel();
    }

    private void InitializeSuccessPanel()
    {
        if (successPanel == null) return;

        successText = successPanel.GetComponentInChildren<TMP_Text>();
    }

    private void InitializeFailPanel()
    {
        if (failPanel == null) return;

        failText = failPanel.GetComponentInChildren<TMP_Text>();
        failButton = failPanel.GetComponentInChildren<Button>();
    }

    private void InitializeGuidePanel()
    {
        if (guidePanel == null) return;

        guideText = guidePanel.GetComponentInChildren<TMP_Text>();
    }

    private void SubscribeToEvents()
    {
        if (Managers.System == null) return;

        Managers.System.OnConditionMet -= ShowPopUpText;
        Managers.System.OnConditionMet += ShowPopUpText;
    }

    #endregion

    #region 챕터 관리

    public void SetActiveChapter(int index)
    {
        if (chapterPanels == null || index < 0 || index >= chapterPanels.Length)
        {

            return;
        }

        chapterIndex = index;

        for (int i = 0; i < chapterPanels.Length; i++)
        {
            if (chapterPanels[i] != null)
            {
                chapterPanels[i].SetActive(i == chapterIndex);
                if (i == chapterIndex)
                    chapterScrollRect.content = chapterPanels[i].GetComponent<RectTransform>();
            }
        }
    }

    public void NextChapter()
    {
        SetActiveChapter(chapterIndex + 1);
    }

    public void PreviousChapter()
    {
        SetActiveChapter(chapterIndex - 1);
    }

    public void SetChapterScrollActive(bool active)
    {
        chapterScrollRect.gameObject.SetActive(active);
    }

    #endregion

    #region 팝업 관리

    Tween m_tween;
    public void ShowPopUpText(string conditionName, Action action = null)
    {
        ClosePopUpText();
        if (m_tween != null)
            m_tween.Kill();
        m_tween = DOVirtual.DelayedCall(0.5f, () =>
        {
            PopUpText popUpText = FindPopUpText(conditionName);
            if (popUpText == null) return;

            switch (popUpText.type)
            {
                case PopUpTextType.Success:
                    ShowSuccessPopup(popUpText.text, action);
                    break;
                case PopUpTextType.Fail:
                    ShowFailPopup(popUpText.text, action);
                    break;
                case PopUpTextType.Guide:
                    ShowGuidePopup(popUpText.text);
                    break;
            }
        });
    }
    public void ShowPopUpTextNoDelay(string conditionName, Action action = null)
    {
        ClosePopUpText();
        if (m_tween != null)
            m_tween.Kill();
        PopUpText popUpText = FindPopUpText(conditionName);
        if (popUpText == null) return;

        switch (popUpText.type)
        {
            case PopUpTextType.Success:
                ShowSuccessPopup(popUpText.text, action);
                break;
            case PopUpTextType.Fail:
                ShowFailPopup(popUpText.text, action);
                break;
            case PopUpTextType.Guide:
                ShowGuidePopup(popUpText.text);
                break;
        }

    }

    private PopUpText FindPopUpText(string conditionName)
    {
        return popUpTexts?.FirstOrDefault(p => p.conditionName == conditionName);
    }

    private void ShowSuccessPopup(string text, Action action)
    {
        if (successPanel == null || successText == null) return;

        successPanel.SetActive(true);
        successText.text = text;
        SetupButtonListener(successButton, action);
    }

    private void ShowFailPopup(string text, Action action)
    {
        if (failPanel == null || failText == null) return;

        failPanel.SetActive(true);
        failText.text = text;
        SetupButtonListener(failButton, action);
    }

    private void ShowGuidePopup(string text)
    {
        if (guidePanel == null || guideText == null) return;

        guidePanel.SetActive(true);
        guideText.text = text;
    }

    public void ClosePopUpText()
    {
        if (m_tween != null)
            m_tween.Kill();
        SetPanelActive(successPanel, false);
        SetPanelActive(failPanel, false);
        SetPanelActive(guidePanel, false);
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    private void SetupButtonListener(Button button, Action action)
    {
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        if (action != null)
        {
            button.onClick.AddListener(() => action());
        }
    }

    public void SetRetryButton()
    {
        if (retryButton == null) return;
        successButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);
        SetupButtonListener(retryButton, () =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

    #endregion

    #region 스텝 버튼 관리

    public Button GetButtonByStepIndex(int targetStepIndex)
    {
        ButtonDragHandler handler = GetButtonDragHandlerByStepIndex(targetStepIndex);
        return handler?.GetComponent<Button>();
    }

    public ButtonDragHandler GetButtonDragHandlerByStepIndex(int targetStepIndex)
    {
        if (IsStepBtnHandlersEmpty())
        {

            return null;
        }

        ButtonDragHandler handler = FindHandlerByStepIndex(targetStepIndex);
        if (handler == null)
        {

        }

        return handler;
    }

    private bool IsStepBtnHandlersEmpty()
    {
        return stepBtnHandlers == null || stepBtnHandlers.Length == 0;
    }

    private ButtonDragHandler FindHandlerByStepIndex(int targetStepIndex)
    {
        return stepBtnHandlers?.FirstOrDefault(handler =>
            handler != null && handler.StepIndex == targetStepIndex);
    }

    public bool HasButtonWithStepIndex(int targetStepIndex)
    {
        return GetButtonDragHandlerByStepIndex(targetStepIndex) != null;
    }

    public bool SetButtonInteractable(int stepIndex, bool interactable)
    {
        Button button = GetButtonByStepIndex(stepIndex);
        if (button == null) return false;

        button.interactable = interactable;
        return true;
    }

    public bool EnableButton(int stepIndex)
    {
        return SetButtonInteractable(stepIndex, true);
    }

    public bool DisableButton(int stepIndex)
    {
        return SetButtonInteractable(stepIndex, false);
    }

    public void SetAllButtonsInteractable(bool interactable)
    {
        if (IsStepBtnHandlersEmpty()) return;

        foreach (ButtonDragHandler handler in stepBtnHandlers)
        {
            if (handler != null)
            {
                Button button = handler.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = interactable;
                }
            }
        }
    }

    public bool SetButtonActive(int stepIndex, bool active)
    {
        Button button = GetButtonByStepIndex(stepIndex);
        if (button == null) return false;

        button.gameObject.SetActive(active);
        return true;
    }

    public bool ShowButton(int stepIndex)
    {
        return SetButtonActive(stepIndex, true);
    }

    public bool HideButton(int stepIndex)
    {
        return SetButtonActive(stepIndex, false);
    }

    public void SetAllButtonsActive(bool active)
    {
        if (IsStepBtnHandlersEmpty()) return;

        foreach (ButtonDragHandler handler in stepBtnHandlers)
        {
            if (handler != null)
            {
                Button button = handler.GetComponent<Button>();
                if (button != null)
                {
                    button.gameObject.SetActive(active);
                }
            }
        }
    }

    public bool IsButtonActive(int stepIndex)
    {
        Button button = GetButtonByStepIndex(stepIndex);
        if (button == null) return false;

        return button.gameObject.activeInHierarchy;
    }

    #endregion

    #region 라벨 버튼 관리
    private void ShowLabelButton(bool isShow)
    {
        if (showLabelButton == null) return;

        isShowLabel = isShow;
        showLabelButton.image.sprite = isShow ? showLabelSprite : hideLabelSprite;
        List<GameObject> objects = Managers.Object.GetGameObjectsByTag("Label");
        foreach (GameObject obj in objects)
        {
            obj.SetActive(isShow);
        }
    }
    #endregion

    #region 이벤트 정리

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void UnsubscribeFromEvents()
    {
        if (Managers.System != null)
        {
            Managers.System.OnConditionMet -= ShowPopUpText;
        }
    }

    #endregion
}