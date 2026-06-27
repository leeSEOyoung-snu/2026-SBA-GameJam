using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DontWorryGame : OneVsThreeBase
{
    [SerializeField] private GameObject fakeHachiPrefab;
    [SerializeField] private GameObject aiHachiPrefab;
    [SerializeField] private GameObject crosshairPrefab;
    [SerializeField] private float timeLimit = 60f;

    [Header("랜덤 스폰 범위")]
    [SerializeField] private Vector2 spawnMin = new(-7f, -3.5f);
    [SerializeField] private Vector2 spawnMax = new(7f, 3.5f);
    [SerializeField] private float minSpawnDistance = 1.5f; // 캐릭터 간 최소 거리

    [Header("결과 델타")]
    [SerializeField] private int realHachiShotNightmare = 5;

    private readonly List<DontWorryFakeHachiController> _fakePlayers = new();
    private bool _gameOver;
    private int _shooterPlayerId;

    public override int NightmareDelta { get; protected set; }
    public override bool IsOneWin { get; protected set; }
    public override int OnePlayerId { get; protected set; }

    private void Start()
    {
        SpawnCharacters();
        StartCoroutine(TimerRoutine());
    }

    private void SpawnCharacters()
    {
        _shooterPlayerId = Random.Range(1, 5);
        OnePlayerId = _shooterPlayerId;
        Debug.Log($"[DontWorry] 슈터: Player {_shooterPlayerId}");

        var usedPositions = new List<Vector2>();

        // AI 해치 생성
        var aiPos = GetRandomSpawnPosition(usedPositions);
        usedPositions.Add(aiPos);
        var aiObj = Instantiate(aiHachiPrefab, (Vector3)aiPos, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(aiObj, gameObject.scene);

        // 크로스헤어 생성
        var crosshairObj = Instantiate(crosshairPrefab, Vector3.zero, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(crosshairObj, gameObject.scene);
        crosshairObj.GetComponent<DontWorryCrosshair>().Init(_shooterPlayerId, OnShotFired);

        // 가짜 해치 3명 생성
        for (int i = 1; i <= 4; i++)
        {
            if (i == _shooterPlayerId) continue;

            var pos = GetRandomSpawnPosition(usedPositions);
            usedPositions.Add(pos);

            var fakeObj = Instantiate(fakeHachiPrefab, (Vector3)pos, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(fakeObj, gameObject.scene);

            var fake = fakeObj.GetComponent<DontWorryFakeHachiController>();
            fake.Init(OnFakeEliminated);

            fakeObj.GetComponent<MiniGameCharacterController>().Init(i);
            _fakePlayers.Add(fake);
        }
    }

    private Vector2 GetRandomSpawnPosition(List<Vector2> usedPositions)
    {
        const int maxAttempts = 30;
        for (int i = 0; i < maxAttempts; i++)
        {
            var candidate = new Vector2(
                Random.Range(spawnMin.x, spawnMax.x),
                Random.Range(spawnMin.y, spawnMax.y)
            );

            bool tooClose = false;
            foreach (var used in usedPositions)
            {
                if (Vector2.Distance(candidate, used) < minSpawnDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose) return candidate;
        }

        // 30번 시도 후 실패하면 그냥 랜덤 위치 반환
        return new Vector2(Random.Range(spawnMin.x, spawnMax.x), Random.Range(spawnMin.y, spawnMax.y));
    }

    // 크로스헤어가 발사됐을 때 호출
    private void OnShotFired(bool hitRealHachi)
    {
        if (_gameOver) return;

        if (hitRealHachi)
        {
            Debug.Log("[DontWorry] 진짜 해치 맞음 → 3명 플레이어 승리");
            EndGame(shooterWins: false);
        }
    }

    private void OnFakeEliminated(DontWorryFakeHachiController fake)
    {
        if (_gameOver) return;

        _fakePlayers.Remove(fake);
        Debug.Log($"[DontWorry] 가짜 해치 제거 / 남은: {_fakePlayers.Count}");

        if (_fakePlayers.Count == 0)
        {
            Debug.Log("[DontWorry] 가짜 해치 전원 제거 → 슈터 승리");
            EndGame(shooterWins: true);
        }
    }

    private IEnumerator TimerRoutine()
    {
        float remaining = timeLimit;
        while (remaining > 0f && !_gameOver)
        {
            remaining -= Time.deltaTime;
            yield return null;
        }

        if (!_gameOver)
        {
            Debug.Log("[DontWorry] 시간 종료 → 3명 플레이어 승리");
            EndGame(shooterWins: false);
        }
    }

    private void EndGame(bool shooterWins)
    {
        if (_gameOver) return;
        _gameOver = true;

        IsOneWin = shooterWins;
        OnePlayerId = _shooterPlayerId;
        NightmareDelta = shooterWins ? 0 : realHachiShotNightmare;

        MiniGameManager.Instance.QuitMiniGame();
    }
}
