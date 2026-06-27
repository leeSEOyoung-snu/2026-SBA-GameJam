using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HechiChaseGame : OneVsThreeBase
{
    [SerializeField] private ChaseCharacterController[] players;
    [SerializeField] private ChaseCharacterController hechi;

    [Header("결과 델타 (해치 승리)")]
    [SerializeField] private int hechiWinNightmare = 5;
    private List<ChaseCharacterController> _normalPlayers = new List<ChaseCharacterController>();

    private bool _gameOver;

    public override int NightmareDelta { get; protected set; }
    public override bool IsOneWin { get; protected set; }

    private void Start()
    {
        InitCharacters();
        StartCoroutine(WaitForGameStart());
    }

    private IEnumerator WaitForGameStart()
    {
        bool started = false;
        void Handler() { started = true; }
        BasicMiniGameCanvas.OnGameStarted += Handler;
        yield return new WaitUntil(() => started);
        BasicMiniGameCanvas.OnGameStarted -= Handler;
        
        if (MiniGameManager.Instance.ResultContainer.IsTimeAttack)
            StartCoroutine(TimerRoutine());
    }

    private void InitCharacters()
    {
        Debug.Log($"[HechiChase] 해치: Player {OnePlayerId}");
        for (int i = 1; i <= 4; i++)
        {
            bool isHechi = i == OnePlayerId;

            if (isHechi)
            {
                players[i - 1].gameObject.SetActive(false);
                hechi.transform.position = players[i - 1].transform.position;
                hechi.GetComponent<MiniGameCharacterController>().Init(i);
                hechi.Init(isHechi, OnPlayerEliminated);
            }
            else
            {
                players[i-1].GetComponent<MiniGameCharacterController>().Init(i);
                players[i-1].Init(isHechi, OnPlayerEliminated);
                _normalPlayers.Add(players[i-1]);
            }
        }
    }

    private void OnPlayerEliminated(ChaseCharacterController characterController)
    {
        _normalPlayers.Remove(characterController);
        Debug.Log($"[HechiChase] Player {characterController.GetComponent<MiniGameCharacterController>().PlayerId} 제거 / 남은 플레이어: {_normalPlayers.Count}");

        if (_normalPlayers.Count == 0)
            EndGame(hechiWins: true);
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
            EndGame(hechiWins: false);
    }

    private void EndGame(bool hechiWins)
    {
        if (_gameOver) return;
        _gameOver = true;

        IsOneWin = hechiWins;
        NightmareDelta = hechiWins ? 0 : hechiWinNightmare;

        MiniGameManager.Instance.QuitMiniGame();
    }
}
