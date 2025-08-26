using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

public class SystemManager : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void NotifyReactApp(string message);
#endif

    public event Action<string, Action> OnConditionMet;

    private int step = -1;
    private int classId = -1;
    private Dictionary<string, bool> conditions = new Dictionary<string, bool>();
    private bool isRetry = false;

    public void Init()
    {
        Debug.Log("EXPERIMENTSTART");
#if UNITY_WEBGL && !UNITY_EDITOR
        NotifyReactApp("EXPERIMENTSTART");
        WebGLInput.captureAllKeyboardInput = false;
#endif
        Application.targetFrameRate = 60;
        RegisterCondition("ExperimentEnd");

    }


    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (Time.timeScale < 8f)
                Time.timeScale *= 2;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (Time.timeScale > 1f)
                Time.timeScale /= 2;
        }
        else if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            Time.timeScale = 10f;
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            Time.timeScale = 1f;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SetShadowCasting(0);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            SetShadowCasting(1);
        }
#endif

    }
    public void RegisterCondition(string conditionName)
    {
        if (!conditions.ContainsKey(conditionName))
        {
            conditions.Add(conditionName, false);
        }
    }

    public void UpdateCondition(string conditionName, bool isMet, bool isPopupActive = false, Action action = null)
    {
        if (conditions.ContainsKey(conditionName))
        {
            conditions[conditionName] = isMet;
            CheckAllConditions();

            if (isPopupActive)
            {
                Debug.Log("UpdateCondition");
                Managers.UI.ShowPopUpText(conditionName, action);
            }
        }
    }

    public bool GetCondition(string conditionName)
    {
        if (conditions.ContainsKey(conditionName))
        {
            return conditions[conditionName];
        }
        return false;
    }

    private void CheckAllConditions()
    {
        bool allConditionsMet = true;
        foreach (var condition in conditions.Values)
        {
            if (!condition)
            {
                allConditionsMet = false;
                break;
            }
        }

        if (allConditionsMet)
        {
            OnConditionMet?.Invoke("AllConditionsMet", NotifyExperimentEnd);
            if (isRetry)
                Managers.UI.SetRetryButton();
        }
    }

    public void NotifyExperimentEnd()
    {
        Debug.Log("EXPERIMENTEND");
#if UNITY_WEBGL && !UNITY_EDITOR
        NotifyReactApp("EXPERIMENTEND");
#endif
        Managers.UI.ClosePopUpText();
    }

    //Save or Load step
    public void SaveStep(int step)
    {
        if (classId == -1) return;
        Debug.Log("SaveStep!" + step);
        PlayerPrefs.SetInt(classId.ToString(), step);
        PlayerPrefs.Save();
    }
    public void SaveStep(string data)
    {
        if (classId == -1) return;
        Debug.Log("SaveStep!" + step);
        PlayerPrefs.SetString(classId.ToString(), data);
        PlayerPrefs.Save();
    }

    public void LoadStep()
    {
        if (PlayerPrefs.HasKey(classId.ToString()))
        {
            step = PlayerPrefs.GetInt(classId.ToString());
            Debug.Log("LoadStep!" + step);
            if (step >= 0)
            {
                Managers.Command.RedoCommand(step);
            }
        }
    }
    public void SetClassId(int id)
    {
        classId = id;
        LoadStep();
    }

    public void ClearSaveData(int classId)
    {
        PlayerPrefs.DeleteKey(classId.ToString());
    }

    public void AllowKeyboardInput()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = true;
#endif
    }
    public void DisallowKeyboardInput()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = false;
#endif
    }

    //encyclopedia Copyright Link
    public enum LinkType
    {
        SourceLink,
        CCBYLink
    }
    public void NotifyUrlLink(LinkType linkType, string url)
    {
        // 행성별, 이미지 인덱스별 저작권 링크 처리
        string urlPrefix = linkType == LinkType.SourceLink ? "COPYRIGHTIMG_" : "COPYRIGHTLINK_";
        // URL이 이미 전달된 경우 (ScriptableObject에서 가져온 경우)
        if (!string.IsNullOrEmpty(url))
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            NotifyReactApp(urlPrefix + url);
#endif
            Debug.Log(urlPrefix + url);
        }

    }

    /// <summary>
    /// 웹에서 JSON 테이블 데이터를 받는 함수 (자바스크립트에서 호출됨)
    /// 
    /// 자바스크립트 사용법:
    /// - 일반적인 경우: SendMessage('SystemManager', 'ReceiveJsonTableData', jsonString);
    /// - Unity Instance 사용: unityInstance.SendMessage('SystemManager', 'ReceiveJsonTableData', jsonString);
    /// 
    /// 응답 메시지:
    /// - 성공: JSON_SUCCESS_JSON 데이터 로드 완료
    /// - 실패: JSON_ERROR_[오류메시지]
    /// 
    /// JSON 형식은 WebDeveloper_API_Guide.md 참조
    /// </summary>
    /// <param name="jsonData">JSON 형태의 테이블 퀴즈 데이터</param>
    public void SetJsonTableData(string jsonData)
    {
        try
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogError("[SystemManager] JSON 데이터가 비어있습니다.");
                NotifyJsonResult(false, "JSON 데이터가 비어있습니다.");
                return;
            }

            Debug.Log($"[SystemManager] JSON 데이터 수신됨: {jsonData.Length}자");

            // WebDataLoader를 통해 JSON 데이터 처리
            var webDataLoader = FindFirstObjectByType<WebDataLoader>();
            if (webDataLoader == null)
            {
                Debug.LogError("[SystemManager] WebDataLoader를 찾을 수 없습니다.");
                NotifyJsonResult(false, "WebDataLoader를 찾을 수 없습니다.");
                return;
            }

            // 이벤트 구독으로 결과 처리
            WebDataLoader.OnDataLoaded += OnJsonDataLoaded;
            WebDataLoader.OnDataLoadError += OnJsonDataError;

            // JSON 데이터 파싱 및 적용
            webDataLoader.LoadFromJSON(jsonData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SystemManager] JSON 데이터 처리 중 예외 발생: {e.Message}");
            NotifyJsonResult(false, $"JSON 처리 예외: {e.Message}");
        }
    }

    /// <summary>
    /// JSON 데이터 로드 성공 시 호출되는 콜백
    /// </summary>
    /// <param name="data">로드된 테이블 퀴즈 데이터</param>
    private void OnJsonDataLoaded(TableQuizData data)
    {
        // 이벤트 구독 해제
        WebDataLoader.OnDataLoaded -= OnJsonDataLoaded;
        WebDataLoader.OnDataLoadError -= OnJsonDataError;

        Debug.Log("[SystemManager] JSON 데이터 처리가 성공적으로 완료되었습니다.");
        NotifyJsonResult(true, "JSON 데이터 로드 완료");
    }

    /// <summary>
    /// JSON 데이터 로드 실패 시 호출되는 콜백
    /// </summary>
    /// <param name="errorMessage">오류 메시지</param>
    private void OnJsonDataError(string errorMessage)
    {
        // 이벤트 구독 해제
        WebDataLoader.OnDataLoaded -= OnJsonDataLoaded;
        WebDataLoader.OnDataLoadError -= OnJsonDataError;

        Debug.LogError($"[SystemManager] JSON 데이터 처리 중 오류: {errorMessage}");
        NotifyJsonResult(false, $"JSON 처리 실패: {errorMessage}");
    }

    /// <summary>
    /// JSON 처리 결과를 웹으로 전달
    /// </summary>
    /// <param name="success">성공 여부</param>
    /// <param name="message">결과 메시지</param>
    private void NotifyJsonResult(bool success, string message)
    {
        string resultMessage = success ? $"JSON_SUCCESS_{message}" : $"JSON_ERROR_{message}";

#if UNITY_WEBGL && !UNITY_EDITOR
        NotifyReactApp(resultMessage);
#endif
        Debug.Log($"[SystemManager] 웹으로 결과 전달: {resultMessage}");
    }

    public void SetRetry(int value)
    {
        isRetry = value == 1;
    }

    public void SetFPS(int value)
    {
        FindFirstObjectByType<PerformanceMonitor>().SetVisible(value == 1);
    }

    public void SetShadowCasting(int value)
    {
        var renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        var shadowCastingMode = value == 1 ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;

        foreach (var renderer in renderers)
        {
            renderer.shadowCastingMode = shadowCastingMode;
        }
    }
}
