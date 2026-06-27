using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 날아라 해치 V — 협동 (Cooperative)
// 4명이 해치 V의 팔/다리 하나씩 조종
// 제한 시간 안에 목표 높이에 도달하면 성공
public class HachiFlyGame : CooperativeBase
{
    [Header("프리팹")]
    [SerializeField] private GameObject hachiVPrefab;

    [Header("게임 설정")]
    [SerializeField] private float timeLimit  = 30f;   // 제한 시간 (초)
    [SerializeField] private float goalY      = 15f;   // 목표 높이 (이 Y 이상 도달 시 성공)
    [SerializeField] private Vector3 spawnPos = new(0f, -3f, 0f);

    [Header("결과 델타")]
    [SerializeField] private int failNightmare = 5;

    private HachiFlyController _hachiV;
    private bool _gameOver;
    private float _elapsed;

    public override int  NightmareDelta { get; protected set; }
    public override bool IsSuccess      { get; protected set; }

    // UI 연결용 (없어도 동작)
    [SerializeField] private TMPro.TextMeshProUGUI timerText;
    [SerializeField] private TMPro.TextMeshProUGUI goalText;

    private void Start()
    {
        var obj = Instantiate(hachiVPrefab, spawnPos, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
        _hachiV = obj.GetComponent<HachiFlyController>();
        _hachiV.Init();

        // 카메라 타겟 런타임 연결
        var cam = Camera.main.GetComponent<HachiFlyCamera>();
        if (cam != null) cam.SetTarget(obj.transform);

        StartCoroutine(GameRoutine());
    }

    private void Update()
    {
        if (_gameOver) return;

        // 목표 높이 도달 체크
        if (_hachiV != null && _hachiV.transform.position.y >= goalY)
            EndGame(success: true);
    }

    private IEnumerator GameRoutine()
    {
        _elapsed = 0f;

        while (_elapsed < timeLimit && !_gameOver)
        {
            _elapsed += Time.deltaTime;

            float remaining = timeLimit - _elapsed;
            if (timerText != null)
                timerText.text = remaining.ToString("F1");

            if (goalText != null)
                goalText.text = $"목표까지: {Mathf.Max(0f, goalY - _hachiV.transform.position.y):F1}";

            yield return null;
        }

        if (!_gameOver)
            EndGame(success: false);
    }

    private void EndGame(bool success)
    {
        if (_gameOver) return;
        _gameOver = true;

        IsSuccess      = success;
        NightmareDelta = success ? 0 : failNightmare;

        Debug.Log(success
            ? "[HachiFly] 목표 도달 성공! 모두 호감도 +3"
            : "[HachiFly] 시간 초과 — 악몽 +5");

        MiniGameManager.Instance.QuitMiniGame();
    }
}
