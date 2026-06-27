using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

/// <summary>
/// 분류해라! 재활용 쓰레기 — 협동 미니게임.
/// 쓰레기가 처리(빈 투입 or 낙하)된 직후 짧은 딜레이로 다음 쓰레기 스폰.
/// </summary>
public class RecyclingGame : CooperativeBase
{
    [Header("씬 오브젝트 참조")]
    [SerializeField] private RecyclingSeesaw[] seesaws;
    [SerializeField] private RecyclingBin[]    bins;
    [SerializeField] private Transform[]       spawnPoints;
    [SerializeField] private TMP_Text          goalDescText;
    [SerializeField] private TMP_Text          goalCountText;

    [Header("쓰레기 프리팹 (TrashType 순서 0~3)")]
    [SerializeField] private GameObject[] trashPrefabs;

    [Header("게임 설정")]
    [SerializeField] private int   totalTrash        = 15;
    [SerializeField] private int   mistakeLimit      = 3;
    [SerializeField] private float nextSpawnDelay    = 1.2f;
    [SerializeField] private int   failNightmareDelta = 5;

    private int  _spawned;
    private int  _processed;
    private int  _sortedCorrectly;
    private int  _mistakes;
    private bool _gameOver;

    public override bool IsSuccess     { get; protected set; }
    public override int  NightmareDelta { get; protected set; }

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

        InitSeesaws();
        InitBins();
        UpdateGoalView();
        SpawnTrash(); // 첫 쓰레기 바로 스폰
    }

    private void InitSeesaws()
    {
        for (int i = 0; i < seesaws.Length; i++)
        {
            if (seesaws[i] == null) continue;
            seesaws[i].Init(i + 1);
        }
    }

    private void InitBins()
    {
        foreach (var bin in bins)
        {
            if (bin == null) continue;
            bin.Init(OnTrashEnterBin);
        }
    }

    private void SpawnTrash()
    {
        if (_gameOver || _spawned >= totalTrash) return;

        Transform spawnPt = spawnPoints[spawnPoints.Length / 2];
        int typeIdx = Random.Range(0, trashPrefabs.Length);
        var prefab  = trashPrefabs[typeIdx];
        if (prefab == null) return;

        var obj = Instantiate(prefab, spawnPt.position, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
        obj.GetComponent<RecyclingTrash>().Init((RecyclingTrashType)typeIdx, OnTrashFellOff);
        _spawned++;
    }

    private void ScheduleNextSpawn()
    {
        if (_spawned < totalTrash && !_gameOver)
            StartCoroutine(SpawnAfterDelay());
    }

    private IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSeconds(nextSpawnDelay);
        SpawnTrash();
    }

    private void OnTrashEnterBin(bool correct, RecyclingTrash trash)
    {
        if (_gameOver) return;

        if (!correct)
        {
            _mistakes++;
            Debug.Log($"[RecyclingSort] 오분류! 실수 {_mistakes}/{mistakeLimit}");
            if (_mistakes >= mistakeLimit) { EndGame(false); return; }
        }
        else
        {
            _sortedCorrectly++;
            Debug.Log("[RecyclingSort] 정확한 분류!");
        }

        _processed++;
        UpdateGoalView();
        CheckAllProcessed();
        ScheduleNextSpawn();
    }

    private void OnTrashFellOff(RecyclingTrash trash)
    {
        if (_gameOver) return;

        _mistakes++;
        Debug.Log($"[RecyclingSort] 낙하 실수 {_mistakes}/{mistakeLimit}");
        if (_mistakes >= mistakeLimit) { EndGame(false); return; }

        _processed++;
        UpdateGoalView();
        CheckAllProcessed();
        ScheduleNextSpawn();
    }

    private void CheckAllProcessed()
    {
        if (_processed >= totalTrash)
            EndGame(_mistakes < mistakeLimit);
    }

    private void EndGame(bool success)
    {
        if (_gameOver) return;
        _gameOver      = true;
        IsSuccess      = success;
        NightmareDelta = success ? 0 : failNightmareDelta;
        Debug.Log($"[RecyclingSort] 종료 — 성공:{success} 실수:{_mistakes}");
        MiniGameManager.Instance.QuitMiniGame();
    }

    private void UpdateGoalView()
    {
        if (goalDescText != null)
            goalDescText.text = "목표 분리수거";

        if (goalCountText != null)
            goalCountText.text = $"{_sortedCorrectly}/{totalTrash}";
    }
}
