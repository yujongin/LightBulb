using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// 키보드 UI를 관리하는 매니저 클래스
/// WebGL 환경에서 가상 키보드 표시/숨김 및 애니메이션 처리
/// 
/// 사용법:
/// 1. 이 스크립트를 매니저 GameObject에 부착
/// 2. 키보드 프리팹을 할당
/// 3. Auto Find Input Fields를 체크하면 자동으로 모든 InputField와 연결됨
/// </summary>
public class KeyBoardUIManager : MonoBehaviour
{
    #region Serialized Fields
    [Header("키보드 설정")]
    [SerializeField] private GameObject m_keyboardPrefab;
    [SerializeField] private bool m_showOnStart = false;

    [Header("InputField 설정")]
    [SerializeField] private TMP_InputField[] m_manualInputFields = new TMP_InputField[0]; // 수동으로 할당할 InputField들
    [SerializeField] private bool m_autoFindInputFields = true; // 자동으로 InputField 찾기
    [SerializeField] private string[] m_excludeInputFieldNames = new string[0]; // 제외할 InputField 이름들

    [Header("버튼 눌림 효과 설정")]
    [SerializeField] private bool m_enableButtonPressEffect = true;
    [SerializeField] private float m_buttonPressScale = 0.9f;
    [SerializeField] private float m_buttonPressDuration = 0.1f;
    [SerializeField] private Ease m_buttonPressEase = Ease.OutQuad;
    [SerializeField] private Ease m_buttonReleaseEase = Ease.OutBounce;

    [Header("리소스 자동 적용 설정")]
    [SerializeField] private bool m_autoApplyResources = true; // 자동으로 패키지 리소스 적용
    [SerializeField] private bool m_applyKeySprite = true; // 키 스프라이트 자동 적용
    [SerializeField] private bool m_applyFont = true; // 폰트 자동 적용
    #endregion

    #region Private Fields
    private RectTransform m_keyboardRectTransform;
    private bool m_isKeyboardVisible = false;
    private TMP_InputField m_currentInputField; // 현재 연결된 InputField
    private List<TMP_InputField> m_registeredInputFields = new List<TMP_InputField>(); // 등록된 InputField 목록
    #endregion

    #region Properties

    /// <summary>
    /// 키보드가 현재 표시되고 있는지 여부
    /// </summary>
    public bool IsKeyboardVisible
    {
        get
        {
            return m_keyboardPrefab != null && m_keyboardPrefab.activeInHierarchy;
        }
    }

    /// <summary>
    /// 현재 연결된 InputField
    /// </summary>
    public TMP_InputField CurrentInputField => m_currentInputField;

    /// <summary>
    /// 등록된 InputField 목록
    /// </summary>
    public IReadOnlyList<TMP_InputField> RegisteredInputFields => m_registeredInputFields.AsReadOnly();

    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializeKeyboard();

        // 먼저 수동으로 할당된 InputField들을 등록
        RegisterManualInputFields();

        // 수동 할당이 없거나 자동 검색이 활성화된 경우 자동으로 씬의 InputField들을 찾아서 등록
        if ((m_manualInputFields == null || m_manualInputFields.Length == 0) && m_autoFindInputFields)
        {
            
            AutoRegisterInputFields();
        }
        else if (m_manualInputFields != null && m_manualInputFields.Length > 0 && m_autoFindInputFields)
        {
            
            AutoRegisterInputFields();
        }

        if (m_showOnStart)
        {
            ShowKeyboard();
        }
    }

    private void OnDestroy()
    {
        // 모든 등록된 InputField 해제
        UnregisterAllInputFields();

        // 버튼 눌림 애니메이션들만 정리
        if (m_keyboardPrefab != null)
        {
            Transform bgTransform = m_keyboardPrefab.transform.Find("BG");
            if (bgTransform != null)
            {
                Button[] keyButtons = bgTransform.GetComponentsInChildren<Button>();
                foreach (Button keyButton in keyButtons)
                {
                    if (keyButton != null && keyButton.transform != null)
                    {
                        keyButton.transform.DOKill();
                    }
                }
            }
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 키보드를 표시합니다
    /// </summary>
    public void ShowKeyboard()
    {
        if (m_keyboardPrefab == null)
        {
            
            return;
        }

        if (m_isKeyboardVisible)
        {
            
            return;
        }

        
        m_keyboardPrefab.SetActive(true);
        m_isKeyboardVisible = true;
    }

    /// <summary>
    /// 키보드를 표시하고 InputField를 연결합니다
    /// </summary>
    /// <param name="inputField">연결할 InputField</param>
    public void ShowKeyboard(TMP_InputField inputField)
    {
        if (inputField == null)
        {
            
            return;
        }

        m_currentInputField = inputField;
        ShowKeyboard();
        
        
    }

    /// <summary>
    /// 키보드를 숨깁니다
    /// </summary>
    public void HideKeyboard()
    {
        if (m_keyboardPrefab == null || !m_isKeyboardVisible) return;

        m_isKeyboardVisible = false;
        m_currentInputField = null; // InputField 연결 해제

        
        m_keyboardPrefab.SetActive(false);
    }

    /// <summary>
    /// 키보드 표시/숨김을 토글합니다
    /// </summary>
    public void ToggleKeyboard()
    {
        if (m_isKeyboardVisible)
        {
            HideKeyboard();
        }
        else
        {
            ShowKeyboard();
        }
    }

    /// <summary>
    /// 키보드 위치를 즉시 설정합니다 (현재는 사용하지 않음)
    /// </summary>
    /// <param name="position">설정할 위치</param>
    public void SetKeyboardPosition(Vector2 position)
    {
        if (m_keyboardRectTransform != null)
        {
            m_keyboardRectTransform.anchoredPosition = position;
            
        }
    }

    /// <summary>
    /// 키 입력을 처리합니다
    /// </summary>
    /// <param name="keyValue">입력된 키 값</param>
    public void OnKeyPressed(string keyValue)
    {
        
        

        if (m_currentInputField == null)
        {
            
            

            // 등록된 InputField가 있다면 첫 번째를 자동으로 연결
            if (m_registeredInputFields.Count > 0 && m_registeredInputFields[0] != null)
            {
                
                m_currentInputField = m_registeredInputFields[0];
            }
            else
            {
                
                return;
            }
        }

        

        // 특수 키 처리
        switch (keyValue.ToLower())
        {
            case "backspace":
                HandleBackspace();
                break;
            case "space":
                HandleSpace();
                break;
            case "enter":
                HandleEnter();
                break;
            case "clear":
                HandleClear();
                break;
            case "period":
                HandlePeriod();
                break;
            default:
                HandleNormalKey(keyValue);
                break;
        }

        
    }

    /// <summary>
    /// 버튼 눌림 효과 설정을 변경합니다
    /// </summary>
    /// <param name="enable">효과 활성화 여부</param>
    public void SetButtonPressEffectEnabled(bool enable)
    {
        m_enableButtonPressEffect = enable;
        
    }

    /// <summary>
    /// 버튼 눌림 효과의 스케일을 설정합니다
    /// </summary>
    /// <param name="scale">눌림 스케일 (0.1 ~ 1.0)</param>
    public void SetButtonPressScale(float scale)
    {
        m_buttonPressScale = Mathf.Clamp(scale, 0.1f, 1.0f);
        
    }

    /// <summary>
    /// InputField를 키보드 매니저에 등록합니다
    /// </summary>
    /// <param name="inputField">등록할 InputField</param>
    public void RegisterInputField(TMP_InputField inputField)
    {
        if (inputField == null)
        {
            
            return;
        }

        if (m_registeredInputFields.Contains(inputField))
        {
            
            return;
        }

        // InputField를 목록에 추가
        m_registeredInputFields.Add(inputField);

        // 이벤트 리스너 추가
        inputField.onSelect.AddListener((text) => OnInputFieldSelected(inputField));
        inputField.onDeselect.AddListener((text) => OnInputFieldDeselected(inputField));
        inputField.onSubmit.AddListener((text) => OnInputFieldSubmitted(inputField, text));

        
    }

    /// <summary>
    /// InputField를 키보드 매니저에서 등록 해제합니다
    /// </summary>
    /// <param name="inputField">등록 해제할 InputField</param>
    public void UnregisterInputField(TMP_InputField inputField)
    {
        if (inputField == null || !m_registeredInputFields.Contains(inputField))
        {
            return;
        }

        // 이벤트 리스너 제거
        inputField.onSelect.RemoveListener((text) => OnInputFieldSelected(inputField));
        inputField.onDeselect.RemoveListener((text) => OnInputFieldDeselected(inputField));
        inputField.onSubmit.RemoveListener((text) => OnInputFieldSubmitted(inputField, text));

        // 목록에서 제거
        m_registeredInputFields.Remove(inputField);

        // 현재 활성 InputField인 경우 키보드 숨김
        if (m_currentInputField == inputField)
        {
            HideKeyboard();
        }

        
    }

    /// <summary>
    /// 모든 등록된 InputField를 해제합니다
    /// </summary>
    public void UnregisterAllInputFields()
    {
        // 복사본을 만들어서 순회 (원본 리스트가 수정되는 것을 방지)
        var inputFieldsCopy = new List<TMP_InputField>(m_registeredInputFields);

        foreach (var inputField in inputFieldsCopy)
        {
            UnregisterInputField(inputField);
        }

        
    }

    /// <summary>
    /// 특정 InputField로 키보드를 직접 연결합니다
    /// </summary>
    /// <param name="inputField">연결할 InputField</param>
    public void ConnectToInputField(TMP_InputField inputField)
    {
        if (inputField == null)
        {
            
            return;
        }

        // 등록되지 않은 InputField인 경우 자동 등록
        if (!m_registeredInputFields.Contains(inputField))
        {
            RegisterInputField(inputField);
        }

        // 키보드 표시 및 연결
        ShowKeyboard(inputField);
    }

    /// <summary>
    /// 수동으로 씬의 InputField들을 다시 검색하여 등록합니다
    /// </summary>
    public void RefreshInputFields()
    {
        

        // 기존 등록된 InputField들 해제
        UnregisterAllInputFields();

        // 수동 할당된 InputField들을 먼저 등록
        RegisterManualInputFields();

        // 자동 검색이 활성화된 경우 추가 등록
        if (m_autoFindInputFields)
        {
            AutoRegisterInputFields();
        }
    }

    /// <summary>
    /// 자동 InputField 검색 기능을 설정합니다
    /// </summary>
    /// <param name="enable">자동 검색 활성화 여부</param>
    public void SetAutoFindInputFields(bool enable)
    {
        m_autoFindInputFields = enable;
        

        if (enable)
        {
            RefreshInputFields();
        }
    }

    /// <summary>
    /// 제외할 InputField 이름 목록을 설정합니다
    /// </summary>
    /// <param name="excludeNames">제외할 이름들</param>
    public void SetExcludeInputFieldNames(params string[] excludeNames)
    {
        m_excludeInputFieldNames = excludeNames ?? new string[0];
        

        if (m_autoFindInputFields)
        {
            RefreshInputFields();
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 키보드 초기화
    /// </summary>
    private void InitializeKeyboard()
    {
        if (m_keyboardPrefab == null)
        {
            
            return;
        }

        // 키보드 프리팹의 RectTransform 참조
        m_keyboardRectTransform = m_keyboardPrefab.GetComponent<RectTransform>();

        if (m_keyboardRectTransform == null)
        {
            
            return;
        }

        // Canvas 컴포넌트 확인 및 설정
        Canvas keyboardCanvas = m_keyboardPrefab.GetComponent<Canvas>();
        if (keyboardCanvas != null)
        {
            keyboardCanvas.sortingOrder = 1000; // 다른 UI보다 위에 표시
            keyboardCanvas.overrideSorting = true;
        }

        // 프리팹의 초기 위치 유지하고 비활성화만 실행
        m_keyboardPrefab.SetActive(false);

        // BG 하위의 키 버튼들 설정
        SetupKeyButtons();

        // 리소스 자동 적용
        if (m_autoApplyResources)
        {
            ApplyKeyboardResources();
        }

        
    }

    /// <summary>
    /// BG 하위의 키 버튼들을 설정합니다
    /// </summary>
    private void SetupKeyButtons()
    {
        // BG 오브젝트 찾기
        Transform bgTransform = m_keyboardPrefab.transform.Find("BG");
        if (bgTransform == null)
        {
            
            return;
        }

        // BG 하위의 모든 버튼 컴포넌트를 찾아서 이벤트 연결
        Button[] keyButtons = bgTransform.GetComponentsInChildren<Button>();
        int setupCount = 0;

        foreach (Button keyButton in keyButtons)
        {
            string keyName = keyButton.name;

            // 버튼 클릭 이벤트 연결
            keyButton.onClick.AddListener(() =>
            {
                
                OnKeyPressed(keyName);
                if (m_enableButtonPressEffect)
                {
                    PlayButtonPressAnimation(keyButton.transform);
                }
            });

            setupCount++;
            
        }

        
    }

    /// <summary>
    /// 키보드 프리팹에 패키지 리소스를 자동으로 적용합니다
    /// </summary>
    private void ApplyKeyboardResources()
    {
        if (m_keyboardPrefab == null)
        {
            
            return;
        }

        

        // 키 스프라이트 적용
        if (m_applyKeySprite)
        {
            ApplyKeySprites();
        }

        // 폰트 적용
        if (m_applyFont)
        {
            ApplyKeyboardFont();
        }

        
    }

    /// <summary>
    /// 키 버튼들에 스프라이트를 적용합니다
    /// </summary>
    private void ApplyKeySprites()
    {
        // Resources 폴더에서 키 스프라이트 로드
        Sprite keySprite = Resources.Load<Sprite>("KeyBoardSprites/OSX_Key");

        if (keySprite == null)
        {
            
            return;
        }

        // BG 하위의 모든 Button 컴포넌트를 찾아서 KeyBG에 스프라이트 적용
        Transform bgTransform = m_keyboardPrefab.transform.Find("BG");

        bgTransform.GetComponent<Image>().sprite = keySprite;

        if (bgTransform != null)
        {
            Button[] keyButtons = bgTransform.GetComponentsInChildren<Button>();
            int appliedCount = 0;

            foreach (Button keyButton in keyButtons)
            {
                // KeyBG 오브젝트를 찾아서 스프라이트 적용
                Transform keyBGTransform = keyButton.transform.Find("KeyBG");
                if (keyBGTransform != null)
                {
                    Image keyBGImage = keyBGTransform.GetComponent<Image>();
                    if (keyBGImage != null)
                    {
                        keyBGImage.sprite = keySprite;
                        appliedCount++;
                        
                    }
                    else
                    {
                        
                    }
                }
                else
                {
                    // KeyBG가 없는 경우 첫 번째 자식에서 Image 컴포넌트 찾기 (fallback)
                    if (keyButton.transform.childCount > 0)
                    {
                        Image childImage = keyButton.transform.GetChild(0).GetComponent<Image>();
                        if (childImage != null)
                        {
                            childImage.sprite = keySprite;
                            appliedCount++;
                            
                        }
                    }
                    else
                    {
                        
                    }
                }
            }

            
        }
        else
        {
            
        }
    }

    /// <summary>
    /// 키보드 텍스트 컴포넌트들에 폰트를 적용합니다
    /// </summary>
    private void ApplyKeyboardFont()
    {
        // Resources 폴더에서 폰트 로드
        TMP_FontAsset keyboardFont = Resources.Load<TMP_FontAsset>("KeyBoardSprites/Fonts/Anton_Lowres");

        if (keyboardFont == null)
        {
            
            return;
        }

        // 키보드 전체의 TextMeshPro 컴포넌트들에 폰트 적용
        TextMeshProUGUI[] textComponents = m_keyboardPrefab.GetComponentsInChildren<TextMeshProUGUI>();
        int appliedCount = 0;

        foreach (TextMeshProUGUI textComponent in textComponents)
        {
            textComponent.font = keyboardFont;
            appliedCount++;
            
        }

        
    }

    /// <summary>
    /// 버튼 눌림 애니메이션을 재생합니다
    /// </summary>
    /// <param name="buttonTransform">애니메이션을 적용할 버튼의 Transform</param>
    private void PlayButtonPressAnimation(Transform buttonTransform)
    {
        if (buttonTransform == null || !m_enableButtonPressEffect) return;

        // 기존 애니메이션이 있다면 중단
        buttonTransform.DOKill();

        // 원래 스케일 저장
        Vector3 originalScale = buttonTransform.localScale;

        // 스케일 값 검증 (0보다 큰 값이어야 함)
        float pressScale = Mathf.Clamp(m_buttonPressScale, 0.1f, 1.0f);
        float pressDuration = Mathf.Max(0.01f, m_buttonPressDuration);

        // 버튼 눌림 애니메이션 시퀀스
        Sequence pressSequence = DOTween.Sequence();

        pressSequence
            .Append(buttonTransform.DOScale(originalScale * pressScale, pressDuration).SetEase(m_buttonPressEase)) // 작아지기
            .Append(buttonTransform.DOScale(originalScale, pressDuration).SetEase(m_buttonReleaseEase))           // 원래 크기로 돌아오기
            .OnComplete(() =>
            {
                // 애니메이션 완료 후 정확한 스케일로 설정 (부동 소수점 오차 방지)
                buttonTransform.localScale = originalScale;
            });

        
    }

    /// <summary>
    /// 일반 키 입력 처리
    /// </summary>
    /// <param name="keyValue">키 값</param>
    private void HandleNormalKey(string keyValue)
    {
        m_currentInputField.text += keyValue;
        m_currentInputField.caretPosition = m_currentInputField.text.Length;
    }

    /// <summary>
    /// 백스페이스 키 처리
    /// </summary>
    private void HandleBackspace()
    {
        if (m_currentInputField.text.Length > 0)
        {
            m_currentInputField.text = m_currentInputField.text.Substring(0, m_currentInputField.text.Length - 1);
            m_currentInputField.caretPosition = m_currentInputField.text.Length;
        }
    }

    /// <summary>
    /// 스페이스 키 처리
    /// </summary>
    private void HandleSpace()
    {
        m_currentInputField.text += " ";
        m_currentInputField.caretPosition = m_currentInputField.text.Length;
    }

    /// <summary>
    /// 엔터 키 처리
    /// </summary>
    private void HandleEnter()
    {
        // InputField의 onSubmit 이벤트 호출
        m_currentInputField.onSubmit?.Invoke(m_currentInputField.text);
        HideKeyboard();
    }

    /// <summary>
    /// 클리어 키 처리
    /// </summary>
    private void HandleClear()
    {
        m_currentInputField.text = "";
        m_currentInputField.caretPosition = 0;
    }

    /// <summary>
    /// 마침표 키 처리
    /// </summary>
    private void HandlePeriod()
    {
        m_currentInputField.text += ".";
        m_currentInputField.caretPosition = m_currentInputField.text.Length;
    }

    /// <summary>
    /// 수동으로 할당된 InputField들을 등록
    /// </summary>
    private void RegisterManualInputFields()
    {
        if (m_manualInputFields == null || m_manualInputFields.Length == 0)
        {
            
            return;
        }

        

        foreach (TMP_InputField inputField in m_manualInputFields)
        {
            if (inputField != null)
            {
                
                RegisterInputField(inputField);
            }
            else
            {
                
            }
        }

        
    }

    /// <summary>
    /// 자동으로 씬의 InputField들을 찾아서 등록
    /// </summary>
    private void AutoRegisterInputFields()
    {
        // 씬에서 InputField 컴포넌트들 찾기
        TMP_InputField[] inputFields = FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None);
        

        int addedCount = 0;
        foreach (TMP_InputField inputField in inputFields)
        {
            // 이미 등록된 InputField는 건너뛰기
            if (m_registeredInputFields.Contains(inputField))
            {
                
                continue;
            }

            // 제외할 InputField 이름들 확인
            bool exclude = false;
            foreach (string excludeName in m_excludeInputFieldNames)
            {
                if (inputField.name.Contains(excludeName))
                {
                    exclude = true;
                    break;
                }
            }

            if (!exclude)
            {
                

                // 등록
                RegisterInputField(inputField);
                addedCount++;
            }
            else
            {
                
            }
        }

        
        
    }

    /// <summary>
    /// InputField 선택 시 호출되는 이벤트 핸들러
    /// </summary>
    /// <param name="inputField">선택된 InputField</param>
    private void OnInputFieldSelected(TMP_InputField inputField)
    {
        
        ShowKeyboard(inputField);
        
    }

    /// <summary>
    /// InputField 선택 해제 시 호출되는 이벤트 핸들러
    /// </summary>
    /// <param name="inputField">선택 해제된 InputField</param>
    private void OnInputFieldDeselected(TMP_InputField inputField)
    {
        // 선택 해제 시에는 키보드를 자동으로 숨기지 않음
        
    }

    /// <summary>
    /// InputField 제출 시 호출되는 이벤트 핸들러
    /// </summary>
    /// <param name="inputField">제출된 InputField</param>
    /// <param name="text">입력된 텍스트</param>
    private void OnInputFieldSubmitted(TMP_InputField inputField, string text)
    {
        
        HideKeyboard();
        
    }

    /// <summary>
    /// 키보드 애니메이션 처리 (현재는 사용하지 않음)
    /// </summary>
    /// <param name="targetPosition">목표 위치</param>
    /// <param name="ease">이징 타입</param>
    /// <param name="onComplete">완료 콜백</param>
    private void AnimateKeyboard(Vector2 targetPosition, Ease ease, System.Action onComplete = null)
    {
        // 애니메이션 기능이 비활성화됨
        
        onComplete?.Invoke();
    }
    #endregion

    #region Editor Methods
#if UNITY_EDITOR
    [ContextMenu("키보드 표시 테스트")]
    private void TestShowKeyboard()
    {
        ShowKeyboard();
    }

    [ContextMenu("키보드 숨김 테스트")]
    private void TestHideKeyboard()
    {
        HideKeyboard();
    }

    [ContextMenu("InputField 연결 테스트")]
    private void TestInputFieldConnection()
    {
        

        if (m_registeredInputFields.Count == 0)
        {
            
            RefreshInputFields();
            return;
        }

        // 첫 번째 등록된 InputField로 테스트
        var firstInputField = m_registeredInputFields[0];
        if (firstInputField != null)
        {
            
            
            // 테스트용 키보드 연결
            ShowKeyboard(firstInputField);
            
            // 테스트 키 입력
            OnKeyPressed("TEST");
            
            
            
            HideKeyboard();
        }
    }

    [ContextMenu("키보드 상태 디버그")]
    private void DebugKeyboardState()
    {
        
        
        

        if (m_keyboardPrefab != null)
        {
            
        }
    }

    [ContextMenu("키 변환 디버그")]
    private void DebugChildKeyTransforms()
    {
        if (m_keyboardPrefab == null)
        {
            
            return;
        }

        Transform bgTransform = m_keyboardPrefab.transform.Find("BG");
        if (bgTransform == null)
        {
            
            return;
        }

        Button[] keyButtons = bgTransform.GetComponentsInChildren<Button>();
        

        foreach (Button keyButton in keyButtons)
        {
            
        }
    }

    [ContextMenu("전체 연결 상태 체크")]
    private void CheckAllConnections()
    {
        
        
        // 1. 키보드 프리팹 확인
        bool prefabOK = m_keyboardPrefab != null;
        
        
        // 2. BG 및 키 버튼 확인
        bool buttonsOK = false;
        if (prefabOK)
        {
            Transform bgTransform = m_keyboardPrefab.transform.Find("BG");
            if (bgTransform != null)
            {
                Button[] keyButtons = bgTransform.GetComponentsInChildren<Button>();
                buttonsOK = keyButtons.Length > 0;
                
            }
            else
            {
                
            }
        }
        
        // 3. 자동 검색 설정 확인
        
        
        
        // 5. 등록된 InputField 확인
        
        foreach (var inputField in m_registeredInputFields)
        {
            if (inputField != null)
            {
                
            }
            else
            {
                
            }
        }
        
        // 6. 씬의 전체 InputField 수 확인
        TMP_InputField[] allInputFields = FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None);
        
        
        // 종합 결과
        bool allOK = prefabOK && buttonsOK && (m_registeredInputFields.Count > 0);
        
        
        if (!allOK)
        {
            
        }
    }

    [ContextMenu("InputField 새로고침")]
    private void RefreshInputFieldsDebug()
    {
        RefreshInputFields();
    }

    [ContextMenu("씬의 모든 InputField 표시")]
    private void ShowAllInputFieldsInScene()
    {
        TMP_InputField[] allInputFields = FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None);
        
        
        for (int i = 0; i < allInputFields.Length; i++)
        {
            var inputField = allInputFields[i];
            bool isRegistered = m_registeredInputFields.Contains(inputField);
            bool isExcluded = false;
            
            foreach (string excludeName in m_excludeInputFieldNames)
            {
                if (inputField.name.Contains(excludeName))
                {
                    isExcluded = true;
                    break;
                }
            }
            
            string status = isRegistered ? "[등록됨]" : (isExcluded ? "[제외됨]" : "[미등록]");
            
        }
    }

    [ContextMenu("키보드 리소스 수동 적용")]
    private void ApplyKeyboardResourcesManual()
    {
        ApplyKeyboardResources();
    }

    [ContextMenu("키 스프라이트만 적용")]
    private void ApplyKeySpritesManual()
    {
        ApplyKeySprites();
    }

    [ContextMenu("키보드 폰트만 적용")]
    private void ApplyKeyboardFontManual()
    {
        ApplyKeyboardFont();
    }
#endif
    #endregion
}