using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HechiChaseGame : OneVsThreeBase
{
    [SerializeField] private ChaseCharacterController[] players;
    [SerializeField] private ChaseCharacterController hechi;

    [SerializeField] private BasicPlayerCanvasManager basicPlayerCanvasManager;

    [Header("결과 델타 (해치 승리)")]
    [SerializeField] private int hechiWinNightmare = 5;

    [Header("오디오")]
    [SerializeField] private AudioClip catchClip;
    [SerializeField] private AudioClip explodeClip;
    private List<ChaseCharacterController> _normalPlayers = new List<ChaseCharacterController>();

    private int _totalPlayers;
    private bool _gameOver;

    public override int NightmareDelta { get; protected set; }
    public override bool IsOneWin { get; protected set; }

    private void Start()
    {
        InitCharacters();
        MiniGameManager.Instance?.ArrangeOneVsThreePlayerLayout(OnePlayerId);
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

        _totalPlayers = _normalPlayers.Count;
        // 이 씬 전용: 해치(1인팀) 말풍선을 캐릭터와 안 겹치게 위로 올리고 크게, 글자는 작게
        basicPlayerCanvasManager?.StyleStackBubble(OnePlayerId, 90f, new Vector2(230f, 150f), 28f);
        UpdateHechiCatchText();
    }

    // 해치(1인팀, 1P 자리) 슬롯에 잡은 캐릭터 수 표기
    private void UpdateHechiCatchText()
    {
        int caught = _totalPlayers - _normalPlayers.Count;
        basicPlayerCanvasManager?.SetStackAsString(OnePlayerId, $"잡은 캐릭터\n{caught} / {_totalPlayers}");
    }

    private void OnPlayerEliminated(ChaseCharacterController characterController)
    {
        int playerId = characterController.GetComponent<MiniGameCharacterController>().PlayerId;
        basicPlayerCanvasManager?.GreyOutCharacter(playerId);
        _normalPlayers.Remove(characterController);
        UpdateHechiCatchText();
        MiniGameManager.Instance.Audio?.PlaySfx(catchClip);
        Debug.Log($"[HechiChase] Player {playerId} 제거 / 남은 플레이어: {_normalPlayers.Count}");

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

        if (hechiWins)
            MiniGameManager.Instance.Audio?.PlaySfx(explodeClip);

        MiniGameManager.Instance.QuitMiniGame();
    }
}
