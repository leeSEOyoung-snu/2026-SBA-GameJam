using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class HechiShootingGame : AffectionBattleBase
{
    [SerializeField] private GameObject crosshairPrefab;
    [SerializeField] private GameObject nightmarePrefab;
    [SerializeField] private GameObject hachiPrefab;
    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private float nightmareSpawnInterval = 2f;
    [SerializeField] private int maxNightmares = 8;

    [Header("스폰 범위")]
    [SerializeField] private Vector2 spawnMin = new(-7f, -3.5f);
    [SerializeField] private Vector2 spawnMax = new(7f, 3.5f);

    [Header("결과")]
    [SerializeField] private int hachiShotNightmareDelta = 5;

    private readonly int[] _scores = new int[5]; // index 1~4
    private bool _gameOver;
    private bool _hachiKilledGame;

    public override int AffectionDeltaPlayer1 { get; protected set; }
    public override int AffectionDeltaPlayer2 { get; protected set; }
    public override int AffectionDeltaPlayer3 { get; protected set; }
    public override int AffectionDeltaPlayer4 { get; protected set; }
    public override int NightmareDelta { get; protected set; }

    private void Start()
    {
        SpawnHachi();
        SpawnCrosshairs();
        StartCoroutine(NightmareSpawnRoutine());
        StartCoroutine(TimerRoutine());
    }

    private void SpawnHachi()
    {
        var pos = GetRandomSpawnPosition();
        var hachiObj = Instantiate(hachiPrefab, (Vector3)pos, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(hachiObj, gameObject.scene);
        hachiObj.GetComponent<HechiShootingHachi>().Init(OnHachiGameOver);
    }

    private void SpawnCrosshairs()
    {
        for (int i = 1; i <= 4; i++)
        {
            var crosshairObj = Instantiate(crosshairPrefab, Vector3.zero, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(crosshairObj, gameObject.scene);
            int playerId = i;
            crosshairObj.GetComponent<HechiShootingCrosshair>().Init(
                playerId,
                OnNightmareHit,
                () => { } // 해치를 맞춰도 크로스헤어 쪽엔 별도 처리 없음
            );
        }
    }

    private IEnumerator NightmareSpawnRoutine()
    {
        while (!_gameOver)
        {
            yield return new WaitForSeconds(nightmareSpawnInterval);
            if (_gameOver) yield break;

            // 현재 살아있는 악몽 수 확인
            var existing = FindObjectsByType<HechiShootingNightmare>(FindObjectsSortMode.None);
            if (existing.Length < maxNightmares)
                SpawnNightmare();
        }
    }

    private Vector2 GetRandomSpawnPosition()
    {
        return new Vector2(Random.Range(spawnMin.x, spawnMax.x), Random.Range(spawnMin.y, spawnMax.y));
    }

    private void SpawnNightmare()
    {
        var pos = GetRandomSpawnPosition();
        var obj = Instantiate(nightmarePrefab, (Vector3)pos, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
        obj.GetComponent<HechiShootingNightmare>().Init(OnNightmareDestroyed);
    }

    private void OnNightmareHit(int playerId)
    {
        // 점수는 nightmare가 destroyed될 때 주지 않고, 크로스헤어 히트 시 바로 부여
        _scores[playerId]++;
        Debug.Log($"[HechiShooting] Player {playerId} 점수: {_scores[playerId]}");
    }

    private void OnNightmareDestroyed(HechiShootingNightmare nightmare, int playerId)
    {
        Destroy(nightmare.gameObject);
    }

    private void OnHachiGameOver()
    {
        if (_gameOver) return;
        _hachiKilledGame = true;
        Debug.Log("[HechiShooting] 해치 3번 피격 → 게임 오버");
        EndGame();
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
            Debug.Log("[HechiShooting] 시간 종료");
            EndGame();
        }
    }

    private void EndGame()
    {
        if (_gameOver) return;
        _gameOver = true;

        NightmareDelta = _hachiKilledGame ? hachiShotNightmareDelta : 0;

        // 랭킹 계산 (점수 내림차순, 동점이면 낮은 playerId 우선)
        var ranking = new List<int> { 1, 2, 3, 4 };
        ranking.Sort((a, b) =>
        {
            int scoreDiff = _scores[b] - _scores[a];
            return scoreDiff != 0 ? scoreDiff : a - b;
        });

        int[] affectionByRank = { 3, 2, 1, 0 };
        int[] affection = new int[5];
        for (int i = 0; i < ranking.Count; i++)
            affection[ranking[i]] = affectionByRank[i];

        AffectionDeltaPlayer1 = affection[1];
        AffectionDeltaPlayer2 = affection[2];
        AffectionDeltaPlayer3 = affection[3];
        AffectionDeltaPlayer4 = affection[4];

        Debug.Log($"[HechiShooting] 결과 - 1등:{ranking[0]} 2등:{ranking[1]} 3등:{ranking[2]} 4등:{ranking[3]}");

        MiniGameManager.Instance.QuitMiniGame();
    }
}
