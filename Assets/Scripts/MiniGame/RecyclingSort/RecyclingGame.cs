using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

/// <summary>
/// 분류해라! 재활용 쓰레기 — 협동 미니게임.
///
/// 규칙:
///   · 4명이 각자 시소 하나를 SL/SR로 조작
///   · 하늘 배출구(3곳)에서 15개의 쓰레기가 랜덤 낙하 (뒤로 갈수록 간격 단축)
///   · 올바른 통에 넣으면 성공 카운트++
///   · 틀린 통 또는 화면 밖 낙하 → 실수++
///   · 실수 3회 → 즉시 실패 (악몽+5)
///   · 15개 모두 처리 후 실수 < 3 → 성공
/// </summary>
public class RecyclingGame : CooperativeBase
{
    // ── Inspector ──────────────────────────────────────────
    [Header("씬 오브젝트 참조")]
    [SerializeField] private RecyclingSeesaw[] seesaws;        // 인덱스 0=플레이어1 … 3=플레이어4
    [SerializeField] private RecyclingBin[]    bins;           // 4개, AcceptedType 각각 설정
    [SerializeField] private Transform[]       spawnPoints;    // 3개 배출구 위치

    [Header("쓰레기 프리팹 (TrashType 순서 0~3)")]
    [SerializeField] private GameObject[] trashPrefabs;        // Paper, Plastic, Glass, Metal

    [Header("게임 설정")]
    [SerializeField] private int   totalTrash        = 15;
    [SerializeField] private int   mistakeLimit      = 3;
    [SerializeField] private float spawnIntervalMin  = 0.8f;   // 최소 간격(초)
    [SerializeField] private float spawnIntervalMax  = 2.5f;   // 최대 간격(초, 처음)
    [SerializeField] private int   failNightmareDelta = 5;

    // ── 상태 ───────────────────────────────────────────────
    private int  _spawned;
    private int  _processed;   // 빈에 들어갔거나 화면 밖으로 사라진 수
    private int  _mistakes;
    private bool _gameOver;

    // ── CooperativeBase ────────────────────────────────────
    public override bool IsSuccess    { get; protected set; }
    public override int  NightmareDelta { get; protected set; }

    // ──────────────────────────────────────────────────────
    private void Start()
    {
        InitSeesaws();
        InitBins();
        StartCoroutine(SpawnRoutine());
    }

    private void InitSeesaws()
    {
        for (int i = 0; i < seesaws.Length; i++)
        {
            if (seesaws[i] == null) continue;
            int playerId = i + 1; // 1~4
            seesaws[i].Init(playerId);
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

    // ── 쓰레기 스폰 ────────────────────────────────────────
    private IEnumerator SpawnRoutine()
    {
        while (_spawned < totalTrash && !_gameOver)
        {
            // 진행될수록 간격 단축 (선형 보간)
            float t        = (float)_spawned / totalTrash;
            float interval = Mathf.Lerp(spawnIntervalMax, spawnIntervalMin, t);
            yield return new WaitForSeconds(interval);

            if (_gameOver) yield break;
            SpawnTrash();
        }
    }

    private void SpawnTrash()
    {
        // 랜덤 배출구 선택
        Transform spawnPt = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // 랜덤 쓰레기 종류
        int typeIdx = Random.Range(0, trashPrefabs.Length);
        var prefab  = trashPrefabs[typeIdx];
        if (prefab == null) return;

        var obj = Instantiate(prefab, spawnPt.position, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(obj, gameObject.scene);

        var trash = obj.GetComponent<RecyclingTrash>();
        trash.Init((RecyclingTrashType)typeIdx, OnTrashFellOff);

        _spawned++;
    }

    // ── 이벤트 콜백 ───────────────────────────────────────
    private void OnTrashEnterBin(bool correct, RecyclingTrash trash)
    {
        if (_gameOver) return;

        if (!correct)
        {
            _mistakes++;
            Debug.Log($"[RecyclingSort] 오분류! 실수 {_mistakes}/{mistakeLimit}");
            if (_mistakes >= mistakeLimit)
            {
                EndGame(success: false);
                return;
            }
        }
        else
        {
            Debug.Log("[RecyclingSort] 정확한 분류!");
        }

        _processed++;
        CheckAllProcessed();
    }

    private void OnTrashFellOff(RecyclingTrash trash)
    {
        if (_gameOver) return;

        _mistakes++;
        Debug.Log($"[RecyclingSort] 쓰레기 낙하 실수 {_mistakes}/{mistakeLimit}");
        if (_mistakes >= mistakeLimit)
        {
            EndGame(success: false);
            return;
        }

        _processed++;
        CheckAllProcessed();
    }

    private void CheckAllProcessed()
    {
        if (_processed >= totalTrash)
            EndGame(success: _mistakes < mistakeLimit);
    }

    // ── 게임 종료 ──────────────────────────────────────────
    private void EndGame(bool success)
    {
        if (_gameOver) return;
        _gameOver  = true;
        IsSuccess  = success;
        NightmareDelta = success ? 0 : failNightmareDelta;

        Debug.Log($"[RecyclingSort] 종료 — 성공:{success} 실수:{_mistakes}");
        MiniGameManager.Instance.QuitMiniGame();
    }
}
