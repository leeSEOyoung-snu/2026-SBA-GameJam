using System.Collections;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HachiFlyGame : CooperativeBase
{
    [Header("프리팹")]
    [SerializeField] private GameObject hachiVPrefab;

    [Header("게임 설정")]
    [SerializeField] private int   maxLives   = 5;
    [SerializeField] private float invincibleDuration = 1.5f;
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
        StartCoroutine(WaitForGameStart());
    }

    private IEnumerator WaitForGameStart()
    {
        bool started = false;
        void Handler() { started = true; }
        BasicMiniGameCanvas.OnGameStarted += Handler;
        yield return new WaitUntil(() => started);
        BasicMiniGameCanvas.OnGameStarted -= Handler;

        _lives = maxLives;

        var obj = Instantiate(hachiVPrefab, spawnPos, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
        _hachiV = obj.GetComponent<HachiFlyController>();
        _hachiV.Init();
        HachiTransform = obj.transform;

        cam.Follow = obj.transform;

        var spawner = GetComponent<HachiFlyObstacleSpawner>();
        if (spawner != null) spawner.Init(HachiTransform);

        if (MiniGameManager.Instance.ResultContainer.IsTimeAttack)
            StartCoroutine(GameRoutine());
    }

    public void TakeHit()
    {
        if (_gameOver || _invincible) return;

        _lives--;
        Debug.Log($"[HachiFly] 피격! 남은 목숨: {_lives}");
        if (livesText != null) livesText.text = $"♥ x{_lives}";

        if (_lives <= 0)
            EndGame(success: false);
        else
            StartCoroutine(InvincibleRoutine());
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

        while (elapsed < MiniGameManager.Instance.ResultContainer.TimeAttackSeconds && !_gameOver)
        {
            elapsed += Time.deltaTime;
            if (timerText != null)
                timerText.text = (MiniGameManager.Instance.ResultContainer.TimeAttackSeconds - elapsed).ToString("F1");
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
        StartCoroutine(EndRoutine());
    }

    private IEnumerator EndRoutine()
    {
        var canvas = FindObjectOfType<BasicMiniGameCanvas>();
        if (canvas != null)
            yield return canvas.PlayGameEnd().WaitForCompletion();
        MiniGameManager.Instance.QuitMiniGame();
    }
}
