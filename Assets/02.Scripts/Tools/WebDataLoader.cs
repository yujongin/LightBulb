using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 웹에서 JSON 데이터를 받아와서 표와 퀴즈 시스템에 적용하는 매니저
/// </summary>
public class WebDataLoader : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private TableManager m_tableManager;
    [SerializeField] private TableDataController m_tableDataController;
    [SerializeField] private QuizManager m_quizManager;

    [Header("Web Settings")]
    [SerializeField] private string m_baseUrl = "https://your-api-server.com";
    [SerializeField] private float m_requestTimeout = 30f;
    [SerializeField] private bool m_enableCaching = true;

    [Header("Debug")]
    [SerializeField] private bool m_verboseLogs = true;
    [SerializeField] private bool m_useLocalTestData = false;
    #endregion

    #region Private Fields
    private Dictionary<string, Sprite> m_imageCache = new Dictionary<string, Sprite>();
    private bool m_isLoading = false;
    #endregion

    #region Events
    public static event Action<TableQuizData> OnDataLoaded;
    public static event Action<string> OnDataLoadError;
    public static event Action<float> OnLoadProgress;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        ValidateReferences();
    }

    private void Start()
    {
        if (m_useLocalTestData)
        {
            StartCoroutine(WaitForTableAndLoadTestData());
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 웹에서 표-퀴즈 데이터를 로드합니다
    /// </summary>
    /// <param name="dataId">데이터 ID</param>
    public void LoadTableQuizData(string dataId)
    {
        if (m_isLoading)
        {
            Log("이미 데이터를 로딩 중입니다.", LogType.Warning);
            return;
        }

        StartCoroutine(LoadDataCoroutine(dataId));
    }

    /// <summary>
    /// 로컬 JSON 파일에서 데이터를 로드합니다 (테스트용)
    /// </summary>
    /// <param name="jsonString">JSON 문자열</param>
    public void LoadFromJSON(string jsonString)
    {
        StartCoroutine(ProcessJSONData(jsonString));
    }

    /// <summary>
    /// 테스트 데이터를 로드합니다
    /// </summary>
    public void LoadTestData()
    {
        Log("테스트 데이터 로딩 시작");
        TableQuizData testData = WebDataConverter.CreateSampleData();
        StartCoroutine(ApplyTableQuizData(testData));
    }

    /// <summary>
    /// 현재 로딩 중인 작업을 취소합니다
    /// </summary>
    public void CancelLoading()
    {
        if (m_isLoading)
        {
            StopAllCoroutines();
            m_isLoading = false;
            Log("데이터 로딩이 취소되었습니다.");
        }
    }

    /// <summary>
    /// 이미지 캐시를 정리합니다
    /// </summary>
    public void ClearImageCache()
    {
        foreach (var kvp in m_imageCache)
        {
            if (kvp.Value != null)
            {
                DestroyImmediate(kvp.Value);
            }
        }
        m_imageCache.Clear();
        Log("이미지 캐시가 정리되었습니다.");
    }
    #endregion

    #region Private Methods - Data Loading
    /// <summary>
    /// 테이블 초기화를 기다리고 테스트 데이터를 로드합니다
    /// </summary>
    private IEnumerator WaitForTableAndLoadTestData()
    {
        Log("테이블 초기화 대기 중...");
        
        TableManager tableManager = FindFirstObjectByType<TableManager>();
        if (tableManager == null)
        {
            Log("TableManager를 찾을 수 없습니다!", LogType.Error);
            yield break;
        }

        // 테이블 초기화 대기 (폴링 방식)
        float timeout = 10f;
        float elapsed = 0f;
        while (!tableManager.IsTableInitialized() && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (!tableManager.IsTableInitialized())
        {
            Log("테이블 초기화 타임아웃!", LogType.Error);
            yield break;
        }
        
        Log("테이블 초기화 완료 - 테스트 데이터 로딩 시작");
        LoadTestData();
    }

    private IEnumerator LoadDataCoroutine(string dataId)
    {
        m_isLoading = true;
        OnLoadProgress?.Invoke(0f);

        string url = $"{m_baseUrl}/api/table-quiz/{dataId}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = (int)m_requestTimeout;
            yield return request.SendWebRequest();
            
            OnLoadProgress?.Invoke(0.3f);

            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorMessage = $"데이터 로드 실패: {request.error}";
                Log(errorMessage, LogType.Error);
                OnDataLoadError?.Invoke(errorMessage);
                m_isLoading = false;
                yield break;
            }

            string jsonData = request.downloadHandler.text;
            yield return StartCoroutine(ProcessJSONData(jsonData));
        }

        m_isLoading = false;
    }

    private IEnumerator ProcessJSONData(string jsonString)
    {
        OnLoadProgress?.Invoke(0.4f);

        TableQuizData data = WebDataConverter.ParseFromJSON(jsonString);
        if (data == null)
        {
            string errorMessage = "JSON 파싱에 실패했습니다.";
            Log(errorMessage, LogType.Error);
            OnDataLoadError?.Invoke(errorMessage);
            yield break;
        }

        OnLoadProgress?.Invoke(0.5f);
        yield return StartCoroutine(LoadImages(data));

        OnLoadProgress?.Invoke(0.8f);
        yield return StartCoroutine(ApplyTableQuizData(data));

        OnLoadProgress?.Invoke(1.0f);
        OnDataLoaded?.Invoke(data);
    }

    private IEnumerator LoadImages(TableQuizData data)
    {
        if (data.quizData?.cellQuizzes == null) yield break;

        foreach (var cellQuiz in data.quizData.cellQuizzes)
        {
            if (cellQuiz.quizData?.imageChoiceUrls != null)
            {
                foreach (string imageUrl in cellQuiz.quizData.imageChoiceUrls)
                {
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        yield return StartCoroutine(LoadImageFromURL(imageUrl));
                    }
                }
            }

            if (cellQuiz.quizData?.answerResult != null && 
                !string.IsNullOrEmpty(cellQuiz.quizData.answerResult.resultImageUrl))
            {
                yield return StartCoroutine(LoadImageFromURL(cellQuiz.quizData.answerResult.resultImageUrl));
            }
        }
    }

    private IEnumerator LoadImageFromURL(string url)
    {
        if (m_enableCaching && m_imageCache.ContainsKey(url))
        {
            Log($"캐시에서 이미지 로드: {url}");
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            request.timeout = (int)m_requestTimeout;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                
                if (m_enableCaching)
                {
                    m_imageCache[url] = sprite;
                }
                
                Log($"이미지 로드 성공: {url}");
            }
            else
            {
                Log($"이미지 로드 실패: {url} - {request.error}", LogType.Error);
            }
        }
    }

    private Sprite GetCachedImage(string url)
    {
        return m_enableCaching && m_imageCache.TryGetValue(url, out Sprite sprite) ? sprite : null;
    }
    #endregion

    #region Private Methods - Data Application
    private IEnumerator ApplyTableQuizData(TableQuizData data)
    {
        if (data == null)
        {
            Log("적용할 데이터가 null입니다.", LogType.Error);
            yield break;
        }

        if (m_tableManager == null)
        {
            Log("TableManager를 찾을 수 없습니다!", LogType.Error);
            yield break;
        }

        if (!m_tableManager.IsTableInitialized())
        {
            Log("테이블 초기화 대기 중...");
            
            // 테이블 초기화 대기 (폴링 방식)
            float timeout = 10f;
            float elapsed = 0f;
            while (!m_tableManager.IsTableInitialized() && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            if (!m_tableManager.IsTableInitialized())
            {
                Log("테이블 초기화 타임아웃!", LogType.Error);
                yield break;
            }
        }

        // 표 크기 설정은 리팩토링된 TableManager에서 제거됨
        // 대신 로그만 출력
        if (data.tableInfo != null)
        {
            Log($"표 크기 정보: {data.tableInfo.width}x{data.tableInfo.height} (크기 조정은 수동으로 필요)");
            yield return new WaitForEndOfFrame();
        }

        // 표 콘텐츠 적용
        Log("표 콘텐츠 적용 시작");
        ApplyTableContent(data.tableContent);
        yield return new WaitForEndOfFrame();

        // 퀴즈 데이터 적용
        Log("퀴즈 데이터 적용 시작");
        ApplyQuizData(data.quizData);

        Log("표-퀴즈 데이터 적용 완료");
    }

    private void ApplyTableContent(TableContent content)
    {
        if (content == null || m_tableManager == null)
        {
            Log("TableContent 또는 TableManager가 null입니다.", LogType.Error);
            return;
        }

        // 코너 셀 설정
        if (content.cornerCell != null)
        {
            m_tableManager.SetCornerCellTexts(content.cornerCell.topText, content.cornerCell.bottomText);
        }

        // 헤더와 라벨 설정
        if (content.headerTexts != null && content.labelTexts != null)
        {
            m_tableManager.SetHeaderRowTexts(content.headerTexts);
            m_tableManager.SetFirstColumnTexts(content.labelTexts);
        }
    }

    private void ApplyQuizData(QuizDataCollection quizCollection)
    {
        if (quizCollection?.cellQuizzes == null || m_tableDataController == null)
        {
            Log("QuizDataCollection 또는 TableDataController가 null입니다.", LogType.Warning);
            return;
        }

        foreach (var cellQuiz in quizCollection.cellQuizzes)
        {
            if (cellQuiz?.quizData != null)
            {
                Vector2Int position = new Vector2Int(cellQuiz.column, cellQuiz.row);
                
                // 이미지 스프라이트 배열 생성
                Sprite[] imageSprites = null;
                if (cellQuiz.quizData.imageChoiceUrls != null && cellQuiz.quizData.imageChoiceUrls.Length > 0)
                {
                    imageSprites = new Sprite[cellQuiz.quizData.imageChoiceUrls.Length];
                    for (int i = 0; i < cellQuiz.quizData.imageChoiceUrls.Length; i++)
                    {
                        imageSprites[i] = GetCachedImage(cellQuiz.quizData.imageChoiceUrls[i]);
                    }
                }

                // WebQuizData를 QuizData로 변환
                QuizData quizData = WebDataConverter.ConvertToQuizData(cellQuiz.quizData, imageSprites);
                
                // 퀴즈 데이터 저장 로그
                Log($"퀴즈 데이터 저장: 위치 {position}, 타입 {quizData.quizType}");
            }
        }
    }
    #endregion

    #region Private Methods - Utility
    private void ValidateReferences()
    {
        if (m_tableManager == null)
        {
            m_tableManager = FindFirstObjectByType<TableManager>();
            if (m_tableManager != null)
            {
                Log($"TableManager 자동 할당: {m_tableManager.name}");
            }
        }

        if (m_tableDataController == null)
        {
            m_tableDataController = FindFirstObjectByType<TableDataController>();
            if (m_tableDataController != null)
            {
                Log($"TableDataController 자동 할당: {m_tableDataController.name}");
            }
        }

        if (m_quizManager == null)
        {
            m_quizManager = FindFirstObjectByType<QuizManager>();
            if (m_quizManager != null)
            {
                Log($"QuizManager 자동 할당: {m_quizManager.name}");
            }
        }
    }

    private void Log(string message, LogType logType = LogType.Log)
    {
        if (!m_verboseLogs) return;

        string formattedMessage = $"[WebDataLoader] {message}";
        switch (logType)
        {
            case LogType.Error:
                
                break;
            case LogType.Warning:
                
                break;
            default:
                
                break;
        }
    }
    #endregion

    #region Context Menu Methods
    [ContextMenu("Load Test Data")]
    private void TestLoadData()
    {
        LoadTestData();
    }

    [ContextMenu("Clear Image Cache")]
    private void TestClearCache()
    {
        ClearImageCache();
    }

    [ContextMenu("Generate Sample JSON")]
    private void TestGenerateSampleJSON()
    {
        TableQuizData sampleData = WebDataConverter.CreateSampleData();
        string json = WebDataConverter.ConvertToJSON(sampleData, true);
        
    }

    [ContextMenu("Validate Sample JSON")]
    private void TestValidateSampleJSON()
    {
        #if UNITY_EDITOR
        string jsonPath = "Assets/02.Scripts/Managers/SampleTableQuizData.json";
        if (System.IO.File.Exists(jsonPath))
        {
            string jsonContent = System.IO.File.ReadAllText(jsonPath);
            TableQuizData data = WebDataConverter.ParseFromJSON(jsonContent);
            
            if (data != null)
            {
                Log("✅ JSON 검증 성공!");
                Log($"표 크기: {data.tableInfo.width} x {data.tableInfo.height}");
                Log($"헤더 개수: {data.tableContent.headerTexts?.Length ?? 0}");
                Log($"라벨 개수: {data.tableContent.labelTexts?.Length ?? 0}");
                Log($"퀴즈 개수: {data.quizData.cellQuizzes?.Length ?? 0}");
            }
            else
            {
                Log("❌ JSON 파싱 실패!", LogType.Error);
            }
        }
        else
        {
            Log($"샘플 JSON 파일을 찾을 수 없습니다: {jsonPath}", LogType.Error);
        }
        #else
        Log("에디터에서만 사용 가능한 기능입니다.", LogType.Error);
        #endif
    }
    #endregion
} 