# 02.Scripts - 유니티 WebGL 과학실험 시뮬레이션 스크립트 가이드

## 📋 목차
1. [프로젝트 개요](#프로젝트-개요)
2. [폴더 구조](#폴더-구조)
3. [핵심 매니저 시스템](#핵심-매니저-시스템)
4. [컨트롤러 및 인터랙션](#컨트롤러-및-인터랙션)
5. [UI 시스템](#ui-시스템)
6. [스텝 기반 명령 시스템](#스텝-기반-명령-시스템)
7. [도구 및 유틸리티](#도구-및-유틸리티)
8. [사용 방법](#사용-방법)

---

## 🎯 프로젝트 개요

유니티 6 WebGL 환경에서 동작하는 과학실험 시뮬레이션 프로젝트의 핵심 스크립트 모음입니다.
SOLID 원칙을 기반으로 설계되었으며, WebGL 최적화와 교육적 가치를 극대화하는 구조로 구성되어 있습니다.

### 🔑 핵심 설계 원칙
- **단일 책임 원칙**: 각 매니저는 고유한 역할 담당
- **의존성 주입**: `Managers` 싱글톤을 통한 중앙집중식 관리
- **이벤트 기반 설계**: 느슨한 결합으로 확장성 확보
- **WebGL 최적화**: 메모리 효율성과 성능 최적화

---

## 📁 폴더 구조

```
02.Scripts/
├── Controller/          # 오브젝트 상호작용 제어
├── Managers/           # 시스템 핵심 매니저들
├── Scriptable/         # ScriptableObject 기반 데이터
├── StepContents/       # 단계별 실험 내용 관리
├── Test/              # 테스트 및 디버깅 도구
├── Tools/             # 성능 모니터링 및 유틸리티
└── UI/                # 사용자 인터페이스 컴포넌트
```

---

## 🎮 핵심 매니저 시스템

### `Managers.cs` - 중앙 관리 시스템
```csharp
// 매니저 접근 방법
Managers.System.SaveStep(stepIndex);
Managers.UI.ShowPopup("실험 완료!");
Managers.Camera.ChangeCamera(0);
Managers.Command.ExecuteCommand(1);
```

#### 주요 매니저 목록
- **SystemManager**: 시스템 설정 및 데이터 저장/로드
- **UIManager**: UI 인터페이스 통합 관리
- **CameraManager**: 씨네머신 카메라 전환 제어
- **CommandManager**: 단계별 명령 실행 및 관리
- **ObjectManager**: 3D 오브젝트 등록 및 관리
- **TableManager**: 테이블 데이터 처리
- **GuideManager**: 가이드 시스템 관리
- **QuizManager**: 퀴즈 시스템 관리
- **KeyBoardUIManager**: 가상 키보드 UI 관리

### `CameraManager.cs` - 카메라 제어
씨네머신을 활용한 카메라 전환 시스템
```csharp
// 카메라 전환
Managers.Camera.ChangeCamera(cameraIndex);

// 블렌드 설정
Managers.Camera.SetCameraBlend(CinemachineBlendDefinition.Styles.EaseInOut, 2.0f);
```

### `CommandManager.cs` - 명령 패턴 기반 실행 시스템
```csharp
// 명령 등록
Managers.Command.RegisterCommand(stepIndex, new YourCommand());

// 명령 실행
Managers.Command.ExecuteCommand(stepIndex);

// 빠른 재실행 (Redo)
Managers.Command.RedoCommand(stepIndex);
```

---

## 🎛️ 컨트롤러 및 인터랙션

### `CtrlInteractable.cs` - 추상 상호작용 컨트롤러
모든 상호작용 가능한 오브젝트의 기반 클래스입니다.

#### 주요 기능
- **클릭/드래그 감지**: 터치 및 마우스 입력 처리
- **드래그 제약**: X, Y, Z축 이동 제한
- **범위 제한**: 박스/원형 영역 내 드래그 제한
- **DOTween 애니메이션**: 부드러운 이동 및 복귀 애니메이션
- **방향 감지**: 드래그 방향별 이벤트 처리

#### 사용 예시
```csharp
public class MyInteractable : CtrlInteractable
{
    protected override void OnClick()
    {
        Debug.Log("오브젝트가 클릭되었습니다!");
    }
    
    protected override void OnDragStart()
    {
        // 드래그 시작 시 처리
    }
    
    protected override void OnDragEnd()
    {
        // 드래그 종료 시 처리
    }
}
```

### `CtrlCube.cs` - 기본 상호작용 구현체
`CtrlInteractable`을 상속받은 기본 구현 예시입니다.

---

## 🖥️ UI 시스템

### `AnimatedContentSizeFitter.cs` - 동적 크기 조절
컨텐츠에 따라 UI 크기를 자동으로 조절하는 애니메이션 컴포넌트입니다.

#### 주요 기능
- **자동 크기 감지**: 자식 오브젝트 크기에 따른 동적 조절
- **DOTween 애니메이션**: 부드러운 크기 변경 애니메이션
- **레이아웃 그룹 지원**: VerticalLayoutGroup, HorizontalLayoutGroup 등 지원
- **분할선 관리**: 컨텐츠 유무에 따른 분할선 표시/숨김

#### 사용 방법
```csharp
// Inspector에서 설정
// - Animation Duration: 애니메이션 지속시간
// - Fit Width/Height: 가로/세로 크기 자동 조절 여부
// - Split Line: 자동 관리할 분할선 오브젝트
```

### `ScrollViewAutoResizer.cs` - 스크롤뷰 자동 조절
스크롤뷰의 크기를 컨텐츠에 맞춰 자동으로 조절합니다.

### `LookCameraLabel.cs` - 카메라 따라보기 라벨
3D 공간에서 항상 카메라를 향하는 UI 라벨 시스템입니다.

---

## 📝 스텝 기반 명령 시스템

### `ICommand.cs` - 명령 인터페이스
```csharp
public interface ICommand
{
    void Execute();  // 일반 실행
    void Redo();     // 빠른 재실행 (애니메이션 없이)
}
```

### `ButtonDragHandler.cs` - 드래그 앤 드롭 시스템
버튼의 자식 이미지를 드래그하여 특정 오브젝트와 매칭하는 시스템입니다.

#### 주요 기능
- **홀로그램 시스템**: 드래그 대상 미리보기
- **매칭 검증**: 올바른 오브젝트 매칭 확인
- **이벤트 시스템**: 매칭 성공 시 UnityEvent 발생
- **자동 복원**: 매칭 실패 시 원위치 복귀

#### 설정 방법
```csharp
// Inspector 설정
// 1. Object Pairs: 홀로그램-실제 오브젝트 쌍 설정
// 2. Hologram Material: 홀로그램 머티리얼 할당
// 3. OnPairMatched: 매칭 성공 이벤트 설정
```

---

## 🔧 도구 및 유틸리티

### `PerformanceMonitor.cs` - 실시간 성능 모니터
WebGL 환경에서의 실시간 성능 측정 도구입니다.

#### 모니터링 항목
- **FPS**: 초당 프레임 수
- **CPU 사용률**: WebGL 환경 특화 추정
- **GPU 사용률**: 렌더링 성능 기반 추정
- **RAM 사용량**: Unity 메모리 사용량
- **VRAM 사용량**: 텍스처 메모리 사용량

#### 사용 방법
```csharp
// F1 키로 토글 또는 코드로 제어
PerformanceMonitor monitor = FindFirstObjectByType<PerformanceMonitor>();
monitor.SetVisible(true);

// 성능 정보 가져오기
PerformanceInfo info = monitor.GetPerformanceInfo();
Debug.Log($"FPS: {info.fps}, CPU: {info.cpuUsage}%");
```

### `WebDataLoader.cs` - 웹 데이터 로더
WebGL 환경에서 외부 데이터를 로드하는 유틸리티입니다.

---

## 🚀 사용 방법

### 1. 기본 설정

1. **Managers 오브젝트 생성**
   ```csharp
   // 씬에 "Managers" 오브젝트 생성
   // 각 매니저 컴포넌트를 자식으로 추가
   ```

2. **카메라 설정**
   ```csharp
   // Main Camera에 CinemachineBrain 컴포넌트 추가
   // 씬에 CinemachineCamera들 배치 후 CameraManager에 등록
   ```

### 2. 상호작용 오브젝트 생성

```csharp
// 1. CtrlInteractable 상속 클래스 생성
public class MyExperimentObject : CtrlInteractable
{
    protected override void OnClick()
    {
        // 클릭 시 동작 정의
        Managers.UI.ShowMessage("실험 기구를 선택했습니다.");
    }
}

// 2. Inspector에서 상호작용 모드 설정
// - InteractionMode: Both (클릭+드래그)
// - 드래그 제약, 범위 제한 등 설정
```

### 3. 단계별 실험 구현

```csharp
// 1. ICommand 구현
public class ExperimentStep1 : MonoBehaviour, ICommand
{
    public void Execute()
    {
        // 일반 실행 (애니메이션 포함)
        StartCoroutine(AnimateExperiment());
    }
    
    public void Redo()
    {
        // 빠른 재실행 (즉시 완료 상태로)
        SetExperimentCompleted();
    }
}

// 2. CommandManager에 등록
Managers.Command.RegisterCommand(1, experimentStep1);
```

### 4. UI 애니메이션 적용

```csharp
// AnimatedContentSizeFitter 컴포넌트 추가
// Inspector에서 설정:
// - Animation Duration: 0.3f
// - Fit Height: true
// - Split Line: 분할선 오브젝트 할당
```

### 5. 성능 모니터링

```csharp
// PerformanceMonitor 컴포넌트를 Canvas에 추가
// F1 키로 성능 정보 토글
// WebGL 빌드에서 실시간 성능 확인
```

---

## 📋 코딩 컨벤션

### 명명 규칙
```csharp
// Public 멤버: PascalCase
public class ExperimentManager { }
public void StartExperiment() { }

// Private 멤버: camelCase with m_ prefix
private float m_currentTemperature;
private bool m_isExperimentRunning;

// 상수: C_ prefix with UPPER_CASE
private const float C_BOILING_POINT = 100.0f;
```

### 아키텍처 원칙
- **단일 책임**: 각 클래스는 하나의 명확한 역할
- **의존성 주입**: Managers를 통한 느슨한 결합
- **이벤트 기반**: UnityEvent 활용한 컴포넌트 간 통신
- **WebGL 최적화**: 메모리 풀링, 즉시 해제 등

---

## 🔗 참조 문서

- [Unity 6 WebGL 최적화 가이드](https://docs.unity3d.com/Manual/webgl-optimization.html)
- [Cinemachine 카메라 시스템](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.0/manual/index.html)
- [DOTween 애니메이션](http://dotween.demigiant.com/documentation.php)
- [SOLID 원칙 적용](https://en.wikipedia.org/wiki/SOLID)

---

## 📞 지원

스크립트 사용 중 문제가 발생하거나 개선 사항이 있다면 개발팀에 문의하시기 바랍니다.

**주의사항**: 모든 코드는 WebGL 환경에 최적화되어 있으며, 에디터에서의 동작과 빌드 후 동작이 다를 수 있습니다. 