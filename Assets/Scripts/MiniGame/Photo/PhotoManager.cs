using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class PhotoManager : SoloBattleBase
{
    [Header("Game Settings")]
    [SerializeField] private int totalRounds = 10;
    [SerializeField] private int failThreshold = 20;
    [SerializeField] private float minWaitTime = 0.8f;
    [SerializeField] private float maxWaitTime = 2f;

    [Header("References")]
    [SerializeField] private PhotoHechi hechi;
    // 플레이어 1~4 대표 이미지 (인스펙터에서 순서대로 연결)
    [SerializeField] private Transform[] playerIcons = new Transform[4];

    [Header("Shutter VFX")]
    [SerializeField] private float punchScale = 3.6f;
    [SerializeField] private float punchDuration = 0.08f;
    [SerializeField] private float shrinkDuration = 0.15f;

    [Space(10)] [SerializeField] private int nightmareDelta = 5;

    public override int NightmareDelta { get; protected set; } = 0;
    public override int RankPlayer1 { get; protected set; }
    public override int RankPlayer2 { get; protected set; }
    public override int RankPlayer3 { get; protected set; }
    public override int RankPlayer4 { get; protected set; }

    private IPlayerInputReader[] _inputs = new IPlayerInputReader[4];
    private int[] _successCounts = new int[4];
    private int[] _failCounts = new int[4];

    private int _currentRound;
    private bool _hechiVisible;
    private bool _gameOver;

    private void Start()
    {
        for (int i = 0; i < 4; i++)
            _inputs[i] = GameManager.Instance.GetPlayerInputReader(i + 1);

        hechi.OnExited += OnHechiExited;
        hechi.gameObject.SetActive(false);

        StartCoroutine(InputRoutine());
        StartCoroutine(GameRoutine());
    }

    private IEnumerator InputRoutine()
    {
        while (!_gameOver)
        {
            for (int i = 0; i < 4; i++)
            {
                if (_inputs[i] != null && _inputs[i].Right)
                    OnPlayerShoot(i);
            }
            yield return null;
        }
    }

    private IEnumerator GameRoutine()
    {
        yield return new WaitForSecondsRealtime(6f);
        
        while (_currentRound < totalRounds && !_gameOver)
        {
            float wait = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(wait);

            _currentRound++;
            _hechiVisible = true;
            hechi.Launch();
            
            yield return new WaitUntil(() => !_hechiVisible || _gameOver);
        }

        EndGame();
    }

    private void OnPlayerShoot(int playerIndex)
    {
        PlayShutterVFX(playerIndex);

        if (hechi.IsInPhotoZone)
        {
            _successCounts[playerIndex]++;
            Debug.Log($"[Player {playerIndex}]  success count: {_successCounts[playerIndex]}");
        }
        else
        {
            _failCounts[playerIndex]++;
            Debug.Log($"[Player {playerIndex}]  fail count: {_failCounts[playerIndex]}");
        }
    }

    private void PlayShutterVFX(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerIcons.Length) return;
        Transform icon = playerIcons[playerIndex];
        if (icon == null) return;

        icon.DOKill();
        icon.localScale = Vector3.one * 3f;
        icon.DOScale(punchScale, punchDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => icon.DOScale(3f, shrinkDuration).SetEase(Ease.InQuad));
    }

    private void OnHechiExited()
    {
        _hechiVisible = false;
    }

    private void EndGame()
    {
        if (_gameOver) return;
        _gameOver = true;

        bool allFailedThreshold = _failCounts.All(f => f >= failThreshold);

        AssignRanks(allFailedThreshold);
        MiniGameManager.Instance.QuitMiniGame();
    }

    private void AssignRanks(bool allFailedThreshold)
    {
        // 성공 수 기준 내림차순 정렬 → 순위 결정
        var ranked = Enumerable.Range(0, 4)
            .OrderByDescending(i => _successCounts[i])
            .ThenBy(i => _failCounts[i])
            .ToList();

        int[] ranks = new int[4];
        for (int r = 0; r < ranked.Count; r++)
            ranks[ranked[r]] = r + 1;

        RankPlayer1 = ranks[0];
        RankPlayer2 = ranks[1];
        RankPlayer3 = ranks[2];
        RankPlayer4 = ranks[3];

        // allFailedThreshold 조건에 따른 NightmareDelta 처리 (필요 시 확장)
        NightmareDelta = allFailedThreshold ? nightmareDelta : 0;
    }
}
