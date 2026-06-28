using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class GameManager : MonoBehaviour
{
    private const int TitleSceneIdx = 99;
    private const int MainSceneIdx  = 0;

    public static GameManager Instance { get; private set; }

    public bool IsMiniGameRunning { get; set; }

    public Action<Dictionary<StateTypes, int>> OnMiniGameQuited;

    private JoyConInputManager _inputManager;
    private List<StateTypes> _evolutionRoute = new();
    private HechiSpriteContainer _hechiSpriteContainer;
    private AudioManager _audioManager;

    private static readonly HashSet<string> MainSceneKeepActive = new()
    {
        "MainSceneManager",
        "Brain Camera",
        "Target Camera"
    };

    private readonly HashSet<int> _playedMiniGameIndices = new();
    private int _currentMiniGameSceneIdx = -1;
    private readonly List<GameObject> _hiddenMainObjects = new();
    private readonly HashSet<GameObject> _keepActiveObjects = new();

    public void RegisterKeepActive(GameObject obj) => _keepActiveObjects.Add(obj);
    public void UnregisterKeepActive(GameObject obj) => _keepActiveObjects.Remove(obj);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);

        _inputManager = GetComponentInChildren<JoyConInputManager>();
        _inputManager.Init();

        _hechiSpriteContainer = GetComponentInChildren<HechiSpriteContainer>();
    }

    public IPlayerInputReader GetPlayerInputReader(int playerId)
        => _inputManager.GetPlayerInputReader(playerId);

    public void SetActiveAllInput(bool active)
        => _inputManager.SetActiveAllInput(active);

    public void LoadMiniGame()
    {
        if (IsMiniGameRunning) return;

        int idx = PickMiniGameSceneIdx();
        if (idx < 0)
        {
            Debug.LogWarning("[GameManager] 플레이 가능한 미니게임 씬이 없습니다.");
            return;
        }

        StartCoroutine(LoadMiniGameRoutine(idx));
    }

    public void QuitMiniGame(Dictionary<StateTypes, int> deltaStates)
    {
        if (!IsMiniGameRunning) return;
        StartCoroutine(QuitMiniGameRoutine(deltaStates));
    }

    private void HideMainScene()
    {
        _hiddenMainObjects.Clear();
        var mainScene = SceneManager.GetSceneByBuildIndex(MainSceneIdx);
        foreach (var root in mainScene.GetRootGameObjects())
        {
            if (MainSceneKeepActive.Contains(root.name) || _keepActiveObjects.Contains(root) || !root.activeSelf) continue;
            root.SetActive(false);
            _hiddenMainObjects.Add(root);
        }
    }

    private void RestoreMainScene()
    {
        foreach (var obj in _hiddenMainObjects)
            if (obj != null) obj.SetActive(true);
        _hiddenMainObjects.Clear();
    }

    // Build 설정에서 Title(0), Main(1)을 제외한 씬 중 미플레이 씬을 랜덤 선정
    // 모두 플레이했으면 기록을 초기화하고 다시 전체 풀에서 선정
    private int PickMiniGameSceneIdx()
    {
        int totalScenes = SceneManager.sceneCountInBuildSettings;

        var available = Enumerable.Range(0, totalScenes)
            .Where(i => i != TitleSceneIdx && i != MainSceneIdx && !_playedMiniGameIndices.Contains(i))
            .ToList();

        if (available.Count == 0)
        {
            Debug.Log("[GameManager] 모든 미니게임 완료 — 기록 초기화 후 재선정");
            _playedMiniGameIndices.Clear();

            available = Enumerable.Range(0, totalScenes)
                .Where(i => i != TitleSceneIdx && i != MainSceneIdx)
                .ToList();
        }

        if (available.Count == 0) return -1;

        return available[UnityEngine.Random.Range(0, available.Count)];
    }

    private IEnumerator LoadMiniGameRoutine(int miniGameSceneIdx)
    {
        IsMiniGameRunning = true;
        _currentMiniGameSceneIdx = miniGameSceneIdx;
        _playedMiniGameIndices.Add(miniGameSceneIdx);

        yield return SceneManager.LoadSceneAsync(miniGameSceneIdx, LoadSceneMode.Additive);

        var miniGameScene = SceneManager.GetSceneByBuildIndex(miniGameSceneIdx);
        foreach (var root in miniGameScene.GetRootGameObjects())
        foreach (var cam in root.GetComponentsInChildren<Camera>(true))
        {
            cam.depth = 100f;
            cam.clearFlags = CameraClearFlags.Depth;

            if (cam.TryGetComponent<AudioListener>(out var listener))
                listener.enabled = false;
        }

        HideMainScene();

        Debug.Log($"[GameManager] 미니게임 씬 로드 완료 (Build idx: {miniGameSceneIdx})");
    }

    private IEnumerator QuitMiniGameRoutine(Dictionary<StateTypes, int> deltaStates)
    {
        // TODO: 로딩 화면 들어오면 순서 다시 정리
        
        yield return SceneManager.UnloadSceneAsync(_currentMiniGameSceneIdx);
        _currentMiniGameSceneIdx = -1;
        //IsMiniGameRunning = false;

        RestoreMainScene();

        Debug.Log("[GameManager] 미니게임 씬 종료, Main으로 복귀");
        OnMiniGameQuited?.Invoke(deltaStates);
    }

    public Sprite GetHechiSpriteOnMain()
        => _hechiSpriteContainer.GetHechiSpriteOnMain(_evolutionRoute);

    public Sprite GetHechiSpriteOnMiniGame()
        => _hechiSpriteContainer.GetHechiSpriteOnMiniGame(_evolutionRoute);

    public string GetHechiName()
        => _hechiSpriteContainer.GetHechiName(_evolutionRoute);
    

    public (Sprite sprite, string name) OnEvolution(StateTypes newState)
    {
        _evolutionRoute.Add(newState);
        return (GetHechiSpriteOnMain(),  GetHechiName());
    }
}
