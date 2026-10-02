using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers _instance;
    public static Managers Instance { get { Init(); return _instance; } }

    #region Core
    private readonly InputManager _inputManager = new();
    public static InputManager Input => Instance._inputManager;
    #endregion

    #region Contents
    private readonly DateManager _dateManager = new();
    public static DateManager Date => Instance._dateManager;
    private readonly FaxManager _faxManager = new();
    public static FaxManager Fax => Instance._faxManager;
    private readonly GameManager _gameManager = new();
    public static GameManager Game => Instance._gameManager;
    private readonly SoundManager _soundManager = new();
    public static SoundManager Sound => Instance._soundManager;
    private readonly LightManager _lightManager = new();
    public static LightManager Light => Instance._lightManager;
    private readonly PostProcessingManager _postProcessingManager = new();
    public static PostProcessingManager PostProcessing => Instance._postProcessingManager;
    #endregion

    void Start()
    {
        Init();
    }

    public static void Init()
    {
        if (_instance == null)
        {
            GameObject go = GameObject.Find("@Manager");
            if (go == null)
            {
                go = new GameObject { name = "@Manager" };
                go.AddComponent<Managers>();
            }
            DontDestroyOnLoad(go);
            _instance = go.GetComponent<Managers>();

            _instance._inputManager.Init();

            _instance._dateManager.Init();
            _instance._faxManager.Init();
            _instance._gameManager.Init();
            _instance._soundManager.Init();
            _instance._lightManager.Init();
            _instance._postProcessingManager.Init();

            GameObject eventSystem = Instantiate(Resources.Load<GameObject>("Prefabs/UIs/EventSystem"));

            eventSystem.transform.SetParent(_instance.transform);
        }
    }

    public static void Clear()
    {
        Input.Clear();
        Date.Clear();
        Fax.Clear();
        Game.Clear();
        Sound.Clear();
        Light.Clear();
        PostProcessing.Clear();
    }
}
