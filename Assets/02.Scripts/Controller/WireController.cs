using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;

public class WireController : CtrlInteractable
{
    #region Serialized Fields
    private const int C_BLEND_SHAPE_INDEX = 0;
    private const float C_PRESSED_VALUE = 100f;
    private const float C_RELEASED_VALUE = 0f;
    private SkinnedMeshRenderer m_skinnedMeshRenderer;
    private Tween m_currentTween;
    [Header("스냅 설정")]
    [SerializeField] private float m_snapDistance = 0.1f;
    [SerializeField] private float m_rotationDistance = 0.3f;
    [SerializeField] private bool m_showSnapRange = true;
    [SerializeField] private Color m_snapRangeColor = Color.yellow;
    [SerializeField] private Color m_rotationRangeColor = Color.blue;

    [Header("페어 설정")]
    [SerializeField] private WireController m_pairWire;
    #endregion

    #region Private Fields
    private bool m_isConnected = false;
    private ConnectionPoint m_startConnectionPoint = null;
    private List<ConnectionPoint> m_allConnectionPoints = new List<ConnectionPoint>();
    private ConnectionPoint m_closestStartPoint = null;
    private CircuitManager m_circuitManager;
    private Transform m_originalParent;
    #endregion

    #region Public Properties
    public ConnectionPoint StartConnectionPoint => m_startConnectionPoint;

    // 페어 관련 프로퍼티
    public bool IsConnected => m_isConnected;
    public bool IsPairConnected => m_pairWire != null && m_pairWire.IsConnected;
    public bool IsFullWireConnected => IsConnected && IsPairConnected;
    public WireController PairWire => m_pairWire;
    #endregion

    #region Unity Lifecycle
    protected override void Start()
    {
        base.Start();
        SetupPairEvents();
        FindAllConnectionPoints();
        m_skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        m_circuitManager = FindFirstObjectByType<CircuitManager>();
        m_originalParent = transform.parent;
    }

    protected override void Update()
    {
        base.Update();

        if (m_isDragging)
        {
            CheckForConnectionPoints();
            UpdateWireOrientation();
        }
    }

    protected override void OnDestroy()
    {
        // 페어 이벤트 해제
        if (m_pairWire != null)
        {
            // 페어 이벤트 구독 해제
        }
        base.OnDestroy();
    }

    private void OnDrawGizmos()
    {
        if (!m_showSnapRange) return;

        // 스냅 범위를 시각적으로 표시 (노란색)
        Gizmos.color = m_snapRangeColor;

        Gizmos.DrawWireSphere(transform.position, m_snapDistance);


        // 회전 범위를 시각적으로 표시 (파란색)
        Gizmos.color = m_rotationRangeColor;
        Gizmos.DrawWireSphere(transform.position, m_rotationDistance);

    }
    #endregion

    #region Event Handlers
    protected override void OnDragStart()
    {
        base.OnDragStart();
        m_isDragging = true;

        AnimateBlendShape(C_PRESSED_VALUE);
        // 기존 연결 해제
        DisconnectFromAllPoints();
        m_circuitManager.ResetCircuit();

    }

    protected override void OnDragEnd()
    {
        base.OnDragEnd();
        m_isDragging = false;

        AnimateBlendShape(C_RELEASED_VALUE);

        // 연결점에 스냅 시도
        TrySnapToConnectionPoints();

    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 페어 이벤트 설정
    /// </summary>
    private void SetupPairEvents()
    {
        // 페어가 설정되어 있으면 이벤트 구독
        if (m_pairWire != null)
        {
            // 페어의 연결 상태 변경을 감지할 수 있도록 설정
        }
    }

    /// <summary>
    /// 페어 연결 상태 변경 알림
    /// </summary>
    public void OnPairStateChanged()
    {

        // 페어가 연결되면 시각적 피드백 제공
        if (IsPairConnected)
        {
            // 페어 연결 시 시각적 효과 (예: 색상 변경, 파티클 효과 등)
        }
        else
        {
            // 페어 해제 시 시각적 효과
        }
    }

    /// <summary>
    /// 페어에게 연결 알림
    /// </summary>
    private void NotifyPairConnection()
    {
        if (m_pairWire != null)
        {
            m_pairWire.OnPairStateChanged();
        }
    }

    /// <summary>
    /// 페어에게 연결 해제 알림
    /// </summary>
    private void NotifyPairDisconnection()
    {
        if (m_pairWire != null)
        {
            m_pairWire.OnPairStateChanged();
        }
    }

    /// <summary>
    /// 전선을 특정 연결점에 연결
    /// </summary>
    /// <param name="connectionPoint">연결할 연결점</param>
    /// <param name="isStartPoint">시작점인지 여부</param>
    /// <returns>연결 성공 여부</returns>
    public bool ConnectToPoint(ConnectionPoint connectionPoint)
    {
        if (connectionPoint == null)
        {
            return false;
        }

        if (m_isConnected)
        {
            return false;
        }

        if (connectionPoint.TryConnectWire(this))
        {
            m_startConnectionPoint = connectionPoint;
            m_isConnected = true;

            // 시작점 위치를 연결점으로 고정
            transform.position = connectionPoint.transform.GetChild(0).position;
            transform.rotation = connectionPoint.transform.GetChild(0).rotation;

            transform.SetParent(connectionPoint.transform.parent);
            // m_circuitManager.CheckCircuitCompletion();
            // 페어에게 연결 알림
            NotifyPairConnection();

            return true;
        }
        else
        {
            
        }


        return false;
    }

    /// <summary>
    /// 모든 연결점에서 연결 해제
    /// </summary>
    public void DisconnectFromAllPoints()
    {
        transform.SetParent(m_originalParent);

        if (m_startConnectionPoint != null)
        {
            
            m_startConnectionPoint.DisconnectWire();
            m_startConnectionPoint = null;
            m_isConnected = false;
        }

        // 페어에게 연결 해제 알림
        NotifyPairDisconnection();


        
    }

    #endregion

    #region Private Methods

    private void AnimateBlendShape(float targetValue)
    {
        // 기존 애니메이션이 실행 중이면 중지
        m_currentTween?.Kill();

        float currentValue = m_skinnedMeshRenderer.GetBlendShapeWeight(C_BLEND_SHAPE_INDEX);

        m_currentTween = DOTween.To(
            () => currentValue,
            value => m_skinnedMeshRenderer.SetBlendShapeWeight(C_BLEND_SHAPE_INDEX, value),
            targetValue,
            0.1f
        ).SetEase(Ease.InOutSine);
    }

    /// <summary>
    /// 모든 연결점 찾기
    /// </summary>
    private void FindAllConnectionPoints()
    {
        m_allConnectionPoints.Clear();
        ConnectionPoint[] connectionPoints = FindObjectsByType<ConnectionPoint>(FindObjectsSortMode.None);
        m_allConnectionPoints.AddRange(connectionPoints);
    }

    /// <summary>
    /// 연결점 감지 및 가장 가까운 연결점 찾기
    /// </summary>
    private void CheckForConnectionPoints()
    {
        if (m_allConnectionPoints.Count == 0) return;

        Vector3 startPos = transform.position;

        m_closestStartPoint = null;
        float closestStartDistance = float.MaxValue;

        foreach (ConnectionPoint connectionPoint in m_allConnectionPoints)
        {
            if (connectionPoint == null || connectionPoint.IsConnected) continue;

            // 시작점 연결 확인 (스냅 범위)
            if (!m_isConnected)
            {
                float distanceToStart = Vector3.Distance(connectionPoint.transform.position, startPos);
                if (distanceToStart <= m_snapDistance && distanceToStart < closestStartDistance)
                {
                    closestStartDistance = distanceToStart;
                    m_closestStartPoint = connectionPoint;
                }
            }
        }
    }

    /// <summary>
    /// 전선 방향 업데이트 (가장 가까운 연결점을 바라보도록)
    /// </summary>
    private void UpdateWireOrientation()
    {
        Vector3 startPos = transform.position;

        // 시작점 방향 업데이트 (회전 범위)
        if (!m_isConnected)
        {
            ConnectionPoint closestStartPointInRotationRange = FindClosestConnectionPointInRange(startPos, m_rotationDistance);
            if (closestStartPointInRotationRange != null)
            {
                Vector3 directionToStart = (closestStartPointInRotationRange.transform.position - startPos).normalized;
                directionToStart.y = 0; // Y축 회전만 (수평 회전)

                if (directionToStart.magnitude > 0.01f)
                {
                    // Right가 연결점을 바라보도록 하기 위해 direction을 -90도 회전
                    Vector3 rightDirection = Quaternion.AngleAxis(-90f, Vector3.up) * directionToStart;
                    Quaternion targetRotation = Quaternion.LookRotation(rightDirection);

                    // X축을 90도로 고정하고 Y축만 회전
                    Vector3 targetEuler = targetRotation.eulerAngles;
                    targetEuler.x = 90f; // X축 90도 고정

                    // 시작점만 회전 (전체 전선이 아닌)
                    Vector3 currentStartEuler = transform.rotation.eulerAngles;
                    float newStartYRotation = Mathf.LerpAngle(currentStartEuler.y, targetEuler.y, Time.deltaTime * 5f);
                    transform.rotation = Quaternion.Euler(90f, newStartYRotation, currentStartEuler.z);
                }
            }
        }
    }

    /// <summary>
    /// 연결점에 스냅 시도
    /// </summary>
    private void TrySnapToConnectionPoints()
    {
        
        // CheckForConnectionPoints에서 찾은 가장 가까운 연결점들 사용
        if (m_closestStartPoint != null)
        {
            ConnectToPoint(m_closestStartPoint);
        }
    }

    /// <summary>
    /// 특정 범위 내에서 가장 가까운 연결점 찾기
    /// </summary>
    /// <param name="position">기준 위치</param>
    /// <param name="range">검색 범위</param>
    /// <returns>가장 가까운 연결점</returns>
    private ConnectionPoint FindClosestConnectionPointInRange(Vector3 position, float range)
    {
        if (m_allConnectionPoints.Count == 0) return null;

        ConnectionPoint closestPoint = null;
        float closestDistance = float.MaxValue;

        foreach (ConnectionPoint connectionPoint in m_allConnectionPoints)
        {
            if (connectionPoint == null || connectionPoint.IsConnected) continue;

            float distance = Vector3.Distance(connectionPoint.transform.position, position);
            if (distance <= range && distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = connectionPoint;
            }
        }

        return closestPoint;
    }

    #endregion
}