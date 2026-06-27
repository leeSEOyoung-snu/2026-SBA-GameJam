using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HechiChaseGame : OneVsThreeBase
{
    [SerializeField] private GameObject hechiPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector3[] playerInitPos; // 4개 필요
    [SerializeField] private Vector3 hechiInitPos;

    [SerializeField] private BasicPlayerCanvasManager basicPlayerCanvasManager;

    [Header("결과 델타 (해치 승리)")]
    [SerializeField] private int hechiWinNightmare = 5;

    private readonly List<ChaseCharacterController> _normalPlayers = new();
    private bool _gameOver;

    public override int NightmareDelta { get; protected set; }
    public override bool IsOneWin { get; protected set; }

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

        SpawnCharacters();
        if (MiniGameManager.Instance.ResultContainer.IsTimeAttack)
            StartCoroutine(TimerRoutine());
    }

    private void SpawnCharacters()
    {
        Debug.Log($"[HechiChase] 해치: Player {OnePlayerId}");
        int playerCnt = 0;
        for (int i = 1; i <= 4; i++)
        {
            bool isHechi = i == OnePlayerId;
            var prefab = isHechi ? hechiPrefab : playerPrefab;
            var pos = isHechi ? hechiInitPos : playerInitPos[playerCnt++];
            var character = Instantiate(prefab, pos, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(character.gameObject, gameObject.scene);
            character.GetComponent<ChaseCharacterController>().Init(isHechi, OnPlayerEliminated);
            character.GetComponent<MiniGameCharacterController>().Init(i);

            if (!isHechi)
                _normalPlayers.Add(character.GetComponent<ChaseCharacterController>());
        }
    }

    private void OnPlayerEliminated(ChaseCharacterController characterController)
    {
        int playerId = characterController.GetComponent<MiniGameCharacterController>().PlayerId;
        basicPlayerCanvasManager?.GreyOutCharacter(playerId);
        _normalPlayers.Remove(characterController);
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

        MiniGameManager.Instance.QuitMiniGame();
    }
}
