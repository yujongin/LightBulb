using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class EncyclopediaDataParser : MonoBehaviour
{
    [Header("데이터 설정")]
    [SerializeField] private TextAsset m_jsonFile;

    [Header("파싱 결과")]
    [SerializeField] private bool m_showParsedData = true;

    // 파싱된 데이터 저장
    private List<EncyclopediaData> m_parsedData = new List<EncyclopediaData>();




    [System.Serializable]
    private class EncyclopediaListData
    {
        public EncyclopediaData[] encyclopediaItems;
    }

    /// <summary>
    /// 파싱된 데이터 접근자
    /// </summary>
    public EncyclopediaData[] ParsedData => m_parsedData.ToArray();

    /// <summary>
    /// 파싱된 데이터 개수
    /// </summary>
    public int DataCount => m_parsedData.Count;

    private void Start()
    {
        if (m_jsonFile != null)
        {
            ParseJsonData();
        }
    }


    [ContextMenu("Parse JSON Data")]
    public void ParseJsonData()
    {
        if (m_jsonFile == null)
        {
            
            return;
        }

        try
        {
            string jsonContent = m_jsonFile.text;
            ParseJsonContent(jsonContent);
            
        }
        catch (Exception e)
        {
            
        }
    }


    /// <summary>
    /// JSON 내용을 파싱합니다
    /// </summary>
    /// <param name="jsonContent">JSON 문자열</param>
    public void ParseJsonContent(string jsonContent)
    {
        if (string.IsNullOrEmpty(jsonContent))
        {
            
            return;
        }

        try
        {
            EncyclopediaListData listData = JsonUtility.FromJson<EncyclopediaListData>(jsonContent);

            if (listData?.encyclopediaItems == null)
            {
                
                return;
            }

            m_parsedData.Clear();
            // 캐시는 지우지 않음 (EncyclopediaItem에서 미리 설정한 캐시 유지)

            // 다운로드 진행 상황 초기화
            m_totalImagesToDownload = 0;
            m_downloadedImagesCount = 0;

            // 먼저 총 이미지 개수 계산 (캐시에 없는 이미지만)
            foreach (var jsonItem in listData.encyclopediaItems)
            {
                if (jsonItem.itemImageData != null && !m_imageCache.ContainsKey(jsonItem.itemKey))
                {
                    foreach (var jsonImg in jsonItem.itemImageData)
                    {
                        if (!string.IsNullOrEmpty(jsonImg.itemImage))
                        {
                            m_totalImagesToDownload++;
                        }
                    }
                }
            }

            foreach (var jsonItem in listData.encyclopediaItems)
            {
                if (string.IsNullOrEmpty(jsonItem.itemKey) || string.IsNullOrEmpty(jsonItem.itemName))
                {
                    
                    continue;
                }

                var encyclopediaData = new EncyclopediaData
                {
                    itemKey = jsonItem.itemKey,
                    itemName = jsonItem.itemName,
                    itemDescription = jsonItem.itemDescription ?? new string[0],
                    lifeStyle = jsonItem.lifeStyle ?? new string[0]
                };

                // 이미지 데이터 변환 및 다운로드
                if (jsonItem.itemImageData != null)
                {
                    var imageDataList = new List<ImageData>();
                    bool needsDownload = !m_imageCache.ContainsKey(jsonItem.itemKey);

                    foreach (var jsonImg in jsonItem.itemImageData)
                    {
                        var imageData = new ImageData
                        {
                            itemImage = jsonImg.itemImage,
                            authorship = jsonImg.authorship ?? "",
                            sourceLink = jsonImg.sourceLink ?? "",
                            CCBYLink = jsonImg.CCBYLink ?? ""
                        };
                        imageDataList.Add(imageData);

                        // itemImage가 URL인 경우 다운로드 (캐시에 없을 때만)
                        if (!string.IsNullOrEmpty(jsonImg.itemImage) && needsDownload)
                        {
                            StartCoroutine(DownloadImageForParsing(jsonItem.itemKey, jsonImg.itemImage));
                        }
                    }
                    encyclopediaData.itemImageData = imageDataList.ToArray();
                }
                else
                {
                    encyclopediaData.itemImageData = new ImageData[0];
                }

                m_parsedData.Add(encyclopediaData);
            }

            

            if (m_totalImagesToDownload == 0)
            {
                
            }
        }
        catch (Exception e)
        {
            
        }
    }

    /// <summary>
    /// itemKey로 데이터를 검색합니다
    /// </summary>
    /// <param name="key">검색할 itemKey</param>
    /// <returns>찾은 데이터, 없으면 null</returns>
    public EncyclopediaData FindByKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;

        return m_parsedData.Find(data => data.itemKey == key);
    }

    /// <summary>
    /// itemName으로 데이터를 검색합니다
    /// </summary>
    /// <param name="name">검색할 itemName</param>
    /// <param name="exactMatch">정확한 일치 여부</param>
    /// <returns>찾은 데이터 배열</returns>
    public EncyclopediaData[] FindByName(string name, bool exactMatch = false)
    {
        if (string.IsNullOrEmpty(name)) return new EncyclopediaData[0];

        var results = new List<EncyclopediaData>();

        foreach (var data in m_parsedData)
        {
            if (exactMatch)
            {
                if (data.itemName == name)
                    results.Add(data);
            }
            else
            {
                if (data.itemName.Contains(name))
                    results.Add(data);
            }
        }

        return results.ToArray();
    }

    /// <summary>
    /// 모든 itemKey 목록을 가져옵니다
    /// </summary>
    /// <returns>모든 itemKey 배열</returns>
    public string[] GetAllKeys()
    {
        var keys = new List<string>();
        foreach (var data in m_parsedData)
        {
            keys.Add(data.itemKey);
        }
        return keys.ToArray();
    }

    /// <summary>
    /// 파싱된 데이터를 초기화합니다
    /// </summary>
    public void ClearParsedData()
    {
        m_parsedData.Clear();
    }

    /// <summary>
    /// 데이터 통계 정보를 반환합니다
    /// </summary>
    /// <returns>통계 정보 문자열</returns>
    public string GetStatistics()
    {
        int totalCount = m_parsedData.Count;
        int withImages = 0;
        int withDescription = 0;
        int withLifeStyle = 0;

        foreach (var data in m_parsedData)
        {
            if (data.itemImageData != null && data.itemImageData.Length > 0)
                withImages++;
            if (data.itemDescription != null && data.itemDescription.Length > 0)
                withDescription++;
            if (data.lifeStyle != null && data.lifeStyle.Length > 0)
                withLifeStyle++;
        }

        return $"총 아이템 수: {totalCount}\n" +
               $"이미지가 있는 아이템: {withImages}\n" +
               $"설명이 있는 아이템: {withDescription}\n" +
               $"사용법이 있는 아이템: {withLifeStyle}";
    }

#if UNITY_EDITOR
    /// <summary>
    /// 에디터 메뉴에서 실행할 수 있는 정적 메서드
    /// </summary>
    [MenuItem("Tools/Encyclopedia/Parse JSON Data")]
    public static void ParseJsonDataFromMenu()
    {
        var parser = FindAnyObjectByType<EncyclopediaDataParser>();
        if (parser == null)
        {
            
            return;
        }

        parser.ParseJsonData();
    }

    [MenuItem("Tools/Encyclopedia/Print Statistics")]
    public static void PrintStatisticsFromMenu()
    {
        var parser = FindAnyObjectByType<EncyclopediaDataParser>();
        if (parser == null)
        {
            
            return;
        }

        
    }
#endif


    #region Image Download Functions

    // 이미지 캐시용 Dictionary
    private Dictionary<string, List<Sprite>> m_imageCache = new Dictionary<string, List<Sprite>>();

    [Header("이미지 다운로드 설정")]
    [SerializeField] private int m_maxRetryAttempts = 3;
    [SerializeField] private float m_retryDelay = 3.0f;

    [Header("다운로드 상태 추적")]
    [SerializeField] private bool m_showDownloadProgress = true;
    private int m_totalImagesToDownload = 0;
    private int m_downloadedImagesCount = 0;

    /// <summary>
    /// 이미지 URL에서 이미지를 다운로드하고 Dictionary에 저장합니다
    /// </summary>
    /// <param name="key">저장할 키</param>
    /// <param name="imageUrl">다운로드할 이미지 URL</param>
    public void DownloadAndCacheImage(string key, string imageUrl)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(imageUrl))
        {
            
            return;
        }

        StartCoroutine(DownloadImageCoroutine(key, imageUrl, 0));
    }

    /// <summary>
    /// 파싱 중에 이미지를 다운로드합니다
    /// </summary>
    /// <param name="key">저장할 키</param>
    /// <param name="imageUrl">다운로드할 이미지 URL</param>
    private IEnumerator DownloadImageForParsing(string key, string imageUrl)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(imageUrl))
        {
            
            yield break;
        }

        yield return StartCoroutine(DownloadImageCoroutine(key, imageUrl, 0));
    }

    /// <summary>
    /// 이미지 다운로드 코루틴 (재시도 기능 포함)
    /// </summary>
    /// <param name="key">저장할 키</param>
    /// <param name="imageUrl">다운로드할 이미지 URL</param>
    /// <param name="attemptCount">현재 시도 횟수</param>
    private IEnumerator DownloadImageCoroutine(string key, string imageUrl, int attemptCount)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 텍스처 가져오기
                Texture2D texture = DownloadHandlerTexture.GetContent(request);

                // Sprite 생성
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                // Dictionary에 추가
                if (!m_imageCache.ContainsKey(key))
                {
                    m_imageCache[key] = new List<Sprite>();
                }

                m_imageCache[key].Add(sprite);

                // 진행 상황 업데이트
                m_downloadedImagesCount++;

                if (m_showDownloadProgress)
                {
                    
                }

                // 모든 이미지 다운로드 완료 체크
                if (m_downloadedImagesCount >= m_totalImagesToDownload)
                {
                    
                }
            }
            else
            {
                // 재시도 로직
                if (attemptCount < m_maxRetryAttempts)
                {
                    int nextAttempt = attemptCount + 1;
                    
                    

                    // 3초 대기 후 재시도
                    yield return new WaitForSeconds(m_retryDelay);
                    yield return StartCoroutine(DownloadImageCoroutine(key, imageUrl, nextAttempt));
                }
                else
                {
                    // 최대 재시도 횟수 도달 시 완전 실패
                    
                }
            }
        }
    }

    public void SetCachedImages(string key, Sprite[] sprites)
    {
        m_imageCache[key] = new List<Sprite>(sprites);
    }
    /// <summary>
    /// 캐시된 이미지 목록을 가져옵니다
    /// </summary>
    /// <param name="key">검색할 키</param>
    /// <returns>이미지 배열, 없으면 null</returns>
    public List<Sprite> GetCachedImages(string key)
    {
        if (m_imageCache.ContainsKey(key))
        {
            return m_imageCache[key];
        }
        return null;
    }

    /// <summary>
    /// 캐시를 초기화합니다
    /// </summary>
    public void ClearImageCache()
    {
        m_imageCache.Clear();
    }

    /// <summary>
    /// 재시도 설정을 변경합니다
    /// </summary>
    /// <param name="maxAttempts">최대 재시도 횟수</param>
    /// <param name="retryDelay">재시도 대기 시간(초)</param>
    public void SetRetrySettings(int maxAttempts, float retryDelay)
    {
        m_maxRetryAttempts = Mathf.Max(0, maxAttempts);
        m_retryDelay = Mathf.Max(0.1f, retryDelay);

        
    }

    /// <summary>
    /// 모든 이미지 다운로드가 완료되었는지 확인합니다
    /// </summary>
    /// <returns>모든 이미지 다운로드 완료 여부</returns>
    public bool IsAllImagesDownloaded()
    {
        return m_downloadedImagesCount >= m_totalImagesToDownload && m_totalImagesToDownload > 0;
    }

    /// <summary>
    /// 다운로드 진행 상황을 반환합니다
    /// </summary>
    /// <returns>다운로드 진행률 (0.0 ~ 1.0)</returns>
    public float GetDownloadProgress()
    {
        if (m_totalImagesToDownload == 0) return 1.0f;
        return (float)m_downloadedImagesCount / m_totalImagesToDownload;
    }

    /// <summary>
    /// 다운로드 상태 정보를 반환합니다
    /// </summary>
    /// <returns>다운로드 상태 문자열</returns>
    public string GetDownloadStatus()
    {
        if (m_totalImagesToDownload == 0)
        {
            return "다운로드할 이미지가 없습니다.";
        }

        return $"이미지 다운로드 진행 상황: {m_downloadedImagesCount}/{m_totalImagesToDownload} ({GetDownloadProgress() * 100:F1}%)";
    }

    #endregion
}