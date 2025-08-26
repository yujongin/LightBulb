using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class CircuitManager : MonoBehaviour
{
    #region Serialized Fields
    [Header("회로 설정")]
    [SerializeField] private GameObject m_batteryHolder;
    [SerializeField] private GameObject m_bulbHolder;
    [SerializeField] private GameObject m_switchHolder;
    [SerializeField] private CtrlBattery m_battery;
    [SerializeField] private CtrlBulb m_bulb;
    [SerializeField] private List<WireController> m_wires = new List<WireController>();
    #endregion

    #region Private Fields
    private bool m_isCircuitComplete = false;
    private bool m_isSwitchOn = false;
    private bool m_isBulbLit = false;
    private List<ConnectionPoint> m_allConnectionPoints = new List<ConnectionPoint>();
    private CircuitPath m_currentCircuitPath;

    private bool hasSwitch = false;
    private bool isExperimentEnd = false;
    private bool isCorrectCircuit = false;
    #endregion

    #region Public Properties
    public bool IsCircuitComplete => m_isCircuitComplete;
    public bool IsSwitchOn => m_isSwitchOn;
    public bool IsBulbLit => m_isBulbLit;
    public List<WireController> ConnectedWires => m_wires.Where(w => w.IsConnected).ToList();
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializeCircuit();
    }

    #endregion

    #region Public Methods
    public void StartCheckCircuitCompletionCoroutine()
    {
        StartCoroutine(CheckCircuitCompletionCoroutine());
    }

    public void StopCheckCircuitCompletionCoroutine()
    {
        StopCoroutine(CheckCircuitCompletionCoroutine());
    }
    public IEnumerator CheckCircuitCompletionCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            CheckCircuitCompletion();
        }
    }


    /// <summary>
    /// 회로 완성도 확인
    /// </summary>
    /// <returns>회로가 완성되었는지 여부</returns>
    public bool CheckCircuitCompletion()
    {
        if (m_battery == null || m_switchHolder == null || m_bulb == null)
        {

            return false;
        }

        // 모든 전선이 연결되었는지 확인

        // 회로 경로 검증
        m_currentCircuitPath = ValidateCircuitPath();
        if (m_currentCircuitPath == null)
        {
            return false;
        }

        m_isCircuitComplete = true;

        UpdateBulbState();

        bool allWiresConnected = m_wires.All(wire => wire.IsFullWireConnected);
        if (allWiresConnected && hasSwitch && !isCorrectCircuit && !isExperimentEnd)
        {
            Managers.UI.ShowPopUpText("PushSwitch");
            isCorrectCircuit = true;
        }
        return true;
    }

    /// <summary>
    /// 스위치 상태 변경
    /// </summary>
    /// <param name="isOn">스위치 ON/OFF 상태</param>
    public void SetSwitchState(bool isOn)
    {
        // if (!hasSwitch) return;
        m_isSwitchOn = isOn;


        // 회로가 완성되었고 스위치가 ON이면 전구 점등
        UpdateBulbState();
    }

    /// <summary>
    /// 전구 상태 업데이트
    /// </summary>
    public void UpdateBulbState()
    {
        bool shouldBeLit = hasSwitch ? m_isCircuitComplete && m_isSwitchOn : m_isCircuitComplete;
        // bool shouldBeLit = m_isCircuitComplete;

        if (shouldBeLit != m_isBulbLit)
        {
            m_isBulbLit = shouldBeLit;

            if (m_bulb != null)
            {
                if (m_isBulbLit)
                {
                    TurnOnBulb();
                }
                else
                {
                    TurnOffBulb();
                }
            }
        }

        if (hasSwitch && m_isSwitchOn && m_isCircuitComplete && !isExperimentEnd)
        {
            Managers.System.UpdateCondition("ExperimentEnd", true);
            isExperimentEnd = true;
        }
    }

    /// <summary>
    /// 전구 점등
    /// </summary>
    public void TurnOnBulb()
    {
        if (m_bulb != null)
        {
            // 전구 점등 로직 호출
            m_bulb.TurnOn();

        }
    }

    /// <summary>
    /// 전구 소등
    /// </summary>
    public void TurnOffBulb()
    {
        if (m_bulb != null)
        {
            // 전구 소등 로직 호출
            m_bulb.TurnOff();

        }
    }

    /// <summary>
    /// 회로 초기화
    /// </summary>
    public void ResetCircuit()
    {
        m_isCircuitComplete = false;
        m_isBulbLit = false;
        m_currentCircuitPath = null;

        if (m_bulb != null)
        {
            TurnOffBulb();
        }
        if (!isExperimentEnd)
        {
            Managers.UI.ShowPopUpTextNoDelay("MoveAlligatorLead");
        }
        isCorrectCircuit = false;
    }

    #endregion

    #region Private Methods
    /// <summary>
    /// 회로 초기화
    /// </summary>
    private void InitializeCircuit()
    {
        // 필수 컴포넌트 찾기
        if (m_battery == null)
        {
            m_battery = FindFirstObjectByType<CtrlBattery>();
        }


        if (m_bulb == null)
        {
            m_bulb = FindFirstObjectByType<CtrlBulb>();
        }

        // 모든 전선 찾기
        if (m_wires.Count == 0)
        {
            WireController[] allWires = FindObjectsByType<WireController>(FindObjectsSortMode.None);
            m_wires.AddRange(allWires);
        }

        // 모든 연결점 찾기
        FindAllConnectionPoints();


    }

    /// <summary>
    /// 모든 연결점 찾기
    /// </summary>
    private void FindAllConnectionPoints()
    {
        m_allConnectionPoints.Clear();
        ConnectionPoint[] connectionPoints = FindObjectsByType<ConnectionPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        m_allConnectionPoints.AddRange(connectionPoints);


    }


    /// <summary>
    /// 회로 경로 검증
    /// </summary>
    /// <returns>검증된 회로 경로</returns>
    private CircuitPath ValidateCircuitPath()
    {
        // 배터리 양극에서 시작
        ConnectionPoint batteryPositive = FindConnectionPoint(m_batteryHolder, ConnectionPoint.CONNECTION_TYPE.Positive);
        if (batteryPositive == null || !batteryPositive.IsConnected)
        {

            return null;
        }

        WireController startWire = batteryPositive.ConnectedWire;
        if (startWire == null)
        {

            return null;
        }

        // 모든 가능한 경로를 시도
        CircuitPath path = FindValidCircuitPath(startWire);
        if (path != null)
        {

            return path;
        }


        return null;
    }

    /// <summary>
    /// 모든 가능한 회로 경로를 찾아서 검증
    /// </summary>
    /// <param name="startWire">시작 전선</param>
    /// <returns>검증된 회로 경로</returns>
    private CircuitPath FindValidCircuitPath(WireController startWire)
    {
        // 방문한 컴포넌트를 추적
        HashSet<GameObject> visitedComponents = new HashSet<GameObject>();

        // 배터리 홀더는 이미 방문한 것으로 간주
        visitedComponents.Add(m_batteryHolder);

        // DFS로 모든 가능한 경로 탐색
        return ExploreCircuitPath(startWire, visitedComponents);
    }

    /// <summary>
    /// DFS로 회로 경로 탐색
    /// </summary>
    /// <param name="currentWire">현재 전선</param>
    /// <param name="visitedComponents">방문한 컴포넌트들</param>
    /// <returns>검증된 회로 경로</returns>
    private CircuitPath ExploreCircuitPath(WireController currentWire, HashSet<GameObject> visitedComponents)
    {
        if (currentWire == null) return null;

        // 현재 전선이 연결된 연결점 찾기
        ConnectionPoint connectedPoint = FindConnectedPoints(currentWire.PairWire);

        GameObject parentObject = GetParentObject(connectedPoint);
        if (parentObject == null) return null;

        // 배터리 음극에 도달했는지 확인
        if (IsBatteryNegative(connectedPoint))
        {
            // 회로가 완성되었는지 확인
            if (IsCircuitPathComplete(visitedComponents))
            {

                CircuitPath path = new CircuitPath();
                path.AddConnection(connectedPoint, currentWire);
                return path;
            }
        }
        // 이미 방문한 컴포넌트는 건너뛰기
        if (visitedComponents.Contains(parentObject)) return null;


        // 현재 컴포넌트 방문 표시
        visitedComponents.Add(parentObject);

        // 다음 전선 찾기
        WireController nextWire = FindNextWire(connectedPoint);
        if (nextWire != null)
        {
            CircuitPath result = ExploreCircuitPath(nextWire, visitedComponents);
            if (result != null)
            {
                result.AddConnection(connectedPoint, currentWire);
                return result;
            }
        }

        // 백트래킹: 방문 표시 제거
        visitedComponents.Remove(parentObject);


        return null;
    }

    /// <summary>
    /// 전선이 연결된 모든 연결점 찾기
    /// </summary>
    /// <param name="wire">검색할 전선</param>
    /// <returns>연결된 연결점들</returns>
    private ConnectionPoint FindConnectedPoints(WireController wire)
    {
        ConnectionPoint connectedPoint = null;

        foreach (ConnectionPoint point in m_allConnectionPoints)
        {
            if (point.IsConnected && point.ConnectedWire == wire)
            {
                connectedPoint = point;
            }
        }

        return connectedPoint;
    }

    /// <summary>
    /// 연결점의 부모 오브젝트 찾기
    /// </summary>
    /// <param name="point">연결점</param>
    /// <returns>부모 오브젝트</returns>
    private GameObject GetParentObject(ConnectionPoint point)
    {
        if (point == null) return null;

        Transform parent = point.transform.parent;
        while (parent != null)
        {
            if (parent.GetComponent<CtrlBattery>() != null) return parent.gameObject;
            if (parent.GetComponent<CtrlSwitch>() != null) return parent.gameObject;
            if (parent.GetComponent<CtrlBulb>() != null) return parent.gameObject;
            if (parent.gameObject == m_batteryHolder) return m_batteryHolder;
            if (parent.gameObject == m_bulbHolder) return m_bulbHolder;
            if (parent.gameObject == m_switchHolder) return m_switchHolder;

            parent = parent.parent;
        }

        return null;
    }

    /// <summary>
    /// 배터리 음극인지 확인
    /// </summary>
    /// <param name="point">확인할 연결점</param>
    /// <returns>배터리 음극 여부</returns>
    private bool IsBatteryNegative(ConnectionPoint point)
    {
        if (point == null) return false;

        Transform parent = point.transform.parent;
        while (parent != null)
        {

            if (parent.gameObject == m_batteryHolder)
            {
                return point.ConnectionType == ConnectionPoint.CONNECTION_TYPE.Negative;
            }
            parent = parent.parent;
        }

        return false;
    }

    /// <summary>
    /// 회로가 완성되었는지 확인 (모든 필수 컴포넌트 방문)
    /// </summary>
    /// <param name="visitedComponents">방문한 컴포넌트들</param>
    /// <returns>회로 완성 여부</returns>
    private bool IsCircuitPathComplete(HashSet<GameObject> visitedComponents)
    {
        bool hasBattery = visitedComponents.Contains(m_batteryHolder);
        bool hasBulb = visitedComponents.Contains(m_bulbHolder);

        // 배터리와 전구는 반드시 있어야 함
        if (!hasBattery || !hasBulb) return false;

        // 스위치는 선택사항 (연결되어 있으면 방문해야 함)
        hasSwitch = visitedComponents.Contains(m_switchHolder);
        bool switchConnected = m_switchHolder != null && ValidateSwitchConnection(null) != null;

        if (switchConnected && !hasSwitch) return false;

        return true;
    }

    /// <summary>
    /// 다음 전선 찾기
    /// </summary>
    /// <param name="currentPoint">현재 연결점</param>
    /// <param name="currentWire">현재 전선</param>
    /// <returns>다음 전선</returns>
    private WireController FindNextWire(ConnectionPoint currentPoint)
    {
        if (currentPoint == null) return null;

        // 현재 컴포넌트의 다른 연결점들 찾기
        GameObject parentObject = GetParentObject(currentPoint);
        if (parentObject == null) return null;

        ConnectionPoint[] allPoints = parentObject.GetComponentsInChildren<ConnectionPoint>();

        foreach (ConnectionPoint point in allPoints)
        {
            if (point != currentPoint && point.IsConnected)
            {
                return point.ConnectedWire;
            }
        }

        return null;
    }



    /// <summary>
    /// 스위치 연결 검증 (극성 구분 없음)
    /// </summary>
    /// <returns>스위치가 올바르게 연결되었는지 여부</returns>
    private WireController ValidateSwitchConnection(WireController currentWire)
    {
        if (m_switchHolder == null) return null;

        ConnectionPoint[] switchPoints = m_switchHolder.GetComponentsInChildren<ConnectionPoint>();
        if (switchPoints.Length < 2)
        {

            return null;
        }

        // 스위치의 모든 연결점이 연결되어 있는지 확인
        bool allConnected = switchPoints.All(point => point.IsConnected);
        if (!allConnected)
        {

            return null;
        }

        for (int i = 0; i < switchPoints.Length; i++)
        {
            if (switchPoints[i].ConnectedWire == currentWire)
            {
                return switchPoints[1 - i].ConnectedWire;
            }
        }

        return null;
    }


    /// <summary>
    /// 특정 오브젝트의 연결점 찾기
    /// </summary>
    /// <param name="targetObject">대상 오브젝트</param>
    /// <param name="connectionType">연결점 타입</param>
    /// <returns>연결점</returns>
    private ConnectionPoint FindConnectionPoint(GameObject targetObject, ConnectionPoint.CONNECTION_TYPE connectionType)
    {
        if (targetObject == null) return null;

        ConnectionPoint[] connectionPoints = targetObject.GetComponentsInChildren<ConnectionPoint>();
        return connectionPoints.FirstOrDefault(cp => cp.ConnectionType == connectionType);
    }
    #endregion

    #region CircuitPath Class
    /// <summary>
    /// 회로 경로를 나타내는 클래스
    /// </summary>
    private class CircuitPath
    {
        private List<ConnectionPoint> m_connectionPoints = new List<ConnectionPoint>();
        private List<WireController> m_wires = new List<WireController>();

        public void AddConnection(ConnectionPoint connectionPoint, WireController wire)
        {
            m_connectionPoints.Add(connectionPoint);
            m_wires.Add(wire);
        }

        public bool IsComplete()
        {
            // 배터리(+) → 스위치(+) → 스위치(-) → 전구(+) → 전구(-) → 배터리(-)
            return m_connectionPoints.Count >= 6;
        }
    }
    #endregion
}