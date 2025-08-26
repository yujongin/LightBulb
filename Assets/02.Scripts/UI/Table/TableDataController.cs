using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// TableManager의 텍스트 데이터를 인스펙터에서 쉽게 관리할 수 있는 컨트롤러
/// QuizManager와 연동하여 내부 셀 클릭 시 퀴즈 시스템을 제공
/// </summary>
public class TableDataController : MonoBehaviour
{
    [Header("Table Reference")]
    [SerializeField] private TableManager m_tableManager;
    
    [Header("Table Text Data")]
    [SerializeField] private string m_cornerTopText = "용질";
    [SerializeField] private string m_cornerBottomText = "회차";
    
    [Space]
    [SerializeField] private string[] m_headerTexts = new string[]
    {
        "용질", "차가운 물", "실온 물", "따뜻한 물"
    };
    
    [Space]
    [SerializeField] private string[] m_rowLabelTexts = new string[]
    {
        "1회차", "2회차", "3회차", "4회차", "5회차"
    };
    
    [Header("Inner Cell Data (Optional)")]
    [SerializeField] private bool m_useInnerCellData = false;
    [SerializeField] private InnerCellRow[] m_innerCellData = new InnerCellRow[]
    {
        new InnerCellRow { rowData = new string[] { "", "", "" } },
        new InnerCellRow { rowData = new string[] { "", "", "" } },
        new InnerCellRow { rowData = new string[] { "", "", "" } },
        new InnerCellRow { rowData = new string[] { "", "", "" } },
        new InnerCellRow { rowData = new string[] { "", "", "" } }
    };
    
    [Header("Auto Update")]
    [SerializeField] private bool m_autoUpdateOnStart = true;
    [SerializeField] private bool m_autoUpdateOnValidate = true;

    [Header("Quiz System Integration")]
    [SerializeField] private QuizManager m_quizManager;
    [SerializeField] private bool m_enableQuizIntegration = true;
    
    [System.Serializable]
    public class InnerCellRow
    {
        public string[] rowData;
    }
    
    private void Awake()
    {
        AutoAssignReferences();
        TableManager.OnTableInitialized += OnTableInitialized;
    }

    private void Start()
    {
        AutoAssignReferences();

        // 테이블이 이미 초기화되었는지 확인
        if (m_tableManager != null && m_tableManager.IsTableInitialized())
        {
            if (m_autoUpdateOnStart)
            {
                UpdateTableData();
            }
        }

        // QuizManager 연동 설정
        if (m_enableQuizIntegration)
        {
            SetupQuizIntegration();
        }
    }

    /// <summary>
    /// TableManager와 QuizManager 자동 할당
    /// </summary>
    private void AutoAssignReferences()
    {
        // TableManager 자동 할당
        if (m_tableManager == null)
        {
            m_tableManager = FindFirstObjectByType<TableManager>();
            if (m_tableManager == null)
            {
                
            }
        }

        // QuizManager 자동 할당
        if (m_quizManager == null)
        {
            m_quizManager = FindFirstObjectByType<QuizManager>();
            if (m_quizManager == null)
            {
                
                m_enableQuizIntegration = false;
            }
            else if (!m_enableQuizIntegration)
            {
                m_enableQuizIntegration = true;
            }
        }
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        if (m_enableQuizIntegration)
        {
            TableManager.OnInnerCellClicked -= OnCellClicked;
        }
        
        // TableManager 초기화 완료 이벤트 해제
        TableManager.OnTableInitialized -= OnTableInitialized;
    }
    
    private void OnValidate()
    {
        if (m_autoUpdateOnValidate && Application.isPlaying && m_tableManager != null)
        {
            UpdateTableData();
        }
    }
    
    /// <summary>
    /// TableManager 초기화 완료 시 호출되는 메서드
    /// </summary>
    private void OnTableInitialized()
    {
        if (m_autoUpdateOnStart)
        {
            UpdateTableData();
        }
    }
    
    [ContextMenu("Update Table Data")]
    public void UpdateTableData()
    {
        if (m_tableManager == null)
        {
            
            return;
        }
        
        if (!m_tableManager.IsTableInitialized())
        {
            
            return;
        }
        
        if (!m_tableManager.IsTableValid())
        {
            
            return;
        }
        
        // 헤더와 라벨 설정
        if (m_headerTexts != null && m_headerTexts.Length > 0)
        {
            m_tableManager.SetCornerCellTexts(m_cornerTopText, m_cornerBottomText);
            m_tableManager.SetHeaderRowTexts(m_headerTexts, "");
            if (m_rowLabelTexts != null && m_rowLabelTexts.Length > 0)
            {
                m_tableManager.SetFirstColumnTexts(m_rowLabelTexts);
            }
        }
        
        // 내부 셀 데이터 설정 (옵션)
        if (m_useInnerCellData && m_innerCellData != null && m_innerCellData.Length > 0)
        {
            string[][] innerData = new string[m_innerCellData.Length][];
            for (int i = 0; i < m_innerCellData.Length; i++)
            {
                innerData[i] = m_innerCellData[i].rowData;
            }
            m_tableManager.SetInnerCellData(innerData);
        }
    }
    
    [ContextMenu("Clear All Cell Data")]
    public void ClearAllCellData()
    {
        if (m_tableManager != null)
        {
            m_tableManager.ClearAllCells();
        }
    }
    
    [ContextMenu("Reset to Default Data")]
    public void ResetToDefaultData()
    {
        m_cornerTopText = "용질";
        m_cornerBottomText = "회차";
        
        m_headerTexts = new string[] { "용질", "차가운 물", "실온 물", "따뜻한 물" };
        m_rowLabelTexts = new string[] { "1회차", "2회차", "3회차", "4회차", "5회차" };
        
        // 내부 셀 데이터 초기화
        m_innerCellData = new InnerCellRow[5];
        for (int i = 0; i < 5; i++)
        {
            m_innerCellData[i] = new InnerCellRow { rowData = new string[3] };
        }
        
        if (Application.isPlaying)
        {
            UpdateTableData();
        }
    }

    #region Quiz System Methods
    /// <summary>
    /// 특정 셀의 퀴즈를 완료 처리 (회전 애니메이션 포함)
    /// </summary>
    /// <param name="column">열 인덱스 (1부터 시작, 내부 셀)</param>
    /// <param name="row">행 인덱스 (1부터 시작, 내부 셀)</param>
    /// <param name="childImageSprite">자식 이미지에 설정할 스프라이트</param>
    /// <param name="childText">자식에 표시할 텍스트</param>
    /// <param name="textColor">텍스트 색상 (선택사항)</param>
    public void CompleteQuizForCell(int column, int row, Sprite childImageSprite = null, string childText = "", Color? textColor = null)
    {
        if (m_tableManager == null)
        {
            
            return;
        }

        Vector2Int position = new Vector2Int(column, row);
        m_tableManager.CompleteQuiz(position, childImageSprite, childText, textColor);
    }

    /// <summary>
    /// 특정 셀의 퀴즈 완료 상태 설정
    /// </summary>
    /// <param name="column">열 인덱스</param>
    /// <param name="row">행 인덱스</param>
    /// <param name="answered">퀴즈 완료 여부</param>
    public void SetCellAnswered(int column, int row, bool answered)
    {
        if (m_tableManager == null)
        {
            
            return;
        }

        Vector2Int position = new Vector2Int(column, row);
        m_tableManager.SetInnerCellAnswered(position, answered);
    }

    /// <summary>
    /// 특정 셀의 퀴즈 완료 상태 확인
    /// </summary>
    /// <param name="column">열 인덱스</param>
    /// <param name="row">행 인덱스</param>
    /// <returns>퀴즈 완료 여부</returns>
    public bool IsCellAnswered(int column, int row)
    {
        if (m_tableManager == null)
        {
            
            return false;
        }

        Vector2Int position = new Vector2Int(column, row);
        return m_tableManager.GetInnerCellAnswered(position);
    }

    /// <summary>
    /// 특정 셀의 자식 이미지 설정
    /// </summary>
    /// <param name="column">열 인덱스</param>
    /// <param name="row">행 인덱스</param>
    /// <param name="sprite">설정할 스프라이트</param>
    /// <param name="autoActivate">자동 활성화 여부</param>
    public void SetCellChildImage(int column, int row, Sprite sprite, bool autoActivate = true)
    {
        if (m_tableManager == null)
        {
            
            return;
        }

        Vector2Int position = new Vector2Int(column, row);
        m_tableManager.SetInnerCellChildImageSprite(position, sprite, autoActivate);
    }

    /// <summary>
    /// 특정 셀의 활성화 상태 설정
    /// </summary>
    /// <param name="column">열 인덱스</param>
    /// <param name="row">행 인덱스</param>
    /// <param name="enabled">활성화 여부</param>
    public void SetCellEnabled(int column, int row, bool enabled)
    {
        if (m_tableManager == null)
        {
            
            return;
        }

        Vector2Int position = new Vector2Int(column, row);
        m_tableManager.SetInnerCellEnabled(position, enabled);
    }

    #region Quiz Integration Methods
    /// <summary>
    /// QuizManager와의 연동 설정
    /// </summary>
    private void SetupQuizIntegration()
    {
        // QuizManager가 null인 경우 한 번 더 자동 탐지 시도
        if (m_quizManager == null)
        {
            m_quizManager = FindFirstObjectByType<QuizManager>();
        }

        if (m_quizManager == null)
        {
            
            m_enableQuizIntegration = false;
            return;
        }

        // TableManager 이벤트 등록
        TableManager.OnInnerCellClicked += OnCellClicked;
    }

    /// <summary>
    /// 테이블 셀 클릭 시 호출되는 메서드
    /// </summary>
    /// <param name="position">클릭된 셀 위치</param>
    private void OnCellClicked(Vector2Int position)
    {
        if (!m_enableQuizIntegration)
        {
            
            return;
        }

        if (m_quizManager == null)
        {
            
            return;
        }

        // 퀴즈 데이터 생성
        QuizData quizData = CreateSampleQuizData(position);
        
        if (quizData == null)
        {
            
            return;
        }
        
        // 퀴즈 데이터 검증 및 시작
        if (ValidateQuizData(quizData, position))
        {
            try
            {
                m_quizManager.StartQuiz(quizData, 
                    onComplete: () => OnQuizComplete(position),
                    onResult: (isCorrect) => OnQuizResult(position, isCorrect));
            }
            catch (System.Exception e)
            {
                
            }
        }
        else
        {
            
        }
    }

    /// <summary>
    /// 퀴즈 데이터 유효성 검증
    /// </summary>
    /// <param name="quizData">검증할 퀴즈 데이터</param>
    /// <param name="position">셀 위치 (로그용)</param>
    /// <returns>유효성 검증 결과</returns>
    private bool ValidateQuizData(QuizData quizData, Vector2Int position)
    {
        if (quizData == null || string.IsNullOrEmpty(quizData.title) || string.IsNullOrEmpty(quizData.question))
        {
            
            return false;
        }

        // 퀴즈 타입별 데이터 검증
        switch (quizData.quizType)
        {
            case QuizType.MultipleChoiceText:
                if (quizData.textChoices == null || quizData.textChoices.Length == 0 ||
                    quizData.correctAnswerIndex < 0 || quizData.correctAnswerIndex >= quizData.textChoices.Length)
                {
                    
                    return false;
                }
                break;

            case QuizType.MultipleChoiceImage:
                if (quizData.imageChoices == null || quizData.imageChoices.Length == 0 ||
                    quizData.correctAnswerIndex < 0 || quizData.correctAnswerIndex >= quizData.imageChoices.Length)
                {
                    
                    return false;
                }
                break;

            case QuizType.OXQuiz:
                // OX 퀴즈는 별도 검증 필요 없음
                break;

            default:
                
                return false;
        }

        return true;
    }

    /// <summary>
    /// 퀴즈 완료 시 호출되는 메서드
    /// </summary>
    /// <param name="position">퀴즈가 완료된 셀 위치</param>
    private void OnQuizComplete(Vector2Int position)
    {
        // 퀴즈 완료 후 추가 처리가 필요한 경우 여기에 구현
    }

    /// <summary>
    /// 퀴즈 결과 처리 메서드
    /// </summary>
    /// <param name="position">퀴즈 셀 위치</param>
    /// <param name="isCorrect">정답 여부</param>
    private void OnQuizResult(Vector2Int position, bool isCorrect)
    {
        if (isCorrect)
        {
            HandleCorrectAnswer(position);
        }
    }

    /// <summary>
    /// 정답 처리 시 퀴즈 타입에 따른 셀 표시 설정
    /// </summary>
    /// <param name="position">퀴즈 셀 위치</param>
    private void HandleCorrectAnswer(Vector2Int position)
    {
        // 퀴즈 타입 결정 (QuizType enum 순서에 맞게 매핑)
        int quizTypeIndex = (position.x + position.y * 2) % 3;
        QuizType quizType;
        
        switch (quizTypeIndex)
        {
            case 0:
                quizType = QuizType.MultipleChoiceText;  // 0: 텍스트 퀴즈
                break;
            case 1:
                quizType = QuizType.MultipleChoiceImage; // 1: 이미지 퀴즈
                break;
            case 2:
                quizType = QuizType.OXQuiz;              // 2: OX 퀴즈
                break;
            default:
                quizType = QuizType.MultipleChoiceText;
                break;
        }

        switch (quizType)
        {
            case QuizType.OXQuiz:
                HandleOXQuizCorrectAnswer(position);
                break;
                
            case QuizType.MultipleChoiceText:
                HandleTextQuizCorrectAnswer(position);
                break;
                
            case QuizType.MultipleChoiceImage:
                HandleImageQuizCorrectAnswer(position);
                break;
                
            default:
                // 기본 처리
                CompleteQuizForCell(position.x, position.y, null, "완료!", Color.white);
                break;
        }
    }

    /// <summary>
    /// OX 퀴즈 정답 처리 (정답에 따라 O/X 텍스트와 색상 표시)
    /// </summary>
    /// <param name="position">퀴즈 셀 위치</param>
    private void HandleOXQuizCorrectAnswer(Vector2Int position)
    {
        // OX 퀴즈 데이터 다시 생성하여 정답 확인
        QuizData oxQuizData = CreateOXQuizData(position);
        
        if (oxQuizData.isCorrectAnswer)
        {
            // 정답이 O인 경우
            Color32 oColor = new Color32(0x1C, 0x63, 0xB8, 0xFF); // 파란색 #1C63B8
            CompleteQuizForCell(position.x, position.y, null, "O", oColor);
        }
        else
        {
            // 정답이 X인 경우
            Color32 xColor = new Color32(0xC8, 0x24, 0x3E, 0xFF); // 빨간색 #C8243E
            CompleteQuizForCell(position.x, position.y, null, "X", xColor);
        }
    }

    /// <summary>
    /// 텍스트 퀴즈 정답 처리 (정답 텍스트를 셀에 표시)
    /// </summary>
    /// <param name="position">퀴즈 셀 위치</param>
    private void HandleTextQuizCorrectAnswer(Vector2Int position)
    {
        // 텍스트 퀴즈 데이터 다시 생성하여 정답 텍스트 가져오기
        QuizData textQuizData = CreateMultipleChoiceQuizData(position);
        
        if (textQuizData != null && textQuizData.textChoices != null && 
            textQuizData.correctAnswerIndex >= 0 && textQuizData.correctAnswerIndex < textQuizData.textChoices.Length)
        {
            string correctAnswerText = textQuizData.textChoices[textQuizData.correctAnswerIndex];
            Color32 defaultColor = new Color32(0xAB, 0x3E, 0x0E, 0xFF);
            CompleteQuizForCell(position.x, position.y, null, correctAnswerText, defaultColor);
        }
        else
        {
            // 정답 텍스트를 가져올 수 없는 경우 기본 처리
            Color32 defaultColor = new Color32(0xAB, 0x3E, 0x0E, 0xFF);
            CompleteQuizForCell(position.x, position.y, null, "완료!", defaultColor);
        }
    }

    /// <summary>
    /// 이미지 퀴즈 정답 처리 (정답 이미지를 셀에 표시)
    /// </summary>
    /// <param name="position">퀴즈 셀 위치</param>
    private void HandleImageQuizCorrectAnswer(Vector2Int position)
    {
        // 이미지 퀴즈 데이터 다시 생성하여 정답 이미지 가져오기
        QuizData imageQuizData = CreateImageQuizData(position);
        
        if (imageQuizData != null && imageQuizData.imageChoices != null && 
            imageQuizData.correctAnswerIndex >= 0 && imageQuizData.correctAnswerIndex < imageQuizData.imageChoices.Length)
        {
            Sprite correctImage = imageQuizData.imageChoices[imageQuizData.correctAnswerIndex];
            Color32 defaultColor = new Color32(0xAB, 0x3E, 0x0E, 0xFF);
            CompleteQuizForCell(position.x, position.y, correctImage, "", defaultColor);
        }
        else
        {
            // 이미지를 가져올 수 없는 경우 기본 처리
            Color32 defaultColor = new Color32(0xAB, 0x3E, 0x0E, 0xFF);
            CompleteQuizForCell(position.x, position.y, null, "완료!", defaultColor);
        }
    }

    /// <summary>
    /// 샘플 퀴즈 데이터 생성 (4지 텍스트, 4지 이미지, OX)
    /// QuizType enum 순서에 맞게 수정: MultipleChoiceText(0), MultipleChoiceImage(1), OXQuiz(2)
    /// </summary>
    /// <param name="position">셀 위치</param>
    /// <returns>퀴즈 데이터</returns>
    private QuizData CreateSampleQuizData(Vector2Int position)
    {
        // 3가지 퀴즈 타입을 순환하여 제공 (QuizType enum 순서에 맞게)
        int quizTypeIndex = (position.x + position.y * 2) % 3;

        switch (quizTypeIndex)
        {
            case 0:
                return CreateMultipleChoiceQuizData(position); // MultipleChoiceText
            case 1:
                return CreateImageQuizData(position);          // MultipleChoiceImage
            case 2:
                return CreateOXQuizData(position);             // OXQuiz
            default:
                return CreateMultipleChoiceQuizData(position);
        }
    }

    /// <summary>
    /// 4지선다 객관식 퀴즈 데이터 생성
    /// </summary>
    /// <param name="position">셀 위치</param>
    /// <returns>4지선다 퀴즈 데이터</returns>
    private QuizData CreateMultipleChoiceQuizData(Vector2Int position)
    {
        // 다양한 4지선다 문제 풀
        var multipleChoiceQuestions = new[]
        {
            new {
                question = "다음 중 가장 큰 숫자는?",
                choices = new string[] { "15", "8", "23", "12" },
                correctIndex = 2
            },
            new {
                question = "물의 화학식은?",
                choices = new string[] { "H2O", "CO2", "NaCl", "O2" },
                correctIndex = 0
            },
            new {
                question = "지구의 위성은?",
                choices = new string[] { "화성", "달", "태양", "금성" },
                correctIndex = 1
            },
            new {
                question = "1년은 몇 개월인가?",
                choices = new string[] { "10개월", "11개월", "12개월", "13개월" },
                correctIndex = 2
            },
            new {
                question = "다음 중 가장 작은 단위는?",
                choices = new string[] { "킬로미터", "미터", "센티미터", "밀리미터" },
                correctIndex = 3
            },
            new {
                question = "사과의 색깔은?",
                choices = new string[] { "파란색", "빨간색", "보라색", "검은색" },
                correctIndex = 1
            },
            new {
                question = "다음 중 동물이 아닌 것은?",
                choices = new string[] { "강아지", "고양이", "나무", "토끼" },
                correctIndex = 2
            },
            new {
                question = "100 + 50은?",
                choices = new string[] { "140", "150", "160", "170" },
                correctIndex = 1
            }
        };

        // 위치를 기반으로 문제 선택
        int questionIndex = (position.x * 3 + position.y * 2) % multipleChoiceQuestions.Length;
        var selectedQuestion = multipleChoiceQuestions[questionIndex];

        return new QuizData
        {
            title = $"4지선다 퀴즈 ({position.x}, {position.y})",
            question = selectedQuestion.question,
            quizType = QuizType.MultipleChoiceText,
            textChoices = selectedQuestion.choices,
            correctAnswerIndex = selectedQuestion.correctIndex
        };
    }

    /// <summary>
    /// OX 퀴즈 데이터 생성
    /// </summary>
    /// <param name="position">셀 위치</param>
    /// <returns>OX 퀴즈 데이터</returns>
    private QuizData CreateOXQuizData(Vector2Int position)
    {
        // 다양한 OX 문제 풀
        var oxQuestions = new[]
        {
            new { question = "물의 끓는점은 100도이다", answer = true },
            new { question = "지구는 평평하다", answer = false },
            new { question = "사람은 두 개의 눈을 가지고 있다", answer = true },
            new { question = "일주일은 8일이다", answer = false },
            new { question = "태양은 동쪽에서 뜬다", answer = true },
            new { question = "얼음은 물보다 가볍다", answer = true },
            new { question = "1 + 1 = 3이다", answer = false },
            new { question = "한국의 수도는 서울이다", answer = true },
            new { question = "물고기는 하늘을 날 수 있다", answer = false },
            new { question = "겨울은 여름보다 춥다", answer = true }
        };

        // 위치를 기반으로 문제 선택
        int questionIndex = (position.x * 2 + position.y * 3) % oxQuestions.Length;
        var selectedQuestion = oxQuestions[questionIndex];

        return new QuizData
        {
            title = $"OX 퀴즈 ({position.x}, {position.y})",
            question = selectedQuestion.question,
            quizType = QuizType.OXQuiz,
            isCorrectAnswer = selectedQuestion.answer
        };
    }

    /// <summary>
    /// 4지 이미지 퀴즈 데이터 생성
    /// </summary>
    /// <param name="position">셀 위치</param>
    /// <returns>4지 이미지 퀴즈 데이터</returns>
    private QuizData CreateImageQuizData(Vector2Int position)
    {
        // 이미지 퀴즈 문제 풀
        var imageQuestions = new[]
        {
            new {
                question = "다음 중 Next 버튼 이미지는?",
                correctIndex = 0,
                description = "Next_Btn vs Return_Btn vs Click1 vs BtnBG"
            },
            new {
                question = "다음 중 성공 표시 이미지는?",
                correctIndex = 1,
                description = "BtnBG vs success-mark vs Click1 vs Return_Btn"
            },
            new {
                question = "다음 중 클릭 효과 이미지는?",
                correctIndex = 2,
                description = "Return_Btn vs Next_Btn vs Click1 vs success-mark"
            },
            new {
                question = "다음 중 배경 버튼 이미지는?",
                correctIndex = 3,
                description = "success-mark vs Click1 vs Return_Btn vs BtnBG"
            }
        };

        // 위치를 기반으로 문제 선택
        int questionIndex = (position.x * 4 + position.y) % imageQuestions.Length;
        var selectedQuestion = imageQuestions[questionIndex];

        // QuizManager에서 테스트 스프라이트를 가져오기 위해 4개 스프라이트 생성 요청
        Sprite[] imageChoices = CreateTestImageChoices(4);

        if (imageChoices == null || imageChoices.Length == 0)
        {
            
            return null;
        }

        return new QuizData
        {
            title = $"이미지 퀴즈 ({position.x}, {position.y})",
            question = selectedQuestion.question,
            quizType = QuizType.MultipleChoiceImage,
            imageChoices = imageChoices,
            correctAnswerIndex = selectedQuestion.correctIndex
        };
    }

    /// <summary>
    /// 테스트용 이미지 선택지 생성
    /// </summary>
    /// <param name="count">생성할 이미지 개수</param>
    /// <returns>스프라이트 배열</returns>
    private Sprite[] CreateTestImageChoices(int count)
    {
        // QuizManager의 테스트 이미지 스프라이트 사용
        if (m_quizManager != null)
        {
            #if UNITY_EDITOR
            return LoadTestSpritesFromAssets(count);
            #else
            return CreateFallbackSprites(count);
            #endif
        }

        return CreateFallbackSprites(count);
    }

    #if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 테스트 스프라이트 로드
    /// </summary>
    /// <param name="count">로드할 스프라이트 개수</param>
    /// <returns>스프라이트 배열</returns>
    private Sprite[] LoadTestSpritesFromAssets(int count)
    {
        string[] spritePaths = {
            "Assets/06.Sprites/CommonUI/Next_Btn.png",
            "Assets/06.Sprites/CommonUI/Retry_Btn.png", 
            "Assets/06.Sprites/CommonUI/Click1.png",
            "Assets/06.Sprites/CommonUI/BtnBG.png"
        };
        
        Sprite[] sprites = new Sprite[count];
        
        for (int i = 0; i < count; i++)
        {
            if (i < spritePaths.Length)
            {
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[i]);
            }
            
            // 로드 실패 시 기본 스프라이트 사용
            if (sprites[i] == null)
            {
                sprites[i] = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            }
        }
        
        return sprites;
    }
    #endif

    /// <summary>
    /// 기본 스프라이트 생성 (빌드 환경용)
    /// </summary>
    /// <param name="count">생성할 스프라이트 개수</param>
    /// <returns>스프라이트 배열</returns>
    private Sprite[] CreateFallbackSprites(int count)
    {
        Sprite[] sprites = new Sprite[count];
        
        Sprite[] defaultSprites = {
            Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd"),
            Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd"),
            Resources.GetBuiltinResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
            Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd")
        };
        
        for (int i = 0; i < count; i++)
        {
            sprites[i] = defaultSprites[i % defaultSprites.Length];
        }
        
        return sprites;
    }

    /// <summary>
    /// JSON에서 퀴즈 데이터 로드 (향후 구현 예정)
    /// </summary>
    /// <param name="position">셀 위치</param>
    /// <returns>퀴즈 데이터</returns>
    public QuizData LoadQuizDataFromJSON(Vector2Int position)
    {
        // TODO: 웹에서 JSON 데이터를 받아와서 QuizData 객체로 변환
        return CreateSampleQuizData(position);
    }
    #endregion

    #region Test Methods (Context Menu)
    [ContextMenu("Test - Complete Quiz (1,1)")]
    public void TestCompleteQuiz()
    {
        Color32 defaultColor = new Color32(0xAB, 0x3E, 0x0E, 0xFF); // #AB3E0E
        CompleteQuizForCell(1, 1, null, "완료!", defaultColor);
    }

    [ContextMenu("Test - Disable Cell (2,2)")]
    public void TestDisableCell()
    {
        SetCellEnabled(2, 2, false);
    }

    [ContextMenu("Test - Enable Cell (2,2)")]
    public void TestEnableCell()
    {
        SetCellEnabled(2, 2, true);
    }



    [ContextMenu("Test - 설정 상태 확인")]
    public void TestCheckSetupStatus()
    {
        
        
        
    }

    [ContextMenu("Fix - QuizManager 자동 할당")]
    public void FixAssignQuizManager()
    {
        if (m_quizManager == null)
        {
            m_quizManager = FindFirstObjectByType<QuizManager>();
            
            if (m_quizManager != null)
            {
                if (!m_enableQuizIntegration)
                {
                    m_enableQuizIntegration = true;
                }
                SetupQuizIntegration();
                
            }
            else
            {
                
            }
        }
    }

    [ContextMenu("Fix - TableManager 자동 할당")]
    public void FixAssignTableManager()
    {
        if (m_tableManager == null)
        {
            m_tableManager = FindFirstObjectByType<TableManager>();
            
            if (m_tableManager != null)
            {
                
            }
            else
            {
                
            }
        }
    }
    #endregion
    #endregion
} 