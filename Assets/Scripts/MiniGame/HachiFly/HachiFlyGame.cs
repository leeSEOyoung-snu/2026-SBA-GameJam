using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HachiFlyGame : CooperativeBase
{
    [Header("프리팹")]
    [SerializeField] private GameObject hachiVPrefab;

    [Header("게임 설정")]
    [SerializeField] private float timeLimit  = 60f;
    [SerializeField] private int   maxLives   = 5;
    [SerializeField] private float invincibleDuration = 1.5f;  // 피격 후 무적 시간
    [SerializeField] private Vector3 spawnPos = new(0f, -3f, 0f);

    [Header("결과 델타")]
    [SerializeField] private int failNightmare = 5;

    [Space(10)] [SerializeField] private CinemachineCamera cam;

    [Header("UI (없어도 동작)")]
    [SerializeField] private TMPro.TextMeshProUGUI timerText;
    [SerializeField] private TMPro.TextMeshProUGUI livesText;

    public override int  NightmareDelta { get; protected set; }
    public override bool IsSuccess      { get; protected set; }

    public Transform HachiTransform { get; private set; }

    private HachiFlyController _hachiV;
    private int   _lives;
    private bool  _gameOver;
    private bool  _invincible;

    private void Start()
    {
        _lives = maxLives;

        var obj = Instantiate(hachiVPrefab, spawnPos, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
        _hachiV = obj.GetComponent<HachiFlyController>();
        _hachiV.Init();
        HachiTransform = obj.transform;

        cam.Follow = obj.transform;

        // 스포너에 플레이어 위치 전달
        var spawner = GetComponent<HachiFlyObstacleSpawner>();
        if (spawner != null) spawner.Init(HachiTransform);

        StartCoroutine(GameRoutine());
    }

    // 장애물이 호출
    public void TakeHit()
    {
        if (_gameOver || _invincible) return;

        _lives--;
        Debug.Log($"[HachiFly] 피격! 남은 목숨: {_lives}");
        if (livesText != null) livesText.text = $"♥ x{_lives}";

        if (_lives <= 0)
        {
            EndGame(success: false);
        }
        else
        {
            StartCoroutine(InvincibleRoutine());
        }
    }

    private IEnumerator InvincibleRoutine()
    {
        _invincible = true;
        yield return new WaitForSeconds(invincibleDuration);
        _invincible = false;
    }

    private IEnumerator GameRoutine()
    {
        float elapsed = 0f;
        if (livesText != null) livesText.text = $"♥ x{_lives}";

        while (elapsed < timeLimit && !_gameOver)
        {
            elapsed += Time.deltaTime;
            if (timerText != null)
                timerText.text = (timeLimit - elapsed).ToString("F1");
            yield return null;
        }

        if (!_gameOver)
            EndGame(success: true);
    }

    private void EndGame(bool success)
    {
        if (_gameOver) return;
        _gameOver = true;

        IsSuccess      = success;
        NightmareDelta = success ? 0 : failNightmare;

        Debug.Log(success ? "[HachiFly] 60초 생존 성공!" : "[HachiFly] 격추 — 악몽 +5");
        MiniGameManager.Instance.QuitMiniGame();
    }
}
