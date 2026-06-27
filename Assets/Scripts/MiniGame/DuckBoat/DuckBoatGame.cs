using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

// 화면 왼쪽 레인: Team1 / 오른쪽 레인: Team2
// 보트는 위로 전진, 장애물은 위에서 아래로 내려옴, 좌우로 피함
public class DuckBoatGame : TwoVsTwoBase
{
    [Header("프리팹")]
    [SerializeField] private GameObject boatPrefab;

    [Header("레인 설정")]
    [SerializeField] private Vector3 team1SpawnPos = new(-4f, -3f, 0f);
    [SerializeField] private Vector3 team2SpawnPos = new( 4f, -3f, 0f);

    [Header("결과 델타")]
    [SerializeField] private int bothDeadNightmare = 5;

    private DuckBoat _boat1;
    private DuckBoat _boat2;
    private bool _gameOver;

    public override int NightmareDelta        { get; protected set; }
    public override TwoVsTwoWinner Winner     { get; protected set; }

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

        _boat1 = SpawnBoat(team1SpawnPos, PlayerIdsTeam1, laneXMin: -8f,  laneXMax: -0.5f);
        _boat2 = SpawnBoat(team2SpawnPos, PlayerIdsTeam2, laneXMin:  0.5f, laneXMax:  8f);

        _boat1.OnBoatDead += () => OnTeamDead();
        _boat2.OnBoatDead += () => OnTeamDead();

        if (MiniGameManager.Instance.ResultContainer.IsTimeAttack)
            StartCoroutine(TimerRoutine());
    }

    private DuckBoat SpawnBoat(Vector3 pos, List<int> playerIds, float laneXMin, float laneXMax)
    {
        var obj = Instantiate(boatPrefab, pos, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
        var boat = obj.GetComponent<DuckBoat>();
        boat.Init(playerIds, laneXMin, laneXMax);
        return boat;
    }

    private void OnTeamDead()
    {
        if (_gameOver) return;

        bool boat1Dead = _boat1.IsDead;
        bool boat2Dead = _boat2.IsDead;

        if (boat1Dead && boat2Dead)
        {
            Debug.Log("[DuckBoat] 두 팀 모두 게임오버 — 거리 비교");
            NightmareDelta = bothDeadNightmare;
            Winner = _boat1.Distance >= _boat2.Distance
                ? TwoVsTwoWinner.Team1
                : TwoVsTwoWinner.Team2;
        }
        else
        {
            NightmareDelta = 0;
            Winner = boat1Dead ? TwoVsTwoWinner.Team2 : TwoVsTwoWinner.Team1;
            Debug.Log($"[DuckBoat] {(boat1Dead ? "Team2" : "Team1")} 승리");
        }

        _gameOver = true;
        MiniGameManager.Instance.QuitMiniGame();
    }

    private IEnumerator TimerRoutine()
    {
        yield return new WaitForSeconds(MiniGameManager.Instance.ResultContainer.TimeAttackSeconds);
        if (_gameOver) yield break;

        Debug.Log("[DuckBoat] 시간 종료 → 거리 비교");
        NightmareDelta = 0;
        Winner = _boat1.Distance >= _boat2.Distance ? TwoVsTwoWinner.Team1 : TwoVsTwoWinner.Team2;
        _gameOver = true;
        MiniGameManager.Instance.QuitMiniGame();
    }
}
