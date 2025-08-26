using UnityEngine;
using DG.Tweening;

public class ConnectionPoint : MonoBehaviour
{
    #region Serialized Fields
    [Header("연결점 설정")]
    [SerializeField] private CONNECTION_TYPE m_connectionType = CONNECTION_TYPE.Positive;
    [SerializeField] private float m_connectionRadius = 0.1f;
    [SerializeField] private Color m_positiveColor = Color.red;
    [SerializeField] private Color m_negativeColor = Color.blue;
    [SerializeField] private Color m_connectedColor = Color.green;
    #endregion

    #region Private Fields
    private bool m_isConnected = false;
    private WireController m_connectedWire = null;
    #endregion

    #region Public Properties
    public CONNECTION_TYPE ConnectionType => m_connectionType;
    public bool IsConnected => m_isConnected;
    public WireController ConnectedWire => m_connectedWire;
    public float ConnectionRadius => m_connectionRadius;
    #endregion

    #region Enums
    public enum CONNECTION_TYPE
    {
        Positive,   // 양극 (+)
        Negative    // 음극 (-)
    }
    
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializeConnectionPoint();
    }

    private void OnDrawGizmos()
    {
        // 에디터에서만 연결 반경을 시각적으로 표시
        Gizmos.color = m_isConnected ? m_connectedColor : GetPolarityColor();
        Gizmos.DrawWireSphere(transform.position, m_connectionRadius);
        
        // 연결 상태에 따른 추가 시각적 표시
        if (m_isConnected)
        {
            // 연결된 상태일 때는 작은 구체로 표시
            Gizmos.color = m_connectedColor;
            Gizmos.DrawSphere(transform.position, m_connectionRadius * 0.3f);
        }
        else
        {
            // 연결되지 않은 상태일 때는 극성에 따른 색상으로 작은 구체 표시
            Gizmos.color = GetPolarityColor();
            Gizmos.DrawSphere(transform.position, m_connectionRadius * 0.2f);
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 전선 연결 시도
    /// </summary>
    /// <param name="wire">연결하려는 전선</param>
    /// <returns>연결 성공 여부</returns>
    public bool TryConnectWire(WireController wire)
    {
        if (m_isConnected)
        {
            
            return false;
        }

        if (wire == null)
        {
            
            return false;
        }

        // 연결 성공
        m_connectedWire = wire;
        m_isConnected = true;
        
        
        return true;
    }

    /// <summary>
    /// 전선 연결 해제
    /// </summary>
    public void DisconnectWire()
    {
        if (!m_isConnected)
        {
            
            return;
        }

        WireController disconnectedWire = m_connectedWire;
        m_connectedWire = null;
        m_isConnected = false;
        
        
    }

    /// <summary>
    /// 연결점이 특정 위치에서 연결 가능한지 확인
    /// </summary>
    /// <param name="position">확인할 위치</param>
    /// <returns>연결 가능 여부</returns>
    public bool CanConnectAt(Vector3 position)
    {
        if (m_isConnected) return false;
        
        float distance = Vector3.Distance(transform.position, position);
        return distance <= m_connectionRadius;
    }

    /// <summary>
    /// 연결점의 방향 벡터 반환 (전선이 바라볼 방향)
    /// </summary>
    /// <returns>연결점의 방향 벡터</returns>
    public Vector3 GetConnectionDirection()
    {
        return transform.forward;
    }


    #endregion

    #region Private Methods
    /// <summary>
    /// 연결점 초기화
    /// </summary>
    private void InitializeConnectionPoint()
    {
        
    }

    /// <summary>
    /// 극성에 따른 색상 반환
    /// </summary>
    /// <returns>극성 색상</returns>
    private Color GetPolarityColor()
    {
        return m_connectionType == CONNECTION_TYPE.Positive ? m_positiveColor : m_negativeColor;
    }
    #endregion
} 