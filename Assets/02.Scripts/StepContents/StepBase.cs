using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public abstract class StepBase : MonoBehaviour, ICommand
{
    [SerializeField] protected int stepIndex;

    protected virtual void Start()
    {
        Managers.Command.RegisterCommand(stepIndex, this);
    }

    #region ICommand 구현
    public abstract void Execute();
    public abstract void Redo();
    #endregion

    #region 오브젝트 관리 기능
    /// <summary>
    /// 이름으로 오브젝트 가져오기
    /// </summary>
    protected GameObject GetObjectByName(string objectName)
    {
        return Managers.Object.GetObject(objectName);
    }

    /// <summary>
    /// 이름으로 특정 컴포넌트 가져오기
    /// </summary>
    protected T GetComponentByName<T>(string objectName) where T : Component
    {
        return Managers.Object.GetComponent<T>(objectName);
    }

    /// <summary>
    /// 카테고리로 오브젝트 목록 가져오기
    /// </summary>
    protected List<GameObject> GetObjectsByCategory(string category)
    {
        return Managers.Object.GetGameObjectsByCategory(category);
    }

    /// <summary>
    /// 카테고리에서 첫 번째 오브젝트 가져오기
    /// </summary>
    protected GameObject GetFirstObjectByCategory(string category)
    {
        var objects = GetObjectsByCategory(category);
        return objects.Count > 0 ? objects[0] : null;
    }

    /// <summary>
    /// 태그로 오브젝트 목록 가져오기
    /// </summary>
    protected List<GameObject> GetObjectsByTag(string tag)
    {
        return Managers.Object.GetGameObjectsByTag(tag);
    }

    /// <summary>
    /// 태그에서 첫 번째 오브젝트 가져오기
    /// </summary>
    protected GameObject GetFirstObjectByTag(string tag)
    {
        var objects = GetObjectsByTag(tag);
        return objects.Count > 0 ? objects[0] : null;
    }

    /// <summary>
    /// 지정된 오브젝트의 활성화 상태 설정
    /// </summary>
    protected void SetObjectActiveByName(string objectName, bool active)
    {
        Managers.Object.SetObjectActive(objectName, active);
    }

    /// <summary>
    /// 카테고리 전체 오브젝트의 활성화 상태 설정
    /// </summary>
    protected void SetCategoryActive(string category, bool active)
    {
        Managers.Object.SetCategoryActive(category, active);
    }

    /// <summary>
    /// 태그 전체 오브젝트의 활성화 상태 설정
    /// </summary>
    protected void SetTagActive(string tag, bool active)
    {
        Managers.Object.SetTagActive(tag, active);
    }
    #endregion

    #region 이동 및 애니메이션 기능
    /// <summary>
    /// 지정된 오브젝트를 오프셋만큼 이동시킨 후 원래 위치로 돌아오는 애니메이션
    /// </summary>
    public void LinearOffsetMove(GameObject target, Vector3 offset, float duration = 1f, Ease ease = Ease.InOutSine)
    {
        if (target == null) return;

        Vector3 originalPosition = target.transform.localPosition;
        Vector3 targetPosition = originalPosition + offset;

        // 목표 위치로 이동 후 원래 위치로 돌아오는 시퀀스
        Sequence moveSequence = DOTween.Sequence();
        moveSequence.Append(target.transform.DOLocalMove(targetPosition, duration * 0.5f).SetEase(ease));
        moveSequence.Append(target.transform.DOLocalMove(originalPosition, duration * 0.5f).SetEase(ease));
    }

    /// <summary>
    /// 이름으로 오브젝트를 찾아 오프셋 이동 애니메이션 실행
    /// </summary>
    protected void LinearOffsetMoveByName(string objectName, Vector3 offset, float duration = 1f, Ease ease = Ease.InOutSine)
    {
        GameObject target = GetObjectByName(objectName);
        if (target != null)
        {
            LinearOffsetMove(target, offset, duration, ease);
        }
    }

    /// <summary>
    /// 지정된 오브젝트의 스케일을 변경하는 애니메이션
    /// </summary>
    public void ScaleMove(GameObject target, Vector3 scale, bool isStartZero = false, float duration = 1f, Ease ease = Ease.OutBack)
    {
        if (target == null) return;
        if (isStartZero)
        {
            target.transform.localScale = Vector3.zero;
        }
        target.transform.DOScale(scale, duration).SetEase(ease);
    }

    /// <summary>
    /// 이름으로 오브젝트를 찾아 스케일 애니메이션 실행
    /// </summary>
    protected void ScaleMoveByName(string objectName, Vector3 scale, bool isStartZero = false, float duration = 1f, Ease ease = Ease.OutBack)
    {
        GameObject target = GetObjectByName(objectName);
        if (target != null)
        {
            ScaleMove(target, scale, isStartZero, duration, ease);
        }
    }

    /// <summary>
    /// 카테고리의 모든 오브젝트에 이동 애니메이션 적용
    /// </summary>
    protected void LinearOffsetMoveByCategory(string category, Vector3 offset, float duration = 1f, Ease ease = Ease.InOutSine)
    {
        var objects = GetObjectsByCategory(category);
        foreach (var obj in objects)
        {
            LinearOffsetMove(obj, offset, duration, ease);
        }
    }

    /// <summary>
    /// 태그의 모든 오브젝트에 이동 애니메이션 적용
    /// </summary>
    protected void LinearOffsetMoveByTag(string tag, Vector3 offset, float duration = 1f, Ease ease = Ease.InOutSine)
    {
        var objects = GetObjectsByTag(tag);
        foreach (var obj in objects)
        {
            LinearOffsetMove(obj, offset, duration, ease);
        }
    }

    /// <summary>
    /// 카테고리의 모든 오브젝트에 스케일 애니메이션 적용
    /// </summary>
    protected void ScaleMoveByCategory(string category, Vector3 scale, bool isStartZero = false, float duration = 1f, Ease ease = Ease.OutBack)
    {
        var objects = GetObjectsByCategory(category);
        foreach (var obj in objects)
        {
            ScaleMove(obj, scale, isStartZero, duration, ease);
        }
    }

    /// <summary>
    /// 태그의 모든 오브젝트에 스케일 애니메이션 적용
    /// </summary>
    protected void ScaleMoveByTag(string tag, Vector3 scale, bool isStartZero = false, float duration = 1f, Ease ease = Ease.OutBack)
    {
        var objects = GetObjectsByTag(tag);
        foreach (var obj in objects)
        {
            ScaleMove(obj, scale, isStartZero, duration, ease);
        }
    }
    #endregion

    #region 버튼 제어 기능
    /// <summary>
    /// 현재 스텝의 버튼을 비활성화
    /// </summary>
    protected void DisableCurrentStepButton()
    {
        Managers.UI.DisableButton(stepIndex);
    }

    /// <summary>
    /// 현재 스텝의 버튼을 활성화
    /// </summary>
    protected void EnableCurrentStepButton()
    {
        Managers.UI.EnableButton(stepIndex);
    }

    /// <summary>
    /// 다음 스텝의 버튼을 활성화
    /// </summary>
    protected void EnableNextStepButton()
    {
        Managers.UI.EnableButton(stepIndex + 1);
    }

    /// <summary>
    /// 지정된 스텝의 버튼을 활성화
    /// </summary>
    protected void EnableStepButton(int targetStepIndex)
    {
        Managers.UI.EnableButton(targetStepIndex);
    }

    /// <summary>
    /// 지정된 스텝의 버튼을 비활성화
    /// </summary>
    protected void DisableStepButton(int targetStepIndex)
    {
        Managers.UI.DisableButton(targetStepIndex);
    }

    /// <summary>
    /// 지정된 스텝의 버튼 상호작용 설정
    /// </summary>
    protected void SetStepButtonInteractable(int targetStepIndex, bool interactable)
    {
        Managers.UI.SetButtonInteractable(targetStepIndex, interactable);
    }

    /// <summary>
    /// 현재 스텝의 버튼 보이기
    /// </summary>
    protected void ShowCurrentStepButton()
    {
        Managers.UI.ShowButton(stepIndex);
    }

    /// <summary>
    /// 현재 스텝의 버튼 숨기기
    /// </summary>
    protected void HideCurrentStepButton()
    {
        Managers.UI.HideButton(stepIndex);
    }

    /// <summary>
    /// 다음 스텝의 버튼 보이기
    /// </summary>
    protected void ShowNextStepButton()
    {
        Managers.UI.ShowButton(stepIndex + 1);
    }

    /// <summary>
    /// 다음 스텝의 버튼 숨기기
    /// </summary>
    protected void HideNextStepButton()
    {
        Managers.UI.HideButton(stepIndex + 1);
    }

    /// <summary>
    /// 지정된 스텝의 버튼 보이기
    /// </summary>
    protected void ShowStepButton(int targetStepIndex)
    {
        Managers.UI.ShowButton(targetStepIndex);
    }

    /// <summary>
    /// 지정된 스텝의 버튼 숨기기
    /// </summary>
    protected void HideStepButton(int targetStepIndex)
    {
        Managers.UI.HideButton(targetStepIndex);
    }

    /// <summary>
    /// 지정된 스텝의 버튼 활성화 상태 설정
    /// </summary>
    protected void SetStepButtonActive(int targetStepIndex, bool active)
    {
        Managers.UI.SetButtonActive(targetStepIndex, active);
    }

    /// <summary>
    /// 현재 스텝의 버튼 활성화 상태 설정
    /// </summary>
    protected void SetCurrentStepButtonActive(bool active)
    {
        Managers.UI.SetButtonActive(stepIndex, active);
    }

    /// <summary>
    /// 다음 스텝의 버튼 활성화 상태 설정
    /// </summary>
    protected void SetNextStepButtonActive(bool active)
    {
        Managers.UI.SetButtonActive(stepIndex + 1, active);
    }

    /// <summary>
    /// 지정된 스텝의 버튼이 활성화되어 있는지 확인
    /// </summary>
    protected bool IsStepButtonActive(int targetStepIndex)
    {
        return Managers.UI.IsButtonActive(targetStepIndex);
    }

    /// <summary>
    /// 현재 스텝의 버튼이 활성화되어 있는지 확인
    /// </summary>
    protected bool IsCurrentStepButtonActive()
    {
        return Managers.UI.IsButtonActive(stepIndex);
    }
    #endregion

    #region 유틸리티 함수
    /// <summary>
    /// 다음 스텝으로 초기화 (필요시 오버라이드)
    /// </summary>
    protected virtual void InitializeNextStep()
    {
        EnableNextStepButton();
        HideCurrentStepButton();
    }
    #endregion
}
