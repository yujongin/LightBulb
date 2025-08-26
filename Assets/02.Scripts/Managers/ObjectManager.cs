using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ObjectManager : MonoBehaviour
{
    [System.Serializable]
    public class ObjectData
    {
        [Header("기본 정보")]
        public string objectName; // 오브젝트 이름 (고유 키)
        public GameObject gameObject; // 씬에 이미 존재하는 게임오브젝트
        
        [Header("추가 정보")]
        public string category = "Default"; // 카테고리 (선택사항)
        public string[] tags; // 태그들 (검색용)
        public string description; // 설명 (선택사항)
    }

    [Header("오브젝트 데이터")]
    [SerializeField] private List<ObjectData> objectDatas = new List<ObjectData>();
    
    [Header("자동 등록 설정")]
    [SerializeField] private bool autoRegisterOnStart = true; // 시작 시 자동으로 씬의 오브젝트들 등록
    [SerializeField] private string[] autoRegisterTags; // 자동 등록할 태그들
    
    // 빠른 검색을 위한 딕셔너리
    private Dictionary<string, ObjectData> objectDict = new Dictionary<string, ObjectData>();
    private Dictionary<string, List<ObjectData>> categoryDict = new Dictionary<string, List<ObjectData>>();

    public void Init()
    {
        InitializeDictionaries();
        
        if (autoRegisterOnStart)
        {
            AutoRegisterObjects();
        }
        
        
    }

    /// <summary>
    /// 딕셔너리 초기화
    /// </summary>
    private void InitializeDictionaries()
    {
        objectDict.Clear();
        categoryDict.Clear();

        foreach (var data in objectDatas)
        {
            if (string.IsNullOrEmpty(data.objectName))
            {
                
                continue;
            }

            // 이름 딕셔너리에 추가
            if (!objectDict.ContainsKey(data.objectName))
            {
                objectDict.Add(data.objectName, data);
            }
            else
            {
                
            }

            // 카테고리 딕셔너리에 추가
            if (!categoryDict.ContainsKey(data.category))
            {
                categoryDict[data.category] = new List<ObjectData>();
            }
            categoryDict[data.category].Add(data);
        }
    }

    /// <summary>
    /// 씬의 오브젝트들을 자동으로 등록
    /// </summary>
    private void AutoRegisterObjects()
    {
        if (autoRegisterTags == null || autoRegisterTags.Length == 0) return;

        foreach (string tag in autoRegisterTags)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.CompareTag(tag))
                {
                    // 이미 등록되지 않은 경우에만 추가
                    if (!HasObject(obj.name))
                    {
                        RegisterObject(obj.name, obj, tag, new string[] { tag });
                    }
                }
            }
        }
    }

    /// <summary>
    /// 이름으로 오브젝트 데이터 가져오기
    /// </summary>
    public ObjectData GetObjectData(string objectName)
    {
        if (objectDict.TryGetValue(objectName, out ObjectData data))
        {
            return data;
        }
        
        
        return null;
    }

    /// <summary>
    /// 이름으로 게임오브젝트 가져오기
    /// </summary>
    public GameObject GetObject(string objectName)
    {
        ObjectData data = GetObjectData(objectName);
        return data?.gameObject;
    }

    /// <summary>
    /// 이름으로 컴포넌트 가져오기
    /// </summary>
    public T GetComponent<T>(string objectName) where T : Component
    {
        GameObject obj = GetObject(objectName);
        return obj?.GetComponent<T>();
    }

    /// <summary>
    /// 카테고리로 오브젝트 목록 가져오기
    /// </summary>
    public List<ObjectData> GetObjectsByCategory(string category)
    {
        if (categoryDict.TryGetValue(category, out List<ObjectData> objects))
        {
            return objects;
        }
        
        return new List<ObjectData>();
    }

    /// <summary>
    /// 카테고리로 게임오브젝트 목록 가져오기
    /// </summary>
    public List<GameObject> GetGameObjectsByCategory(string category)
    {
        List<ObjectData> objectDatas = GetObjectsByCategory(category);
        return objectDatas.Select(data => data.gameObject).Where(obj => obj != null).ToList();
    }

    /// <summary>
    /// 태그로 오브젝트 검색
    /// </summary>
    public List<ObjectData> GetObjectsByTag(string tag)
    {
        return objectDatas.Where(data => data.tags != null && data.tags.Contains(tag)).ToList();
    }

    /// <summary>
    /// 태그로 게임오브젝트 목록 가져오기
    /// </summary>
    public List<GameObject> GetGameObjectsByTag(string tag)
    {
        List<ObjectData> objectDatas = GetObjectsByTag(tag);
        return objectDatas.Select(data => data.gameObject).Where(obj => obj != null).ToList();
    }

    /// <summary>
    /// 모든 카테고리 목록 가져오기
    /// </summary>
    public List<string> GetAllCategories()
    {
        return categoryDict.Keys.ToList();
    }

    /// <summary>
    /// 런타임에 오브젝트 등록
    /// </summary>
    public void RegisterObject(string objectName, GameObject gameObject, string category = "Runtime", string[] tags = null)
    {
        // 이미 존재하는지 확인
        if (objectDict.ContainsKey(objectName))
        {
            
            return;
        }

        ObjectData newData = new ObjectData
        {
            objectName = objectName,
            gameObject = gameObject,
            category = category,
            tags = tags ?? new string[0]
        };

        objectDatas.Add(newData);
        objectDict.Add(objectName, newData);

        // 카테고리 딕셔너리에 추가
        if (!categoryDict.ContainsKey(category))
        {
            categoryDict[category] = new List<ObjectData>();
        }
        categoryDict[category].Add(newData);

        
    }

    /// <summary>
    /// 런타임에 오브젝트 데이터 제거
    /// </summary>
    public void UnregisterObject(string objectName)
    {
        ObjectData data = GetObjectData(objectName);
        if (data == null) return;

        // 딕셔너리에서 제거
        objectDict.Remove(objectName);
        categoryDict[data.category].Remove(data);
        
        // 리스트에서 제거
        objectDatas.Remove(data);

        
    }

    /// <summary>
    /// 오브젝트 존재 여부 확인
    /// </summary>
    public bool HasObject(string objectName)
    {
        return objectDict.ContainsKey(objectName);
    }

    /// <summary>
    /// 등록된 모든 오브젝트 이름 가져오기
    /// </summary>
    public List<string> GetAllObjectNames()
    {
        return objectDict.Keys.ToList();
    }

    /// <summary>
    /// 오브젝트 활성화/비활성화
    /// </summary>
    public void SetObjectActive(string objectName, bool active)
    {
        GameObject obj = GetObject(objectName);
        if (obj != null)
        {
            obj.SetActive(active);
        }
    }

    /// <summary>
    /// 카테고리의 모든 오브젝트 활성화/비활성화
    /// </summary>
    public void SetCategoryActive(string category, bool active)
    {
        List<GameObject> objects = GetGameObjectsByCategory(category);
        foreach (GameObject obj in objects)
        {
            obj.SetActive(active);
        }
    }

    /// <summary>
    /// 태그의 모든 오브젝트 활성화/비활성화
    /// </summary>
    public void SetTagActive(string tag, bool active)
    {
        List<GameObject> objects = GetGameObjectsByTag(tag);
        foreach (GameObject obj in objects)
        {
            obj.SetActive(active);
        }
    }

    /// <summary>
    /// 디버그용: 등록된 오브젝트 정보 출력
    /// </summary>
    [ContextMenu("Debug Object Info")]
    public void DebugObjectInfo()
    {
        
        
        
        
        foreach (var category in categoryDict)
        {
            
            foreach (var data in category.Value)
            {
                string status = data.gameObject != null ? (data.gameObject.activeInHierarchy ? "활성" : "비활성") : "null";
                
            }
        }
    }
}
