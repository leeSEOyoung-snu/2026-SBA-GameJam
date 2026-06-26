using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const string MiniGameSceneName = "MiniGameBase";

    public static GameManager Instance { get; private set; }

    public bool IsMiniGameRunning { get; private set; }

    private JoyConInputManager _inputManager;

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
        _inputManager = GetComponentInChildren<JoyConInputManager>();
        _inputManager.Init();
    }

    public IPlayerInputReader GetPlayerInputReader(int playerId)
        => _inputManager.GetPlayerInputReader(playerId);

    public void LoadMiniGame()
    {
        if (IsMiniGameRunning) return;
        StartCoroutine(LoadMiniGameRoutine());
    }

    public void QuitMiniGame()
    {
        if (!IsMiniGameRunning) return;
        StartCoroutine(QuitMiniGameRoutine());
    }

    private IEnumerator LoadMiniGameRoutine()
    {
        IsMiniGameRunning = true;
        yield return SceneManager.LoadSceneAsync(MiniGameSceneName, LoadSceneMode.Additive);

        // MiniGameBase 씬의 카메라를 최상단 depth로 설정
        var miniGameScene = SceneManager.GetSceneByName(MiniGameSceneName);
        foreach (var root in miniGameScene.GetRootGameObjects())
        foreach (var cam in root.GetComponentsInChildren<Camera>(true))
        {
            cam.depth = 100f;
            cam.clearFlags = CameraClearFlags.Depth;

            // Main 씬의 AudioListener와 충돌 방지
            if (cam.TryGetComponent<AudioListener>(out var listener))
                listener.enabled = false;
        }

        Debug.Log("[GameManager] MiniGameBase 씬 로드 완료");
    }

    private IEnumerator QuitMiniGameRoutine()
    {
        yield return SceneManager.UnloadSceneAsync(MiniGameSceneName);
        IsMiniGameRunning = false;
        Debug.Log("[GameManager] MiniGameBase 씬 종료, Main으로 복귀");
    }
}
