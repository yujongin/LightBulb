using UnityEngine;

public class Managers : MonoBehaviour
{
    public static bool Initialized { get; set; } = false;
    public static Managers s_instance;
    public static Managers Instance { get { return s_instance; } }
    private GameManager _game;
    private SystemManager _system;
    private UIManager _ui;
    private CameraManager _camera;
    private GuideManager _guide;
    private CommandManager _command;
    private ObjectManager _object;

    public static SystemManager System { get { return Instance?._system; } }
    public static UIManager UI { get { return Instance?._ui; } }
    public static GameManager Game { get { return Instance?._game; } }
    public static CameraManager Camera { get { return Instance?._camera; } }
    public static GuideManager Guide { get { return Instance?._guide; } }
    public static CommandManager Command { get { return Instance?._command; } }
    public static ObjectManager Object { get { return Instance?._object; } }
    
    private void Awake()
    {
        _system = GetComponentInChildren<SystemManager>();
        _ui = GetComponentInChildren<UIManager>();
        _game = GetComponentInChildren<GameManager>();
        _camera = GetComponentInChildren<CameraManager>();
        _guide = GetComponentInChildren<GuideManager>();
        _command = GetComponentInChildren<CommandManager>();
        _object = GetComponentInChildren<ObjectManager>();
        Init();
        _system?.Init();
        _ui?.Init();
        _camera?.Init();
        _object?.Init();
    }

    public static void Init()
    {
        Initialized = true;

        GameObject go = GameObject.Find("Managers");

        if (go == null)
        {
            go = new GameObject { name = "Managers" };
            go.AddComponent<Managers>();
        }
        s_instance = go.GetComponent<Managers>();

    }
}
