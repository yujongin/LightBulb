using UnityEngine;
using TMPro;

/// <summary>
/// KeyBoardUIManager 사용법을 보여주는 예제 스크립트
/// </summary>
public class KeyboardExample : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private KeyBoardUIManager m_keyboardManager;
    [SerializeField] private TMP_InputField m_inputField;
    
    [Header("테스트 버튼")]
    [SerializeField] private UnityEngine.UI.Button m_showKeyboardButton;
    [SerializeField] private UnityEngine.UI.Button m_hideKeyboardButton;
    [SerializeField] private UnityEngine.UI.Button m_toggleKeyboardButton;

    private void Start()
    {
        // 키보드 매니저 찾기 (할당되지 않은 경우)
        if (m_keyboardManager == null)
        {
            m_keyboardManager = FindFirstObjectByType<KeyBoardUIManager>();
        }

        // InputField 찾기 (할당되지 않은 경우)
        if (m_inputField == null)
        {
            m_inputField = FindFirstObjectByType<TMP_InputField>();
        }

        // 버튼 이벤트 연결
        SetupButtonEvents();

        // InputField 이벤트 연결 예시
        SetupInputFieldEvents();
    }

    private void SetupButtonEvents()
    {
        if (m_showKeyboardButton != null)
        {
            m_showKeyboardButton.onClick.AddListener(() => {
                if (m_keyboardManager != null && m_inputField != null)
                {
                    m_keyboardManager.ShowKeyboard(m_inputField);
                }
            });
        }

        if (m_hideKeyboardButton != null)
        {
            m_hideKeyboardButton.onClick.AddListener(() => {
                if (m_keyboardManager != null)
                {
                    m_keyboardManager.HideKeyboard();
                }
            });
        }

        if (m_toggleKeyboardButton != null)
        {
            m_toggleKeyboardButton.onClick.AddListener(() => {
                if (m_keyboardManager != null)
                {
                    m_keyboardManager.ToggleKeyboard();
                }
            });
        }
    }

    private void SetupInputFieldEvents()
    {
        if (m_inputField == null) return;

        // InputField 선택 시 자동으로 키보드 표시
        m_inputField.onSelect.AddListener((text) => {
            Debug.Log($"InputField 선택됨: {text}");
            if (m_keyboardManager != null)
            {
                m_keyboardManager.ShowKeyboard(m_inputField);
            }
        });

        // InputField 입력 완료 시
        m_inputField.onSubmit.AddListener((text) => {
            Debug.Log($"InputField 입력 완료: {text}");
            if (m_keyboardManager != null)
            {
                m_keyboardManager.HideKeyboard();
            }
        });

        // InputField 값 변경 시
        m_inputField.onValueChanged.AddListener((text) => {
            Debug.Log($"InputField 값 변경: {text}");
        });
    }

    // 프로그래밍 방식으로 키 입력 시뮬레이션
    [ContextMenu("키 입력 테스트")]
    private void TestKeyInput()
    {
        if (m_keyboardManager != null)
        {
            // 여러 키를 순차적으로 입력
            m_keyboardManager.OnKeyPressed("H");
            m_keyboardManager.OnKeyPressed("e");
            m_keyboardManager.OnKeyPressed("l");
            m_keyboardManager.OnKeyPressed("l");
            m_keyboardManager.OnKeyPressed("o");
            m_keyboardManager.OnKeyPressed("space");
            m_keyboardManager.OnKeyPressed("W");
            m_keyboardManager.OnKeyPressed("o");
            m_keyboardManager.OnKeyPressed("r");
            m_keyboardManager.OnKeyPressed("l");
            m_keyboardManager.OnKeyPressed("d");
        }
    }

    // 키보드 설정 변경 예시
    [ContextMenu("키보드 설정 변경")]
    private void ChangeKeyboardSettings()
    {
        if (m_keyboardManager != null)
        {
            // 버튼 효과 설정
            m_keyboardManager.SetButtonPressEffectEnabled(true);
            m_keyboardManager.SetButtonPressScale(0.8f);

            // 자동 검색 설정
            m_keyboardManager.SetAutoFindInputFields(true);

            // 제외할 InputField 설정
            m_keyboardManager.SetExcludeInputFieldNames("Debug", "Console");

            Debug.Log("키보드 설정이 변경되었습니다.");
        }
    }

    // InputField 수동 등록 예시
    public void RegisterCustomInputField(TMP_InputField inputField)
    {
        if (m_keyboardManager != null && inputField != null)
        {
            m_keyboardManager.RegisterInputField(inputField);
            Debug.Log($"InputField '{inputField.name}'가 등록되었습니다.");
        }
    }

    // InputField 등록 해제 예시
    public void UnregisterCustomInputField(TMP_InputField inputField)
    {
        if (m_keyboardManager != null && inputField != null)
        {
            m_keyboardManager.UnregisterInputField(inputField);
            Debug.Log($"InputField '{inputField.name}'가 등록 해제되었습니다.");
        }
    }

    // 키보드 상태 확인
    public void CheckKeyboardStatus()
    {
        if (m_keyboardManager != null)
        {
            Debug.Log($"키보드 표시 상태: {m_keyboardManager.IsKeyboardVisible}");
            Debug.Log($"현재 연결된 InputField: {(m_keyboardManager.CurrentInputField != null ? m_keyboardManager.CurrentInputField.name : "없음")}");
            Debug.Log($"등록된 InputField 수: {m_keyboardManager.RegisteredInputFields.Count}");
        }
    }
} 