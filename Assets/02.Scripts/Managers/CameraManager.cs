using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera[] cameras;
    private CinemachineBlendDefinition.Styles blendStyle = CinemachineBlendDefinition.Styles.EaseInOut;
    private float blendTime = 2f;

    public void Init()
    {
        cameras = GetComponentsInChildren<CinemachineCamera>();
    }

    public float BlendTime
    {
        get
        {
            return Camera.main.GetComponent<CinemachineBrain>().DefaultBlend.BlendTime;
        }
    }

    public void ChangeCamera(int index)
    {
        foreach (var camera in cameras)
        {
            camera.Priority = 0;
        }
        cameras[index].Priority = 10;
    }

    public void SetCameraBlend()
    {
        var brain = Camera.main.GetComponent<CinemachineBrain>();
        brain.DefaultBlend = new CinemachineBlendDefinition(blendStyle, blendTime);
    }

    public void SetCameraBlend(CinemachineBlendDefinition.Styles style, float time)
    {
        blendStyle = style;
        blendTime = time;
        SetCameraBlend();
    }

    public void SetBlendStyle(CinemachineBlendDefinition.Styles style)
    {
        blendStyle = style;
        SetCameraBlend();
    }

    public void SetBlendTime(float time)
    {
        blendTime = time;
        SetCameraBlend();
    }
}
