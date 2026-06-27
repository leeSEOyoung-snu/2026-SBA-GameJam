using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;

// 제한 시간 내에 컨트롤러를 최대한 많이 스윙 (위-아래 1세트 = 1회)
// 전원 스윙 합산이 기준치 이상이면 성공 → 개인 스윙 순위 결정
// 기준치 미달이면 실패
[DefaultExecutionOrder(100)]
public class BokBokManager : SoloBattleBase
{
    [SerializeField] private int successThreshold = 100;

    public override int NightmareDelta { get; protected set; } = 0;
    public override int RankPlayer1 { get; protected set; }
    public override int RankPlayer2 { get; protected set; }
    public override int RankPlayer3 { get; protected set; }
    public override int RankPlayer4 { get; protected set; }

    [SerializeField] private BokBokHandVisual[] handVisuals;
    [SerializeField] private BasicPlayerCanvasManager basicPlayerCanvasManager;

    private int[] _swingCounts = new int[4];
    private IPlayerInputReader[] _inputs = new IPlayerInputReader[4];
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

        if (MiniGameManager.Instance.ResultContainer.IsTimeAttack)
            StartCoroutine(GameRoutine());
    }

    private void Update()
    {
        if (_gameOver) return;

        for (int i = 0; i < 4; i++)
        {
            if (!_inputs[i].Swing) continue;

            _swingCounts[i]++;
            Debug.Log($"[BokBok] Player {i + 1} 세트={_swingCounts[i]}");
            basicPlayerCanvasManager.UpdateStackCnt(i + 1, _swingCounts[i]);

            if (handVisuals != null && i < handVisuals.Length && handVisuals[i] != null)
                handVisuals[i].PlayStroke();
        }
    }

    private IEnumerator GameRoutine()
    {
        float elapsed = 0f;

        while (elapsed < MiniGameManager.Instance.ResultContainer.TimeAttackSeconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        EndGame();
    }

    private void EndGame()
    {
        if (_gameOver) return;
        _gameOver = true;

        int[] setCounts = _swingCounts.ToArray();
        int total = setCounts.Sum();
        bool success = total >= successThreshold;

        Debug.Log($"[BokBok] 총 스윙: {total} / 기준: {successThreshold} → {(success ? "성공" : "실패")}");

        var ranked = Enumerable.Range(0, 4)
            .OrderByDescending(i => setCounts[i])
            .ThenBy(i => i)
            .ToList();

        int[] ranks = new int[4];
        for (int r = 0; r < ranked.Count; r++)
            ranks[ranked[r]] = r + 1;

        RankPlayer1 = ranks[0];
        RankPlayer2 = ranks[1];
        RankPlayer3 = ranks[2];
        RankPlayer4 = ranks[3];

        for (int i = 0; i < 4; i++)
            Debug.Log($"[BokBok] Player {i + 1} 세트={_swingCounts[i]}");

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
