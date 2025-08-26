# Unity Virtual Keyboard System

Unity 환경에서 사용할 수 있는 가상 키보드 시스템입니다. WebGL 빌드에서 모바일 환경이나 키보드가 없는 환경에서 텍스트 입력을 위해 개발되었습니다.

## 📋 목차

- [특징](#특징)
- [요구사항](#요구사항)
- [설치 방법](#설치-방법)
- [사용 방법](#사용-방법)
- [설정 옵션](#설정-옵션)
- [API 참조](#api-참조)
- [문제 해결](#문제-해결)
- [라이선스](#라이선스)

## ✨ 특징

- **자동 InputField 감지**: 씬의 모든 TMP_InputField를 자동으로 찾아 연결
- **버튼 애니메이션**: DOTween을 사용한 부드러운 버튼 눌림 효과
- **특수 키 지원**: Backspace, Space, Enter, Clear 키 지원
- **커스터마이징**: 애니메이션 효과, 제외 목록 등 다양한 설정 옵션
- **WebGL 최적화**: 저성능 환경에서도 원활한 동작
- **디버그 기능**: 상세한 로그와 상태 확인 기능

## 🔧 요구사항

### Unity 버전
- Unity 2022.3 LTS 이상

### 필수 패키지
- TextMeshPro (com.unity.textmeshpro)
- DOTween (DOTween Pro 또는 무료 버전)

### 선택적 패키지
- Cinemachine (카메라 관리용, 선택사항)

## 📦 설치 방법

### 1. 패키지 임포트
1. `Assets/04.Prefabs/KeyBoard` 폴더 전체를 프로젝트에 복사
2. DOTween 패키지가 설치되어 있는지 확인
3. `Resources/KeyBoardSprites` 폴더의 모든 리소스가 올바르게 임포트되었는지 확인

### 2. 기본 설정
1. 매니저 GameObject 생성 또는 기존 매니저에 `KeyBoardUIManager` 스크립트 부착
2. 키보드 프리팹을 `Keyboard Prefab` 필드에 할당
3. `Auto Find Input Fields` 체크 (권장)

## 🚀 사용 방법

### 기본 사용법 (자동 모드)

```csharp
// 1. KeyBoardUIManager 컴포넌트를 GameObject에 부착
// 2. Inspector에서 설정:
//    - Keyboard Prefab: 키보드 프리팹 할당
//    - Auto Find Input Fields: ✓ 체크
//    - Show On Start: 필요시 체크

// 3. 씬에 TMP_InputField 배치
// → 자동으로 모든 InputField가 키보드와 연결됨
```

### 수동 할당 사용법

```csharp
// 1. KeyBoardUIManager 컴포넌트를 GameObject에 부착
// 2. Inspector에서 설정:
//    - Keyboard Prefab: 키보드 프리팹 할당
//    - Manual Input Fields: 원하는 InputField들을 배열에 직접 할당
//    - Auto Find Input Fields: 추가 자동 검색 원하면 ✓ 체크

// 3. Manual Input Fields에 할당된 InputField들만 키보드와 연결됨
// → 정확한 제어 가능, 불필요한 InputField 자동 연결 방지
```

### 프로그래밍 방식 사용법

```csharp
// 특정 InputField와 키보드 연결
TMP_InputField myInputField = GetComponent<TMP_InputField>();
keyboardManager.ConnectToInputField(myInputField);

// 키보드 표시/숨김
keyboardManager.ShowKeyboard();
keyboardManager.HideKeyboard();

// 특정 InputField로 키보드 표시
keyboardManager.ShowKeyboard(myInputField);

// InputField 수동 등록/해제
keyboardManager.RegisterInputField(myInputField);
keyboardManager.UnregisterInputField(myInputField);
```

### Managers 패턴 사용 (권장)

```csharp
public class Managers : MonoBehaviour
{
    private KeyBoardUIManager _keyboard;
    public static KeyBoardUIManager Keyboard { get { return Instance?._keyboard; } }
    
    // ... 다른 매니저들
}

// 사용 예시
Managers.Keyboard.ShowKeyboard(inputField);
Managers.Keyboard.OnKeyPressed("Hello");
```

## ⚙️ 설정 옵션

### Inspector 설정

#### 키보드 설정
- **Keyboard Prefab**: 사용할 키보드 프리팹
- **Canvas Parent**: 키보드가 생성될 부모 캔버스 (선택사항)
- **Show On Start**: 게임 시작 시 키보드 표시 여부

#### InputField 설정
- **Manual Input Fields**: Inspector에서 수동으로 할당할 InputField 배열
- **Auto Find Input Fields**: 자동으로 씬의 InputField 찾기
- **Exclude Input Field Names**: 제외할 InputField 이름 목록

#### 버튼 눌림 효과 설정
- **Enable Button Press Effect**: 버튼 눌림 효과 활성화
- **Button Press Scale**: 눌림 시 스케일 (0.1 ~ 1.0)
- **Button Press Duration**: 애니메이션 지속 시간
- **Button Press Ease**: 눌림 애니메이션 이징
- **Button Release Ease**: 복원 애니메이션 이징

#### 리소스 자동 적용 설정
- **Auto Apply Resources**: 시작 시 패키지 리소스 자동 적용
- **Apply Key Sprite**: 키 버튼 스프라이트 자동 적용
- **Apply Font**: TextMeshPro 폰트 자동 적용

### 런타임 설정

```csharp
// 자동 검색 기능 설정
keyboardManager.SetAutoFindInputFields(true);

// 제외할 InputField 이름 설정
keyboardManager.SetExcludeInputFieldNames("Debug", "Console", "Hidden");

// 버튼 효과 설정
keyboardManager.SetButtonPressEffectEnabled(true);
keyboardManager.SetButtonPressScale(0.85f);

// InputField 목록 새로고침
keyboardManager.RefreshInputFields();
```

## 📚 API 참조

### 주요 메서드

| 메서드 | 설명 | 매개변수 |
|--------|------|----------|
| `ShowKeyboard()` | 키보드 표시 | - |
| `ShowKeyboard(TMP_InputField)` | 특정 InputField와 연결하여 키보드 표시 | inputField |
| `HideKeyboard()` | 키보드 숨김 | - |
| `ToggleKeyboard()` | 키보드 표시/숨김 토글 | - |
| `OnKeyPressed(string)` | 키 입력 처리 | keyValue |
| `RegisterInputField(TMP_InputField)` | InputField 등록 | inputField |
| `UnregisterInputField(TMP_InputField)` | InputField 등록 해제 | inputField |
| `ConnectToInputField(TMP_InputField)` | 특정 InputField에 직접 연결 | inputField |
| `RefreshInputFields()` | InputField 목록 새로고침 | - |

### 프로퍼티

| 프로퍼티 | 타입 | 설명 |
|----------|------|------|
| `IsKeyboardVisible` | bool | 키보드 표시 상태 |
| `CurrentInputField` | TMP_InputField | 현재 연결된 InputField |
| `RegisteredInputFields` | IReadOnlyList<TMP_InputField> | 등록된 InputField 목록 |

### 지원하는 특수 키

- `backspace`: 백스페이스 (마지막 문자 삭제)
- `space`: 스페이스바 (공백 입력)
- `enter`: 엔터 (입력 완료 및 키보드 숨김)
- `clear`: 클리어 (전체 텍스트 삭제)
- `period`: 마침표 (.)입력

## 🔍 문제 해결

### 자주 발생하는 문제

#### 1. 키보드가 표시되지 않음
```csharp
// 해결 방법:
// 1. 키보드 프리팹이 할당되었는지 확인
// 2. 콘솔에서 초기화 로그 확인
keyboardManager.DebugKeyboardState(); // Context Menu에서 실행
```

#### 2. InputField에 텍스트가 입력되지 않음
```csharp
// 해결 방법:
// 1. InputField가 등록되었는지 확인
keyboardManager.ShowAllInputFieldsInScene(); // Context Menu에서 실행

// 2. 수동으로 InputField 연결
keyboardManager.ConnectToInputField(myInputField);

// 3. InputField 목록 새로고침
keyboardManager.RefreshInputFields();
```

#### 3. 특정 InputField를 제외하고 싶음
```csharp
// Inspector에서 "Exclude Input Field Names" 배열에 추가
// 또는 코드로 설정:
keyboardManager.SetExcludeInputFieldNames("Debug", "Console");
```

### 디버그 기능

Unity Editor에서 KeyBoardUIManager 컴포넌트를 우클릭하면 다음 메뉴들을 사용할 수 있습니다:

- **전체 연결 상태 체크**: 모든 설정과 연결 상태 확인
- **씬의 모든 InputField 표시**: 등록/제외/미등록 상태 표시
- **InputField 새로고침**: 수동으로 InputField 목록 갱신
- **InputField 연결 테스트**: 실제 키 입력 테스트
- **키보드 상태 디버그**: 현재 키보드 상태 확인
- **키보드 리소스 수동 적용**: 패키지 리소스를 수동으로 적용
- **키 스프라이트만 적용**: 키 버튼 스프라이트만 적용
- **키보드 폰트만 적용**: TextMeshPro 폰트만 적용

### 로그 메시지 의미

| 로그 메시지 | 의미 | 대응 방법 |
|-------------|------|-----------|
| "키보드가 성공적으로 초기화되었습니다" | 정상 초기화 완료 | - |
| "등록된 InputField가 없습니다" | 자동 검색 실패 | RefreshInputFields() 실행 |
| "연결된 InputField가 없습니다" | 키 입력 시 연결된 InputField 없음 | ShowKeyboard(inputField) 호출 |
| "BG 오브젝트를 찾을 수 없습니다" | 키보드 프리팹 구조 문제 | 프리팹에 'BG' 오브젝트 확인 |

## 📄 키보드 프리팹 구조

키보드 프리팹은 다음과 같은 구조를 가져야 합니다:

```
KeyBoard (GameObject)
├── Canvas (Canvas 컴포넌트)
└── BG (GameObject)
    ├── Key_A (Button 컴포넌트)
    ├── Key_B (Button 컴포넌트)
    ├── Key_Backspace (Button 컴포넌트)
    ├── Key_Space (Button 컴포넌트)
    └── ... (기타 키 버튼들)
```

### 키 버튼 명명 규칙

버튼의 이름이 키 값으로 사용됩니다:
- 일반 키: 버튼 이름 그대로 입력 (예: "A", "1", "!")
- 특수 키: 정해진 이름 사용 ("backspace", "space", "enter", "clear", "period")

## 🎨 리소스 구성

### Sprites 폴더
키보드 프리팹에서 사용하는 모든 UI 리소스가 포함되어 있습니다:

#### 이미지 파일
- **OSX_Key.png**: 키 버튼의 기본 이미지 (15KB)
- **SquareSprite.png**: 기본 정사각형 스프라이트 (440B)
- **OSK_chevron.png**: 화살표/방향 아이콘 (3.8KB)
- **OSK_cursor.png**: 텍스트 커서 이미지 (297B)

#### 폰트 리소스 (Fonts 폴더)
- **Anton_Lowres.asset**: TextMeshPro 폰트 에셋 (228KB)
- **Anton OFL.txt**: Anton 폰트 라이선스 파일 (Open Font License)

#### 머티리얼 (Materials 폴더)
- **OSX_Key_BK.mat**: 키 버튼 배경 머티리얼

### 리소스 독립성
- 모든 키보드 관련 리소스가 패키지 내에 포함되어 있어 외부 의존성 없음
- viperOSK 라이브러리가 없어도 키보드 시스템 완전 동작
- 다른 프로젝트로 이식 시 추가 설정 불필요

## 🔧 커스터마이징

### 새로운 특수 키 추가

```csharp
// KeyBoardUIManager.cs의 OnKeyPressed 메서드에 추가
switch (keyValue.ToLower())
{
    case "tab":
        HandleTab();
        break;
    case "caps":
        HandleCapsLock();
        break;
    // ... 기존 케이스들
}

// 해당 핸들러 메서드 구현
private void HandleTab()
{
    m_currentInputField.text += "\t";
    m_currentInputField.caretPosition = m_currentInputField.text.Length;
}
```

### 키보드 레이아웃 변경

1. 키보드 프리팹을 수정하여 원하는 레이아웃으로 변경
2. 버튼 이름을 원하는 키 값으로 설정
3. BG 오브젝트 하위에 배치하면 자동으로 연결됨

## 📝 라이선스

이 패키지는 MIT 라이선스 하에 배포됩니다.

## 🤝 기여하기

버그 리포트나 기능 제안은 언제나 환영합니다.

### 개발 환경
- Unity 2022.3 LTS
- DOTween (필수)
- TextMeshPro (필수)

---

**만든이**: Unity 개발팀  
**버전**: 1.0.0  
**마지막 업데이트**: 2024년 12월 