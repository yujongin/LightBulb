using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Profiling;

/// <summary>
/// 시스템 성능 모니터링 도구
/// FPS, CPU, GPU, RAM, VRAM 사용률 표시
/// </summary>
public class PerformanceMonitor : MonoBehaviour
{
    [Header("모니터링 설정")]
    [SerializeField] private bool m_showOnStart = true;
    [SerializeField] private KeyCode m_toggleKey = KeyCode.F1;
    [SerializeField] private float m_updateInterval = 0.5f;
    
    [Header("색상 설정")]
    [SerializeField] private Color m_goodColor = Color.green;     // 좋은 성능
    [SerializeField] private Color m_averageColor = Color.yellow; // 보통 성능
    [SerializeField] private Color m_poorColor = Color.red;       // 나쁜 성능
    [SerializeField] private Color m_textColor = Color.white;     // 일반 텍스트
    
    // UI 컴포넌트
    private GameObject m_monitorPanel;
    private TextMeshProUGUI m_performanceText;
    private bool m_isVisible = true;
    
    // 성능 데이터
    private float m_currentFPS;
    private float m_cpuUsage;
    private float m_gpuUsage;
    private long m_ramUsage;
    private long m_vramUsage;
    private float m_lastUpdateTime;
    
    // 성능 계산용 변수
    private float m_frameTime;
    private float m_lastFrameTime;
    private int m_frameCount;
    
    // CPU 사용률 계산용 변수
    private float[] m_frameTimeBuffer = new float[30]; // 30프레임 버퍼
    private int m_frameBufferIndex = 0;
    private float m_targetFrameTime = 1.0f / 60.0f; // 60fps 타겟
    
    private void Start()
    {
        CreateMonitorPanel();
        m_isVisible = m_showOnStart;
        m_monitorPanel.SetActive(m_isVisible);
        m_lastUpdateTime = Time.time;
        m_frameTime = Time.deltaTime;
    }
    
    private void Update()
    {
        // 토글 키 처리
        if (Input.GetKeyDown(m_toggleKey))
        {
            ToggleDisplay();
        }
        
        // 프레임 시간 업데이트
        m_frameTime = Time.deltaTime;
        m_frameCount++;
        
        // 성능 정보 업데이트
        if (m_isVisible && Time.time - m_lastUpdateTime >= m_updateInterval)
        {
            UpdatePerformanceData();
            UpdateDisplay();
            m_lastUpdateTime = Time.time;
        }
    }
    
    private void CreateMonitorPanel()
    {
        // Canvas 찾기
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            
            return;
        }
        
        // 모니터 패널 생성
        m_monitorPanel = new GameObject("Performance_Monitor");
        m_monitorPanel.transform.SetParent(canvas.transform, false);
        
        // RectTransform 설정
        RectTransform rectTransform = m_monitorPanel.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(10, -10);
        rectTransform.sizeDelta = new Vector2(220, 140);
        
        // 배경 추가
        Image background = m_monitorPanel.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.7f);
        
        // 성능 텍스트 생성
        GameObject textObj = new GameObject("Performance_Text");
        textObj.transform.SetParent(m_monitorPanel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8, 8);
        textRect.offsetMax = new Vector2(-8, -8);
        
        m_performanceText = textObj.AddComponent<TextMeshProUGUI>();
        m_performanceText.text = "System Performance Monitor";
        m_performanceText.fontSize = 14;
        m_performanceText.color = m_textColor;
        m_performanceText.alignment = TextAlignmentOptions.TopLeft;
        m_performanceText.textWrappingMode = TextWrappingModes.NoWrap;
    }
    
    private void UpdatePerformanceData()
    {
        // FPS 계산
        m_currentFPS = 1.0f / m_frameTime;
        
        // 프레임 시간 버퍼 업데이트
        m_frameTimeBuffer[m_frameBufferIndex] = m_frameTime;
        m_frameBufferIndex = (m_frameBufferIndex + 1) % m_frameTimeBuffer.Length;
        
        // CPU 사용률 추정 (개선된 방식)
        m_cpuUsage = EstimateCPUUsage();
        
        // GPU 사용률 추정 (렌더링 시간 기반)
        m_gpuUsage = EstimateGPUUsage();
        
        // RAM 사용량 (Unity 관련 메모리)
        m_ramUsage = GetRAMUsage();
        
        // VRAM 사용량
        m_vramUsage = GetVRAMUsage();
    }
    
    private float EstimateCPUUsage()
    {
        // 타겟 프레임율 계산
        float targetFrameRate = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60f;
        m_targetFrameTime = 1.0f / targetFrameRate;
        
        // 프레임 시간 버퍼의 평균 계산
        float avgFrameTime = 0f;
        int validSamples = 0;
        
        for (int i = 0; i < m_frameTimeBuffer.Length; i++)
        {
            if (m_frameTimeBuffer[i] > 0)
            {
                avgFrameTime += m_frameTimeBuffer[i];
                validSamples++;
            }
        }
        
        if (validSamples == 0) return 0f;
        avgFrameTime /= validSamples;
        
        // CPU 사용률 계산 (개선된 방식)
        // 1. 기본 프레임 시간 비율
        float baseUsage = (avgFrameTime / m_targetFrameTime) * 100f;
        
        // 2. 프레임 시간 변동성 고려 (스파이크 감지)
        float variance = 0f;
        for (int i = 0; i < m_frameTimeBuffer.Length; i++)
        {
            if (m_frameTimeBuffer[i] > 0)
            {
                float diff = m_frameTimeBuffer[i] - avgFrameTime;
                variance += diff * diff;
            }
        }
        variance = Mathf.Sqrt(variance / validSamples);
        float spikeMultiplier = 1.0f + (variance / m_targetFrameTime) * 0.5f;
        
        // 3. WebGL 환경 특성 고려
        float webglMultiplier = 1.2f; // WebGL은 일반적으로 더 많은 CPU 오버헤드
        
        // 4. 최종 CPU 사용률 계산
        float cpuUsage = baseUsage * spikeMultiplier * webglMultiplier;
        
        // 5. 현실적인 범위로 제한 (최소 10%, 최대 95%)
        cpuUsage = Mathf.Clamp(cpuUsage, 10f, 95f);
        
        // 6. 성능이 좋을 때는 더 낮은 값으로 조정
        if (m_currentFPS >= targetFrameRate * 0.9f)
        {
            cpuUsage = Mathf.Lerp(cpuUsage, 25f, 0.3f); // 좋은 성능일 때는 25% 주변으로 조정
        }
        
        return cpuUsage;
    }
    
    private float EstimateGPUUsage()
    {
        // GPU 사용률 추정 (실제 GPU 사용률을 얻기 어려우므로 프레임 시간과 복잡도 기반으로 추정)
        float baseUsage = Mathf.Clamp01((60f - m_currentFPS) / 60f) * 100f;
        
        // 빌드에서는 기본 추정만 사용
        return Mathf.Clamp(baseUsage, 0f, 100f);
    }
    
    private long GetRAMUsage()
    {
        // Unity에서 할당된 메모리 (바이트)
        long totalAllocated = 0;
        long totalReserved = 0;
        
        try
        {
            totalAllocated = Profiler.GetTotalAllocatedMemoryLong();
            totalReserved = Profiler.GetTotalReservedMemoryLong();
        }
        catch (System.Exception)
        {
            // WebGL이나 일부 플랫폼에서는 지원되지 않을 수 있음
            totalAllocated = System.GC.GetTotalMemory(false);
        }
        
        return totalAllocated > 0 ? totalAllocated : totalReserved;
    }
    
    private long GetVRAMUsage()
    {
        // VRAM 사용량 (바이트)
        long vramUsage = 0;
        
        try
        {
            vramUsage = Profiler.GetAllocatedMemoryForGraphicsDriver();
        }
        catch (System.Exception)
        {
            // WebGL이나 일부 플랫폼에서는 지원되지 않을 수 있음
            // 텍스처 메모리 추정
            vramUsage = EstimateVRAMUsage();
        }
        
        return vramUsage;
    }
    
    private long EstimateVRAMUsage()
    {
        // VRAM 사용량 추정 (정확하지 않지만 근사치)
        // 화면 해상도와 렌더 타겟 기반으로 추정
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        
        // 기본 프레임 버퍼 크기 (32비트 컬러 + 24비트 깊이)
        long frameBufferSize = screenWidth * screenHeight * 7; // 대략 7바이트 per pixel
        
        // 추가 렌더 타겟 및 텍스처 메모리 추정
        long estimatedTextureMemory = frameBufferSize * 2; // 대략 2배 추정
        
        return frameBufferSize + estimatedTextureMemory;
    }
    
    private void UpdateDisplay()
    {
        if (m_performanceText == null) return;
        
        // 성능 정보 텍스트 구성
        string performanceInfo = $"<color=#{ColorUtility.ToHtmlStringRGB(m_textColor)}>System Performance Monitor\n\n";
        
        // FPS 정보
        Color fpsColor = GetPerformanceColor(m_currentFPS, 50f, 30f);
        performanceInfo += $"<color=#{ColorUtility.ToHtmlStringRGB(fpsColor)}>FPS: {m_currentFPS:F0}</color>\n";
        
        // CPU 사용률
        Color cpuColor = GetPerformanceColor(100f - m_cpuUsage, 70f, 50f);
        performanceInfo += $"<color=#{ColorUtility.ToHtmlStringRGB(cpuColor)}>CPU: {m_cpuUsage:F0}%</color>\n";
        
        // GPU 사용률
        Color gpuColor = GetPerformanceColor(100f - m_gpuUsage, 70f, 50f);
        performanceInfo += $"<color=#{ColorUtility.ToHtmlStringRGB(gpuColor)}>GPU: {m_gpuUsage:F0}%</color>\n";
        
        // RAM 사용량
        string ramText = FormatBytes(m_ramUsage);
        performanceInfo += $"<color=#{ColorUtility.ToHtmlStringRGB(m_textColor)}>RAM: {ramText}</color>\n";
        
        // VRAM 사용량
        string vramText = FormatBytes(m_vramUsage);
        performanceInfo += $"<color=#{ColorUtility.ToHtmlStringRGB(m_textColor)}>VRAM: {vramText}</color>";
        
        m_performanceText.text = performanceInfo;
    }
    
    private Color GetPerformanceColor(float value, float goodThreshold, float averageThreshold)
    {
        if (value >= goodThreshold)
        {
            return m_goodColor;
        }
        else if (value >= averageThreshold)
        {
            return m_averageColor;
        }
        else
        {
            return m_poorColor;
        }
    }
    
    private string FormatBytes(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";
        else if (bytes < 1024 * 1024)
            return $"{bytes / 1024f:F1} KB";
        else if (bytes < 1024 * 1024 * 1024)
            return $"{bytes / (1024f * 1024f):F1} MB";
        else
            return $"{bytes / (1024f * 1024f * 1024f):F1} GB";
    }
    
    private void ToggleDisplay()
    {
        m_isVisible = !m_isVisible;
        if (m_monitorPanel != null)
        {
            m_monitorPanel.SetActive(m_isVisible);
        }
    }
    
    /// <summary>
    /// 성능 모니터 표시 활성화/비활성화
    /// </summary>
    public void SetVisible(bool visible)
    {
        m_isVisible = visible;
        if (m_monitorPanel != null)
        {
            m_monitorPanel.SetActive(m_isVisible);
        }
    }
    
    /// <summary>
    /// 현재 성능 정보 반환
    /// </summary>
    public PerformanceInfo GetPerformanceInfo()
    {
        return new PerformanceInfo
        {
            fps = m_currentFPS,
            cpuUsage = m_cpuUsage,
            gpuUsage = m_gpuUsage,
            ramUsage = m_ramUsage,
            vramUsage = m_vramUsage
        };
    }
    
    /// <summary>
    /// 성능 정보를 콘솔에 출력
    /// </summary>
    [ContextMenu("성능 정보 출력")]
    public void PrintPerformanceInfo()
    {
        PerformanceInfo info = GetPerformanceInfo();
        
        
        
        
        
        
        
        
        
        
        
    }
}

/// <summary>
/// 성능 정보 구조체
/// </summary>
[System.Serializable]
public struct PerformanceInfo
{
    public float fps;
    public float cpuUsage;
    public float gpuUsage;
    public long ramUsage;
    public long vramUsage;
} 