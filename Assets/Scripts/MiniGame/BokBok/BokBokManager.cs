using System.Collections;
using System.Linq;
using DG.Tweening;
using TMPro;
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

    [SerializeField] private BokBokHandVisual[] handVisuals; // index 0~3 = player 1~4
    [SerializeField] private BasicPlayerCanvasManager basicPlayerCanvasManager;
    [SerializeField] private TMP_Text goalCountText;
    [SerializeField] private SpriteRenderer bokBokSpriteRenderer;
    [Header("오디오")]
    [SerializeField] private AudioClip purrClip;

    [Header("복복 또잉 연출")]
    [SerializeField] private float boingHeight = 40f;
    [SerializeField] private float boingDuration = 0.45f;
    [SerializeField] private Vector2 squashScale = new(1.12f, 0.86f);
    [SerializeField] private Vector2 stretchScale = new(0.9f, 1.18f);

    // raw 스윙 횟수 누적 → /2 = 세트 수 (위-아래 = 1세트)
    private int[] _swingCounts = new int[4];

    private IPlayerInputReader[] _inputs = new IPlayerInputReader[4];
    private bool _gameOver;
    private Sequence _boingSeq;
    private Vector3 _bokBokOriginLocalPos;
    private Vector3 _bokBokOriginScale;

    private void Start()
    {
        ApplyCurrentHechiSprite();
        CacheBokBokTransform();
        UpdateGoalCountText();

        for (int i = 0; i < 4; i++)
            _inputs[i] = GameManager.Instance.GetPlayerInputReader(i + 1);

        StartCoroutine(GameRoutine());
    }

    private void ApplyCurrentHechiSprite()
    {
        Sprite currentHechiSprite = GameManager.Instance.GetHechiSpriteOnMiniGame();
        if (currentHechiSprite == null)
        {
            Debug.LogWarning("[BokBok] 현재 미니게임 복복이 스프라이트를 찾지 못했습니다.", this);
            return;
        }

        if (bokBokSpriteRenderer != null)
            bokBokSpriteRenderer.sprite = currentHechiSprite;
    }

    private void CacheBokBokTransform()
    {
        if (bokBokSpriteRenderer == null) return;

        Transform target = bokBokSpriteRenderer.transform;
        _bokBokOriginLocalPos = target.localPosition;
        _bokBokOriginScale = target.localScale;
    }

    private void Update()
    {
        if (_gameOver) return;

        for (int i = 0; i < 4; i++)
        {
            if (!_inputs[i].Swing) continue;

            MiniGameManager.Instance.Audio?.PlaySfx(purrClip);
            _swingCounts[i]++;
            Debug.Log($"[BokBok] Player {i + 1} 세트={_swingCounts[i]}"); 
            basicPlayerCanvasManager.UpdateStackCnt(i + 1, _swingCounts[i]);
            UpdateGoalCountText();

            if (handVisuals != null && i < handVisuals.Length && handVisuals[i] != null)
                handVisuals[i].PlayStroke();

            PlayBoing();
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

        MiniGameManager.Instance.QuitMiniGame();
    }

    private void UpdateGoalCountText()
    {
        if (goalCountText == null)
            return;

        goalCountText.text = $"{_swingCounts.Sum()}/{successThreshold}";
    }

    private void PlayBoing()
    {
        if (bokBokSpriteRenderer == null)
            return;

        Transform target = bokBokSpriteRenderer.transform;
        _boingSeq?.Kill(complete: false);
        target.localPosition = _bokBokOriginLocalPos;
        target.localScale = _bokBokOriginScale;

        _boingSeq = DOTween.Sequence();
        _boingSeq.Append(target.DOScale(ScaleBy(_bokBokOriginScale, squashScale), boingDuration * 0.18f)
            .SetEase(Ease.OutQuad));
        _boingSeq.Append(target.DOLocalMoveY(_bokBokOriginLocalPos.y + boingHeight, boingDuration * 0.32f)
            .SetEase(Ease.OutQuad));
        _boingSeq.Join(target.DOScale(ScaleBy(_bokBokOriginScale, stretchScale), boingDuration * 0.32f)
            .SetEase(Ease.OutBack));
        _boingSeq.Append(target.DOLocalMoveY(_bokBokOriginLocalPos.y, boingDuration * 0.5f)
            .SetEase(Ease.OutBounce));
        _boingSeq.Join(target.DOScale(_bokBokOriginScale, boingDuration * 0.5f)
            .SetEase(Ease.OutElastic));
        _boingSeq.OnComplete(() =>
        {
            target.localPosition = _bokBokOriginLocalPos;
            target.localScale = _bokBokOriginScale;
            _boingSeq = null;
        });
    }

    private static Vector3 ScaleBy(Vector3 origin, Vector2 multiplier)
    {
        return new Vector3(origin.x * multiplier.x, origin.y * multiplier.y, origin.z);
    }
}
