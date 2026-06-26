using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HechiBollyBallGame : OneVsThreeBase
{
    [Header("프리팹")]
    [SerializeField] private GameObject hachiPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject ballPrefab;

    [Header("스폰 범위 — 해치 (왼쪽 절반)")]
    [SerializeField] private Vector2 hachiSpawnMin = new(-8f, -3.5f);
    [SerializeField] private Vector2 hachiSpawnMax = new(-0.5f, 3.5f);

    [Header("스폰 범위 — 플레이어 3명 (오른쪽 절반)")]
    [SerializeField] private Vector2 playerSpawnMin = new(0.5f, -3.5f);
    [SerializeField] private Vector2 playerSpawnMax = new(8f, 3.5f);

    [SerializeField] private float minSpawnDistance = 1.2f;
    [SerializeField] private int scoreToWin = 3;

    [Header("결과 델타")]
    [SerializeField] private int threeWinNightmare = 5;

    private int _hachiScore;
    private int _threeScore;
    private bool _gameOver;
    private int _hachiPlayerId;
    private BollyBall _ball;

    public override int NightmareDelta { get; protected set; }
    public override bool IsOneWin { get; protected set; }
    public override int OnePlayerId { get; protected set; }

    public event System.Action<int, int> OnScoreChanged; // (hachiScore, threeScore)

    private void Start()
    {
        SpawnCharacters();
        SpawnBall();
    }

    private void SpawnCharacters()
    {
        _hachiPlayerId = Random.Range(1, 5);
        Debug.Log($"[BollyBall] 해치: Player {_hachiPlayerId}");

        var usedPos = new List<Vector2>();

        // 해치 1명 (왼쪽)
        var hachiPos = GetRandomPos(hachiSpawnMin, hachiSpawnMax, usedPos);
        usedPos.Add(hachiPos);
        var hachiObj = Instantiate(hachiPrefab, (Vector3)hachiPos, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(hachiObj, gameObject.scene);
        Debug.Log($"[BollyBall] 해치 스폰 완료: {hachiPos}");

        if (hachiObj.TryGetComponent<BollyBallCharacterController>(out var hachiCtrl))
            hachiCtrl.Init(isHachi: true);
        else
            Debug.LogError("[BollyBall] hachiPrefab에 BollyBallCharacterController가 없습니다!");

        if (hachiObj.TryGetComponent<MiniGameCharacterController>(out var hachiMini))
            hachiMini.Init(_hachiPlayerId);
        else
            Debug.LogError("[BollyBall] hachiPrefab에 MiniGameCharacterController가 없습니다!");

        // 플레이어 3명 (오른쪽)
        int spawnedCount = 0;
        for (int i = 1; i <= 4; i++)
        {
            if (i == _hachiPlayerId) continue;

            var pos = GetRandomPos(playerSpawnMin, playerSpawnMax, usedPos);
            usedPos.Add(pos);

            var playerObj = Instantiate(playerPrefab, (Vector3)pos, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(playerObj, gameObject.scene);
            spawnedCount++;
            Debug.Log($"[BollyBall] 플레이어 {i} 스폰 완료: {pos}");

            if (playerObj.TryGetComponent<BollyBallCharacterController>(out var ctrl))
                ctrl.Init(isHachi: false);
            else
                Debug.LogError($"[BollyBall] playerPrefab에 BollyBallCharacterController가 없습니다! (Player {i})");

            if (playerObj.TryGetComponent<MiniGameCharacterController>(out var mini))
                mini.Init(i);
            else
                Debug.LogError($"[BollyBall] playerPrefab에 MiniGameCharacterController가 없습니다! (Player {i})");
        }
        Debug.Log($"[BollyBall] 총 플레이어 스폰: {spawnedCount}명");
    }

    private void SpawnBall()
    {
        var ballObj = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(ballObj, gameObject.scene);
        _ball = ballObj.GetComponent<BollyBall>();
        _ball.OnHachiScored += () => AddScore(isHachi: true);
        _ball.OnThreeScored += () => AddScore(isHachi: false);
    }

    private void AddScore(bool isHachi)
    {
        if (_gameOver) return;

        if (isHachi) _hachiScore++;
        else         _threeScore++;

        Debug.Log($"[BollyBall] 점수 — 해치: {_hachiScore} / 3명: {_threeScore}");
        OnScoreChanged?.Invoke(_hachiScore, _threeScore);

        if (_hachiScore >= scoreToWin)      EndGame(hachiWins: true);
        else if (_threeScore >= scoreToWin) EndGame(hachiWins: false);
        else                                _ball.ResetBall();
    }

    private void EndGame(bool hachiWins)
    {
        if (_gameOver) return;
        _gameOver = true;

        IsOneWin       = hachiWins;
        OnePlayerId    = _hachiPlayerId;
        NightmareDelta = hachiWins ? 0 : threeWinNightmare;

        MiniGameManager.Instance.QuitMiniGame();
    }

    private Vector2 GetRandomPos(Vector2 min, Vector2 max, List<Vector2> used)
    {
        const int maxTry = 30;
        for (int i = 0; i < maxTry; i++)
        {
            var c = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
            bool ok = true;
            foreach (var u in used)
                if (Vector2.Distance(c, u) < minSpawnDistance) { ok = false; break; }
            if (ok) return c;
        }
        return new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
    }
}
