using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

// 해치랑 한강 라면 끓이기
// 타이머가 3초까지 보이다가 숨겨지고, 10초에 가장 가깝게 버튼을 누른 순서대로 순위 결정
public class RamenTimingGame : SoloBattleBase
{
    [SerializeField] private float targetTime    = 10f;
    [SerializeField] private float visibleUntil  = 3f;
    [SerializeField] private float timeoutTime   = 20f; // IsTimeAttack 미체크 시 폴백

    [SerializeField] private TMPro.TextMeshProUGUI timerText;

    public override int NightmareDelta { get; protected set; } = 0;
    public override int RankPlayer1 { get; protected set; }
    public override int RankPlayer2 { get; protected set; }
    public override int RankPlayer3 { get; protected set; }
    public override int RankPlayer4 { get; protected set; }

    private IPlayerInputReader[] _inputs = new IPlayerInputReader[4];
    private float?[] _pressTimes = new float?[4];

    private float _elapsed;
    private bool _gameOver;

    private void Start()
    {
        for (int i = 0; i < 4; i++)
            _inputs[i] = GameManager.Instance.GetPlayerInputReader(i + 1);

        StartCoroutine(WaitForGameStart());
    }

    private IEnumerator WaitForGameStart()
    {
        bool started = false;
        void Handler() { started = true; }
        BasicMiniGameCanvas.OnGameStarted += Handler;
        yield return new WaitUntil(() => started);
        BasicMiniGameCanvas.OnGameStarted -= Handler;

        float timeout = MiniGameManager.Instance.ResultContainer.IsTimeAttack
            ? MiniGameManager.Instance.ResultContainer.TimeAttackSeconds
            : timeoutTime;
        StartCoroutine(GameRoutine(timeout));
    }

    private IEnumerator GameRoutine(float timeout)
    {
        _elapsed = 0f;

        while (_elapsed < timeout && !_gameOver)
        {
            _elapsed += Time.deltaTime;

            if (timerText != null)
            {
                if (_elapsed <= visibleUntil)
                {
                    timerText.text = _elapsed.ToString("F1");
                    timerText.enabled = true;
                }
                else
                {
                    timerText.enabled = false;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (_pressTimes[i] == null && _inputs[i].Right)
                {
                    _pressTimes[i] = _elapsed;
                    Debug.Log($"[Ramen] Player {i + 1} 버튼 입력: {_elapsed:F3}초 (오차: {Mathf.Abs(_elapsed - targetTime):F3}초)");
                }
            }

            if (_pressTimes.All(t => t != null))
                break;

            yield return null;
        }

        EndGame();
    }

    private void EndGame()
    {
        if (_gameOver) return;
        _gameOver = true;

        for (int i = 0; i < 4; i++)
        {
            if (_pressTimes[i] == null)
            {
                _pressTimes[i] = float.MaxValue;
                Debug.Log($"[Ramen] Player {i + 1} 시간 초과 — 꼴등 처리");
            }
        }

        var ranked = Enumerable.Range(0, 4)
            .OrderBy(i => Mathf.Abs(_pressTimes[i].Value - targetTime))
            .ToList();

        int[] ranks = new int[4];
        for (int r = 0; r < ranked.Count; r++)
            ranks[ranked[r]] = r + 1;

        RankPlayer1 = ranks[0];
        RankPlayer2 = ranks[1];
        RankPlayer3 = ranks[2];
        RankPlayer4 = ranks[3];

        var resultLog = new System.Text.StringBuilder();
        resultLog.AppendLine($"[Ramen] ===== 게임 결과 (목표: {targetTime}초) =====");
        for (int r = 0; r < ranked.Count; r++)
        {
            int playerId = ranked[r] + 1;
            float pressTime = _pressTimes[ranked[r]].Value;
            float diff = Mathf.Abs(pressTime - targetTime);
            string timeStr = pressTime >= float.MaxValue ? "미입력" : $"{pressTime:F3}초 (오차 {diff:F3}초)";
            resultLog.AppendLine($"  {r + 1}등: Player {playerId} — {timeStr}");
        }
        Debug.Log(resultLog.ToString());

        StartCoroutine(EndRoutine());
    }

    private IEnumerator EndRoutine()
    {
        var canvas = FindObjectOfType<BasicMiniGameCanvas>();
        if (canvas != null)
            yield return canvas.PlayGameEnd().WaitForCompletion();
        MiniGameManager.Instance.QuitMiniGame();
    }
}
