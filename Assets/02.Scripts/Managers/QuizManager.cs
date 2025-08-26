using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class QuizData
{
    public string title;
    public string question;
    public QuizType quizType;
    public string[] textChoices;
    public Sprite[] imageChoices;
    public string correctAnswer; // 인풋 퀴즈용
    public int correctAnswerIndex;
    public bool isCorrectAnswer; // OX 퀴즈용 (true = O, false = X)
}

public class QuizManager : MonoBehaviour
{
    [Header("Main Quiz Panel")]
    [SerializeField] private GameObject m_quizPanel;
    [SerializeField] private GameObject m_typePanel;
    [SerializeField] private TMP_Text m_titleText;

    [Header("Choice Panels")]
    [SerializeField] private GameObject m_textChoicePanel;
    [SerializeField] private GameObject m_imageChoicePanel;
    [SerializeField] private GameObject m_oxChoicePanel;
    [SerializeField] private GameObject m_inputPanel;

    [Header("Choice Components")]
    [SerializeField] private Transform m_textChoiceContainer;
    [SerializeField] private GameObject m_textChoicePrefab;
    [SerializeField] private Transform m_imageChoiceContainer;
    [SerializeField] private GameObject m_imageChoicePrefab;
    [SerializeField] private Transform m_oxChoiceContainer;
    [SerializeField] private GameObject m_oxChoicePrefab;

    [Header("Input Components")]
    [SerializeField] private TMP_InputField m_inputField;
    [SerializeField] private KeyBoardUIManager m_keyboardManager;

    [Header("Buttons")]
    [SerializeField] private Button m_submitButton;
    [SerializeField] private Button m_retryButton;
    [SerializeField] private Button m_closeButton;

    [Header("Result Panels")]
    [SerializeField] private GameObject m_correctPanel;
    [SerializeField] private GameObject m_wrongPanel;

    [Header("Choice Sprites")]
    [SerializeField] private Sprite m_defaultChoiceSprite;
    [SerializeField] private Sprite m_selectedChoiceSprite;
    [SerializeField] private Sprite m_correctChoiceSprite;
    [SerializeField] private Sprite m_wrongChoiceSprite;

    [Header("Radio Button Sprites")]
    [SerializeField] private Sprite m_defaultRadioSprite;
    [SerializeField] private Sprite m_selectedRadioSprite;
    [SerializeField] private Sprite m_correctRadioSprite;
    [SerializeField] private Sprite m_wrongRadioSprite;

    [Header("Input Field Sprites")]
    [SerializeField] private Sprite m_defaultInputSprite;
    [SerializeField] private Sprite m_selectedInputSprite;
    [SerializeField] private Sprite m_correctInputSprite;
    [SerializeField] private Sprite m_wrongInputSprite;

    [Header("Font Assets")]
    [SerializeField] private TMPro.TMP_FontAsset m_boldFontAsset;

    // Private fields
    private QuizData m_currentQuizData;
    private List<Toggle> m_currentChoices = new List<Toggle>();
    private int m_selectedChoiceIndex = -1;
    private bool m_isSubmitted = false;
    private bool m_wasCorrectAnswer = false; // 정답 여부를 추적하는 변수 추가
    private Action m_onQuizComplete;
    private Action<bool> m_onQuizResult;
    private Vector2Int m_currentCellPosition; // 현재 퀴즈가 진행되는 셀 위치

    // Constants
    private const float C_ANSWER_TOLERANCE = 0.001f;
    private const float C_DEFAULT_PANEL_HEIGHT = 149f;
    private const string C_BOLD_COLOR = "#AB3E0E";

    // Layout configurations
    private static readonly Dictionary<int, LayoutConfig> s_textLayoutConfigs = new Dictionary<int, LayoutConfig>
    {
        { 2, new LayoutConfig(1, new Vector2(426, 83), new Vector2(0, 16), 256f) },
        { 3, new LayoutConfig(1, new Vector2(426, 83), new Vector2(0, 16), 349f) },
        { 4, new LayoutConfig(2, new Vector2(208, 88), new Vector2(14, 16), 266f) }
    };

    private static readonly Dictionary<int, LayoutConfig> s_imageLayoutConfigs = new Dictionary<int, LayoutConfig>
    {
        { 2, new LayoutConfig(1, new Vector2(190, 132), new Vector2(14, 16), C_DEFAULT_PANEL_HEIGHT, true) },
        { 3, new LayoutConfig(1, new Vector2(190, 132), new Vector2(14, 16), 460f) },
        { 4, new LayoutConfig(2, new Vector2(190, 132), new Vector2(14, 16), 360f) }
    };

    private static readonly Dictionary<int, LayoutConfig> s_oxLayoutConfigs = new Dictionary<int, LayoutConfig>
    {
        { 2, new LayoutConfig(1, new Vector2(190, 100), new Vector2(14, 16), C_DEFAULT_PANEL_HEIGHT, true) }
    };

    private struct LayoutConfig
    {
        public int constraintCount;
        public Vector2 cellSize;
        public Vector2 spacing;
        public float panelHeight;
        public bool isRowConstraint;

        public LayoutConfig(int count, Vector2 size, Vector2 space, float height, bool isRow = false)
        {
            constraintCount = count;
            cellSize = size;
            spacing = space;
            panelHeight = height;
            isRowConstraint = isRow;
        }
    }

    #region Unity Lifecycle
    void Start()
    {
        InitializeEventListeners();
        TableManager.OnTableInitialized += OnTableInitialized;
        TableManager.OnInnerCellClicked += OnInnerCellClicked;
    }

    void OnDestroy()
    {
        CleanupEventListeners();
        TableManager.OnTableInitialized -= OnTableInitialized;
        TableManager.OnInnerCellClicked -= OnInnerCellClicked;
    }

    private void InitializeEventListeners()
    {
        m_submitButton.onClick.AddListener(SubmitQuiz);
        m_retryButton.onClick.AddListener(RetryQuiz);
        m_closeButton.onClick.AddListener(CloseQuizPanel);

        if (m_inputField != null)
        {
            m_inputField.onValueChanged.AddListener(OnInputFieldChanged);
        }
    }

    private void CleanupEventListeners()
    {
        m_submitButton.onClick.RemoveAllListeners();
        m_retryButton.onClick.RemoveAllListeners();
        m_closeButton.onClick.RemoveAllListeners();
        
        m_inputField.onValueChanged.RemoveAllListeners();
        m_inputField.onSelect.RemoveAllListeners();
        m_inputField.onDeselect.RemoveAllListeners();
        
        ClearChoiceEventListeners();
        
        if (m_keyboardManager != null)
        {
            m_keyboardManager.HideKeyboard();
        }
    }

    private void ClearChoiceEventListeners()
    {
        foreach (var choice in m_currentChoices)
        {
            choice.onValueChanged.RemoveAllListeners();
        }
    }

    private void OnTableInitialized()
    {
        
    }

    private void OnInnerCellClicked(Vector2Int cellPosition)
    {
        m_currentCellPosition = cellPosition;
        
    }
    #endregion

    #region Public Methods
    public void StartQuiz(QuizData quizData, Action onComplete = null, Action<bool> onResult = null)
    {
        if (!ValidateQuizData(quizData)) return;

        m_currentQuizData = quizData;
        m_onQuizComplete = onComplete;
        m_onQuizResult = onResult;

        InitializeQuizPanel();
        SetupQuizUI();
        
        m_quizPanel.SetActive(true);
    }

    public void CloseQuizPanel()
    {
        TableManager tableManager = FindFirstObjectByType<TableManager>();
        
        // 정답이었을 경우에만 TableManager.CompleteQuiz 호출
        if (m_wasCorrectAnswer && tableManager != null)
        {
            // 정답에 따른 결과 텍스트 설정
            string resultText = GetResultText();
            Color resultColor = GetResultColor();
            
            tableManager.CompleteQuiz(m_currentCellPosition, null, resultText, resultColor);
        }
        
        // TableManager를 통해 퀴즈 패널 닫기 (애니메이션 포함)
        if (tableManager != null)
        {
            tableManager.CloseQuizPanel();
        }
        else
        {
            // TableManager가 없으면 직접 패널 비활성화
            m_quizPanel.SetActive(false);
        }
        
        // 정답이었을 경우에만 퀴즈 완료 콜백 호출
        if (m_wasCorrectAnswer)
        {
            m_onQuizComplete?.Invoke();
        }
    }

    public void RetryQuiz()
    {
        m_isSubmitted = false;
        InitializeQuizPanel();
        SetupQuizUI();
    }

    public void SubmitQuiz()
    {
        if (m_isSubmitted) return;

        m_isSubmitted = true;
        bool isCorrect = CheckAnswer();
        
        ShowResult(isCorrect);
        m_onQuizResult?.Invoke(isCorrect);
    }
    #endregion

    #region Private Methods - Validation
    private bool ValidateQuizData(QuizData quizData)
    {
        if (quizData == null)
        {
            
            return false;
        }

        if (m_quizPanel == null)
        {
            
            return false;
        }

        return true;
    }

    private bool ValidateRequiredComponents(QuizType quizType)
    {
        switch (quizType)
        {
            case QuizType.MultipleChoiceText:
                return m_textChoicePanel != null && m_textChoiceContainer != null && m_textChoicePrefab != null;
            case QuizType.MultipleChoiceImage:
                return m_imageChoicePanel != null && m_imageChoiceContainer != null && m_imageChoicePrefab != null;
            case QuizType.OXQuiz:
                return m_oxChoicePanel != null && m_oxChoiceContainer != null && m_oxChoicePrefab != null;
            case QuizType.InputField:
                return m_inputPanel != null && m_inputField != null;
            default:
                return false;
        }
    }
    #endregion

    #region Private Methods - Setup
    private void InitializeQuizPanel()
    {
        DeactivateAllPanels();
        ResetButtonStates();
        ResetQuizState();
        ClearCurrentChoices();
        ResetPanelHeight();
    }

    private void DeactivateAllPanels()
    {
        m_textChoicePanel.SetActive(false);
        m_imageChoicePanel.SetActive(false);
        m_oxChoicePanel.SetActive(false);
        m_inputPanel.SetActive(false);
        m_correctPanel.SetActive(false);
        m_wrongPanel.SetActive(false);
    }

    private void ResetButtonStates()
    {
        m_submitButton.gameObject.SetActive(true);
        m_submitButton.interactable = false;
        m_retryButton.gameObject.SetActive(false);
        m_closeButton.gameObject.SetActive(false);
        
        // 모든 선택지를 다시 활성화
        EnableAllChoiceInteractions();
    }

    private void ResetQuizState()
    {
        m_selectedChoiceIndex = -1;
        m_isSubmitted = false;
        m_wasCorrectAnswer = false; // 정답 상태도 초기화
    }

    private void SetupQuizUI()
    {
        SetupTitleText();
        SetupQuizTypeUI();
    }

    private void SetupTitleText()
    {
        if (string.IsNullOrEmpty(m_currentQuizData.title)) return;

        bool hasBoldTag = m_currentQuizData.title.Contains("<b>") && m_currentQuizData.title.Contains("</b>");
        
        if (hasBoldTag)
        {
            ApplyBoldFormatting();
        }
        else
        {
            m_titleText.text = m_currentQuizData.title;
        }
    }

    private void ApplyBoldFormatting()
    {
        if (m_boldFontAsset != null)
        {
            m_titleText.font = m_boldFontAsset;
        }
        
        if (ColorUtility.TryParseHtmlString(C_BOLD_COLOR, out Color boldColor))
        {
            m_titleText.color = boldColor;
        }
        
        string cleanTitle = m_currentQuizData.title.Replace("<b>", "").Replace("</b>", "");
        m_titleText.text = cleanTitle;
    }

    private void SetupQuizTypeUI()
    {
        if (!ValidateRequiredComponents(m_currentQuizData.quizType))
        {
            
            return;
        }

        switch (m_currentQuizData.quizType)
        {
            case QuizType.MultipleChoiceText:
                SetupTextChoices();
                break;
            case QuizType.MultipleChoiceImage:
                SetupImageChoices();
                break;
            case QuizType.OXQuiz:
                SetupOXChoices();
                break;
            case QuizType.InputField:
                SetupInputField();
                break;
        }
    }

    private void SetupTextChoices()
    {
        m_textChoicePanel.SetActive(true);
        
        ToggleGroup toggleGroup = SetupToggleGroup(m_textChoiceContainer);
        
        // 먼저 모든 선택지 생성
        for (int i = 0; i < m_currentQuizData.textChoices.Length; i++)
        {
            GameObject choiceObj = Instantiate(m_textChoicePrefab, m_textChoiceContainer);
            SetupTextChoice(choiceObj, i, m_currentQuizData.textChoices[i], toggleGroup);
        }
        
        // 레이아웃 설정은 모든 오브젝트 생성 후 수행
        SetupChoiceLayout(m_textChoiceContainer.gameObject, m_currentQuizData.textChoices.Length, s_textLayoutConfigs);
        
        // 다음 프레임에서 레이아웃 업데이트 수행
        StartCoroutine(UpdateLayoutNextFrame());
    }

    private void SetupImageChoices()
    {
        m_imageChoicePanel.SetActive(true);
        
        ToggleGroup toggleGroup = SetupToggleGroup(m_imageChoiceContainer);
        
        // 먼저 모든 선택지 생성
        for (int i = 0; i < m_currentQuizData.imageChoices.Length; i++)
        {
            GameObject choiceObj = Instantiate(m_imageChoicePrefab, m_imageChoiceContainer);
            SetupImageChoice(choiceObj, i, m_currentQuizData.imageChoices[i], toggleGroup);
        }
        
        // 레이아웃 설정은 모든 오브젝트 생성 후 수행
        SetupChoiceLayout(m_imageChoiceContainer.gameObject, m_currentQuizData.imageChoices.Length, s_imageLayoutConfigs);
        
        // 다음 프레임에서 레이아웃 업데이트 수행
        StartCoroutine(UpdateLayoutNextFrame());
    }

    private void SetupOXChoices()
    {
        m_oxChoicePanel.SetActive(true);
        
        string[] oxChoices = { "O", "X" };
        Color32[] oxColors = { 
            new Color32(0x1C, 0x63, 0xB8, 0xFF), // O: #1C63B8
            new Color32(0xC8, 0x24, 0x3E, 0xFF)  // X: #C8243E
        };
        
        ToggleGroup toggleGroup = SetupToggleGroup(m_oxChoiceContainer);
        
        // 먼저 모든 선택지 생성
        for (int i = 0; i < oxChoices.Length; i++)
        {
            GameObject choiceObj = Instantiate(m_oxChoicePrefab, m_oxChoiceContainer);
            SetupOXChoice(choiceObj, i, oxChoices[i], oxColors[i], toggleGroup);
        }
        
        // OX 퀴즈 레이아웃 설정
        SetupChoiceLayout(m_oxChoiceContainer.gameObject, oxChoices.Length, s_oxLayoutConfigs);
        
        // 다음 프레임에서 레이아웃 업데이트 수행
        StartCoroutine(UpdateLayoutNextFrame());
    }

    private void SetupInputField()
    {
        m_inputPanel.SetActive(true);
        m_inputField.text = "";
        
        if (m_keyboardManager == null)
        {
            m_keyboardManager = FindFirstObjectByType<KeyBoardUIManager>();
        }
        
        m_inputField.onSelect.AddListener(OnInputFieldSelected);
        m_inputField.onDeselect.AddListener(OnInputFieldDeselected);
        
        UpdateInputFieldAppearance(false);
        
        m_inputField.Select();
        m_inputField.ActivateInputField();
    }

    private ToggleGroup SetupToggleGroup(Transform container)
    {
        ToggleGroup toggleGroup = container.GetComponent<ToggleGroup>();
        if (toggleGroup == null)
        {
            toggleGroup = container.gameObject.AddComponent<ToggleGroup>();
        }
        toggleGroup.allowSwitchOff = false; // 같은 버튼을 다시 눌러도 선택 해제되지 않음
        return toggleGroup;
    }

    private void SetupTextChoice(GameObject choiceObj, int index, string text, ToggleGroup toggleGroup)
    {
        Toggle toggle = choiceObj.GetComponent<Toggle>();
        TMP_Text textComponent = choiceObj.GetComponentInChildren<TMP_Text>();
        
        textComponent.text = text;
        SetupToggleChoice(toggle, index, toggleGroup);
    }

    private void SetupImageChoice(GameObject choiceObj, int index, Sprite sprite, ToggleGroup toggleGroup)
    {
        Toggle toggle = choiceObj.GetComponent<Toggle>();
        Image contentImage = GetContentImage(choiceObj);
        
        contentImage.sprite = sprite;
        SetupToggleChoice(toggle, index, toggleGroup);
    }

    private void SetupOXChoice(GameObject choiceObj, int index, string text, Color32 color, ToggleGroup toggleGroup)
    {
        Toggle toggle = choiceObj.GetComponent<Toggle>();
        TMP_Text textComponent = choiceObj.GetComponentInChildren<TMP_Text>();
        
        textComponent.text = text;
        textComponent.color = color;
        
        SetupToggleChoice(toggle, index, toggleGroup);
    }

    private void SetupToggleChoice(Toggle toggle, int index, ToggleGroup toggleGroup)
    {
        toggle.group = toggleGroup;
        toggle.onValueChanged.AddListener((isOn) => OnChoiceSelected(isOn ? index : -1));
        
        // Toggle의 targetGraphic을 명시적으로 설정하여 클릭 영역 정확하게 설정
        Image backgroundImage = toggle.GetComponent<Image>();
        toggle.targetGraphic = backgroundImage;
        
        // Toggle 설정 최적화
        toggle.toggleTransition = Toggle.ToggleTransition.None; // Toggle transition 비활성화
        toggle.isOn = false; // 초기 상태는 선택 해제
        toggle.interactable = true;
        
        // Navigation 설정으로 키보드/게임패드 탐색 개선
        Navigation nav = toggle.navigation;
        nav.mode = Navigation.Mode.Automatic;
        toggle.navigation = nav;
        
        m_currentChoices.Add(toggle);
    }

    private void SetupChoiceLayout(GameObject container, int choiceCount, Dictionary<int, LayoutConfig> layoutConfigs)
    {
        GridLayoutGroup gridLayout = GetOrCreateGridLayoutGroup(container);
        
        LayoutConfig config = layoutConfigs.ContainsKey(choiceCount) 
            ? layoutConfigs[choiceCount] 
            : new LayoutConfig(1, new Vector2(426, 83), new Vector2(0, 16), C_DEFAULT_PANEL_HEIGHT);
        
        ApplyLayoutConfig(gridLayout, config);
        SetPanelHeight(config.panelHeight);
        
        // 레이아웃 즉시 업데이트
        LayoutRebuilder.ForceRebuildLayoutImmediate(container.GetComponent<RectTransform>());
    }

    private GridLayoutGroup GetOrCreateGridLayoutGroup(GameObject container)
    {
        // 기존 레이아웃 그룹들 제거 (GridLayoutGroup 제외)
        RemoveConflictingLayoutGroups(container);
        
        // GridLayoutGroup 가져오거나 생성
        GridLayoutGroup gridLayout = container.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = container.AddComponent<GridLayoutGroup>();
        }
        
        return gridLayout;
    }

    private void ApplyLayoutConfig(GridLayoutGroup gridLayout, LayoutConfig config)
    {
        gridLayout.constraint = config.isRowConstraint 
            ? GridLayoutGroup.Constraint.FixedRowCount 
            : GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = config.constraintCount;
        gridLayout.cellSize = config.cellSize;
        gridLayout.spacing = config.spacing;
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        gridLayout.padding = new RectOffset(0, 0, 0, 0);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        
        // GridLayoutGroup이 자식 오브젝트 크기를 제어하지 않도록 설정
        gridLayout.enabled = true;
    }

    private void RemoveConflictingLayoutGroups(GameObject container)
    {
        // ContentSizeFitter 제거 (GridLayoutGroup과 충돌 가능)
        ContentSizeFitter contentSizeFitter = container.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter != null)
        {
            DestroyImmediate(contentSizeFitter);
        }
        
        // 다른 레이아웃 그룹들 제거
        VerticalLayoutGroup verticalLayoutGroup = container.GetComponent<VerticalLayoutGroup>();
        if (verticalLayoutGroup != null)
        {
            DestroyImmediate(verticalLayoutGroup);
        }
        
        HorizontalLayoutGroup horizontalLayoutGroup = container.GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayoutGroup != null)
        {
            DestroyImmediate(horizontalLayoutGroup);
        }
    }

    private void SetPanelHeight(float height)
    {
        RectTransform typePanelRect = m_typePanel.GetComponent<RectTransform>();
        Vector2 sizeDelta = typePanelRect.sizeDelta;
        sizeDelta.y = height;
        typePanelRect.sizeDelta = sizeDelta;
    }

    private void ResetPanelHeight()
    {
        SetPanelHeight(C_DEFAULT_PANEL_HEIGHT);
    }

    private void ClearCurrentChoices()
    {
        ClearChoiceEventListeners();
        
        // 안전하게 선택지 오브젝트들 제거
        for (int i = m_currentChoices.Count - 1; i >= 0; i--)
        {
            if (m_currentChoices[i] != null && m_currentChoices[i].gameObject != null)
            {
                GameObject choiceObj = m_currentChoices[i].gameObject;
                
                // Toggle 이벤트 리스너 제거
                m_currentChoices[i].onValueChanged.RemoveAllListeners();
                
                // 오브젝트 제거
                DestroyImmediate(choiceObj);
            }
        }
        m_currentChoices.Clear();

        ClearContainerChildren();
        
        m_inputField.text = "";
        m_inputField.onSelect.RemoveAllListeners();
        m_inputField.onDeselect.RemoveAllListeners();
        
        if (m_keyboardManager != null)
        {
            m_keyboardManager.HideKeyboard();
        }
    }

    private void ClearContainerChildren()
    {
        Transform[] containers = { m_textChoiceContainer, m_imageChoiceContainer, m_oxChoiceContainer };
        
        foreach (Transform container in containers)
        {
            // 남아있는 자식 오브젝트들 정리
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Transform child = container.GetChild(i);
                
                // LayoutElement가 있다면 제거
                LayoutElement layoutElement = child.GetComponent<LayoutElement>();
                if (layoutElement != null)
                {
                    DestroyImmediate(layoutElement);
                }
                
                DestroyImmediate(child.gameObject);
            }
            
            // ToggleGroup 제거
            ToggleGroup toggleGroup = container.GetComponent<ToggleGroup>();
            if (toggleGroup != null)
            {
                DestroyImmediate(toggleGroup);
            }
        }
    }

    private Image GetContentImage(GameObject choiceObj)
    {
        // Option_Image 이름의 자식 오브젝트에서만 콘텐츠 이미지 찾기
        Transform imageChild = choiceObj.transform.Find("Image");
        if (imageChild != null)
        {
            Image contentImage = imageChild.GetComponent<Image>();
            if (contentImage != null)
            {
                return contentImage;
            }
        }
        
        return null;
    }

    private Image GetRadioButtonImage(GameObject choiceObj)
    {
        // Radio_Button 이름의 자식 오브젝트에서만 라디오 버튼 이미지 찾기
        Transform radioChild = choiceObj.transform.Find("Radio_Button");
        if (radioChild != null)
        {
            Image radioImage = radioChild.GetComponent<Image>();
            if (radioImage != null)
            {
                return radioImage;
            }
        }
        
        return null;
    }


    #endregion

    #region Private Methods - Layout Updates
    private IEnumerator UpdateLayoutNextFrame()
    {
        yield return null; // 다음 프레임까지 대기
        
        // 모든 활성화된 컨테이너의 레이아웃 강제 업데이트
        if (m_textChoicePanel != null && m_textChoicePanel.activeInHierarchy && m_textChoiceContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_textChoiceContainer.GetComponent<RectTransform>());
            FixToggleSizes(m_textChoiceContainer, m_currentQuizData.textChoices.Length, s_textLayoutConfigs);
        }
        
        if (m_imageChoicePanel != null && m_imageChoicePanel.activeInHierarchy && m_imageChoiceContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_imageChoiceContainer.GetComponent<RectTransform>());
            FixToggleSizes(m_imageChoiceContainer, m_currentQuizData.imageChoices.Length, s_imageLayoutConfigs);
        }
        
        if (m_oxChoicePanel != null && m_oxChoicePanel.activeInHierarchy && m_oxChoiceContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_oxChoiceContainer.GetComponent<RectTransform>());
            FixToggleSizes(m_oxChoiceContainer, 2, s_oxLayoutConfigs); // OX 레이아웃 설정 사용
        }
        
        // 전체 Canvas 업데이트
        Canvas.ForceUpdateCanvases();
    }
    
        private void FixToggleSizes(Transform container, int choiceCount, Dictionary<int, LayoutConfig> layoutConfigs)
    {
        Vector2 targetSize;
        
        if (layoutConfigs != null && layoutConfigs.ContainsKey(choiceCount))
        {
            targetSize = layoutConfigs[choiceCount].cellSize;
        }
        else
        {
            // 기본 크기 (fallback)
            targetSize = new Vector2(208, 88);
        }
        
        // 컨테이너의 모든 자식 Toggle 크기 조정
        for (int i = 0; i < container.childCount; i++)
        {
            Transform child = container.GetChild(i);
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            Toggle toggle = child.GetComponent<Toggle>();
            
            // RectTransform 크기를 정확히 설정
            rectTransform.sizeDelta = targetSize;
            
            // Toggle의 클릭 영역을 제한하기 위해 추가 설정
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            // LayoutElement를 추가하여 크기 강제 고정
            LayoutElement layoutElement = child.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = child.gameObject.AddComponent<LayoutElement>();
            }
            layoutElement.preferredWidth = targetSize.x;
            layoutElement.preferredHeight = targetSize.y;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;
            
            // targetGraphic을 다시 설정하여 클릭 영역 보장
            Image backgroundImage = toggle.GetComponent<Image>();
            toggle.targetGraphic = backgroundImage;
            
            // 배경 이미지의 RectTransform도 같은 크기로 설정
            RectTransform bgRect = backgroundImage.GetComponent<RectTransform>();
            bgRect.sizeDelta = targetSize;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            // 이미지가 레이캐스트 타겟이 되도록 설정
            backgroundImage.raycastTarget = true;
            
            // 모든 자식 UI 요소들의 레이캐스트를 비활성화하여 Toggle만 클릭되도록 함
            DisableChildRaycastTargets(child, backgroundImage);
        }
    }
     
     private void DisableChildRaycastTargets(Transform parent, Image excludeImage)
     {
         // 모든 자식 UI 요소들의 raycastTarget을 비활성화 (excludeImage 제외)
         Image[] childImages = parent.GetComponentsInChildren<Image>();
         foreach (Image img in childImages)
         {
             if (img != excludeImage)
             {
                 img.raycastTarget = false;
             }
         }
         
         // 텍스트 컴포넌트들도 raycastTarget 비활성화
         TMP_Text[] childTexts = parent.GetComponentsInChildren<TMP_Text>();
         foreach (TMP_Text txt in childTexts)
         {
             txt.raycastTarget = false;
         }
         
         // 일반 Text 컴포넌트들도 처리
         Text[] childLegacyTexts = parent.GetComponentsInChildren<Text>();
         foreach (Text txt in childLegacyTexts)
         {
             txt.raycastTarget = false;
         }
     }
     #endregion

    #region Private Methods - Event Handlers
    private void OnChoiceSelected(int index)
    {
        if (m_isSubmitted) return;

        if (index == -1)
        {
            // 선택 해제시 현재 활성화된 Toggle 확인
            bool hasActiveToggle = false;
            for (int i = 0; i < m_currentChoices.Count; i++)
            {
                if (m_currentChoices[i].isOn)
                {
                    m_selectedChoiceIndex = i;
                    hasActiveToggle = true;
                    break;
                }
            }
            
            if (!hasActiveToggle)
            {
                m_selectedChoiceIndex = -1;
            }
        }
        else
        {
            m_selectedChoiceIndex = index;
        }
        
        // 한번 선택되면 제출 버튼이 계속 활성화 상태를 유지
        if (m_selectedChoiceIndex >= 0)
        {
            m_submitButton.interactable = true;
        }
        
        UpdateChoiceAppearance(m_selectedChoiceIndex);
    }

    private void OnInputFieldChanged(string text)
    {
        if (m_isSubmitted) return;

        m_submitButton.interactable = !string.IsNullOrEmpty(text.Trim());
        UpdateInputFieldAppearance(true);
    }

    private void OnInputFieldSelected(string text)
    {
        if (m_isSubmitted) return;

        if (m_keyboardManager != null && m_inputField != null)
        {
            m_keyboardManager.ShowKeyboard(m_inputField);
        }
        
        UpdateInputFieldAppearance(true);
    }

    private void OnInputFieldDeselected(string text)
    {
        if (m_isSubmitted) return;

        bool hasText = !string.IsNullOrEmpty(text.Trim());
        UpdateInputFieldAppearance(hasText);
    }

    private void UpdateInputFieldAppearance(bool isSelected)
    {
        Image inputFieldImage = m_inputField.GetComponent<Image>();
        Sprite targetSprite = isSelected ? m_selectedInputSprite : m_defaultInputSprite;
        inputFieldImage.sprite = targetSprite;
    }

    private void UpdateChoiceAppearance(int selectedIndex)
    {
        for (int i = 0; i < m_currentChoices.Count; i++)
        {
            UpdateSingleChoiceAppearance(i, selectedIndex);
        }
    }

    private void UpdateSingleChoiceAppearance(int choiceIndex, int selectedIndex)
    {
        Toggle choice = m_currentChoices[choiceIndex];
        Image backgroundImage = choice.GetComponent<Image>();
        Image radioImage = GetRadioButtonImage(choice.gameObject);

        bool isSelected = (choiceIndex == selectedIndex);
        
        UpdateChoiceSprites(backgroundImage, radioImage, isSelected, false);
    }

    private void UpdateChoiceSprites(Image backgroundImage, Image radioImage, bool isSelected, bool isResult)
    {
        backgroundImage.sprite = GetChoiceSprite(isSelected, isResult, true);
        
        if (radioImage != null)
        {
            radioImage.sprite = GetChoiceSprite(isSelected, isResult, false);
        }
    }

    private Sprite GetChoiceSprite(bool isSelected, bool isResult, bool isBackground)
    {
        if (isResult)
        {
            return isSelected 
                ? (isBackground ? m_correctChoiceSprite : m_correctRadioSprite)
                : (isBackground ? m_defaultChoiceSprite : m_defaultRadioSprite);
        }
        else
        {
            return isSelected 
                ? (isBackground ? m_selectedChoiceSprite : m_selectedRadioSprite)
                : (isBackground ? m_defaultChoiceSprite : m_defaultRadioSprite);
        }
    }


    #endregion

    #region Private Methods - Answer Checking
    private bool CheckAnswer()
    {
        switch (m_currentQuizData.quizType)
        {
            case QuizType.MultipleChoiceText:
            case QuizType.MultipleChoiceImage:
                return m_selectedChoiceIndex == m_currentQuizData.correctAnswerIndex;
            case QuizType.OXQuiz:
                bool userAnswer = m_selectedChoiceIndex == 0; // 0 = O, 1 = X
                return userAnswer == m_currentQuizData.isCorrectAnswer;
            case QuizType.InputField:
                return CheckInputAnswer();
            default:
                return false;
        }
    }

    private bool CheckInputAnswer()
    {
        if (m_inputField == null) return false;
        
        string userInput = m_inputField.text.Trim();
        
        if (!float.TryParse(userInput, out float userAnswer))
        {
            
            return false;
        }
        
        if (!float.TryParse(m_currentQuizData.correctAnswer.Trim(), out float correctAnswer))
        {
            
            return false;
        }
        
        return Mathf.Abs(userAnswer - correctAnswer) < C_ANSWER_TOLERANCE;
    }
    #endregion

    #region Private Methods - Result Display
    private void ShowResult(bool isCorrect)
    {
        // 오답인 경우에만 선택지 비활성화 (정답일 때는 토글 상호작용 유지)
        if (!isCorrect)
        {
            DisableAllChoiceInteractions();
        }
        
        UpdateChoiceAppearanceForResult(isCorrect);
        UpdateButtonsForResult(isCorrect);
        
        // 정답 여부 저장 (콜백은 closeButton 클릭 시에만 호출)
        m_wasCorrectAnswer = isCorrect;
        
        // 오답인 경우에만 즉시 결과 콜백 호출 (retryButton 활성화를 위해)
        if (!isCorrect)
        {
            m_onQuizResult?.Invoke(isCorrect);
        }
    }

    private void UpdateButtonsForResult(bool isCorrect)
    {
        m_submitButton.gameObject.SetActive(false);
        
        if (isCorrect)
        {
            m_correctPanel.SetActive(true);
            m_closeButton.gameObject.SetActive(true);
        }
        else
        {
            m_wrongPanel.SetActive(true);
            m_retryButton.gameObject.SetActive(true);
        }
    }

    private void UpdateChoiceAppearanceForResult(bool isCorrect)
    {
        if (m_currentQuizData.quizType == QuizType.InputField)
        {
            UpdateInputFieldAppearanceForResult(isCorrect);
            return;
        }

        // 객관식 및 OX 퀴즈 처리
        for (int i = 0; i < m_currentChoices.Count; i++)
        {
            if (m_currentChoices[i] == null) continue;

            if (i == m_selectedChoiceIndex)
            {
                Image backgroundImage = m_currentChoices[i].GetComponent<Image>();
                Image radioImage = GetRadioButtonImage(m_currentChoices[i].gameObject);
                
                UpdateChoiceSprites(backgroundImage, radioImage, true, isCorrect);
            }
        }
    }

    private void UpdateInputFieldAppearanceForResult(bool isCorrect)
    {
        if (m_inputField == null) return;

        Image inputFieldImage = m_inputField.GetComponent<Image>();
        if (inputFieldImage != null)
        {
            Sprite targetSprite = isCorrect ? m_correctInputSprite : m_wrongInputSprite;
            if (targetSprite != null)
            {
                inputFieldImage.sprite = targetSprite;
            }
        }
        
        if (m_keyboardManager != null)
        {
            m_keyboardManager.HideKeyboard();
        }
    }

    private void DisableAllChoiceInteractions()
    {
        // 모든 Toggle 선택지 비활성화
        foreach (var choice in m_currentChoices)
        {
            if (choice != null)
            {
                choice.interactable = false;
            }
        }
        
        // Input Field 비활성화
        if (m_inputField != null)
        {
            m_inputField.interactable = false;
        }
    }

    private void EnableAllChoiceInteractions()
    {
        // 모든 Toggle 선택지 활성화
        foreach (var choice in m_currentChoices)
        {
            if (choice != null)
            {
                choice.interactable = true;
            }
        }
        
        // Input Field 활성화
        if (m_inputField != null)
        {
            m_inputField.interactable = true;
        }
    }

    private string GetResultText()
    {
        switch (m_currentQuizData.quizType)
        {
            case QuizType.OXQuiz:
                // 사용자가 선택한 답을 표시 (0 = O, 1 = X)
                return m_selectedChoiceIndex == 0 ? "O" : "X";
            case QuizType.InputField:
                // 사용자가 입력한 답을 표시
                return m_inputField?.text?.Trim() ?? "";
            case QuizType.MultipleChoiceText:
                // 사용자가 선택한 텍스트 답을 표시
                if (m_selectedChoiceIndex >= 0 && m_selectedChoiceIndex < m_currentQuizData.textChoices.Length)
                    return m_currentQuizData.textChoices[m_selectedChoiceIndex];
                return "완료!";
            default:
                return "완료!";
        }
    }

    private Color GetResultColor()
    {
        switch (m_currentQuizData.quizType)
        {
            case QuizType.OXQuiz:
                // 사용자가 선택한 답에 따라 색상 설정 (0 = O, 1 = X)
                return m_selectedChoiceIndex == 0 ? 
                    new Color32(0x1C, 0x63, 0xB8, 0xFF) : // O: #1C63B8 (파란색)
                    new Color32(0xC8, 0x24, 0x3E, 0xFF);  // X: #C8243E (빨간색)
            case QuizType.InputField:
                return Color.black; // 입력 필드는 검은색
            default:
                // 기본값은 #ab3e03 색상
                if (ColorUtility.TryParseHtmlString("#ab3e03", out Color defaultColor))
                    return defaultColor;
                return new Color32(0xAB, 0x3E, 0x03, 0xFF); // #ab3e03
        }
    }
    #endregion

    #region Context Menu (for Testing)
    [ContextMenu("Test - Text Quiz (2 choices)")]
    private void TestTextQuiz2() => TestQuiz(QuizType.MultipleChoiceText, 2);

    [ContextMenu("Test - Text Quiz (3 choices)")]
    private void TestTextQuiz3() => TestQuiz(QuizType.MultipleChoiceText, 3);

    [ContextMenu("Test - Text Quiz (4 choices)")]
    private void TestTextQuiz4() => TestQuiz(QuizType.MultipleChoiceText, 4);

    [ContextMenu("Test - Image Quiz (2 choices)")]
    private void TestImageQuiz2() => TestQuiz(QuizType.MultipleChoiceImage, 2);

    [ContextMenu("Test - Image Quiz (3 choices)")]
    private void TestImageQuiz3() => TestQuiz(QuizType.MultipleChoiceImage, 3);

    [ContextMenu("Test - Image Quiz (4 choices)")]
    private void TestImageQuiz4() => TestQuiz(QuizType.MultipleChoiceImage, 4);

    [ContextMenu("Test - OX Quiz")]
    private void TestOXQuiz() => TestQuiz(QuizType.OXQuiz, 2);

    [ContextMenu("Test - Input Quiz")]
    private void TestInputQuiz() => TestQuiz(QuizType.InputField, 0);

    private void TestQuiz(QuizType quizType, int choiceCount)
    {
        QuizData testQuiz = new QuizData
        {
            title = $"테스트 {quizType} 퀴즈",
            question = "테스트 질문입니다.",
            quizType = quizType,
            correctAnswerIndex = 0
        };

        switch (quizType)
        {
            case QuizType.MultipleChoiceText:
                testQuiz.textChoices = GenerateTestTextChoices(choiceCount);
                break;
            case QuizType.MultipleChoiceImage:
                testQuiz.imageChoices = CreateTestSprites(choiceCount);
                break;
            case QuizType.OXQuiz:
                testQuiz.isCorrectAnswer = true;
                break;
            case QuizType.InputField:
                testQuiz.correctAnswer = "100";
                break;
        }

        StartQuiz(testQuiz);
    }

    private string[] GenerateTestTextChoices(int count)
    {
        string[] choices = new string[count];
        for (int i = 0; i < count; i++)
        {
            choices[i] = $"선택지 {i + 1}";
        }
        return choices;
    }

    private Sprite[] CreateTestSprites(int count)
    {
        count = Mathf.Clamp(count, 1, 6);

        #if UNITY_EDITOR
        return LoadSpritesFromAssets(count);
        #else
        return CreateDefaultSprites(count);
        #endif
    }

    #if UNITY_EDITOR
    private Sprite[] LoadSpritesFromAssets(int count)
    {
        string[] spritePaths = {
            "Assets/06.Sprites/CommonUI/Next_Btn.png",
            "Assets/06.Sprites/CommonUI/Return Btn.png", 
            "Assets/06.Sprites/CommonUI/Click1.png",
            "Assets/06.Sprites/CommonUI/BtnBG.png",
            "Assets/06.Sprites/CommonUI/Quiz/_mark-success.png",
            "Assets/06.Sprites/CommonUI/Quiz/_mark-success-1.png"
        };
        
        Sprite[] sprites = new Sprite[count];
        
        for (int i = 0; i < count; i++)
        {
            sprites[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[i % spritePaths.Length]);
            
            if (sprites[i] == null)
            {
                sprites[i] = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            }
        }
        
        return sprites;
    }
    #endif

    private Sprite[] CreateDefaultSprites(int count)
    {
        Sprite[] sprites = new Sprite[count];
        Sprite[] defaultSprites = {
            Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd"),
            Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd"),
            Resources.GetBuiltinResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
            Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd"),
            Resources.GetBuiltinResource<Sprite>("UI/Skin/Checkmark.psd"),
            Resources.GetBuiltinResource<Sprite>("UI/Skin/DropdownArrow.psd")
        };
        
        for (int i = 0; i < count; i++)
        {
            sprites[i] = defaultSprites[i % defaultSprites.Length];
        }
        
        return sprites;
    }
    #endregion
}

public enum QuizType
{
    MultipleChoiceText,
    MultipleChoiceImage,
    OXQuiz,
    InputField
}