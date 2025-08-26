using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

public class CheckVertexUniformVectors : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private bool m_checkOnStart = true;
    [SerializeField] private bool m_includeInactive = false;
    
    private HashSet<Material> m_checkedMaterials = new HashSet<Material>();

    // JavaScript 플러그인 함수들 (WebGL 전용)
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int GetMaxVertexUniformVectors();
    
    [DllImport("__Internal")]
    private static extern int GetMaxFragmentUniformVectors();
    
    [DllImport("__Internal")]
    private static extern int GetMaxVaryingVectors();
    
    [DllImport("__Internal")]
    private static extern int GetMaxVertexAttribs();
#else
    // WebGL이 아닌 환경에서는 더미 함수 사용
    private static int GetMaxVertexUniformVectors() { return -1; }
    private static int GetMaxFragmentUniformVectors() { return -1; }
    private static int GetMaxVaryingVectors() { return -1; }
    private static int GetMaxVertexAttribs() { return -1; }
#endif

    void Start()
    {
        if (m_checkOnStart)
        {
            CheckAllMaterialsVertexUniformVectors();
        }
    }

    [ContextMenu("VertexUniformVectors 체크")]
    public void CheckAllMaterialsVertexUniformVectors()
    {
        m_checkedMaterials.Clear();
        
        
        
        // 실제 WebGL 제한값 가져오기
        PrintWebGLLimits();
        
        // 머테리얼 체크
        CheckMaterials();
    }
    
    private void PrintWebGLLimits()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // 실제 WebGL 빌드 환경
        try
        {
            int maxVertexUniformVectors = GetMaxVertexUniformVectors();
            int maxFragmentUniformVectors = GetMaxFragmentUniformVectors();
            int maxVaryingVectors = GetMaxVaryingVectors();
            int maxVertexAttribs = GetMaxVertexAttribs();
            
            
            
            
            
            
            // WebGL 버전 판정
            if (maxVertexUniformVectors >= 1024)
            {
                
            }
            else if (maxVertexUniformVectors >= 254)
            {
                
            }
            else
            {
                
            }
        }
        catch (System.Exception e)
        {
            
            PrintFallbackInfo();
        }
#else
        // Unity Editor 또는 다른 플랫폼
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            
        }
        else
        {
            
        }
        PrintFallbackInfo();
#endif
    }
    
    private void PrintFallbackInfo()
    {
        
        
        
        
        // 추정값 제공
        if (SystemInfo.graphicsShaderLevel >= 35)
        {
            
        }
        else
        {
            
        }
    }
    
    private void CheckMaterials()
    {
        // 씬에서 모든 Renderer 찾기
        Renderer[] renderers = m_includeInactive ? 
            Resources.FindObjectsOfTypeAll<Renderer>() : 
            FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        
        
        
        foreach (Renderer renderer in renderers)
        {
            if (!m_includeInactive && !renderer.gameObject.activeInHierarchy)
                continue;
                
            CheckRendererMaterials(renderer);
        }
        
        PrintMaterialSummary();
    }
    
    private void CheckRendererMaterials(Renderer renderer)
    {
        if (renderer?.sharedMaterials == null) return;
        
        foreach (Material material in renderer.sharedMaterials)
        {
            if (material == null || m_checkedMaterials.Contains(material))
                continue;
                
            m_checkedMaterials.Add(material);
            
            // 간단한 머테리얼 정보만 출력
            int estimatedVectors = EstimateVertexUniformVectors(material);
            
            
        }
    }
    
    private int EstimateVertexUniformVectors(Material material)
    {
        if (material?.shader == null) return 0;
        
        int propertyCount = material.shader.GetPropertyCount();
        int vectorProps = 0;
        int floatProps = 0;
        
        for (int i = 0; i < propertyCount; i++)
        {
            var propertyType = material.shader.GetPropertyType(i);
            
            switch (propertyType)
            {
                case UnityEngine.Rendering.ShaderPropertyType.Vector:
                    vectorProps++;
                    break;
                case UnityEngine.Rendering.ShaderPropertyType.Float:
                case UnityEngine.Rendering.ShaderPropertyType.Int:
                    floatProps++;
                    break;
            }
        }
        
        // 간단한 추정: Vector는 1개, Float/Int는 0.25개씩 + 기본 매트릭스 8개
        return vectorProps + Mathf.CeilToInt(floatProps / 4.0f) + 8;
    }
    
    private void PrintMaterialSummary()
    {
        
        
        // 셰이더별 그룹화
        var shaderGroups = m_checkedMaterials
            .Where(m => m?.shader != null)
            .GroupBy(m => m.shader.name)
            .OrderBy(g => g.Key);
            
        foreach (var group in shaderGroups)
        {
            
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            CheckAllMaterialsVertexUniformVectors();
        }
    }
}
