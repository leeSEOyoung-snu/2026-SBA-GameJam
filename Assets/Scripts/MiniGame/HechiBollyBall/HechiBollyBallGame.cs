using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HechiBollyBallGame : OneVsThreeBase
{
    [Header("씬 오브젝트")]
    [SerializeField] private GameObject hachiObject;
    [SerializeField] private GameObject[] playerObjects = new GameObject[3];
    [SerializeField] private BollyBall ball;

    [SerializeField] private int   scoreToWin  = 3;
    [SerializeField] private float resetDelay  = 1.5f;

    [Header("결과 델타")]
    [SerializeField] private int threeWinNightmare = 5;

    private int _hachiScore;
    private int _threeScore;
    private bool _gameOver;

    private BollyBall _ball;
    private GameObject _hachiObj;
    private readonly List<(GameObject obj, Vector3 spawnPos)> _players = new();
    private Vector3 _hachiSpawnPos;
    private Vector3 _ballSpawnPos;
    private Vector3[] _playerSpawnPositions = new Vector3[3];

    public override int NightmareDelta { get; protected set; }
    public override bool IsOneWin      { get; protected set; }

    public event System.Action<int, int> OnScoreChanged;

    private void Awake()
    {
        CacheSceneSpawnPositions();
        SetSceneObjectsActive(false);
        MiniGameManager.Instance.ApplyCurrentMiniGameHechiSprite(hachiObject);
    }

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

        if (!SetupCharacters() || !SetupBall())
            yield break;

        if (MiniGameManager.Instance.ResultContainer.IsTimeAttack)
            StartCoroutine(TimerRoutine());
    }

    private void CacheSceneSpawnPositions()
    {
        if (hachiObject != null)
            _hachiSpawnPos = hachiObject.transform.position;

        if (ball != null)
            _ballSpawnPos = ball.transform.position;

        if (playerObjects == null)
            return;

        int count = Mathf.Min(playerObjects.Length, _playerSpawnPositions.Length);
        for (int i = 0; i < count; i++)
        {
            if (playerObjects[i] != null)
                _playerSpawnPositions[i] = playerObjects[i].transform.position;
        }
    }

    private void SetSceneObjectsActive(bool active)
    {
        hachiObject?.SetActive(active);
        ball?.gameObject.SetActive(active);

        if (playerObjects == null)
            return;

        foreach (var playerObject in playerObjects)
            playerObject?.SetActive(active);
    }

    private bool SetupCharacters()
    {
        _players.Clear();

        if (hachiObject == null)
        {
            Debug.LogError("[BollyBall] 씬에 배치된 hachiObject 참조가 없습니다.");
            return false;
        }

        if (playerObjects == null || playerObjects.Length < 3)
        {
            Debug.LogError("[BollyBall] 씬에 배치된 playerObjects 3개가 필요합니다.");
            return false;
        }

        Debug.Log($"[BollyBall] 해치: Player {OnePlayerId}");

        _hachiObj = hachiObject;
        _hachiObj.SetActive(true);
        ResetActor(_hachiObj, _hachiSpawnPos);
        if (!InitActor(_hachiObj, isHachi: true, OnePlayerId))
            return false;

        int idx = 0;
        for (int i = 1; i <= 4; i++)
        {
            if (i == OnePlayerId) continue;

            var obj = playerObjects[idx];
            if (obj == null)
            {
                Debug.LogError($"[BollyBall] 씬에 배치된 playerObjects[{idx}] 참조가 없습니다. (Player {i})");
                return false;
            }

            var pos = _playerSpawnPositions[idx];
            obj.SetActive(true);
            ResetActor(obj, pos);
            if (!InitActor(obj, isHachi: false, i))
                return false;

            _players.Add((obj, pos));
            idx++;
        }

        return true;
    }

    private bool InitActor(GameObject actor, bool isHachi, int playerId)
    {
        if (actor.TryGetComponent<BollyBallCharacterController>(out var bollyCtrl))
            bollyCtrl.Init(isHachi);
        else
            Debug.LogError($"[BollyBall] {actor.name}에 BollyBallCharacterController가 없습니다.");

        if (actor.TryGetComponent<MiniGameCharacterController>(out var miniGameCtrl))
            miniGameCtrl.Init(playerId);
        else
            Debug.LogError($"[BollyBall] {actor.name}에 MiniGameCharacterController가 없습니다.");

        return bollyCtrl != null && miniGameCtrl != null;
    }

    private bool SetupBall()
    {
        if (ball == null)
        {
            Debug.LogError("[BollyBall] 씬에 배치된 ball 참조가 없습니다.");
            return false;
        }

        _ball = ball;
        _ball.gameObject.SetActive(true);
        _ball.ResetToPosition(_ballSpawnPos);
        _ball.OnHachiScored += HandleHachiScored;
        _ball.OnThreeScored += HandleThreeScored;

        return true;
    }

    private void HandleHachiScored()
    {
        AddScore(isHachi: true);
    }

    private void HandleThreeScored()
    {
        AddScore(isHachi: false);
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
        else                                StartCoroutine(ResetRoutine());
    }

    private IEnumerator ResetRoutine()
    {
        yield return new WaitForSeconds(resetDelay);

        if (_hachiObj != null)
            ResetActor(_hachiObj, _hachiSpawnPos);

        foreach (var (obj, pos) in _players)
        {
            if (obj == null) continue;
            ResetActor(obj, pos);
        }

        _ball.ResetToPosition(_ballSpawnPos);

        Debug.Log("[BollyBall] 전원 리셋 완료");
    }

    private static void ResetActor(GameObject actor, Vector3 position)
    {
        if (actor.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        actor.transform.position = position;
    }

    private IEnumerator TimerRoutine()
    {
        yield return new WaitForSeconds(MiniGameManager.Instance.ResultContainer.TimeAttackSeconds);
        if (_gameOver) yield break;

        Debug.Log("[BollyBall] 시간 종료 → 점수 앞선 팀 승리");
        EndGame(hachiWins: _hachiScore > _threeScore);
    }

    private void EndGame(bool hachiWins)
    {
        if (_gameOver) return;
        _gameOver = true;

        IsOneWin       = hachiWins;
        NightmareDelta = hachiWins ? 0 : threeWinNightmare;

        MiniGameManager.Instance.QuitMiniGame();
    }

    private void OnDestroy()
    {
        if (_ball == null) return;

        _ball.OnHachiScored -= HandleHachiScored;
        _ball.OnThreeScored -= HandleThreeScored;
    }
}
