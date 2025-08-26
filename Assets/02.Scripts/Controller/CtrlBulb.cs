using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CtrlBulb : CtrlInteractable
{
    [SerializeField] private GameObject m_lightBulb;
    [SerializeField] private GameObject m_lightBulbMesh;
    [SerializeField] private Material m_lightBulbOffMaterial;
    [SerializeField] private Material m_lightBulbOnMaterial;
    [SerializeField] private int m_materialIndex = 1; // 변경할 머티리얼 인덱스 (기본값: 1)
    private GameObject m_lightBulbHolder;
    private Animator m_bulbAnimator;
    protected override void Start()
    {
        base.Start();
    }


    protected override void OnDragEnd()
    {
        base.OnDragEnd();
        if (m_lightBulbHolder == null)
        {
            m_lightBulbHolder = Managers.Object.GetObject("LightBulbHolder");
        }

        // 배터리와 홀더의 콜라이더 범위 확인
        if (m_lightBulbHolder != null)
        {
            Collider lightBulbCollider = GetComponent<Collider>();
            Collider holderCollider = m_lightBulbHolder.GetComponent<Collider>();

            if (lightBulbCollider != null && holderCollider != null)
            {
                // 콜라이더 범위가 겹치는지 확인
                if (lightBulbCollider.bounds.Intersects(holderCollider.bounds))
                {
                    // 홀더의 애니메이터 활성화
                    m_bulbAnimator = m_lightBulb.GetComponent<Animator>();
                    if (m_bulbAnimator != null)
                    {
                        m_bulbAnimator.enabled = true;
                        SetInteractionMode(InteractionMode.None);
                        Managers.UI.ClosePopUpText();
                        Sequence seq = DOTween.Sequence();
                        seq.AppendInterval(1f);
                        seq.Append(m_lightBulbHolder.transform.DOMove(new Vector3(0.1f, 0.0084f, 0.15f), 2f).SetEase(Ease.InOutSine));
                        seq.Join(m_lightBulbHolder.transform.DORotate(new Vector3(0, 45, 0), 2f).SetEase(Ease.InOutSine));
                        seq.AppendCallback(() =>
                        {
                            Managers.Command.ExecuteCommand(3);
                        });
                        lightBulbCollider.enabled = false;
                    }
                }
            }
        }
    }
    public void Redo()
    {
        if (m_lightBulbHolder == null)
        {
            m_lightBulbHolder = Managers.Object.GetObject("LightBulbHolder");
        }

        // 홀더의 애니메이터 활성화
        m_bulbAnimator = m_lightBulb.GetComponent<Animator>();
        if (m_bulbAnimator != null)
        {
            SetInteractionMode(InteractionMode.None);
            m_bulbAnimator.enabled = true;
            m_bulbAnimator.Play("BulbInsert", 0, 1);
            Managers.UI.ClosePopUpText();
            m_lightBulbHolder.transform.localPosition = new Vector3(0.1f, 0.0084f, 0.15f);
            m_lightBulbHolder.transform.localRotation = Quaternion.Euler(0, 45, 0);
            GetComponent<Collider>().enabled = false;
        }

    }

    public void TurnOn()
    {
        ChangeBulbMaterial(m_lightBulbOnMaterial);
    }

    public void TurnOff()
    {
        ChangeBulbMaterial(m_lightBulbOffMaterial);
    }
    
    /// <summary>
    /// 전구 머티리얼을 안전하게 변경하는 메서드
    /// </summary>
    private void ChangeBulbMaterial(Material newMaterial)
    {
        if (m_lightBulbMesh == null)
        {
            return;
        }
        
        Renderer bulbRenderer = m_lightBulbMesh.GetComponent<Renderer>();
        if (bulbRenderer == null)
        {
            return;
        }
        
        if (newMaterial == null)
        {
            return;
        }
        
        // 머티리얼 배열을 복사하여 수정
        Material[] materials = bulbRenderer.materials;
        
        // 설정된 인덱스가 유효한지 확인
        if (m_materialIndex >= 0 && m_materialIndex < materials.Length)
        {
            materials[m_materialIndex] = newMaterial;
            bulbRenderer.materials = materials;
        }
        // 설정된 인덱스가 유효하지 않으면 첫 번째 머티리얼 사용
        else if (materials.Length > 0)
        {
            materials[0] = newMaterial;
            bulbRenderer.materials = materials;
        }
    }
}
