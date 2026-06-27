using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class HechiShootingGame : SoloBattleBase
{
    [SerializeField] private GameObject crosshairPrefab;
    [SerializeField] private GameObject nightmarePrefab;
    [SerializeField] private GameObject hachiPrefab;
    [SerializeField] private float nightmareSpawnInterval = 2f;
    [SerializeField] private int maxNightmares = 8;

    [Header("스폰 범위")]
    [SerializeField] private Vector2 spawnMin = new(-7f, -3.5f);
    [SerializeField] private Vector2 spawnMax = new(7f, 3.5f);

    [Header("결과")]
    [SerializeField] private int hachiShotNightmareDelta = 5;

    private readonly int[] _scores = new int[5];
    private bool _gameOver;
    private bool _hachiKilledGame;

    public override int RankPlayer1 { get; protected set; }
    public override int RankPlayer2 { get; protected set; }
    public override int RankPlayer3 { get; protected set; }
    public override int RankPlayer4 { get; protected set; }
    public override int NightmareDelta { get; protected set; }

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

        SpawnHachi();
        SpawnCrosshairs();
        StartCoroutine(NightmareSpawnRoutine());
        if (MiniGameManager.Instance.ResultContainer.IsTimeAttack)
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
            crosshairObj.GetComponent<HechiShootingCrosshair>().Init(playerId, OnNightmareHit, () => { });
        }
    }

    private IEnumerator NightmareSpawnRoutine()
    {
        while (!_gameOver)
        {
            yield return new WaitForSeconds(nightmareSpawnInterval);
            if (_gameOver) yield break;

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
        float remaining = MiniGameManager.Instance.ResultContainer.TimeAttackSeconds;
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

        var ranking = new List<int> { 1, 2, 3, 4 };
        ranking.Sort((a, b) =>
        {
            int diff = _scores[b] - _scores[a];
            return diff != 0 ? diff : a - b;
        });

        int[] rankResult = new int[5];
        for (int i = 0; i < ranking.Count; i++)
            rankResult[ranking[i]] = i + 1;

        RankPlayer1 = rankResult[1];
        RankPlayer2 = rankResult[2];
        RankPlayer3 = rankResult[3];
        RankPlayer4 = rankResult[4];

        Debug.Log($"[HechiShooting] 결과 - 1등:{ranking[0]} 2등:{ranking[1]} 3등:{ranking[2]} 4등:{ranking[3]}");

        MiniGameManager.Instance.QuitMiniGame();
    }
}
