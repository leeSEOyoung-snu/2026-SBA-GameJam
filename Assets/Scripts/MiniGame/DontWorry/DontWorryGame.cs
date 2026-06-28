using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontWorryGame : OneVsThreeBase
{
    [SerializeField] private DontWorryAIHachi aiHachi;
    [SerializeField] private DontWorryCrosshair crosshair;
    [SerializeField] private BoxCollider2D moveArea;
    [SerializeField] private List<DontWorryFakeHachiController> fakeHachis;
    [SerializeField] private BasicPlayerCanvasManager basicPlayerCanvasManager;
    [SerializeField] private Sprite[] crossHairSprites;

    [Header("결과 델타")]
    [SerializeField] private int realHachiShotNightmare = 5;

    private readonly List<DontWorryFakeHachiController> _fakePlayers = new();
    private int _totalFakes;
    private bool _gameOver;

    public override int NightmareDelta { get; protected set; }
    public override bool IsOneWin { get; protected set; }

    private void Start()
    {
        BasicMiniGameCanvas.OnGameStarted += HandleGameStarted;
        FindObjectOfType<DontWorryPlayerCanvasManager>()?.Init(OnePlayerId);
        InitSceneCharacters();
        if (MiniGameManager.Instance.ResultContainer.IsTimeAttack)
            StartCoroutine(TimerRoutine());
    }

    private void InitSceneCharacters()
    {
        Debug.Log($"[DontWorry] 슈터: Player {OnePlayerId}");

        if (fakeHachis == null)
            fakeHachis = new List<DontWorryFakeHachiController>();

        bool hasMoveBounds = TryGetMoveBounds(out Bounds moveBounds);

        if (aiHachi == null)
            Debug.LogWarning("[DontWorry] AI 해치가 씬에 할당되지 않았습니다.", this);
        else
        {
            MiniGameManager.Instance.ApplyCurrentMiniGameHechiSprite(aiHachi.gameObject);
            if (hasMoveBounds)
                aiHachi.SetMoveBounds(moveBounds);
        }

        if (crosshair == null)
            Debug.LogWarning("[DontWorry] 크로스헤어가 씬에 할당되지 않았습니다.", this);
        else
            crosshair.Init(OnePlayerId, OnShotFired, crossHairSprites[OnePlayerId - 1]);

        int fakeIndex = 0;
        for (int i = 1; i <= 4; i++)
        {
            if (i == OnePlayerId) continue;

            if (fakeIndex >= fakeHachis.Count || fakeHachis[fakeIndex] == null)
            {
                Debug.LogWarning($"[DontWorry] Player {i}용 가짜 해치가 씬에 할당되지 않았습니다.", this);
                fakeIndex++;
                continue;
            }

            DontWorryFakeHachiController fake = fakeHachis[fakeIndex];
            fake.Init(i, OnFakeEliminated);

            MiniGameCharacterController character = fake.GetComponent<MiniGameCharacterController>();
            if (character != null)
                character.Init(i);

            if (hasMoveBounds && fake.TryGetComponent<TopViewPhysics>(out var topViewPhysics))
                topViewPhysics.SetMoveBounds(moveBounds);

            SetupSceneFakeHachi(fake.gameObject);
            _fakePlayers.Add(fake);
            fakeIndex++;
        }

        _totalFakes = _fakePlayers.Count;
        // 이 씬 전용: 슈터 말풍선을 캐릭터와 안 겹치게 위로 올리고 크게, 글자는 작게
        basicPlayerCanvasManager?.StyleStackBubble(OnePlayerId, 125f, new Vector2(240f, 180f), 28f, -15f);
        UpdateShooterCatchText();
    }

    // 슈터(1인팀, 1P 자리) 슬롯에 잡은 가짜 해치 수 표기
    private void UpdateShooterCatchText()
    {
        int caught = _totalFakes - _fakePlayers.Count;
        basicPlayerCanvasManager?.SetStackAsString(OnePlayerId, $"잡은 가짜 해치\n{caught} / {_totalFakes}");
    }

    private bool TryGetMoveBounds(out Bounds moveBounds)
    {
        if (moveArea == null)
        {
            moveBounds = default;
            Debug.LogWarning("[DontWorry] MoveArea가 씬에 할당되지 않았습니다.", this);
            return false;
        }

        moveBounds = moveArea.bounds;
        return true;
    }

    private void SetupSceneFakeHachi(GameObject fakeObj)
    {
        CharacterMoveDust moveDust = fakeObj.GetComponent<CharacterMoveDust>();
        if (moveDust != null)
            moveDust.enabled = false;

        MiniGameManager.Instance.ApplyCurrentMiniGameHechiSprite(fakeObj);
    }

    private void HandleGameStarted()
    {
        BasicMiniGameCanvas.OnGameStarted -= HandleGameStarted;
        aiHachi?.StartMovingAfterDelay();
    }

    // 크로스헤어가 발사됐을 때 호출
    private void OnShotFired(bool hitRealHachi)
    {
        if (_gameOver) return;

        if (hitRealHachi)
        {
            Debug.Log("[DontWorry] 진짜 해치 맞음 → 3명 플레이어 승리");
            EndGame(shooterWins: false);
        }
    }

    private void OnFakeEliminated(DontWorryFakeHachiController fake)
    {
        if (_gameOver) return;

        basicPlayerCanvasManager?.GreyOutCharacter(fake.PlayerId);
        _fakePlayers.Remove(fake);
        UpdateShooterCatchText();
        Debug.Log($"[DontWorry] 가짜 해치 제거 / 남은: {_fakePlayers.Count}");

        if (_fakePlayers.Count == 0)
        {
            Debug.Log("[DontWorry] 가짜 해치 전원 제거 → 슈터 승리");
            EndGame(shooterWins: true);
        }
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
        {
            Debug.Log("[DontWorry] 시간 종료 → 3명 플레이어 승리");
            EndGame(shooterWins: false);
        }
    }

    private void EndGame(bool shooterWins)
    {
        if (_gameOver) return;
        _gameOver = true;

        IsOneWin = shooterWins;
        NightmareDelta = shooterWins ? 0 : realHachiShotNightmare;

        MiniGameManager.Instance.QuitMiniGame();
    }

    private void OnDestroy()
    {
        BasicMiniGameCanvas.OnGameStarted -= HandleGameStarted;
    }
}
