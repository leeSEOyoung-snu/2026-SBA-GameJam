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

    [Header("고정 스폰 위치")]
    [SerializeField] private Vector3 hachiSpawnPos   = new(-5f,  0f, 0f);
    [SerializeField] private Vector3 ballSpawnPos    = new(-1f,  0f, 0f);
    [SerializeField] private Vector3 playerSpawnPos1 = new( 5f,  2f, 0f);
    [SerializeField] private Vector3 playerSpawnPos2 = new( 5f,  0f, 0f);
    [SerializeField] private Vector3 playerSpawnPos3 = new( 5f, -2f, 0f);

    [SerializeField] private int   scoreToWin  = 3;
    [SerializeField] private float resetDelay  = 1.5f;

    [Header("결과 델타")]
    [SerializeField] private int threeWinNightmare = 5;

    private int _hachiScore;
    private int _threeScore;
    private bool _gameOver;
    private int _hachiPlayerId;

    private BollyBall _ball;
    private GameObject _hachiObj;
    private readonly List<(GameObject obj, Vector3 spawnPos)> _players = new();

    public override int NightmareDelta { get; protected set; }
    public override bool IsOneWin      { get; protected set; }
    public override int OnePlayerId    { get; protected set; }

    public event System.Action<int, int> OnScoreChanged;

    private void Start()
    {
        SpawnCharacters();
        SpawnBall();
    }

    private void SpawnCharacters()
    {
        _hachiPlayerId = Random.Range(1, 5);
        Debug.Log($"[BollyBall] 해치: Player {_hachiPlayerId}");

        // 해치
        _hachiObj = Instantiate(hachiPrefab, hachiSpawnPos, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(_hachiObj, gameObject.scene);
        if (_hachiObj.TryGetComponent<BollyBallCharacterController>(out var hachiCtrl))
            hachiCtrl.Init(isHachi: true);
        else
            Debug.LogError("[BollyBall] hachiPrefab에 BollyBallCharacterController가 없습니다!");
        _hachiObj.GetComponent<MiniGameCharacterController>().Init(_hachiPlayerId);

        // 플레이어 3명
        var spawnPositions = new[] { playerSpawnPos1, playerSpawnPos2, playerSpawnPos3 };
        int idx = 0;
        for (int i = 1; i <= 4; i++)
        {
            if (i == _hachiPlayerId) continue;
            var pos = spawnPositions[idx++];
            var obj = Instantiate(playerPrefab, pos, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
            if (obj.TryGetComponent<BollyBallCharacterController>(out var ctrl))
                ctrl.Init(isHachi: false);
            else
                Debug.LogError($"[BollyBall] playerPrefab에 BollyBallCharacterController가 없습니다! (Player {i})");
            obj.GetComponent<MiniGameCharacterController>().Init(i);
            _players.Add((obj, pos));
        }
    }

    private void SpawnBall()
    {
        var ballObj = Instantiate(ballPrefab, ballSpawnPos, Quaternion.identity);
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
        else                                StartCoroutine(ResetRoutine());
    }

    private IEnumerator ResetRoutine()
    {
        yield return new WaitForSeconds(resetDelay);

        // 해치 위치 리셋
        if (_hachiObj != null)
        {
            _hachiObj.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            _hachiObj.transform.position = hachiSpawnPos;
        }

        // 플레이어 위치 리셋
        foreach (var (obj, pos) in _players)
        {
            if (obj == null) continue;
            obj.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            obj.transform.position = pos;
        }

        // 공 리셋
        _ball.ResetToPosition(ballSpawnPos);

        Debug.Log("[BollyBall] 전원 리셋 완료");
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
}
