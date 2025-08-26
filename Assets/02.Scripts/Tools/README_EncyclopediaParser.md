# Encyclopedia Data Parser v3.0

Unity 백과사전 시스템을 위한 JSON 데이터 파싱 도구입니다.

## 📋 개요

EncyclopediaDataParser는 JSON 형식의 백과사전 데이터를 파싱하여 메모리에 저장하고 검색할 수 있는 도구입니다. TextAsset을 통해 JSON 파일을 할당하고 파싱된 데이터를 런타임에 활용할 수 있습니다.

## 🚀 주요 기능

### 📁 데이터 파싱
- TextAsset을 통한 JSON 파일 할당
- 자동 JSON 파싱 (Start 시 실행)
- 파싱 오류 처리 및 로그

### 🔍 데이터 검색
- itemKey 기반 검색
- itemName 기반 검색 (부분/완전 일치)
- 키워드 검색 (설명, 사용법 포함)
- 무작위 데이터 선택

### 📊 데이터 관리
- 파싱된 데이터 통계 정보
- 데이터 유효성 검사
- 캐시 새로고침 기능

## 📦 설치 및 설정

### 1. 컴포넌트 추가
```csharp
// 씬에 GameObject 생성 후 EncyclopediaDataParser 컴포넌트 추가
GameObject parserObj = new GameObject("EncyclopediaDataParser");
EncyclopediaDataParser parser = parserObj.AddComponent<EncyclopediaDataParser>();
```

### 2. JSON 파일 할당
Inspector에서 JSON 파일을 TextAsset으로 할당합니다.

### 3. JSON 데이터 형식
```json
{
  "encyclopediaItems": [
    {
      "itemKey": "polar_bear",
      "itemName": "북극곰",
      "itemImageInfo": [
        {
          "itemImage": "https://example.com/polar_bear.jpg",
          "authorship": "National Geographic",
          "sourceLink": "https://example.com/source",
          "CCBYLink": "https://creativecommons.org/licenses/by/4.0/"
        }
      ],
      "itemDescription": [
        "북극곰은 북극 지역에 서식하는 대형 포유동물입니다."
      ],
      "lifeStyle": [
        "북극의 얼음 위에서 생활하며 물개 사냥을 합니다."
      ]
    }
  ]
}
```

## 🛠️ 사용 방법

### 직접 사용
```csharp
// EncyclopediaDataParser 참조 얻기
EncyclopediaDataParser parser = FindAnyObjectByType<EncyclopediaDataParser>();

// 데이터 검색
var data = parser.FindByKey("polar_bear");
if (data != null)
{
    Debug.Log($"아이템 이름: {data.itemName}");
}

// 모든 데이터 가져오기
var allData = parser.ParsedData;
Debug.Log($"총 {allData.Length}개 아이템");
```

### EncyclopediaHelper 사용
```csharp
// 키로 검색
var data = EncyclopediaHelper.FindByKey("polar_bear");

// 이름으로 검색
var results = EncyclopediaHelper.FindByName("북극곰");

// 키워드 검색
var keywordResults = EncyclopediaHelper.SearchByKeyword("북극");

// 무작위 데이터 선택
var randomData = EncyclopediaHelper.GetRandomData(3);

// 통계 정보
Debug.Log(EncyclopediaHelper.GetStatistics());
```

## 🎯 데이터 구조

### EncyclopediaData
```csharp
public class EncyclopediaData
{
    public string itemKey;              // 고유 식별자
    public string itemName;             // 아이템 이름
    public ImageData[] itemImageInfo;   // 이미지 정보 배열
    public string[] itemDescription;    // 설명 배열
    public string[] lifeStyle;          // 사용법/생활양식 배열
}
```

### ImageData
```csharp
public class ImageData
{
    public string itemImage;      // 이미지 경로 또는 URL
    public string authorship;     // 저작권 정보
    public string sourceLink;     // 소스 링크
    public string CCBYLink;       // Creative Commons 링크
}
```

## 🔧 에디터 도구

### 메뉴 항목
- `Tools/Encyclopedia/Parse JSON Data` - JSON 데이터 파싱
- `Tools/Encyclopedia/Print Statistics` - 통계 정보 출력
- `Tools/Encyclopedia/Validate All Data` - 데이터 유효성 검사
- `Tools/Encyclopedia/Print Parsed Data Info` - 파싱된 데이터 정보 출력
- `Tools/Encyclopedia/Refresh Data Parser` - 데이터 파서 새로고침

### 컨텍스트 메뉴
- `Parse JSON Data` - 할당된 JSON 파일 파싱

## 📊 API 참조

### EncyclopediaDataParser

#### 프로퍼티
- `ParsedData` - 파싱된 모든 데이터 배열
- `DataCount` - 파싱된 데이터 개수

#### 메서드
- `ParseJsonContent(string jsonContent)` - JSON 문자열 파싱
- `FindByKey(string key)` - 키로 데이터 검색
- `FindByName(string name, bool exactMatch)` - 이름으로 데이터 검색
- `GetAllKeys()` - 모든 키 목록 반환
- `ClearParsedData()` - 파싱된 데이터 초기화
- `GetStatistics()` - 통계 정보 반환

### EncyclopediaHelper

#### 검색 메서드
- `FindByKey(string key)` - 키로 데이터 검색
- `FindByKeys(params string[] keys)` - 여러 키로 데이터 검색
- `FindByName(string name, bool exactMatch)` - 이름으로 데이터 검색
- `SearchByKeyword(string keyword, bool searchInLifeStyle)` - 키워드 검색

#### 데이터 관리
- `GetAllData()` - 모든 데이터 반환
- `GetAllKeys()` - 모든 키 목록 반환
- `KeyExists(string key)` - 키 존재 여부 확인
- `GetRandomData(int count)` - 무작위 데이터 선택
- `GetStatistics()` - 통계 정보 반환
- `RefreshCache()` - 캐시 새로고침

## 🔍 예제 코드

### 기본 사용 예제
```csharp
public class EncyclopediaExample : MonoBehaviour
{
    private void Start()
    {
        // 키로 검색
        var polarBear = EncyclopediaHelper.FindByKey("polar_bear");
        if (polarBear != null)
        {
            Debug.Log($"발견: {polarBear.itemName}");
            
            // 이미지 정보 출력
            foreach (var imageInfo in polarBear.itemImageInfo)
            {
                Debug.Log($"이미지: {imageInfo.itemImage}");
                Debug.Log($"저작권: {imageInfo.authorship}");
            }
        }
    }
}
```

### 키워드 검색 예제
```csharp
public class KeywordSearchExample : MonoBehaviour
{
    public void SearchByKeyword(string keyword)
    {
        var results = EncyclopediaHelper.SearchByKeyword(keyword, true);
        
        Debug.Log($"'{keyword}' 검색 결과: {results.Length}개");
        
        foreach (var data in results)
        {
            Debug.Log($"- {data.itemName} ({data.itemKey})");
        }
    }
}
```

### 무작위 선택 예제
```csharp
public class RandomDataExample : MonoBehaviour
{
    public void ShowRandomItems(int count)
    {
        var randomData = EncyclopediaHelper.GetRandomData(count);
        
        Debug.Log($"무작위 선택된 {randomData.Length}개 아이템:");
        
        foreach (var data in randomData)
        {
            Debug.Log($"- {data.itemName}: {data.itemDescription[0]}");
        }
    }
}
```

## 📋 변경 사항

### v3.0 (현재 버전)
- 스크립터블 에셋 생성 기능 제거
- 파일 로드 기능 제거 (StreamingAssets, Resources)
- TextAsset을 통한 JSON 파일 할당 방식 도입
- 단순화된 아키텍처로 변경
- 메모리 기반 데이터 관리

### v2.0 (이전 버전)
- itemKey 필드 추가
- URL 이미지 다운로드 기능
- 향상된 검색 기능
- 캐시 시스템 최적화

### v1.0 (초기 버전)
- 기본 JSON 파싱 기능
- 스크립터블 에셋 생성
- 기본 검색 기능

## 🔧 트러블슈팅

### 자주 발생하는 문제

1. **"EncyclopediaDataParser를 찾을 수 없습니다" 오류**
   - 해결: 씬에 EncyclopediaDataParser 컴포넌트가 있는지 확인

2. **JSON 파싱 실패**
   - 해결: JSON 형식이 올바른지 확인, 특수 문자 인코딩 확인

3. **데이터가 파싱되지 않음**
   - 해결: TextAsset이 올바르게 할당되었는지 확인

### 성능 최적화 팁

1. **메모리 사용량 최적화**
   - 필요하지 않은 데이터는 ClearParsedData() 사용
   - 이미지 URL은 필요시에만 로드

2. **검색 성능 향상**
   - 자주 사용하는 데이터는 캐시 활용
   - 키워드 검색은 필요시에만 사용

## 🤝 기여

버그 리포트나 기능 제안은 이슈 트래커를 통해 제출해주세요.

## 📄 라이선스

이 프로젝트는 MIT 라이선스하에 배포됩니다. 