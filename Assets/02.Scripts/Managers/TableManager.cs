using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class TableManager : MonoBehaviour
{
    #region Serialized Fields
    [Header("Table Configuration")]
    [SerializeField] private Transform m_tableParent;
    [SerializeField] private int m_tableWidth = 3;
    [SerializeField] private int m_tableHeight = 8;
    [SerializeField] private GameObject m_cellPrefab;

    [Header("Cell Size Settings")]
    [SerializeField] private Vector2 m_spacing = new Vector2(6f, 6f);
    [SerializeField, Range(0f, 100f)] private float m_screenPadding = 10f;

    [Header("Width-specific Cell Sizes")]
    [SerializeField] private Vector2[] m_rowLabelSizes = new Vector2[]
    {
        new Vector2(224f, 224f), // width 2
        new Vector2(147f, 147f), // width 3  
        new Vector2(109f, 109f)  // width 4
    };

    [SerializeField] private Vector2[] m_headerSizes = new Vector2[]
    {
        new Vector2(224f, 92f),  // width 2
        new Vector2(147f, 128f), // width 3
        new Vector2(109f, 128f)  // width 4
    };

    [SerializeField] private Vector2[] m_cornerSizes = new Vector2[]
    {
        new Vector2(224f, 92f),  // width 2
        new Vector2(147f, 128f), // width 3
        new Vector2(109f, 128f)  // width 4
    };

    [SerializeField] private Vector2[] m_innerCellSizes = new Vector2[]
    {
        new Vector2(224f, 227f), // width 2
        new Vector2(147f, 147f), // width 3
        new Vector2(109f, 109f)  // width 4
    };

    [Header("Text Settings")]
    [SerializeField] private float m_fixedTextSize = 32f;

    [Header("Table Content Settings")]
    [SerializeField] private int m_highlightedRow = 1;

    [Header("Quiz System")]
    [SerializeField] private GameObject m_quizPanel;
    [SerializeField] private float m_cellRotationDuration = 0.5f;
    [SerializeField] private float m_quizPanelSlideDuration = 1.0f;

    [Header("Special Cell (0,0) Settings")]
    [SerializeField] private Sprite[] m_cornerCellSprites = new Sprite[3];
    [SerializeField] private string m_cornerCellTopText = "용질";
    [SerializeField] private string m_cornerCellBottomText = "회차";
    [SerializeField] private TMP_Text m_cornerTopTextComponent;
    [SerializeField] private TMP_Text m_cornerBottomTextComponent;

    [Header("Cell Sprites (per width)")]
    [SerializeField] private Sprite[] m_defaultHeaderSprites = new Sprite[3];
    [SerializeField] private Sprite[] m_defaultRowLabelSprites = new Sprite[3];
    [SerializeField] private Sprite[] m_innerDefaultSprites = new Sprite[3];
    [SerializeField] private Sprite[] m_innerHighlightSprites = new Sprite[3];
    [SerializeField] private Sprite[] m_innerDisabledSprites = new Sprite[3];
    [SerializeField] private Sprite[] m_answerCellSprites = new Sprite[3];
    #endregion

    #region Private Fields
    private Dictionary<Vector2Int, GameObject> m_cellDictionary = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, Button> m_cellButtons = new Dictionary<Vector2Int, Button>();
    private Dictionary<Vector2Int, TMP_Text> m_cellTexts = new Dictionary<Vector2Int, TMP_Text>();
    private Dictionary<Vector2Int, Image> m_cellImages = new Dictionary<Vector2Int, Image>();
    private Dictionary<Vector2Int, TMP_Text> m_cellBottomTexts = new Dictionary<Vector2Int, TMP_Text>();
    private Dictionary<Vector2Int, bool> m_cellEnabledStates = new Dictionary<Vector2Int, bool>();
    private Dictionary<Vector2Int, bool> m_cellAnsweredStates = new Dictionary<Vector2Int, bool>();
    private bool m_isInitialized = false;
    
    // 퀴즈 시스템 관련
    private bool m_isQuizActive = false;
    private RectTransform m_quizPanelRectTransform;
    private Vector2 m_quizPanelHiddenPosition;
    private Vector2 m_quizPanelVisiblePosition;
    private bool m_quizPanelPositionsCalculated = false;
    
    // 정답 완료 데이터 저장용
    private bool m_hasAnswerPending = false;
    private Vector2Int m_pendingAnswerPosition;
    private Sprite m_pendingChildImageSprite;
    private string m_pendingChildText;
    private Color? m_pendingTextColor;
    
    // 이벤트
    public static event Action<Vector2Int> OnInnerCellClicked;
    public static event Action<Vector2Int> OnQuizCompleted;
    public static event Action OnTableInitialized;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (m_tableParent == null) m_tableParent = transform;
    }

    private void Start()
    {
        Initialize();
    }
    #endregion

    #region Public Methods - Cell Management
    public void SetCellValue(Vector2Int position, string value)
    {
        if (!IsValidPosition(position) || !m_cellTexts.TryGetValue(position, out var textComponent)) return;

        textComponent.text = value;
        if (!textComponent.gameObject.activeSelf) textComponent.gameObject.SetActive(true);
        textComponent.enabled = true;
        UpdateTextSize(position, textComponent);

        if (IsInnerCellPosition(position))
        {
            textComponent.fontStyle = TMPro.FontStyles.Normal;
            textComponent.enableAutoSizing = false;
            textComponent.fontSize = 32f;
        }
    }

    public void SetCellImage(Vector2Int position, Sprite sprite)
    {
        if (!IsValidPosition(position) || !m_cellImages.TryGetValue(position, out var imageComponent)) return;

        imageComponent.sprite = sprite;
        imageComponent.enabled = sprite != null;
        if (sprite != null) imageComponent.preserveAspect = true;
    }

    public void SetCellActive(Vector2Int position, bool active)
    {
        if (m_cellDictionary.TryGetValue(position, out var cellObject))
            cellObject.SetActive(active);
    }

    public void SetCellInteractable(Vector2Int position, bool interactable)
    {
        if (m_cellButtons.TryGetValue(position, out var button))
            button.interactable = interactable;
    }

    public string GetCellValue(Vector2Int position)
    {
        return m_cellTexts.TryGetValue(position, out var textComponent) ? textComponent.text : string.Empty;
    }

    public void ClearAllCells()
    {
        foreach (var kvp in m_cellTexts)
        {
            if (kvp.Value != null && kvp.Value != m_cornerTopTextComponent)
                kvp.Value.text = string.Empty;
        }

        foreach (var kvp in m_cellBottomTexts)
        {
            if (kvp.Value != null && kvp.Value != m_cornerBottomTextComponent)
                kvp.Value.text = string.Empty;
        }

        foreach (var kvp in m_cellImages)
        {
            if (kvp.Value != null)
            {
                kvp.Value.sprite = null;
                kvp.Value.enabled = false;
            }
        }

        foreach (var pos in m_cellDictionary.Keys)
            SetCellBackgroundImage(pos, null);

        // 상태 초기화
        var enabledKeys = new List<Vector2Int>(m_cellEnabledStates.Keys);
        foreach (var pos in enabledKeys) m_cellEnabledStates[pos] = true;

        var answeredKeys = new List<Vector2Int>(m_cellAnsweredStates.Keys);
        foreach (var pos in answeredKeys) m_cellAnsweredStates[pos] = false;
    }

    public void SetCornerCellTexts(string topText, string bottomText)
    {
        Vector2Int cornerPos = new Vector2Int(0, 0);

        // 상단 텍스트 설정
        if (m_cellTexts.TryGetValue(cornerPos, out var topTextComponent))
        {
            topTextComponent.text = topText;
            UpdateTextSize(cornerPos, topTextComponent);
        }
        else if (m_cornerTopTextComponent != null)
        {
            m_cornerTopTextComponent.gameObject.SetActive(true);
            m_cornerTopTextComponent.enabled = true;
            m_cornerTopTextComponent.text = topText;
            UpdateTextSize(cornerPos, m_cornerTopTextComponent);
        }

        // 하단 텍스트 설정
        if (m_cellBottomTexts.TryGetValue(cornerPos, out var bottomTextComponent))
        {
            bottomTextComponent.text = bottomText;
            UpdateTextSize(cornerPos, bottomTextComponent);
        }
        else if (m_cornerBottomTextComponent != null)
        {
            m_cornerBottomTextComponent.gameObject.SetActive(true);
            m_cornerBottomTextComponent.enabled = true;
            m_cornerBottomTextComponent.text = bottomText;
            UpdateTextSize(cornerPos, m_cornerBottomTextComponent);
        }
    }

    public void SetInnerCellEnabled(Vector2Int position, bool enabled)
    {
        if (!IsInnerCellPosition(position)) return;

        m_cellEnabledStates[position] = enabled;
        UpdateInnerCellSprite(position);
        
        // 버튼 상태 업데이트
        if (m_cellButtons.TryGetValue(position, out var button))
        {
            bool isHighlighted = position.y == m_highlightedRow;
            bool isAnswered = GetInnerCellAnswered(position);
            button.interactable = isHighlighted && enabled && !isAnswered;
        }
    }

    public bool GetInnerCellEnabled(Vector2Int position)
    {
        return IsInnerCellPosition(position) && m_cellEnabledStates.TryGetValue(position, out var enabled) ? enabled : true;
    }

    public void SetInnerCellChildImageSprite(Vector2Int position, Sprite sprite, bool autoActivate = true)
    {
        if (!IsInnerCellPosition(position) || !m_cellDictionary.TryGetValue(position, out var cellObject)) return;

        Transform childImage = cellObject.transform.Find("InnerCellImage");
        if (childImage?.GetComponent<Image>() is Image imageComponent)
        {
            imageComponent.sprite = sprite;
            if (autoActivate && sprite != null)
                childImage.gameObject.SetActive(true);
        }
    }

    public void SetInnerCellAnswered(Vector2Int position, bool answered)
    {
        if (!IsInnerCellPosition(position)) return;

        m_cellAnsweredStates[position] = answered;
        if (answered) UpdateInnerCellSprite(position);
        
        // 버튼 상태 업데이트
        if (m_cellButtons.TryGetValue(position, out var button))
        {
            bool isHighlighted = position.y == m_highlightedRow;
            bool isEnabled = GetInnerCellEnabled(position);
            button.interactable = isHighlighted && isEnabled && !answered;
        }
    }

    public bool GetInnerCellAnswered(Vector2Int position)
    {
        return IsInnerCellPosition(position) && m_cellAnsweredStates.TryGetValue(position, out var answered) ? answered : false;
    }

    public void CompleteQuiz(Vector2Int position, Sprite childImageSprite = null, string childText = "", Color? textColor = null)
    {
        if (!IsInnerCellPosition(position)) return;

        m_hasAnswerPending = true;
        m_pendingAnswerPosition = position;
        m_pendingChildImageSprite = childImageSprite;
        m_pendingChildText = childText;
        m_pendingTextColor = textColor;
    }

    public void CloseQuizPanel()
    {
        if (!m_isQuizActive) return;
        
        m_isQuizActive = false;
        
        if (m_hasAnswerPending)
        {
            HideQuizPanelWithAnimation(() =>
            {
                StartCellAnswerAnimation(m_pendingAnswerPosition, m_pendingChildImageSprite, m_pendingChildText, m_pendingTextColor);
                ClearPendingAnswerData();
                
                // 행 완료 체크는 애니메이션 완료 후에 수행됨
            });
        }
        else
        {
            HideQuizPanelWithAnimation();
        }
    }

    public void OpenQuizPanel(Vector2Int position)
    {
        // 하이라이트된 행인지 확인
        if (position.y != m_highlightedRow) return;
        
        if (!IsInnerCellPosition(position) || 
            GetInnerCellAnswered(position) || 
            !GetInnerCellEnabled(position)) return;

        m_isQuizActive = true;
        ShowQuizPanelWithAnimation();
        OnInnerCellClicked?.Invoke(position);
    }

    public void SetHighlightedRow(int row)
    {
        if (row < 1 || row >= m_tableHeight) return;

        int previousRow = m_highlightedRow;
        m_highlightedRow = row;

        // 모든 내부 셀의 interactable 상태 업데이트
        for (int r = 1; r < m_tableHeight; r++)
        {
            for (int c = 1; c < m_tableWidth; c++)
            {
                Vector2Int pos = new Vector2Int(c, r);
                bool isHighlighted = r == m_highlightedRow;
                bool isEnabled = GetInnerCellEnabled(pos);
                bool isAnswered = GetInnerCellAnswered(pos);
                
                if (m_cellButtons.TryGetValue(pos, out var button))
                {
                    // 하이라이트된 행이고, 활성화되어 있고, 답변이 완료되지 않은 경우만 클릭 가능
                    button.interactable = isHighlighted && isEnabled && !isAnswered;
                }
            }
        }

        // 이전/새 하이라이트 행의 스프라이트 업데이트
        for (int col = 1; col < m_tableWidth; col++)
        {
            if (previousRow != row && previousRow >= 1 && previousRow < m_tableHeight)
                UpdateInnerCellSprite(new Vector2Int(col, previousRow));
            
            UpdateInnerCellSpriteWithAnimation(new Vector2Int(col, row));
        }
    }

    public int GetHighlightedRow() => m_highlightedRow;

    public void SetTableButtonsInteractable(bool interactable)
    {
        if (interactable)
        {
            // 활성화할 때는 각 셀의 상태에 따라 개별적으로 설정
            UpdateAllButtonStates();
        }
        else
        {
            // 비활성화할 때는 모든 버튼 비활성화
            foreach (var kvp in m_cellButtons)
            {
                if (kvp.Value != null) kvp.Value.interactable = false;
            }
        }
    }

    public bool IsRowCompleted(int row)
    {
        return IsRowCompletedInternal(row);
    }

    public int GetCompletedRowCount()
    {
        int completedCount = 0;
        for (int row = 1; row < m_tableHeight; row++)
        {
            if (IsRowCompletedInternal(row))
            {
                completedCount++;
            }
        }
        return completedCount;
    }

    public float GetProgressPercentage()
    {
        int totalRows = m_tableHeight - 1; // 헤더 행 제외
        int completedRows = GetCompletedRowCount();
        return totalRows > 0 ? (float)completedRows / totalRows * 100f : 0f;
    }

    public bool IsCurrentRowCompleted()
    {
        return IsCurrentRowCompletedInternal();
    }
    #endregion

    #region Public Methods - Easy Data Input
    public void SetHeaderRowTexts(string[] headerTexts, string cornerBottomText = "")
    {
        if (headerTexts == null || headerTexts.Length == 0) return;

        for (int col = 0; col < Mathf.Min(headerTexts.Length, m_tableWidth - 1); col++)
        {
            Vector2Int position = new Vector2Int(col + 1, 0);
            SetCellValue(position, headerTexts[col]);
        }
    }

    public void SetFirstColumnTexts(string[] labelTexts)
    {
        if (labelTexts == null || labelTexts.Length == 0) return;

        for (int row = 1; row < Mathf.Min(labelTexts.Length + 1, m_tableHeight); row++)
        {
            Vector2Int position = new Vector2Int(0, row);
            SetCellValue(position, labelTexts[row - 1]);
        }
    }

    public void SetInnerCellData(string[][] data)
    {
        if (data == null) return;

        for (int row = 0; row < data.Length && row + 1 < m_tableHeight; row++)
        {
            if (data[row] == null) continue;

            for (int col = 0; col < data[row].Length && col + 1 < m_tableWidth; col++)
            {
                Vector2Int position = new Vector2Int(col + 1, row + 1);
                SetCellValue(position, data[row][col]);
            }
        }
    }
    #endregion

    #region Public Methods - Setup
    public void Initialize()
    {
        if (m_isInitialized && Application.isPlaying) return;

        FindAndCacheCells();
        SetupManualLayout();
        m_isInitialized = true;
        
        UpdateTableContent();
        
        if (m_quizPanel != null) CalculateQuizPanelPositions();
        
        // 초기화 완료 후 버튼 상태 업데이트
        UpdateAllButtonStates();
        
        OnTableInitialized?.Invoke();
    }
    
    public bool IsTableInitialized() => m_isInitialized && m_cellDictionary.Count > 0;
    public bool IsTableValid() => m_isInitialized && m_cellDictionary.Count == m_tableWidth * m_tableHeight;
    public bool DoesCellExist(Vector2Int position) => m_cellDictionary.ContainsKey(position);
    #endregion

    #region Private Methods - Core
    private void FindAndCacheCells()
    {
        m_cellDictionary.Clear();
        m_cellButtons.Clear();
        m_cellTexts.Clear();
        m_cellImages.Clear();
        m_cellBottomTexts.Clear();
        m_cellEnabledStates.Clear();
        m_cellAnsweredStates.Clear();

        // 기존 셀 보존 로직 (코너 셀)
        GameObject cornerCellToPreserve = null;
        if (m_cornerTopTextComponent != null || m_cornerBottomTextComponent != null)
        {
            Transform searchParent = m_cornerTopTextComponent?.transform ?? m_cornerBottomTextComponent.transform;
            while (searchParent.parent != null && searchParent.parent != m_tableParent)
                searchParent = searchParent.parent;

            if (searchParent.parent == m_tableParent)
                cornerCellToPreserve = searchParent.gameObject;
        }

        // 기존 셀 제거 (보존할 셀 제외)
        for (int i = m_tableParent.childCount - 1; i >= 0; i--)
        {
            GameObject child = m_tableParent.GetChild(i).gameObject;
            if (child != cornerCellToPreserve) DestroyImmediate(child);
        }

        // 새 셀 생성
        for (int y = 0; y < m_tableHeight; y++)
        {
            for (int x = 0; x < m_tableWidth; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                GameObject cellToUse = null;

                if (x == 0 && y == 0 && cornerCellToPreserve != null)
                {
                    cellToUse = cornerCellToPreserve;
                    cellToUse.name = $"Cell_{x}_{y}";
                }
                else if (m_cellPrefab != null)
                {
                    cellToUse = Instantiate(m_cellPrefab, m_tableParent);
                    cellToUse.name = $"Cell_{x}_{y}";
                }

                if (cellToUse != null)
                {
                    CacheCellComponents(cellToUse, pos);
                    if (IsInnerCellPosition(pos))
                    {
                        m_cellEnabledStates[pos] = true;
                        m_cellAnsweredStates[pos] = false;
                    }
                }
            }
        }
    }

    private void CacheCellComponents(GameObject cellObject, Vector2Int position)
    {
        m_cellDictionary[position] = cellObject;

        // 내부 셀인 경우 버튼 컴포넌트 추가
        if (IsInnerCellPosition(position))
        {
            if (!cellObject.TryGetComponent<Button>(out var button))
                button = cellObject.AddComponent<Button>();
            
            m_cellButtons[position] = button;
            
            // 버튼의 색상 블록 설정 (비활성화 상태를 흰색으로 설정)
            var colors = button.colors;
            colors.disabledColor = new Color(1f, 1f, 1f, 1f); // ffffff, 알파 255
            button.colors = colors;
            
            Vector2Int cellPos = position;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OpenQuizPanel(cellPos));
            
            CreateInnerCellChildImage(cellObject, position);
        }
        else if (cellObject.TryGetComponent<Button>(out var button))
        {
            m_cellButtons[position] = button;
        }

        // 텍스트 컴포넌트 찾기
        var textComponents = cellObject.GetComponentsInChildren<TMP_Text>(true);
        if (textComponents.Length > 0)
        {
            if (position.x == 0 && position.y == 0)
            {
                m_cellTexts[position] = m_cornerTopTextComponent ?? textComponents[0];
                if (textComponents.Length > 1)
                    m_cellBottomTexts[position] = m_cornerBottomTextComponent ?? textComponents[1];
            }
            else
            {
                m_cellTexts[position] = textComponents[0];
            }
        }

        // 이미지 컴포넌트 찾기
        var childImages = cellObject.GetComponentsInChildren<Image>(true);
        foreach (var img in childImages)
        {
            if (img.gameObject != cellObject)
            {
                m_cellImages[position] = img;
                break;
            }
        }
        if (!m_cellImages.ContainsKey(position) && cellObject.TryGetComponent<Image>(out var mainImg))
            m_cellImages[position] = mainImg;
    }

    private void CreateInnerCellChildImage(GameObject parentCell, Vector2Int position)
    {
        if (parentCell.transform.Find("InnerCellImage") != null) return;

        GameObject imageObject = new GameObject("InnerCellImage");
        imageObject.transform.SetParent(parentCell.transform, false);

        RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        Image imageComponent = imageObject.AddComponent<Image>();
        imageComponent.sprite = null;
        imageComponent.color = Color.white;
        imageComponent.raycastTarget = false;

        imageObject.SetActive(false);
    }

    private void SetupManualLayout()
    {
        // 각 열/행의 최대 크기 계산
        float[] columnWidths = new float[m_tableWidth];
        float[] rowHeights = new float[m_tableHeight];

        for (int y = 0; y < m_tableHeight; y++)
        {
            for (int x = 0; x < m_tableWidth; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Vector2 cellSize = GetCellSizeForType(GetCellType(pos), pos);
                columnWidths[x] = Mathf.Max(columnWidths[x], cellSize.x);
                rowHeights[y] = Mathf.Max(rowHeights[y], cellSize.y);
            }
        }

        // 각 셀의 위치와 크기 설정
        for (int y = 0; y < m_tableHeight; y++)
        {
            for (int x = 0; x < m_tableWidth; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (m_cellDictionary.TryGetValue(pos, out GameObject cellObject))
                {
                    RectTransform rectTransform = cellObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        bool isInnerCell = IsInnerCellPosition(pos);
                        
                        rectTransform.anchorMin = new Vector2(0, 1);
                        rectTransform.anchorMax = new Vector2(0, 1);
                        rectTransform.pivot = isInnerCell ? new Vector2(0.5f, 0.5f) : new Vector2(0, 1);

                        Vector2 uniformSize = new Vector2(columnWidths[x], rowHeights[y]);
                        rectTransform.sizeDelta = uniformSize;

                        Vector2 cellPosition = CalculateGridCellPosition(x, y, columnWidths, rowHeights);
                        if (isInnerCell)
                        {
                            cellPosition.x += uniformSize.x * 0.5f;
                            cellPosition.y -= uniformSize.y * 0.5f;
                        }
                        rectTransform.anchoredPosition = cellPosition;
                    }
                }
            }
        }
    }

    private Vector2 CalculateGridCellPosition(int x, int y, float[] columnWidths, float[] rowHeights)
    {
        float posX = m_screenPadding;
        float posY = -m_screenPadding;

        for (int i = 0; i < x; i++) posX += columnWidths[i] + m_spacing.x;
        for (int i = 0; i < y; i++) posY -= (rowHeights[i] + m_spacing.y);

        return new Vector2(posX, posY);
    }
    #endregion

    #region Private Methods - Content Update
    private void UpdateTableContent()
    {
        if (!m_isInitialized) return;

        ClearAllCells();
        UpdateHeaderRow();
        UpdateLabelColumn();
        UpdateInnerCells();
        UpdateAllTextSizes();
        UpdateAllButtonStates();
    }

    private void SetCellBackgroundImage(Vector2Int position, Sprite sprite)
    {
        if (!m_cellDictionary.TryGetValue(position, out var cellObject)) return;

        if (cellObject.TryGetComponent<Image>(out var bgImage))
        {
            bgImage.sprite = sprite;
            bgImage.enabled = sprite != null;
            if (sprite != null) bgImage.preserveAspect = true;
        }
    }

    private void UpdateHeaderRow()
    {
        int spriteIndex = Mathf.Clamp(m_tableWidth - 2, 0, 2);

        for (int col = 0; col < m_tableWidth; col++)
        {
            Vector2Int position = new Vector2Int(col, 0);
            Sprite sprite = null;

            if (col == 0)
            {
                if (m_cornerCellSprites != null && spriteIndex < m_cornerCellSprites.Length)
                    sprite = m_cornerCellSprites[spriteIndex];
            }
            else
            {
                if (m_defaultHeaderSprites != null && spriteIndex < m_defaultHeaderSprites.Length)
                    sprite = m_defaultHeaderSprites[spriteIndex];
            }

            SetCellBackgroundImage(position, sprite);
            SetCellImage(position, null);
        }
    }

    private void UpdateLabelColumn()
    {
        int spriteIndex = Mathf.Clamp(m_tableWidth - 2, 0, 2);

        for (int row = 1; row < m_tableHeight; row++)
        {
            Vector2Int position = new Vector2Int(0, row);
            Sprite sprite = null;
            
            if (m_defaultRowLabelSprites != null && spriteIndex < m_defaultRowLabelSprites.Length)
                sprite = m_defaultRowLabelSprites[spriteIndex];

            SetCellBackgroundImage(position, sprite);
            SetCellImage(position, null);
        }
    }

    private void UpdateInnerCells()
    {
        for (int row = 1; row < m_tableHeight; row++)
        {
            for (int col = 1; col < m_tableWidth; col++)
            {
                Vector2Int position = new Vector2Int(col, row);
                UpdateInnerCellSprite(position);
                SetCellImage(position, null);
                SetCellValue(position, "");
            }
        }
    }

    private void UpdateAllButtonStates()
    {
        // 모든 내부 셀의 버튼 상태 업데이트
        for (int row = 1; row < m_tableHeight; row++)
        {
            for (int col = 1; col < m_tableWidth; col++)
            {
                Vector2Int pos = new Vector2Int(col, row);
                bool isHighlighted = row == m_highlightedRow;
                bool isEnabled = GetInnerCellEnabled(pos);
                bool isAnswered = GetInnerCellAnswered(pos);
                
                if (m_cellButtons.TryGetValue(pos, out var button))
                {
                    button.interactable = isHighlighted && isEnabled && !isAnswered;
                }
            }
        }
    }
    #endregion

    #region Private Methods - Quiz Panel Animation
    private void ClearPendingAnswerData()
    {
        m_hasAnswerPending = false;
        m_pendingAnswerPosition = Vector2Int.zero;
        m_pendingChildImageSprite = null;
        m_pendingChildText = "";
        m_pendingTextColor = null;
    }

    private void CheckAndMoveToNextRow()
    {
        // 현재 하이라이트된 행의 모든 셀이 답변 완료되었는지 확인
        if (IsCurrentRowCompletedInternal())
        {
            // 다음 행으로 이동 (마지막 행이 아닌 경우)
            if (m_highlightedRow < m_tableHeight - 1)
            {
                SetHighlightedRow(m_highlightedRow + 1);
                UpdateAllButtonStates();
            }
        }
    }

    private bool IsCurrentRowCompletedInternal()
    {
        return IsRowCompletedInternal(m_highlightedRow);
    }

    private bool IsRowCompletedInternal(int row)
    {
        if (row < 1 || row >= m_tableHeight) return false;

        // 해당 행의 모든 내부 셀이 답변 완료되었는지 확인
        for (int col = 1; col < m_tableWidth; col++)
        {
            Vector2Int position = new Vector2Int(col, row);
            
            // 셀이 활성화되어 있고 답변이 완료되지 않았다면 행이 완료되지 않음
            if (GetInnerCellEnabled(position) && !GetInnerCellAnswered(position))
            {
                return false;
            }
        }
        
        return true;
    }

    private void CalculateQuizPanelPositions()
    {
        if (m_quizPanel == null) return;

        m_quizPanelRectTransform = m_quizPanel.GetComponent<RectTransform>();
        if (m_quizPanelRectTransform == null) return;

        m_quizPanelHiddenPosition = new Vector2(-474f, m_quizPanelRectTransform.anchoredPosition.y);
        m_quizPanelVisiblePosition = new Vector2(0f, m_quizPanelRectTransform.anchoredPosition.y);
        m_quizPanelPositionsCalculated = true;
    }

    private void ShowQuizPanelWithAnimation()
    {
        if (m_quizPanel == null) return;

        if (!m_quizPanelPositionsCalculated) CalculateQuizPanelPositions();
        if (m_quizPanelRectTransform == null) return;

        SetTableButtonsInteractable(false);
        m_quizPanel.SetActive(true);
        m_quizPanelRectTransform.anchoredPosition = m_quizPanelHiddenPosition;

        m_quizPanelRectTransform.DOAnchorPos(m_quizPanelVisiblePosition, m_quizPanelSlideDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => SetTableButtonsInteractable(true));
    }

    private void HideQuizPanelWithAnimation(System.Action onComplete = null)
    {
        if (m_quizPanel == null || !m_quizPanel.activeSelf) 
        {
            onComplete?.Invoke();
            return;
        }

        if (!m_quizPanelPositionsCalculated) CalculateQuizPanelPositions();
        if (m_quizPanelRectTransform == null) 
        {
            m_quizPanel.SetActive(false);
            onComplete?.Invoke();
            return;
        }

        SetTableButtonsInteractable(false);

        m_quizPanelRectTransform.DOAnchorPos(m_quizPanelHiddenPosition, m_quizPanelSlideDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                m_quizPanel.SetActive(false);
                m_quizPanelRectTransform.anchoredPosition = m_quizPanelVisiblePosition;
                SetTableButtonsInteractable(true);
                onComplete?.Invoke();
            });
    }

    private void StartCellAnswerAnimation(Vector2Int position, Sprite childImageSprite, string childText, Color? textColor = null)
    {
        if (!m_cellDictionary.TryGetValue(position, out var cellObject)) return;

        SetInnerCellAnswered(position, true);

        Sequence animationSequence = DOTween.Sequence();

        animationSequence.Append(
            cellObject.transform.DORotate(new Vector3(0f, 90f, 0f), m_cellRotationDuration * 0.5f)
                .SetEase(Ease.InQuad)
        );

        animationSequence.AppendCallback(() =>
        {
            // Answer 스프라이트로 변경
            int spriteIndex = Mathf.Clamp(m_tableWidth - 2, 0, 2);
            if (m_answerCellSprites != null && spriteIndex < m_answerCellSprites.Length)
                SetCellBackgroundImage(position, m_answerCellSprites[spriteIndex]);

            // 자식 요소 설정
            if (childImageSprite != null)
                SetInnerCellChildImageSprite(position, childImageSprite, true);

            if (!string.IsNullOrEmpty(childText))
            {
                SetCellValue(position, childText);
                
                if (m_cellTexts.TryGetValue(position, out var textComponent))
                {
                    if (textColor.HasValue) textComponent.color = textColor.Value;
                    textComponent.fontSize = (childText == "O" || childText == "X") ? 60f : 32f;
                    textComponent.fontStyle = TMPro.FontStyles.Normal;
                    textComponent.enableAutoSizing = false;
                }
            }
        });

        animationSequence.Append(
            cellObject.transform.DORotate(new Vector3(0f, 0f, 0f), m_cellRotationDuration * 0.5f)
                .SetEase(Ease.OutQuad)
        );

        animationSequence.OnComplete(() => {
            OnQuizCompleted?.Invoke(position);
            
            // 애니메이션 완료 후 행 완료 체크
            CheckAndMoveToNextRow();
        });
        animationSequence.Play();
    }
    #endregion

    #region Private Methods - Utility
    private Vector2 GetCellSizeForType(CellType cellType, Vector2Int position)
    {
        int sizeIndex = Mathf.Clamp(m_tableWidth - 2, 0, 2);

        return cellType switch
        {
            CellType.Corner => m_cornerSizes?[sizeIndex] ?? Vector2.zero,
            CellType.Header => m_headerSizes?[sizeIndex] ?? Vector2.zero,
            CellType.RowLabel => m_rowLabelSizes?[sizeIndex] ?? Vector2.zero,
            CellType.Inner => m_innerCellSizes?[sizeIndex] ?? Vector2.zero,
            _ => Vector2.zero
        };
    }

    private CellType GetCellType(Vector2Int position)
    {
        if (position.x == 0 && position.y == 0) return CellType.Corner;
        if (position.y == 0) return CellType.Header;
        if (position.x == 0) return CellType.RowLabel;
        return CellType.Inner;
    }

    private enum CellType { Corner, Header, RowLabel, Inner }

    private void UpdateTextSize(Vector2Int position, TMP_Text textComponent)
    {
        if (textComponent == null) return;

        CellType cellType = GetCellType(position);
        textComponent.fontSize = m_fixedTextSize;

        if (cellType == CellType.Inner)
        {
            textComponent.fontStyle = TMPro.FontStyles.Normal;
            textComponent.enableAutoSizing = false;
        }
        else if (textComponent.enableAutoSizing)
        {
            textComponent.fontSizeMax = m_fixedTextSize;
            textComponent.fontSizeMin = m_fixedTextSize * 0.7f;
        }
    }

    private void UpdateAllTextSizes()
    {
        foreach (var kvp in m_cellTexts) UpdateTextSize(kvp.Key, kvp.Value);
        foreach (var kvp in m_cellBottomTexts) UpdateTextSize(kvp.Key, kvp.Value);
    }

    private bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < m_tableWidth &&
               position.y >= 0 && position.y < m_tableHeight &&
               m_cellDictionary.ContainsKey(position);
    }

    private bool IsInnerCellPosition(Vector2Int position)
    {
        return position.x > 0 && position.y > 0 &&
               position.x < m_tableWidth && position.y < m_tableHeight;
    }

    private void UpdateInnerCellSprite(Vector2Int position)
    {
        if (!IsInnerCellPosition(position)) return;

        int spriteIndex = Mathf.Clamp(m_tableWidth - 2, 0, 2);
        bool isEnabled = GetInnerCellEnabled(position);
        bool isAnswered = GetInnerCellAnswered(position);
        bool isHighlighted = position.y == m_highlightedRow;

        Sprite bgSprite = null;

        if (isAnswered && m_answerCellSprites?[spriteIndex] != null)
            bgSprite = m_answerCellSprites[spriteIndex];
        else if (!isEnabled && m_innerDisabledSprites?[spriteIndex] != null)
            bgSprite = m_innerDisabledSprites[spriteIndex];
        else if (isHighlighted && m_innerHighlightSprites?[spriteIndex] != null)
            bgSprite = m_innerHighlightSprites[spriteIndex];
        else if (m_innerDefaultSprites?[spriteIndex] != null)
            bgSprite = m_innerDefaultSprites[spriteIndex];

        SetCellBackgroundImage(position, bgSprite);
    }

    private void UpdateInnerCellSpriteWithAnimation(Vector2Int position)
    {
        if (!IsInnerCellPosition(position) || !m_cellDictionary.TryGetValue(position, out var cellObject)) return;

        // 스프라이트 업데이트
        UpdateInnerCellSprite(position);

        // 하이라이트 애니메이션 (펀치 스케일 효과)
        if (position.y == m_highlightedRow)
        {
            cellObject.transform.DOKill(); // 기존 애니메이션 정리
            cellObject.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 1, 0.5f)
                .SetEase(Ease.OutQuad);
        }
    }
    #endregion

    #region Context Menu Methods
    [ContextMenu("Test - Set Highlighted Row to 1")]
    private void TestSetHighlightedRow1() => SetHighlightedRow(1);

    [ContextMenu("Test - Click Highlighted Cell (1,1)")]
    private void TestClickHighlightedCell() => OpenQuizPanel(new Vector2Int(1, m_highlightedRow));

    [ContextMenu("Test - Close Quiz Panel")]
    private void TestCloseQuizPanel() => CloseQuizPanel();
    #endregion
}



