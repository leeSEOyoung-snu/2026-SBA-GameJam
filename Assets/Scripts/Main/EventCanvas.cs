using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EventCanvas : MonoBehaviour
{
    [Serializable]
    private struct PlayerEventSprites
    {
        public int playerId;
        public Sprite first;
        public Sprite second;
    }

    [Header("References")]
    [SerializeField] private CanvasGroup mainEvent;
    [SerializeField] private RectTransform eventImgArea;
    [SerializeField] private RectTransform eventType;
    [SerializeField] private TextMeshProUGUI eventTypeTxt;
    [SerializeField] private Image eventImg;

    [Header("Positions")]
    [SerializeField] private float hiddenPosX = 1300f;
    [SerializeField] private float eventTypeOpenPosX = 730f;
    [SerializeField] private float eventImgAreaOpenPosX = 430f;

    [Header("Open Timing")]
    [SerializeField] private float eventTypeMoveDuration = 0.2f;
    [SerializeField] private float eventImgAreaMoveDelay = 0.1f;
    [SerializeField] private float eventImgAreaMoveDuration = 0.2f;
    [SerializeField] private float mainEventFadeDelay = 1f;
    [SerializeField] private float mainEventFadeDuration = 0.2f;
    [SerializeField] private Ease openEase = Ease.OutQuad;

    [Header("Close Timing")]
    [SerializeField] private float closeDuration = 0.2f;
    [SerializeField] private Ease closeEase = Ease.InQuad;

    [Header("Text")]
    [SerializeField] private string affectionStealText = "호감도 뺏기!";
    [SerializeField] private string stateChangeText = "상태 변화!";
    [SerializeField] private string nightmarePitText = "악몽 구덩이!";

    [Header("Images")]
    [SerializeField] private List<PlayerEventSprites> affectionStealSprites = new();
    [SerializeField] private Sprite nightmarePitSprite;
    [SerializeField] private float affectionFrameInterval = 0.18f;

    private readonly Dictionary<int, Sprite[]> _affectionStealSpritesByPlayerId = new();
    private Sequence _activeSequence;
    private Sequence _affectionImageSequence;

    private void Awake()
    {
        InitAffectionSpriteMap();
        ResetView();
    }

    private void OnDestroy()
    {
        KillSequences();
    }

    public Sequence Open(CellType cellType, StateContainer stateContainer)
    {
        ApplyEventData(cellType, stateContainer);
        ResetView();
        KillActiveSequence();

        _activeSequence = DOTween.Sequence();
        _activeSequence.Append(eventType.DOAnchorPosX(eventTypeOpenPosX, eventTypeMoveDuration).SetEase(openEase));
        _activeSequence.Insert(eventImgAreaMoveDelay,
            eventImgArea.DOAnchorPosX(eventImgAreaOpenPosX, eventImgAreaMoveDuration).SetEase(openEase));
        _activeSequence.Insert(mainEventFadeDelay, mainEvent.DOFade(1f, mainEventFadeDuration));

        return _activeSequence;
    }

    public Sequence Close()
    {
        KillActiveSequence();
        KillAffectionImageSequence();

        _activeSequence = DOTween.Sequence();
        _activeSequence.Join(mainEvent.DOFade(0f, closeDuration));
        _activeSequence.Join(eventImgArea.DOAnchorPosX(hiddenPosX, closeDuration).SetEase(closeEase));
        _activeSequence.Join(eventType.DOAnchorPosX(hiddenPosX, closeDuration).SetEase(closeEase));

        return _activeSequence;
    }

    private void ApplyEventData(CellType cellType, StateContainer stateContainer)
    {
        KillAffectionImageSequence();

        switch (cellType)
        {
            case CellType.AffectionSteal:
                eventTypeTxt.text = affectionStealText;
                ApplyAffectionStealImage(stateContainer);
                break;
            case CellType.StateChange:
                eventTypeTxt.text = stateChangeText;
                eventImg.sprite = GameManager.Instance.GetHechiSpriteOnMiniGame();
                break;
            case CellType.NightmarePit:
                eventTypeTxt.text = nightmarePitText;
                eventImg.sprite = nightmarePitSprite;
                break;
        }
    }

    private void ApplyAffectionStealImage(StateContainer stateContainer)
    {
        int playerId = GetLowestAffectionPlayerId(stateContainer.AffectionById);
        if (!_affectionStealSpritesByPlayerId.TryGetValue(playerId, out Sprite[] sprites) || sprites.Length == 0)
        {
            eventImg.sprite = null;
            return;
        }

        eventImg.sprite = sprites[0];

        if (sprites.Length < 2)
            return;

        int frame = 0;
        _affectionImageSequence = DOTween.Sequence()
            .AppendCallback(() =>
            {
                eventImg.sprite = sprites[frame];
                frame = (frame + 1) % sprites.Length;
            })
            .AppendInterval(affectionFrameInterval)
            .SetLoops(-1);
    }

    private int GetLowestAffectionPlayerId(Dictionary<int, int> affectionById)
    {
        int minAffection = affectionById.Min(pair => pair.Value);
        List<int> lowestPlayerIds = affectionById
            .Where(pair => pair.Value == minAffection)
            .Select(pair => pair.Key)
            .ToList();

        return lowestPlayerIds[Random.Range(0, lowestPlayerIds.Count)];
    }

    private void InitAffectionSpriteMap()
    {
        _affectionStealSpritesByPlayerId.Clear();

        foreach (PlayerEventSprites sprites in affectionStealSprites)
        {
            List<Sprite> validSprites = new();
            if (sprites.first != null) validSprites.Add(sprites.first);
            if (sprites.second != null) validSprites.Add(sprites.second);

            _affectionStealSpritesByPlayerId[sprites.playerId] = validSprites.ToArray();
        }
    }

    private void ResetView()
    {
        mainEvent.alpha = 0f;
        eventImgArea.anchoredPosition = new Vector2(hiddenPosX, eventImgArea.anchoredPosition.y);
        eventType.anchoredPosition = new Vector2(hiddenPosX, eventType.anchoredPosition.y);
    }

    private void KillSequences()
    {
        KillActiveSequence();
        KillAffectionImageSequence();
    }

    private void KillActiveSequence()
    {
        if (_activeSequence == null)
            return;

        _activeSequence.Kill();
        _activeSequence = null;
    }

    private void KillAffectionImageSequence()
    {
        if (_affectionImageSequence == null)
            return;

        _affectionImageSequence.Kill();
        _affectionImageSequence = null;
    }
}
